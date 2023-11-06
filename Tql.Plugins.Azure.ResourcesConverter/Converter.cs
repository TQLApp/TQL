using System.IO.Compression;
using System.Net.Http;
using Dasync.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Tql.Utilities;
using Formatting = Newtonsoft.Json.Formatting;

namespace Tql.Plugins.Azure.ResourcesConverter;

internal class Converter
{
    public async Task Run()
    {
        var assetTypes = ConvertResources();
        var icons = await ConvertRequireConfig();

        using var stream = File.Create(
            "..\\..\\..\\..\\Tql.Plugins.Azure\\Categories\\Resources.json.gz"
        );
        using var gzStream = new GZipStream(stream, CompressionLevel.Optimal);
        using var writer = new StreamWriter(gzStream);
        using var json = new JsonTextWriter(writer);

        var serializer = JsonSerializer.Create(
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }
        );

        serializer.Serialize(json, new AssetCollection(assetTypes, icons));
    }

    private List<AssetType> ConvertResources()
    {
        var obj = JObject.Parse(File.ReadAllText("..\\..\\..\\Resources.json"));
        var manifestJson = (JObject)obj["manifest"]!;
        var assetTypes = new List<AssetType>();
        var seenResourceTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var manifestItemJson in manifestJson)
        {
            var assetTypesJson = (JArray?)manifestItemJson.Value?["assetTypes"];
            if (assetTypesJson == null)
                continue;

            foreach (var assetTypeJson in assetTypesJson.Cast<JObject>())
            {
                var assetKinds = new List<AssetKind>();

                var kindsJson = (JArray?)assetTypeJson["resourceType"]?["kinds"];

                if (kindsJson != null)
                {
                    assetKinds.AddRange(
                        kindsJson.Select(p =>
                        {
                            var kinds = ((JArray)p["kinds"]!).Select(p1 => (string)p1!).ToList();

                            return new AssetKind(
                                (string?)p["name"] ?? throw new InvalidOperationException(),
                                (string?)p["singularDisplayName"],
                                (string?)p["pluralDisplayName"],
                                kinds.Count > 0 ? kinds : null,
                                CreateIcon((JObject)p)
                            );
                        })
                    );
                }

                var assetType = new AssetType(
                    (string?)assetTypeJson["name"] ?? throw new InvalidOperationException(),
                    (string?)assetTypeJson["singularDisplayName"],
                    (string?)assetTypeJson["pluralDisplayName"],
                    (string?)assetTypeJson["resourceType"]?["resourceTypeName"],
                    CreateIcon(assetTypeJson),
                    assetKinds.Count > 0 ? assetKinds : null
                );

                // Only add this asset if it has a resource name and deduplicate.
                if (
                    assetType.ResourceTypeName != null
                    && seenResourceTypes.Add(assetType.ResourceTypeName)
                )
                    assetTypes.Add(assetType);
            }
        }

        return assetTypes;
    }

    private async Task<List<AssetIcon>> ConvertRequireConfig()
    {
        var js = File.ReadAllText("..\\..\\..\\RequireConfig.js");

        var assets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var block in GetAllJsonBlocks(js, "requireconfig\""))
        {
            var obj = JObject.Parse(block);

            foreach (var entry in (JObject)obj["dependencyTree"]!)
            {
                var haveSvg = false;
                foreach (var item in (JObject)entry.Value!)
                {
                    if (item.Key.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                    {
                        haveSvg = true;
                        break;
                    }
                }

                if (haveSvg)
                {
                    var url = entry.Key;
                    if (!url.Contains("://", StringComparison.Ordinal))
                        url = "https://portal.azure.com/" + url.TrimStart('/');
                    url += ".js";

                    assets.Add(url);
                }
            }
        }

        var icons = new List<AssetIcon>();

        using var client = new HttpClient();

        var cachePath = Path.Combine(Path.GetTempPath(), "TqlAzureResourceConverter");
        Directory.CreateDirectory(cachePath);

        await assets.ParallelForEachAsync(
            async (asset) =>
            {
                var hash = Encryption.Sha1Hash(asset);
                var cacheFileName = Path.Combine(cachePath, hash);

                if (!File.Exists(cacheFileName))
                {
                    using (var source = await client.GetStreamAsync(asset))
                    using (var target = File.Create(cacheFileName + ".tmp"))
                    {
                        await source.CopyToAsync(target);
                    }

                    File.Move(cacheFileName + ".tmp", cacheFileName);
                }

                var data = File.ReadAllText(cacheFileName);

                foreach (var line in data.Split('\n'))
                {
                    if (!line.Contains("<svg"))
                        continue;

                    foreach (var block in GetAllJsonBlocks(line, "return"))
                    {
                        JObject obj;

                        try
                        {
                            obj = JObject.Parse(block);
                        }
                        catch
                        {
                            // Ignore.
                            continue;
                        }

                        lock (icons)
                        {
                            var type = (int)obj["type"]!;
                            if (type != 1)
                                icons.Add(new AssetIcon(type, (string)obj["data"]!));
                        }
                    }
                }
            }
        );

        icons.Sort((a, b) => a.Type.CompareTo(b.Type));

        return icons;
    }

    private AssetIcon? CreateIcon(JObject? element)
    {
        if (element?["icon"] is JObject icon)
        {
            var type = (int?)icon["type"];
            var data = (string?)icon["data"];

            if (!type.HasValue)
                throw new InvalidOperationException();

            return new AssetIcon(type.Value, data);
        }

        return null;
    }

    private IEnumerable<string> GetAllJsonBlocks(string data, string start)
    {
        var offset = 0;

        while (true)
        {
            var pos = data.IndexOf(start, offset, StringComparison.OrdinalIgnoreCase);
            if (pos == -1)
                break;
            pos = data.IndexOf('{', pos);
            if (pos == -1)
                throw new InvalidOperationException();

            offset = pos;

            yield return GetJsonBlock(data, ref offset);
        }
    }

    private string GetJsonBlock(string data, ref int offset)
    {
        var stack = new Stack<char>();
        var start = offset;

        for (var i = offset; i < data.Length; i++)
        {
            var c = data[i];

            switch (c)
            {
                case '{':
                    stack.Push('}');
                    break;
                case '[':
                    stack.Push(']');
                    break;

                case '}':
                case ']':
                    if (c != stack.Pop())
                        throw new InvalidOperationException();
                    if (stack.Count == 0)
                    {
                        offset = i + 1;
                        return data.Substring(start, offset - start);
                    }
                    break;
            }
        }

        throw new InvalidOperationException();
    }
}

record AssetCollection(List<AssetType> Assets, List<AssetIcon> Icons);

record AssetType(
    string Name,
    string? SingularDisplayName,
    string? PluralDisplayName,
    string? ResourceTypeName,
    AssetIcon? Icon,
    List<AssetKind>? Kinds
);

record AssetKind(
    string Name,
    string? SingularDisplayName,
    string? PluralDisplayName,
    List<string>? Kinds,
    AssetIcon? Icon
);

record AssetIcon(int Type, string? Data);
