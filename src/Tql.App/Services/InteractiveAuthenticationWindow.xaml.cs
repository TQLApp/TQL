using System.Windows.Interop;
using Tql.Abstractions;
using Tql.App.Support;
using Tql.Utilities;
using IWin32Window = System.Windows.Forms.IWin32Window;

namespace Tql.App.Services;

internal partial class InteractiveAuthenticationWindow
{
    public class Factory(IPluginManager pluginManager)
    {
        public InteractiveAuthenticationWindow CreateInstance(
            InteractiveAuthenticationResource resource,
            Func<IWin32Window, Task> action,
            IUI ui
        ) => new(resource, action, pluginManager, ui);
    }

    private readonly Func<IWin32Window, Task> _action;
    private readonly IUI _ui;

    public Exception? Exception { get; private set; }

    public InteractiveAuthenticationWindow(
        InteractiveAuthenticationResource resource,
        Func<IWin32Window, Task> action,
        IPluginManager pluginManager,
        IUI ui
    )
    {
        _action = action;
        _ui = ui;

        InitializeComponent();

        var plugin = pluginManager.Plugins.Single(p => p.Id == resource.PluginId);

        _resourceName.Text = $"{plugin.Title} - {resource.ResourceName}";
        _resourceIcon.Source = resource.ResourceIcon;
    }

    private async void _acceptButton_Click(object? sender, RoutedEventArgs e)
    {
        var handle = new WindowInteropHelper(this).Handle;

        while (true)
        {
            try
            {
                await _action(new Win32Window(handle));

                Close();
                return;
            }
            catch (Exception ex)
            {
                var result = _ui.ShowException(
                    this,
                    Labels.InteractiveAuthenticationWindow_FailedToAuthenticate,
                    ex,
                    buttons: DialogCommonButtons.Retry | DialogCommonButtons.Cancel
                );

                if (result != System.Windows.Forms.DialogResult.Retry)
                {
                    Exception = ex;

                    Close();
                    return;
                }
            }
        }
    }
}
