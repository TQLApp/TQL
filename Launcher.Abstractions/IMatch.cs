using System.Windows.Media;

namespace Launcher.Abstractions;

public interface IMatch
{
    string Text { get; }
    ImageSource Icon { get; }
    MatchTypeId TypeId { get; }
}
