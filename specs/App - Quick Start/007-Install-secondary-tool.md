---
testspace:
title: App - Quick Start / 007 - Install secondary tool
description:
  Validate that you can restart the tutorial to install a secondary tool.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/JIRA.md %}
{% include Configuration/Plugins/Azure-DevOps.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Clean-Setup.md %}

## Setup the quick start tutorial

{% include Setup/Local-package-source.md %}

{% include Setup/App/QuickStart/Reset-quick-start.md %}

| Action                                                                                                                                     | Expected result                                                          |
| ------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------ |
| Open the app.                                                                                                                              | The quick start window opens.                                            |
| Complete the quick start tutorial, picking JIRA and using the settings below.                                                              |                                                                          |
| Progress the tutorial until you get to a step called **Using Techie's Quick Launcher**.                                                    | This step shows immediately after you've saved the configuration window. |
| Click **Next**.                                                                                                                            |                                                                          |
| Progress through the tutorial phase that explains the application until you get to the completion step with the **Close Tutorial** button. |                                                                          |

- Connection settings:
  - Name: `{{ jira_name }}`
  - URL: `{{ jira_url }}`
  - User name: `{{ jira_user_name }}`
  - Password: `{{ jira_password }}`

## Configure secondary tool

{% include Setup/Local-package-source.md %}

Validate that you can restart the tutorial, configuring a secondary tool:

| Action                                                                                        | Expected result                                                  |
| --------------------------------------------------------------------------------------------- | ---------------------------------------------------------------- |
| Pick the **Azure DevOps** tool.                                                               | The tutorial proceeds to help you setup with Azure DevOps.       |
| Progress the tutorial until you've installed the **Azure DevOps** plugin and restart the app. | The app restarts.                                                |
| Progress the tutorial to setup a connection.                                                  | Use the settings below to setup the connection.                  |
| Progress the tutorial until you get to completion step with the **Close Tutorial** button.    | The step called **Using Techie's Quick Launcher** does not show. |

- Connection settings:
  - Name: `{{ azure_devops_name }}`
  - URL: `{{ azure_devops_url }}`
  - PAT token: `{{ azure_devops_pat_token }}`
