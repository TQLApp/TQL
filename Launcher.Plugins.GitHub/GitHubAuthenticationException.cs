namespace Launcher.Plugins.GitHub;

internal class GitHubAuthenticationException : Exception
{
    public GitHubAuthenticationException() { }

    public GitHubAuthenticationException(string message)
        : base(message) { }

    public GitHubAuthenticationException(string message, Exception innerException)
        : base(message, innerException) { }
}
