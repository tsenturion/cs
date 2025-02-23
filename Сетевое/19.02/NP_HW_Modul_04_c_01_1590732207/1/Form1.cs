namespace _1
{
	public partial class Form1 : Form
	{
		Server server = new();
		Task serverTask;
		CancellationTokenSource cts = new CancellationTokenSource();
		CancellationToken ct = new();
		public Form1()
		{
			InitializeComponent();
			serverTask = new Task(delegate { Server.StartServer(server, ct); });
		}

		private void checkBoxConnect_CheckedChanged(object sender, EventArgs e)
		{
			if(checkBoxConnect.Checked)
			{
				serverTask.Start();
				checkBoxConnect.Text = "Disconnect";
			}
			else if (!checkBoxConnect.Checked) 
			{
				cts.Cancel();
				checkBoxConnect.Text = "Connect";
			}

		}
	}
}
