---
testspace:
title: App / Quick Start / 002 Quick start window features
description: Validate the features of the quick start window.
---

# {{ spec.title }}

{{ spec.description }}

{% include Setup/Clean-Setup.md %}

## [setup]

Restart the quick start tutorial:

- If the quick start window doesn't show:
  - Open the **Configuration** window.
  - Navigate to the **Application | General** configuration page.
  - Click the **Restart Tutorial** button and click **Yes**.
  - Click **Cancel**. The quick start tutorial will now restart.

## Can close

Verify that the quick start window can be closed:

- Click the **Dismiss** icon (the cross). A message box is shown asking you whether you want to close the quick start window.
- Click **Yes**. The quick start window closes.

## Can cancel close

Verify that cancelling closing the quick start window doesn't close it:

- Click the **Dismiss** icon (the cross). A message box is shown asking you whether you want to close the quick start window.
- Click **No**. The quick start window does not close.

## Verify quick start window opens

Verify that the quick start window opens the first time the app starts:

- Open the app. The quick start window must open.
- Verify that you cannot interact with the app and that you cannot close the app without interacting with the quick start window.

## Verify visual appearance

Verify that the quick start window has certain visual features:

- A colored hand emoji shows.
- Keyboard keys, e.g. **Enter**, are rendered distinctly different from other text.
- A keyboard and cursor icons are shown, in the same color as the text.

## Verify behavior of close button

The quick start window uses non standard UI components. Validate that these work:

- Press the mouse down on the **Dismiss** icon, move it off the icon and release it. The window must not move and the message box must not show.

## Verify the window can be moved

Quick start windows that are not locked to a control can be moved (e.g. the first window):

- Hover the mouse over the quick start window, not over a control. A **Grab** mouse cursor appears.
- Hold the left mouse button down and drag the window. The window moves.

## Verify the next and back buttons

Verify that next and previous work:

- Click **Next**. The next quick start window opens.
- Click on the **Arrow left** button in the top left. The first screen opens again.

## Verify choice buttons

Verify that the choice buttons work:

- Click **Next** until you find the list of tools.
- Click **JIRA**. The next window opens.

## Verify anchoring

Verify that the quick start window can be anchored to a control:

- Progress the quick start tutorial until you've picked the **JIRA** tool. The quick start window now has an arrow pointing at the **Cog** icon and the icon has a blue border around it.

## Verify that quick start sticks to windows

Verify that the quick start window sticks to windows when applicable:

- Progress the quick start tutorial until you've opened the configuration window. The quick start window will show.
- Move the configuration window. The quick start window keeps its relative place to the configuration window.
- Move the configuration window to the far side of the screen, the side where the quick start window is at. (If the quick start window opens to the left of the configuration window, move the configuration window to the far left.) The quick start window does not move off of the screen.
- Manually drag the window to a different local.
- Move the configuration window slightly. The quick start window snaps back to its original relative location.

## Verify second monitor behavior

Verify that the quick start window can move to the second monitor:

- Progress the quick start tutorial until you've opened the configuration window. The quick start window will show.
- Move the configuration window to a different monitor. The quick start window moves to the same monitor as the configuration window.

## Verify anchoring on configuration window

Verify that anchoring works as expected in the configuration window:

- Progress the quick start tutorial until the quick start window is anchored to a control in the configuration window, e.g. after you've selected the plugin you're installing.
- Try to move the quick start window. It cannot be moved.
- Move the configuration window:
  - The configuration window can move freely.
  - The quick start window keeps its position relative to the configuration window.
  - The quick start window does not stay on the screen. It moves off of the screen if you move the configuration window, e.g. moving the anchored control off of the screen.
