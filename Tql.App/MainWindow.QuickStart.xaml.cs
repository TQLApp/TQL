using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Tql.App.QuickStart;

namespace Tql.App;

internal partial class MainWindow
{
    private void HandleQuickStart()
    {
        if (_quickStart.State.IsDismissed)
            return;

        if (!_quickStart.State.WelcomeShown)
            QuickStartWelcome();
        else if (!_quickStart.State.SelectedTool.HasValue)
            QuickStartSelectTool();
    }

    private void QuickStartWelcome()
    {
        _quickStart.Show(
            this,
            QuickStartPopup
                .CreateBuilder()
                .WithTitle("Welcome to Techie's Quick Launcher")
                .WithText(
                    """
                    👋 Hi there! I'm here to help you get setup Techie's Quick Launcher.
                    I'll walk you through the basic functionality of the application to
                    get you up and running in no time.

                    Click OK to get going.
                    """
                )
                .WithButton(
                    "OK",
                    () =>
                    {
                        _quickStart.State = _quickStart.State with { WelcomeShown = true };
                        HandleQuickStart();
                    }
                )
                .Build(),
            QuickStartPopupMode.Modal
        );
    }

    private void QuickStartSelectTool()
    {
        _quickStart.Show(
            this,
            QuickStartPopup
                .CreateBuilder()
                .WithTitle("What tool do you use primarily?")
                .WithText(
                    """
                    Right now, Techie's Quick Launcher doesn't do much. First thing
                    we have to do is setup a tool. I can set you up with JIRA,
                    Azure DevOps or GitHub. If you don't use any of these, we
                    can also have a look at setting up Outlook!

                    For now, pick the tool you use most. We can setup more tools
                    later on.
                    """
                )
                .WithChoiceButton("JIRA", () => SelectTool(QuickStartTool.JIRA))
                .WithChoiceButton("Azure DevOps", () => SelectTool(QuickStartTool.AzureDevOps))
                .WithChoiceButton("GitHub", () => SelectTool(QuickStartTool.GitHub))
                .WithChoiceButton("Outlook", () => SelectTool(QuickStartTool.Outlook))
                .Build(),
            QuickStartPopupMode.Modal
        );

        void SelectTool(QuickStartTool tool)
        {
            _quickStart.State = _quickStart.State with { SelectedTool = tool };
            HandleQuickStart();
        }
    }
}
