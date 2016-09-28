# ServerBackup
Initial commit of public ServerBackup project.   Still a work in progress.

## Description
Serverbackup is a utility to typically copy files from one location to another.  It can be run from the command line, but is usually run as a service which is configured to execute the copy periodically (usually daily).  In a more general form, Serverbackup executes *commands* either via command line or periodically as a service upon a *source* to a *destination*.  

#### Commands
The basic commands implemented are copy, delete, zip, hash, and simulate.  Future commands to be implemented are 7z, verify, and perhaps others.

#### File Selection
To select files upon which to execute commands, a file selector needs to be used.  Two file selectors are implemented, match by regular expression, and match by time.   These two can be combined.

A basic usage would be `serverbackup.exe copy c:\source\ d:\destination\ -include .*txt -newerthan 30` which copies all files that end in `.txt` and are also less than 30 days old to the destination.
