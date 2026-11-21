---
testspace:
title: Plugins - YouTrack / 003 - Search
description: Verify search features.
---

{% if page %}{% assign spec = page %}{% endif %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Find issue

| Action                                      | Expected result                           |
| ------------------------------------------- | ----------------------------------------- |
| Open the app.                               |                                           |
| **Enter** the **YouTrack Issues** category. | No issues show.                           |
| **Type** a search term.                     | All issues matching the search term show. |
| **Type** a partial word, like `kubecod`.    | Issues with words starting with it show.  |
| **Activate** an issue.                      | The issue opens.                          |

## Find issue by ID

| Action                                                      | Expected result                                |
| ----------------------------------------------------------- | ---------------------------------------------- |
| Open the app.                                               |                                                |
| **Enter** the **YouTrack Issues** category.                 |                                                |
| **Type** an existing issue ID in upper case, like `KC-230`. | The issue shows first.                         |
| **Type** the same issue ID in lower case, like `kc-230`.    | The issue shows first.                         |
| **Activate** the issue.                                     | The issue opens directly.                      |
| **Type** an issue ID that does not exist, like `KC-999999`. | No error shows; the list is searched as usual. |

## Find issue with a YouTrack query

| Action                                      | Expected result                              |
| ------------------------------------------- | -------------------------------------------- |
| Open the app.                               |                                              |
| **Enter** the **YouTrack Issues** category. |                                              |
| **Type** in `#Unresolved for: me`.          | Only unresolved issues assigned to you show. |
| **Activate** an issue.                      | The issue opens.                             |

## Find project

| Action                                        | Expected result            |
| --------------------------------------------- | -------------------------- |
| Open the app.                                 |                            |
| **Enter** the **YouTrack Projects** category. | A list of projects shows.  |
| Verify the list.                              | No archived projects show. |
| **Activate** a project.                       | The project page opens.    |

## Find issues in project

| Action                                        | Expected result                                     |
| --------------------------------------------- | --------------------------------------------------- |
| Open the app.                                 |                                                     |
| **Enter** the **YouTrack Projects** category. | A list of projects shows.                           |
| **Enter** a project.                          | All issues of the project show, last updated first. |
| **Type** a search term.                       | Only issues of the project matching the term show.  |
| **Activate** an issue.                        | The issue opens.                                    |

## Find agile board

| Action                                            | Expected result                            |
| ------------------------------------------------- | ------------------------------------------ |
| Open the app.                                     |                                            |
| **Enter** the **YouTrack Agile Boards** category. | A list of boards shows.                    |
| **Type** a search term.                           | The list of boards is filtered by name.    |
| **Activate** a board.                             | The board opens, also if it has no sprint. |
| Select a board.                                   | It cannot be entered to search in it.      |

## Find saved search

| Action                                              | Expected result                                  |
| --------------------------------------------------- | ------------------------------------------------ |
| Open the app.                                       |                                                  |
| **Enter** the **YouTrack Saved Searches** category. | A list of saved searches shows.                  |
| **Activate** a saved search.                        | The issues of the saved search show in YouTrack. |

## Find issue in saved search

| Action                                              | Expected result                                          |
| --------------------------------------------------- | -------------------------------------------------------- |
| Open the app.                                       |                                                          |
| **Enter** the **YouTrack Saved Searches** category. |                                                          |
| **Enter** a saved search.                           | All issues matching the saved search show.               |
| **Type** a search term.                             | Only issues matching the saved search and the term show. |
| **Activate** an issue.                              | The issue opens.                                         |

## Find dashboard

| Action                                          | Expected result                             |
| ----------------------------------------------- | ------------------------------------------- |
| Open the app.                                   |                                             |
| **Enter** the **YouTrack Dashboards** category. | A list of dashboards shows.                 |
| **Type** a search term.                         | The list of dashboards is filtered by name. |
| **Activate** a dashboard.                       | The dashboard opens.                        |
| Select a dashboard.                             | It cannot be entered to search in it.       |

## Create issue

| Action                                         | Expected result                                          |
| ---------------------------------------------- | -------------------------------------------------------- |
| Open the app.                                  |                                                          |
| **Enter** the **New YouTrack Issue** category. | One item per project shows.                              |
| **Type** in the name of a project.             | The item of that project shows.                          |
| **Activate** the item.                         | The new issue page opens with that project pre-selected. |
