using Launcher.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Launcher.Plugins.AzureDevOps.Categories.People;

internal class EmailMatch : IRunnableMatch, ISerializableMatch
{
    private readonly GraphUserDto _dto;

    public string Text => _dto.DisplayName;
    public ImageSource Icon => Images.Outlook;
    public MatchTypeId TypeId => TypeIds.Email;

    public EmailMatch(GraphUserDto dto)
    {
        _dto = dto;
    }

    public Task Run(IServiceProvider serviceProvider, Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().LaunchUrl($"mailto:{_dto.EmailAddress}");

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }
}
