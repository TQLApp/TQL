---
testspace:
title: Plugins - JIRA / 005 - Search second connection
description: Verify functionality with a second connection.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/JIRA.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Verify history of the connections

| Action                                                       | Expected result                                     |
| ------------------------------------------------------------ | --------------------------------------------------- |
| Open the app.                                                |                                                     |
| **Enter** the **JIRA Board ({{ jira_name }})** category.     | The previously selected board shows in the history. |
| **Enter** the **JIRA Board ({{ jira_alt_name }})** category. | No history shows.                                   |

## Delete connection

| Action                                                                 | Expected result                             |
| ---------------------------------------------------------------------- | ------------------------------------------- |
| Open the **Configuration** window.                                     |                                             |
| Navigate to **JIRA**.                                                  | The **JIRA > General** page is highlighted. |
| Highlight the **{{ jira_alt_name }}** connection and click **Delete**. | The connection is deleted.                  |
| Click **Save**.                                                        |                                             |

## Verify single connection

| Action                | Expected result                                                                                                                |
| --------------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| Open the app.         |                                                                                                                                |
| Type in `jira board`. | Only a **JIRA Board** item must show instead of the **JIRA Board ({{ jira_name }})** and **JIRA Board ({{ jira_alt_name }})**. |
