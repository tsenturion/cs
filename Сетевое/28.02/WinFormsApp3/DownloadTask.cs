using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;


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
    public class DownloadTask
    {
        public string Url { get; }
        public string SavePath { get; set; }
        public string Tags { get; }
        public string Threads { get; set; }
        private CancellationTokenSource cts;

        public event Action<string> ProgressChanged;
        public event Action Completed;
        public event Action Failed;
        public DownloadTask(string url, string savePath, string tags, int threads)
        {
            Url = url;
            SavePath = savePath;
            Tags = tags;
            Threads = threads.ToString();
            cts = new CancellationTokenSource();
        }
        public async Task StartDownload()
        {
            try{
                using (var client = new WebClient())
                {
                    client.DownloadProgressChanged += (s, e) =>
                    {
                        ProgressChanged?.Invoke($"{e.ProgressPercentage}%");
                    };
                    client.DownloadFileCompleted += (s, e) =>
                    {
                        if (e.Error == null)
                        {
                            Completed?.Invoke();
                        }
                        else
                        {
                            Failed?.Invoke();
                        }
                    };
                }
            }
            catch
            {
                Failed?.Invoke();
            }
        }
        public void Stop()
        {
            cts.Cancel();
            if (File.Exists(SavePath)) File.Delete(SavePath);
        }
        public void Pause()
        {
            cts.Cancel();
        }
    

    }
}
