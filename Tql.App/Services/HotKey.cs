using System.Windows.Forms;

namespace Tql.App.Services;

internal record HotKey(bool Win, bool Control, bool Alt, bool Shift, Keys Key)
{
#if DEBUG
    public static HotKey Default = new(false, false, true, false, Keys.Back);
#else
    public static HotKey Default = new(false, false, true, false, Keys.Space);
#endif

    public static HotKey FromJson(string json) => JsonSerializer.Deserialize<HotKey>(json)!;

    public static HotKey FromSettings(Settings settings)
    {
        var hotKeyJson = settings.HotKey;
        if (hotKeyJson != null)
            return FromJson(hotKeyJson);

        return Default;
    }

    public static readonly ImmutableArray<(Keys Key, string Label)> AvailableKeys =
        BuildAvailableKeys();

    private static ImmutableArray<(Keys Key, string Label)> BuildAvailableKeys()
    {
        var standardKeys = new List<(Keys Key, string Label)>
        {
            ((Keys)0x41, "A"),
            ((Keys)0x42, "B"),
            ((Keys)0x43, "C"),
            ((Keys)0x44, "D"),
            ((Keys)0x45, "E"),
            ((Keys)0x46, "F"),
            ((Keys)0x47, "G"),
            ((Keys)0x48, "H"),
            ((Keys)0x49, "I"),
            ((Keys)0x4A, "J"),
            ((Keys)0x4B, "K"),
            ((Keys)0x4C, "L"),
            ((Keys)0x4D, "M"),
            ((Keys)0x4E, "N"),
            ((Keys)0x4F, "O"),
            ((Keys)0x50, "P"),
            ((Keys)0x51, "Q"),
            ((Keys)0x52, "R"),
            ((Keys)0x53, "S"),
            ((Keys)0x54, "T"),
            ((Keys)0x55, "U"),
            ((Keys)0x56, "V"),
            ((Keys)0x57, "W"),
            ((Keys)0x58, "X"),
            ((Keys)0x59, "Y"),
            ((Keys)0x5A, "Z"),
            ((Keys)0x30, "0"),
            ((Keys)0x31, "1"),
            ((Keys)0x32, "2"),
            ((Keys)0x33, "3"),
            ((Keys)0x34, "4"),
            ((Keys)0x35, "5"),
            ((Keys)0x36, "6"),
            ((Keys)0x37, "7"),
            ((Keys)0x38, "8"),
            ((Keys)0x39, "9"),
            ((Keys)0x60, string.Format(Labels.HotKeyNumericPadKey, 0)),
            ((Keys)0x61, string.Format(Labels.HotKeyNumericPadKey, 1)),
            ((Keys)0x62, string.Format(Labels.HotKeyNumericPadKey, 2)),
            ((Keys)0x63, string.Format(Labels.HotKeyNumericPadKey, 3)),
            ((Keys)0x64, string.Format(Labels.HotKeyNumericPadKey, 4)),
            ((Keys)0x65, string.Format(Labels.HotKeyNumericPadKey, 5)),
            ((Keys)0x66, string.Format(Labels.HotKeyNumericPadKey, 6)),
            ((Keys)0x67, string.Format(Labels.HotKeyNumericPadKey, 7)),
            ((Keys)0x68, string.Format(Labels.HotKeyNumericPadKey, 8)),
            ((Keys)0x69, string.Format(Labels.HotKeyNumericPadKey, 9)),
            ((Keys)0x70, "F1"),
            ((Keys)0x71, "F2"),
            ((Keys)0x72, "F3"),
            ((Keys)0x73, "F4"),
            ((Keys)0x74, "F5"),
            ((Keys)0x75, "F6"),
            ((Keys)0x76, "F7"),
            ((Keys)0x77, "F8"),
            ((Keys)0x78, "F9"),
            ((Keys)0x79, "F10"),
            ((Keys)0x7A, "F11"),
            ((Keys)0x7B, "F12"),
            ((Keys)0x7C, "F13"),
            ((Keys)0x7D, "F14"),
            ((Keys)0x7E, "F15"),
            ((Keys)0x7F, "F16"),
            ((Keys)0x80, "F17"),
            ((Keys)0x81, "F18"),
            ((Keys)0x82, "F19"),
            ((Keys)0x83, "F20"),
            ((Keys)0x84, "F21"),
            ((Keys)0x85, "F22"),
            ((Keys)0x86, "F23"),
            ((Keys)0x87, "F24")
        };

        var labeledKeys = new List<(Keys Key, string Label)>
        {
            ((Keys)0x6B, Labels.HotKeyAddKey),
            ((Keys)0x08, Labels.HotKeyBackspaceKey),
            ((Keys)0xA6, Labels.HotKeyBrowserBackKey),
            ((Keys)0xAB, Labels.HotKeyBrowserFavoritesKey),
            ((Keys)0xA7, Labels.HotKeyBrowserForwardKey),
            ((Keys)0xA8, Labels.HotKeyBrowserRefreshKey),
            ((Keys)0xAA, Labels.HotKeyBrowserSearchKey),
            ((Keys)0xAC, Labels.HotKeyBrowserStartAndHomeKey),
            ((Keys)0xA9, Labels.HotKeyBrowserStopKey),
            ((Keys)0x0C, Labels.HotKeyClearKey),
            ((Keys)0x6E, Labels.HotKeyDecimalKey),
            ((Keys)0x2E, Labels.HotKeyDelKey),
            ((Keys)0x6F, Labels.HotKeyDivideKey),
            ((Keys)0x28, Labels.HotKeyDownArrowKey),
            ((Keys)0x23, Labels.HotKeyEndKey),
            ((Keys)0x0D, Labels.HotKeyEnterKey),
            ((Keys)0x1B, Labels.HotKeyEscKey),
            ((Keys)0x2B, Labels.HotKeyExecuteKey),
            ((Keys)0x2F, Labels.HotKeyHelpKey),
            ((Keys)0x24, Labels.HotKeyHomeKey),
            ((Keys)0x2D, Labels.HotKeyInsKey),
            ((Keys)0x25, Labels.HotKeyLeftArrowKey),
            ((Keys)0x6A, Labels.HotKeyMultiplyKey),
            ((Keys)0xB0, Labels.HotKeyNextTrackKey),
            ((Keys)0x22, Labels.HotKeyPageDownKey),
            ((Keys)0x21, Labels.HotKeyPageUpKey),
            ((Keys)0x13, Labels.HotKeyPauseKey),
            ((Keys)0xB3, Labels.HotKeyPlayPauseMediaKey),
            ((Keys)0xB1, Labels.HotKeyPreviousTrackKey),
            ((Keys)0x2A, Labels.HotKeyPrintKey),
            ((Keys)0x2C, Labels.HotKeyPrintScreenKey),
            ((Keys)0x27, Labels.HotKeyRightArrowKey),
            ((Keys)0x29, Labels.HotKeySelectKey),
            ((Keys)0xB5, Labels.HotKeySelectMediaKey),
            ((Keys)0x6C, Labels.HotKeySeparatorKey),
            ((Keys)0x20, Labels.HotKeySpacebarKey),
            ((Keys)0xB6, Labels.HotKeyStartApplication1Key),
            ((Keys)0xB7, Labels.HotKeyStartApplication2Key),
            ((Keys)0xB4, Labels.HotKeyStartMailKey),
            ((Keys)0xB2, Labels.HotKeyStopMediaKey),
            ((Keys)0x6D, Labels.HotKeySubtractKey),
            ((Keys)0x09, Labels.HotKeyTabKey),
            ((Keys)0x26, Labels.HotKeyUpArrowKey),
            ((Keys)0xAE, Labels.HotKeyVolumeDownKey),
            ((Keys)0xAD, Labels.HotKeyVolumeMuteKey),
            ((Keys)0xAF, Labels.HotKeyVolumeUpKey)
        };

        labeledKeys.Sort(
            (a, b) => string.Compare(a.Label, b.Label, StringComparison.CurrentCultureIgnoreCase)
        );

        return standardKeys.Concat(labeledKeys).ToImmutableArray();
    }

    public string ToJson() => JsonSerializer.Serialize(this);
}
