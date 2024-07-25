using EasyChat.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace EasyChat.Helpers
{
    public class MessageDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var fe = container as FrameworkElement;
            var obj = item as MessageViewModel;
            DataTemplate dt = null;
            if (obj != null && fe != null)
            {
                if (obj is TimeMessageViewModel)
                    dt = fe.FindResource("time") as DataTemplate;
                if (obj is TextMessageViewModel && obj.FromSelf)
                    dt = fe.FindResource("mytext") as DataTemplate;
                if (obj is TextMessageViewModel && !obj.FromSelf)
                    dt = fe.FindResource("hertext") as DataTemplate;
                if (obj is ImageMessageViewModel && obj.FromSelf)
                    dt = fe.FindResource("myimage") as DataTemplate;
                if (obj is ImageMessageViewModel && !obj.FromSelf)
                    dt = fe.FindResource("herimage") as DataTemplate;
            }

            return dt;
        }
    }
}
