using System.Runtime.InteropServices;

namespace Program
{
	class Program
	{
		[DllImport("User32.dll")]
		static extern void MessageBox(IntPtr hWnd, string Text, string Caption, uint Type);
		static uint MB_OK = 0x00000000;
		static uint MB_ICONINFORMATION = 0x00000040;
		static void Main()
		{
			Console.WriteLine("Задание 1");
			MessageBox(IntPtr.Zero, "Hello, World!", "MessageBox", MB_OK | MB_ICONINFORMATION);

			Console.WriteLine("Задание 2\nНапишите число от 0 до 100");
			int num = -1;
			num = Convert.ToInt32(Console.ReadLine());
			Random rand = new Random();
			int pcAnswer = rand.Next(0, 101);
			if (pcAnswer == num) MessageBox(IntPtr.Zero, $"Ваше число равно {pcAnswer}", "Отгадка", MB_OK | MB_ICONINFORMATION);
			else MessageBox(IntPtr.Zero, $"Не угадал.. Мой ответ: {pcAnswer}", "Отгадка", MB_OK | MB_ICONINFORMATION);
		}
	}
}