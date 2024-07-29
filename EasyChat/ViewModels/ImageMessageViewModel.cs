using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Windows.Media.Imaging;

namespace EasyChat.ViewModels
{
    public class ImageMessageViewModel : MessageViewModel
    {
        public BitmapSource Source { get; set; }
        public string FileName { get; set; }

        public AsyncRelayCommand OpenImageClickCommandAsync { get; set; }

        public ImageMessageViewModel(string id, DateTime sendTime, string from, string to, bool fromSelf, byte[] data,string fileName) : base(id, sendTime, from, to, fromSelf)
        {
            BytesToSource(data);
            FileName = fileName;
            OpenImageClickCommandAsync = new AsyncRelayCommand(OpenImageClickAsync);
        }

        public void BytesToSource(byte[] data)
        {
            var source = new BitmapImage();
            try
            {
                using (MemoryStream ms = new MemoryStream(data))
                {
                    source.BeginInit();
                    source.StreamSource = ms;
                    source.CacheOption = BitmapCacheOption.OnLoad;
                    source.EndInit();

                    Source = source;
                }
            }
            finally
            {
                source.Freeze();
            }
        }

        private void SaveImageToFile(BitmapSource image, string filePath)
        {
            BitmapEncoder encoder = GetBitmapEncoder(filePath);
            encoder.Frames.Add(BitmapFrame.Create(image));
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(stream);
            }
        }

        private BitmapEncoder GetBitmapEncoder(string filePath)
        {
            var extName = Path.GetExtension(filePath).ToLower();
            if (extName.Equals(".png"))
            {
                return new PngBitmapEncoder();
            }
            else
            {
                return new JpegBitmapEncoder();
            }
        }

        private Task OpenImageClickAsync()
        {
            var folder = Path.Combine(Path.Combine(Path.GetTempPath(), "EasychatCacheImages"));
            var fileName = $"{Guid.NewGuid()}.png";
            var path = Path.Combine(folder, fileName);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            try
            {
                SaveImageToFile(Source, path);
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = path;
                process.StartInfo.Arguments = "rundl132.exe C://WINDOWS//system32//shimgvw.dll,ImageView_Fullscreen";
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                process.Start();
                process.Close();

                return Task.CompletedTask;
            }
            catch
            {
                return Task.CompletedTask;
            }
        }
    }
}
