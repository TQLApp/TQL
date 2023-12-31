﻿// Taken from https://stackoverflow.com/questions/2450373/set-global-hotkeys-using-c-sharp.

using System.Windows.Forms;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Tql.App.Interop;

public sealed class KeyboardHook : IDisposable
{
    /// <summary>
    /// Represents the window that is used internally to get the messages.
    /// </summary>
    private class Window : NativeWindow, IDisposable
    {
        public Window()
        {
            CreateHandle(new CreateParams());
        }

        /// <summary>
        /// Overridden to get the notifications.
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            // check if we got a hot key pressed.
            if (m.Msg == PInvoke.WM_HOTKEY)
            {
                // get the keys.
                Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                ModifierKeys modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);

                // invoke the event to notify the parent.
                if (KeyPressed != null)
                    KeyPressed(this, new HotkeyPressedEventArgs(modifier, key));
            }
        }

        public event EventHandler<HotkeyPressedEventArgs>? KeyPressed;

        #region IDisposable Members

        public void Dispose()
        {
            this.DestroyHandle();
        }

        #endregion
    }

    private Window _window = new();
    private int _currentId;

    public KeyboardHook()
    {
        // register the event of the inner native window.
        _window.KeyPressed += delegate(object? sender, HotkeyPressedEventArgs args)
        {
            if (KeyPressed != null)
                KeyPressed(this, args);
        };
    }

    /// <summary>
    /// Registers a hot key in the system.
    /// </summary>
    /// <param name="modifier">The modifiers that are associated with the hot key.</param>
    /// <param name="key">The key itself that is associated with the hot key.</param>
    public int RegisterHotKey(ModifierKeys modifier, Keys key)
    {
        // increment the counter.
        _currentId = _currentId + 1;

        // register the hot key.
        if (
            !PInvoke.RegisterHotKey(
                new HWND(_window.Handle),
                _currentId,
                (HOT_KEY_MODIFIERS)modifier,
                (uint)key
            )
        )
            throw new InvalidOperationException(Labels.Error_CouldNotRegisterHotKey);

        return _currentId;
    }

    public void UnregisterHotKey(int id)
    {
        PInvoke.UnregisterHotKey(new HWND(_window.Handle), id);
    }

    /// <summary>
    /// A hot key has been pressed.
    /// </summary>
    public event EventHandler<HotkeyPressedEventArgs>? KeyPressed;

    #region IDisposable Members

    public void Dispose()
    {
        // unregister all the registered hot keys.
        for (int i = _currentId; i > 0; i--)
        {
            PInvoke.UnregisterHotKey(new HWND(_window.Handle), i);
        }

        // dispose the inner native window.
        _window.Dispose();
    }

    #endregion
}

/// <summary>
/// Event Args for the event that is fired after the hot key has been pressed.
/// </summary>
public class HotkeyPressedEventArgs(ModifierKeys modifier, Keys key) : EventArgs
{
    public ModifierKeys Modifier { get; } = modifier;
    public Keys Key { get; } = key;
}
