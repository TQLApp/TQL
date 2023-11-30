---
testspace:
title: Plugins / JIRA / 003 - Search
description: Verify search features.
---

{% if page %}{% assign spec = page %}{% endif %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Find page

Find a page in JIRA:

- Open the app.
- Enter the **JIRA Search** category.
- Type in `pieter`. At least a **Pieter van Ginkel › Overview** search result
  appears.
- **Activate** the **Pieter van Ginkel › Overview** item. The confluence page
  opens in the browser.

## Find person

Find a person in JIRA:

- Open the app.
- Enter the **JIRA Search** category.
- Type in `pieter`. At least a **Pieter van Ginkel** search result appears.
- **Activate** the **Pieter van Ginkel** item. The person space for Pieter van
  Ginkel opens in the browser.

## Verify history

Verify that the activated items are in the history:

- Open the app.
- Enter the **JIRA Search** category. The **Pieter van Ginkel** and **Pieter van
  Ginkel › Overview** items appear in the history.

## Find space

Find a space in JIRA:

- Open the app.
- Enter the **JIRA Space** category.
- Type in `pieter`. At least a **Pieter van Ginkel Space** search result appears
  with a custom avatar. This may not appear the first time. If not, close the
  app and search again. It should appear the second time.
- **Activate** the **Pieter van Ginkel Space** item. The space opens in the
  browser.
