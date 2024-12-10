using System.Net;
using System.Net.Sockets;

namespace HimeLib
{
    public static class GetMyIP
    {
        /// <summary>
        /// Get all IP, 通常第一個回傳的字串有很高的機率是上網IP
        /// </summary>
        public static string GetLocalIPAddressList()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            string ips = "";
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ips += ip.ToString() + "\n";
                }
            }
            return ips.ToString();
        }

        /// <summary>
        /// Get Online IP, if have no internet , it will return null
        /// </summary>
        public static string GetOnlineIPAddress()
        {
            string localIP = "";

            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                try
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    localIP = endPoint.Address.ToString();
                }
                catch { }
            }
            return localIP;
        }
    }
}