using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ZabgcExamsDesktop.MVVM.Model
{
    public partial class BaseDto : ObservableObject
    {
        public bool IsNew { get; set; }
        [ObservableProperty] public bool _isEditing;
    }
}
