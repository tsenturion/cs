using Microsoft.Win32;

namespace Program
{
	class Program
	{
		static void Main()
		{
			RegistryKey regSoftware = Registry.CurrentUser.CreateSubKey("Software");
			RegistryKey MyRegTest = regSoftware.CreateSubKey("MyRegistryTest");
			MyRegTest.SetValue("", "Это тестовый ключ");
			MyRegTest.SetValue("TestString", "Hell o, world!", RegistryValueKind.String);
			MyRegTest.SetValue("TestInt", 42, RegistryValueKind.DWord);

			Console.WriteLine(MyRegTest.GetValue(""));
			Console.WriteLine(MyRegTest.GetValue("TestString"));
			Console.WriteLine(MyRegTest.GetValue("TestInt"));
			if(MyRegTest.GetValue("1") == null) Console.WriteLine("Значение отсутствует!!!!!!!!!!11");

			MyRegTest.SetValue("TestString", "Изменённое значение");
			MyRegTest.SetValue("TestInt", 100);

			regSoftware.DeleteSubKey("MyRegistryTest");
		}
	}
}