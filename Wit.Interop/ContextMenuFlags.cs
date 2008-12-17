using System;

namespace Wit.Interop
{
    public enum ContextMenuFlags : uint
    {
        Normal          = 0x00000000,
        DefaultOnly     = 0x00000001,
        VerbsOnly       = 0x00000002,
        Explore         = 0x00000004,
        NoVerbs         = 0x00000008,
        CanRename       = 0x00000010,
        NoDefault       = 0x00000020,
        IncludeStatic   = 0x00000040,
        Reserved        = 0xffff0000      // View specific
    }
}