using System;
using System.IO;
using System.Threading;

public class Bank
{
    private int _money;
    private string _name;
    private int _percent;

    private readonly object _lock = new object();

    public int Money
    {
        get => _money;
        set
        {
            _money = value;
            LogPropertyChange("Деньги", value);
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            LogPropertyChange("Имя", value);
        }
    }

    public int Percent
    {
        get => _percent;
        set
        {
            _percent = value;
            LogPropertyChange("Процент", value);
        }
    }

    private void LogPropertyChange(string propertyName, object newValue)
    {
        new Thread(() =>
        {
            lock (_lock) 
            {
                string filePath = "BankLog.txt";
                string logEntry = $"{DateTime.Now}: {propertyName} изменено на {newValue}";

                try
                {
                    File.AppendAllText(filePath, logEntry + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ошибка записи {ex.Message}");
                }
            }
        }).Start();
    }

    public Bank(int money, string name, int percent)
    {
        _money = money;
        _name = name;
        _percent = percent;
    }
}

class Program
{
    static void Main()
    {
        Bank bank = new Bank(1000, "МойБанк", 5);

        bank.Money = 2000;
        bank.Name = "ВашБанк";
        bank.Percent = 10;

        Console.WriteLine("изменения записаны");
    }
}