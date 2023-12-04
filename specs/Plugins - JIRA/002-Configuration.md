---
testspace:
title: Plugins - JIRA / 002 - Configuration
description: Configure the JIRA plugin.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/JIRA.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Configuration

| Action                                            | Expected result                             |
| ------------------------------------------------- | ------------------------------------------- |
| Open the **Configuration** window.                |                                             |
| Navigate to **JIRA**.                             | The **JIRA > General** page is highlighted. |
| **Add** a new connection with the settings below. |                                             |
| Click **Save**.                                   |                                             |
| Click **Save**.                                   |                                             |

- Connection settings:
  - Name: `{{ jira_name }}`
  - URL: `{{ jira_url }}`
  - User name: `{{ jira_user_name }}`
  - Password: `{{ jira_password }}`

## Validate configuration

| Action                                       | Expected result                             |
| -------------------------------------------- | ------------------------------------------- |
| Open the **Configuration** window.           |                                             |
| Navigate to **JIRA**.                        | The **JIRA > General** page is highlighted. |
| Highlight the connection and click **Edit**. | The settings conform to the settings below. |

- Connection settings:
  - Name: `{{ jira_name }}`
  - URL: `{{ jira_url }}`
  - User name: `{{ jira_user_name }}`
  - Password: `{{ jira_password }}`

## Validate authentication

| Action                                 | Expected result                      |
| -------------------------------------- | ------------------------------------ |
| Open the app.                          |                                      |
| Type in `jira`.                        | The **JIRA Board** category appears. |
| **Enter** the **JIRA Board** category. | A list of boards appears.            |
