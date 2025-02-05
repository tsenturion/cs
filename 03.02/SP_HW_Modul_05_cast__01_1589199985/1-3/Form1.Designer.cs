namespace _1_3
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
			buttonAnalyze = new Button();
			labelSentences = new Label();
			labelWords = new Label();
			labelSymbols = new Label();
			labelInterrogative = new Label();
			labelExclamatory = new Label();
			checkBox1 = new CheckBox();
			buttonStop = new Button();
			SuspendLayout();
			// 
			// richTextBox1
			// 
			richTextBox1.Location = new Point(210, 102);
			richTextBox1.Name = "richTextBox1";
			richTextBox1.Size = new Size(439, 147);
			richTextBox1.TabIndex = 0;
			richTextBox1.Text = "";
			// 
			// buttonAnalyze
			// 
			buttonAnalyze.Location = new Point(373, 58);
			buttonAnalyze.Name = "buttonAnalyze";
			buttonAnalyze.Size = new Size(75, 23);
			buttonAnalyze.TabIndex = 1;
			buttonAnalyze.Text = "Analyze";
			buttonAnalyze.UseVisualStyleBackColor = true;
			buttonAnalyze.Click += buttonAnalyze_Click;
			// 
			// labelSentences
			// 
			labelSentences.AutoSize = true;
			labelSentences.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
			labelSentences.Location = new Point(265, 274);
			labelSentences.Name = "labelSentences";
			labelSentences.Size = new Size(199, 21);
			labelSentences.TabIndex = 2;
			labelSentences.Text = "Количество предложений:";
			// 
			// labelWords
			// 
			labelWords.AutoSize = true;
			labelWords.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
			labelWords.Location = new Point(265, 295);
			labelWords.Name = "labelWords";
			labelWords.Size = new Size(132, 21);
			labelWords.TabIndex = 3;
			labelWords.Text = "Количество слов:";
			// 
			// labelSymbols
			// 
			labelSymbols.AutoSize = true;
			labelSymbols.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
			labelSymbols.Location = new Point(265, 316);
			labelSymbols.Name = "labelSymbols";
			labelSymbols.Size = new Size(169, 21);
			labelSymbols.TabIndex = 4;
			labelSymbols.Text = "Количество символов:";
			// 
			// labelInterrogative
			// 
			labelInterrogative.AutoSize = true;
			labelInterrogative.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
			labelInterrogative.Location = new Point(265, 337);
			labelInterrogative.Name = "labelInterrogative";
			labelInterrogative.Size = new Size(321, 21);
			labelInterrogative.TabIndex = 5;
			labelInterrogative.Text = "Количество вопросительных предложений:";
			// 
			// labelExclamatory
			// 
			labelExclamatory.AutoSize = true;
			labelExclamatory.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 204);
			labelExclamatory.Location = new Point(265, 358);
			labelExclamatory.Name = "labelExclamatory";
			labelExclamatory.Size = new Size(328, 21);
			labelExclamatory.TabIndex = 6;
			labelExclamatory.Text = "Количество восклицательных предложений:";
			// 
			// checkBox1
			// 
			checkBox1.AutoSize = true;
			checkBox1.Location = new Point(348, 33);
			checkBox1.Name = "checkBox1";
			checkBox1.Size = new Size(125, 19);
			checkBox1.TabIndex = 7;
			checkBox1.Text = "Сохранять в файл";
			checkBox1.UseVisualStyleBackColor = true;
			checkBox1.CheckedChanged += checkBox1_CheckedChanged;
			// 
			// buttonStop
			// 
			buttonStop.Enabled = false;
			buttonStop.Location = new Point(124, 150);
			buttonStop.Name = "buttonStop";
			buttonStop.Size = new Size(75, 49);
			buttonStop.TabIndex = 8;
			buttonStop.Text = "СТОП МАШИНА!";
			buttonStop.UseVisualStyleBackColor = true;
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Controls.Add(buttonStop);
			Controls.Add(checkBox1);
			Controls.Add(labelExclamatory);
			Controls.Add(labelInterrogative);
			Controls.Add(labelSymbols);
			Controls.Add(labelWords);
			Controls.Add(labelSentences);
			Controls.Add(buttonAnalyze);
			Controls.Add(richTextBox1);
			Name = "Form1";
			Text = "Form1";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private RichTextBox richTextBox1;
		private Button buttonAnalyze;
		private Label labelSentences;
		private Label labelWords;
		private Label labelSymbols;
		private Label labelInterrogative;
		private Label labelExclamatory;
		private CheckBox checkBox1;
		private Button buttonStop;
	}
}
