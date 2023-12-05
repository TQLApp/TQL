---
testspace:
title: Plugins - Azure / 004 - Second connection
description: Configure a second connection.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/Azure.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Create connection

| Action                                            | Expected result                                     |
| ------------------------------------------------- | --------------------------------------------------- |
| Open the **Configuration** window.                |                                                     |
| Navigate to **Azure Portal**.                     | The **Azure Portal > General** page is highlighted. |
| **Add** a new connection with the settings below. |                                                     |
| Click **Save**.                                   |                                                     |
| Click **Save**.                                   |                                                     |

- Connection settings:
  - Name: `{{ azure_alt_name }}`
  - Tenant ID: `{{ azure_tenant_id }}`

## Verify multiple connections

| Action           | Expected result                                                                                              |
| ---------------- | ------------------------------------------------------------------------------------------------------------ |
| Open the app.    |                                                                                                              |
| Type in `azure`. | One **Azure Portal ({{ azure_name }})** item and one **Azure Portal ({{ azure_alt_name }})** item must show. |

## Complete interactive authentication

| Action                                                          | Expected result                        |
| --------------------------------------------------------------- | -------------------------------------- |
| Open the app.                                                   |                                        |
| **Enter** the **Azure Portal ({{ azure_alt_name }})** category. |                                        |
| Type in `tql`.                                                  | A dialog shows requesting credentials. |
| Click **OK**.                                                   |                                        |
| Complete authentication with Azure.                             | Items matching the search term show.   |
