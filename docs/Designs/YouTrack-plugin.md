# YouTrack plugin: design

Status: built, 2026-11-21. The Windows test run (`specs/Plugins - YouTrack/`) is
still to do.

## Decisions

- A new plugin, `Tql.Plugins.YouTrack` (NuGet package
  `TQLApp.Plugins.YouTrack`), built the way the JIRA plugin is: connections in
  the plugin configuration, categories cached per connection, searchable
  matches, and a people directory for the Microsoft Teams plugin.
- Self-hosted YouTrack is the primary target, YouTrack Cloud works the same way.
  The only thing the plugin assumes about an installation is that its REST API
  lives at `<service URL>/api`, which is how JetBrains documents both. Developed
  against https://issues.webathome.org (YouTrack 2026.2, build 18991).
- Authentication is a permanent token, nothing else. Tokens work on every
  installation without the administrator registering anything.
- Version 1 has JIRA parity: issues, projects, agile boards, saved searches,
  dashboards, new issue per project, and the people directory.
- The code that needs no WPF (the REST client, its DTOs, URL and query building)
  lives in files without WPF or TQL dependencies. A plain `net8.0` test project
  compiles those files, so the tests run on Linux in KubeCoder as
  `kc project test`.

## Self-hosted installations

### The service URL

The user enters the address they open YouTrack on. JetBrains calls it the
service URL, and the REST API is `<service URL>/api` in every case:

| Installation               | Service URL                                |
| -------------------------- | ------------------------------------------ |
| Self-hosted, at the root   | `https://issues.webathome.org`             |
| Self-hosted, under a path  | `https://www.example.com/youtrack`         |
| Cloud                      | `https://example.youtrack.cloud`           |
| Cloud, on the older domain | `https://example.myjetbrains.com/youtrack` |

The plugin keeps the URL with one trailing slash and builds every API call and
every link from it, so a path prefix needs no special handling.

### A URL copied from the address bar

A self-hosted YouTrack answers unknown paths with its web app. Measured on
issues.webathome.org: `GET /issues/api/users/me` returns HTTP 200 with
`text/html`, so a service URL copied from a page such as
`https://issues.webathome.org/issues` fails as a JSON parse error, not as a 404.

Saving a connection therefore checks it in two steps:

1. `GET <service URL>/api/config?fields=version` must answer JSON with `$type`
   `FrontendConfig`. No token is needed for this call. If it answers HTML, the
   plugin tries the URL with its last path segment removed, down to the bare
   host. When one of those answers, the dialog offers it: "Did you mean
   https://issues.webathome.org/?".
2. `GET <service URL>/api/users/me` with the token must answer 200. A 401 means
   the token is wrong; the dialog says so rather than showing an exception.
   Measured: an invalid token and no token both get 401.

### URLs the API returns

The API returns some URLs relative to the host. User avatars come back as
`/hub/api/rest/avatar/...`, because issues.webathome.org runs the built-in Hub
(`/api/config` reports `ring.url` as `/hub`). Project icons are often `null`.
The plugin resolves every such URL against the service URL by the normal URI
rules, so a host-relative path lands on the right host with or without a path
prefix. A project without an icon gets the category icon.

The token goes only to the service URL's own scheme, host and port. An avatar or
icon on another host, such as Gravatar or an external Hub, is fetched without
it.

### Certificates, Hub and guest access

- TLS uses the Windows certificate store, through `HttpClient`. An installation
  behind an internal CA needs that CA trusted by Windows. There is no switch to
  ignore certificate errors. (issues.webathome.org has a Let's Encrypt
  certificate.)
- With an external Hub, a token is created in the Hub profile instead of the
  YouTrack one. Version 1 never calls Hub, so nothing else changes.
- Some self-hosted servers allow guest access. `/api/users/me` still answers 401
  without a valid token, which is why the connection check uses it.

### Versions and undocumented endpoints

JetBrains documents no minimum version for the `/api` REST API, so the plugin
targets current releases. Three things it uses work on 2026.2 but are not in the
REST API reference:

- `GET /api/dashboards`;
- the `query` parameter on `GET /api/users`;
- the `/newIssue?project=<short name>` link.

Each one degrades on its own. If an endpoint answers 403 or 404, its category is
left out for that connection and the error is logged. A failing endpoint never
fails the rest of the connection's cache.

### Multiple connections

Several connections can be configured, for example a self-hosted instance and a
Cloud one. As in the JIRA plugin, each category label gets the connection name
once there is more than one.

## Features

Paged lists use `$skip` and `$top`; every call passes `$top`, because the
default page size is not documented (measured: `/api/issues` returned 500 items
without it).

- **YouTrack Issues**: searched live with
  `GET /api/issues?query=<text>&$top=100`. The text goes to YouTrack as a query,
  so `#Unresolved for: me` works. An issue ID, in any case, is fetched directly
  first. Opens `<url>/issue/KC-230`.
- **YouTrack Projects**: cached from `GET /api/admin/projects`, archived
  projects left out. A project searches its own issues with
  `project: KC <text>`, or `project: KC sort by: updated desc` without text.
  Opens `<url>/projects/KC`.
- **YouTrack Agile Boards**: cached from `GET /api/agiles`, not searchable in
  version 1. Opens `<url>/agiles/<id>/current`.
- **YouTrack Saved Searches**: cached from `GET /api/savedQueries`. A saved
  search narrows with the text: `(<saved query>) and (<text>)`. Opens
  `<url>/issues?q=<query>`.
- **YouTrack Dashboards**: cached from `GET /api/dashboards`, not searchable.
  Opens `<url>/dashboard?id=<id>`.
- **New YouTrack Issue**: one match per cached project. Opens
  `<url>/newIssue?project=KC`.

Measured on issues.webathome.org: YouTrack matches word prefixes without a
wildcard (`kubecod` finds "KubeCoder"), so the plugin never appends `*`. Doing
so could also break a query the user typed. `GET /api/issues/<id>` answers 404
for an issue that doesn't exist, and the search then runs as usual. A half-typed
query such as `#Unres` or `state: Op` answers 400 with `invalid_query`; the
plugin shows no results for it rather than an error.

The people directory searches `GET /api/users?query=<text>&$top=100`. It leaves
out users without an email address, banned users and guests, and hands the rest
to the Microsoft Teams plugin. A token that may not list users gets 403, which
yields an empty result and a log line.

Board quick filters have no YouTrack counterpart. Searching a board's sprints is
a possible later addition.

## Structure

The project follows `Tql.Plugins.Jira` file by file:

- `YouTrackPlugin.cs`, `Configuration.cs`, `Images.cs`, `GlobalUsings.cs`.
- `Categories/`: a type and a match class per category, `TypeIds.cs` with new
  GUIDs, `RootItemDto.cs`.
- `Data/`: `YouTrackData` and `YouTrackCacheManager`, which loads projects,
  boards, saved searches and dashboards per connection.
- `Services/`: `ConfigurationManager`, `YouTrackApi` (creates a client per
  connection and shows the "unable to connect" notification bar on a 401, as the
  JIRA plugin does), `IconCacheManager` and `YouTrackPeopleDirectory`.
- `ConfigurationUI/`: the connections list and the connection dialog.
- `Labels.resx`, `Labels.nl.resx` and `Labels.Designer.cs`.
- `Resources/`: Fluent UI System Icons (MIT, already credited in the README),
  recoloured to `#B6C2CF` like the JIRA category icons, and the YouTrack product
  icon from JetBrains' brand resources.

These four files have no WPF or TQL dependencies, and the test project includes
them as linked `Compile` items:

- `Services/YouTrackClient.cs`
- `Services/YouTrackDtos.cs`
- `Support/YouTrackUrls.cs`
- `Support/YouTrackQueries.cs`

A connection is
`Connection(Guid Id, string Name, string Url, string ProtectedToken)`, the token
encrypted with `IEncryption` as the JIRA plugin encrypts its password. The
connection dialog asks for:

- **Name**.
- **URL**, with the help text "The address you open YouTrack on, e.g.
  https://youtrack.example.com or https://example.youtrack.cloud."
- **Token**, a password box. Its help text says where a token is made: Profile,
  Account Security, New token, with the YouTrack scope.

Wiring it in touches `Tql.sln` (the Plugins folder), `Tql.DebugApp` (a project
reference and an entry in `DebugAssemblies`) and a new `packages.lock.json`.
Every file is CRLF, like the rest of the tree. Visual Studio's resource
generator doesn't run on Linux, so a `Labels.Designer.cs` written outside Visual
Studio is written by hand to match; Visual Studio regenerates it on the next
`.resx` edit.

## Testing

- **In KubeCoder.** `tests/Tql.Plugins.YouTrack.Test` targets `net8.0`, with
  NUnit and `NUnit3TestAdapter` (the existing test projects have no adapter, so
  `dotnet test` finds nothing in them). A fake `HttpMessageHandler` replays JSON
  captured from issues.webathome.org. The tests cover request URLs, fields and
  paging, the Bearer header, resolving relative URLs, the token staying off
  other hosts, a 404 becoming "no issue", an HTML answer becoming a clear error,
  and query building. `kc project test` runs them.
- **Against a real instance.** A second fixture runs the same client against a
  live YouTrack, read-only, when `YOUTRACK_TEST_URL` and `YOUTRACK_TEST_TOKEN`
  are set, and is ignored otherwise. This environment has a token for
  issues.webathome.org in `YOUTRACK_API_KEY_SECRET`.
- **On Windows.** `kc project build` publishes `build/DebugApp`. Windows won't
  run it from the share, so run
  `\\tqlapp-tql-49385e.home\work\TQL\scripts\Run-KubeCoder-Build.bat`, which
  mirrors it into `%TEMP%\TQL-DebugApp` and starts
  `Tql.DebugApp.exe --env Debug` there. A manual test script in
  `specs/Plugins - YouTrack/`, modelled on the JIRA one, covers installing,
  configuring (a URL copied from the address bar included), searching every
  category, and a second connection. The configuration page and the connection
  dialog can only be tested this way.

## Open points

- **Undocumented behaviour to confirm on the desktop:** the project page at
  `<url>/projects/KC`, `<url>/agiles/<id>/current` for a board without sprints,
  `<url>/newIssue?project=KC`, and the dashboard link.
- **Non-admin tokens.** Every measurement used the operator's administrator
  token. Whether a plain user's token may call `/api/admin/projects` and
  `/api/users` is untested; both are coded to degrade.
- **Documentation.** The configuration's documentation link points at a
  `YouTrack-plugin` page on the GitHub wiki, which lives outside this
  repository.
- **Not in version 1:** the quick start tutorial, sprints, Knowledge Base
  articles, and OAuth through Hub. OAuth would need each administrator to
  register TQL as a Hub service, which is a poor fit for self-hosted
  installations.
