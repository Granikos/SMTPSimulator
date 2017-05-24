// MIT License
// 
// Copyright (c) 2017 Granikos GmbH & Co. KG (https://www.granikos.eu)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.ComponentModel.Composition;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using Granikos.SMTPSimulator.Service.Models.Providers;

namespace Granikos.SMTPSimulator.Service.Providers
{
    [Export(typeof (ILogProvider))]
    public class FileLogProvider : ILogProvider
    {
        private string LogFolder
        {
            get
            {
                var folder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                var logFolder = ConfigurationManager.AppSettings["LogFolder"];
                return Path.Combine(folder, logFolder);
            }
        }

        public string[] FileNames
        {
            get
            {
                var length = Path.GetFullPath(LogFolder).Length + 1;

                return Directory.GetFiles(LogFolder, "*", SearchOption.AllDirectories)
                    .Select(p => p.Substring(length))
                    .ToArray();
            }
        }

        public void GetFile(Stream stream, string name)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (!(stream.CanWrite)) throw new ArgumentException();
            if (name == null) throw new ArgumentNullException("name");
            if (Path.IsPathRooted(name)) throw new ArgumentException();
            if (name.Contains("..")) throw new ArgumentException();

            var logFile = Path.Combine(LogFolder, name);

            using (var logStream = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                logStream.CopyTo(stream);
            }
        }
    }
}