# Activating matches

When the user activates a match, the `Run` method on the runnable match class is
called. We haven't yet implemented this so we'll do this now.

Update the `Run` method on the `PackageMatch` class to the following:

```cs
public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
{
    var url = $"https://www.nuget.org/packages/{Uri.EscapeUriString(_dto.PackageId)}";

    serviceProvider.GetRequiredService<IUI>().OpenUrl(url);

    return Task.CompletedTask;
}
```

If you just want to open a URL, the above implementation is the way to go. Note
that we're resolving the `IUI` service on the fly here instead of having it
injected. This is not required but, it does keep memory usage of the app down a
bit.

Note that this method also takes an `owner` parameter. The purpose of this is to
allow you to built your own UI. There's no requirement that matches only open
URLs. If you want to present the user with a screen, you should use the `owner`
parameter to correctly parent the screen.

Now, if you search again you'll be able to activate the match and the NuGet page
should open for you.

Next we'll look at serialization.
