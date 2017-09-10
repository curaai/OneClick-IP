using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace IpSetter
{
    class IpConf
    {
        string ip_address;
        string subnet_mask;
        string gateway;
        string dns;
        string dnsSub;
        Boolean isDynamic;

        public IpConf()
        {
            ip_address = null;
            subnet_mask = null;
        }

        public IpConf(string ip_address, string subnet_mask, string gateway, string dns, string dnsSub, bool isDynamic)
        {
            Setting(ip_address, subnet_mask, gateway, dns, dnsSub, isDynamic);
        }

        // if configed return true else return False
        public bool IsConfiged()
        {
            return this.ip_address != null;
        }

        public void Setting(string ip_address, string subnet_mask, string gateway, string dns, string dnsSub, bool isDynamic)
        {
            this.ip_address = ip_address;
            this.subnet_mask = subnet_mask;
            this.gateway = gateway;
            this.dns = dns;
            this.dnsSub = dnsSub;
            this.isDynamic = isDynamic;
        }

        public void setIP()
        {
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();

            foreach (ManagementObject objMO in objMOC)
            {
                if ((bool)objMO["IPEnabled"])
                {
                    try
                    {
                        ManagementBaseObject setIP;
                        ManagementBaseObject newIP =
                            objMO.GetMethodParameters("EnableStatic");

                        newIP["IPAddress"] = new string[] { ip_address };
                        newIP["SubnetMask"] = new string[] { subnet_mask };

                        setIP = objMO.InvokeMethod("EnableStatic", newIP, null);
                    }
                    catch (Exception)
                    {
                        throw;
                    }


                }
            }
        }
        /// <summary>
        /// Set's a new Gateway address of the local machine
        /// </summary>
        /// <param name="gateway">The Gateway IP Address</param>
        /// <remarks>Requires a reference to the System.Management namespace</remarks>
        public void setGateway()
        {
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();

            foreach (ManagementObject objMO in objMOC)
            {
                if ((bool)objMO["IPEnabled"])
                {
                    try
                    {
                        ManagementBaseObject setGateway;
                        ManagementBaseObject newGateway =
                            objMO.GetMethodParameters("SetGateways");

                        newGateway["DefaultIPGateway"] = new string[] { gateway };
                        newGateway["GatewayCostMetric"] = new int[] { 1 };

                        setGateway = objMO.InvokeMethod("SetGateways", newGateway, null);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }

        public void setDNS()
        {
            ManagementClass adapterConfig = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection networkCollection = adapterConfig.GetInstances();
            foreach (ManagementObject adapter in networkCollection)
            {
                if ((bool)adapter["IPEnabled"])
                {
                    ManagementBaseObject objdns = adapter.GetMethodParameters("SetDNSServerSearchOrder");
                    if (objdns != null)
                    {
                        string[] s = { dns, dnsSub };
                        objdns["DNSServerSearchOrder"] = s;
                        adapter.InvokeMethod("SetDNSServerSearchOrder", objdns, null);
                    }
                }
            }
        }

        public void setDNSDHCP()
        {
            ManagementClass adapterConfig = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection networkCollection = adapterConfig.GetInstances();
            foreach (ManagementObject adapter in networkCollection)
            {
                if ((bool)adapter["IPEnabled"])
                {
                    ManagementBaseObject objdns = adapter.GetMethodParameters("EnableDHCP");
                    if (objdns != null)
                    {
                        adapter.InvokeMethod("EnableDHCP", objdns, null);
                    }
                }
            }
        }
    }
}
