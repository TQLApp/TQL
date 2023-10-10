using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Formatting = Newtonsoft.Json.Formatting;

var obj = JObject.Parse(File.ReadAllText("..\\..\\..\\Resources.json"));
var manifestJson = (JObject)obj["manifest"]!;
var assetTypes = new List<AssetType>();
var seenResourceTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
var seenIconTypes = new HashSet<int>();

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
        if (assetType.ResourceTypeName != null && seenResourceTypes.Add(assetType.ResourceTypeName))
            assetTypes.Add(assetType);
    }
}

var doc = new HtmlDocument();

doc.Load("..\\..\\..\\FxImages.html");

var icons = new List<AssetIcon>();

foreach (var element in doc.DocumentNode.Descendants("symbol"))
{
    foreach (var title in element.Descendants("title").Reverse().ToList())
    {
        title.Remove();
    }

    element.Name = "svg";
    element.Attributes.Add("xmlns", "http://www.w3.org/2000/svg");

    var dataType = element.GetAttributeValue("data-type", -1);
    if (dataType == -1)
        throw new InvalidOperationException();

    // These don't seem to apply to the resources we're writing here.
    if (dataType == 1)
        continue;

    if (seenIconTypes.Add(dataType))
        icons.Add(new AssetIcon(dataType, element.OuterHtml));
}

icons.Sort((a, b) => a.Type.CompareTo(b.Type));

File.WriteAllText(
    "..\\..\\..\\..\\Launcher.Plugins.Azure\\Categories\\Resources.json",
    JsonConvert.SerializeObject(
        new AssetCollection(assetTypes, icons),
        new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        }
    )
);

AssetIcon? CreateIcon(JObject? element)
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
