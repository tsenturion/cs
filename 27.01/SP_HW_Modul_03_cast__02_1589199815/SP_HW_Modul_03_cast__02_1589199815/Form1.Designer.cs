namespace SP_HW_Modul_03_cast__02_1589199815
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
			panel1 = new Panel();
			buttonStart = new Button();
			numericUpDown1 = new NumericUpDown();
			((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
			SuspendLayout();
			// 
			// panel1
			// 
			panel1.Location = new Point(12, 36);
			panel1.Name = "panel1";
			panel1.Size = new Size(776, 402);
			panel1.TabIndex = 0;
			// 
			// buttonStart
			// 
			buttonStart.Location = new Point(360, 7);
			buttonStart.Name = "buttonStart";
			buttonStart.Size = new Size(75, 23);
			buttonStart.TabIndex = 1;
			buttonStart.Text = "Start";
			buttonStart.UseVisualStyleBackColor = true;
			buttonStart.Click += buttonStart_Click;
			// 
			// numericUpDown1
			// 
			numericUpDown1.Location = new Point(453, 7);
			numericUpDown1.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			numericUpDown1.Name = "numericUpDown1";
			numericUpDown1.Size = new Size(120, 23);
			numericUpDown1.TabIndex = 2;
			numericUpDown1.Value = new decimal(new int[] { 1, 0, 0, 0 });
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Controls.Add(numericUpDown1);
			Controls.Add(buttonStart);
			Controls.Add(panel1);
			Name = "Form1";
			Text = "Form1";
			((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
			ResumeLayout(false);
		}

		#endregion

		private Panel panel1;
		private Button buttonStart;
		private NumericUpDown numericUpDown1;
	}
}
