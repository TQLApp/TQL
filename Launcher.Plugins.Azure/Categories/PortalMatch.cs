using Launcher.Abstractions;
using Launcher.Plugins.Azure.Support;
using Microsoft.Extensions.DependencyInjection;

namespace Launcher.Plugins.Azure.Categories;

internal class PortalMatch : IRunnableMatch, ISerializableMatch
{
    private readonly PortalMatchDto _dto;

    public string Text { get; }
    public IImage Icon { get; }
    public MatchTypeId TypeId => TypeIds.Portal;

    public PortalMatch(PortalMatchDto dto, Images images, IImageFactory imageFactory)
    {
        var resourceName = ResourceNames.GetResourceName(dto.Type, dto.Kind);
        _dto = dto;

        if (resourceName?.Icon != null)
            Icon = imageFactory.FromImageSource(resourceName.Icon);
        else
            Icon = images.Azure;

        var sb = StringBuilderCache.Acquire();

        var resourceDisplayName = resourceName?.SingularDisplayName ?? dto.Type;

        if (!dto.ResourceGroup.IsEmpty())
            sb.Append(dto.ResourceGroup).Append('/');
        sb.Append(dto.Name).Append(" - ").Append(resourceDisplayName);

        Text = StringBuilderCache.GetStringAndRelease(sb);
    }

    public Task Run(IServiceProvider serviceProvider, Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().LaunchUrl(_dto.GetUrl());

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
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
