using System.IO;
using System.Windows.Media.Imaging;

namespace EasyChat.ViewModels
{
    public class ImageMessageViewModel : MessageViewModel
    {
        public BitmapSource Source { get; set; }
        public string FileName { get; set; }

        public ImageMessageViewModel(string id, DateTime sendTime, string from, string to, bool fromSelf, byte[] data,string fileName) : base(id, sendTime, from, to, fromSelf)
        {
            BytesToSource(data);
            FileName = fileName;
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
    }
}
