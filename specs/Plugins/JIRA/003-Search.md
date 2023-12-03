---
testspace:
title: Plugins / JIRA / 003 - Search
description: Verify search features.
---

{% if page %}{% assign spec = page %}{% endif %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Find board

| Action                             | Expected result           |
| ---------------------------------- | ------------------------- |
| Open the app.                      |                           |
| Enter the **JIRA Board** category. | A list of boards appears. |
| Activate a board.                  | The board opens.          |

## Find board quick filter

| Action                                          | Expected result                                               |
| ----------------------------------------------- | ------------------------------------------------------------- |
| Open the app.                                   |                                                               |
| Enter the **JIRA Board** category.              |                                                               |
| Enter the **CM Board - Kanban board** category. | A list of quick filters and a category **All Issue** appears. |
| Activate the **All Issues** item.               | The board opens without a quick filter selected.              |
| Activate a quick filter.                        | The board opens with the quick filter selected.               |

## Find issue on a board

| Action                                          | Expected result                                     |
| ----------------------------------------------- | --------------------------------------------------- |
| Open the app.                                   |                                                     |
| Enter the **JIRA Board** category.              |                                                     |
| Enter the **CM Board - Kanban board** category. |                                                     |
| Enter the **All Issues** category.              | All issues on the board appear, last updated first. |
| Enter a search term.                            | All issues matching the search term appear.         |

## Find issue in a quick filter

| Action                                          | Expected result                                                   |
| ----------------------------------------------- | ----------------------------------------------------------------- |
| Open the app.                                   |                                                                   |
| Enter the **JIRA Board** category.              |                                                                   |
| Enter the **CM Board - Kanban board** category. |                                                                   |
| Enter the quick filter category.                | All issues matching the quick filter appear.                      |
| Enter a search term.                            | Only issues matching the search term and the quick filter appear. |

## Find dashboard

| Action                                 | Expected result              |
| -------------------------------------- | ---------------------------- |
| Open the app.                          |                              |
| Enter the **JIRA Dashboard** category. | A list of dashboards appear. |
| Activate a dashboard.                  | The dashboard opens.         |

## Find filter

| Action                              | Expected result           |
| ----------------------------------- | ------------------------- |
| Open the app.                       |                           |
| Enter the **JIRA Filter** category. | A list of filters appear. |
| Activate a filter.                  | The filter opens.         |

## Find issue in a filter

| Action                              | Expected result                                            |
| ----------------------------------- | ---------------------------------------------------------- |
| Open the app.                       |                                                            |
| Enter the **JIRA Filter** category. |                                                            |
| Enter a filter.                     | All issues matching the filter appear.                     |
| Enter a search term.                | All issues matching the filter and the search term appear. |
| Activate an issue.                  | The issue opens.                                           |

**TODO**
