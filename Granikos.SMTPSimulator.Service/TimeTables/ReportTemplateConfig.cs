using System.Configuration;

namespace Granikos.SMTPSimulator.Service.TimeTables
{
    public class ReportTemplateConfig : ConfigurationSection
    {
        public static ReportTemplateConfig GetConfig()
        {
            return (ReportTemplateConfig)ConfigurationManager.GetSection("ReportTemplate") ?? new ReportTemplateConfig();
        }

        [ConfigurationProperty("Subject", IsRequired = true)]
        public ConfigurationTextElement<string> SubjectTemplateElement
        {
            get
            {
                return this["Subject"] as ConfigurationTextElement<string>;
            }
        }

        public string SubjectTemplate
        {
            get
            {
                return SubjectTemplateElement.Value;
            }
        }

        [ConfigurationProperty("Text", IsRequired = true)]
        public ConfigurationTextElement<string> TextTemplateElement
        {
            get
            {
                return this["Text"] as ConfigurationTextElement<string>;
            }
        }

        public string TextTemplate
        {
            get
            {
                return TextTemplateElement.Value;
            }
        }

        [ConfigurationProperty("RowText", IsRequired = true)]
        public ConfigurationTextElement<string> RowTextTemplateElement
        {
            get
            {
                return this["RowText"] as ConfigurationTextElement<string>;
            }
        }

        public string RowTextTemplate
        {
            get
            {
                return RowTextTemplateElement.Value;
            }
        }

        [ConfigurationProperty("Html", IsRequired = true)]
        public ConfigurationTextElement<string> HtmlTemplateElement
        {
            get
            {
                return this["Html"] as ConfigurationTextElement<string>;
            }
        }

        public string HtmlTemplate
        {
            get
            {
                return HtmlTemplateElement.Value;
            }
        }

        [ConfigurationProperty("RowHtml", IsRequired = true)]
        public ConfigurationTextElement<string> RowHtmlTemplateElement
        {
            get
            {
                return this["RowHtml"] as ConfigurationTextElement<string>;
            }
        }

        public string RowHtmlTemplate
        {
            get
            {
                return RowHtmlTemplateElement.Value;
            }
        }
    }
}
