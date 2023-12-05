---
testspace:
title: Plugins - Outlook / 002 - Configuration
description: Configure the Outlook plugin.
---

{% if page %}{% assign spec = page %}{% endif %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Configuration

| Action                                              | Expected result                                |
| --------------------------------------------------- | ---------------------------------------------- |
| Open the **Configuration** window.                  |                                                |
| Navigate to **Outlook**.                            | The **Outlook > General** page is highlighted. |
| Ensure that the **Name format** is set to **None**. |                                                |
| Click **Save**.                                     |                                                |

## Outlook must be running

| Action                            | Expected result                                                 |
| --------------------------------- | --------------------------------------------------------------- |
| **Close** Outlook.                |                                                                 |
| Open the app.                     |                                                                 |
| **Enter** the **Email** category. |                                                                 |
| **Type** a search term.           | An notification bar shows stating that Outlook must be running. |
| **Start** Outlook.                |                                                                 |
| **Enter** the **Email** category. |                                                                 |
| **Type** a search term.           | The notification bar is not displayed.                          |
