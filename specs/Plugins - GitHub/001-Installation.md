---
testspace:
title: Plugins - GitHub / 001 - Installation
description: Install the GitHub plugin.
---

{% if page %}{% assign spec = page %}{% endif %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Clean-Setup.md %}

## Installation

{% include Setup/Local-package-source.md %}

| Action                                      | Expected result |
| ------------------------------------------- | --------------- |
| Open the **Configuration** window.          |                 |
| Navigate to **Application > Plugins**.      |                 |
| Select the **TQL GitHub Plugin** plugin.    |                 |
| **Install** the plugin.                     |                 |
| **Restart** the application when requested. |                 |

## Verify installation

| Action                                         | Expected result                                      |
| ---------------------------------------------- | ---------------------------------------------------- |
| Open the **Configuration** window.             |                                                      |
| Navigate to **Application > Plugins**.         |                                                      |
| Switch to the **Installed** tab.               | The **TQL GitHub Plugin** plugin shows as installed. |
| Navigate to the **GitHub** configuration page. | The configuration page opens.                        |
