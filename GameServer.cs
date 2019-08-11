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

        
        public void init(int port)
        {
            //获取本地ip
            IPHostEntry ipe = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] localIpGroup = ipe.AddressList;
            IPAddress ipdress = null;
            for (int i = 0; i < localIpGroup.Length; i++)
            {

                if (localIpGroup[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    ipdress = localIpGroup[i];
                    break;
                }
            }
            //获取本地ip end
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if(m_socket == null)
            {
                return;
            }
            
            IPEndPoint endPoint = new IPEndPoint(ipdress, port);
            m_socket.Bind(endPoint);
            m_socket.Listen(20);
            m_socket.BeginAccept(call, this);
        }

        public static void AcceptCallback(IAsyncResult result)
        {
            lock (lockobj) // 测试下 不加锁可不可以 怀疑这个不是一个新线程 就是主线程回调  
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
                    clientMap.Add(self.clientId, connector);
                    connector.Recv();

                    connector.SendHello("欢迎客户端" + self.clientId.ToString() +
                        newClientSocket.RemoteEndPoint.ToString() + "登陆游戏"); // 实际服务器给客户端发消息 应该是通过某个按钮来触发
                    //或者根据具体的事物来触发 
                    //写一个心跳连接 ，如果没有收到客户端的回复 则认为短线了应该回收socket
                    new HeartBeat(newClientSocket, self,self.clientId);
                    ++self.clientId;
                }
                self.m_socket.BeginAccept(self.call, self);
            }
        }

        public void CloseSocket(int clientId)
        {
            if(!clientMap.ContainsKey(clientId))
            {
                return;
            }
            var connectorTmp  = clientMap[clientId];
            Console.WriteLine(connectorTmp.ConnectSocket.RemoteEndPoint.ToString() + "退出连接,连接号为" + clientId.ToString());
            connectorTmp.Close();
            clientMap.Remove(clientId);

        }


    }
}
