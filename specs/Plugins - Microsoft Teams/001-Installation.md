---
testspace:
title: Plugins - Microsoft Teams / 001 - Installation
description: Install the Microsoft Teams plugin.
---

{% if page %}{% assign spec = page %}{% endif %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Clean-Setup.md %}

## Prerequisite

{% include Setup/Local-package-source.md %}

The demo plugin is necessary to get demo people directories.

| Action                                      | Expected result |
| ------------------------------------------- | --------------- |
| Open the **Configuration** window.          |                 |
| Navigate to **Application > Plugins**.      |
| Select the **TQL Demo Plugin** plugin.      |                 |
| **Install** the plugin.                     |                 |
| **Restart** the application when requested. |                 |

## Installation

{% include Setup/Local-package-source.md %}

| Action                                            | Expected result |
| ------------------------------------------------- | --------------- |
| Open the **Configuration** window.                |                 |
| Navigate to **Application > Plugins**.            |
| Select the **TQL Microsoft Teams Plugin** plugin. |                 |
| **Install** the plugin.                           |                 |
| **Restart** the application when requested.       |                 |

## Verify installation

| Action                                                  | Expected result                                               |
| ------------------------------------------------------- | ------------------------------------------------------------- |
| Open the **Configuration** window.                      |                                                               |
| Navigate to **Application > Plugins**.                  |                                                               |
| Switch to the **Installed** tab.                        | The **TQL Microsoft Teams Plugin** plugin shows as installed. |
| Navigate to the **Microsoft Teams** configuration page. | The configuration page opens.                                 |
