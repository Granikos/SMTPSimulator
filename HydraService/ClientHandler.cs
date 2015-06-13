using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using HydraCore;
using HydraCore.Logging;
using HydraService.Models;

namespace HydraService
{
    internal class ClientHandler
    {
        private readonly TcpClient _client;
        private readonly IPEndPoint _localEndpoint;
        private readonly string _name;
        private readonly IPEndPoint _remoteEndpoint;
        private readonly string _session;
        private readonly SMTPServer _smtpServer;
        private readonly TLSConnector _tlsConnector;

        [ImportMany] private IEnumerable<ISMTPLogger> _loggers;

        private StreamReader _reader;
        private bool _startTLS;
        private Stream _stream;
        private SMTPTransaction _transaction;
        private StreamWriter _writer;

        public ClientHandler(TcpClient client, SMTPServer smtpServer)
        {
            _client = client;
            _smtpServer = smtpServer;

            _tlsConnector = new TLSConnector(_smtpServer.Connector.TLSSettings, CertificateLog);
            _localEndpoint = (IPEndPoint) _client.Client.LocalEndPoint;
            _remoteEndpoint = (IPEndPoint) _client.Client.RemoteEndPoint;
            _session = Guid.NewGuid().ToString("N").Substring(0, 10);
            _name = _localEndpoint.Address.ToString();
            _stream = _client.GetStream();
            _startTLS = false;
        }

        private void CertificateLog(string msg)
        {
            Log(LogEventType.Certificate, msg);
        }

        private void Log(LogEventType type, string data = null)
        {
            foreach (var logger in _loggers)
            {
                logger.Log(_name, _session, _localEndpoint, _remoteEndpoint, LogPartType.Server, type, data);
            }
        }

        public async Task Process()
        {
            Log(LogEventType.Connect);

            SMTPResponse response;
            _transaction = _smtpServer.Core.StartTransaction(_remoteEndpoint.Address, _smtpServer.Settings, out response);

            if (_tlsConnector.Settings.Mode == TLSMode.FullTunnel)
            {
                await StartTLS();
            }

            RefreshReaderAndWriter();

            _transaction.OnStartTLS += OnStartTLS;
            _transaction.OnClose += OnCloseHandler;

            await Write(response);

            try
            {
                while (!_reader.EndOfStream && !_transaction.Closed)
                {
                    await Write(_transaction.ExecuteCommand(await Read()));

                    while (_transaction.InDataMode)
                    {
                        string line;
                        var data = new StringBuilder();

                        do
                        {
                            line = await _reader.ReadLineAsync();
                        } while (_transaction.HandleDataLine(line, data));

                        await Write(_transaction.HandleData(data.ToString()));
                    }

                    if (_startTLS)
                    {
                        await StartTLS();

                        RefreshReaderAndWriter();
                        RefreshTransaction();

                        _startTLS = false;
                    }
                }
            }
            catch (IOException)
            {
            }
            finally
            {
                Close();
            }
        }

        private void RefreshReaderAndWriter()
        {
            RefreshWriter();
            RefreshReader();
        }

        private void OnStartTLS(object sender, CancelEventArgs e)
        {
            _startTLS = true;
        }

        private void Close()
        {
            _transaction.Close();
            _writer.Close();
            _reader.Close();
            _stream.Close();
            _client.Close();
        }

        private async Task<SMTPCommand> Read()
        {
            var line = await _reader.ReadLineAsync();

            Log(LogEventType.Incoming, line);

            var parts = line.Split(new[] {' '}, 2);

            return parts.Length > 1 ? new SMTPCommand(parts[0], parts[1]) : new SMTPCommand(parts[0]);
        }

        private async Task Write(SMTPResponse response)
        {
            if (_stream.CanWrite)
            {
                Log(LogEventType.Outgoing, response.ToString());

                await _writer.WriteLineAsync(response.ToString());
                _writer.Flush();
            }
        }

        private void OnCloseHandler(SMTPTransaction transaction)
        {
            Log(LogEventType.Disconnect);
        }

        private void RefreshTransaction()
        {
            SMTPResponse response;
            _transaction.OnClose -= OnCloseHandler;
            _transaction.Close();
            _transaction = _smtpServer.Core.StartTransaction(_remoteEndpoint.Address, _smtpServer.Settings, out response);
            _transaction.TLSEnabled = true;
            _transaction.OnClose += OnCloseHandler;
        }

        private async Task StartTLS()
        {
            var sslStream = _tlsConnector.GetSslStream(_stream);

            await _tlsConnector.AuthenticateAsServerAsync(sslStream);

            Log(LogEventType.Other, "TLS Layer established.");

            _tlsConnector.DisplaySecurityLevel(sslStream);
            _tlsConnector.DisplaySecurityServices(sslStream);
            _tlsConnector.DisplayCertificateInformation(sslStream);

            _transaction.TLSEnabled = true;
            _stream = sslStream;
        }

        private void RefreshReader()
        {
            if (_reader != null) _reader.Close();
            _reader = new StreamReader(_stream, Encoding.ASCII, false, 1000, true);
        }

        private void RefreshWriter()
        {
            if (_writer != null) _writer.Close();
            _writer = new StreamWriter(_stream, Encoding.ASCII, 1000, true) {NewLine = "\r\n"};
        }
    }
}