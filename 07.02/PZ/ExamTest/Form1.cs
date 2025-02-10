using System.Text;

namespace ExamTest
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
			string forbidden = "привет";
			List<string> files = Directory.EnumerateFiles(@"C:\Users\Nout\source\repos\cs\07.02\PZ\ExamTest").ToList();
			foreach(string filepath in files)
			{
				if(filepath.Split('.').Last() == "txt")
				{
					//FileStream fs = File.OpenReadfilepath);
					byte[] bytes = File.ReadAllBytes(filepath);
					string[] text = Encoding.UTF8.GetString(bytes).Split(' ', ',', '.', '!', '?');
					//fs.Close();
					for(int i=0;i<text.Length;i++)
					{
						//if (text[i] == forbidden)
						//forbidden.GroupBy(w => w).OrderByDescending(g => g.Count())
						//Directory.GetLogicalDrives
						text.Where
					}
					label1.Text += text + "\n"; 
				}
			}
		}
	}
}
