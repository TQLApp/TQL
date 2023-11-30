---
testspace:
title: Plugins / Confluence / 004 - Second connection
description: Configure a second connection.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/Confluence.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Create connection

Create a new connection:

- Open the **Configuration** window.
- Navigate to **Confluence**. The **Confluence | General** page is highlighted.
- **Add** a new connection with the following information:
  - Name: `{{ confluence_alt_name }}`
  - URL: `{{ confluence_url }}`
  - User name: `{{ confluence_user_name }}`
  - Password: `{{ confluence_password }}`
- Click **Save**.
- Click **Save**.

## Verify multiple connections

Verify that there are multiple **Confluence** entries:

- Open the app.
- Type in `confluence`. One **Confluence Search ({{ confluence_name }})** item
  and one **Confluence Search ({{ confluence_alt_name }})** entry must item.
