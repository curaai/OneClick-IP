using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Configuration;
using System.Management;


namespace IpSetter
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        IpConf configuration;

        public MainWindow()
        {
            InitializeComponent();

            configuration = LoadConf();
        }

        private IpConf LoadConf()
        {
            Boolean isDynamic = false;
            Dictionary<String, String> confDict = new Dictionary<string, string>();
            try
            {
                var appSettings = ConfigurationManager.AppSettings;

                if (appSettings.Count == 0)
                {
                    Console.WriteLine("AppSettings is empty.");
                }
                else
                {
                    foreach (String key in appSettings.AllKeys)
                    {
                        confDict[key] = appSettings[key];
                    }
                    isDynamic = confDict["isDynamic"] == "true" ? true : false;
                    // toggle.Checked = isDynamic;
                }
                return new IpConf(confDict["ip"], confDict["subnet"], confDict["gateway"], confDict["dns"], confDict["dnsSub"], isDynamic);

            }
            catch (Exception )
            {
                return new IpConf();
            }
        }

        private void SaveButtonClick(Object sender, RoutedEventArgs e)
        {
            String ip, subnet, gateway, dns, dnsSub;

            String[] octetList = new String[4];

            octetList[0] = this.ipFirstOctet.Text;
            octetList[1] = this.ipSecondOctet.Text;
            octetList[2] = this.ipThirdOctet.Text;
            octetList[3] = this.ipFourthOctet.Text;
            ip = octetList.Aggregate((cur, next) => cur + "." + next);

            octetList[0] = this.subnetFirstOctet.Text;
            octetList[1] = this.subnetSecondOctet.Text;
            octetList[2] = this.subnetThirdOctet.Text;
            octetList[3] = this.subnetFourthOctet.Text;
            subnet = octetList.Aggregate((cur, next) => cur + "." + next);

            octetList[0] = this.gateFirstOctet.Text;
            octetList[1] = this.gateSecondOctet.Text;
            octetList[2] = this.gateThirdOctet.Text;
            octetList[3] = this.gateFourthOctet.Text;
            gateway = octetList.Aggregate((cur, next) => cur + "." + next);

            octetList[0] = this.dnsFirstOctet.Text;
            octetList[1] = this.dnsSecondOctet.Text;
            octetList[2] = this.dnsThirdOctet.Text;
            octetList[3] = this.dnsFourthOctet.Text;
            dns = octetList.Aggregate((cur, next) => cur + "." + next);

            octetList[0] = this.dnsSubFirstOctet.Text;
            octetList[1] = this.dnsSubSecondOctet.Text;
            octetList[2] = this.dnsSubThirdOctet.Text;
            octetList[3] = this.dnsSubFourthOctet.Text;
            dnsSub = octetList.Aggregate((cur, next) => cur + "." + next);
            

            SaveConfig(ip, subnet, gateway, dns, dnsSub);
        }

        private void SaveConfig(String ip, String subnet, String gateway, String dns, String dnsSub)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                if (config.AppSettings.Settings["ip"] == null)
                {
                    config.AppSettings.Settings.Add("ip", ip);
                    config.AppSettings.Settings.Add("subnet", subnet);
                    config.AppSettings.Settings.Add("gateway", gateway);
                    config.AppSettings.Settings.Add("dns", dns);
                    config.AppSettings.Settings.Add("dnsSub", dnsSub);
                    config.AppSettings.Settings.Add("isDynamic", GetLocalIpAllocationMode());

                    configuration.Setting(ip, subnet, gateway, dns, dnsSub, GetLocalIpAllocationMode() == "true" ? true : false );
                }
                else
                {
                    config.AppSettings.Settings["ip"].Value = ip;
                    config.AppSettings.Settings["subnet"].Value = subnet;
                    config.AppSettings.Settings["gateway"].Value = gateway;
                    config.AppSettings.Settings["dns"].Value = dns;
                    config.AppSettings.Settings["dnsSub"].Value = dnsSub;
                    config.AppSettings.Settings["isDynamic"].Value = GetLocalIpAllocationMode();
                }

                config.Save();
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString() + "Confirmation");
            }
        }

        // i DHCP return true else false
        public static String GetLocalIpAllocationMode()
        {
            String MethodResult;
            try
            {
                ManagementObjectSearcher searcherNetwork = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_NetworkAdapterConfiguration");

                Dictionary<string, string> Properties = new Dictionary<string, string>();

                foreach (ManagementObject queryObj in searcherNetwork.Get())
                {
                    foreach (var prop in queryObj.Properties)
                    {
                        if (prop.Name != null && prop.Value != null && !Properties.ContainsKey(prop.Name))
                        {
                            Properties.Add(prop.Name, prop.Value.ToString());
                        }
                    }
                }

                MethodResult = Properties["DHCPEnabled"].ToLower() == "true" ? "true" : "false";
            }
            catch (Exception ex)
            {
                return "false";
            }

            return MethodResult;
        }

        private void toggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (configuration.IsConfiged())
            {
                configuration.setIP();
                configuration.setGateway();
                configuration.setDNS();
            }
            else
            {
                MessageBox.Show("interface 설정을 해주십시오", "Confirmation");
            }
        }

        private void toggle_Checked(object sender, RoutedEventArgs e)
        {
            if (configuration.IsConfiged())
            {
                configuration.setDNSDHCP();
            }
            else
            {
                MessageBox.Show("interface 설정을 해주십시오", "Confirmation");
            }
        }
    }
}
