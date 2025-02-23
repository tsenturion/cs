using System.Net;
using System.Net.Sockets;
using System.Text;

namespace _1
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
    internal class Server
    {
        IPEndPoint IPEnd { get; set; }
        Socket Socket { get; set; }
		CancellationToken CaT { get; set; }
		//Dictionary<nickname, dictionary>
		Dictionary<string, string> Messages { get; set; } = null;
        public Server()
        {
            IPEnd = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1024);
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //tcp or ip
            Socket.Bind(IPEnd);
        }
        public static void StartServer(Server server, CancellationToken ct)
        {
            server.Socket.Listen();
			Socket userSocket = server.Socket.Accept();
            userSocket.Send(Encoding.UTF8.GetBytes($"Hello, {userSocket.RemoteEndPoint}!\n"));
            //server.Socket.BeginAccept(BeginAcceptCallback, server.Socket);

            byte[] buffer = new byte[1024];
            while(ct.IsCancellationRequested)
            {
                userSocket.Receive(buffer);
				userSocket.Send(Encoding.UTF8.GetBytes("\r\n"));
				userSocket.Send(buffer);
			}
			userSocket.Send(Encoding.UTF8.GetBytes("Bye!\r\n"));
            //userSocket.Shutdown(SocketShutdown.Both);
            userSocket.Close();

        }
        static void BeginAcceptCallback(IAsyncResult iar)
        {
            Socket serverSocket = (Socket)iar;
            IAsyncResult userIAR = null;
            Socket userSocket = serverSocket.EndAccept(userIAR);

            string message = "";
            userSocket.Send(Encoding.UTF8.GetBytes("\r\n¬ведите логин:"));
        }
    }
}