using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HydraClient.ConfigurationService;

namespace HydraClient
{
    public class SubnetConfiguration
    {
        public IPAddress Address { get; set; }

        public int Size { get; set; }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var cs = new ConfigurationServiceClient();

            ServerBindings = cs.GetServerBindings();

            AllowedSubnets = new []
            {
                new SubnetConfiguration
                {
                   Address = IPAddress.Parse("127.0.0.1"),
                   Size = 24
                }
            };

            DataContext = this;
        }

        public ServerBindingConfiguration[] ServerBindings { get; set; }

        public SubnetConfiguration[] AllowedSubnets { get; set; }
    }
}
