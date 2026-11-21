---
testspace:
title: Plugins - YouTrack / 002 - Configuration
description: Configure the YouTrack plugin.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/YouTrack.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

A token is created in YouTrack under Profile, Account Security, New token, with
the YouTrack scope.

## Validate dialog

| Action                              | Expected result                                                                                                                     |
| ----------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------- |
| Open the **Configuration** window.  |                                                                                                                                     |
| Navigate to **YouTrack**.           | The **YouTrack > General** page is highlighted.                                                                                     |
| **Add** a new connection.           | The dialog has the fields **Name**, **URL** and **Token**.                                                                          |
| Look at the help text of **URL**.   | It reads "The address you open YouTrack on, e.g. https://youtrack.example.com or https://example.youtrack.cloud."                   |
| Look at the help text of **Token**. | It says where a token is made: Profile, Account Security, New token, with the YouTrack scope. The token is shown as a password box. |
| **Cancel** the dialog.              | No connection is added.                                                                                                             |

## URL copied from the address bar

| Action                                            | Expected result                                                                          |
| ------------------------------------------------- | ---------------------------------------------------------------------------------------- |
| Open the **Configuration** window.                |                                                                                          |
| Navigate to **YouTrack**.                         |                                                                                          |
| **Add** a new connection with the settings below. |                                                                                          |
| Click **Save**.                                   | The dialog asks "Did you mean {{ youtrack_url }}/?". No exception shows.                 |
| Decline the suggestion.                           | The connection is not saved with the wrong URL; the dialog stays open or closes cleanly. |
| Click **Save** again and accept the suggestion.   | The URL changes to `{{ youtrack_url }}/` and the connection is accepted.                 |
| **Cancel** the dialog.                            |                                                                                          |

- Connection settings:
  - Name: `{{ youtrack_name }}`
  - URL: `{{ youtrack_bad_url }}`
  - Token: `{{ youtrack_token }}`

## Wrong token

| Action                                            | Expected result                                                             |
| ------------------------------------------------- | --------------------------------------------------------------------------- |
| Open the **Configuration** window.                |                                                                             |
| Navigate to **YouTrack**.                         |                                                                             |
| **Add** a new connection with the settings below. |                                                                             |
| Click **Save**.                                   | A plain message says the token is wrong. No exception or stack trace shows. |
| **Cancel** the dialog.                            | No connection is added.                                                     |

- Connection settings:
  - Name: `{{ youtrack_name }}`
  - URL: `{{ youtrack_url }}`
  - Token: `wrong-token`

## Configuration

| Action                                            | Expected result                                  |
| ------------------------------------------------- | ------------------------------------------------ |
| Open the **Configuration** window.                |                                                  |
| Navigate to **YouTrack**.                         | The **YouTrack > General** page is highlighted.  |
| **Add** a new connection with the settings below. |                                                  |
| Click **Save**.                                   | The connection is accepted without any question. |
| Click **Save**.                                   |                                                  |

- Connection settings:
  - Name: `{{ youtrack_name }}`
  - URL: `{{ youtrack_url }}`
  - Token: `{{ youtrack_token }}`

## Validate configuration

| Action                                       | Expected result                                                                   |
| -------------------------------------------- | --------------------------------------------------------------------------------- |
| Open the **Configuration** window.           |                                                                                   |
| Navigate to **YouTrack**.                    | The **YouTrack > General** page is highlighted.                                   |
| Highlight the connection and click **Edit**. | The settings conform to the settings below. The token is not shown in clear text. |

- Connection settings:
  - Name: `{{ youtrack_name }}`
  - URL: `{{ youtrack_url }}/`
  - Token: `{{ youtrack_token }}`

## Validate authentication

| Action                                        | Expected result               |
| --------------------------------------------- | ----------------------------- |
| Open the app.                                 |                               |
| Type in `youtrack`.                           | The YouTrack categories show. |
| **Enter** the **YouTrack Projects** category. | A list of projects shows.     |
