using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Granikos.Hydra.Service.Models;
using Granikos.Hydra.Service.TimeTables;
using log4net;

namespace Granikos.Hydra.Service.Providers
{
    [Export(typeof(ITimeTableProvider))]
    public class TimeTableProvider : DefaultProvider<TimeTable>, ITimeTableProvider
    {
        private string TemplateFolder
        {
            get
            {
                var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var logFolder = ConfigurationManager.AppSettings["MailTemplates"];
                return Path.Combine(folder, logFolder);
            }
        }

        private string AttachmentFolder
        {
            get
            {
                var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var logFolder = ConfigurationManager.AppSettings["AttachmentFolder"];
                return Path.Combine(folder, logFolder);
            }
        }

        private static readonly ILog Logger = LogManager.GetLogger(typeof(TimeTableProvider));

        protected override bool Validate(TimeTable entity, out string message)
        {
            if (!base.Validate(entity, out message)) return false;

            if (entity.MailContentTemplate.Contains(".."))
            {
                message = "Invalid MailContentTemplate";
                return false;
            }

            var file = Path.Combine(TemplateFolder, entity.MailContentTemplate + ".xml");

            if (!File.Exists(file))
            {
                message = "Invalid MailContentTemplate";
                return false;
            }

            return true;
        }

        public TimeTableProvider()
            : base("TimeTables")
        {
            OnAdded += tt =>
            {
                if (OnAdd != null) OnAdd(tt);
            };

            OnRemoved += tt =>
            {
                if (OnRemove != null) OnRemove(tt);
            };

            var fileName = "TimeTablesResults.txt";
            var folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                ConfigurationManager.AppSettings["DataFolder"]);

            if (!File.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            ResultFileName = Path.GetFullPath(Path.Combine(folderPath, fileName));

            if (File.Exists(ResultFileName)) LoadResults();
        }

        public string ResultFileName { get; private set; }

#if DEBUG
        protected override IEnumerable<TimeTable> Initializer()
        {
            yield return new TimeTable
            {
                Name = "Test",
                Active = true,
                MailContentTemplate = "Test Mail Content",
                MailType = "default",
                MinRecipients = 1,
                MaxRecipients = 2,
                ProtocolLevel = ProtocolLevel.On,
                ReportType = ReportType.Daily,
                Type = "static",
                StaticRecipient = true,
                RecipientMailbox = "fu@bar.de",
                StaticSender = true,
                SenderMailbox = "bernd@test.de",
                AttachmentType = AttachmentType.Off,
                Parameters = new Dictionary<string, string>
                {
                    {"staticType", "1h"}
                },
                ReportMailAddress = "report@test.de",
                SendEicarFile = false
            };
            yield return new TimeTable
            {
                Name = "Test 2",
                Active = false,
                MailContentTemplate = "Test 2 Mail Content"
            };
        }
#endif

        [Import(AllowRecomposition = true)]
        private CompositionContainer _container { get; set; }

        public IEnumerable<TimeTableTypeInfo> GetTimeTableTypes()
        {
            foreach (var type in _container.GetExportedTypesWithContracts<ITimeTableType>())
            {
                DisplayNameAttribute dn = (DisplayNameAttribute)Attribute.GetCustomAttribute(type.Item1, typeof(DisplayNameAttribute));

                yield return new TimeTableTypeInfo
                {
                    Name = type.Item2,
                    DisplayName = dn != null ? dn.DisplayName : type.Item2
                };

            }
        }

        public IDictionary<string, string> GetTimeTableTypeData(string type)
        {
            return _container.GetExport<ITimeTableType>(type).Value.Data;
        }

        public event TimeTableChangeHandler OnAdd;
        public event TimeTableChangeHandler OnRemove;
        public void IncreaseErrorMailCount(int id)
        {
            Get(id).IncreaseError();

            StoreResults();
        }

        public void IncreaseSuccessMailCount(int id)
        {
            Get(id).IncreaseSuccess();

            StoreResults();
        }

        public IEnumerable<MailTemplateType> GetMailTemplates()
        {
            var files = Directory.GetFiles(TemplateFolder, "*.xml");
            var serializer = new XmlSerializer(typeof(NikosTwo));

            return files.Select(file =>
            {

                using (var stream = new FileStream(file, FileMode.Open))
                using (var reader = new XmlTextReader(stream))
                {
                    reader.WhitespaceHandling = WhitespaceHandling.All;

                    var nikosTwo = (NikosTwo)serializer.Deserialize(reader);

                    nikosTwo.MailTemplate.File = Path.GetFileNameWithoutExtension(file);

                    return nikosTwo.MailTemplate;
                }
            });
        }

        public string[] GetAttachments()
        {
            if (!File.Exists(AttachmentFolder))
                Directory.CreateDirectory(AttachmentFolder);

            return Directory.GetFiles(AttachmentFolder).Select(Path.GetFileName).ToArray();
            
        }

        public byte[] GetAttachmentContent(string name)
        {
            return File.ReadAllBytes(Path.Combine(AttachmentFolder, name));
        }

        protected void StoreResults()
        {
            try
            {
                using (var sw = new StreamWriter(ResultFileName))
                {
                    foreach (var tt in All())
                    {
                        sw.WriteLine("{0} {1} {2}", tt.Id, tt.MailsSuccess, tt.MailsError);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("Could not save time table results", e);
            }
        }

        private void LoadResults()
        {
            try
            {
                using (var sr = new StreamReader(ResultFileName))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        var parts = line.Split(' ');
                        var tt = Get(Int32.Parse(parts[0]));

                        var successes = Int32.Parse(parts[1]);
                        var errors = Int32.Parse(parts[2]);

                        tt.InitializeResults(successes, errors);
                    }
                }

            }
            catch (Exception e)
            {
                Logger.Error("Could not load time table results", e);
            }
        }
    }
}