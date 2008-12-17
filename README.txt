To build you currently need Visual Studio 2008.  If that's a problem, let me
know and we'll try to figure out another build system.

Before building, you need to change one line in Git.cs.  In the method
'RunProcess' it currently sets the environment $HOME to my home
dir in cygwin (\cygwin\home\cody).  You need to change this to wherever
you keep your .gitconfig file.  For bonus points, figure out how to make
this happen automagically. :)

To install this you need to do:
  regasm wit.dll
  gacutil /i wit.dll

You may also need to restart your shell, or your system.

Each time you rebuild it you need to do:
  gacutil /u wit
  gacutil /i wit.dll

You'll need to restart your shell, and possibly your system. :(

If you make changes to things like the Guid or to the registry keys, make
sure you regasm /u with.dll -before- you rebuild the DLL with your changes.
And once you rebuild the DLL, you'll need to regasm and gacutil /i it.
If you need to add regisry keys, use the RegisteredBy attribute.

And to completely uninstall it:
  gacutil /u wit
  regasm /u wit.dll
