//using System;
//using System.Diagnostics;
//using System.Threading.Tasks;

//namespace hw2
//{
//    class Program
//    {
//        static async Task Main(string[] args)
//        {
//            string processName = "notepad.exe";

//            try
//            {
//                Process process = new Process();
//                process.StartInfo.FileName = processName;
//                process.StartInfo.UseShellExecute = false;

//                process.Start();

//                Console.WriteLine($"Процесс '{processName}' запущен с идентификатором: {process.Id}");

//                await Task.Run(() => process.WaitForExit());

//                int exitCode = process.ExitCode;

//                Console.WriteLine($"Процесс '{processName}' завершен с кодом: {exitCode}");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Ошибка при управлении процессом: {ex.Message}");
//            }

//            Console.WriteLine("Нажмите любую клавишу для выхода...");
//            Console.ReadKey();
//        }
//    }
//}

//using System;
//using System.Diagnostics;
//using System.Threading.Tasks;

//namespace ProcessManager
//{
//    class Program
//    {
//        static void Main(string[] args)
//        {
//            string processName = "notepad.exe";

//            try
//            {
//                Process process = new Process();
//                process.StartInfo.FileName = processName;
//                process.StartInfo.UseShellExecute = false;

//                process.Start();

//                Console.WriteLine($"Процесс '{processName}' запущен с идентификатором: {process.Id}");

//                Console.WriteLine("Выберите действие:");
//                Console.WriteLine("1. Ожидать завершения процесса");
//                Console.WriteLine("2. Принудительно завершить процесс");
//                Console.Write("Введите 1 или 2: ");

//                string choice = Console.ReadLine();

//                if (choice == "1")
//                {
//                    process.WaitForExit();
//                    int exitCode = process.ExitCode;
//                    Console.WriteLine($"Процесс '{processName}' завершен с кодом: {exitCode}");
//                }
//                else if (choice == "2")
//                {
//                    process.Kill();
//                    Console.WriteLine($"Процесс '{processName}' был принудительно завершен.");
//                }
//                else
//                {
//                    Console.WriteLine("Некорректный выбор. Процесс будет завершен принудительно.");
//                    process.Kill();
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Ошибка при управлении процессом: {ex.Message}");
//            }

//            Console.WriteLine("Нажмите любую клавишу для выхода...");
//            Console.ReadKey();
//        }
//    }
//}

//using System;
//using System.Threading.Tasks;

//namespace SingleProjectProcessManager
//{
//    class Program
//    {
//        static void Main(string[] args)
//        {
//            Console.WriteLine("Введите первое число");
//            string number1 = Console.ReadLine();
//            Console.WriteLine("Введите второе число");
//            string number2 = Console.ReadLine();
//            Console.WriteLine("Введите операцию с числами");
//            string operation = Console.ReadLine();

//            Task childTask = Task.Run(() => ChildProcess(number1, number2, operation));

//            if (childTask.IsCompletedSuccessfully)
//            {
//                Console.WriteLine("Дочерний процесс завершен успешно.");
//            }
//            else
//            {
//                Console.WriteLine("Дочерний процесс завершен с ошибкой.");
//            }

//            Console.WriteLine("Нажмите любую клавишу для выхода...");
//            Console.ReadKey();
//        }

//        static void ChildProcess(string num1, string num2, string op)
//        {
//            Console.WriteLine("Дочерний процесс запущен.");

//            if (!double.TryParse(num1, out double number1))
//            {
//                Console.WriteLine("Первый аргумент не является числом.");
//                return;
//            }

//            if (!double.TryParse(num2, out double number2))
//            {
//                Console.WriteLine("Второй аргумент не является числом.");
//                return;
//            }

//            string operation = op;

//            double result = 0;
//            bool validOperation = true;

//            switch (operation)
//            {
//                case "+":
//                    result = number1 + number2;
//                    break;
//                case "-":
//                    result = number1 - number2;
//                    break;
//                case "*":
//                    result = number1 * number2;
//                    break;
//                case "/":
//                    if (number2 == 0)
//                    {
//                        Console.WriteLine("Деление на ноль.");
//                        return;
//                    }
//                    result = number1 / number2;
//                    break;
//                default:
//                    validOperation = false;
//                    Console.WriteLine("Неподдерживаемая операция.");
//                    break;
//            }

//            if (validOperation)
//            {
//                Console.WriteLine($"Аргументы: {number1} {operation} {number2}");
//                Console.WriteLine($"Результат: {result}");
//            }

//            Console.WriteLine("Дочерний процесс завершен.");
//        }
//    }
//}

//using System;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;

//namespace SingleProjectFileSearch
//{
//    class Program
//    {
//        static void Main(string[] args)
//        {
//            Console.WriteLine("Родительское приложение запущено.");

//            Console.Write("Введите путь к файлу: ");
//            string filePath = Console.ReadLine();

//            Console.Write("Введите слово для поиска: ");
//            string searchWord = Console.ReadLine();

//            Task childTask = Task.Run(() => ChildProcess(filePath, searchWord));

//            Console.WriteLine("Родительское приложение ожидает завершения дочернего процесса.");
//            childTask.Wait();

//            Console.WriteLine("Нажмите любую клавишу для выхода...");
//            Console.ReadKey();
//        }

//        static void ChildProcess(string filePath, string searchWord)
//        {
//            Console.WriteLine("Дочерний процесс запущен.");

//            if (!File.Exists(filePath))
//            {
//                Console.WriteLine($"Файл по пути '{filePath}' не существует.");
//                return;
//            }

//            try
//            {
//                string content = File.ReadAllText(filePath);

//                int count = content.Split(new string[] { searchWord }, StringSplitOptions.None).Length - 1;

//                Console.WriteLine($"Слово '{searchWord}' встречается {count} раз в файле '{filePath}'.");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Ошибка при чтении файла: {ex.Message}");
//            }

//            Console.WriteLine("Дочерний процесс завершен.");
//        }
//    }
//}