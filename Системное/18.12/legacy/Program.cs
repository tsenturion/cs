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

	// Пример использования в C#
	public class DeviceManager
	{
		public bool Initialize()
		{
			int result = DeviceApi.InitializeDevice();
			if (result == 0)
			{
				Console.WriteLine("Device initialized successfully");
				return true;
			}
			else
			{
				Console.WriteLine($"Failed to initialize device. Error code: {result}");
				return false;
			}
		}

		public bool ReadSensorData(out int sensorValue)
		{
			sensorValue = 0;
			int result = DeviceApi.ReadData(out sensorValue);

			if (result == 0)
			{
				Console.WriteLine($"Sensor value: {sensorValue}");
				return true;
			}
			else
			{
				Console.WriteLine($"Failed to read data. Error code: {result}");
				return false;
			}
		}

		public void Dispose()
		{
			DeviceApi.CloseDevice();
			Console.WriteLine("Device connection closed");
		}
	}

	// Главный класс программы для демонстрации работы с устройством
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("=== Тестирование взаимодействия с устройством ===\n");

			// Создаем менеджер устройства
			DeviceManager deviceManager = new DeviceManager();

			try
			{
				// Пытаемся инициализировать устройство
				bool initialized = deviceManager.Initialize();

				if (initialized)
				{
					Console.WriteLine("\nПроверка статуса устройства:");
					int status = DeviceApi.GetDeviceStatus();
					Console.WriteLine($"Статус устройства: {status}");

					Console.WriteLine("\nПопытка чтения данных с датчика:");
					int sensorValue;
					bool readSuccess = deviceManager.ReadSensorData(out sensorValue);

					if (readSuccess)
					{
						Console.WriteLine("\nПопытка записи данных на устройство:");
						int writeResult = DeviceApi.WriteData(sensorValue + 10);
						Console.WriteLine($"Результат записи: {writeResult} (0 = успех)");
					}

					Console.WriteLine("\nПроверка статуса после операций:");
					status = DeviceApi.GetDeviceStatus();
					Console.WriteLine($"Статус устройства: {status}");
				}
				else
				{
					Console.WriteLine("Устройство не было инициализировано, пропускаем дальнейшие операции.");
				}
			}
			catch (DllNotFoundException)
			{
				Console.WriteLine("\nОШИБКА: Не найдена библиотека device_driver.dll");
				Console.WriteLine("Создайте тестовую библиотеку или подключите реальный драйвер устройства.");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"\nОШИБКА: {ex.GetType().Name}: {ex.Message}");
			}
			finally
			{
				// Всегда освобождаем ресурсы устройства
				Console.WriteLine("\nЗавершение работы с устройством...");
				deviceManager.Dispose();
			}

			Console.WriteLine("\n=== Тестирование завершено ===");
		}
	}
}