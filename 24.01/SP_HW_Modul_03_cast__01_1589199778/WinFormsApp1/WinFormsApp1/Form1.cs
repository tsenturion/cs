using System.Diagnostics.Metrics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace WinFormsApp1
{
	public partial class Form1 : Form
	{
		static int startValue = 2;
		static int endValue = 32000;
		Thread threadSN;
		Thread threadFN;
		class ThreadParams
		{
			public TextBox TextBox { get; set; }
			public CancellationTokenSource CTS { get; set; }
			public CancellationToken CT { get; set; }
			public ThreadParams(TextBox textBox, CancellationTokenSource Cts)
			{
				CTS = new(); CT = CTS.oken;
				TextBox = textBox;
			}
		}
		ThreadParams threadSNParams;
		ThreadParams threadFNParams;
		public Form1()
		{
			InitializeComponent();

			CancellationTokenSource CtsSN = new();
			threadSNParams = new(this.textBoxSimpleNumbers, CtsSN);
			threadSN = new(new ParameterizedThreadStart(ThreadSNFunc));

			CancellationTokenSource CtsFN = new();
			threadFNParams = new(this.textBoxFibNumbers, CtsFN);
			threadFN = new(new ParameterizedThreadStart(ThreadFNFunc));
		}
		static void ThreadSNFunc(object obj)
		{
			ThreadParams tp = (ThreadParams)obj;
			TextBox textBox = tp.TextBox;
			for (int i = startValue; i < endValue; i++)
			{
				if (tp.CT.IsCancellationRequested) return;
				if (IsPrime(i)) { textBox.Invoke(() => textBox.Text += $"{i} "); }
			}
		}
		static void ThreadFNFunc(object obj)
		{
			ThreadParams tp = (ThreadParams)obj;
			TextBox textBox = tp.TextBox;
			for (int i = startValue; i < endValue; i++)
			{
				if (tp.CT.IsCancellationRequested) return;
				if (IsFibonacci(i)) { textBox.Invoke(() => textBox.Text += $"{i} "); }
			}
		}
		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			if (checkBoxSN.Checked == true)
			{
				textBoxSimpleNumbers.Text = "";
				threadSN.Start(threadSNParams);
				checkBoxSN.Text = "■";
				labelSN.Text = "Stop generating";
			}
			else
			{
				threadSNParams.CTS.Cancel();
				this.checkBoxSN.Enabled = false;
			}
		}
		private void checkBoxFibNumbers_CheckedChanged(object sender, EventArgs e)
		{
			if (checkBoxFibNumbers.Checked == true)
			{
				textBoxFibNumbers.Text = "";
				threadFN.Start(threadFNParams);
				checkBoxFibNumbers.Text = "■";
				labelFN.Text = "Stop generating";
			}
			else
			{
				threadFNParams.CTS.Cancel();
				this.checkBoxFibNumbers.Enabled = false;
			}
		}
		private static bool IsPrime(int number)
		{
			if (number < 2)
				return false;

			for (int i = 2; i * i <= number; i++)
			{
				if (number % i == 0)
					return false;
			}

			return true;
		}
		private static bool IsFibonacci(int number)
		{
			for (int fib1 = 0, fib2 = 1, temp; fib2 <= number;)
			{
				if (fib2 == number) return true;
				temp = fib1;
				fib1 = fib2;
				fib2 = temp + fib2;
			}
			return false;
		}

	}
}
