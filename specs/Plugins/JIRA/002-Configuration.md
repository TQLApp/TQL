---
testspace:
title: Plugins / JIRA / 002 - Configuration
description: Configure the JIRA plugin.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/JIRA.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Configuration

Create a new connection:

- Open the **Configuration** window.
- Navigate to **JIRA**. The **JIRA | General** page is highlighted.
- **Add** a new connection with the following information:
  - Name: `{{ jira_name }}`
  - URL: `{{ jira_url }}`
  - User name: `{{ jira_user_name }}`
  - Password: `{{ jira_password }}`
- Click **Save**.
- Click **Save**.

## Validate configuration

Validate that the connection has been saved:

- Open the **Configuration** window.
- Navigate to **JIRA**. The **JIRA | General** page is highlighted.
- Highlight the connection and click **Edit**.
- Validate that the the values are as follows:
  - Name: `{{ jira_name }}`
  - URL: `{{ jira_url }}`
  - User name: `{{ jira_user_name }}`
  - Password: `{{ jira_password }}`

## Validate authentication

Validate access to JIRA:

- Open the app.
- Type in `jira`. The **JIRA Board** category must appear.
- **Enter** the **JIRA Board** category. A list of boards should appear.
