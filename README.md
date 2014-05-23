gVerManager
===========

A simple version manager for managing AssemblyInfo.cs's AssemblyVersion field that implemented in C#

_**Only for AssemblyInfo.cs**_


Prerequisites
=============
- .net 3.5

Usage
======

gVerManager.exe -f <filepath> [options]

Version Format
==============

Major.Minor.Build.Revision

Options
==========
	-rm

		Max revision build number (default : 100)

	-bm 

		Max build number (default : 100)

	-ri 

		Increment for revision (default : 1)

	-bi

		Increment for build (default : 0)

Examples
============

gVerManager.exe -f AssemblyInfo.cs"

gVerManager.exe -f AssemblyInfo.cs -rm 200 -bm 2000 -ri 1 -bi 0";
