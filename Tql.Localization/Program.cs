using CommandLine;
using Tql.Localization;

Parser.Default.ParseArguments<Options>(args).WithParsed(p => new Runner(p).Run());
