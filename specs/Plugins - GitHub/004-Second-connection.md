---
testspace:
title: Plugins - GitHub / 004 - Second connection
description: Configure a second connection.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/GitHub.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Create connection

| Action                                            | Expected result                               |
| ------------------------------------------------- | --------------------------------------------- |
| Open the **Configuration** window.                |                                               |
| Navigate to **GitHub**.                           | The **GitHub > General** page is highlighted. |
| **Add** a new connection with the settings below. |                                               |
| Click **Save**.                                   |                                               |
| Click **Save**.                                   |                                               |

- Connection settings:
  - Name: `{{ github_alt_name }}`

## Verify multiple connections

| Action                          | Expected result                                                                                                                 |
| ------------------------------- | ------------------------------------------------------------------------------------------------------------------------------- |
| Open the app.                   |                                                                                                                                 |
| Type in `my github repository`. | One **My GitHub Repository ({{ github_name }})** item and one **My GitHub Repository ({{ github_alt_name }})** entry must item. |
