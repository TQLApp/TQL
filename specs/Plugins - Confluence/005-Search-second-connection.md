---
testspace:
title: Plugins / Confluence / 005 - Search second connection
description: Verify functionality with a second connection.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/Confluence.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Verify history of the connections

| Action                                                                    | Expected result                                                                             |
| ------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------- |
| Open the app.                                                             |                                                                                             |
| **Enter** the **Confluence Search ({{ confluence_name }})** category.     | The **Pieter van Ginkel** and **Pieter van Ginkel › Overview** items appear in the history. |
| **Enter** the **Confluence Search ({{ confluence_alt_name }})** category. | No history appears.                                                                         |

## Delete connection

| Action                                                                       | Expected result                                   |
| ---------------------------------------------------------------------------- | ------------------------------------------------- |
| Open the **Configuration** window.                                           |                                                   |
| Navigate to **Confluence**.                                                  | The **Confluence > General** page is highlighted. |
| Highlight the **{{ confluence_alt_name }}** connection and click **Delete**. | The connection is deleted.                        |
| Click **Save**.                                                              |                                                   |

## Verify single connection

| Action                | Expected result                            |
| --------------------- | ------------------------------------------ |
| Open the app.         |                                            |
| Type in `confluence`. | Only a **Confluence Search** item appears. |
