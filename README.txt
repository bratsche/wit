To build you currently need Visual Studio 2008.  If that's a problem, let me
know and we'll try to figure out another build system.

To install this you need to do:
  regasm wit.dll
  gacutil /i wit.dll

You may also need to restart your shell, or your system.

Each time you rebuild it you need to do:
  gacutil /u wit
  gacutil /i wit.dll

You'll need to restart your shell, and possibly your system. :(

And to completely uninstall it:
  gacutil /u wit
  regasm /u wit.dll
