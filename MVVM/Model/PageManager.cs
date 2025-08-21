using System.Windows.Controls;

namespace ZabgcExamsDesktop.MVVM.Model
{
    public class PageManager
    {
        public Frame mainFrame;
        public PageManager(Frame frame)
        {
            mainFrame = frame;
        }
        public void ChangePage(Page page)
        {
            mainFrame.Navigate(page);
        }
    }
}
