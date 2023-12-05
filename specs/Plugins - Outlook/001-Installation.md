---
testspace:
title: Plugins - Outlook / 001 - Installation
description: Install the Outlook plugin.
---

{% if page %}{% assign spec = page %}{% endif %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Clean-Setup.md %}

## Installation

| Action                                      | Expected result |
| ------------------------------------------- | --------------- |
| Open the **Configuration** window.          |                 |
| Navigate to **Application > Plugins**.      |                 |
| Select the **TQL Outlook Plugin** plugin.   |                 |
| **Install** the plugin.                     |                 |
| **Restart** the application when requested. |                 |

## Verify installation

| Action                                          | Expected result                                       |
| ----------------------------------------------- | ----------------------------------------------------- |
| Open the **Configuration** window.              |                                                       |
| Navigate to **Application > Plugins**.          |                                                       |
| Switch to the **Installed** tab.                | The **TQL Outlook Plugin** plugin shows as installed. |
| Navigate to the **Outlook** configuration page. | The configuration page opens.                         |
