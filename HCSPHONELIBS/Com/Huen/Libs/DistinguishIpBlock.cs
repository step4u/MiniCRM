using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Com.Huen.Libs
{
    public class DistinguishIpBlock
    {
        public DistinguishIpBlock() { }

        private static List<string> IpBlocks = new List<string>() {
            "10.0.0.0/8 ",
            "172.16.0.0/12",
            "192.168.0.0/16 "
        };

        public static bool IsIpNat(string ip)
        {
            IPAddress incomingIp = IPAddress.Parse(ip);
            foreach (var subnet in IpBlocks)
            {
                IPNetwork network = IPNetwork.Parse(subnet);

                if (IPNetwork.Contains(network, incomingIp))
                    return true;
            }
            return false;
        }

        public static IPAddress GetIPAddress()
        {
            IPAddress ip = null;
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            int count = host.AddressList.Where(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && IsIpNat(x.ToString()) == false).Count();

            if (count > 0)
            {
                ip = host.AddressList.FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && IsIpNat(x.ToString()) == false);
            }
            else
            {
                ip = host.AddressList.FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && IsIpNat(x.ToString()) == true);
            }

            return ip;
        }

        public static int IpAddress2Int()
        {
            int intAddress;

            if (BitConverter.IsLittleEndian)
            {
                intAddress = BitConverter.ToInt32(IPAddress.Parse(GetIPAddress().ToString()).GetAddressBytes(), 0);
            }
            else
            {
                intAddress = BitConverter.ToInt32(IPAddress.Parse(GetIPAddress().ToString()).GetAddressBytes().Reverse().ToArray(), 0);
            }

            return intAddress;
        }

    }
}
