# Create a plugin

This guide shows how to create a plugin for Techie's Quick Launcher. TQL plugins
are C# class libraries. You need Visual Studio to build one. Have a look at the
[Development environment](Development-environment.md) page for how to setup your
computer.

## Create a new project

1. Open Visual Studio:

   ![=2x](../Images/Open-Visual-Studio.png)

2. Click **Create a new project**, search for "class library" and pick the C#
   version of the **Class Library** template:

   ![=2x](../Images/Search-for-class-library.png)

3. Give your plugin a name, e.g. "TqlNuGetPlugin":

   ![=2x](../Images/Configure-your-new-project.png)

4. In **Framework**, pick the latest framework. TQL plugins must be .NET
   Framework 4.8. We'll change to that in a bit:

   ![=2x](../Images/Set-the-target-framework.png)

   Visual Studio now opens the newly created project. We still need to make a
   few changes to the project file to make sure the plugin is going to work
   properly with TQL.

5. Right click on the project and click **Edit Project File**.

6. Change the project file to the following:

   ```xml
   <Project Sdk="Microsoft.NET.Sdk">
     <PropertyGroup>
       <TargetFramework>net48</TargetFramework>
       <ImplicitUsings>enable</ImplicitUsings>
       <Nullable>enable</Nullable>
       <LangVersion>latest</LangVersion>
       <UseWPF>true</UseWPF>
       <UseWindowsForms>true</UseWindowsForms>
       <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>
     </PropertyGroup>
   </Project>
   ```

   - This makes the following changes to the project:
     - The target framework is .NET 4.8.
     - We use the latest C# language version.
     - WPF and Windows Forms is enabled.
     - NuGet lock files are enabled.

## Add NuGet dependencies

The entry point for TQL into your plugin is the plugin class. This class needs
to implement the `ITqlPlugin` interface from the
[TQLApp.Abstractions](https://www.nuget.org/packages/TQLApp.Abstractions) NuGet
package. TQL publishes two NuGet packages to help you create TQL plugins. The
second is a NuGet package with utility classes. Instead of adding the
abstractions NuGet package directly, you should add the
[TQLApp.Utilities](https://www.nuget.org/packages/TQLApp.Utilities) NuGet
package instead:

1. Right click on the project file and click **Manage NuGet Packages...**.

2. Search for "tqlapp.utilities" and click **Install**:

   ![=2x](../Images/Install-utilities-NuGet-package.png)

## Create the plugin class

TQL plugins must implement the `ITqlPlugin` interface and must specify the
`TqlPluginAttribute` attribute for TQL to pick them up.
