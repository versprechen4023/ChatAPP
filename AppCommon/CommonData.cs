using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AppCommon
{
    public class CommonData
    {
        /// <summary>
        /// 데이터 전송시 최대 값(5mb)
        /// </summary>
        const int MAX_DATA_SIZE = 5120;

        /// <summary>
        /// 전송 데이터 버퍼링
        /// </summary>
        public byte[] Data = new byte[MAX_DATA_SIZE];

        /// <summary>
        /// 통신할 클라이언트
        /// </summary>
        public TcpClientExtension Client {  get; private set; }

        public CommonData(TcpClientExtension clientInfo)
        {
            Client = clientInfo;
        }

        /// <summary>
        /// 데이터 통신시 한글 깨짐 방지를 위해 UTF8로 인코딩
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Encoding.UTF8.GetString(Data);
        }
    }

    /// <summary>
    /// 액티브에 접근하기위한 TcpListner 상속 클래스
    /// </summary>
    public class TcpListenerExtension : TcpListener
    {
        /// <summary>
        /// 통신시 액티브 대기여부를 검증 대기중일경우 ture 그렇지 않을경우 false
        /// </summary>
        public new bool Active => base.Active;
        public TcpListenerExtension(IPEndPoint endPoint) : base(endPoint) { }
    }

    /// <summary>
    /// 통신시 접속중인 클라이언트를 확인하기 위한 클래스
    /// TcpClient의 소켓 프로퍼티에 접근하기 편하게 하기위해 참조한 확장 클래스
    /// </summary>
    public class TcpClientExtension
    {
        public EndPoint RemoteEndPoint => Socket.RemoteEndPoint;

        public TcpClient Client { get; private set; }
        public Socket Socket => Client.Client;
        public TcpClientExtension(TcpClient client)
        {
            Client = client;
        }

        public TcpClientExtension(IPEndPoint endPoint)
        {
            Client = new TcpClient(endPoint);
        }
    }
}
