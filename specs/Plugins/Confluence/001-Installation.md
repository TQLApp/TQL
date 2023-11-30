---
testspace:
title: Plugins / Confluence / 001 - Installation
description: Install the Confluence plugin.
---

{% if page %}{% assign spec = page %}{% endif %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Clean-Setup.md %}

## Installation

Install the Confluence plugin:

- Open the **Configuration** window.
- Navigate to **Application | Plugins**.
- Select the **TQL Confluence Plugin** plugin.
- **Install** the plugin.
- **Restart** the application when requested.

## Verify installation

Verify that the Confluence plugin has been installed:

- Open the **Configuration** window.
- Navigate to **Application | Plugins**.
- Switch to the **Installed** tab. The **TQL Confluence Plugin** plugin shows as
  installed.
- Navigate to the **Confluence** configuration page. The configuration page
  opens.
