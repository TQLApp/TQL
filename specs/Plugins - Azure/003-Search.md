---
testspace:
title: Plugins - Azure / 003 - Search
description: Verify search features.
---

{% if page %}{% assign spec = page %}{% endif %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Find resource

| Action                                       | Expected result                                                              |
| -------------------------------------------- | ---------------------------------------------------------------------------- |
| Open the app.                                |                                                                              |
| **Enter** the **Azure Portal** category.     |                                                                              |
| **Type** in `tql-ai`.                        | At least the **tql-rg/tql-ai** resource shows with a purple light bulb icon. |
| **Activate** the **tql-rg/tql-ai** resource. | The resource opens.                                                          |

## Find resource group

| Action                                      | Expected result                                            |
| ------------------------------------------- | ---------------------------------------------------------- |
| Open the app.                               |                                                            |
| **Enter** the **Azure Portal** category.    |                                                            |
| **Type** in `tql-rg`.                       | The **tql-rg** resource group shows with a blue cube icon. |
| **Activate** the **tql-rg** resource group. | The resource group opens.                                  |

## Verify history

| Action                                   | Expected result                                                     |
| ---------------------------------------- | ------------------------------------------------------------------- |
| Open the app.                            |                                                                     |
| **Enter** the **Azure Portal** category. | The **tql-rg/tql-ai** item and **tql-rg** item show in the history. |
