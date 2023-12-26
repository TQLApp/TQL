---
id: welcome-instructions
title: Welcome to Techie's Quick Launcher
---

:wave: Hi there! I'm here to help you use Techie's Quick Launcher.

In this tutorial, I'll show you how to use the app.

When you need to use the keyboard, I'll put a ![](Keyboard.svg?color=true) icon
in front of the instructions. I'll indicate keys by putting a box around the
text, e.g. ::Enter:: for the enter key. If you need to click something, I'll use
the ![](Cursor+Click.svg?color=true) icon and if you need to do something else
with the mouse like point at something, I'll use the ![](Cursor.svg?color=true)
icon.

---
id: welcome-scope
title: What we'll be doing
---

The app has support for different tools. I'll walk you through the setup of the
one you want to use. After that we'll explore the functionality of the app.

---
id: welcome-open-app
title: Opening the app
---

The app hides itself automatically when you switch to a different app. You can
get the app back by pressing ::{0}::.

You can also open the app by clicking on the ![](Space+Shuttle.svg) icon in the
lower right corner of your screen, next to the clock and the WiFi symbol. You'll
have to click small arrow (^) to reveal it. When you do, we'll continue where we
left of.

---
id: select-tool
title: What tool do you want to setup?
---

Before you can use the app, you need to setup a tool. I'll show you how to do
this.

Click the tool you want to setup:

---
id: open-configuration-window-to-install
title: Open the configuration window
---

:ok_hand: Excellent. Let's go!

![](Cursor+Click.svg?color=true) Click the ![](Settings.svg?color=true) icon to
open the **Configuration** window.

---
id: select-plugins-page
title: Select the plugins page
---

The configuration window is organized into pages. These are shown in the tree
view to the left.

To set you up with a tool, you have to install it first.

![](Cursor+Click.svg?color=true) Select the **Plugins** page to open the page
where you install and remove plugins.

---
id: find-plugin
title: Find the plugin
---

Good, you've found the plugin page. This page shows all plugins supported by
Techie's Quick Launcher. For now we'll just install the **{0}** plugin.

![](Cursor+Click.svg?color=true) Find the {0} plugin in the list and select it.

---
id: install-plugin
title: Install the plugin
---

![](Cursor+Click.svg?color=true) Now click **Install** to install the {0}
plugin. This will take a moment.

---
id: complete-install-plugin
title: Restart TQL
---

You're doing great. Installation of the plugin has completed successfully and
you can now restart the application.

![](Cursor+Click.svg?color=true) Click the **Restart** button to restart the
app. We'll pick up once the app has restarted. This may take a moment.

---
id: open-configuration-window-to-configure
title: Open the configuration window
---

:wave: Welcome back. The plugin has now been installed. It's not yet ready for
use though. First you have to set it up with your credentials.

![](Cursor+Click.svg?color=true) Click on the ![](Settings.svg?color=true) icon
again to open the **Configuration** window.

---
id: select-plugin-configuration-page
title: Select the plugins' configuration page
---

If a plugin requires setup, it'll add a configuration page on installation. In
this case, a new configuration page called {0} has been added to the tree view
to the left.

![](Cursor+Click.svg?color=true) Select the **{0}** configuration page.

---
id: configure-plugin
title: Configure the plugin
---

Fantastic work, you've found the {0} configuration page! You can now use this
page to setup the {0} plugin.

You can setup connections to multiple {0} environments. We'll start with setting
up the one you use most. If you have access to more {0} environments, you can
add them at any time.

![](Cursor+Click.svg?color=true) Click the **Add** button to start adding a
connection.

---
id: add-connection-jira
title: Configure the plugin
---

You need to give the connection a **Name** first. If you configure more than one
connection, this name is used to distinguish between them when you use the app.

The **URL** is the URL you use to access JIRA, e.g.
https://mycompany.atlassian.net.

If you can, you should use an API token to setup access to JIRA. Click the
**Documentation** link if you need help creating one. Once you've created the
API token, paste it into the **Password** field and set the **User name** field
to your email address.

![](Cursor+Click.svg?color=true) Once you've completed the form, click **Save**
to add the connection.

---
id: add-connection-azure-devops
title: Add the connection
---

You need to give the connection a **Name** first. If you configure more than one
connection, this name is used to distinguish between them when you use the app.

The **URL** is the URL you use to access Azure DevOps, e.g.
https://dev.azure.com/yourcompany.

You get the value for the **PAT Token** field from Azure DevOps. Click the
**Documentation** if you need help creating one.

![](Cursor+Click.svg?color=true) Once you've completed the form, click **Save**
to add the connection.

---
id: configure-github
title: Configure the plugin
---

For now, you only have to give the connection a name in the **Name** field. If
you configure more than one connection, this name is used to distinguish between
them when you use the app.

You'll be asked to enter your credentials in a moment.

![](Cursor+Click.svg?color=true) Once you've completed the form, click **Save**
to add the connection.

---
id: added-connection
title: Save your changes
---

Great, the connection has been added to the list.

![](Cursor+Click.svg?color=true) Now click **Save** to save your changes.

---
id: using-the-app
title: Using Techie's Quick Launcher
---

Well done! With the configuration completed, we can start using the plugin.

The app works best if you use the keyboard. We'll practice this in the remainder
of the tutorial.

---
id: list-all-categories
title: List all categories
---

Plugins organize their search features in categories. You can list these.

![](Keyboard.svg?color=true) Press ::Spacebar:: to list all available
categories.

---
id: select-category
title: Select a category
---

Great job! This list shows all search types of the {0} plugin. We call these
categories. Let's try the **{1}** category!

![](Keyboard.svg?color=true) Use the ::↑:: and ::↓:: keys to highlight the
category.

---
id: enter-category
title: Enter a category
---

![](Keyboard.svg?color=true) Now press ::Tab:: to enter the **{0}** category.

---
id: find-match
title: Find a {2}
---

Did you notice that the search box changed? It now shows **{0}** with the {2}
icon. This way you can see that you're searching in a category.

The list shows all {1} that you have access to. The list may be quite long, so
we need a better way to find items than using the arrow keys. We'll use the
search function instead.

![](Keyboard.svg?color=true) Type in the name of a {2}.

---
id: activate-match
title: Activate a search result
---

As you type, the list is filtered to include only the items that match what
you've typed.

![](Keyboard.svg?color=true) Use the ::↑:: and ::↓:: keys to highlight the item
you want to open.

![](Keyboard.svg?color=true) Press ::Enter:: to activate the selected item.

The app will close when you activate an item.

![](Keyboard.svg?color=true) Press ::{0}:: to open the app again.

---
id: nested-categories
title: Nested categories
---

We'll now look at the {0} category. This is a nested category. I'll show you
what I mean.

![](Keyboard.svg?color=true) Press ::Spacebar:: again to show the list of
categories.

![](Keyboard.svg?color=true) Use the ::↑:: and ::↓:: keys to highlight the
**{0}** category and press ::Tab:: to enter it.

---
id: search-nested-category
title: Nested categories
---

If you look at the highlighted board, you'll find that there are icons to the
right of it. These are also shown in the cheat sheet at the bottom of the
window.

Search results are identified by the ![](Person+Running.svg?color=true) icon,
and categories by the ![](Apps+List.svg?color=true) icon.

![](Keyboard.svg?color=true) Press ::Tab:: to enter the highlighted category.

---
id: search-hint-jira
title: Search hint
---

Categories show a search hint explaining what you can search for. In this case,
you can search for quick filters. If you activate one, the JIRA board will be
opened with that quick filter selected.

You can also go a level further, into the quick filter, and find issues matching
the quick filter.

![](Keyboard.svg?color=true) Press ::Esc:: twice to go back to the main search
results.

---
id: search-hint-azure-devops
title: Search hint
---

Categories show a search hint explaining what you can search for. In this case,
you can search for work items shown on the board you've selected.

![](Keyboard.svg?color=true) Press ::Esc:: twice to go back to the main search
results.

---
id: search-hint-github
title: Search hint
---

Categories show a search hint explaining what you can search for. In this case,
you can search for issues created for the repository you've selected.

![](Keyboard.svg?color=true) Press ::Esc:: twice to go back to the main search
results.

---
id: search-in-history
title: Using the history
---

Let's try something else. When we started out this list was empty, but not
anymore. By now a few items are shown.

Whenever you use a search result or category, it's automatically added to your
history. These are shown in the list.

You can search in this list the same way you search in categories. This way you
can quickly find things you've found before.

![](Keyboard.svg?color=true) Type in part of the name of the **{0}** again.

You'll find that you get far fewer search results now.

---
id: pinning-items
title: Pinning items
---

Great job! If you search in your history, the order of the search results will
be influenced by how often you've used them.

If you find that you want certain items to appear at the top of the list, you
can pin them.

![](Cursor.svg?color=true) Hover your mouse over the **second** item in the
list.

![](Cursor+Click.svg?color=true) Click the ![](Pin.svg?color=true) icon.

---
id: unpinning-items
title: Unpinning items
---

The item is now at the top of the list instead of one down.

![](Cursor.svg?color=true) Hover over the ![](Pin.svg?color=true) icon of the
**first** item.

![](Cursor+Click.svg?color=true) Click the ![](Pin+Off.svg?color=true) icon to
unpin the item.

---
id: remove-favorite
title: Removing items from the history
---

Perfect. Lastly, you can also completely remove items from your history. Items
that come from your history will have a ![](Star.svg?color=true) icon next to
them.

If you hover over this, it turns into a ![](Dismiss.svg?color=true) icon. If you
click this, the item disappears from the list entirely and you will have to go
into a category again to find it.

![](Cursor+Click.svg?color=true) Click the ![](Dismiss.svg?color=true) icon.

---
id: complete-more-tools
title: Good job!
---

:thumbsup: Good job making it this far into the tutorial.

If you want to learn more about the app, have a look at the online
documentation. You can get to the **online documentation** by clicking the
![](Help.svg?color=true) icon.

You can restart this tutorial at any time. To do this, open the
**Configuration** window by clicking the ![](Settings.svg?color=true) icon.
Then, on the main page click the **Restart Tutorial** button.

If you want me to help you setup another tool, click on the one you want below.

Otherwise, I hope you'll have a lot of fun using the app :).

---
id: complete-2nd-more-tools
title: Good job!
---

That's it. If you want me to help you out setup another tool, click on the one
you want me to help with.

---
id: complete
title: Good job!
---

You've setup all tools! You must be a power user :). I hope you can make good
use of the app!
