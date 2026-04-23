using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace ZabgcExamsDesktop
{
    public static class SecureKeyStorage
    {
        private static string _cachedKey = null;

        public static string GetApiKey()
        {
            if (_cachedKey != null)
                return _cachedKey;

            try
            {
                byte[] encrypted = File.ReadAllBytes("license.dat");
                byte[] decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                _cachedKey = Encoding.UTF8.GetString(decrypted);
                return _cachedKey;
            }
            catch
            {
                MessageBox.Show("Ошибка API KEY. Убедитесь, что API ключ был установлен при помощи приложения ApiKeyCreate. Обратитесь к администратору.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
                return null;
            }
        }
    }
}
