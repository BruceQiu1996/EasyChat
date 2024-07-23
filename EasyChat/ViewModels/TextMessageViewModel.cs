namespace EasyChat.ViewModels
{
    /// <summary>
    /// 文本消息
    /// </summary>
    public class TextMessageViewModel : MessageViewModel
    {
        public string Text { get; set; }

        public override string GetDesc()
        {
            return Text;
        }
    }
}
