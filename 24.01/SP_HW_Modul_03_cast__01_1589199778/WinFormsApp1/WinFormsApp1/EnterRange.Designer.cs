namespace WinFormsApp1
{
	partial class EnterRange
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
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
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			label1 = new Label();
			textBoxStartValue = new TextBox();
			textBoxEndValue = new TextBox();
			label2 = new Label();
			label3 = new Label();
			buttonSubmit = new Button();
			SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Font = new Font("Segoe UI Semilight", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 204);
			label1.Location = new Point(50, 21);
			label1.Name = "label1";
			label1.Size = new Size(283, 30);
			label1.TabIndex = 0;
			label1.Text = "Введите границы диапазона";
			// 
			// textBoxStartValue
			// 
			textBoxStartValue.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
			textBoxStartValue.Location = new Point(50, 66);
			textBoxStartValue.MaxLength = 3;
			textBoxStartValue.Name = "textBoxStartValue";
			textBoxStartValue.Size = new Size(124, 33);
			textBoxStartValue.TabIndex = 2;
			textBoxStartValue.TextChanged += textBox1_TextChanged;
			// 
			// textBoxEndValue
			// 
			textBoxEndValue.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
			textBoxEndValue.Location = new Point(233, 66);
			textBoxEndValue.MaxLength = 3;
			textBoxEndValue.Name = "textBoxEndValue";
			textBoxEndValue.Size = new Size(124, 33);
			textBoxEndValue.TabIndex = 3;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Font = new Font("Segoe UI Semilight", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
			label2.Location = new Point(12, 72);
			label2.Name = "label2";
			label2.Size = new Size(32, 21);
			label2.TabIndex = 4;
			label2.Text = "От:";
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Font = new Font("Segoe UI Semilight", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
			label3.Location = new Point(193, 72);
			label3.Name = "label3";
			label3.Size = new Size(34, 21);
			label3.TabIndex = 5;
			label3.Text = "До:";
			// 
			// buttonSubmit
			// 
			buttonSubmit.DialogResult = DialogResult.OK;
			buttonSubmit.Font = new Font("Segoe UI Semilight", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
			buttonSubmit.Location = new Point(153, 113);
			buttonSubmit.Name = "buttonSubmit";
			buttonSubmit.Size = new Size(84, 32);
			buttonSubmit.TabIndex = 6;
			buttonSubmit.Text = "Submit";
			buttonSubmit.UseVisualStyleBackColor = true;
			// 
			// EnterRange
			// 
			AutoScaleDimensions = new SizeF(6F, 12F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(381, 157);
			Controls.Add(buttonSubmit);
			Controls.Add(label3);
			Controls.Add(label2);
			Controls.Add(textBoxEndValue);
			Controls.Add(textBoxStartValue);
			Controls.Add(label1);
			Name = "EnterRange";
			Text = "EnterRange";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Label label1;
		private TextBox textBoxStartValue;
		private TextBox textBoxEndValue;
		private Label label2;
		private Label label3;
		private Button buttonSubmit;
	}
}