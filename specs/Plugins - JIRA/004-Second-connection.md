---
testspace:
title: Plugins - JIRA / 004 - Second connection
description: Configure a second connection.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/JIRA.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Create connection

| Action                                            | Expected result                             |
| ------------------------------------------------- | ------------------------------------------- |
| Open the **Configuration** window.                |                                             |
| Navigate to **JIRA**.                             | The **JIRA > General** page is highlighted. |
| **Add** a new connection with the settings below. |                                             |
| Click **Save**.                                   |                                             |
| Click **Save**.                                   |                                             |

- Connection settings:
  - Name: `{{ jira_alt_name }}`
  - URL: `{{ jira_url }}`
  - User name: `{{ jira_user_name }}`
  - Password: `{{ jira_password }}`

## Verify multiple connections

| Action                | Expected result                                                                                         |
| --------------------- | ------------------------------------------------------------------------------------------------------- |
| Open the app.         |                                                                                                         |
| Type in `jira board`. | One **JIRA Board ({{ jira_name }})** item and one **JIRA Board ({{ jira_alt_name }})** entry must item. |
