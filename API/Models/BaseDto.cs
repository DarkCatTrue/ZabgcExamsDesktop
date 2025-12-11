using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ZabgcExamsDesktop.API.Models
{
    public abstract class BaseDto : INotifyPropertyChanged
    {
        private bool _isEditing;
        public bool IsNew { get; set; }
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (_isEditing != value)
                {
                    _isEditing = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsPersisted { get; set; }
       
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
