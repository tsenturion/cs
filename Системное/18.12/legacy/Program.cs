using System;
using System.Runtime.InteropServices;

namespace DeviceInterop
{
	public static class DeviceApi
	{
		// Инициализация устройства
		[DllImport("device_driver.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern int InitializeDevice();

		// Чтение данных с устройства
		[DllImport("device_driver.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern int ReadData(out int value);

		// Запись данных на устройство
		[DllImport("device_driver.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern int WriteData(int value);

		// Получение статуса устройства
		[DllImport("device_driver.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern int GetDeviceStatus();

		// Закрытие соединения с устройством
		[DllImport("device_driver.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern int CloseDevice();
	}
}

namespace LegacyWrapper
{
	// Адаптационный слой для изоляции legacy-кода
	public class DeviceService
	{
		private bool _isInitialized = false;

		public bool Start()
		{
			// Вызов неуправляемого кода для инициализации устройства
			int result = DeviceInterop.DeviceApi.InitializeDevice();
			_isInitialized = (result == 0);
			return _isInitialized;
		}

		public int GetValue()
		{
			if (!_isInitialized)
			{
				// Предотвращение работы с неинициализированным устройством
				throw new InvalidOperationException("Устройство не инициализировано. Вызовите Start() перед GetValue()");
			}

			int value;
			// Вызов неуправляемого кода для чтения данных
			int result = DeviceInterop.DeviceApi.ReadData(out value);

			if (result != 0)
			{
				// Преобразование кода ошибки в управляемое исключение
				throw new InvalidOperationException("Ошибка чтения данных устройства. Код ошибки: " + result);
			}

			return value;
		}

		public void WriteValue(int value)
		{
			if (!_isInitialized)
			{
				throw new InvalidOperationException("Устройство не инициализировано. Вызовите Start() перед WriteValue()");
			}

			int result = DeviceInterop.DeviceApi.WriteData(value);

			if (result != 0)
			{
				throw new InvalidOperationException("Ошибка записи данных на устройство. Код ошибки: " + result);
			}
		}

		public int GetStatus()
		{
			if (!_isInitialized)
			{
				throw new InvalidOperationException("Устройство не инициализировано. Вызовите Start() перед GetStatus()");
			}

			return DeviceInterop.DeviceApi.GetDeviceStatus();
		}

		public void Stop()
		{
			if (_isInitialized)
			{
				DeviceInterop.DeviceApi.CloseDevice();
				_isInitialized = false;
			}
		}

		// Деструктор для гарантированного освобождения ресурсов
		~DeviceService()
		{
			Stop();
		}
	}
}

// Главный класс программы для демонстрации работы адаптационного слоя
class Program
{
	static void Main(string[] args)
	{
		Console.WriteLine("=== Демонстрация адаптационного слоя для legacy-кода ===\n");

		// Создаем экземпляр обертки вместо прямого использования DeviceApi
		var deviceService = new LegacyWrapper.DeviceService();

		try
		{
			Console.WriteLine("1. Инициализация устройства через адаптер...");
			bool started = deviceService.Start();
			Console.WriteLine($"   Результат: {(started ? "УСПЕХ" : "ОШИБКА")}");

			if (started)
			{
				Console.WriteLine("\n2. Проверка статуса устройства...");
				int status = deviceService.GetStatus();
				Console.WriteLine($"   Статус устройства: {status}");

				Console.WriteLine("\n3. Чтение данных с устройства...");
				try
				{
					int value = deviceService.GetValue();
					Console.WriteLine($"   Полученное значение: {value}");

					Console.WriteLine("\n4. Запись данных на устройство...");
					deviceService.WriteValue(value + 100);
					Console.WriteLine($"   Запись успешно выполнена");
				}
				catch (InvalidOperationException ex)
				{
					Console.WriteLine($"   ИСКЛЮЧЕНИЕ: {ex.Message}");
					Console.WriteLine("   Продолжаем работу для демонстрации...");
				}
			}
			else
			{
				Console.WriteLine("\nПропускаем дальнейшие операции т.к. инициализация не удалась.");
			}

			Console.WriteLine("\n5. Повторная попытка чтения без инициализации...");
			try
			{
				// Создаем новый экземпляр без вызова Start()
				var badService = new LegacyWrapper.DeviceService();
				int value = badService.GetValue();
			}
			catch (InvalidOperationException ex)
			{
				Console.WriteLine($"   ОЖИДАЕМОЕ ИСКЛЮЧЕНИЕ: {ex.Message}");
			}
		}
		catch (DllNotFoundException)
		{
			Console.WriteLine("\nКРИТИЧЕСКАЯ ОШИБКА: Не найдена библиотека device_driver.dll");
			Console.WriteLine("Это демонстрирует, что адаптер не скрывает фундаментальные проблемы,");
			Console.WriteLine("а лишь преобразует стиль взаимодействия с legacy-кодом.");
		}
		finally
		{
			Console.WriteLine("\n6. Завершение работы...");
			deviceService.Stop();
		}

		Console.WriteLine("\n=== Демонстрация завершена ===");
	}
}