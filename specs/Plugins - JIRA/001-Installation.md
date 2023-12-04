---
testspace:
title: Plugins - JIRA / 001 - Installation
description: Install the JIRA plugin.
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
| Select the **TQL JIRA Plugin** plugin.      |                 |
| **Install** the plugin.                     |                 |
| **Restart** the application when requested. |                 |

## Verify installation

| Action                                       | Expected result                                    |
| -------------------------------------------- | -------------------------------------------------- |
| Open the **Configuration** window.           |                                                    |
| Navigate to **Application > Plugins**.       |                                                    |
| Switch to the **Installed** tab.             | The **TQL JIRA Plugin** plugin shows as installed. |
| Navigate to the **JIRA** configuration page. | The configuration page opens.                      |
