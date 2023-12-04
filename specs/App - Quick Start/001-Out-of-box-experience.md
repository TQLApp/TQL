---
testspace:
title: App - Quick Start / 001 - Out-of-box experience
description: Validate the out-of-box experience of the quick start tutorial.
---

{% if page %}{% assign spec = page %}{% endif %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Clean-Setup.md %}

## Verify quick start window opens

| Action                        | Expected result                                                                                            |
| ----------------------------- | ---------------------------------------------------------------------------------------------------------- |
| Open the app.                 | The quick start window opens.                                                                              |
| Try to interact with the app. | You cannot interact with the app or close the app without interacting with closing the quick start window. |
