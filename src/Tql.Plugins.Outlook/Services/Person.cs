using Tql.Abstractions;

namespace Tql.Plugins.Outlook.Services;

internal record Person(string DisplayName, string EmailAddress) : IPerson;
