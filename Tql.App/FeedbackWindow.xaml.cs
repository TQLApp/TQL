﻿using Tql.Abstractions;
using Tql.App.Services;

namespace Tql.App;

internal partial class FeedbackWindow
{
    private readonly IUI _ui;

    public FeedbackWindow(IUI ui, IPluginManager pluginManager)
    {
        _ui = ui;

        InitializeComponent();

        _icon.Source = Images.Astronaut;
        _copyImage.Source = Images.Copy;

        var sb = new StringBuilder();

        sb.AppendLine($"Version: {GetType().Assembly.GetName().Version}");
        sb.AppendLine($"OS Version: {Environment.OSVersion.Version}");

        sb.AppendLine();
        sb.AppendLine("Installed plugins:");
        sb.AppendLine();

        foreach (var plugin in pluginManager.Plugins)
        {
            var assemblyName = plugin.GetType().Assembly.GetName();

            sb.AppendLine($"- {assemblyName.Name} ({assemblyName.Version}): {plugin.Id}");
        }

        _systemInformation.Text = sb.ToString().TrimEnd();
    }

    private void _bugReport_Click(object sender, RoutedEventArgs e)
    {
        _ui.OpenUrl(Constants.BugReportUrl);
    }

    private void _featureRequest_Click(object sender, RoutedEventArgs e)
    {
        _ui.OpenUrl(Constants.FeatureRequestUrl);
    }

    private void _copySystemInformation_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(_systemInformation.Text);
    }
}