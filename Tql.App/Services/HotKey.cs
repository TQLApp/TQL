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
        ImmutableArray.Create(
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
            ((Keys)0x60, "Numericpad 0"),
            ((Keys)0x61, "Numericpad 1"),
            ((Keys)0x62, "Numericpad 2"),
            ((Keys)0x63, "Numericpad 3"),
            ((Keys)0x64, "Numericpad 4"),
            ((Keys)0x65, "Numericpad 5"),
            ((Keys)0x66, "Numericpad 6"),
            ((Keys)0x67, "Numericpad 7"),
            ((Keys)0x68, "Numericpad 8"),
            ((Keys)0x69, "Numericpad 9"),
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
            ((Keys)0x87, "F24"),
            ((Keys)0x6B, "Add"),
            ((Keys)0x08, "Backspace"),
            ((Keys)0xA6, "Browser Back"),
            ((Keys)0xAB, "Browser Favorites"),
            ((Keys)0xA7, "Browser Forward"),
            ((Keys)0xA8, "Browser Refresh"),
            ((Keys)0xAA, "Browser Search"),
            ((Keys)0xAC, "Browser Start and Home"),
            ((Keys)0xA9, "Browser Stop"),
            ((Keys)0x0C, "Clear"),
            ((Keys)0x6E, "Decimal"),
            ((Keys)0x2E, "Del"),
            ((Keys)0x6F, "Divide"),
            ((Keys)0x28, "Down Arrow"),
            ((Keys)0x23, "End"),
            ((Keys)0x0D, "Enter"),
            ((Keys)0x1B, "Esc"),
            ((Keys)0x2B, "Execute"),
            ((Keys)0x2F, "Help"),
            ((Keys)0x24, "Home"),
            ((Keys)0x2D, "Ins"),
            ((Keys)0x25, "Left Arrow"),
            ((Keys)0x6A, "Multiply"),
            ((Keys)0xB0, "Next Track"),
            ((Keys)0x22, "Page Down"),
            ((Keys)0x21, "Page Up"),
            ((Keys)0x13, "Pause"),
            ((Keys)0xB3, "Play/Pause Media"),
            ((Keys)0xB1, "Previous Track"),
            ((Keys)0x2A, "Print"),
            ((Keys)0x2C, "Print Screen"),
            ((Keys)0x27, "Right Arrow"),
            ((Keys)0x29, "Select"),
            ((Keys)0xB5, "Select Media"),
            ((Keys)0x6C, "Separator"),
            ((Keys)0x20, "Spacebar"),
            ((Keys)0xB6, "Start Application 1"),
            ((Keys)0xB7, "Start Application 2"),
            ((Keys)0xB4, "Start Mail"),
            ((Keys)0xB2, "Stop Media"),
            ((Keys)0x6D, "Subtract"),
            ((Keys)0x09, "Tab"),
            ((Keys)0x26, "Up Arrow"),
            ((Keys)0xAE, "Volume Down"),
            ((Keys)0xAD, "Volume Mute"),
            ((Keys)0xAF, "Volume Up")
        );

    public string ToJson() => JsonSerializer.Serialize(this);
}
