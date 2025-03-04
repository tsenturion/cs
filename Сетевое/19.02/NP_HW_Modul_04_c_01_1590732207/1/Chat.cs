using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace _1
{
	public partial class ChatForm : Form
	{
		//подключить отдельно
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
		string CurrentUserLogin { get; set; }
		Server ActiveServer { get; set; }
		Socket Client { get; set; }
		JsonSerializer Serializer = Newtonsoft.Json.JsonSerializer.CreateDefault();
		public ChatForm(string login, Socket userSocket, LogInForm logInForm)
		{
			InitializeComponent();
			//dictionary<this_user, dictionary<other_user, List<Message> > >

			//dictionary<this_user, List<Chat>
			//class Chat
			//List<Users>
			//List<Messages>
			CurrentUserLogin = login;
			ActiveServer = logInForm.Server;
			Client = userSocket;
			//foreach (var other_user in server.GetChats(login)
			//{
			//	other_user
			//}
			//comboBox1.Items.Add();
		}
		private async void ChatForm_Load(object sender, EventArgs e)
		{
			byte[] buffer = new byte[1024];
			//await Client.ReceiveAsync(buffer);
			//await ActiveServer.Socket.ReceiveAsync(buffer);

			await Client.SendAsync(Encoding.UTF8.GetBytes($"/get_user_chats,{CurrentUserLogin};"));
			await Client.ReceiveAsync(buffer);

			{
				string temp = Encoding.UTF8.GetString(buffer);
				string answer = Encoding.UTF8.GetString(buffer).Split(";")[0];
				comboBoxChats.Items.Add(answer);
			}

			//await ActiveServer.Socket.SendAsync(Encoding.UTF8.GetBytes("get_user_chats"));
		}

		private async void buttonNewChat_Click(object sender, EventArgs e)
		{
			//await Client.SendAsync(Encoding.UTF8.GetBytes($"/create_chat,{CurrentUserLogin};"));

		}

		private void comboBoxChats_SelectedIndexChanged(object sender, EventArgs e)
		{
		}

		private void textBoxMessage_TextChanged(object sender, EventArgs e)
		{
		}

		private async void buttonSend_Click(object sender, EventArgs e)
		{
			Message message = new(CurrentUserLogin, textBoxMessage.Text, Convert.ToInt32(comboBoxChats.SelectedText.Split(":")[0]));
			richTextBox1.Text += $"\n{message.User}: {message.Text}\n";

			string json = JsonConvert.SerializeObject(message);
			await Client.SendAsync(Encoding.UTF8.GetBytes($"/message,{json};"));
			
		}
	}
}
