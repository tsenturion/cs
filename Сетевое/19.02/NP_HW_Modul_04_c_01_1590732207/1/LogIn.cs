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
		Server server = new();
		Task serverTask;
		CancellationTokenSource cts = new CancellationTokenSource();
		CancellationToken ct = new();
		public LogInForm()
		{
			InitializeComponent();
			//нужно каждому серверу будет приписать свой таск
			serverTask = new Task(delegate { Server.StartServer(server, ct); });
		}
		public void buttonConnect_Click(object sender, EventArgs e)
		{
			Socket userSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			userSocket.Connect(server.IPEnd);
			Thread.Sleep(10);
			if (userSocket.Connected)
			{
				chat = new(this.labelLogin.Text, userSocket, server);
				chat.Show();
			}
			else MessageBox.Show("Не удалось подключиться", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}
}
