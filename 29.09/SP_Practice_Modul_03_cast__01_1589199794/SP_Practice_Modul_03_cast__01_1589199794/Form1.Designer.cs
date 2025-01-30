namespace SP_Practice_Modul_03_cast__01_1589199794
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
			richTextBox1 = new RichTextBox();
			buttonStart = new Button();
			numericUpDownStartValue = new NumericUpDown();
			numericUpDownEndValue = new NumericUpDown();
			numericUpDownThreadsNumber = new NumericUpDown();
			label1 = new Label();
			label2 = new Label();
			label3 = new Label();
			((System.ComponentModel.ISupportInitialize)numericUpDownStartValue).BeginInit();
			((System.ComponentModel.ISupportInitialize)numericUpDownEndValue).BeginInit();
			((System.ComponentModel.ISupportInitialize)numericUpDownThreadsNumber).BeginInit();
			SuspendLayout();
			// 
			// richTextBox1
			// 
			richTextBox1.Location = new Point(12, 79);
			richTextBox1.Name = "richTextBox1";
			richTextBox1.Size = new Size(776, 222);
			richTextBox1.TabIndex = 1;
			richTextBox1.Text = "";
			// 
			// buttonStart
			// 
			buttonStart.Location = new Point(318, 37);
			buttonStart.Name = "buttonStart";
			buttonStart.Size = new Size(75, 23);
			buttonStart.TabIndex = 2;
			buttonStart.Text = "Start";
			buttonStart.UseVisualStyleBackColor = true;
			buttonStart.Click += buttonStart_Click;
			// 
			// numericUpDownStartValue
			// 
			numericUpDownStartValue.Location = new Point(234, 37);
			numericUpDownStartValue.Name = "numericUpDownStartValue";
			numericUpDownStartValue.Size = new Size(50, 23);
			numericUpDownStartValue.TabIndex = 3;
			// 
			// numericUpDownEndValue
			// 
			numericUpDownEndValue.Location = new Point(423, 37);
			numericUpDownEndValue.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			numericUpDownEndValue.Name = "numericUpDownEndValue";
			numericUpDownEndValue.Size = new Size(49, 23);
			numericUpDownEndValue.TabIndex = 4;
			numericUpDownEndValue.Value = new decimal(new int[] { 1, 0, 0, 0 });
			// 
			// numericUpDownThreadsNumber
			// 
			numericUpDownThreadsNumber.Location = new Point(12, 37);
			numericUpDownThreadsNumber.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			numericUpDownThreadsNumber.Name = "numericUpDownThreadsNumber";
			numericUpDownThreadsNumber.Size = new Size(120, 23);
			numericUpDownThreadsNumber.TabIndex = 5;
			numericUpDownThreadsNumber.Value = new decimal(new int[] { 1, 0, 0, 0 });
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(196, 9);
			label1.Name = "label1";
			label1.Size = new Size(125, 12);
			label1.TabIndex = 6;
			label1.Text = "Стартовое значение";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new Point(385, 9);
			label2.Name = "label2";
			label2.Size = new Size(117, 12);
			label2.TabIndex = 7;
			label2.Text = "Конечное значение";
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Location = new Point(8, 9);
			label3.Name = "label3";
			label3.Size = new Size(124, 12);
			label3.TabIndex = 8;
			label3.Text = "Количество потоков";
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(6F, 12F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 329);
			Controls.Add(label3);
			Controls.Add(label2);
			Controls.Add(label1);
			Controls.Add(numericUpDownThreadsNumber);
			Controls.Add(numericUpDownEndValue);
			Controls.Add(numericUpDownStartValue);
			Controls.Add(buttonStart);
			Controls.Add(richTextBox1);
			Name = "Form1";
			Text = "Form1";
			Load += Form1_Load;
			((System.ComponentModel.ISupportInitialize)numericUpDownStartValue).EndInit();
			((System.ComponentModel.ISupportInitialize)numericUpDownEndValue).EndInit();
			((System.ComponentModel.ISupportInitialize)numericUpDownThreadsNumber).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private RichTextBox richTextBox1;
		private Button buttonStart;
		private NumericUpDown numericUpDownStartValue;
		private NumericUpDown numericUpDownEndValue;
		private NumericUpDown numericUpDownThreadsNumber;
		private Label label1;
		private Label label2;
		private Label label3;
	}
}
