using System;
using Dalamud.Game;

namespace GatherBuddy.SeFunctions;

public delegate void UpdateCatchDelegate(IntPtr module, uint fishId, bool large, ushort size, byte amount, byte level, byte unk7, byte unk8, byte unk9, byte unk10,
    byte unk11, byte unk12);

public sealed class UpdateFishCatch : SeFunctionBase<UpdateCatchDelegate>
{
    public UpdateFishCatch(ISigScanner sigScanner)
        // C-fix(7.3): the TC_ok game-7.1 value (`40 55 56 41 54 41 56 41 57 48 8D 6C 24 ?? ...`, itself from
        // upstream 899c3c6f 2024-06-29) threw KeyNotFoundException on TC game v7.20 and failed the whole
        // plugin ctor (runtime-observed 2026-07-28, FishingParser.cs line 27). Value below is upstream's own
        // aa8e2d83 "Initial update for 7.3" (2025-08-07) and is still HEAD's, so it is also the last
        // pre-7.4 value. AutoHook/Fishing/FishingManager.cs hooks the same function -- kept in sync there.
        : base(sigScanner, "48 89 6C 24 ?? 56 41 56 41 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 48 8B 01")
    {}
}
