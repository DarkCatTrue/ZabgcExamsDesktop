using System.Windows.Controls;
using System.Windows.Navigation;

namespace ZabgcExamsDesktop.MVVM.Model
{
    public class PageManager
    {
        public Frame mainFrame;
        private Dictionary<Type, Page> _pageCache = new Dictionary<Type, Page>();

        public PageManager(Frame frame)
        {
            mainFrame = frame;
        }

        public void ChangePage<T>() where T : Page, new()
        {
            var pageType = typeof(T);

            if (!_pageCache.ContainsKey(pageType))
            {
                _pageCache[pageType] = new T();
            }

            mainFrame.Navigate(_pageCache[pageType]);
            ClearNavigationHistory();
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
