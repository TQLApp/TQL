---
testspace:
title: Plugins - Confluence / 002 - Configuration
description: Configure the Confluence plugin.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/Confluence.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Configuration

| Action                                            | Expected result                                   |
| ------------------------------------------------- | ------------------------------------------------- |
| Open the **Configuration** window.                |                                                   |
| Navigate to **Confluence**.                       | The **Confluence > General** page is highlighted. |
| **Add** a new connection with the settings below. |                                                   |
| Click **Save**.                                   |                                                   |
| Click **Save**.                                   |                                                   |

- Connection settings:
  - Name: `{{ confluence_name }}`
  - URL: `{{ confluence_url }}`
  - User name: `{{ confluence_user_name }}`
  - Password: `{{ confluence_password }}`

## Validate configuration

| Action                                       | Expected result                                   |
| -------------------------------------------- | ------------------------------------------------- |
| Open the **Configuration** window.           |                                                   |
| Navigate to **Confluence**.                  | The **Confluence > General** page is highlighted. |
| Highlight the connection and click **Edit**. | The settings conform to the settings below.       |

- Connection settings:
  - Name: `{{ confluence_name }}`
  - URL: `{{ confluence_url }}`
  - User name: `{{ confluence_user_name }}`
  - Password: `{{ confluence_password }}`

## Validate authentication

| Action                                        | Expected result                           |
| --------------------------------------------- | ----------------------------------------- |
| Open the app.                                 |                                           |
| Type in `confluence`.                         | The **Confluence Search** category shows. |
| **Enter** the **Confluence Search** category. | No items show.                            |
| Type in `pieter`.                             | At least one search result shows.         |
