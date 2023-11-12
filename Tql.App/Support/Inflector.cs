// ReSharper disable StringLiteralTypo

using System.Text.RegularExpressions;

namespace Tql.App.Support;

internal static class Inflector
{
    private static readonly List<(Regex Re, string Replacement)> PluralRules = new();
    private static readonly List<(Regex Re, string Replacement)> SingularRules = new();
    private static readonly List<string> Uncountables =
        new() { "equipment", "information", "rice", "money", "species", "series", "fish", "sheep" };

    static Inflector()
    {
        AddPlural("$", "s", true);
        AddPlural("s$", "s");
        AddPlural("(ax|test)is$", "$1es");
        AddPlural("(octop|vir)us$", "$1i");
        AddPlural("(alias|status)$", "$1es");
        AddPlural("(bu)s$", "$1ses");
        AddPlural("(buffal|tomat)o$", "$1oes");
        AddPlural("([ti])um$", "$1a");
        AddPlural("sis$", "ses");
        AddPlural("(?:([^f])fe|([lr])f)$", "$1$2ves");
        AddPlural("(hive)$", "$1s");
        AddPlural("([^aeiouy]|qu)y$", "$1ies");
        AddPlural("(x|ch|ss|sh)$", "$1es");
        AddPlural("(matr|vert|ind)(?:ix|ex)$", "$1ices");
        AddPlural("([m|l])ouse$", "$1ice");
        AddPlural("^(ox)$", "$1en");
        AddPlural("(quiz)$", "$1zes");

        AddSingular("s$", "");
        AddSingular("(n)ews$", "$1ews");
        AddSingular("([ti])a$", "$1um");
        AddSingular("((a)naly|(b)a|(d)iagno|(p)arenthe|(p)rogno|(s)ynop|(t)he)ses$", "$1$2sis");
        AddSingular("(^analy)ses$", "$1sis");
        AddSingular("([^f])ves$", "$1fe");
        AddSingular("(hive)s$", "$1");
        AddSingular("(tive)s$", "$1");
        AddSingular("([lr])ves$", "$1f");
        AddSingular("([^aeiouy]|qu)ies$", "$1y");
        AddSingular("(s)eries$", "$1eries");
        AddSingular("(m)ovies$", "$1ovie");
        AddSingular("(x|ch|ss|sh)es$", "$1");
        AddSingular("([m|l])ice$", "$1ouse");
        AddSingular("(bus)es$", "$1");
        AddSingular("(o)es$", "$1");
        AddSingular("(shoe)s$", "$1");
        AddSingular("(cris|ax|test)es$", "$1is");
        AddSingular("(octop|vir)i$", "$1us");
        AddSingular("(alias|status)es$", "$1");
        AddSingular("^(ox)en", "$1");
        AddSingular("(vert|ind)ices$", "$1ex");
        AddSingular("(matr)ices$", "$1ix");
        AddSingular("(quiz)zes$", "$1");

        AddIrregular("person", "people");
        AddIrregular("man", "men");
        AddIrregular("child", "children");
        AddIrregular("sex", "sexes");
        AddIrregular("move", "moves");
        AddIrregular("cow", "kine");
    }

    private static void AddIrregular(string singular, string plural)
    {
        AddPlural(
            singular.Substring(0, 1).ToLower() + singular.Substring(1) + "$",
            plural.Substring(0, 1).ToLower() + plural.Substring(1)
        );
        AddPlural(
            singular.Substring(0, 1).ToUpper() + singular.Substring(1) + "$",
            plural.Substring(0, 1).ToUpper() + plural.Substring(1)
        );
        AddSingular(
            plural.Substring(0, 1).ToLower() + plural.Substring(1) + "$",
            singular.Substring(0, 1).ToLower() + singular.Substring(1)
        );
        AddSingular(
            plural.Substring(0, 1).ToUpper() + plural.Substring(1) + "$",
            singular.Substring(0, 1).ToUpper() + singular.Substring(1)
        );
    }

    private static void AddPlural(string expression, string replacement, bool caseSensitive = false)
    {
        var re = caseSensitive
            ? new Regex(expression)
            : new Regex(expression, RegexOptions.IgnoreCase);

        PluralRules.Insert(0, (re, replacement));
    }

    private static void AddSingular(
        string expression,
        string replacement,
        bool caseSensitive = false
    )
    {
        var re = caseSensitive
            ? new Regex(expression)
            : new Regex(expression, RegexOptions.IgnoreCase);

        SingularRules.Insert(0, (re, replacement));
    }

    public static string Pluralize(string value)
    {
        if (Uncountables.Contains(value))
            return value;

        foreach (var rule in PluralRules)
        {
            if (rule.Re.IsMatch(value))
                return rule.Re.Replace(value, rule.Replacement);
        }

        return value;
    }

    public static string Singularize(string value)
    {
        if (Uncountables.Contains(value))
            return value;

        foreach (var rule in SingularRules)
        {
            if (rule.Re.IsMatch(value))
                return rule.Re.Replace(value, rule.Replacement);
        }

        return value;
    }
}
