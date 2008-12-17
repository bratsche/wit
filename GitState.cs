using System;

namespace Wit
{
    [Flags]
    public enum GitState
    {
        GitNotFound         = 0x00000001,
        InGitDirectory      = 0x00000002
    }
}