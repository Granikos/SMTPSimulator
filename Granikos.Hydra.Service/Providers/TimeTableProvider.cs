using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.IO;
using Granikos.Hydra.Service.Models;
using log4net;

namespace Granikos.Hydra.Service.Providers
{
    [Export(typeof(ITimeTableProvider))]
    public class TimeTableProvider : DefaultProvider<TimeTable>, ITimeTableProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TimeTableProvider));

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
                MailContent = "Test Mail Content",
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
                MailContent = "Test 2 Mail Content"
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