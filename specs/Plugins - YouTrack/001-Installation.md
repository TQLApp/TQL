---
testspace:
title: Plugins - YouTrack / 001 - Installation
description: Install the YouTrack plugin.
---

{% if page %}{% assign spec = page %}{% endif %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Clean-Setup.md %}

## Installation

{% include Setup/Local-package-source.md %}

Until the plugin is published, run it from the KubeCoder dev build instead. The
plugin is already loaded in that build, so skip the installation table below and
continue with **Verify installation**, which then only checks that the plugin is
present:

- Run `\\tqlapp-tql-49385e.home\work\TQL\scripts\Run-KubeCoder-Build.bat`. It
  copies the build to `%TEMP%\TQL-DebugApp` and starts
  `Tql.DebugApp.exe --env Debug` there.

For the published plugin:

| Action                                      | Expected result |
| ------------------------------------------- | --------------- |
| Open the **Configuration** window.          |                 |
| Navigate to **Application > Plugins**.      |                 |
| Select the **TQL YouTrack Plugin** plugin.  |                 |
| **Install** the plugin.                     |                 |
| **Restart** the application when requested. |                 |

## Verify installation

| Action                                           | Expected result                                        |
| ------------------------------------------------ | ------------------------------------------------------ |
| Open the **Configuration** window.               |                                                        |
| Navigate to **Application > Plugins**.           |                                                        |
| Switch to the **Installed** tab.                 | The **TQL YouTrack Plugin** plugin shows as installed. |
| Navigate to the **YouTrack** configuration page. | The configuration page opens.                          |
