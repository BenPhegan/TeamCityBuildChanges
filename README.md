TeamCityBuildChanges
====================

Provides a command line utility to query changes associated with TeamCity builds.  Useful to create dynamic build/release notes as part of a TeamCity build.  Think pretty, Razor-templated release notes automagically.

Features
=======

*  Queries TeamCity, Jira and TFS to provide a consolidated view of change across a set of builds.
*  Can retrieve issue details from TeamCity plugins (Jira or TFS) or can be configured to go directly to the source based on commit details.
*  Provides NuGet package dependency change information for build ranges.
*  Inbuilt Razor Engine allows all reporting to use a Razor template via a simple ChangeManifest Model object.
