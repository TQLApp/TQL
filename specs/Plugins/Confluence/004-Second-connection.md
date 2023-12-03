---
testspace:
title: Plugins / Confluence / 004 - Second connection
description: Configure a second connection.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/Confluence.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Create connection

| Action                                            | Expected result                                   |
| ------------------------------------------------- | ------------------------------------------------- |
| Open the **Configuration** window.                |                                                   |
| Navigate to **Confluence**.                       | The **Confluence > General** page is highlighted. |
| **Add** a new connection with the settings below. |                                                   |
| Click **Save**.                                   |                                                   |
| Click **Save**.                                   |                                                   |

- Connection settings:
  - Name: `{{ confluence_alt_name }}`
  - URL: `{{ confluence_url }}`
  - User name: `{{ confluence_user_name }}`
  - Password: `{{ confluence_password }}`

## Verify multiple connections

| Action                | Expected result                                                                                                                   |
| --------------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| Open the app.         |                                                                                                                                   |
| Type in `confluence`. | One **Confluence Search ({{ confluence_name }})** item and one **Confluence Search ({{ confluence_alt_name }})** entry must item. |
