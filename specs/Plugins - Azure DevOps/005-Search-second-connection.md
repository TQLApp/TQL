---
testspace:
title: Plugins - Azure DevOps / 005 - Search second connection
description: Verify functionality with a second connection.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/Azure-DevOps.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Verify history of the connections

| Action                                                                       | Expected result                                       |
| ---------------------------------------------------------------------------- | ----------------------------------------------------- |
| Open the app.                                                                |                                                       |
| **Enter** the **Azure DevOps Board ({{ azure_devops_name }})** category.     | The previously selected board appears in the history. |
| **Enter** the **Azure DevOps Board ({{ azure_devops_alt_name }})** category. | No history appears.                                   |

## Delete connection

| Action                                                                         | Expected result                                     |
| ------------------------------------------------------------------------------ | --------------------------------------------------- |
| Open the **Configuration** window.                                             |                                                     |
| Navigate to **Azure DevOps**.                                                  | The **Azure DevOps > General** page is highlighted. |
| Highlight the **{{ azure_devops_alt_name }}** connection and click **Delete**. | The connection is deleted.                          |
| Click **Save**.                                                                |                                                     |

## Verify single connection

| Action                        | Expected result                                                                                                                                                          |
| ----------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Open the app.                 |                                                                                                                                                                          |
| Type in `azure devops board`. | Only a **Azure DevOps Board** item must appear instead of the **Azure DevOps Board ({{ azure_devops_name }})** and **Azure DevOps Board ({{ azure_devops_alt_name }})**. |
