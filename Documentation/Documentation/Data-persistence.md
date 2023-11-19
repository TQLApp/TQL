# Data persistence

This page describes all pages where TQL stores data on the users machine. When
the name contains the `ENV` environment variable, it's replaced by the name of
the environment, or stripped. The default folder name is called "TQL" and the
folder for e.g. the Debug environment is called "TQL - Debug".

# File storage

The installer installs binaries into `%LOCALAPPDATA%\Programs\TQL`. This
location is not edited by the app at runtime and is wholly owned by the
installer.

The app manages two sets of files:

- `%LOCALAPPDATA%\TQL - %ENV%`: Stores binaries and cache data.
- `%APPDATA%\TQL - %ENV%`: Stores user data like configuration and connection
  details.

There's an outstanding issue [#31](https://github.com/TQLApp/TQL/issues/31) to
support roaming profiles.

# Registry storage

All registry data is stored under
`Computer\HKEY_CURRENT_USER\Software\TQL - %ENV%`. The installer also stores
some data under the registry key owned by the primary environment. This means
that this environment key has shared ownership between the app and the
installer.
