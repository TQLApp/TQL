---
testspace:
title: Plugins - Azure DevOps / 002 - Configuration
description: Configure the Azure DevOps plugin.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/Azure-DevOps.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Configuration

| Action                                            | Expected result                                     |
| ------------------------------------------------- | --------------------------------------------------- |
| Open the **Configuration** window.                |                                                     |
| Navigate to **Azure DevOps**.                     | The **Azure DevOps > General** page is highlighted. |
| **Add** a new connection with the settings below. |                                                     |
| Click **Save**.                                   |                                                     |
| Click **Save**.                                   |                                                     |

- Connection settings:
  - Name: `{{ azure_devops_name }}`
  - URL: `{{ azure_devops_url }}`
  - PAT token: `{{ azure_devops_pat_token }}`

## Validate configuration

| Action                                       | Expected result                                     |
| -------------------------------------------- | --------------------------------------------------- |
| Open the **Configuration** window.           |                                                     |
| Navigate to **Azure DevOps**.                | The **Azure DevOps > General** page is highlighted. |
| Highlight the connection and click **Edit**. | The settings conform to the settings below.         |

- Connection settings:
  - Name: `{{ azure_devops_name }}`
  - URL: `{{ azure_devops_url }}`
  - PAT token: `{{ azure_devops_pat_token }}`

## Validate authentication

| Action                                         | Expected result                            |
| ---------------------------------------------- | ------------------------------------------ |
| Open the app.                                  |                                            |
| Type in `azure devops`.                        | The **Azure DevOps Board** category shows. |
| **Enter** the **Azure DevOps Board** category. | A list of boards shows.                    |
