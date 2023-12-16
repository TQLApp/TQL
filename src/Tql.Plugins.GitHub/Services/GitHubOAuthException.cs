namespace Tql.Plugins.GitHub.Services;

internal class GitHubOAuthException : Exception
{
    public GitHubOAuthException() { }

    public GitHubOAuthException(string? message)
        : base(message) { }

    public GitHubOAuthException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
