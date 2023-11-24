---
id: welcome
title: Welcome to Techie's Quick Launcher
---

:wave: Hi there! I'm here to help you get setup with Techie's Quick Launcher.

Techie's Quick Launcher supports different tools. I'll walk you through the setup
of the one you want to use. After that we'll explore the functionality of the app.

The app hides itself automatically when you switch to a different app. You can
get the app back by pressing ::{0}::. This by the way is how I'll show
you which keys you can use for something.

You can also get the app back by clicking on the ![](Space+Shuttle.svg) icon in the
lower right corner of your screen, next to the clock and the WiFi symbol. You'll
have to click small arrow (^) to reveal it. When you do, we continue where we left of.

Click **OK** to start.

---
id: select-tool
title: What tool do you want to setup?
---

Before you can use the app, you need to setup a tool. I'll show you how to do this.

Click the tool you want to setup:

---
id: open-configuration-window-to-install
title: Open the configuration window
---

:ok_hand: Excellent. Let's go!

Open the **Configuration** window by clicking the ![](Settings.svg?color=true) icon.

---
id: select-plugins-page
title: Select the plugins page
---

The configuration window is organized into pages. These are shown in
the tree view to the left.

To set you up with a tool, you have to install it first. Select the
**Plugins** page to open the page where you install and remove plugins.

---
id: find-plugin
title: Find the plugin
---

Good, you've found the plugin page. This page shows all plugins supported
by Techie's Quick Launcher. For now we'll just install the **{0}** plugin.

See if you can find the {0} plugin in the list and select it.

---
id: install-plugin
title: Install the plugin
---

Now click **Install** to install the {0} plugin. This will take a moment.

---
id: complete-install-plugin
title: Restart TQL
---

You're doing great. Installation of the plugin has completed successfully
and you can now restart the application.

Click the **Restart** button to restart the app. We'll pick up once the app
has restarted. This may take a moment.

---
id: open-configuration-window-to-configure
title: Open the configuration window
---

:wave: Welcome back. The plugin has now been installed. It's not yet ready for use
though. First you have to set it up with your credentials.

Click on the ![](Settings.svg?color=true) icon again to open the **Configuration** window.

---
id: select-plugin-configuration-page
title: Select the plugins' configuration page
---

If a plugin requires setup, it'll add a configuration page on installation.
In this case, a new configuration page called {0} has been added to the tree
view to the left.

Select the **{0}** configuration page.

---
id: configure-jira
title: Configure the plugin
---

Fantastic work, you've found the JIRA configuration page! You can now use this page to setup
the JIRA plugin.

You can setup connections to multiple JIRA environments. We'll
start with setting up the one you use most. If you have access to more JIRA
environments, you can add them at any time.

You need to give the connection a **Name** first. If you configure more than one
connection, this name is used to distinguish between them when you use the app.

The **URL** is the URL you use to access JIRA, e.g. https://mycompany.atlassian.net.

If you can, you should use an API token to setup access to JIRA. Click
the **Documentation** link if you need help creating one. Once you've created the
API token, paste it into the **Password** field and set the **User name** field
to your email address.

Once you've completed the form, click **Update** to add the connection
and **Save** to commit your changes.

---
id: configure-azure-devops
title: Configure the plugin
---

Fantastic work, you've found the Azure DevOps configuration page! You can now use this page to setup
the Azure DevOps plugin.

You can setup connections to multiple Azure DevOps environments. We'll
start with setting up the one you use most. If you have access to more Azure DevOps
environments, you can add them at any time.

Click the **Add** button to start adding a connection.

---
id: add-connection-azure-devops
title: Add the connection
---

You need to give the connection a **Name** first. If you configure more than one
connection, this name is used to distinguish between them when you use the app.

The **URL** is the URL you use to access Azure DevOps, e.g. https://dev.azure.com/yourcompany.

You get the value for the **PAT Token** field from Azure DevOps. Click the
**Documentation** if you need help creating one.

Once you've completed the form, click **Save** to add the connection.

---
id: added-connection-azure-devops
title: Save your changes
---

Now click **Save** to save your changes.

---
id: configure-github
title: Configure the plugin
---

Fantastic work, you've found the GitHub configuration page! You can now use this page to setup
the GitHub plugin.

You can setup connections to multiple GitHub accounts. We'll
start with setting up the one you use most. If you have access to more GitHub
accounts, you can add them at any time.

For now, you only have to give the connection a name in the **Name** field. If you configure more than one
connection, this name is used to distinguish between them when you use the app.

You'll be asked to enter your credentials in a moment.

Once you've completed the form, click **Update** to add the connection
and **Save** to commit your changes.

---
id: discover-plugin
title: Discover the {0} plugin
---

Well done! With the configuration completed, we can start using the plugin.

Plugins organize their search features in categories. You can list
all categories by typing ::Spacebar:: into the search box.

Give this a try!

---
id: select-category
title: Select a category
---

Great job! This list shows all search types of the {0} plugin. We call
these categories. Let's try the **{1}** category!

Find the item in the list. You may need to scroll down a little
bit to get it into view. Once you've found it, click it to select {1}
as the current category.

---
id: find-match
title: Find a {2}
---

Did you notice that the search box changed? It now shows **{0}**
with the {2} icon. This way you can see that you're searching in a category.

The list shows all {1} that you have access to. The list may be
quite long, so we need a better way to find items than just scrolling
through the list.

Instead, try finding a {2} by typing in part of the name.
You can type in the whole name of the {2}, or part of it.
Just try a few different things.

---
id: activate-match
title: Activate a search result
---

As you type, the list is filtered down to include only the items
that match what you've typed. Once you've found the item you're looking
for, activate it by pressing ::Enter::.

When you activate a search result, the app will close. You open the
app again by pressing ::{0}::. Do this after you activate the
search result to resume the tutorial.

::Enter:: will pick the first item of the list by default. This is the item
that's currently highlighted. If you
want an item lower down in the list, use the ::↑:: and ::↓::
keys to highlight it and then press ::Enter::.

---
id: nested-categories
title: Nested categories
---

{0} {1} are a bit special. Most of the time, search results can
only be activated. Sometimes though, search results themselves are
categories. Go back into the **{2}** category and I'll show you how
this works.

---
id: search-nested-category
title: Nested categories
---

If you look at the highlighted board, you'll find that there
are icons next to it to the far right. These are also shown in the cheat sheet at the
bottom of the window.

The ![](Person+Running.svg?color=true) icon indicates that you can activate the search result.
That's how we opened the board before.

The ![](Apps+List.svg?color=true) icon indicates that the search result is also a category. If you highlight
it and press ::Tab::, you will enter the category.
Try this now.

---
id: search-hint-jira
title: Search hint
---

Nested categories will show a search hint telling you what you
can search for. In this case, you can search for quick filters. If you
activate one, the JIRA board will be opened with that quick
filter selected.

You can also go a level further, into the quick filter, and find
issues matching the quick filter.

You can back out of a category by pressing ::Esc::. Try this now to
go back to the main search results.

---
id: search-hint-azure-devops
title: Search hint
---

Nested categories will show a search hint telling you what you
can search for. In this case, you can search for work items shown
on the board you've selected.

You can back out of a category by pressing ::Esc::. Try this now to
go back to the main search results.

---
id: search-hint-github
title: Search hint
---

Nested categories will show a search hint telling you what you
can search for. In this case, you can search for issues
created for the repository you've selected.

You can back out of a category by pressing ::Esc::. Try this now to
go back to the main search results.

---
id: search-in-history
title: Using the history
---

Let's try something else. When we started out this
list was empty, but not anymore. But by now a few items will have appeared.

Whenever you activate a search result or open a category, it's automatically added your
history. The items you see here come
from there. They're ordered chronologically, so the top item is the one you last used.

You can search in this list the same way as you've searched in categories.
This way you can quickly find things you've found before.

Give this a try. Type in part of the name of the {0}
again. You'll find that you get far fewer search results now.

---
id: pinning-items
title: Pinning items
---

Great job! If you search in your history, the
order of the search results will be influenced by how often
you've used them.

If you find that you want certain items to appear at the top
of the list, you can pin them.

Try this now. Hover your mouse over the **second** item in the list
click the ![](Pin.svg?color=true) icon.

---
id: unpinning-items
title: Unpinning items
---

The item is now at the top of the list instead of one down.

If you hover over the ![](Pin.svg?color=true) icon again, you'll see that it changes to
a the ![](Pin+Off.svg?color=true) icon. Clicking this will unpin the item. Give this a try!

---
id: remove-favorite
title: Removing items from the history
---

Perfect. Lastly, you can also completely remove items from
your history. Items that come from your history will have a ![](Star.svg?color=true)
icon next to them.

If you hover over this, it turns into a ![](Dismiss.svg?color=true)
icon. If you click this, the item disappears from the list
entirely and you will have to go into a category again to find it.

Let's try this out!

---
id: complete-more-tools
title: Good job!
---

:thumbsup: Good job making it this far into the tutorial.

If you want to learn more about the app, have a look at the
online documentation. You can get to the **online documentation**
by clicking the ![](Help.svg?color=true) icon.

You can restart this tutorial at any time. To do this,
open the **Configuration** window by clicking the ![](Settings.svg?color=true)
icon. Then, on the main page click the **Restart Tutorial**
button.

If you want me to help you setup another tool, click on the
one you want below.

Otherwise, I hope you'll have a lot of fun using the app :).

---
id: complete-2nd-more-tools
title: Good job!
---

That's it. If you want me to help you
out setup another tool, click on the one you want me to help with.

---
id: complete
title: Good job!
---

You've setup all tools! You must be a power user :). I hope you
can make good use of the app!
