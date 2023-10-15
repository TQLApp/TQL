using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;

namespace Tql.Plugins.AzureDevOps.Categories.People;

internal class EmailMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch
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
        serviceProvider.GetRequiredService<IUI>().OpenUrl(GetUrl());

        return Task.CompletedTask;
    }

    private string GetUrl()
    {
        return $"mailto:{_dto.EmailAddress}";
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, GetUrl());

        return Task.CompletedTask;
    }
}
