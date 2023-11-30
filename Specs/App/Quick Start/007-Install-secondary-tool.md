---
testspace:
title: App / Quick Start / 007 - Install secondary tool
description:
  Validate that you can restart the tutorial to install a secondary tool.
---

{% include Configuration/Plugins/JIRA.md %}
{% include Configuration/Plugins/Azure-DevOps.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Clean-Setup.md %}

## Setup the quick start tutorial

{% if page %}{% assign spec = page %}{% endif %}
{% include Setup/App/QuickStart/Reset-quick-start.md %}

Configure JIRA:

- Open the app. The quick start window must open.
- Complete the quick start tutorial, picking JIRA and using the following
  configuration:
  - Name: `{{ jira_name }}`
  - URL: `{{ jira_url }}`
  - User name: `{{ jira_user_name }}`
  - Password: `{{ jira_password }}`
- Progress the tutorial until you get to a step called **Using Techie's Quick
  Launcher**. This step shows immediately after you've saved the configuration
  window. Click **Next**.
- Progress through the tutorial phase that explains the application until you
  get to the completion step with the **Close Tutorial** button.

## Configure secondary tool

Validate that you can restart the tutorial, configuring a secondary tool:

- Pick the **Azure DevOps** tool. The tutorial proceeds to help you setup with
  Azure DevOps.
- Progress the tutorial until you've installed the **Azure DevOps** plugin and
  restart the app. The app restarts.
- Progress the tutorial to setup a connection. Uset the following values to set
  up the connection:
  - Name: `{{ azure_devops_name }}`
  - URL: `{{ azure_devops_url }}`
  - PAT token: `{{ azure_devops_pat_token }}`
- Progress the tutorial until you get to completion step with the **Close
  Tutorial** button. The step called **Using Techie's Quick Launcher** does not
  show.
