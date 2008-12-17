using System;

namespace Wit.Interop
{
    public enum GetCommandStringFlags : uint
    {
        VerbA       = 0x00000000,     // canonical verb
        HelpTextA   = 0x00000001,     // help text (for status bar)
        ValidateA   = 0x00000002,     // validate command exists
        VerbW       = 0x00000004,     // canonical verb (unicode)
        HelpTextW   = 0x00000005,     // help text (unicode version)
        ValidateW   = 0x00000006,     // validate command exists (unicode)
        Unicode     = 0x00000004,     // for bit testing - Unicode string
        Verb        = GetCommandStringFlags.VerbA,
        HelpText    = GetCommandStringFlags.HelpTextA,
        Validate    = GetCommandStringFlags.ValidateA
    }
}