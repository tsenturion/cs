using System.Runtime.InteropServices;

namespace HomeWork
{
	class HomeWork()
	{
		//Win32 API использует неуправляемые типы данных (например, HANDLE, DWORD, LPSTR и т.д.), которые нужно сопоставитьс управляемыми типами C# (например, IntPtr, uint, string).
		[DllImport("user32.dll")]
		private static extern void MessageBox(IntPtr hWnd, string text, string caption, uint type);
		public const uint MB_OK = 0x00000000;
		public const uint MB_ICONINFORMATION = 0x00000040;
		public const uint MB_ICONWARNING = 0x00000030;
		public const uint MB_ICONQUESTION = 0x00000020;

		[DllImport("user32.dll")]
		private static extern IntPtr FindWindow(string ClassName, string WindowName);
		[DllImport("Kernel32.dll")]
		private static extern bool Beep(uint Frequency, uint Duration);
		[DllImport("user32.dll")]
		private static extern bool MessageBeep(uint Type);

		private static readonly int C4 = 261, D4 = 293, E4 = 329, F4 = 349,
							   G4 = 392, A4 = 440, B4 = 493, C5 = 523;
		static void Main()
		{
			Console.Title = "HomeWork";
			Console.WriteLine("Задание 1");
			MessageBox(0, "Агашков", "Фамилия", MB_OK | MB_ICONINFORMATION);
			MessageBox(0, "Лев", "Имя", MB_OK | MB_ICONQUESTION);
			MessageBox(0, "Олегович", "Jтчество", MB_OK | MB_ICONWARNING);

			Console.WriteLine("Задание 2");
			IntPtr window = FindWindow("", "HomeWork");
			if (window == IntPtr.Zero) Console.WriteLine("Окно не найдено(");
			else Console.WriteLine("Выберите действие"); //почему то не находится окно

			Console.WriteLine("Задание 3");

			Beep((uint)E4, 250);
			Beep((uint)E4, 250);
			Beep((uint)E4, 300);

			Thread.Sleep(500);

			Beep((uint)E4, 250);
			Beep((uint)E4, 250);
			Beep((uint)E4, 300);

			Thread.Sleep(500);

			Beep((uint)E4, 250);
			Beep((uint)G4, 250);
			Beep((uint)C4, 300);
			Beep((uint)D4, 250);
			Beep((uint)E4, 300);

			MessageBox(0, "That's all, folks!", "_EndOfSong", MB_OK | MB_ICONINFORMATION);

			//Console.WriteLine("Задание 4");

		}
	}
}