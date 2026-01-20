using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NetworkTypesAndLayersDemo
{
	class Program
	{
		static async Task Main()
		{
			Console.WriteLine("=== ТИПЫ СЕТЕЙ И УРОВНИ ОРГАНИЗАЦИИ ===\n");

			// Часть 1: Типы сетей по масштабу
			Console.WriteLine("1. ТИПЫ СЕТЕЙ ПО МАСШТАБУ:");
			DemonstrateNetworkTypesByScale();

			// Часть 2: Типы сетей по назначению
			Console.WriteLine("\n2. ТИПЫ СЕТЕЙ ПО НАЗНАЧЕНИЮ:");
			DemonstrateNetworkTypesByPurpose();

			// Часть 3: Уровневая организация сетей
			Console.WriteLine("\n3. УРОВНЕВАЯ ОРГАНИЗАЦИЯ СЕТЕЙ:");
			await DemonstrateNetworkLayersAsync();

			// Часть 4: Практическое применение уровневого подхода
			Console.WriteLine("\n4. УРОВНЕВОЙ ПОДХОД В РАЗРАБОТКЕ:");
			DemonstrateLayeredDevelopment();

			// Часть 5: Масштабирование и совместимость
			Console.WriteLine("\n5. МАСШТАБИРОВАНИЕ И СОВМЕСТИМОСТЬ:");
			await DemonstrateScalingAndCompatibilityAsync();
		}

		static void DemonstrateNetworkTypesByScale()
		{
			Console.WriteLine($"  СЕТИ РАЗНОГО МАСШТАБА:");

			// Локальная сеть (LAN)
			Console.WriteLine($"\n  1. ЛОКАЛЬНАЯ СЕТЬ (LAN - Local Area Network):");
			var lanNetwork = new NetworkSimulator("Офисная сеть", NetworkScale.LAN);
			lanNetwork.SimulateOperation("Передача файла между компьютерами");
			Console.WriteLine($"    Характеристики:");
			Console.WriteLine($"      - Радиус: Одно здание/кампус");
			Console.WriteLine($"      - Задержка: 1-10 мс");
			Console.WriteLine($"      - Пропускная способность: Высокая");
			Console.WriteLine($"      - Типичное использование: Общие ресурсы, принтеры");

			// Городская сеть (MAN)
			Console.WriteLine($"\n  2. ГОРОДСКАЯ СЕТЬ (MAN - Metropolitan Area Network):");
			var manNetwork = new NetworkSimulator("Городская сеть", NetworkScale.MAN);
			manNetwork.SimulateOperation("Связь между офисами в разных районах");
			Console.WriteLine($"    Характеристики:");
			Console.WriteLine($"      - Радиус: Город");
			Console.WriteLine($"      - Задержка: 10-50 мс");
			Console.WriteLine($"      - Пропускная способность: Средняя");
			Console.WriteLine($"      - Типичное использование: Связь филиалов");

			// Глобальная сеть (WAN)
			Console.WriteLine($"\n  3. ГЛОБАЛЬНАЯ СЕТЬ (WAN - Wide Area Network):");
			var wanNetwork = new NetworkSimulator("Корпоративная WAN", NetworkScale.WAN);
			wanNetwork.SimulateOperation("Синхронизация данных между странами");
			Console.WriteLine($"    Характеристики:");
			Console.WriteLine($"      - Радиус: Страна/континент/мир");
			Console.WriteLine($"      - Задержка: 50-500+ мс");
			Console.WriteLine($"      - Пропускная способность: Ограниченная");
			Console.WriteLine($"      - Типичное использование: Распределённые системы");

			// Сравнение характеристик
			Console.WriteLine($"\n  СРАВНЕНИЕ ХАРАКТЕРИСТИК:");
			var networks = new[] { lanNetwork, manNetwork, wanNetwork };

			Console.WriteLine($"    {"Сеть",-20} {"Задержка",-15} {"Стабильность",-15} {"Стоимость"}");
			Console.WriteLine($"    {"".PadRight(60, '-')}");

			foreach (var net in networks)
			{
				Console.WriteLine($"    {net.Name,-20} {net.GetLatency(),-15} {net.GetStability(),-15} {net.GetCost()}");
			}

			// Демонстрация влияния масштаба на приложение
			Console.WriteLine($"\n  ВЛИЯНИЕ МАСШТАБА НА ПРИЛОЖЕНИЕ:");

			var testApp = new TestApplication();

			foreach (var net in networks)
			{
				Console.WriteLine($"\n    Тест в сети: {net.Name}");
				testApp.TestNetworkOperation(net);
			}
		}

		static void DemonstrateNetworkTypesByPurpose()
		{
			Console.WriteLine($"  СЕТИ РАЗНОГО НАЗНАЧЕНИЯ:");

			// Внутренняя сеть
			Console.WriteLine($"\n  1. ВНУТРЕННЯЯ СЕТЬ (Intranet):");
			var internalNet = new PurposeNetwork("Корпоративная сеть", NetworkPurpose.Internal);
			internalNet.SimulateUsage();
			Console.WriteLine($"    Требования:");
			Console.WriteLine($"      - Безопасность: Высокая (ограниченный доступ)");
			Console.WriteLine($"      - Доступность: Высокая (работа критична)");
			Console.WriteLine($"      - Мониторинг: Полный контроль");

			// Публичная сеть
			Console.WriteLine($"\n  2. ПУБЛИЧНАЯ СЕТЬ (Internet):");
			var publicNet = new PurposeNetwork("Общедоступная сеть", NetworkPurpose.Public);
			publicNet.SimulateUsage();
			Console.WriteLine($"    Требования:");
			Console.WriteLine($"      - Безопасность: Ограниченная (много угроз)");
			Console.WriteLine($"      - Доступность: Средняя (допустимы сбои)");
			Console.WriteLine($"      - Мониторинг: Частичный контроль");

			// Производственная сеть
			Console.WriteLine($"\n  3. ПРОИЗВОДСТВЕННАЯ СЕТЬ (Industrial):");
			var industrialNet = new PurposeNetwork("Заводская сеть", NetworkPurpose.Industrial);
			industrialNet.SimulateUsage();
			Console.WriteLine($"    Требования:");
			Console.WriteLine($"      - Безопасность: Критическая (безопасность людей)");
			Console.WriteLine($"      - Доступность: Критическая (простой стоит дорого)");
			Console.WriteLine($"      - Мониторинг: Реальное время");

			// Демонстрация требований к приложениям
			Console.WriteLine($"\n  ТРЕБОВАНИЯ К ПРИЛОЖЕНИЯМ:");

			var appDesigner = new AppDesigner();

			Console.WriteLine($"\n    Дизайн для внутренней сети:");
			appDesigner.DesignForNetwork(internalNet);

			Console.WriteLine($"\n    Дизайн для публичной сети:");
			appDesigner.DesignForNetwork(publicNet);

			Console.WriteLine($"\n    Дизайн для производственной сети:");
			appDesigner.DesignForNetwork(industrialNet);
		}

		static async Task DemonstrateNetworkLayersAsync()
		{
			Console.WriteLine($"  УРОВНЕВАЯ ОРГАНИЗАЦИЯ:");

			// Создаём многоуровневую систему
			Console.WriteLine($"\n  МОДЕЛЬ УРОВНЕВОЙ СИСТЕМЫ:");

			var layeredSystem = new LayeredNetworkSystem();

			// Демонстрация передачи данных через уровни
			Console.WriteLine($"\n  1. ПЕРЕДАЧА ДАННЫХ ЧЕРЕЗ УРОВНИ:");
			string originalMessage = "Важные данные для передачи";
			Console.WriteLine($"    Исходное сообщение: '{originalMessage}'");

			var transmissionResult = await layeredSystem.TransmitDataAsync(originalMessage);
			Console.WriteLine($"    Результат передачи: {transmissionResult}");

			// Работа на отдельном уровне
			Console.WriteLine($"\n  2. РАБОТА НА КОНКРЕТНОМ УРОВНЕ:");

			var appLayer = layeredSystem.GetLayer(NetworkLayer.Application);
			string appResult = await appLayer.ProcessAsync("Данные от пользователя");
			Console.WriteLine($"    Результат прикладного уровня: {appResult}");

			var transportLayer = layeredSystem.GetLayer(NetworkLayer.Transport);
			string transportResult = await transportLayer.ProcessAsync("Сегменты данных");
			Console.WriteLine($"    Результат транспортного уровня: {transportResult}");

			// Изменение реализации уровня
			Console.WriteLine($"\n  3. ИЗМЕНЕНИЕ РЕАЛИЗАЦИИ УРОВНЯ:");
			Console.WriteLine($"    Меняем реализацию транспортного уровня...");

			layeredSystem.ReplaceLayer(NetworkLayer.Transport, new AdvancedTransportLayer());
			var newTransmissionResult = await layeredSystem.TransmitDataAsync("Тест после замены");
			Console.WriteLine($"    Результат с новой реализацией: {newTransmissionResult}");

			// Демонстрация независимости уровней
			Console.WriteLine($"\n  4. НЕЗАВИСИМОСТЬ УРОВНЕЙ:");

			Console.WriteLine($"    Тестируем каждый уровень отдельно:");
			foreach (var layer in layeredSystem.GetAllLayers())
			{
				Console.WriteLine($"\n      Уровень: {layer.LayerName}");
				Console.WriteLine($"        Ответственность: {layer.GetResponsibility()}");

				// Тестируем уровень
				string testInput = $"Тест для {layer.LayerName}";
				string testOutput = await layer.ProcessAsync(testInput);
				Console.WriteLine($"        Вход: '{testInput}' -> Выход: '{testOutput}'");

				Console.WriteLine($"        Может работать автономно: {layer.CanWorkIndependently()}");
			}
		}

		static void DemonstrateLayeredDevelopment()
		{
			Console.WriteLine($"  УРОВНЕВОЙ ПОДХОД В РАЗРАБОТКЕ:");

			// Пример многоуровневого приложения
			Console.WriteLine($"\n  ПРИМЕР: СИСТЕМА УПРАВЛЕНИЯ СКЛАДОМ");

			var warehouseSystem = new LayeredWarehouseSystem();

			// Демонстрация обработки заказа через уровни
			Console.WriteLine($"\n  1. ОБРАБОТКА ЗАКАЗА:");
			var order = new WarehouseOrder
			{
				OrderId = "ORD-001",
				Customer = "Клиент А",
				Items = new List<string> { "Товар1", "Товар2", "Товар3" },
				Priority = OrderPriority.High
			};

			Console.WriteLine($"    Новый заказ: {order.OrderId} для {order.Customer}");

			string processingResult = warehouseSystem.ProcessOrder(order);
			Console.WriteLine($"    Результат обработки: {processingResult}");

			// Изолированная работа на уровне
			Console.WriteLine($"\n  2. ИЗОЛИРОВАННАЯ РАБОТА УРОВНЕЙ:");

			Console.WriteLine($"    Тестируем уровень бизнес-логики:");
			var businessLayer = warehouseSystem.GetBusinessLayer();
			string businessResult = businessLayer.ValidateOrder(order);
			Console.WriteLine($"      Валидация заказа: {businessResult}");

			Console.WriteLine($"\n    Тестируем уровень данных:");
			var dataLayer = warehouseSystem.GetDataLayer();
			string dataResult = dataLayer.SaveOrder(order);
			Console.WriteLine($"      Сохранение заказа: {dataResult}");

			// Демонстрация замены реализации
			Console.WriteLine($"\n  3. ЗАМЕНА РЕАЛИЗАЦИИ УРОВНЯ:");

			Console.WriteLine($"    Меняем хранилище данных (старое -> новое)...");
			var newDataLayer = new AdvancedDataLayer();
			warehouseSystem.ReplaceDataLayer(newDataLayer);

			string newOrderResult = warehouseSystem.ProcessOrder(new WarehouseOrder
			{
				OrderId = "ORD-002",
				Customer = "Клиент Б",
				Items = new List<string> { "Товар4" }
			});

			Console.WriteLine($"    Результат с новым хранилищем: {newOrderResult}");

			// Демонстрация отладки по уровням
			Console.WriteLine($"\n  4. ОТЛАДКА ПО УРОВНЯМ:");

			var debugOrder = new WarehouseOrder
			{
				OrderId = "DEBUG-001",
				Customer = "Тестовый клиент",
				Items = new List<string>(),
				Priority = OrderPriority.Normal
			};

			Console.WriteLine($"    Отладка заказа {debugOrder.OrderId}:");
			warehouseSystem.DebugOrderProcessing(debugOrder);
		}

		static async Task DemonstrateScalingAndCompatibilityAsync()
		{
			Console.WriteLine($"  МАСШТАБИРОВАНИЕ И СОВМЕСТИМОСТЬ:");

			// Симуляция масштабирования системы
			Console.WriteLine($"\n  1. МАСШТАБИРОВАНИЕ СИСТЕМЫ:");

			var scalableSystem = new ScalableNetworkSystem();

			Console.WriteLine($"    Начальная конфигурация:");
			scalableSystem.PrintStatus();

			// Добавляем новые компоненты
			Console.WriteLine($"\n    Добавляем региональный центр...");
			scalableSystem.AddComponent("Региональный центр", NetworkScale.MAN);
			scalableSystem.PrintStatus();

			Console.WriteLine($"\n    Добавляем удалённый филиал...");
			scalableSystem.AddComponent("Удалённый филиал", NetworkScale.WAN);
			scalableSystem.PrintStatus();

			// Тестируем совместимость
			Console.WriteLine($"\n  2. ТЕСТИРОВАНИЕ СОВМЕСТИМОСТИ:");

			var compatibilityTester = new CompatibilityTester();

			Console.WriteLine($"    Тест совместимости протоколов:");
			bool protocolCompatible = await compatibilityTester.TestProtocolCompatibilityAsync();
			Console.WriteLine($"      Протоколы совместимы: {protocolCompatible}");

			Console.WriteLine($"\n    Тест совместимости форматов данных:");
			bool dataCompatible = compatibilityTester.TestDataFormatCompatibility();
			Console.WriteLine($"      Форматы данных совместимы: {dataCompatible}");

			// Демонстрация постепенного обновления
			Console.WriteLine($"\n  3. ПОСТЕПЕННОЕ ОБНОВЛЕНИЕ:");

			var upgradeManager = new SystemUpgradeManager();

			Console.WriteLine($"    Начинаем обновление системы...");

			var upgradeSteps = new[]
			{
				"Обновление уровня представления",
				"Обновление бизнес-логики",
				"Миграция данных",
				"Обновление сетевого уровня"
			};

			foreach (var step in upgradeSteps)
			{
				Console.Write($"      {step}... ");
				bool success = await upgradeManager.PerformUpgradeStepAsync(step);
				Console.WriteLine(success ? "✓" : "✗");

				if (!success)
				{
					Console.WriteLine($"      Откат изменений...");
					await upgradeManager.RollbackAsync();
					break;
				}
			}

			// Демонстрация работы с устаревшими компонентами
			Console.WriteLine($"\n  4. РАБОТА С УСТАРЕВШИМИ КОМПОНЕНТАМИ:");

			var legacyAdapter = new LegacySystemAdapter();

			Console.WriteLine($"    Старая система отправляет данные...");
			string legacyData = legacyAdapter.GetLegacyData();
			Console.WriteLine($"      Данные в старом формате: {legacyData}");

			Console.WriteLine($"\n    Адаптер преобразует данные...");
			string modernData = legacyAdapter.AdaptForModernSystem(legacyData);
			Console.WriteLine($"      Данные в новом формате: {modernData}");

			Console.WriteLine($"\n    Совместимость обеспечена: {legacyAdapter.IsCompatible}");
		}
	}

	// Вспомогательные классы для демонстрации

	enum NetworkScale { LAN, MAN, WAN }
	enum NetworkPurpose { Internal, Public, Industrial }
	enum NetworkLayer { Physical, DataLink, Network, Transport, Session, Presentation, Application }
	enum OrderPriority { Low, Normal, High, Critical }

	class NetworkSimulator
	{
		public string Name { get; }
		public NetworkScale Scale { get; }
		private Random _random = new Random();

		public NetworkSimulator(string name, NetworkScale scale)
		{
			Name = name;
			Scale = scale;
		}

		public void SimulateOperation(string operation)
		{
			Console.Write($"    {operation}... ");

			// Имитация задержки в зависимости от масштаба
			int delay = Scale switch
			{
				NetworkScale.LAN => _random.Next(1, 10),
				NetworkScale.MAN => _random.Next(10, 50),
				NetworkScale.WAN => _random.Next(50, 300),
				_ => 100
			};

			Thread.Sleep(delay);

			// Имитация надёжности
			bool success = _random.Next(0, 10) > (int)Scale; // Чем больше сеть, тем менее надёжна

			Console.WriteLine(success ? "✓ Успешно" : "✗ Ошибка");
			if (!success)
				Console.WriteLine($"      Причина: {GetFailureReason()}");
		}

		public string GetLatency()
		{
			return Scale switch
			{
				NetworkScale.LAN => "1-10 мс",
				NetworkScale.MAN => "10-50 мс",
				NetworkScale.WAN => "50-300+ мс",
				_ => "Неизвестно"
			};
		}

		public string GetStability()
		{
			return Scale switch
			{
				NetworkScale.LAN => "Высокая",
				NetworkScale.MAN => "Средняя",
				NetworkScale.WAN => "Низкая",
				_ => "Неизвестно"
			};
		}

		public string GetCost()
		{
			return Scale switch
			{
				NetworkScale.LAN => "Низкая",
				NetworkScale.MAN => "Средняя",
				NetworkScale.WAN => "Высокая",
				_ => "Неизвестно"
			};
		}

		private string GetFailureReason()
		{
			return Scale switch
			{
				NetworkScale.LAN => "Оборудование ЛВС",
				NetworkScale.MAN => "Провайдер",
				NetworkScale.WAN => "Межконтинентальная связь",
				_ => "Неизвестная причина"
			};
		}
	}

	class TestApplication
	{
		public void TestNetworkOperation(NetworkSimulator network)
		{
			// Тестируем разные типы операций
			string[] operations =
			{
				"Маленький запрос (1 КБ)",
				"Средний запрос (100 КБ)",
				"Большой запрос (10 МБ)",
				"Потоковая передача"
			};

			foreach (var op in operations)
			{
				Console.Write($"      {op}: ");
				Stopwatch sw = Stopwatch.StartNew();

				network.SimulateOperation(op);

				sw.Stop();

				if (network.Scale == NetworkScale.WAN && op.Contains("Большой"))
					Console.WriteLine($"        Предупреждение: В WAN большие запросы неэффективны");
				else if (network.Scale == NetworkScale.LAN && op.Contains("Потоковая"))
					Console.WriteLine($"        Отлично: LAN идеален для потоковой передачи");
			}
		}
	}

	class PurposeNetwork
	{
		public string Name { get; }
		public NetworkPurpose Purpose { get; }

		public PurposeNetwork(string name, NetworkPurpose purpose)
		{
			Name = name;
			Purpose = purpose;
		}

		public void SimulateUsage()
		{
			Console.Write($"    Использование: ");

			switch (Purpose)
			{
				case NetworkPurpose.Internal:
					Console.WriteLine("Доступ к корпоративным ресурсам");
					break;
				case NetworkPurpose.Public:
					Console.WriteLine("Обслуживание внешних пользователей");
					break;
				case NetworkPurpose.Industrial:
					Console.WriteLine("Управление производственным оборудованием");
					break;
			}
		}
	}

	class AppDesigner
	{
		public void DesignForNetwork(PurposeNetwork network)
		{
			Console.WriteLine($"      Архитектурные решения:");

			switch (network.Purpose)
			{
				case NetworkPurpose.Internal:
					Console.WriteLine($"        - Верификация пользователей (LDAP/AD)");
					Console.WriteLine($"        - Подробное логирование");
					Console.WriteLine($"        - Высокая доступность (кластеризация)");
					break;

				case NetworkPurpose.Public:
					Console.WriteLine($"        - Защита от DDoS атак");
					Console.WriteLine($"        - Балансировка нагрузки");
					Console.WriteLine($"        - Геораспределение (CDN)");
					break;

				case NetworkPurpose.Industrial:
					Console.WriteLine($"        - Детерминированное время отклика");
					Console.WriteLine($"        - Избыточность систем");
					Console.WriteLine($"        - Протоколы реального времени");
					break;
			}
		}
	}

	interface INetworkLayer
	{
		NetworkLayer LayerType { get; }
		string LayerName { get; }
		string GetResponsibility();
		Task<string> ProcessAsync(string input);
		bool CanWorkIndependently();
	}

	class PhysicalLayer : INetworkLayer
	{
		public NetworkLayer LayerType => NetworkLayer.Physical;
		public string LayerName => "Физический уровень";

		public string GetResponsibility() =>
			"Передача битов по среде (кабель, Wi-Fi, оптика)";

		public async Task<string> ProcessAsync(string input)
		{
			await Task.Delay(10);
			return $"Биты: {ConvertToBits(input)}";
		}

		public bool CanWorkIndependently() => false; // Зависит от среды

		private string ConvertToBits(string data)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(data);
			return string.Join("", bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
		}
	}

	class TransportLayer : INetworkLayer
	{
		public NetworkLayer LayerType => NetworkLayer.Transport;
		public string LayerName => "Транспортный уровень";

		public string GetResponsibility() =>
			"Надёжная доставка данных, управление потоками";

		public async Task<string> ProcessAsync(string input)
		{
			await Task.Delay(15);
			return $"Сегмент[{input.Length}]: {input}";
		}

		public bool CanWorkIndependently() => true;
	}

	class ApplicationLayer : INetworkLayer
	{
		public NetworkLayer LayerType => NetworkLayer.Application;
		public string LayerName => "Прикладной уровень";

		public string GetResponsibility() =>
			"Интерфейс для приложений (HTTP, FTP, SMTP)";

		public async Task<string> ProcessAsync(string input)
		{
			await Task.Delay(5);
			return $"Приложение: {input}";
		}

		public bool CanWorkIndependently() => true;
	}

	class AdvancedTransportLayer : INetworkLayer
	{
		public NetworkLayer LayerType => NetworkLayer.Transport;
		public string LayerName => "Улучшенный транспортный уровень";

		public string GetResponsibility() =>
			"Надёжная доставка с сжатием и шифрованием";

		public async Task<string> ProcessAsync(string input)
		{
			await Task.Delay(20);
			return $"Сжатый_сегмент[{input.Length}->{input.Length / 2}]: {input.Substring(0, Math.Min(input.Length, 10))}...";
		}

		public bool CanWorkIndependently() => true;
	}

	class LayeredNetworkSystem
	{
		private Dictionary<NetworkLayer, INetworkLayer> _layers = new();

		public LayeredNetworkSystem()
		{
			// Инициализируем уровни
			_layers[NetworkLayer.Physical] = new PhysicalLayer();
			_layers[NetworkLayer.Transport] = new TransportLayer();
			_layers[NetworkLayer.Application] = new ApplicationLayer();
		}

		public async Task<string> TransmitDataAsync(string data)
		{
			Console.WriteLine($"      Начинаем передачу данных...");

			string currentData = data;

			// Проход через уровни от прикладного к физическому
			var layersInOrder = new[]
			{
				NetworkLayer.Application,
				NetworkLayer.Transport,
				NetworkLayer.Physical
			};

			foreach (var layerType in layersInOrder)
			{
				var layer = _layers[layerType];
				Console.WriteLine($"        {layer.LayerName}: {currentData}");
				currentData = await layer.ProcessAsync(currentData);
			}

			Console.WriteLine($"      Данные переданы по сети");

			// Обратный проход (получение)
			Console.WriteLine($"      Получаем данные...");

			foreach (var layerType in layersInOrder.Reverse())
			{
				var layer = _layers[layerType];
				currentData = await layer.ProcessAsync(currentData);
				Console.WriteLine($"        {layer.LayerName}: {currentData}");
			}

			return currentData;
		}

		public INetworkLayer GetLayer(NetworkLayer layerType)
		{
			return _layers[layerType];
		}

		public IEnumerable<INetworkLayer> GetAllLayers()
		{
			return _layers.Values;
		}

		public void ReplaceLayer(NetworkLayer layerType, INetworkLayer newLayer)
		{
			_layers[layerType] = newLayer;
			Console.WriteLine($"      Уровень {layerType} заменён");
		}
	}

	class WarehouseOrder
	{
		public string OrderId { get; set; }
		public string Customer { get; set; }
		public List<string> Items { get; set; }
		public OrderPriority Priority { get; set; }

		public override string ToString() =>
			$"{OrderId} ({Customer}): {Items.Count} товаров";
	}

	interface IWarehouseLayer
	{
		string ProcessOrder(WarehouseOrder order);
		string GetLayerInfo();
	}

	class PresentationLayer : IWarehouseLayer
	{
		public string ProcessOrder(WarehouseOrder order)
		{
			return $"Подготовка UI: Заказ {order.OrderId}";
		}

		public string GetLayerInfo() => "UI и взаимодействие с пользователем";
	}

	class BusinessLogicLayer : IWarehouseLayer
	{
		public string ProcessOrder(WarehouseOrder order)
		{
			if (order.Items == null || order.Items.Count == 0)
				return "Ошибка: Нет товаров в заказе";

			if (order.Priority == OrderPriority.Critical)
				return "Срочная обработка заказа";

			return $"Бизнес-логика: Проверка заказа {order.OrderId}";
		}

		public string ValidateOrder(WarehouseOrder order)
		{
			return order.Items.Count > 10 ?
				"Требуется дополнительная проверка" :
				"Заказ валиден";
		}

		public string GetLayerInfo() => "Бизнес-правила и валидация";
	}

	class DataAccessLayer : IWarehouseLayer
	{
		public string ProcessOrder(WarehouseOrder order)
		{
			// Симуляция сохранения в БД
			Thread.Sleep(50);
			return $"Сохранено в БД: {order.OrderId}";
		}

		public string SaveOrder(WarehouseOrder order)
		{
			return $"Заказ сохранён с ID: {order.OrderId}";
		}

		public string GetLayerInfo() => "Работа с базой данных";
	}

	class AdvancedDataLayer : IWarehouseLayer
	{
		public string ProcessOrder(WarehouseOrder order)
		{
			// Новая реализация с кэшированием
			Thread.Sleep(20);
			return $"Сохранено с кэшированием: {order.OrderId}";
		}

		public string GetLayerInfo() => "Продвинутая работа с данными + кэш";
	}

	class LayeredWarehouseSystem
	{
		private IWarehouseLayer _presentationLayer;
		private IWarehouseLayer _businessLayer;
		private IWarehouseLayer _dataLayer;

		public LayeredWarehouseSystem()
		{
			_presentationLayer = new PresentationLayer();
			_businessLayer = new BusinessLogicLayer();
			_dataLayer = new DataAccessLayer();
		}

		public string ProcessOrder(WarehouseOrder order)
		{
			Console.WriteLine($"      Уровни обработки:");

			string result1 = _presentationLayer.ProcessOrder(order);
			Console.WriteLine($"        UI: {result1}");

			string result2 = _businessLayer.ProcessOrder(order);
			Console.WriteLine($"        Бизнес-логика: {result2}");

			string result3 = _dataLayer.ProcessOrder(order);
			Console.WriteLine($"        Данные: {result3}");

			return $"Заказ {order.OrderId} обработан через 3 уровня";
		}

		public BusinessLogicLayer GetBusinessLayer() => (BusinessLogicLayer)_businessLayer;
		public DataAccessLayer GetDataLayer() => (DataAccessLayer)_dataLayer;

		public void ReplaceDataLayer(IWarehouseLayer newLayer)
		{
			_dataLayer = newLayer;
		}

		public void DebugOrderProcessing(WarehouseOrder order)
		{
			// Проверяем каждый уровень отдельно
			try
			{
				string step1 = _presentationLayer.ProcessOrder(order);
				Console.WriteLine($"      UI уровень: {step1}");

				string step2 = _businessLayer.ProcessOrder(order);
				Console.WriteLine($"      Бизнес-уровень: {step2}");

				string step3 = _dataLayer.ProcessOrder(order);
				Console.WriteLine($"      Уровень данных: {step3}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"      Ошибка на уровне: {ex.Message}");
				// Можно точно определить, на каком уровне проблема
			}
		}
	}

	class ScalableNetworkSystem
	{
		private List<NetworkComponent> _components = new();

		public void AddComponent(string name, NetworkScale scale)
		{
			_components.Add(new NetworkComponent(name, scale));
		}

		public void PrintStatus()
		{
			Console.WriteLine($"      Компоненты системы ({_components.Count}):");
			foreach (var comp in _components)
			{
				Console.WriteLine($"        - {comp.Name} ({comp.Scale})");
			}
		}
	}

	class NetworkComponent
	{
		public string Name { get; }
		public NetworkScale Scale { get; }

		public NetworkComponent(string name, NetworkScale scale)
		{
			Name = name;
			Scale = scale;
		}
	}

	class CompatibilityTester
	{
		public async Task<bool> TestProtocolCompatibilityAsync()
		{
			await Task.Delay(100);
			return new Random().Next(0, 10) > 2; // 70% совместимости
		}

		public bool TestDataFormatCompatibility()
		{
			// Тестируем разные форматы данных
			var formats = new[] { "JSON", "XML", "Protobuf", "CSV" };
			return formats.All(f => f != "CSV"); // CSV несовместим
		}
	}

	class SystemUpgradeManager
	{
		public async Task<bool> PerformUpgradeStepAsync(string step)
		{
			await Task.Delay(200);

			// Симуляция возможной ошибки
			bool success = new Random().Next(0, 10) > 1; // 80% успеха

			if (!success)
				Console.WriteLine($"Ошибка на шаге: {step}");

			return success;
		}

		public async Task RollbackAsync()
		{
			Console.WriteLine($"      Восстановление предыдущей версии...");
			await Task.Delay(300);
			Console.WriteLine($"      Откат завершён");
		}
	}

	class LegacySystemAdapter
	{
		public bool IsCompatible => true;

		public string GetLegacyData()
		{
			// Старый формат данных
			return "LEGACY|ORDER|123|ITEM1,ITEM2,ITEM3|2024-01-15";
		}

		public string AdaptForModernSystem(string legacyData)
		{
			// Преобразование в новый формат
			var parts = legacyData.Split('|');

			if (parts.Length < 5)
				return "{}";

			// Исправлено: правильное использование LINQ
			var items = parts[3].Split(',');
			var jsonItems = string.Join(", ", items.Select(item => $"\"{item}\""));

			return $@"
{{
    ""system"": ""{parts[0]}"",
    ""type"": ""{parts[1]}"",
    ""id"": {parts[2]},
    ""items"": [{jsonItems}],
    ""date"": ""{parts[4]}""
}}".Trim();
		}
	}
}