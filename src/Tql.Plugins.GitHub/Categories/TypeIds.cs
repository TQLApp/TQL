using Tql.Abstractions;

namespace Tql.Plugins.GitHub.Categories;

internal static class TypeIds
{
    private static MatchTypeId CreateId(string id) => new(Guid.Parse(id), GitHubPlugin.Id);

    public static readonly MatchTypeId Repositories = CreateId(
        "d4d42f26-f777-46c5-895b-b18287fd6fb9"
    );
    public static readonly MatchTypeId Repository = CreateId(
        "2214bb85-7004-49c1-a8e3-61b0c8a68e4b"
    );

    public static readonly MatchTypeId Issues = CreateId("7ff14022-2f3d-486e-9ccd-77051c47e3db");
    public static readonly MatchTypeId Issue = CreateId("b1f1dca8-8973-4cac-9cd5-d0e5b27b334a");

    public static readonly MatchTypeId PullRequests = CreateId(
        "b2e9f741-9336-44ab-b740-b386e23ad702"
    );
    public static readonly MatchTypeId PullRequest = CreateId(
        "211d36ac-c317-42d8-a6c7-d14f65e4ad91"
    );

    public static readonly MatchTypeId Users = CreateId("f6714e76-8784-429e-a67d-e332e7b55b97");
    public static readonly MatchTypeId User = CreateId("10f03c08-66f5-49c1-909f-277925a5888e");

    public static readonly MatchTypeId Gists = CreateId("660f3739-c1fb-4e59-af2d-608b743dcc7c");
    public static readonly MatchTypeId Gist = CreateId("948f5282-6bb8-4122-8c5f-ff2a1929d2c1");

    public static readonly MatchTypeId News = CreateId("d5561f08-6bbf-4133-8ab4-d55cd1c97d57");
    public static readonly MatchTypeId New = CreateId("0141215a-4947-4d12-9e45-ba929314178d");

    public static readonly MatchTypeId NewPullRequests = CreateId(
        "c2d7c46f-d37c-4b34-85f3-509584998e90"
    );
    public static readonly MatchTypeId NewPullRequest = CreateId(
        "919c0586-f40d-45c6-8789-5237fd36ee83"
    );

    public static readonly MatchTypeId NewIssue = CreateId("44c8a7d7-657c-4064-9530-3ee2457e43bd");

    public static readonly MatchTypeId Projects = CreateId("0c3b2a6c-123b-49c9-a3dd-cee663deda7c");
    public static readonly MatchTypeId Project = CreateId("b3e3f65c-b264-438b-a6d7-49b460acb1af");
    public static readonly MatchTypeId ProjectItem = CreateId(
        "abcade5d-3c5a-4b87-8219-46c3568d30d5"
    );

    public static readonly MatchTypeId Milestones = CreateId(
        "60b8e412-15ac-4ceb-a484-b8856008b4a8"
    );
    public static readonly MatchTypeId Milestone = CreateId("c3228fb6-f28d-4a36-87f8-f83cbb0bbab6");

    public static readonly MatchTypeId WorkflowRuns = CreateId(
        "dacd101b-1ec1-4394-a9e3-748b414e52d4"
    );
    public static readonly MatchTypeId WorkflowRun = CreateId(
        "f089c85a-57bc-4d87-90e4-bfc9b7857bf1"
    );
}
