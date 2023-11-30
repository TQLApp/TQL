---
testspace:
title: App / Quick Start / 004 - Azure DevOps setup
description:
  Validate that the quick start tutorial will set you up with Azure DevOps.
---

{% include Configuration/Plugins/Azure-DevOps.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Clean-Setup.md %}

## Restar quick start

{% include Setup/App/QuickStart/Reset-quick-start.md %}

## Successfully setup Azure DevOps

Verify that the quick start tutorial will set you up with Azure DevOps:

- Open the app. The quick start window must open.
- Progress the tutorial until you get to pick the tool.
- Pick the **Azure DevOps** tool. The tutorial proceeds to help you setup with
  Azure DevOps.
- Progress the tutorial until you've installed the **Azure DevOps** plugin and
  restart the app. The app restarts.
- Progress the tutorial to setup a connection. Uset the following values to set
  up the connection:
  - Name: `{{ azure_devops_name }}`
  - URL: `{{ azure_devops_url }}`
  - PAT token: `{{ azure_devops_pat_token }}`
- Progress the tutorial until you get to a step called **Using Techie's Quick
  Launcher**. This step shows immediately after you've saved the configuration
  window. **Dismiss** the quick start tutorial.
- Search for `Azure Board`. A category with the name **Azure Board** shows.
