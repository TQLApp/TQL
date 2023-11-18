using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.Azure.Support;
using Tql.Utilities;

namespace Tql.Plugins.Azure.Categories;

internal class PortalMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    private readonly PortalMatchDto _dto;

    public string Text { get; }
    public ImageSource Icon { get; }
    public MatchTypeId TypeId => TypeIds.Portal;

    public PortalMatch(PortalMatchDto dto)
    {
        _dto = dto;

        var resourceName = ResourceNames.GetResourceName(dto.Type, dto.Kind);
        if (resourceName?.Icon != null)
            Icon = resourceName.Icon;
        else
            Icon = Images.Azure;

        var sb = StringBuilderCache.Acquire();

        var resourceDisplayName = resourceName?.SingularDisplayName ?? dto.Type;

        if (
            !dto.ResourceGroup.IsEmpty()
            && !string.Equals(
                dto.Type,
                "Microsoft.Resources/subscriptions/resourceGroups",
                StringComparison.OrdinalIgnoreCase
            )
        )
            sb.Append(dto.ResourceGroup).Append('/');
        sb.Append(dto.Name).Append(" - ").Append(resourceDisplayName);

        Text = StringBuilderCache.GetStringAndRelease(sb);
    }

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(_dto.GetUrl());

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, _dto.GetUrl());

        return Task.CompletedTask;
    }
}

internal record PortalMatchDto(
    Guid ConnectionId,
    string DefaultDomain,
    string Id,
    string Name,
    string Type,
    string Kind,
    Guid SubscriptionId,
    string ResourceGroup,
    string NormalizedName
)
{
    public string GetUrl() =>
        $"https://portal.azure.com/#@{Uri.EscapeUriString(DefaultDomain)}/resource/{Id.TrimStart('/')}";
};
