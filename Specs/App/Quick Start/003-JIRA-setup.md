---
testspace:
title: App / Quick Start / 003 - JIRA setup
description: Validate that the quick start tutorial will set you up with JIRA.
---

{% include Configuration/Plugins/JIRA.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Clean-Setup.md %}

## Restar quick start

{% include Setup/App/QuickStart/Reset-quick-start.md %}

## Successfully setup JIRA

Verify that the quick start tutorial will set you up with JIRA:

- Open the app. The quick start window must open.
- Progress the tutorial until you get to pick the tool.
- Pick the **JIRA** tool. The tutorial proceeds to help you setup with JIRA.
- Progress the tutorial until you've installed the **JIRA** plugin and restart the app. The app restarts.
- Progress the tutorial to setup a connection. Uset the following values to set up the connection:
  - Name: `{{ jira_name }}`
  - URL: `{{ jira_url }}`
  - User name: `{{ jira_user_name }}`
  - Password: `{{ jira_password }}`
- Progress the tutorial until you get to a step called **Using Techie's Quick Launcher**. This step shows immediately after you've saved the configuration window. **Dismiss** the quick start tutorial.
- Search for `JIRA Board`. A category with the name **JIRA Board** shows.
