using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Threading;

namespace WindowsFormsApp1
{
	public partial class Form1 : Form
	{
		[DllImport("user32.dll")]
		private static extern void MessageBoxW(IntPtr hWnd, string text, string caption, uint type);
		public const uint MB_OK = 0x00000000;
		public const uint MB_ICONINFORMATION = 0x00000040;
		public const uint MB_ICONWARNING = 0x00000030;
		public const uint MB_ICONQUESTION = 0x00000020;

		[DllImport("user32.dll")]
		private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
		[DllImport("User32.dll")]
		static extern bool SendMessage(IntPtr hWnd, uint Msg, IntPtr lParam, string wParam);
		const uint WM_SETTEXT = 0x000C;
		const uint WM_CLOSE = 0x0010;
		public Form1()
		{
			InitializeComponent();
			//Process.Start("notepad.exe");
		}

		private void button3_Click(object sender, EventArgs e)
		{
			SendMessage(FindWindow("Notepad", null), WM_CLOSE, IntPtr.Zero, null); //работает, только если блокнот запускать вручную
		}

		string Time = DateTime.Now.ToString();
		private void button4_Click(object sender, EventArgs e)
		{
			SendMessage(FindWindow("Notepad", null), WM_SETTEXT, IntPtr.Zero, Time); //работает, только если блокнот запускать вручную
			timer1.Enabled = true;
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			Time = DateTime.Now.ToString();
			SendMessage(FindWindow("Notepad", null), WM_SETTEXT, IntPtr.Zero, Time);
		}
	}
}
