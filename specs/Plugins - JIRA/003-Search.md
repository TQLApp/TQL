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

| Action                             | Expected result         |
| ---------------------------------- | ----------------------- |
| Open the app.                      |                         |
| Enter the **JIRA Board** category. | A list of boards shows. |
| **Activate** a board.              | The board opens.        |

## Find board quick filter

| Action                                              | Expected result                                             |
| --------------------------------------------------- | ----------------------------------------------------------- |
| Open the app.                                       |                                                             |
| **Enter** the **JIRA Board** category.              |                                                             |
| **Enter** the **CM Board - Kanban board** category. | A list of quick filters and a category **All Issue** shows. |
| **Activate** the **All Issues** item.               | The board opens without a quick filter selected.            |
| **Activate** a quick filter.                        | The board opens with the quick filter selected.             |

## Find issue on a board

| Action                                              | Expected result                                   |
| --------------------------------------------------- | ------------------------------------------------- |
| Open the app.                                       |                                                   |
| **Enter** the **JIRA Board** category.              |                                                   |
| **Enter** the **CM Board - Kanban board** category. |                                                   |
| **Enter** the **All Issues** category.              | All issues on the board show, last updated first. |
| **Type** a search term.                             | All issues matching the search term show.         |

## Find issue in a quick filter

| Action                                              | Expected result                                                 |
| --------------------------------------------------- | --------------------------------------------------------------- |
| Open the app.                                       |                                                                 |
| **Enter** the **JIRA Board** category.              |                                                                 |
| **Enter** the **CM Board - Kanban board** category. |                                                                 |
| **Enter** the quick filter category.                | All issues matching the quick filter show.                      |
| **Type** a search term.                             | Only issues matching the search term and the quick filter show. |

## Find dashboard

| Action                                     | Expected result            |
| ------------------------------------------ | -------------------------- |
| Open the app.                              |                            |
| **Enter** the **JIRA Dashboard** category. | A list of dashboards show. |
| **Activate** a dashboard.                  | The dashboard opens.       |

## Find filter

| Action                              | Expected result         |
| ----------------------------------- | ----------------------- |
| Open the app.                       |                         |
| Enter the **JIRA Filter** category. | A list of filters show. |
| Activate a filter.                  | The filter opens.       |

## Find issue in a filter

| Action                                  | Expected result                                          |
| --------------------------------------- | -------------------------------------------------------- |
| Open the app.                           |                                                          |
| **Enter** the **JIRA Filter** category. |                                                          |
| **Enter** a filter.                     | All issues matching the filter show.                     |
| **Type** a search term.                 | All issues matching the filter and the search term show. |
| **Activate** an issue.                  | The issue opens.                                         |

## Find issue

| Action                                 | Expected result                           |
| -------------------------------------- | ----------------------------------------- |
| Open the app.                          |                                           |
| **Enter** the **JIRA Issue** category. | No issues show.                           |
| **Type** a search term.                | All issues matching the search term show. |
| **Activate** an issue.                 | The issue opens.                          |

## Create query

| Action                                   | Expected result                                                             |
| ---------------------------------------- | --------------------------------------------------------------------------- |
| Open the app.                            |                                                                             |
| **Enter** the **JIRA New** category.     | A list of things to create shows.                                           |
| **Type** in `query`.                     | One or more **... › New Query** categories show.                            |
| **Activate** a **... › New Query** item. | The query editor opens with the selected project pre-selected in the query. |

## Create issue

| Action                                  | Expected result                                  |
| --------------------------------------- | ------------------------------------------------ |
| Open the app.                           |                                                  |
| **Enter** the **JIRA New** category.    | A list of things to create shows.                |
| **Type** in `story`.                    | One or more **... › New Story** categories show. |
| **Activate** a **... › New Story** item | The new story editor opens.                      |

## Find project

| Action                                   | Expected result                  |
| ---------------------------------------- | -------------------------------- |
| Open the app.                            |                                  |
| **Enter** the **JIRA Project** category. | A list of projects show.         |
| **Activate** a project.                  | The board for the project opens. |

## Find issues in project

| Action                               | Expected result                                     |
| ------------------------------------ | --------------------------------------------------- |
| Open the app.                        |                                                     |
| Enter the **JIRA Project** category. | A list of projects show.                            |
| **Enter** a project.                 | All issues of the project show, last updated first. |
| **Type** a search term.              | All issues matching the search term show.           |
