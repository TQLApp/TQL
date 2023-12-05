---
testspace:
title: Plugins - Microsoft Teams / 005 - Search second directory
description: Verify functionality with a second directory.
---

{% if page %}{% assign spec = page %}{% endif %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Verify history of the connections

| Action                                                | Expected result                           |
| ----------------------------------------------------- | ----------------------------------------- |
| Open the app.                                         |                                           |
| **Enter** the **Teams Chat (English Demo)** category. | The previously selected person must show. |
| **Enter** the **Teams Chat (French Demo)** category.  | No history must show.                     |

## Delete connection

| Action                                 | Expected result                                        |
| -------------------------------------- | ------------------------------------------------------ |
| Open the **Configuration** window.     |                                                        |
| Navigate to **Microsoft Teams**.       | The **Microsoft Teams > General** page is highlighted. |
| Uncheck the **French Demo** directory. |                                                        |
| Click **Save**.                        |                                                        |

## Verify single connection

| Action                | Expected result                       |
| --------------------- | ------------------------------------- |
| Open the app.         |                                       |
| Type in `teams chat`. | Only a **Teams Chat** item must show. |
