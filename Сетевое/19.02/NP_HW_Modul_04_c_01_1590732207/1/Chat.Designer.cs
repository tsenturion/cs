namespace _1
{
    partial class ChatForm
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
			buttonSend = new Button();
			textBoxMessage = new TextBox();
			label1 = new Label();
			comboBoxChats = new ComboBox();
			buttonNewChat = new Button();
			SuspendLayout();
			// 
			// richTextBox1
			// 
			richTextBox1.Location = new Point(82, 51);
			richTextBox1.Name = "richTextBox1";
			richTextBox1.ScrollBars = RichTextBoxScrollBars.Vertical;
			richTextBox1.Size = new Size(176, 224);
			richTextBox1.TabIndex = 2;
			richTextBox1.Text = "";
			// 
			// buttonSend
			// 
			buttonSend.Font = new Font("Segoe UI Semilight", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
			buttonSend.Location = new Point(128, 336);
			buttonSend.Name = "buttonSend";
			buttonSend.Size = new Size(87, 41);
			buttonSend.TabIndex = 4;
			buttonSend.Text = "Send";
			buttonSend.UseVisualStyleBackColor = true;
			buttonSend.Click += buttonSend_Click;
			// 
			// textBoxMessage
			// 
			textBoxMessage.Location = new Point(82, 291);
			textBoxMessage.Name = "textBoxMessage";
			textBoxMessage.Size = new Size(176, 23);
			textBoxMessage.TabIndex = 5;
			textBoxMessage.TextChanged += textBoxMessage_TextChanged;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
			label1.Location = new Point(12, 9);
			label1.Name = "label1";
			label1.Size = new Size(101, 25);
			label1.TabIndex = 6;
			label1.Text = "Select chat";
			// 
			// comboBoxChats
			// 
			comboBoxChats.FormattingEnabled = true;
			comboBoxChats.Location = new Point(119, 14);
			comboBoxChats.Name = "comboBoxChats";
			comboBoxChats.Size = new Size(212, 20);
			comboBoxChats.TabIndex = 7;
			comboBoxChats.SelectedIndexChanged += comboBoxChats_SelectedIndexChanged;
			// 
			// buttonNewChat
			// 
			buttonNewChat.Location = new Point(12, 51);
			buttonNewChat.Name = "buttonNewChat";
			buttonNewChat.Size = new Size(64, 61);
			buttonNewChat.TabIndex = 8;
			buttonNewChat.Text = "New chat";
			buttonNewChat.UseVisualStyleBackColor = true;
			buttonNewChat.Click += buttonNewChat_Click;
			// 
			// ChatForm
			// 
			AutoScaleDimensions = new SizeF(6F, 12F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(343, 389);
			Controls.Add(buttonNewChat);
			Controls.Add(comboBoxChats);
			Controls.Add(label1);
			Controls.Add(textBoxMessage);
			Controls.Add(buttonSend);
			Controls.Add(richTextBox1);
			Name = "ChatForm";
			Text = "Chat";
			Load += ChatForm_Load;
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private RichTextBox richTextBox1;
		private Button buttonSend;
		private TextBox textBoxMessage;
		private Label label1;
		private ComboBox comboBoxChats;
		private Button buttonNewChat;
	}
}
