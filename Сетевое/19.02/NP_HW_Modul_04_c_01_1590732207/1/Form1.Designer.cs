namespace _1
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
			label1 = new Label();
			richTextBox1 = new RichTextBox();
			checkBoxConnect = new CheckBox();
			buttonSend = new Button();
			textBoxMessage = new TextBox();
			SuspendLayout();
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(12, 9);
			label1.Name = "label1";
			label1.Size = new Size(108, 12);
			label1.TabIndex = 0;
			label1.Text = "Connect to server";
			// 
			// richTextBox1
			// 
			richTextBox1.Location = new Point(12, 52);
			richTextBox1.Name = "richTextBox1";
			richTextBox1.ScrollBars = RichTextBoxScrollBars.Vertical;
			richTextBox1.Size = new Size(176, 224);
			richTextBox1.TabIndex = 2;
			richTextBox1.Text = "";
			// 
			// checkBoxConnect
			// 
			checkBoxConnect.Appearance = Appearance.Button;
			checkBoxConnect.AutoSize = true;
			checkBoxConnect.Location = new Point(12, 24);
			checkBoxConnect.Name = "checkBoxConnect";
			checkBoxConnect.Size = new Size(61, 22);
			checkBoxConnect.TabIndex = 3;
			checkBoxConnect.Text = "Connect";
			checkBoxConnect.UseVisualStyleBackColor = true;
			checkBoxConnect.CheckedChanged += checkBoxConnect_CheckedChanged;
			// 
			// buttonSend
			// 
			buttonSend.Location = new Point(113, 320);
			buttonSend.Name = "buttonSend";
			buttonSend.Size = new Size(75, 23);
			buttonSend.TabIndex = 4;
			buttonSend.Text = "Send";
			buttonSend.UseVisualStyleBackColor = true;
			// 
			// textBoxMessage
			// 
			textBoxMessage.Location = new Point(12, 291);
			textBoxMessage.Name = "textBoxMessage";
			textBoxMessage.Size = new Size(176, 23);
			textBoxMessage.TabIndex = 5;
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(6F, 12F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(200, 355);
			Controls.Add(textBoxMessage);
			Controls.Add(buttonSend);
			Controls.Add(checkBoxConnect);
			Controls.Add(richTextBox1);
			Controls.Add(label1);
			Name = "Form1";
			Text = "Form1";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Label label1;
		private RichTextBox richTextBox1;
		private CheckBox checkBoxConnect;
		private Button buttonSend;
		private TextBox textBoxMessage;
	}
}
