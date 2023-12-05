---
testspace:
title: Plugins - Outlook / 003 - Search
description: Verify search features.
---

{% if page %}{% assign spec = page %}{% endif %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## [setup]

- Ensure that Outlook is running.
- Ensure that there is at least one person in the contact list.

## Send email

| Action                            | Expected result                                               |
| --------------------------------- | ------------------------------------------------------------- |
| Open the app.                     |                                                               |
| **Enter** the **Email** category. | No people show.                                               |
| **Type** a search term.           | People matching the search term show.                         |
| **Activate** an item.             | A new email screen shows with the person in the **To** field. |
