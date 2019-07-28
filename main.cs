using System;


namespace YJServer
{
    class Program
    {
        static void Main(string[] args)
        {
            GameServer server = new GameServer();
            server.init("127.0.0.1",9002);
            Console.ReadKey();
        }
    }
}
