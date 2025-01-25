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
			components = new System.ComponentModel.Container();
			timer1 = new System.Windows.Forms.Timer(components);
			textBox1 = new TextBox();
			button2 = new Button();
			label1 = new Label();
			labelSN = new Label();
			checkBox1 = new CheckBox();
			SuspendLayout();
			// 
			// timer1
			// 
			timer1.Interval = 1000;
			timer1.Tick += timer1_Tick;
			// 
			// textBox1
			// 
			textBox1.Location = new Point(137, 53);
			textBox1.Multiline = true;
			textBox1.Name = "textBox1";
			textBox1.ScrollBars = ScrollBars.Vertical;
			textBox1.Size = new Size(642, 96);
			textBox1.TabIndex = 5;
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
			// checkBox1
			// 
			checkBox1.Appearance = Appearance.Button;
			checkBox1.AutoSize = true;
			checkBox1.Font = new Font("Britannic Bold", 18F, FontStyle.Regular, GraphicsUnit.Point, 0);
			checkBox1.Location = new Point(48, 93);
			checkBox1.Name = "checkBox1";
			checkBox1.Size = new Size(46, 37);
			checkBox1.TabIndex = 9;
			checkBox1.Text = "►";
			checkBox1.UseVisualStyleBackColor = true;
			checkBox1.CheckedChanged += checkBox1_CheckedChanged;
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(6F, 12F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 436);
			Controls.Add(checkBox1);
			Controls.Add(labelSN);
			Controls.Add(label1);
			Controls.Add(button2);
			Controls.Add(textBox1);
			Name = "Form1";
			Text = "Form1";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private System.Windows.Forms.Timer timer1;
		private TextBox textBox1;
		private Button button2;
		private Label label1;
		private Label labelSN;
		private CheckBox checkBox1;
	}
}
