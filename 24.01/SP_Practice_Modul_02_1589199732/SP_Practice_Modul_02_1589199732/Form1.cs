using System.Diagnostics;
using System.Windows.Forms;

namespace SP_Practice_Modul_02_1589199732
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
			LoadProcesses();

		}
		private void Form1_Load(object sender, EventArgs e)
		{
		}
		private void LoadProcesses()
		{
			listBox1.Items.Clear();
			Process[] processes = Process.GetProcesses();
			foreach (Process process in processes)
			{
				try
				{
					listBox1.Items.Add($"{process.ProcessName}{Path.GetExtension(process.MainModule.FileName)}");
				}
				catch (Exception e)
				{
					listBox1.Items.Add($"{process.ProcessName}");
				}
			}
		}
		private void TimerTick(object obj)
		{
			LoadProcesses();
		}

		private void numericUpDown1_ValueChanged(object sender, EventArgs e)
		{
			if (numericUpDown1.Value < 1) numericUpDown1.Value = 1;
			timer1.Interval = (int)numericUpDown1.Value * 1000;
			
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			LoadProcesses();
		}
	}
}
