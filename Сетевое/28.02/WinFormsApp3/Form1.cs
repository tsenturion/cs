using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;

namespace WinFormsApp3
{
    /*
    Пользователь указывает путь для скачивания и путь для сохранения, ко
    личествопотоковдляскачивания,тегидляскачиваемогофайла(необязатель
    но). Пользовательможетприостановитьзакачку,остановитьзакачку,удалить 
    закачку. Пользователь может искать скачанные файлы по тегам. Интерфейс 
    приложенияотображаетудачные,неудачныеитекущиезакачки.Пользователь 
    может через интерфейс приложения удалять, переименовывать, перемещать 
    скачанныефайлы.
    */
    public partial class Form1 : Form
    {
        private List<DownloadTask> downloads = new List<DownloadTask>();
        private object lockObj = new object();
        public Form1()
        {
            InitializeComponent();
        }

        private async void bntDownload_Click(object sender, EventArgs e)
        {
            string url = txtUrl.Text.Trim();
            string savePath = txtSavePath.Text.Trim();
            string tags = txtTags.Text.Trim();
            int threads = (int)numThreads.Value;

            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(savePath))
            {
                MessageBox.Show("Поля путь для скачивания и путь для сохранения не могут быть пустыми.", 
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string fileName = Path.GetFileName(new Uri(url).LocalPath);
            string fullSavePath = Path.Combine(savePath, fileName);

            var task = new DownloadTask(url, fullSavePath, tags, threads);
            downloads.Add(task);
            
            var listViewItem = new ListViewItem(new [] { fileName, "Загрузка...", fullSavePath, tags}) { Tag = task };
            lstDownloads.Items.Add(listViewItem);

            task.ProgressChanged += (status) => {
                this.Invoke((Action)(() => listViewItem.SubItems[1].Text = status));
            };
            task.Completed += () => {
                this.Invoke((Action)(() => listViewItem.SubItems[1].Text =  "Готово"));
            };
            task.Failed += () => {
                this.Invoke((Action)(() => listViewItem.SubItems[1].Text = "Ошибка"));
            };
            await task.StartDownload();
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if(lstDownloads.SelectedItems.Count > 0)
            {
                DownloadTask task = lstDownloads.SelectedItems[0].Tag as DownloadTask;
                task?.Pause();
                lstDownloads.SelectedItems[0].SubItems[1].Text = "Пауза";
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if(lstDownloads.SelectedItems.Count > 0)
            {
                DownloadTask task = lstDownloads.SelectedItems[0].Tag as DownloadTask;
                task?.Stop();
                lstDownloads.SelectedItems[0].SubItems[1].Text = "Остановлено";
            }

        }

        private void btnDeleteDownload_Click(object sender, EventArgs e)
        {
            if(lstDownloads.SelectedItems.Count > 0)
            {
                DownloadTask task = lstDownloads.SelectedItems[0].Tag as DownloadTask;
                task?.Stop();
                downloads.Remove(task);
                lstDownloads.Items.Remove(lstDownloads.SelectedItems[0]);
            }

        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchTags = txtSearchTags.Text.Trim();
            if(string.IsNullOrWhiteSpace(searchTags))
            {
                MessageBox.Show("Теги для поиска не могут быть пустыми.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            foreach (ListViewItem item in lstDownloads.Items)
            {
                if (!item.SubItems[3].Text.Contains(searchTags))
                {
                    item.Remove();
                }
            }
        }

        private void btnRenameFile_Click(object sender, EventArgs e)
        {
            if(lstDownloads.SelectedItems.Count > 0)
            {
                DownloadTask task = lstDownloads.SelectedItems[0].Tag as DownloadTask;
                string newName = Microsoft.VisualBasic.Interaction.InputBox("Введите новое имя файла:", 
                "Переименование файла", Path.GetFileName(task.SavePath));
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    string newPath = Path.Combine(Path.GetDirectoryName(task.SavePath), newName);
                    
                    File.Move(task.SavePath, newPath);
                    task.SavePath = newPath;
                    lstDownloads.SelectedItems[0].SubItems[2].Text = newPath;
                }
            }
        }

        private void btnMoveFile_Click(object sender, EventArgs e)
        {
            if(lstDownloads.SelectedItems.Count > 0)
            {
                DownloadTask task = lstDownloads.SelectedItems[0].Tag as DownloadTask;
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        string newPath = Path.Combine(dialog.SelectedPath, Path.GetFileName(task.SavePath));
                        
                        File.Move(task.SavePath, newPath);
                        task.SavePath = newPath;
                        lstDownloads.SelectedItems[0].SubItems[2].Text = newPath;
                    }
                }
            }
        }

        private void btnBrowsePath_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtSavePath.Text = dialog.SelectedPath;
                }
            }
        }
    }
}
