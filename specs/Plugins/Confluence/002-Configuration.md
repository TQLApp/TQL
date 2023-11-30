---
testspace:
title: Plugins / Confluence / 002 - Configuration
description: Configure the Confluence plugin.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/Confluence.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Configuration

Create a new connection:

- Open the **Configuration** window.
- Navigate to **Confluence**. The **Confluence | General** page is highlighted.
- **Add** a new connection with the following information:
  - Name: `{{ confluence_name }}`
  - URL: `{{ confluence_url }}`
  - User name: `{{ confluence_user_name }}`
  - Password: `{{ confluence_password }}`
- Click **Save**.
- Click **Save**.

## Validate configuration

Validate that the connection has been saved:

- Open the **Configuration** window.
- Navigate to **Confluence**. The **Confluence | General** page is highlighted.
- Highlight the connection and click **Edit**.
- Validate that the the values are as follows:
  - Name: `{{ confluence_name }}`
  - URL: `{{ confluence_url }}`
  - User name: `{{ confluence_user_name }}`
  - Password: `{{ confluence_password }}`

## Validate authentication

Validate access to Confluence:

- Open the app.
- Type in `confluence`. The **Confluence Search** category must appear.
- **Enter** the **Confluence Search** category. No items should appear.
- Type in `pieter`. At least one search result appears.
