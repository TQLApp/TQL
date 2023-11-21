# Localize TQL

There are two sets of resources that need to be localized. The main source of
translations is resource dictionaries. The **Tql.Localization** project can
export and import all localization strings from all projects in the TQL solution
into an Excel file, and read it back in again.

There are two launch configurations for this project that are setup to do this
for Dutch translations. If you want to translate TQL into a different language,
change the locale of the **Export** and **Import** launch configuration to the
language you want to translate TQL into, and use the localization tool to export
and import localized strings.

The quick start tutorial uses a different mechanism to localize the bulk of the
text. There's a file called Playbook.md in the QuickStart folder in the Tql.App
project. To localize the quick start tutorial into a new language, you have to
copy the **Playbook.md** file to **Playbook.&lt;locale&gt;.md** (e.g.
Playbook.nl.md).

When running TQL in debug mode, changes to the playbook file are reloaded into
the app in real time. You can translate the playbook into your locale while
going through the quick start tutorial. This makes it a lot easier to see if the
text flows well and the translations look right.

After you've translated TQL into a new locale, please create a pull request with
your changes. I'll be happy to accept it. Please let me know in the PR whether
you'd like to be notified when TQL needs to be translated again.
