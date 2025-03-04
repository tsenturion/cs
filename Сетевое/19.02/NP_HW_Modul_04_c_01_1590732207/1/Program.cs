using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

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
			Application.Run(LogIn); //добавить в конструктор формы список сокетов для подключения
		}
	}
	public class Server
	{
		public string Name { get; private set; }
		public IPEndPoint IPEnd { get; private set; }
		public Socket Socket { get; set; }
		CancellationToken CaT { get; set; }
		public class Message
		{
			public int ChatID { get; set; }
			public string User { get; set; }
			public string Text { get; set; }
			//DateTime
			public Message(string user, string text, int chatID)
			{
				User = user; Text = text; ChatID = chatID;
			}
		}
		public class Chat
		{
			public int ID { get; set; }
			public string[] UsersLogin { get; set; } = new string[10];
			public List<Message> Messages { get; set; } = new List<Message>();
			public Chat(string[] participants)
			{
				UsersLogin = participants;
				Random r = new();
				ID = r.Next(1000000);
			}
		}
		Dictionary<string, List<Chat>> userChats { get; set; } = new();

		public Server(string name, string address, int port) //пока что лишь один сервер на одном и том же сокете
		{
			Name = name;
			IPEnd = new IPEndPoint(IPAddress.Parse(address), port);
			Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //tcp or ip
			Socket.Bind(IPEnd);

			List<Chat> testChat = new();
			testChat.Add(new Chat(["edfherae", "noname"]));
			userChats.Add("edfherae", testChat);
			//MessageLoopTask = new Task(MessageLoop);
		}
		//public void CreateChat(string login);
		//public Chat GetChat(string login)

		//Принмаем все входящие подключения, записываем и для каждого подключения включаем Цикл сообщений
		public static async Task StartServer(Server server, CancellationToken ct)
		{
			server.Socket.Listen();
			Console.WriteLine("Сервер запущен. Ожидание подключений...");
			while(!ct.IsCancellationRequested)
			{
				Socket client = await server.Socket.AcceptAsync();
				Console.WriteLine($"Подключен клиент {client.RemoteEndPoint}");
				_ = ClientHandle(client, server);
			}
			Console.WriteLine("Сервер отключен.");
			//userSocket.Send(Encoding.UTF8.GetBytes($"Hello, {userSocket.RemoteEndPoint}!\n"));
			//server.Socket.BeginAccept(BeginAcceptCallback, server.Socket);
		}
		//public string[] Commands =
		//{
		//	"log_in",
		//	"get_user_chats",
		//	"error"
		//};
		//enum Answers
		//{ 
		//	send_user_chats,
		//	error = 404
		//}
		static async Task ClientHandle(Socket client, Server server)
		{
			// ClientHandle собирает информацию о юзере и распоряжается ею
			
			//string userLogin = "";

			//byte[] temp = new byte[1024];
			//первым сообщением сервер принимает логин (и пароль)
			//дальше он лишь обрабатывает команды

			//get_user_chats\0
			//send_user_chats;List<Chat>\0

			//await client.ReceiveAsync(temp); //получаем логин
			//userLogin = Encoding.UTF8.GetString(temp);

			//информация по юзеру: логин, количество чатов, контактов
			//client.SendAsync(buffer);

			// Цикл сообщений
			// Сервер обрабатывает определённые строковые команды
			// Команда имеет вид: /command, данные
			// Сервер отвечает на команду кодом ответа и данными:
			// 400,данные;

			// Команды:
			// /log_in, /get_user_chats
			while (client.Connected)
			{
				string[] command = new string[8];
				byte[] buffer = new byte[1024];
				await client.ReceiveAsync(buffer);
				command = Encoding.UTF8.GetString(buffer).Split(";")[0].Split(",");
				switch(command[0])
				{
					case "/get_user_chats":
						{
							List<Chat> userChats;
							if (server.userChats.TryGetValue(command[1], out userChats))
							{
								string chatList = "";
								for (int i = 0; i < userChats.Count; i++)
								{
									int length = userChats[i].UsersLogin.Length;
									chatList += $"{userChats[i].ID}: ";
									for (int j=0;j< length; j++)
									{
										chatList += userChats[i].UsersLogin[j] + (j == length ? "" : ", ");
									}
								}
								await client.SendAsync(Encoding.UTF8.GetBytes(chatList + ";")); 
							}
							else await client.SendAsync(Encoding.UTF8.GetBytes("Chats not found"));
							break;
						}
					case "/message":
						{
							Message message = JsonConvert.DeserializeObject<Message>(command[1]);
							List<Chat> chats = null;
							server.userChats.TryGetValue(message.User, out chats);
							//для чатов use dictionary
							Chat chat;
							for(int i=0;i<chats.Count;i++)
							{
								if (chats[i].ID == message.ChatID) 
								{
									chats[i].Messages.Add(message);
									break;
								}
							}
							break;
						}
					default: { await client.SendAsync(Encoding.UTF8.GetBytes("Unknown command")); break; }
				}

			}
			//userSocket.Send(Encoding.UTF8.GetBytes("\r\nВведите логин:"));

			//while (ct.IsCancellationRequested)
			//{
			//	userSocket.Receive(buffer);
			//	userSocket.Send(Encoding.UTF8.GetBytes("\r\n"));
			//	userSocket.Send(buffer);
			//}
			//client.Send(Encoding.UTF8.GetBytes("Bye!\r\n"));
			client.Shutdown(SocketShutdown.Both);
			client.Close();
		}
	}
}