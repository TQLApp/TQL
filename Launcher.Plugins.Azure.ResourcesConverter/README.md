# Azure Resources Converter

This tool converts resources from Azure Portal into a format the app understands.
Note that this makes use of internals of the Azure Portal, so will very likely
change in the future.

## Instructions

- Open the Azure Portal and the Inspection tab of Chrome.
- Open the Elements tab and find a DIV called FxSymbolContainer. Copy the element
  (highlight it and press Ctrl+C) and copy the contents into the FxImages.html file.
  Note however that these images are loaded on the fly. It's best to open the
  All Resources panel and scroll to some of the pages there to seed the cache.
- Open the Sources tab and do a global search for "open ai" (different options work,
  but at the time of writing, this open returned just one JSON file). The JSON file
  should have a format with at the top level an object with a "manifests" property,
  and nested two levels down a property called "assetTypes". Copy the contents of
  this file into the Resources.json file.
- Run the app.
