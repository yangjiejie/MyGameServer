using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YJServer
{
    public class HeartBeat
    {
        public Socket m_socket;
        public GameServer m_gs;
        public int lastrecvTime = -1;
        public int m_clientId = -1;
        //接受消息 
        SocketAsyncEventArgs m_receiveEventArgs = null;
        //发送消息
        SocketAsyncEventArgs m_sendEventArgs = null;

        Timer timer = null;


        public HeartBeat(Socket s, GameServer gs,int clientId)
        {
            m_clientId = clientId;
            m_socket = s;
            m_gs = gs;
            m_receiveEventArgs = new SocketAsyncEventArgs();
            m_receiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecv);

            m_sendEventArgs = new SocketAsyncEventArgs();
            this.Recv();

            timer = new Timer(Update, null, 0, 1000); //参数2 是update函数的参数

           
           
        }

        public void Update(object state)
        {
            var str = state as string;
            
            Console.WriteLine("执行心跳的update逻辑");
            
            Judge();

            Send();

        }

        public void Send()
        {
            if(m_socket == null)
            {
                return; 
            }
            IOSocket io = new IOSocket();
            io.WriteInt32(NetMsgDefine.HeadBeat);
            io.WriteByte(1);
            m_sendEventArgs.SetBuffer(io.GetBuffer(), 0, io.GetLength());
            m_socket.SendAsync(m_sendEventArgs);
        }

        public void Judge()
        {
            Console.WriteLine("judge");
            if (lastrecvTime != -1 && Environment.TickCount - lastrecvTime > 3000)
            {
                Console.WriteLine("应该关闭socket");
                CloseConnect();
            }
        }

        public void CloseConnect()
        {
            lastrecvTime = -1;
            timer.Dispose();
            timer = null;
            m_gs.CloseSocket(this.m_clientId);
            m_clientId = -1;
            m_receiveEventArgs = null;
            m_socket = null;
            //心跳对象也要关闭 （心跳是不是要写一个管理类） c#没有 delete对象之说  
        }
          

        public void OnRecv(object target, SocketAsyncEventArgs e)
        {
            IOSocket io = new IOSocket(e.Buffer); // 思考下这里怎么避免重复new对象 避免gc
            io.Seek(0);
            int type = io.ReadInt32();
            switch (type)
            {
                case NetMsgDefine.HeadBeat:
                    {
                        lastrecvTime = Environment.TickCount;
                        Console.WriteLine("收到客户端心跳包"+io.ReadByte().ToString());
                        Recv();
                    }
                    
                    break;
                default:
                    break;
            }
        }

        public void Recv()
        {
            byte[] by = new byte[1000];
            m_receiveEventArgs.SetBuffer(by, 0, 1000); 
            m_socket.ReceiveAsync(m_receiveEventArgs);

        }
        


    }
}
