---
testspace:
title: Plugins - YouTrack / 005 - Search second connection
description: Verify functionality with a second connection.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/YouTrack.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Verify history of the connections

| Action                                                                  | Expected result                                       |
| ----------------------------------------------------------------------- | ----------------------------------------------------- |
| Open the app.                                                           |                                                       |
| **Enter** the **YouTrack Projects ({{ youtrack_name }})** category.     | The previously selected project shows in the history. |
| **Enter** the **YouTrack Projects ({{ youtrack_alt_name }})** category. | No history shows.                                     |

## Search in the second connection

| Action                                                                | Expected result                           |
| --------------------------------------------------------------------- | ----------------------------------------- |
| Open the app.                                                         |                                           |
| **Enter** the **YouTrack Issues ({{ youtrack_alt_name }})** category. | No issues show.                           |
| **Type** a search term.                                               | All issues matching the search term show. |
| **Activate** an issue.                                                | The issue opens.                          |

## Delete connection

| Action                                                                     | Expected result                                 |
| -------------------------------------------------------------------------- | ----------------------------------------------- |
| Open the **Configuration** window.                                         |                                                 |
| Navigate to **YouTrack**.                                                  | The **YouTrack > General** page is highlighted. |
| Highlight the **{{ youtrack_alt_name }}** connection and click **Delete**. | The connection is deleted.                      |
| Click **Save**.                                                            |                                                 |

## Verify single connection

| Action                       | Expected result                                                                                                                                         |
| ---------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Open the app.                |                                                                                                                                                         |
| Type in `youtrack projects`. | Only a **YouTrack Projects** item shows instead of the **YouTrack Projects ({{ youtrack_name }})** and **YouTrack Projects ({{ youtrack_alt_name }})**. |
