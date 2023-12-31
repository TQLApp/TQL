﻿using Tql.Utilities;

namespace Tql.Plugins.GitHub;

internal static class Images
{
    private static ImageSource LoadImage(string name) =>
        ImageFactory.FromEmbeddedResource(typeof(Images), $"Resources.{name}");

    public static readonly ImageSource GitHub = LoadImage("GitHub.svg");
    public static readonly ImageSource Copy = LoadImage("Copy.svg");
    public static readonly ImageSource OpenIssue = LoadImage("Open Issue.svg");
    public static readonly ImageSource ClosedIssue = LoadImage("Closed Issue.svg");
    public static readonly ImageSource Repository = LoadImage("Repository.svg");
    public static readonly ImageSource Issue = LoadImage("Issue.svg");
    public static readonly ImageSource PullRequest = LoadImage("Pull Request.svg");
    public static readonly ImageSource User = LoadImage("User.svg");
    public static readonly ImageSource Gist = LoadImage("Gist.svg");
    public static readonly ImageSource New = LoadImage("New.svg");
    public static readonly ImageSource Organization = LoadImage("Organization.svg");
    public static readonly ImageSource Codespace = LoadImage("Codespace.svg");
    public static readonly ImageSource ImportRepository = LoadImage("Import Repository.svg");
    public static readonly ImageSource Project = LoadImage("Project.svg");
    public static readonly ImageSource OpenPullRequest = LoadImage("Open Pull Request.svg");
    public static readonly ImageSource ClosedPullRequest = LoadImage("Closed Pull Request.svg");
    public static readonly ImageSource MergedPullRequest = LoadImage("Merged Pull Request.svg");
    public static readonly ImageSource DraftIssue = LoadImage("Draft Issue.svg");
    public static readonly ImageSource Milestone = LoadImage("Milestone.svg");
    public static readonly ImageSource QueuedWorkflow = LoadImage("Queued Workflow.svg");
    public static readonly ImageSource InProgressWorkflow = LoadImage("In Progress Workflow.svg");
    public static readonly ImageSource SucceededWorkflow = LoadImage("Succeeded Workflow.svg");
    public static readonly ImageSource FailedWorkflow = LoadImage("Failed Workflow.svg");
    public static readonly ImageSource CancelledWorkflow = LoadImage("Cancelled Workflow.svg");
    public static readonly ImageSource Workflow = LoadImage("Workflow.svg");
}
