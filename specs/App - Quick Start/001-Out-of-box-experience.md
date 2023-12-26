---
testspace:
title: App - Quick Start / 001 - Out-of-box experience
description: Validate the out-of-box experience of the quick start tutorial.
---

{% if page %}{% assign spec = page %}{% endif %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Clean-Setup.md %}

The purpose of this test is to verify that the quick start tutorial opens for
new users. Because if this, you can't use custom reset scripts to complete this
test. The only valid approach is to use the reset shortcut approach described in
[TQL documentation](https://tqlapp.github.io/TQL/Documentation/Running-manual-tests.html).
This means that the app may show in a different language (the Windows default
language), but that's fine. That's not under test here.

## Verify quick start window opens

| Action                        | Expected result                                                                                            |
| ----------------------------- | ---------------------------------------------------------------------------------------------------------- |
| Open the app.                 | The quick start window opens.                                                                              |
| Try to interact with the app. | You cannot interact with the app or close the app without interacting with closing the quick start window. |
