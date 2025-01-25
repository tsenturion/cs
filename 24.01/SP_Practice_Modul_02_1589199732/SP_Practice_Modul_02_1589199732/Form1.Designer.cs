namespace SP_Practice_Modul_02_1589199732
{
	partial class Form1
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			listBox1 = new ListBox();
			numericUpDown1 = new NumericUpDown();
			label1 = new Label();
			timer1 = new System.Windows.Forms.Timer(components);
			((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
			SuspendLayout();
			// 
			// listBox1
			// 
			listBox1.FormattingEnabled = true;
			listBox1.ItemHeight = 12;
			listBox1.Location = new Point(187, 125);
			listBox1.Name = "listBox1";
			listBox1.Size = new Size(425, 196);
			listBox1.TabIndex = 1;
			// 
			// numericUpDown1
			// 
			numericUpDown1.Location = new Point(298, 12);
			numericUpDown1.Name = "numericUpDown1";
			numericUpDown1.Size = new Size(44, 23);
			numericUpDown1.TabIndex = 2;
			numericUpDown1.Value = new decimal(new int[] { 5, 0, 0, 0 });
			numericUpDown1.ValueChanged += numericUpDown1_ValueChanged;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(12, 18);
			label1.Name = "label1";
			label1.Size = new Size(268, 12);
			label1.TabIndex = 3;
			label1.Text = "Количество секунд для обновления списка:";
			// 
			// timer1
			// 
			timer1.Enabled = true;
			timer1.Interval = 5000;
			timer1.Tick += timer1_Tick;
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(6F, 12F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Controls.Add(label1);
			Controls.Add(numericUpDown1);
			Controls.Add(listBox1);
			Name = "Form1";
			Text = "Form1";
			Load += Form1_Load;
			((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private ListBox listBox1;
		private NumericUpDown numericUpDown1;
		private Label label1;
		private System.Windows.Forms.Timer timer1;
	}
}
