---
testspace:
title: Plugins / JIRA / 004 - Second connection
description: Configure a second connection.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/JIRA.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Create connection

Create a new connection:

- Open the **Configuration** window.
- Navigate to **JIRA**. The **JIRA | General** page is highlighted.
- **Add** a new connection with the following information:
  - Name: `{{ jira_alt_name }}`
  - URL: `{{ jira_url }}`
  - User name: `{{ jira_user_name }}`
  - Password: `{{ jira_password }}`
- Click **Save**.
- Click **Save**.

## Verify multiple connections

Verify that there are multiple **JIRA** entries:

- Open the app.
- Type in `confluence`. One **JIRA Search ({{ jira_name }})** item and one
  **JIRA Search ({{ jira_alt_name }})** entry must item.
