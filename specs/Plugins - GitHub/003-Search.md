---
testspace:
title: Plugins - GitHub / 003 - Search
description: Verify search features.
---

{% if page %}{% assign spec = page %}{% endif %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Find gist

| Action                                  | Expected result                      |
| --------------------------------------- | ------------------------------------ |
| Open the app.                           |                                      |
| **Enter** the **GitHub Gist** category. | No gists show.                       |
| **Type** a search term.                 | Gists matching the search term show. |
| **Activate** a gist.                    | The gist opens.                      |

## Find my gist

| Action                                     | Expected result                           |
| ------------------------------------------ | ----------------------------------------- |
| Open the app.                              |                                           |
| **Enter** the **My GitHub Gist** category. | A list of gists shows.                    |
| **Type** a search term.                    | Only gists matching the search term show. |
| **Activate** a gist.                       | The gist opens.                           |

## Find repository

| Action                                        | Expected result                                 |
| --------------------------------------------- | ----------------------------------------------- |
| Open the app.                                 |                                                 |
| **Enter** the **GitHub Repository** category. | No repositories show.                           |
| **Type** a search term.                       | All repositories matching the search term show. |
| **Activate** an repository.                   | The repository opens.                           |

## Find my repository

| Action                                           | Expected result                                    |
| ------------------------------------------------ | -------------------------------------------------- |
| Open the app.                                    |                                                    |
| **Enter** the **My GitHub Repository** category. | My repositories show.                              |
| **Type** a search term.                          | All my repositories matching the search term show. |
| **Activate** an repository.                      | The repository opens.                              |

## Find issue

| Action                                           | Expected result                           |
| ------------------------------------------------ | ----------------------------------------- |
| Open the app.                                    |                                           |
| **Enter** the **GitHub My Repository** category. | Your repositories show.                   |
| **Activate** the **TQL** repository.             | Sub categories show.                      |
| **Activate** the **... › Issue** category.       | All issues in the repository show.        |
| **Type** a search term.                          | All issues matching the search term show. |
| **Activate** an issue.                           | The issue opens.                          |

## Find pull request

| Action                                            | Expected result                                  |
| ------------------------------------------------- | ------------------------------------------------ |
| Open the app.                                     |                                                  |
| Open the app.                                     |                                                  |
| **Enter** the **GitHub My Repository** category.  | Your repositories show.                          |
| **Activate** the **TQL** repository.              | Sub categories show.                             |
| **Activate** the **... › Pull Request** category. | All pull requests in the repository show.        |
| **Type** a search term.                           | All pull requests matching the search term show. |
| **Activate** an pull request.                     | The pull request opens.                          |

## Find user

| Action                                  | Expected result                          |
| --------------------------------------- | ---------------------------------------- |
| Open the app.                           |                                          |
| **Enter** the **GitHub User** category. | No users show.                           |
| **Type** a search term.                 | All users matching the search term show. |
| **Activate** an user.                   | The users home page opens.               |

## Find workflow run

| Action                                            | Expected result                                  |
| ------------------------------------------------- | ------------------------------------------------ |
| Open the app.                                     |                                                  |
| **Enter** the **GitHub My Repository** category.  | Your repositories show.                          |
| **Activate** the **TQL** repository.              | Sub categories show.                             |
| **Activate** the **... › Workflow Run** category. | All workflow runs in the repository show.        |
| **Type** a search term.                           | All workflow runs matching the search term show. |
| **Activate** a workflow run.                      | The workflow run opens.                          |

## Find milestone

| Action                                           | Expected result                               |
| ------------------------------------------------ | --------------------------------------------- |
| Open the app.                                    |                                               |
| **Enter** the **GitHub My Repository** category. | Your repositories show.                       |
| **Activate** the **TQL** repository.             | Sub categories show.                          |
| **Activate** the **... › Milestone** category.   | All milestones in the repository show.        |
| **Type** a search term.                          | All milestones matching the search term show. |
| **Activate** a milestone run.                    | The milestone run opens.                      |

## Create repository

| Action                                     | Expected result                                |
| ------------------------------------------ | ---------------------------------------------- |
| Open the app.                              |                                                |
| **Enter** the **GitHub New** category.     | A list of things to create show.               |
| **Activate** the **New Repository** match. | The web page to create a new repository opens. |

## Import repository

| Action                                        | Expected result                                |
| --------------------------------------------- | ---------------------------------------------- |
| Open the app.                                 |                                                |
| **Enter** the **GitHub New** category.        | A list of things to create show.               |
| **Activate** the **Import Repository** match. | The web page to import a new repository opens. |

## Create codespace

| Action                                    | Expected result                               |
| ----------------------------------------- | --------------------------------------------- |
| Open the app.                             |                                               |
| **Enter** the **GitHub New** category.    | A list of things to create show.              |
| **Activate** the **New Codespace** match. | The web page to create a new codespace opens. |

## Create gist

| Action                                 | Expected result                          |
| -------------------------------------- | ---------------------------------------- |
| Open the app.                          |                                          |
| **Enter** the **GitHub New** category. | A list of things to create show.         |
| **Activate** the **New Gist** match.   | The web page to create a new gist opens. |

## Create organization

| Action                                       | Expected result                                  |
| -------------------------------------------- | ------------------------------------------------ |
| Open the app.                                |                                                  |
| **Enter** the **GitHub New** category.       | A list of things to create show.                 |
| **Activate** the **New Organization** match. | The web page to create a new organization opens. |

## Create issue in a repository with templates

| Action                                             | Expected result                                                           |
| -------------------------------------------------- | ------------------------------------------------------------------------- |
| Open the app.                                      |                                                                           |
| **Enter** the **GitHub New** category.             | A list of things to create show.                                          |
| Type `tql new issue`                               | Matches to create a new issue, and to create issues from a template show. |
| **Activate** the **TQLApp/TQL › New Issue** match. | The web page to select an issue template opens.                           |

## Create issue in a repository without templates

| Action                                                          | Expected result                                                           |
| --------------------------------------------------------------- | ------------------------------------------------------------------------- |
| Open the app.                                                   |                                                                           |
| **Enter** the **GitHub New** category.                          | A list of things to create show.                                          |
| Type `tqlapp new issue`                                         | Matches to create a new issue, and to create issues from a template show. |
| **Activate** the **TQLApp/tqlapp.github.io › New Issue** match. | The web page to create an issue opens.                                    |

## Create an issue from a template

| Action                                                          | Expected result                                                           |
| --------------------------------------------------------------- | ------------------------------------------------------------------------- |
| Open the app.                                                   |                                                                           |
| **Enter** the **GitHub New** category.                          | A list of things to create show.                                          |
| Type `tql new issue`                                            | Matches to create a new issue, and to create issues from a template show. |
| **Activate** the **TQLApp/TQL › New Issue › Bug report** match. | The web page to create a new issue with the bug report template opens.    |

## Create pull request

This spec requires a recent new branch. Create this first if it doesn't exist.

| Action                                                                                     | Expected result |
| ------------------------------------------------------------------------------------------ | --------------- |
| Open Azure DevOps at https://github.com.                                                   |                 |
| Open a repository of your own.                                                             |                 |
| Open the branches drop-down and click **View all branches**.                               |                 |
| Click **New branch**.                                                                      |                 |
| Change **Source** to **main**, give the new branch a name and click **Create new branch**. |                 |

This new branch should now show up in the following spec:

| Action                                                                    | Expected result                                                               |
| ------------------------------------------------------------------------- | ----------------------------------------------------------------------------- |
| Open the app.                                                             |                                                                               |
| **Enter** the **GitHub New** category.                                    | A list of things to create shows.                                             |
| **Type** in `pull request`                                                | One or more **... › New Pull Request** category show.                         |
| **Activate** the **... › New Pull Request** category for your repository. | One or more branches show, including at least the one you just created.       |
| **Activate** the branch you just created.                                 | The new pull request page opens in GitHub with the activated branch selected. |
