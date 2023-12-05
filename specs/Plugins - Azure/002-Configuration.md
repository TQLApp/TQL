---
testspace:
title: Plugins - Azure / 002 - Configuration
description: Configure the Azure plugin.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/Azure.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Configuration

| Action                                            | Expected result                                     |
| ------------------------------------------------- | --------------------------------------------------- |
| Open the **Configuration** window.                |                                                     |
| Navigate to **Azure Portal**.                     | The **Azure Portal > General** page is highlighted. |
| **Add** a new connection with the settings below. |                                                     |
| Click **Save**.                                   |                                                     |
| Click **Save**.                                   |                                                     |

- Connection settings:
  - Name: `{{ azure_name }}`
  - Tenant ID: `{{ azure_tenant_id }}`

## Validate configuration

| Action                                       | Expected result                                        |
| -------------------------------------------- | ------------------------------------------------------ |
| Open the **Configuration** window.           |                                                        |
| Navigate to **Azure Portal**.                | The **Azure Portal > General** page is highlighted.    |
| Highlight the connection and click **Edit**. | The settings conform to the connection settings below. |

- Connection settings:
  - Name: `{{ azure_name }}`
  - Tenant ID: `{{ azure_tenant_id }}`

## Complete interactive authentication

| Action                                   | Expected result                        |
| ---------------------------------------- | -------------------------------------- |
| Open the app.                            |                                        |
| Type in `azure`.                         | The **Azure Portal** category shows.   |
| **Enter** the **Azure Portal** category. | No items show.                         |
| Type in `tql`.                           | A dialog shows requesting credentials. |
| Click **OK**.                            |                                        |
| Complete authentication with Azure.      | Items matching the search term show.   |
