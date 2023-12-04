---
testspace:
title: Plugins - JIRA / 003 - Search
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
| **Activate** a board.              | The board opens.          |

## Find board quick filter

| Action                                              | Expected result                                               |
| --------------------------------------------------- | ------------------------------------------------------------- |
| Open the app.                                       |                                                               |
| **Enter** the **JIRA Board** category.              |                                                               |
| **Enter** the **CM Board - Kanban board** category. | A list of quick filters and a category **All Issue** appears. |
| **Activate** the **All Issues** item.               | The board opens without a quick filter selected.              |
| **Activate** a quick filter.                        | The board opens with the quick filter selected.               |

## Find issue on a board

| Action                                              | Expected result                                     |
| --------------------------------------------------- | --------------------------------------------------- |
| Open the app.                                       |                                                     |
| **Enter** the **JIRA Board** category.              |                                                     |
| **Enter** the **CM Board - Kanban board** category. |                                                     |
| **Enter** the **All Issues** category.              | All issues on the board appear, last updated first. |
| **Type** a search term.                             | All issues matching the search term appear.         |

## Find issue in a quick filter

| Action                                              | Expected result                                                   |
| --------------------------------------------------- | ----------------------------------------------------------------- |
| Open the app.                                       |                                                                   |
| **Enter** the **JIRA Board** category.              |                                                                   |
| **Enter** the **CM Board - Kanban board** category. |                                                                   |
| **Enter** the quick filter category.                | All issues matching the quick filter appear.                      |
| **Type** a search term.                             | Only issues matching the search term and the quick filter appear. |

## Find dashboard

| Action                                     | Expected result              |
| ------------------------------------------ | ---------------------------- |
| Open the app.                              |                              |
| **Enter** the **JIRA Dashboard** category. | A list of dashboards appear. |
| **Activate** a dashboard.                  | The dashboard opens.         |

## Find filter

| Action                              | Expected result           |
| ----------------------------------- | ------------------------- |
| Open the app.                       |                           |
| Enter the **JIRA Filter** category. | A list of filters appear. |
| Activate a filter.                  | The filter opens.         |

## Find issue in a filter

| Action                                  | Expected result                                            |
| --------------------------------------- | ---------------------------------------------------------- |
| Open the app.                           |                                                            |
| **Enter** the **JIRA Filter** category. |                                                            |
| **Enter** a filter.                     | All issues matching the filter appear.                     |
| **Type** a search term.                 | All issues matching the filter and the search term appear. |
| **Activate** an issue.                  | The issue opens.                                           |

## Find issue

| Action                                 | Expected result                             |
| -------------------------------------- | ------------------------------------------- |
| Open the app.                          |                                             |
| **Enter** the **JIRA Issue** category. | No issues appear.                           |
| **Type** a search term.                | All issues matching the search term appear. |
| **Activate** an issue.                 | The issue opens.                            |

## Create query

| Action                               | Expected result                                                             |
| ------------------------------------ | --------------------------------------------------------------------------- |
| Open the app.                        |                                                                             |
| **Enter** the **JIRA New** category. | A list of things to create appears.                                         |
| Type in `query`.                     | One or more **... › Query** categories appear.                              |
| **Activate** a **... › Query** item. | The query editor opens with the selected project pre-selected in the query. |

## Create issue

| Action                               | Expected result                                |
| ------------------------------------ | ---------------------------------------------- |
| Open the app.                        |                                                |
| **Enter** the **JIRA New** category. | A list of things to create appears.            |
| Type in `story`.                     | One or more **... › Story** categories appear. |
| **Activate** a **... › Story** item  | The new story editor opens.                    |

## Find project

| Action                               | Expected result                  |
| ------------------------------------ | -------------------------------- |
| Open the app.                        |                                  |
| Enter the **JIRA Project** category. | A list of projects appear.       |
| **Activate** a project.              | The board for the project opens. |

## Find issues in project

| Action                               | Expected result                                       |
| ------------------------------------ | ----------------------------------------------------- |
| Open the app.                        |                                                       |
| Enter the **JIRA Project** category. | A list of projects appear.                            |
| **Enter** a project.                 | All issues of the project appear, last updated first. |
| **Type** a search term.              | All issues matching the search term appear.           |
