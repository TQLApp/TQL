---
testspace:
title: Plugins - YouTrack / 004 - Second connection
description: Configure a second connection.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/YouTrack.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Create connection

| Action                                            | Expected result                                 |
| ------------------------------------------------- | ----------------------------------------------- |
| Open the **Configuration** window.                |                                                 |
| Navigate to **YouTrack**.                         | The **YouTrack > General** page is highlighted. |
| **Add** a new connection with the settings below. |                                                 |
| Click **Save**.                                   |                                                 |
| Click **Save**.                                   |                                                 |

- Connection settings:
  - Name: `{{ youtrack_alt_name }}`
  - URL: `{{ youtrack_url }}`
  - Token: `{{ youtrack_token }}`

## Verify multiple connections

| Action                       | Expected result                                                                                                         |
| ---------------------------- | ----------------------------------------------------------------------------------------------------------------------- |
| Open the app.                |                                                                                                                         |
| Type in `youtrack projects`. | One **YouTrack Projects ({{ youtrack_name }})** item and one **YouTrack Projects ({{ youtrack_alt_name }})** item show. |
| Type in `youtrack issues`.   | Both an **YouTrack Issues ({{ youtrack_name }})** and an **YouTrack Issues ({{ youtrack_alt_name }})** item show.       |
| Type in `youtrack agile`.    | The agile boards categories carry the connection names too.                                                             |
| Type in `youtrack saved`.    | The saved searches categories carry the connection names too.                                                           |
| Type in `youtrack dash`.     | The dashboards categories carry the connection names too.                                                               |
| Type in `new youtrack`.      | The **New YouTrack Issue** categories carry the connection names too.                                                   |
