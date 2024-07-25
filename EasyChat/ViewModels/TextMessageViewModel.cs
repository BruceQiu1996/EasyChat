
namespace EasyChat.ViewModels
{
    /// <summary>
    /// 文本消息
    /// </summary>
    public class TextMessageViewModel : MessageViewModel
    {
        public TextMessageViewModel(string id, DateTime sendTime, string from, string to, bool fromSelf, string text) : base(id, sendTime, from, to, fromSelf)
        {
            Text = text;
        }

        public string Text { get; set; }

        public override string GetDesc()
        {
            return Text;
        }
    }
}
