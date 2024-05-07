using System.Net;
using System.Net.Sockets;

namespace MainApp
{
	public partial class MainClient
	{
		/// <summary>
		/// 사설 IP 처리
		/// </summary>
		/// <returns></returns>
		private string GetLocalIP()
		{
			string localIP = string.Empty;

			var host = Dns.GetHostEntry(Dns.GetHostName());

			foreach (var ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					localIP = ip.ToString();
					break;
				}
			}
			return localIP;
		}

		/// <summary>
		/// 공인 IP 처리
		/// </summary>
		/// <returns></returns>
		private string GetPublicIP()
		{
			string publicIP = new WebClient().DownloadString("http://ipinfo.io/ip").Trim();

			if (string.IsNullOrWhiteSpace(publicIP))
			{
				publicIP = GetLocalIP();
			}
			return publicIP;
		}
	}
}
