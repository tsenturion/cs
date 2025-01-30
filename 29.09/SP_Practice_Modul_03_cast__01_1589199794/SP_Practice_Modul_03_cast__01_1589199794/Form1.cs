using System.Windows.Forms;

namespace SP_Practice_Modul_03_cast__01_1589199794
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{

		}
		private void GenerateNumbers()
		{
			if(richTextBox1.InvokeRequired)
			{
				Invoke(delegate
				{
					for (int i = (int)numericUpDownStartValue.Value; i < numericUpDownEndValue.Value; i++)
					{
						richTextBox1.Text += $"{i} ";
					}
				});
			}
			else
			{
				for (int i = (int)numericUpDownStartValue.Value; i < numericUpDownEndValue.Value; i++)
				{
					richTextBox1.Text += $"{i} ";
				}
			}
		}

		private void buttonStart_Click(object sender, EventArgs e)
		{
			for(int i=0;i<numericUpDownThreadsNumber.Value;i++)
			{
				Thread thread = new Thread(GenerateNumbers);
				thread.Start();
			}
		}
	}
}
