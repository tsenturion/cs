using System;
using System.Runtime.InteropServices;

namespace InteropSamples
{
	// Пример 1: Базовый DllImport
	public static class NativeMethods
	{
		[DllImport("kernel32.dll")]
		public static extern uint GetTickCount(); // Получение времени работы системы в миллисекундах

		// Для macOS/Linux можно использовать:
		// [DllImport("libc", EntryPoint = "clock_gettime")]
	}

	// Пример 2: Обработка ошибок WinAPI
	public static class ErrorExample
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool CloseHandle(IntPtr handle); // Закрытие дескриптора объекта Windows

		public static void Close(IntPtr handle)
		{
			if (!CloseHandle(handle))
			{
				// Получение кода последней системной ошибки
				int errorCode = Marshal.GetLastWin32Error();
				throw new InvalidOperationException($"Код ошибки: {errorCode}");
			}
		}
	}

	// Пример 3: Работа со строками и кодировками
	public static class StringInterop
	{
		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern int MessageBox(
			IntPtr hWnd,            // Родительское окно
			string text,            // Текст сообщения (маршалинг в Unicode)
			string caption,         // Заголовок окна
			uint type               // Тип диалога (кнопки, иконки)
		);

		// ANSI версия (устаревшая, для совместимости)
		[DllImport("user32.dll", CharSet = CharSet.Ansi)]
		public static extern int MessageBoxA(
			IntPtr hWnd,
			string text,
			string caption,
			uint type
		);
	}

	// Пример 4: Структуры с явным расположением в памяти
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct Point
	{
		public int X;               // 4 байта, смещение 0
		public int Y;               // 4 байта, смещение 4

		// Без StructLayout CLR может оптимизировать расположение полей
		// Pack = 4 означает выравнивание по 4-байтовой границе
	}

	// Пример 5: Структура с фиксированным строковым буфером
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct SystemInfo
	{
		public int processorCount;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string processorName; // Фиксированный буфер на 256 символов

		public int memorySizeMB;
	}

	// Пример 6: Управление неуправляемой памятью
	public static class MemoryExample
	{
		[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern int memcpy(
			IntPtr dest,            // Целевой указатель
			IntPtr src,             // Источник
			int count               // Количество байт
		);

		public static void CopyMemory(byte[] source, byte[] destination)
		{
			if (source.Length != destination.Length)
				throw new ArgumentException("Размеры массивов должны совпадать");

			IntPtr srcPtr = Marshal.AllocHGlobal(source.Length);    // Выделение неуправляемой памяти
			IntPtr dstPtr = Marshal.AllocHGlobal(destination.Length);

			try
			{
				// Копирование из управляемого массива в неуправляемую память
				Marshal.Copy(source, 0, srcPtr, source.Length);

				// Вызов нативной функции копирования памяти
				memcpy(dstPtr, srcPtr, source.Length);

				// Копирование результата обратно в управляемый массив
				Marshal.Copy(dstPtr, destination, 0, destination.Length);
			}
			finally
			{
				// Обязательное освобождение памяти даже при исключениях
				Marshal.FreeHGlobal(srcPtr);
				Marshal.FreeHGlobal(dstPtr);
			}
		}
	}

	// Пример 7: Работа с делегатами и обратными вызовами
	public static class CallbackExample
	{
		// Объявление делегата, совместимого с нативной callback-функцией
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate void LogCallback(
			[MarshalAs(UnmanagedType.LPStr)] string message,    // Строка как char*
			int severity                                        // Уровень серьезности
		);

		[DllImport("logging.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern void SetLogger(LogCallback callback);
	}

	// Пример 8: COM-взаимодействие (сокращенный пример)
	[ComImport]
	[Guid("00000000-0000-0000-C000-000000000046")] // IUnknown
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IUnknown
	{
		int QueryInterface(ref Guid riid, out IntPtr ppvObject);
		int AddRef();
		int Release();
	}
}

// Основная программа для демонстрации всех примеров
class Program
{
	static void Main(string[] args)
	{
		Console.WriteLine("=== Демонстрация System.Runtime.InteropServices ===\n");

		try
		{
			Console.WriteLine("1. GetTickCount (время работы системы):");
			uint ticks = InteropSamples.NativeMethods.GetTickCount();
			Console.WriteLine($"   Система работает: {ticks / 1000} секунд\n");

			Console.WriteLine("2. Структура Point:");
			var point = new InteropSamples.Point { X = 10, Y = 20 };
			Console.WriteLine($"   Point size: {Marshal.SizeOf(point)} байт");
			Console.WriteLine($"   Значения: X={point.X}, Y={point.Y}\n");

			Console.WriteLine("3. Управление памятью через Marshal:");
			byte[] source = { 1, 2, 3, 4, 5 };
			byte[] dest = new byte[5];
			InteropSamples.MemoryExample.CopyMemory(source, dest);
			Console.WriteLine($"   Копирование массива: {string.Join(",", dest)}\n");

			Console.WriteLine("4. Демонстрация кодировок:");
			Console.WriteLine($"   Размер Unicode char: {Marshal.SystemDefaultCharSize} байт");
			Console.WriteLine($"   Size of int: {Marshal.SizeOf(typeof(int))} байт\n");

			Console.WriteLine("=== СОВМЕСТИМОСТЬ ===");
			Console.WriteLine($"   Platform: {(Environment.Is64BitProcess ? "x64" : "x86")}");
			Console.WriteLine($"   OS: {Environment.OSVersion}");

			// Демонстрация обработки ошибок
			Console.WriteLine("\n6. Обработка ошибок (демо с несуществующим дескриптором):");
			try
			{
				InteropSamples.ErrorExample.Close(IntPtr.Zero);
			}
			catch (InvalidOperationException ex)
			{
				Console.WriteLine($"   Перехвачено: {ex.Message}");
			}
		}
		catch (DllNotFoundException ex)
		{
			Console.WriteLine($"Ошибка: {ex.Message}");
			Console.WriteLine("Это демонстрация - некоторые библиотеки могут отсутствовать.");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Неожиданная ошибка: {ex}");
		}
	}
}