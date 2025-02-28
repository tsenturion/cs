namespace WinFormsApp3
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
            grpSettings = new GroupBox();
            bntDownload = new Button();
            txtTags = new TextBox();
            lblTags = new Label();
            numThreads = new NumericUpDown();
            lblThreads = new Label();
            btnBrowsePath = new Button();
            txtSavePath = new TextBox();
            lblSavePath = new Label();
            txtUrl = new TextBox();
            lblUrl = new Label();
            lstDownloads = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            btnPause = new Button();
            btnStop = new Button();
            btnDeleteDownload = new Button();
            lblSearchTags = new Label();
            txtSearchTags = new TextBox();
            btnSearch = new Button();
            btnRenameFile = new Button();
            btnMoveFile = new Button();
            grpSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numThreads).BeginInit();
            SuspendLayout();
            // 
            // grpSettings
            // 
            grpSettings.Controls.Add(bntDownload);
            grpSettings.Controls.Add(txtTags);
            grpSettings.Controls.Add(lblTags);
            grpSettings.Controls.Add(numThreads);
            grpSettings.Controls.Add(lblThreads);
            grpSettings.Controls.Add(btnBrowsePath);
            grpSettings.Controls.Add(txtSavePath);
            grpSettings.Controls.Add(lblSavePath);
            grpSettings.Controls.Add(txtUrl);
            grpSettings.Controls.Add(lblUrl);
            grpSettings.Location = new Point(10, 10);
            grpSettings.Name = "grpSettings";
            grpSettings.Size = new Size(648, 156);
            grpSettings.TabIndex = 0;
            grpSettings.TabStop = false;
            grpSettings.Text = "Настройки";
            // 
            // bntDownload
            // 
            bntDownload.Location = new Point(530, 110);
            bntDownload.Name = "bntDownload";
            bntDownload.Size = new Size(110, 40);
            bntDownload.TabIndex = 8;
            bntDownload.Text = "Скачать";
            bntDownload.UseVisualStyleBackColor = true;
            bntDownload.Click += bntDownload_Click;
            // 
            // txtTags
            // 
            txtTags.Location = new Point(80, 112);
            txtTags.Name = "txtTags";
            txtTags.Size = new Size(200, 39);
            txtTags.TabIndex = 7;
            // 
            // lblTags
            // 
            lblTags.AutoSize = true;
            lblTags.Location = new Point(10, 115);
            lblTags.Name = "lblTags";
            lblTags.Size = new Size(68, 32);
            lblTags.TabIndex = 6;
            lblTags.Text = "Теги:";
            // 
            // numThreads
            // 
            numThreads.Location = new Point(260, 85);
            numThreads.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numThreads.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numThreads.Name = "numThreads";
            numThreads.Size = new Size(50, 39);
            numThreads.TabIndex = 5;
            numThreads.Value = new decimal(new int[] { 3, 0, 0, 0 });
            // 
            // lblThreads
            // 
            lblThreads.AutoSize = true;
            lblThreads.Location = new Point(10, 85);
            lblThreads.Name = "lblThreads";
            lblThreads.Size = new Size(247, 32);
            lblThreads.TabIndex = 4;
            lblThreads.Text = "Количество потоков:";
            // 
            // btnBrowsePath
            // 
            btnBrowsePath.Location = new Point(510, 52);
            btnBrowsePath.Name = "btnBrowsePath";
            btnBrowsePath.Size = new Size(130, 40);
            btnBrowsePath.TabIndex = 1;
            btnBrowsePath.Text = "Выбрать...";
            btnBrowsePath.UseVisualStyleBackColor = true;
            btnBrowsePath.Click += btnBrowsePath_Click;
            // 
            // txtSavePath
            // 
            txtSavePath.Location = new Point(210, 52);
            txtSavePath.Name = "txtSavePath";
            txtSavePath.Size = new Size(350, 39);
            txtSavePath.TabIndex = 3;
            // 
            // lblSavePath
            // 
            lblSavePath.AutoSize = true;
            lblSavePath.Location = new Point(10, 55);
            lblSavePath.Name = "lblSavePath";
            lblSavePath.Size = new Size(206, 32);
            lblSavePath.TabIndex = 2;
            lblSavePath.Text = "Путь сохранения:";
            // 
            // txtUrl
            // 
            txtUrl.Location = new Point(135, 22);
            txtUrl.Name = "txtUrl";
            txtUrl.Size = new Size(378, 39);
            txtUrl.TabIndex = 1;
            // 
            // lblUrl
            // 
            lblUrl.AutoSize = true;
            lblUrl.Location = new Point(10, 25);
            lblUrl.Name = "lblUrl";
            lblUrl.Size = new Size(129, 32);
            lblUrl.TabIndex = 0;
            lblUrl.Text = "URL файла";
            // 
            // lstDownloads
            // 
            lstDownloads.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3, columnHeader4 });
            lstDownloads.FullRowSelect = true;
            lstDownloads.GridLines = true;
            lstDownloads.Location = new Point(10, 200);
            lstDownloads.Name = "lstDownloads";
            lstDownloads.Size = new Size(600, 300);
            lstDownloads.TabIndex = 1;
            lstDownloads.UseCompatibleStateImageBehavior = false;
            lstDownloads.View = View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Файл";
            columnHeader1.Width = 150;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Статус";
            columnHeader2.Width = 150;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Путь";
            columnHeader3.Width = 150;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "Теги";
            columnHeader4.Width = 150;
            // 
            // btnPause
            // 
            btnPause.Location = new Point(10, 510);
            btnPause.Name = "btnPause";
            btnPause.Size = new Size(100, 40);
            btnPause.TabIndex = 2;
            btnPause.Text = "Пауза";
            btnPause.UseVisualStyleBackColor = true;
            btnPause.Click += btnPause_Click;
            // 
            // btnStop
            // 
            btnStop.Location = new Point(120, 510);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(150, 40);
            btnStop.TabIndex = 3;
            btnStop.Text = "Остановить";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Click;
            // 
            // btnDeleteDownload
            // 
            btnDeleteDownload.Location = new Point(280, 510);
            btnDeleteDownload.Name = "btnDeleteDownload";
            btnDeleteDownload.Size = new Size(110, 40);
            btnDeleteDownload.TabIndex = 4;
            btnDeleteDownload.Text = "Удалить";
            btnDeleteDownload.UseVisualStyleBackColor = true;
            btnDeleteDownload.Click += btnDeleteDownload_Click;
            // 
            // lblSearchTags
            // 
            lblSearchTags.AutoSize = true;
            lblSearchTags.Location = new Point(10, 560);
            lblSearchTags.Name = "lblSearchTags";
            lblSearchTags.Size = new Size(190, 32);
            lblSearchTags.TabIndex = 5;
            lblSearchTags.Text = "Поиск по тегам:";
            // 
            // txtSearchTags
            // 
            txtSearchTags.Location = new Point(200, 560);
            txtSearchTags.Name = "txtSearchTags";
            txtSearchTags.Size = new Size(400, 39);
            txtSearchTags.TabIndex = 6;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(565, 560);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(90, 40);
            btnSearch.TabIndex = 7;
            btnSearch.Text = "Найти";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            // 
            // btnRenameFile
            // 
            btnRenameFile.Location = new Point(10, 600);
            btnRenameFile.Name = "btnRenameFile";
            btnRenameFile.Size = new Size(200, 40);
            btnRenameFile.TabIndex = 8;
            btnRenameFile.Text = "Переименовать";
            btnRenameFile.UseVisualStyleBackColor = true;
            btnRenameFile.Click += btnRenameFile_Click;
            // 
            // btnMoveFile
            // 
            btnMoveFile.Location = new Point(220, 600);
            btnMoveFile.Name = "btnMoveFile";
            btnMoveFile.Size = new Size(170, 40);
            btnMoveFile.TabIndex = 9;
            btnMoveFile.Text = "Переместить";
            btnMoveFile.UseVisualStyleBackColor = true;
            btnMoveFile.Click += btnMoveFile_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(664, 756);
            Controls.Add(btnMoveFile);
            Controls.Add(btnRenameFile);
            Controls.Add(btnSearch);
            Controls.Add(txtSearchTags);
            Controls.Add(lblSearchTags);
            Controls.Add(btnDeleteDownload);
            Controls.Add(btnStop);
            Controls.Add(btnPause);
            Controls.Add(lstDownloads);
            Controls.Add(grpSettings);
            Name = "Form1";
            Text = "Form1";
            grpSettings.ResumeLayout(false);
            grpSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numThreads).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox grpSettings;
        private Label lblUrl;
        private TextBox txtSavePath;
        private Label lblSavePath;
        private TextBox txtUrl;
        private Label lblThreads;
        private Button btnBrowsePath;
        private Label lblTags;
        private NumericUpDown numThreads;
        private Button bntDownload;
        private TextBox txtTags;
        private ListView lstDownloads;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private ColumnHeader columnHeader4;
        private Button btnPause;
        private Button btnStop;
        private Button btnDeleteDownload;
        private Label lblSearchTags;
        private TextBox txtSearchTags;
        private Button btnSearch;
        private Button btnRenameFile;
        private Button btnMoveFile;
    }
}
