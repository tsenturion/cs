namespace _1
{
	partial class LogInForm
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
			labelServers = new Label();
			comboBoxServersList = new ComboBox();
			labelLogin = new Label();
			textBoxLogin = new TextBox();
			buttonConnect = new Button();
			SuspendLayout();
			// 
			// labelServers
			// 
			labelServers.AutoSize = true;
			labelServers.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
			labelServers.Location = new Point(12, 9);
			labelServers.Name = "labelServers";
			labelServers.Size = new Size(156, 25);
			labelServers.TabIndex = 0;
			labelServers.Text = "Select chat server";
			// 
			// comboBoxServersList
			// 
			comboBoxServersList.FormattingEnabled = true;
			comboBoxServersList.Location = new Point(174, 12);
			comboBoxServersList.Name = "comboBoxServersList";
			comboBoxServersList.Size = new Size(162, 20);
			comboBoxServersList.TabIndex = 2;
			// 
			// labelLogin
			// 
			labelLogin.AutoSize = true;
			labelLogin.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
			labelLogin.Location = new Point(12, 60);
			labelLogin.Name = "labelLogin";
			labelLogin.Size = new Size(57, 25);
			labelLogin.TabIndex = 3;
			labelLogin.Text = "Login";
			// 
			// textBoxLogin
			// 
			textBoxLogin.Location = new Point(75, 62);
			textBoxLogin.MaxLength = 32;
			textBoxLogin.Name = "textBoxLogin";
			textBoxLogin.Size = new Size(261, 23);
			textBoxLogin.TabIndex = 4;
			// 
			// buttonConnect
			// 
			buttonConnect.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 204);
			buttonConnect.Location = new Point(101, 103);
			buttonConnect.Name = "buttonConnect";
			buttonConnect.Size = new Size(143, 45);
			buttonConnect.TabIndex = 5;
			buttonConnect.Text = "Connect";
			buttonConnect.UseVisualStyleBackColor = true;
			buttonConnect.Click += this.buttonConnect_Click;
			// 
			// LogInForm
			// 
			AutoScaleDimensions = new SizeF(6F, 12F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(348, 160);
			Controls.Add(buttonConnect);
			Controls.Add(textBoxLogin);
			Controls.Add(labelLogin);
			Controls.Add(comboBoxServersList);
			Controls.Add(labelServers);
			Name = "LogInForm";
			Text = "LogIn";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private Label labelServers;
		private ComboBox comboBoxServersList;
		private Label labelLogin;
		private TextBox textBoxLogin;
		private Button buttonConnect;
	}
}