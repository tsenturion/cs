using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Diagnostics;

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
						clientSocket.Send(Encoding.UTF8.GetBytes(@"To end a sentence, enter !"));
						string serverAnswer;
						byte[] buffer = new byte[1024];
						string clientAnswer = "";
						while (!clientAnswer.EndsWith(@"!"));
						{
							//clientSocket.BeginReceive(buffer, 0, 10, SocketFlags.None, );
						}
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

							clientAnswer = Encoding.ASCII.GetString(buffer);
							if (clientAnswer.Contains("Bye"))
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
			//Server server = new();
			//Server.StartServer(server);

			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080));
			socket.Listen();

			Socket CS = socket.Accept();

			// Буфер для получения данных
			byte[] buffer = new byte[1024];

			StateObject so = new StateObject { Buffer = buffer, Socket = CS};
			// Запускаем асинхронное получение данных
			CS.BeginReceive(so.Buffer, 0, so.Buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), so);
			//StateObject

			Thread.Sleep(120000);
		}
		class StateObject
		{
			public byte[] Buffer { get; set; }
			public Socket Socket { get; set; }
		}
		static void ReceiveCallback(IAsyncResult ar)
		{
			Random r = new();
			string[] answers = { "Hi", "How are you", "Helo word", "HH", "What a day to be alive", "Bye" };
			StateObject so = (StateObject)ar.AsyncState;
			Socket CS = so.Socket;
			int bytesReceived = CS.EndReceive(ar);
			string answer = Encoding.UTF8.GetString(so.Buffer);

			//if (bytesReceived > 0)
			if (!answer.Contains("!"))
			{
				//CS.Send(Encoding.UTF8.GetBytes("\r\n" + answer));
				CS.BeginReceive(so.Buffer, bytesReceived, so.Buffer.Length - bytesReceived, SocketFlags.None, new AsyncCallback(ReceiveCallback), so);
			}
			else if (answer.Contains("!"))
			{
				CS.Send(Encoding.UTF8.GetBytes("\r\n" + answers[r.Next(answers.Length)] + "\r\n"));
				so.Buffer = new byte[1024];
				//сделать буфер за циклом сообщений
				//избавиться от callback
				CS.BeginReceive(so.Buffer, 0, so.Buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), so);
			}
			else
			{
				CS.Send(Encoding.UTF8.GetBytes("Connection closed."));
				CS.Close();
			}
		}
	}
}
