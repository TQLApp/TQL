using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tql.UITests.Support;

internal static class ShellUtils
{
    public static string PrintArguments(params string[] args) =>
        PrintArguments((IEnumerable<string>)args);

    public static string PrintArguments(IEnumerable<string> args)
    {
        var sb = new StringBuilder();

        foreach (var arg in args)
        {
            if (sb.Length > 0)
                sb.Append(' ');
            sb.Append('"').Append(arg.Replace("\"", "\"\"")).Append('"');
        }

        return sb.ToString();
    }
}
