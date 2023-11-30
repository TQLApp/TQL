---
testspace:
title: Plugins / JIRA / 005 - Search second connection
description: Verify functionality with a second connection.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/JIRA.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Verify history of the connections

Verify that the history of the connections are isolated:

- Open the app.
- **Enter** the **JIRA Search ({{ jira_name }})** category. The **Pieter van
  Ginkel** and **Pieter van Ginkel › Overview** items appear in the history.
- **Enter** the **JIRA Search ({{ jira_alt_name }})** category. No history must
  appear.

## Delete connection

Verify that the connection can be deleted"

- Open the **Configuration** window.
- Navigate to **JIRA**. The **JIRA | General** page is highlighted.
- Highlight the **{{ jira_alt_name }}** connection and click **Delete**. The
  connection must be deleted.
- Click **Save**.

## Verify single connection

- Open the app.
- Type in `confluence`. Only a **JIRA Search** item must appear.
