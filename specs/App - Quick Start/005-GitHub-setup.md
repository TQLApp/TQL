---
testspace:
title: App / Quick Start / 005 - GitHub setup
description: Validate that the quick start tutorial will set you up with GitHub.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/GitHub.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Clean-Setup.md %}

## Restart quick start

{% include Setup/App/QuickStart/Reset-quick-start.md %}

## Successfully setup GitHub

| Action                                                                                       | Expected result                                                          |
| -------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------ |
| Open the app.                                                                                | The quick start window must open.                                        |
| Progress the tutorial until you get to pick the tool.                                        |                                                                          |
| Pick the **GitHub** tool.                                                                    | The tutorial proceeds to help you setup with GitHub.                     |
| Progress the tutorial until you've installed the **GitHub** plugin and restart the app.      | The app restarts.                                                        |
| Progress the tutorial to setup a connection. Use the settings below to setup the connection. |                                                                          |
| Progress the tutorial until you get to a step called **Using Techie's Quick Launcher**.      | This step shows immediately after you've saved the configuration window. |
| **Dismiss** the quick start tutorial.                                                        |                                                                          |
| Search for `github repository`.                                                              | A category with the name **GitHub Repository** shows.                    |

- Connection settings:
  - Name: `{{ github_name }}`
