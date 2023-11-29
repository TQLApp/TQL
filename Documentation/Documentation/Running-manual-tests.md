# Running manual tests

The manual regression test suite expects a certain setup on your machine. This page describes how to prepare this setup.

## Setup

1. Ensure that the latest version of TQL is installed.

Create a shortcut to a dedicated manual testing environment:

1. Create a new Windows Shortcut, e.g. on your desktop.

2. Enter the following location:

   ```batch
   %LOCALAPPDATA%\Programs\TQL\Tql.App.exe --env ManualTesting
   ```

3. Enter the following name:

   **TQL - Manual Testing**

4. Complete the wizard.

Create a shortcut to reset the manual testing environment:

1. Create a new Windows Shortcut, e.g. on your desktop.

2. Enter the following location:

   ```batch
   %LOCALAPPDATA%\Programs\TQL\Tql.App.exe --env ManualTesting --reset
   ```

3. Enter the following name:

   **TQL - Manual Testing - Reset**

4. Complete the wizard.

## Usage

When you execute a test suite, run the **TQL - Manual Testing** shortcut to start TQL with a dedicated environment.

If you require the dedicated environment to be reset, use the **TQL - Manual Testing - Reset** shortcut. This will reset the following:

- All configuration.
- All data (including your history).
- All installed plugins.
