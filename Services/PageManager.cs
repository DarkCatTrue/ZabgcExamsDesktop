using System.Windows.Controls;

namespace ZabgcExamsDesktop.Services
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
            ClearNavigationHistory();
        }

        private void ClearNavigationHistory()
        {
            while (mainFrame.CanGoBack)
            {
                mainFrame.RemoveBackEntry();
            }
        }
    }
}
