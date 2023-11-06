using System.Windows.Media;

namespace Tql.Abstractions;

public interface IMatch
{
    string Text { get; }
    ImageSource Icon { get; }
    MatchTypeId TypeId { get; }
}
