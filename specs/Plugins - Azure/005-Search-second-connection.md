---
testspace:
title: Plugins - Azure / 005 - Search second connection
description: Verify functionality with a second connection.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/Azure.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Verify history of the connections

| Action                                                          | Expected result                                                     |
| --------------------------------------------------------------- | ------------------------------------------------------------------- |
| Open the app.                                                   |                                                                     |
| **Enter** the **Azure Portal ({{ azure_name }})** category.     | The **tql-rg/tql-ai** item and **tql-rg** item show in the history. |
| **Enter** the **Azure Portal ({{ azure_alt_name }})** category. | No history must show.                                               |

## Delete connection

| Action                                                                  | Expected result                                     |
| ----------------------------------------------------------------------- | --------------------------------------------------- |
| Open the **Configuration** window.                                      |                                                     |
| Navigate to **Azure Portal**.                                           | The **Azure Portal > General** page is highlighted. |
| Highlight the **{{ azure_alt_name }}** connection and click **Delete**. | The connection is be deleted.                       |
| Click **Save**.                                                         |                                                     |

## Verify single connection

| Action           | Expected result                         |
| ---------------- | --------------------------------------- |
| Open the app.    |                                         |
| Type in `azure`. | Only a **Azure Portal** item must show. |
