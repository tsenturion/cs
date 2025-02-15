using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Exam2
{
	internal static class Program
	{
		[DllImport("kernel32.dll")]
		static extern bool AllocConsole();

		delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll")]
		static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr GetModuleHandle(string lpModuleName);

		private const int WH_KEYBOARD_LL = 13;
		private const int WM_KEYDOWN = 0x0100;
		static HookProc proc = HookCallback;
		static IntPtr hook = IntPtr.Zero;

		[STAThread]
		static void Main()
		{

			hook = SetHook(proc);
			//ApplicationConfiguration.Initialize();
			Form1 form = new Form1();
			Application.Run(form);
			//AllocConsole();
			//Console.ReadKey();
			UnhookWindowsHookEx(hook);
		}
		static IntPtr SetHook(HookProc proc)
		{
			using (Process currProcess = Process.GetCurrentProcess())
			using (ProcessModule currModule = currProcess.MainModule)
			{
				return SetWindowsHookEx(WH_KEYBOARD_LL,
					proc,
					GetModuleHandle(currModule.ModuleName),
					0);
			}
		}
		
		delegate void SetFormText(string text);
		static SetFormText labelProc = SetText;
		private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if ((nCode >= 0) && (wParam == (IntPtr)WM_KEYDOWN))
			{
				int vkCode = Marshal.ReadInt32(lParam);
				if (((Keys)vkCode == Keys.LWin) || ((Keys)vkCode == Keys.RWin))
				{

					return (IntPtr)1;
				}
			}
			return CallNextHookEx(hook, nCode, wParam, lParam);
		}


	}
}