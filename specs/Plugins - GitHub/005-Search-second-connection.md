---
testspace:
title: Plugins - GitHub / 005 - Search second connection
description: Verify functionality with a second connection.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/GitHub.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Verify history of the connections

| Action                                                                   | Expected result                                          |
| ------------------------------------------------------------------------ | -------------------------------------------------------- |
| Open the app.                                                            |                                                          |
| **Enter** the **My GitHub Repository ({{ github_name }})** category.     | The previously selected repository shows in the history. |
| **Enter** the **My GitHub Repository ({{ github_alt_name }})** category. | No history shows.                                        |

## Delete connection

| Action                                                                   | Expected result                               |
| ------------------------------------------------------------------------ | --------------------------------------------- |
| Open the **Configuration** window.                                       |                                               |
| Navigate to **GitHub**.                                                  | The **GitHub > General** page is highlighted. |
| Highlight the **{{ github_alt_name }}** connection and click **Delete**. | The connection is deleted.                    |
| Click **Save**.                                                          |                                               |

## Verify single connection

| Action                          | Expected result                                                                                                                                                  |
| ------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Open the app.                   |                                                                                                                                                                  |
| Type in `my github repository`. | Only a **My GitHub Repository** item must show instead of the **My GitHub Repository ({{ github_name }})** and **My GitHub Repository ({{ github_alt_name }})**. |
