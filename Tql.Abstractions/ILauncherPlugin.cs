using Microsoft.Extensions.DependencyInjection;

namespace Tql.Abstractions;

public interface ILauncherPlugin
{
    Guid Id { get; }

    void ConfigureServices(IServiceCollection services);

    void Initialize(IServiceProvider serviceProvider);

    IMatch? DeserializeMatch(Guid typeId, string json);

    IEnumerable<IMatch> GetMatches();
}
