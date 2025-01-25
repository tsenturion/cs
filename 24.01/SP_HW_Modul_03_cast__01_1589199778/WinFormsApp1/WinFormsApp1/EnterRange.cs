using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp1
{
	public partial class EnterRange : Form
	{
		char[] nums = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9']; 
		public EnterRange()
		{
			InitializeComponent();
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{
			if (textBoxStartValue.TextLength > 0 && Array.IndexOf(nums, textBoxStartValue.Text.ToArray().Last()) == -1)
			{
				textBoxStartValue.Text = textBoxStartValue.Text.Remove(textBoxStartValue.TextLength - 1);
			}
		}
		public int GetStartValue()
		{
			return Convert.ToInt32(textBoxStartValue.Text);
		}
		public int GetEndValue()
		{
			return Convert.ToInt32(textBoxEndValue.Text);
		}
	}
}
