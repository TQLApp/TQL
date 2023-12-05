---
testspace:
title: Plugins - Azure DevOps / 004 - Second connection
description: Configure a second connection.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/Azure-DevOps.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Create connection

| Action                                            | Expected result                                     |
| ------------------------------------------------- | --------------------------------------------------- |
| Open the **Configuration** window.                |                                                     |
| Navigate to **Azure DevOps**.                     | The **Azure DevOps > General** page is highlighted. |
| **Add** a new connection with the settings below. |                                                     |
| Click **Save**.                                   |                                                     |
| Click **Save**.                                   |                                                     |

- Connection settings:
  - Name: `{{ azure_devops_alt_name }}`
  - URL: `{{ azure_devops_url }}`
  - PAT token: `{{ azure_devops_pat_token }}`

## Verify multiple connections

| Action                        | Expected result                                                                                                                         |
| ----------------------------- | --------------------------------------------------------------------------------------------------------------------------------------- |
| Open the app.                 |                                                                                                                                         |
| Type in `azure devops board`. | One **Azure DevOps Board ({{ azure_devops_name }})** item and one **Azure DevOps Board ({{ azure_devops_alt_name }})** entry must item. |
