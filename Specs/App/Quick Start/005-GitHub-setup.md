---
testspace:
title: App / Quick Start / 005 - GitHub setup
description: Validate that the quick start tutorial will set you up with GitHub.
---

{% include Configuration/Plugins/GitHub.md %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Clean-Setup.md %}

## Restar quick start

{% include Setup/App/QuickStart/Reset-quick-start.md %}

## Successfully setup GitHub

Verify that the quick start tutorial will set you up with GitHub:

- Open the app. The quick start window must open.
- Progress the tutorial until you get to pick the tool.
- Pick the **GitHub** tool. The tutorial proceeds to help you setup with GitHub.
- Progress the tutorial until you've installed the **GitHub** plugin and restart
  the app. The app restarts.
- Progress the tutorial to setup a connection. Uset the following values to set
  up the connection:
  - Name: `{{ github_name }}`
- Progress the tutorial until you get to a step called **Using Techie's Quick
  Launcher**. This step shows immediately after you've saved the configuration
  window. **Dismiss** the quick start tutorial.
- Search for `GitHub Repository`. A category with the name **GitHub Repository**
  shows.
