---
testspace:
title: Plugins / Azure / 003 - Search
description: Verify search features.
---

{% if page %}{% assign spec = page %}{% endif %}

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Run-App.md %}

## Find resource

Find a resource in Azure Portal:

- Open the app.
- Enter the **Azure Portal** category.
- Type in `tql-ai`. At least the **tql-rg/tql-ai** resource appears with a
  purple light bulb icon.
- **Activate** the **tql-rg/tql-ai** resource. The resource opens in Azure
  Portal.

## Find resource group

Find a resource group in Azure Portal:

- Open the app.
- Enter the **Azure Portal** category.
- Type in `tql-rg`. The **tql-rg** resource group appears with a blue cube icon.
- **Activate** the **tql-rg** resource group. The resource group opens in Azure
  Portal.

## Verify history

Verify that the activated items are in the history:

- Open the app.
- Entere the **Azure Portal** category. The **tql-rg/tql-ai** item and
  **tql-rg** item appear in the history.
