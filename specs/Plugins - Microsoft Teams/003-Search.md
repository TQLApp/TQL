---
testspace:
title: Plugins - Microsoft Teams / 003 - Search
description: Verify search features.
---

{% if page %}{% assign spec = page %}{% endif %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Start a chat

| Action                                 | Expected result                           |
| -------------------------------------- | ----------------------------------------- |
| Open the app.                          |                                           |
| **Enter** the **Teams Chat** category. | No people show.                           |
| **Type** a search term.                | All people matching the search term show. |
| **Activate** an item.                  | A chat starts with the selected person.   |

## Start a call

| Action                                 | Expected result                           |
| -------------------------------------- | ----------------------------------------- |
| Open the app.                          |                                           |
| **Enter** the **Teams Call** category. | No people show.                           |
| **Type** a search term.                | All people matching the search term show. |
| **Activate** an item.                  | A call starts with the selected person.   |

## Start a video call

| Action                                       | Expected result                               |
| -------------------------------------------- | --------------------------------------------- |
| Open the app.                                |                                               |
| **Enter** the **Teams Video Call** category. | No people show.                               |
| **Type** a search term.                      | All people matching the search term show.     |
| **Activate** an item.                        | A video call starts with the selected person. |
