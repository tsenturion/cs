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
            //Server server = new();
            LogInForm LogIn = new();
            Application.Run(LogIn); //добавить в конструктор формы список сокетов дл€ подключени€
        }
    }
    public class Server
    {
        public IPEndPoint IPEnd { get; private set; }
        Socket Socket { get; set; }
		CancellationToken CaT { get; set; }
        public class Message
        {
            string User { get; set; }
            string Text { get; set; }
            //DateTime
            public Message(string user, string text)
            {
                User = user; Text = text;
            }
        }
		public class Chat
        {
            string[] UsersLogin { get; set; } = new string[10];
            List<Message> Messages { get; set; } = new List<Message>();
			List<Message> GetMessages(string login)
            {
                if (UsersLogin.Contains(login)) return Messages;
                else return new List<Message>(); //плохо
            }
        }
        Dictionary<string, List<Chat>> userChats;

		public Server() //пока что лишь один сервер на одном и том же сокете
        {
            IPEnd = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1024);
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //tcp or ip
            Socket.Bind(IPEnd);
        }

        public void CreateChat(string login);
        public Chat GetChat(string login)
        public static void StartServer(Server server, CancellationToken ct)
        {
            server.Socket.Listen();
			Socket userSocket = server.Socket.Accept();
            //userSocket.Send(Encoding.UTF8.GetBytes($"Hello, {userSocket.RemoteEndPoint}!\n"));
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