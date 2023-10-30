using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Tql.Plugins.Demo.Services;

internal record PersonNamesDto(
    [property: JsonPropertyName("surnames_list")] ImmutableArray<string> Surnames,
    [property: JsonPropertyName("male_names_list")] ImmutableArray<string> MaleNames,
    [property: JsonPropertyName("female_names_list")] ImmutableArray<string> FemaleNames
);
