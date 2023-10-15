namespace Tql.Setup.App.Support;

internal static class ExceptionUtils
{
    public static string PrintStackTrace(this Exception self)
    {
        var sb = new StringBuilder();

        PrintException(self);

        return sb.ToString();

        void PrintException(Exception exception)
        {
            if (exception.InnerException != null)
            {
                PrintException(exception.InnerException);

                sb.AppendLine();
                sb.AppendLine("=== Caused ===");
                sb.AppendLine();
            }

            sb.AppendLine($"{exception.Message} ({exception.GetType().FullName})");

            if (exception.StackTrace != null)
                sb.AppendLine(exception.StackTrace.TrimEnd());
        }
    }
}
