using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Granikos.SMTPSimulator.Service
{
    public class NewtonsoftJsonDispatchFormatter : IDispatchMessageFormatter
    {
        OperationDescription operation;
        Dictionary<string, int> parameterNames;

        public NewtonsoftJsonDispatchFormatter(OperationDescription operation, bool isRequest)
        {
            this.operation = operation;
            if (isRequest)
            {
                int operationParameterCount = operation.Messages[0].Body.Parts.Count;
                if (operationParameterCount > 1)
                {
                    this.parameterNames = new Dictionary<string, int>();
                    for (int i = 0; i < operationParameterCount; i++)
                    {
                        this.parameterNames.Add(operation.Messages[0].Body.Parts[i].Name, i);
                    }
                }
            }
        }

        public void DeserializeRequest(Message message, object[] parameters)
        {
            object bodyFormatProperty;
            if (!message.Properties.TryGetValue(WebBodyFormatMessageProperty.Name, out bodyFormatProperty) ||
                (bodyFormatProperty as WebBodyFormatMessageProperty).Format != WebContentFormat.Raw)
            {
                throw new InvalidOperationException("Incoming messages must have a body format of Raw. Is a ContentTypeMapper set on the WebHttpBinding?");
            }

            var bodyReader = message.GetReaderAtBodyContents();
            bodyReader.ReadStartElement("Binary");
            byte[] rawBody = bodyReader.ReadContentAsBase64();
            var ms = new MemoryStream(rawBody);

            var sr = new StreamReader(ms);
            var serializer = new Newtonsoft.Json.JsonSerializer();
            if (parameters.Length == 1)
            {
                // single parameter, assuming bare
                parameters[0] = serializer.Deserialize(sr, operation.Messages[0].Body.Parts[0].Type);
            }
            else
            {
                // multiple parameter, needs to be wrapped
                Newtonsoft.Json.JsonReader reader = new Newtonsoft.Json.JsonTextReader(sr);
                reader.Read();
                if (reader.TokenType != Newtonsoft.Json.JsonToken.StartObject)
                {
                    throw new InvalidOperationException("Input needs to be wrapped in an object");
                }

                reader.Read();
                while (reader.TokenType == Newtonsoft.Json.JsonToken.PropertyName)
                {
                    var parameterName = reader.Value as string;
                    reader.Read();
                    if (this.parameterNames.ContainsKey(parameterName))
                    {
                        int parameterIndex = this.parameterNames[parameterName];
                        parameters[parameterIndex] = serializer.Deserialize(reader, this.operation.Messages[0].Body.Parts[parameterIndex].Type);
                    }
                    else
                    {
                        reader.Skip();
                    }

                    reader.Read();
                }

                reader.Close();
            }

            sr.Close();
            ms.Close();
        }

        public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
        {
            byte[] body;
            var serializer = new Newtonsoft.Json.JsonSerializer();

            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms, Encoding.UTF8))
                {
                    using (Newtonsoft.Json.JsonWriter writer = new Newtonsoft.Json.JsonTextWriter(sw))
                    {
                        //writer.Formatting = Newtonsoft.Json.Formatting.Indented;
                        serializer.Serialize(writer, result);
                        sw.Flush();
                        body = ms.ToArray();
                    }
                }
            }

            System.ServiceModel.Channels.Message replyMessage = System.ServiceModel.Channels.Message.CreateMessage(messageVersion, operation.Messages[1].Action, new RawBodyWriter(body));
            replyMessage.Properties.Add(WebBodyFormatMessageProperty.Name, new WebBodyFormatMessageProperty(WebContentFormat.Raw));
            var respProp = new HttpResponseMessageProperty();
            respProp.Headers[HttpResponseHeader.ContentType] = "application/json";
            replyMessage.Properties.Add(HttpResponseMessageProperty.Name, respProp);
            return replyMessage;
        }
    }

    public class RawBodyWriter : BodyWriter
    {
        byte[] content;
        public RawBodyWriter(byte[] content)
            : base(true)
        {
            this.content = content;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            writer.WriteStartElement("Binary");
            writer.WriteBase64(content, 0, content.Length);
            writer.WriteEndElement();
        }
    }

    public class NewtonsoftJsonBehavior : WebHttpBehavior
    {
        public override void Validate(ServiceEndpoint endpoint)
        {
            base.Validate(endpoint);

            var elements = endpoint.Binding.CreateBindingElements();
            var webEncoder = elements.Find<WebMessageEncodingBindingElement>();
            if (webEncoder == null)
            {
                throw new InvalidOperationException("This behavior must be used in an endpoint with the WebHttpBinding (or a custom binding with the WebMessageEncodingBindingElement).");
            }

            foreach (OperationDescription operation in endpoint.Contract.Operations)
            {
                this.ValidateOperation(operation);
            }
        }

        protected override IDispatchMessageFormatter GetRequestDispatchFormatter(OperationDescription operationDescription, ServiceEndpoint endpoint)
        {
            if (this.IsGetOperation(operationDescription))
            {
                // no change for GET operations
                return base.GetRequestDispatchFormatter(operationDescription, endpoint);
            }

            if (operationDescription.Messages[0].Body.Parts.Count == 0)
            {
                // nothing in the body, still use the default
                return base.GetRequestDispatchFormatter(operationDescription, endpoint);
            }

            return new NewtonsoftJsonDispatchFormatter(operationDescription, true);
        }

        protected override IDispatchMessageFormatter GetReplyDispatchFormatter(OperationDescription operationDescription, ServiceEndpoint endpoint)
        {
            if (operationDescription.Messages.Count == 1 || operationDescription.Messages[1].Body.ReturnValue.Type == typeof(void))
            {
                return base.GetReplyDispatchFormatter(operationDescription, endpoint);
            }
            else
            {
                return new NewtonsoftJsonDispatchFormatter(operationDescription, false);
            }
        }

        private void ValidateOperation(OperationDescription operation)
        {
            if (operation.Messages.Count > 1)
            {
                if (operation.Messages[1].Body.Parts.Count > 0)
                {
                    throw new InvalidOperationException("Operations cannot have out/ref parameters.");
                }
            }

            WebMessageBodyStyle bodyStyle = this.GetBodyStyle(operation);
            int inputParameterCount = operation.Messages[0].Body.Parts.Count;
            if (!this.IsGetOperation(operation))
            {
                var wrappedRequest = bodyStyle == WebMessageBodyStyle.Wrapped || bodyStyle == WebMessageBodyStyle.WrappedRequest;
                if (inputParameterCount == 1 && wrappedRequest)
                {
                    throw new InvalidOperationException("Wrapped body style for single parameters not implemented in this behavior.");
                }
            }

            var wrappedResponse = bodyStyle == WebMessageBodyStyle.Wrapped || bodyStyle == WebMessageBodyStyle.WrappedResponse;
            var isVoidReturn = operation.Messages.Count == 1 || operation.Messages[1].Body.ReturnValue.Type == typeof(void);
            if (!isVoidReturn && wrappedResponse)
            {
                throw new InvalidOperationException("Wrapped response not implemented in this behavior.");
            }
        }

        private WebMessageBodyStyle GetBodyStyle(OperationDescription operation)
        {
            var wga = operation.Behaviors.Find<WebGetAttribute>();
            if (wga != null)
            {
                return wga.BodyStyle;
            }

            var wia = operation.Behaviors.Find<WebInvokeAttribute>();
            if (wia != null)
            {
                return wia.BodyStyle;
            }

            return this.DefaultBodyStyle;
        }

        private bool IsGetOperation(OperationDescription operation)
        {
            var wga = operation.Behaviors.Find<WebGetAttribute>();
            if (wga != null)
            {
                return true;
            }

            var wia = operation.Behaviors.Find<WebInvokeAttribute>();
            if (wia != null)
            {
                return wia.Method == "HEAD";
            }

            return false;
        }
    }

    public class NewtonsoftJsonBehaviorExtension : BehaviorExtensionElement
    {
        public override Type BehaviorType
        {
            get { return typeof(NewtonsoftJsonBehavior); }
        }

        protected override object CreateBehavior()
        {
            return new NewtonsoftJsonBehavior();
        }
    }

    public class NewtonsoftJsonContentTypeMapper : WebContentTypeMapper
    {
        public override WebContentFormat GetMessageFormatForContentType(string contentType)
        {
            return WebContentFormat.Raw;
        }
    }
}
