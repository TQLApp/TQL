using CommandLine;
using Tql.Localization;

var exitCode = -1;

Parser.Default.ParseArguments<Options>(args).WithParsed(p => exitCode = new Runner(p).Run());

return exitCode;
