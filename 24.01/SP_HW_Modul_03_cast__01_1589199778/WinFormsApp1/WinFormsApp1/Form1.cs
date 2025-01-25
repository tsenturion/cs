using System.Diagnostics.Metrics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace WinFormsApp1
{
	//public class SimpleNumbersGenerator
	//{
	//	ThreadStart ts = new ThreadStart(GenerateNumbers);
	//	Thread thread = new Thread();
	//	private static string GenerateNumbers(int start, int end)
	//	{
	//		string result = "";

	//		return result;
	//	}
	//}
	public partial class Form1 : Form
	{
		static int startValue = 2;
		static int endValue = 32000;
		static string Thread1String = "";
		//EnterRange enterRange = new EnterRange();

		//static ThreadStart pts1 = new ThreadStart(Thread1_Generator);
		//Thread Thread1 = new Thread(pts1);
		//static string labelText = "";
		//static int textGenerationSpeed = 100;

		//ThreadStart ts = new ThreadStart(Thread1Func);
		Thread thread1 = new Thread(Thread1Func);
		public Form1()
		{
			InitializeComponent();
			//if (enterRange.ShowDialog() == DialogResult.OK)
			//{
			//	//Thread1.Priority = ThreadPriority.Highest;
			//	Thread1.Start();
			//	//startValue = enterRange.GetStartValue();
			//	//endValue = enterRange.GetEndValue();
			//}
			thread1.Start();
			timer1.Enabled = true;

		}
		static void Thread1Func()
		{
			for (int i = startValue; i < endValue; i++)
			{
				if (IsPrime(i)) { Thread1String += $"{i} "; }
			}
		}
		//static void SetLabel1Text(string text)
		//{
		//	//label1.Text += $"{startValue++}";
		//}

		//private async void Thread1_Generator()
		//{
		//	for(int i = 2; i < endValue; i++)
		//	{
		//		//if (IsPrime(i))
		//	}
		//}
		private void timer1_Tick(object sender, EventArgs e)
		{
			//label1.Text = labelText;
			//label1.Left -= 5;
			UpdateSNTextbox();
		}
		private void UpdateSNTextbox()
		{
			textBox1.Clear();
			textBox1.Text = Thread1String;
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

		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			if(checkBox1.Checked == true)
			{
				thread1.Start();
				checkBox1.Text = "■";
				labelSN.Text = "Stop generating";
			}
			else
			{
				thread1.Suspend();
				checkBox1.Text = "►";
				labelSN.Text = "Start generating";
			}
		}

		//	static void Main(string[] args)
		//	{
		//		ParameterizedThreadStart threadstart = new ParameterizedThreadStart(ThreadFunk);

		//		// Запуск первого потока.
		//		Thread thread1 = new Thread(threadstart);
		//		thread1.Start((object)"One");

		//		// Запуск второго потока.
		//		Thread thread2 = new Thread(threadstart);
		//		thread2.Start((object)"\t\tTwo");
		//	}

		//	static void ThreadFunk(object a)
		//	{
		//		// Получаем String из прнятого object.
		//		string ID = (string)a;

		//		for (int i = 0; i < 100; i++)
		//		{
		//			Console.WriteLine(ID);
		//		}
		//	}
	}
}
