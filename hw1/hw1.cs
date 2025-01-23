using System;
using System.Windows.Forms;

namespace MessageBoxApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            button3.Text = "Показать сообщение";

            button3.Click += new EventHandler(button3_Click);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string appName = "Привет, как дела?";
            string appDescription = "Съешь этих французский булочек, да выпей чаю";

            MessageBox.Show(appName, "Название приложения", MessageBoxButtons.OK, MessageBoxIcon.Information);
            MessageBox.Show(appDescription, "Описание приложения", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }
    }
}