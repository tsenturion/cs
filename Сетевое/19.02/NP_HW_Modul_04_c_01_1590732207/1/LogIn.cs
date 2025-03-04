using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;

namespace _1
{
	
	public partial class LogInForm : Form
	{
		ChatForm chat = null;
		public Server Server { get; set; } = new("ChatP2P v1", "127.0.0.1", 1024);
		Task serverTask;
		CancellationTokenSource cts = new CancellationTokenSource();
		CancellationToken ct;
		public LogInForm()
		{
			InitializeComponent();
			//нужно каждому серверу будет приписать свой таск
			ct = cts.Token;
			comboBoxServersList.Items.Add(Server.Name);
			serverTask = Task.Run(delegate { Server.StartServer(Server, ct); });
		}
		public async void buttonConnect_Click(object sender, EventArgs e)
		{
			Socket userSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			await userSocket.ConnectAsync(Server.IPEnd);

			//!!!
			//Серверу отправляется логин и пароль, он их проверяет и на основе этого дает подключение, либо нет
			//await userSocket.SendAsync(Encoding.UTF8.GetBytes("/log_in,pass"));

			if (userSocket.Connected)
			{
				chat = new(this.textBoxLogin.Text, userSocket, this);
				chat.Show();
			}
			else MessageBox.Show("Не удалось подключиться", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}
}
