---
testspace:
title: Plugins \ Azure \ 002 - Configuration
description: Configure the Azure plugin.
---

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Configuration

Create a new connection:

- Open the **Configuration** window.
- Navigate to **Azure Portal**. The **Azure Portal | General** page is highlighted.
- **Add** a new connection with the following information:
  - Name: `pvginkel`
  - Tenant ID: `4bfd811a-711a-4de5-afa8-9c904fca2045`
- Click **Save**.
- Click **Save**.

## Validate configuration

Validate that the connection has been saved:

- Open the **Configuration** window.
- Navigate to **Azure Portal**. The **Azure Portal | General** page is highlighted.
- Highlight the connection and click **Edit**.
- Validate that the the values are as follows:
  - Name: `pvginkel`
  - Tenant ID: `4bfd811a-711a-4de5-afa8-9c904fca2045`

## Complete interactive authentication

Complete first time authentication with Azure Portal:

- Open the app.
- Type in `Azure`. The **Azure Portal** category must appear.
- **Enter** the **Azure Portal** category. No items should appear.
- Type in `tql`. A dialog shows requesting credentials.
- Click **OK**.
- Complete authentication with Azure.
