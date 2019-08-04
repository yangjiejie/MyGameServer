using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace YJServer
{
    public class Connector
    {
        Socket m_socket; // 服务器和客户端 沟通的socket

        Socket m_serverSocket; // 服务器socket

        //接受消息 
        SocketAsyncEventArgs m_receiveEventArgs = null;
        SocketAsyncEventArgs m_sendEventArgs = null;


        //发送消息 
        public Connector(Socket so, Socket server)
        {
            m_serverSocket = server;
            m_socket = so;
            m_receiveEventArgs = new SocketAsyncEventArgs();
            m_receiveEventArgs.UserToken = "";
            m_receiveEventArgs.RemoteEndPoint = m_socket.LocalEndPoint;
            m_receiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecv);

            m_sendEventArgs = new SocketAsyncEventArgs();

            m_sendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSend);

        }
        public void Init(Socket so)
        {
            m_socket = so;
        }
        public void Exit()
        {
            if (m_socket != null)
            {
                m_socket.Close();
                m_socket = null;
            }
        }
        public void Recv()
        {
            byte[] buffer = new byte[400];

            m_receiveEventArgs.RemoteEndPoint = m_socket.RemoteEndPoint;
            m_receiveEventArgs.SetBuffer(buffer, 0, 400);
           
            m_socket.ReceiveAsync(m_receiveEventArgs);

        }

        public void Send()
        {
            var buffer2 = Encoding.Default.GetBytes("欢迎登陆小哥");
            //var  buffer = new byte[1000];
            //Array.Copy(buffer2, buffer, buffer2.Length);
            m_sendEventArgs.SetBuffer(buffer2, 0, buffer2.Length);

            m_socket.SendAsync(m_sendEventArgs);
        }
        public void OnRecv(object sender, SocketAsyncEventArgs e)
        {
            int n = e.BytesTransferred;

            string str = System.Text.Encoding.Default.GetString(e.Buffer);
            Console.WriteLine(str);
            this.Recv(); // 成功recv之后继续recv 我们测试客户
        }

        public void OnSend(object sender, SocketAsyncEventArgs e)
        {

        }
        // 服务器发送心跳包 
        public void SendXingTiaoPackage()
        {

        }
        //客户都回复心跳包
        public void RecvXingTiaoPackage()
        {

        }


    }
}
