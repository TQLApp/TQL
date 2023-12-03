---
testspace:
title: App / Quick Start / 006 - Application functionality
description:
  Validate that the quick start tutorial helps you understand the applications
  functionality.
---

{% if page %}{% assign spec = page %}{% endif %}
{% include Configuration/Plugins/JIRA.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Clean-Setup.md %}

## Setup the quick start tutorial

{% include Setup/App/QuickStart/Reset-quick-start.md %}

| Action                                                                                  | Expected result                                                          |
| --------------------------------------------------------------------------------------- | ------------------------------------------------------------------------ |
| Open the app.                                                                           | The quick start window must open.                                        |
| Complete the quick start tutorial, picking JIRA and using the settings below.           |                                                                          |
| Progress the tutorial until you get to a step called **Using Techie's Quick Launcher**. | This step shows immediately after you've saved the configuration window. |
| Click **Next**.                                                                         |                                                                          |

- Connection settings:
  - Name: `{{ jira_name }}`
  - URL: `{{ jira_url }}`
  - User name: `{{ jira_user_name }}`
  - Password: `{{ jira_password }}`

## Validate the quick start tutorial

Validate that the quick start tutorial helps you with the following:

- Listing all categories.
- Selecting a category with the arrow keys.
- Entering a category.
- Finding an item in a category.
- Activating an item.
- Enter a nested category.
- Understand that there's a search hint.
- Navigate out of a nested category.
- Find items in the history.
- Pinning items.
- Unpinning items.
- Removing items from the history.

## Closing the tutorial

| Action                               | Expected result                                             |
| ------------------------------------ | ----------------------------------------------------------- |
| Complete all steps in the tutorial.  | The completion step shows with a **Close Tutorial** button. |
| Click the **Close Tutorial** button. | The tutorial closes.                                        |
