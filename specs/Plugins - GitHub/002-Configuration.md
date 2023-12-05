---
testspace:
title: Plugins - GitHub / 002 - Configuration
description: Configure the GitHub plugin.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/GitHub.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Configuration

| Action                                            | Expected result                               |
| ------------------------------------------------- | --------------------------------------------- |
| Open the **Configuration** window.                |                                               |
| Navigate to **GitHub**.                           | The **GitHub > General** page is highlighted. |
| **Add** a new connection with the settings below. |                                               |
| Click **Save**.                                   |                                               |
| Click **Save**.                                   |                                               |

- Connection settings:
  - Name: `{{ github_name }}`

## Validate configuration

| Action                                       | Expected result                               |
| -------------------------------------------- | --------------------------------------------- |
| Open the **Configuration** window.           |                                               |
| Navigate to **GitHub**.                      | The **GitHub > General** page is highlighted. |
| Highlight the connection and click **Edit**. | The settings conform to the settings below.   |

- Connection settings:
  - Name: `{{ github_name }}`

## Validate authentication

| Action                                           | Expected result                              |
| ------------------------------------------------ | -------------------------------------------- |
| Open the app.                                    |                                              |
| Type in `github`.                                | The **My GitHub Repository** category shows. |
| **Enter** the **My GitHub Repository** category. | A list of repository shows.                  |
