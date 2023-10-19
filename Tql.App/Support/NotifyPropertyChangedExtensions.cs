using System.ComponentModel;

namespace Tql.App.Support;

internal static class NotifyPropertyChangedExtensions
{
    public static void AttachPropertyChanged(
        this INotifyPropertyChanged self,
        string propertyName,
        Action<object, PropertyChangedEventArgs> action
    )
    {
        self.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == propertyName)
                action(s, e);
        };
    }
}
