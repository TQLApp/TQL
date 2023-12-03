---
testspace:
title: App / Quick Start / 003 - JIRA setup
description: Validate that the quick start tutorial will set you up with JIRA.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/JIRA.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Clean-Setup.md %}

## Restart quick start

{% include Setup/App/QuickStart/Reset-quick-start.md %}

## Successfully setup JIRA

| Action                                                                                        | Expected result                                                          |
| --------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------ |
| Open the app. The quick start window opens.                                                   |                                                                          |
| Progress the tutorial until you get to pick the tool.                                         |                                                                          |
| Pick the **JIRA** tool.                                                                       | The tutorial proceeds to help you setup with JIRA.                       |
| Progress the tutorial until you've installed the **JIRA** plugin and restart the app.         | The app restarts.                                                        |
| Progress the tutorial to setup a connection. Use the settings below to set up the connection. |                                                                          |
| Progress the tutorial until you get to a step called **Using Techie's Quick Launcher**.       | This step shows immediately after you've saved the configuration window. |
| **Dismiss** the quick start tutorial.                                                         |                                                                          |
| Search for `jira board`.                                                                      | A category with the name **JIRA Board** shows.                           |

- Connection settings:
  - Name: `{{ jira_name }}`
  - URL: `{{ jira_url }}`
  - User name: `{{ jira_user_name }}`
  - Password: `{{ jira_password }}`
