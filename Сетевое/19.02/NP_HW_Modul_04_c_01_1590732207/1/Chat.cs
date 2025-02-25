using System.Net.Sockets;

namespace _1
{
	public partial class ChatForm : Form
	{
		string CurrentUserLogin { get; set; }
		Server ActiveServer { get; set; }
		public ChatForm(string login, Socket userSocket, Server server)
		{
			InitializeComponent();
			//dictionary<this_user, dictionary<other_user, List<Message> > >

			//dictionary<this_user, List<Chat>
			//class Chat
			//List<Users>
			//List<Messages>
			CurrentUserLogin = login;
			ActiveServer = server;
			foreach (var other_user in server.GetChats(login)
			{
				other_user
			}
			comboBox1.Items.Add();
		}

		private void buttonNewChat_Click(object sender, EventArgs e)
		{
			ActiveServer.
		}
	}
}
