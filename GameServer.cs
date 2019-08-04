using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace YJServer
{

    

    public class GameServer
    {

        /// </summary>
        /// 
        static object lockobj = new object();
        
        Socket m_socket;

        Dictionary<int, Connector> clientMap = new Dictionary<int, Connector>();

        public AsyncCallback call = new AsyncCallback(AcceptCallback);

        public int clientId = 0;

        public GameServer()
        {
          
        }

        
        public void init(string strip,int port)
        {
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if(m_socket == null)
            {
                return;
            }
            IPAddress ipaddress  ;
            IPAddress.TryParse(strip, out ipaddress);
            IPEndPoint endPoint = new IPEndPoint(ipaddress,port);
            m_socket.Bind(endPoint);
            m_socket.Listen(20);
            m_socket.BeginAccept(call, this);
        }

        public static void AcceptCallback(IAsyncResult result)
        {
            lock (lockobj)
            {
                var self = (GameServer)result.AsyncState;
                if (self == null || self.m_socket == null)
                {
                    return;
                }
               
                var serverSocket = self.m_socket;
                var clientMap = self.clientMap;
                Socket newClientSocket = null;
                try
                {
                    newClientSocket = serverSocket.EndAccept(result); // 和客户端对接的socket
                    Console.WriteLine("{0}连接成功", newClientSocket.RemoteEndPoint.ToString());
                    
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
                var connector = new Connector(newClientSocket, serverSocket);
                if (!clientMap.ContainsValue(connector))
                {
                    //这里全部需要异步操作 不允许有同步的操作 
                    clientMap.Add(self.clientId++, connector);
                    connector.Recv(); 
                    connector.Send(); // 实际服务器给客户端发消息 应该是通过某个按钮来触发
                    //或者根据具体的事物来触发 
                }
                self.m_socket.BeginAccept(self.call, self);
            }
        }




    }
}
