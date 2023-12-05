---
testspace:
title: Plugins - Microsoft Teams / 002 - Configuration
description: Configure the Microsoft Teams plugin.
---

{% if page %}{% assign spec = page %}{% endif %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Configuration

| Action                                                        | Expected result                                        |
| ------------------------------------------------------------- | ------------------------------------------------------ |
| Open the **Configuration** window.                            |                                                        |
| Navigate to **Microsoft Teams**.                              | The **Microsoft Teams > General** page is highlighted. |
| Ensure **Enable these directories only** is checked.          | The list of directories becomes enabled.               |
| Ensure only the **English Demo** people directory is checked. |                                                        |
| Click **Save**.                                               |                                                        |
