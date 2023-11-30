using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;

namespace Tql.Plugins.Outlook.Categories;

internal class EmailMatch(PersonDto dto) : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    public string Text => dto.DisplayName;
    public ImageSource Icon => Images.Outlook;
    public MatchTypeId TypeId => TypeIds.Email;

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(GetUrl());

        return Task.CompletedTask;
    }

    private string GetUrl()
    {
        return $"mailto:{dto.EmailAddress}";
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
