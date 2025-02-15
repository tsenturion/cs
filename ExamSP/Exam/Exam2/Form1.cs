namespace Exam2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
		public void SetLabelText(string text)
		{
            label1.Text += text;
		}
	}
}
