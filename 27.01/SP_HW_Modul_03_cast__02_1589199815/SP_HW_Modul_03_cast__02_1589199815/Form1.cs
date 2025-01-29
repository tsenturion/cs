using System.Windows.Forms;

namespace SP_HW_Modul_03_cast__02_1589199815
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}
		private void buttonStart_Click(object sender, EventArgs e)
		{
			int barCount = (int)numericUpDown1.Value;
			panel1.Controls.Clear();

			for (int i = 0; i < barCount; i++)
			{
				ProgressBar progressBar = new ProgressBar();
				progressBar.SetBounds(20, 30 * i, 300, 20);
				panel1.Controls.Add(progressBar);

				Task.Run(() => AnimateProgressBar(progressBar));
			}
		}

		private void AnimateProgressBar(ProgressBar progressBar)
		{
			Random random = new Random((int)DateTime.Now.Ticks);
			progressBar.ForeColor = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
			while (progressBar.Value != progressBar.Maximum)
			{
				progressBar.Invoke((MethodInvoker)delegate
				{
					progressBar.Step = random.Next(progressBar.Minimum, 10);
					progressBar.PerformStep();
				});
				Thread.Sleep(random.Next(100, 500));
			}
		}
	}
}