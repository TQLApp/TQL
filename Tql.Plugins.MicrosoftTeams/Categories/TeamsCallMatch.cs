using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;

namespace Tql.Plugins.MicrosoftTeams.Categories;

internal class TeamsCallMatch(PersonDto dto) : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    public string Text => dto.DisplayName;
    public ImageSource Icon => Images.Teams;
    public MatchTypeId TypeId => TypeIds.TeamsCall;

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(GetUrl());

        return Task.CompletedTask;
    }

    private string GetUrl()
    {
        return $"msteams:/l/call/0/0?users={Uri.EscapeDataString(dto.EmailAddress)}";
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, GetUrl());

        return Task.CompletedTask;
    }
}
