---
testspace:
title: Plugins - Microsoft Teams / 004 - Second directory
description: Configure a second directory.
---

{% if page %}{% assign spec = page %}{% endif %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Create connection

| Action                                                                 | Expected result                                        |
| ---------------------------------------------------------------------- | ------------------------------------------------------ |
| Open the **Configuration** window.                                     |                                                        |
| Navigate to **Microsoft Teams**.                                       | The **Microsoft Teams > General** page is highlighted. |
| Ensure that both the **English Demo** and **French Demo** are checked. |                                                        |
| Click **Save**.                                                        |                                                        |

## Verify multiple connections

| Action                | Expected result                                                                             |
| --------------------- | ------------------------------------------------------------------------------------------- |
| Open the app.         |                                                                                             |
| Type in `teams chat`. | One **Teams Chat (English Demo)** item and one **Teams Chat (French Demo)** item must show. |
