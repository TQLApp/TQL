# Creating `Header.sfx`

`Header.sfx` has been created as follows:

- Get `7zSD.sfx` from the `bin` folder from the LZMA SDK from the 7zip download
  site at https://www.7-zip.org/download.html.
- Get ResourceHacker from https://angusj.com/resourcehacker/ and use it to
  replace the icon with the icon in this project.
- Then, use the same tool to add a manifest. Complete the following steps
  to do this:
  - Open the menu option Action | Add using Script Template.
  - Choose the `MANIFEST` template.
  - Paste the following text over the template:

```
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<assembly xmlns="urn:schemas-microsoft-com:asm.v1" manifestVersion="1.0">
    <trustInfo xmlns="urn:schemas-microsoft-com:asm.v3">
        <security>
            <requestedPrivileges>
                <requestedExecutionLevel level="asInvoker" uiAccess="false"/>
            </requestedPrivileges>
        </security>
    </trustInfo>
</assembly>

```

  - Open the menu option Action | Compile Script.
- Save the header.
