using System.Net.Sockets;
using System.Net;
using System.Text;

namespace _3
{
	public partial class Form1 : Form
	{
		enum MessageMode
		{
			Human_Human,
			Human_Server,
			Computer_Human,
			Computer_Server
		};
		class Server
		{
			static string[] answers = { "Hi", "How are you", "Helo word", "HH", "What a day to be alive", "Bye" };
			static Random r = new();
			Socket ServerSocket { get; set; } = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
			IPEndPoint IPEnd = new(IPAddress.Parse("127.0.0.1"), 1024);
			public Server()
			{
				ServerSocket.Bind(IPEnd);
				ServerSocket.Listen();
			}
			public static void StartServer(Server server)
			{
				Task StartAcceptingTask = Task.Run(() => StartAcceptingDelegate(server));
			}
			static void StartAcceptingDelegate(Server server)
			{
				while (true)
				{
					Socket clientSocket = server.ServerSocket.Accept();
					while (clientSocket.Connected)
					{
						string serverAnswer;
						byte[] buffer = new byte[1024];
						string clientAnswer;
						try
						{
							serverAnswer = answers[r.Next(answers.Length)];
							if (serverAnswer == "Bye")
							{
								clientSocket.Send(Encoding.ASCII.GetBytes(serverAnswer + "\r\n"));
								Thread.Sleep(1000);
								clientSocket.Shutdown(SocketShutdown.Both);
								clientSocket.Close();
							}
							clientSocket.Send(Encoding.ASCII.GetBytes(serverAnswer + "\r\n"));

							clientSocket.Receive(buffer);
							clientAnswer = Encoding.ASCII.GetString(buffer);
							if (clientAnswer == "Bye")
							{
								clientSocket.Send(Encoding.ASCII.GetBytes("Bye bye!" + "\r\n"));
								clientSocket.Shutdown(SocketShutdown.Both);
								clientSocket.Close();
							}
						}
						catch (Exception e)
						{
							Console.WriteLine(e.Message);
						}
					}
				}
			}
		}
		public Form1()
		{
			InitializeComponent();
			Server server = new();
			Server.StartServer(server);
			Thread.Sleep(120000);
		}
	}
}
