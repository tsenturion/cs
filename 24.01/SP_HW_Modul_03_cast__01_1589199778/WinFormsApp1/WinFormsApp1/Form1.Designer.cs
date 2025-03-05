namespace WinFormsApp1
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
			textBoxSimpleNumbers = new TextBox();
			button2 = new Button();
			label1 = new Label();
			labelSN = new Label();
			checkBoxSN = new CheckBox();
			label2 = new Label();
			textBoxFibNumbers = new TextBox();
			labelFN = new Label();
			checkBoxFibNumbers = new CheckBox();
			SuspendLayout();
			// 
			// textBoxSimpleNumbers
			// 
			textBoxSimpleNumbers.Location = new Point(137, 53);
			textBoxSimpleNumbers.Multiline = true;
			textBoxSimpleNumbers.Name = "textBoxSimpleNumbers";
			textBoxSimpleNumbers.ScrollBars = ScrollBars.Vertical;
			textBoxSimpleNumbers.Size = new Size(642, 96);
			textBoxSimpleNumbers.TabIndex = 5;
			// 
			// button2
			// 
			button2.Location = new Point(265, 309);
			button2.Name = "button2";
			button2.Size = new Size(8, 8);
			button2.TabIndex = 6;
			button2.Text = "button2";
			button2.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Font = new Font("Segoe UI Semilight", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 204);
			label1.Location = new Point(355, 9);
			label1.Name = "label1";
			label1.Size = new Size(162, 30);
			label1.TabIndex = 7;
			label1.Text = "Simple numbers";
			// 
			// labelSN
			// 
			labelSN.AutoSize = true;
			labelSN.Location = new Point(21, 67);
			labelSN.Name = "labelSN";
			labelSN.Size = new Size(98, 12);
			labelSN.TabIndex = 8;
			labelSN.Text = "Start generating";
			// 
			// checkBoxSN
			// 
			checkBoxSN.Appearance = Appearance.Button;
			checkBoxSN.AutoSize = true;
			checkBoxSN.Font = new Font("Britannic Bold", 18F, FontStyle.Regular, GraphicsUnit.Point, 0);
			checkBoxSN.Location = new Point(48, 93);
			checkBoxSN.Name = "checkBoxSN";
			checkBoxSN.Size = new Size(46, 37);
			checkBoxSN.TabIndex = 9;
			checkBoxSN.Text = "►";
			checkBoxSN.UseVisualStyleBackColor = true;
			checkBoxSN.CheckedChanged += checkBox1_CheckedChanged;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Font = new Font("Segoe UI Semilight", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 204);
			label2.Location = new Point(355, 172);
			label2.Name = "label2";
			label2.Size = new Size(187, 30);
			label2.TabIndex = 10;
			label2.Text = "Fibonacci numbers";
			// 
			// textBoxFibNumbers
			// 
			textBoxFibNumbers.Location = new Point(137, 221);
			textBoxFibNumbers.Multiline = true;
			textBoxFibNumbers.Name = "textBoxFibNumbers";
			textBoxFibNumbers.ScrollBars = ScrollBars.Vertical;
			textBoxFibNumbers.Size = new Size(642, 96);
			textBoxFibNumbers.TabIndex = 11;
			// 
			// labelFN
			// 
			labelFN.AutoSize = true;
			labelFN.Location = new Point(21, 240);
			labelFN.Name = "labelFN";
			labelFN.Size = new Size(98, 12);
			labelFN.TabIndex = 12;
			labelFN.Text = "Start generating";
			// 
			// checkBoxFibNumbers
			// 
			checkBoxFibNumbers.Appearance = Appearance.Button;
			checkBoxFibNumbers.AutoSize = true;
			checkBoxFibNumbers.Font = new Font("Britannic Bold", 18F, FontStyle.Regular, GraphicsUnit.Point, 0);
			checkBoxFibNumbers.Location = new Point(48, 268);
			checkBoxFibNumbers.Name = "checkBoxFibNumbers";
			checkBoxFibNumbers.Size = new Size(46, 37);
			checkBoxFibNumbers.TabIndex = 13;
			checkBoxFibNumbers.Text = "►";
			checkBoxFibNumbers.UseVisualStyleBackColor = true;
			checkBoxFibNumbers.CheckedChanged += checkBoxFibNumbers_CheckedChanged;
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(6F, 12F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 344);
			Controls.Add(checkBoxFibNumbers);
			Controls.Add(labelFN);
			Controls.Add(textBoxFibNumbers);
			Controls.Add(label2);
			Controls.Add(checkBoxSN);
			Controls.Add(labelSN);
			Controls.Add(label1);
			Controls.Add(button2);
			Controls.Add(textBoxSimpleNumbers);
			Name = "Form1";
			Text = "Form1";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private TextBox textBoxSimpleNumbers;
		private Button button2;
		private Label label1;
		private Label labelSN;
		private CheckBox checkBoxSN;
		private Label label2;
		private TextBox textBoxFibNumbers;
		private Label labelFN;
		private CheckBox checkBoxFibNumbers;
	}
}
