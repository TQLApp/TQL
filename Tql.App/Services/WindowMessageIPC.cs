using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Tql.App.Services;

internal class WindowMessageIPC : IDisposable
{
    private const string MessageName =
#if DEBUG
        "$TQL.IPC^WM$#DEBUG";
#else
        "$TQL.IPC^WM$";
#endif

    private const string WindowName =
#if DEBUG
        "$TQL.IPC$#DEBUG";
#else
        "$TQL.IPC$";
#endif

    private readonly Window? _window;

    public bool IsFirstRunner { get; }

    public event EventHandler? Received;

    public WindowMessageIPC()
    {
        var windowMessage = RegisterWindowMessage(MessageName);

        var handle = FindWindow(null, WindowName);

        if (handle == IntPtr.Zero)
        {
            _window = new Window(windowMessage, OnReceived);
            IsFirstRunner = true;
        }
        else
        {
            PostMessage(handle, windowMessage, IntPtr.Zero, IntPtr.Zero);
        }
    }

    protected virtual void OnReceived() => Received?.Invoke(this, EventArgs.Empty);

    public void Dispose()
    {
        _window?.Dispose();
    }

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern uint RegisterWindowMessage(string lpString);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    private class Window : NativeWindow, IDisposable
    {
        private readonly uint _windowMessage;
        private readonly Action _action;

        public Window(uint windowMessage, Action action)
        {
            _windowMessage = windowMessage;
            _action = action;

            CreateHandle(new CreateParams { Caption = WindowName });
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == _windowMessage)
                _action();
        }

        public void Dispose()
        {
            DestroyHandle();
        }
    }
}
