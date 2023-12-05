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

## Find issue

| Action                                   | Expected result                           |
| ---------------------------------------- | ----------------------------------------- |
| Open the app.                            |                                           |
| **Enter** the **GitHub Issue** category. | No issues show.                           |
| **Type** a search term.                  | All issues matching the search term show. |
| **Activate** an issue.                   | The issue opens.                          |

## Find my issue

| Action                                      | Expected result                              |
| ------------------------------------------- | -------------------------------------------- |
| Open the app.                               |                                              |
| **Enter** the **My GitHub Issue** category. | My issues show.                              |
| **Type** a search term.                     | All my issues matching the search term show. |
| **Activate** an issue.                      | The issue opens.                             |

## Find pull request

| Action                                          | Expected result                                  |
| ----------------------------------------------- | ------------------------------------------------ |
| Open the app.                                   |                                                  |
| **Enter** the **GitHub Pull Request** category. | No pull requests show.                           |
| **Type** a search term.                         | All pull requests matching the search term show. |
| **Activate** an pull request.                   | The pull request opens.                          |

## Find my pull request

| Action                                             | Expected result                                     |
| -------------------------------------------------- | --------------------------------------------------- |
| Open the app.                                      |                                                     |
| **Enter** the **My GitHub Pull Request** category. | My pull requests show.                              |
| **Type** a search term.                            | All my pull requests matching the search term show. |
| **Activate** an pull request.                      | The pull request opens.                             |

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

## Find user

| Action                                  | Expected result                          |
| --------------------------------------- | ---------------------------------------- |
| Open the app.                           |                                          |
| **Enter** the **GitHub User** category. | No users show.                           |
| **Type** a search term.                 | All users matching the search term show. |
| **Activate** an user.                   | The users home page opens.               |
