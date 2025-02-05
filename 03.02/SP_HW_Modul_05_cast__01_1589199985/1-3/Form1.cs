using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace _1_3
{
	public partial class Form1 : Form
	{
		const string DEFAULT_TEXT = "На краю дороги стоял дуб. Вероятно, в десять раз старше берез, составлявших лес, он был в десять раз толще, и в два раза выше каждой березы. Это был огромный, в два обхвата дуб, с обломанными, давно, видно, суками и с обломанной корой, заросшей старыми болячками. С огромными своими неуклюже, несимметрично растопыренными корявыми руками и пальцами, он старым, сердитым и презрительным уродом стоял между улыбающимися березами. Только он один не хотел подчиняться обаянию весны и не хотел видеть ни весны, ни солнца.";
		Action countWords;
		
		public Form1()
		{
			InitializeComponent();
			richTextBox1.Text = DEFAULT_TEXT;
			countWords = new Action(delegate { ChangeLabelSentences(""); });
		}
		void ChangeLabelSentences(string text)
		{
			labelSentences.Text = text;
		}
		Task countWordsTask = new Task(delegate
		{
			string temp = "";
			temp = DEFAULT_TEXT.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length.ToString();
			labelWords.Text = "1";
			labelWords.Invoke(delegate { labelWords.Text = "Количество слов: " + temp; });
		});

		Task countSentencesTask = new Task(delegate
		{
			string temp = "";
			temp = DEFAULT_TEXT.Split(['.', '!', '?'], StringSplitOptions.RemoveEmptyEntries).Length.ToString();
			ChangeLabelSentences("Количество предложений: " + temp);
			//labelSentences.Invoke(delegate { labelSentences.Text = "Количество предложений: " + temp; });
		});

		// Не работает 
		Task countSymbols = Task.Run(delegate
		{
			string symbols = "";
			//char[] symbols
			for (int i = 0; i < text.Length; i++)
			{
				if (!(text[i] > 0x2F && text[i] < 0x3A) || !(text[i] > 0x40 && text[i] < 0x5B) || !(text[i] > 0x60 && text[i] < 0x7B))
					symbols += text[i];
			}
			labelSymbols.Invoke(delegate { labelSymbols.Text = symbols; });
			//temp = text.Split(['.', '!', '?'], StringSplitOptions.RemoveEmptyEntries).Length.ToString();
			//labelSentences.Invoke(delegate { labelSentences.Text = "Количество предложений: " + temp; });
		});

		Task countInterrogativeSentences = Task.Run(delegate
		{
			int count = 0;
			for (int i = 0; i < text.Length; i++) if (text[i] == '?') count++;
			labelInterrogative.Invoke(delegate { labelInterrogative.Text = "Количество вопросительных предложений: " + count; });
		});

		Task countExclamatorySentences = Task.Run(delegate
		{
			int count = 0;
			for (int i = 0; i < text.Length; i++) if (text[i] == '?') count++;
			labelExclamatory.Invoke(delegate { labelExclamatory.Text = "Количество восклицательных предложений: " + count; });
		});

		//Task.WhenAll()
		Task saveToFileTask = Task.Run(delegate
		{
			while (checkBox1.Checked)
			{
				if (countWordsTask.IsCompleted && countSentencesTask.IsCompleted && countInterrogativeSentences.IsCompleted && countExclamatorySentences.IsCompleted)
				{
					File.Delete("log.txt");
					StreamWriter sw = new StreamWriter("log.txt", false);
					sw.WriteLine(labelSentences.Text);
					sw.WriteLine(labelWords.Text);
					sw.WriteLine(labelSymbols.Text);
					sw.WriteLine(labelInterrogative.Text);
					sw.WriteLine(labelExclamatory.Text);
					sw.Close();
					break;
				}
				else Thread.Sleep(1000);
			}
		});
		

		private void buttonAnalyze_Click(object sender, EventArgs e)
		{
			string text = richTextBox1.Text;
			buttonStop.Enabled = true;

			
			buttonStop.Enabled = false;
		}

		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			//SaveToFile = checkBox1.Checked;
		}
	}
}
