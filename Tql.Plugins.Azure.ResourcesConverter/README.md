# Azure Resources Converter

This tool converts resources from Azure Portal into a format the app understands.
Note that this makes use of internals of the Azure Portal, so will very likely
change in the future.

## Instructions

- Open the Azure Portal and Chrome DevTools and go to the Sources tab.
- Do a global search for `open ai` (different options work, but at the time of
  writing, this open returned just one JSON file). The JSON file should have a
  format with at the top level an object with a "manifests" property, and nested
  two levels down a property called "assetTypes". Copy the contents of this file
  into the Resources.json file.
- Do a global search for `requireconfig\"` (including the backslash and quote).
  This should give you at least two files where the search matched an property
  name `"requireConfig"` or `"fxRequireConfig"`. Copy the full contents of these
  files into the RequireConfig.js file. The algorithm is smart enough to pick up
  the interesting parts from the copied JavaScript.
- Run the app.
