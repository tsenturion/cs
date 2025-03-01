using System.Net.Sockets;
using System.Text;

namespace _1
{
	public partial class ChatForm : Form
	{
		string CurrentUserLogin { get; set; }
		Server ActiveServer { get; set; }
		Socket Client { get; set; }
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
			await Client.ReceiveAsync(buffer);
			//await ActiveServer.Socket.ReceiveAsync(buffer);
			richTextBox1.Text = Encoding.UTF8.GetString(buffer);

			await ActiveServer.Socket.SendAsync(Encoding.UTF8.GetBytes("get_user_chats"));
		}

		private void buttonNewChat_Click(object sender, EventArgs e)
		{
			//ActiveServer.
		}

	}
}
