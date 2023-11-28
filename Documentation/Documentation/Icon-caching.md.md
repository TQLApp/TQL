# Icon caching

This guide shows how to cache icons. We'll extend the NuGet plugin from the
[Create a plugin](Create-a-plugin.md) guide with the ability to download icons
from GitHub and show them in the matches. The
[TQLApp.Utilities](https://www.nuget.org/packages/TQLApp.Utilities) NuGet
package contains functionality to simplify this work.

## Create a IconCacheManager

Cache management is done using the `IconCacheManager<T>` class. To use this
class, a new class must be created that inherits from it.

1. Add a new class named **IconCacheManager** and paste in the following
   content:

   ```cs
   using System.IO;
   using System.Net.Http;
   using Microsoft.Extensions.Logging;
   using Tql.Abstractions;
   using Tql.Utilities;
   
   namespace TqlNuGetPlugin;
   
   internal class IconCacheManager(
       IStore store,
       ILogger<IconCacheManager> logger,
       HttpClient httpClient
   )
       : IconCacheManager<PackageIcon>(
           store,
           logger,
           new IconCacheManagerConfiguration(Plugin.PluginId, TimeSpan.FromDays(1))
       )
   {
       protected override async Task<IconData> LoadIcon(PackageIcon key)
       {
           using var response = await httpClient.GetAsync(key.IconUrl);
   
           await using var stream = await response.Content.ReadAsStreamAsync();
           using var target = new MemoryStream();
   
           await stream.CopyToAsync(target);
   
           return new IconData(response.Content.Headers.ContentType?.MediaType, target.ToArray());
       }
   }
   
   internal record PackageIcon(string IconUrl);
   ```

2. Add this class to the DI container. Update the **ConfigureServices** method in the **Plugin** class:
   
   ```cs
   public void ConfigureServices(IServiceCollection services)
   {
       services.AddSingleton<IconCacheManager>();
   
       ...
   ```

3. Update the **PackageDto** class in the **PackageMatch.cs** file with a new property to track the icon URL:
   
   ```cs
   internal record PackageDto(string PackageId, string? IconUrl);
   ```

4. Add the **IconCacheManager** to the primary constructor of the **PackageMatch** class replace the **Icon** property with an implementation that gets the icon from the icon cache manager:
   
   ```cs
   internal class PackageMatch(PackageDto dto, IconCacheManager iconCacheManager)
       : IRunnableMatch,
           ISerializableMatch,
           ICopyableMatch
   {
       public string Text => dto.PackageId;

       public ImageSource Icon
       {
           get
           {
               var icon = default(ImageSource);
               if (dto.IconUrl != null)
                   icon = iconCacheManager.GetIcon(new PackageIcon(dto.IconUrl));
   
               return icon ?? Images.NuGetLogo;
           }
       }

       ...
   ```

If you now compile the project, you'll get a compilation error that the **IconCacheManager** needs to be provided. Normally, this would be done by weaving through this required parameter everywhere **PackageMatch** is instantiated.

TQL provides an alternative option. The **IMatchFactory** interface provides a factory for match classes. This factory takes two generic arguments: the type of the match class and the type of the DTO object. It assumes that any match class will have one DTO parameter, and the rest can be retrieved from DI.

5. Add a **IMatchFactory** parameter to the **PackagesMatch** class:
   
   ```cs
   internal class PackagesMatch(NuGetClient client, IMatchFactory<PackageMatch, PackageDto> factory)
       : ISearchableMatch,
           ISerializableMatch
   ```

6. Change the way the **PackageMatch** class is instantiated in the **Search** method:
   
   ```cs
   public async Task<IEnumerable<IMatch>> Search(
       ISearchContext context,
       string text,
       CancellationToken cancellationToken
   )
   {
       ...
   
       return from package in await client.Search(text, cancellationToken)
           select factory.Create(new PackageDto(package.Identity.Id, package.IconUrl?.ToString()));
   }
   ```

If you now run the app, the icons should appear:

![=2x](Package-match-icons.png)

They won't appear the first time though. The **GetIcon** method of the cache manager is synchronous. However, loading the icon is done asynchronously. The first time an icon is retrieved the icons will be downloaded in the background. Then, if the icon is downloaded by the next time the **GetIcon** method is called, the cached icon is returned. This is done to not slow down the users search query, and to simplify using the icon cache manager.