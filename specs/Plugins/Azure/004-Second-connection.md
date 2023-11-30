---
testspace:
title: Plugins / Azure / 004 - Second connection
description: Configure a second connection.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/Azure.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Create connection

Create a new connection:

- Open the **Configuration** window.
- Navigate to **Azure Portal**. The **Azure Portal | General** page is
  highlighted.
- **Add** a new connection with the following information:
  - Name: `{{ azure_alt_name }}`
  - Tenant ID: `{{ azure_tenant_id }}`
- Click **Save**.
- Click **Save**.

## Verify multiple connections

Verify that there are multiple **Azure Portal** entries:

- Open the app.
- Type in `azure`. One **Azure Portal ({{ azure_name }})** item and one **Azure
  Portal ({{ azure_alt_name }})** item must appear.

## Complete interactive authentication

Complete first time authentication with Azure Portal:

- Open the app.
- **Enter** the **Azure Portal ({{ azure_alt_name }})** category.
- Type in `tql`. A dialog shows requesting credentials.
- Click **OK**.
- Complete authentication with Azure.
