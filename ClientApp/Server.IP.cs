using AppCommon;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace ServerApp
{
	public partial class ServerClient
	{
		/// <summary>
		/// 아이피 체크 전용 함수 대리자
		/// </summary>
		/// <param name="ip"></param>
		/// <returns></returns>
		private delegate bool IpCheckDelegate(string ip);

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

		/// <summary>
		/// 한국 아이피 확인
		/// </summary>
		/// <param name="ip"></param>
		/// <returns></returns>
		private bool IsKRIP(string ip)
		{
			try
			{
				// 사설 아이피 확인
				if (IsPrivateIP(ip)) { return true; }

				// url 작성
				string apiKey = "Sxg%2F3MQsYRuNFuYI2f3UnLO4QMtIfT00P9C0OxBzEE79CYCM%2BfGA354hNLUFpw76Ka7aMCl4wIgY8AXCwz4Krg%3D%3D";
				string url = "http://apis.data.go.kr/B551505/whois/ipas_country_code";
				url += $"?ServiceKey={apiKey}"; // Service Key
				url += $"&query={ip}";
				url += "&answer=json";

				// 리퀘스트 작성
				var request = (HttpWebRequest)WebRequest.Create(url);
				request.Method = "GET";
				request.ContentType = "application/json";

				// 리스폰스 응답 저장
				using (var response = request.GetResponse() as HttpWebResponse)
				{
					StreamReader reader = new StreamReader(response.GetResponseStream());
					string json = reader.ReadToEnd();
					JObject jsonData = JObject.Parse(json);

					// JSON 데이터에서 컨트리코드를 가져온다
					string countryCode = jsonData["response"]["whois"]["countryCode"].ToString();

					// 한국 아이피면 True 반환
					if (countryCode.Equals("KR"))
					{
						return true;
					}
					else
					{
						return false;
					}
				}
			}
			catch (Exception ex)
			{
				UpdateLog(ex.Message); return false;
			}
		}

		/// <summary>
		/// 사설 IP 검증
		/// </summary>
		/// <param name="ip"></param>
		/// <returns></returns>
		private bool IsPrivateIP(string ip)
		{
			var ipBytes = IPAddress.Parse(ip).GetAddressBytes();
			switch (ipBytes[0])
			{
				case 10:
					return true; // 10.0.0.1
				case 172:
					return ipBytes[1] <= 31 && ipBytes[1] >= 16; // 172.16~31
				case 192:
					return ipBytes[1] == 168; // 192.168
				default:
					return false;
			}
		}

		/// <summary>
		/// 해외 or 사설아이피 체크 함수
		/// </summary>
		/// <param name="clientInfo"></param>
		/// <param name="ip"></param>
		/// <param name="method">IsKRIP, IsPrivateIP 메서드</param>
		/// <returns></returns>
		private bool CheckIp(TcpClientExtension clientInfo, IPEndPoint ip, string errorMsg, IpCheckDelegate method)
		{
			var result = method(ip.Address.ToString());

			if (!result)
			{
				UpdateLog($"{clientInfo.Client.Client.RemoteEndPoint} {errorMsg}");
				clientInfo.Socket.Close();
			}

			return result;
		}
	}
}
