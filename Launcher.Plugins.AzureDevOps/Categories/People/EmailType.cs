using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories.People;

internal class EmailType : IMatchType
{
    private readonly Images _images;

    public Guid Id => TypeIds.Email.Id;

    public EmailType(Images images)
    {
        _images = images;
    }

    public IMatch Deserialize(string json)
    {
        return new EmailMatch(JsonSerializer.Deserialize<GraphUserDto>(json)!, _images);
    }
}
