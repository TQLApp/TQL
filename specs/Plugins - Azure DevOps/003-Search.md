---
testspace:
title: Plugins - Azure DevOps / 003 - Search
description: Verify search features.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/Azure-DevOps.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Find backlog

| Action                                       | Expected result           |
| -------------------------------------------- | ------------------------- |
| Open the app.                                |                           |
| Enter the **Azure DevOps Backlog** category. | A list of backlogs shows. |
| **Activate** a backlog.                      | The backlog opens.        |

## Find work item in a backlog

| Action                                           | Expected result                               |
| ------------------------------------------------ | --------------------------------------------- |
| Open the app.                                    |                                               |
| **Enter** the **Azure DevOps Backlog** category. | A list of backlogs shows.                     |
| **Enter** a backlog.                             | All work items in the backlog show.           |
| **Type** a search term.                          | All work items matching the search term show. |
| **Activate** a work item.                        | The work item opens.                          |

## Find board

| Action                                         | Expected result         |
| ---------------------------------------------- | ----------------------- |
| Open the app.                                  |                         |
| **Enter** the **Azure DevOps Board** category. | A list of boards shows. |
| **Activate** a board.                          | The board opens.        |

## Find work item in a board

| Action                                         | Expected result                               |
| ---------------------------------------------- | --------------------------------------------- |
| Open the app.                                  |                                               |
| **Enter** the **Azure DevOps Board** category. | A list of boards shows.                       |
| **Enter** a board.                             | All work items on the board show.             |
| **Type** a search term.                        | All work items matching the search term show. |
| **Activate** a work item.                      | The work item opens.                          |

## Find dashboard

| Action                                             | Expected result            |
| -------------------------------------------------- | -------------------------- |
| Open the app.                                      |                            |
| **Enter** the **Azure DevOps Dashboard** category. | A list of dashboards show. |
| **Activate** a dashboard.                          | The dashboard opens.       |

## Create query

| Action                                       | Expected result                                  |
| -------------------------------------------- | ------------------------------------------------ |
| Open the app.                                |                                                  |
| **Enter** the **Azure DevOps New** category. | A list of things to create shows.                |
| **Type** in `query`.                         | One or more **... › New Query** categories show. |
| **Activate** a **... › New Query** item.     | The query editor opens in the selected project.  |

## Create user story

| Action                                       | Expected result                                   |
| -------------------------------------------- | ------------------------------------------------- |
| Open the app.                                |                                                   |
| **Enter** the **Azure DevOps New** category. | A list of things to create shows.                 |
| **Type** in `user story`.                    | One or more **... › User Story** categories show. |
| **Activate** a **... › User Story** item     | The new user story editor opens.                  |

## Create a pull request

This spec requires a recent new branch. Create this first if it doesn't exist.

| Action                                                                            | Expected result |
| --------------------------------------------------------------------------------- | --------------- |
| Open Azure DevOps at {{ azure_devops_url }}.                                      |                 |
| Open the Launcher project.                                                        |                 |
| Open the Launcher repository.                                                     |                 |
| Open the branches drop-down and click **+ New branch**.                           |                 |
| Change **Based on** to **main**, give the new branch a name and click **Create**. |                 |

This new branch should now show up in the following spec:

| Action                                                                | Expected result                                                                     |
| --------------------------------------------------------------------- | ----------------------------------------------------------------------------------- |
| Open the app.                                                         |                                                                                     |
| **Enter** the **Azure DevOps New** category.                          | A list of things to create shows.                                                   |
| **Type** in `pull request`                                            | One or more **... › New Pull Request** category show.                               |
| **Activate** the **Launcher › Launcher › New Pull Request** category. | One or more branches show, including at least the one you just created.             |
| **Activate** the branch you just created.                             | The new pull request page opens in Azure DevOps with the activated branch selected. |

## Find pipeline

| Action                                            | Expected result            |
| ------------------------------------------------- | -------------------------- |
| Open the app.                                     |                            |
| **Enter** the **Azure DevOps Pipeline** category. | A list of pipelines shows. |
| **Activate** a pipeline.                          | The pipeline opens.        |

## Find query

| Action                                         | Expected result                            |
| ---------------------------------------------- | ------------------------------------------ |
| Open the app.                                  |                                            |
| **Enter** the **Azure DevOps Query** category. | No default list shows.                     |
| **Type** a search term.                        | All queries matching the search term show. |
| **Activate** a query.                          | The query opens.                           |

## Find work items in a query

| Action                                         | Expected result                                             |
| ---------------------------------------------- | ----------------------------------------------------------- |
| Open the app.                                  |                                                             |
| **Enter** the **Azure DevOps Query** category. |                                                             |
| **Enter** a query.                             | All work items matching the query show.                     |
| **Type** a search term.                        | All work items matching the query and the search term show. |
| **Activate** an work item.                     | The work item opens.                                        |

## Find repository

| Action                                              | Expected result        |
| --------------------------------------------------- | ---------------------- |
| Open the app.                                       |                        |
| **Enter** the **Azure DevOps Repository** category. | All repositories show. |
| **Activate** a repository.                          | The repository opens.  |

## Find files in repository

| Action                                              | Expected result                          |
| --------------------------------------------------- | ---------------------------------------- |
| Open the app.                                       |                                          |
| **Enter** the **Azure DevOps Repository** category. | All repositories show.                   |
| **Enter** a repository.                             | All files in the repository show.        |
| **Type** a search term.                             | All files matching the search term show. |
| **Activate** a file.                                | The file opens.                          |

## Find work item

| Action                                             | Expected result                               |
| -------------------------------------------------- | --------------------------------------------- |
| Open the app.                                      |                                               |
| **Enter** the **Azure DevOps Work Item** category. | No work items show.                           |
| **Type** a search term.                            | All work items matching the search term show. |
| **Activate** a work item.                          | The work item opens.                          |
