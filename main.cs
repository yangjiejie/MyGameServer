using System;
using System.Net;
using System.Net.Sockets;

namespace YJServer
{
    class Program
    {
        static void Main(string[] args)
        {
            GameServer server = new GameServer();

            //自动获取本地ip地址 
            
            
            server.init(9002);
            Console.WriteLine("启动服务器");
            Console.ReadKey();
        }
    }
}
