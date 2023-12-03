---
testspace:
title: Plugins / Confluence / 003 - Search
description: Verify search features.
---

{% if page %}{% assign spec = page %}{% endif %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Find page

| Action                                                  | Expected result                                                    |
| ------------------------------------------------------- | ------------------------------------------------------------------ |
| Open the app.                                           |                                                                    |
| Enter the **Confluence Search** category.               |                                                                    |
| Type in `pieter`.                                       | At least a **Pieter van Ginkel › Overview** search result appears. |
| **Activate** the **Pieter van Ginkel › Overview** item. | The confluence page opens.                                         |

## Find person

| Action                                       | Expected result                                         |
| -------------------------------------------- | ------------------------------------------------------- |
| Open the app.                                |                                                         |
| Enter the **Confluence Search** category.    |                                                         |
| Type in `pieter`.                            | At least a **Pieter van Ginkel** search result appears. |
| **Activate** the **Pieter van Ginkel** item. | The person space for Pieter van Ginkel opens.           |

## Verify history

| Action                                    | Expected result                                                                             |
| ----------------------------------------- | ------------------------------------------------------------------------------------------- |
| Open the app.                             |                                                                                             |
| Enter the **Confluence Search** category. | The **Pieter van Ginkel** and **Pieter van Ginkel › Overview** items appear in the history. |

## Find space

| Action                                             | Expected result                                                                    |
| -------------------------------------------------- | ---------------------------------------------------------------------------------- |
| Open the app.                                      |                                                                                    |
| Enter the **Confluence Space** category.           |                                                                                    |
| Type in `pieter`.                                  | At least a **Pieter van Ginkel Space** search result appears with a custom avatar. |
| **Activate** the **Pieter van Ginkel Space** item. | The space opens.                                                                   |

The avatar may not appear the first time. If this happens, close the app and
search again. It should appear the second time.
