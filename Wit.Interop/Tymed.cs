using System;

namespace Wit.Interop
{
    public enum Tymed : uint
    {
        NULL        = 0,
        HGlobal     = 1,
        File        = 2,
        IStream     = 4,
        IStorage    = 8,
        GDI         = 16,
        MfPict      = 32,
        EnhMF       = 64
    }
}