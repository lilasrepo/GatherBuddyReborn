using FFXIVClientStructs.FFXIV.Client.Game.Event;

namespace GatherBuddy.SeFunctions;

public sealed unsafe class EventFramework
{
    public FishingEventHandler* FishingEventHandler
        => (FishingEventHandler*)FFXIVClientStructs.FFXIV.Client.Game.Event.EventFramework.Instance()->EventHandlerModule.FishingEventHandler;

    public uint? CurrentSwimBait
    {
        get
        {
            var ptr = FishingEventHandler;
            if (ptr == null)
                return null;

            return ptr->CurrentSelectedSwimBait switch
            {
                0x00 when ptr->SwimBaitId1 != 0 => ptr->SwimBaitId1,
                0x01 when ptr->SwimBaitId2 != 0 => ptr->SwimBaitId2,
                0x02 when ptr->SwimBaitId3 != 0 => ptr->SwimBaitId3,
                _                                      => null,
            };
        }
    }

    public uint? SwimBait(int idx)
    {
        var ptr = FishingEventHandler;
        if (ptr == null)
            return null;

        return idx switch
        {
            0x00 when ptr->SwimBaitId1 != 0 => ptr->SwimBaitId1,
            0x01 when ptr->SwimBaitId2 != 0 => ptr->SwimBaitId2,
            0x02 when ptr->SwimBaitId3 != 0 => ptr->SwimBaitId3,
            _                                      => null,
        };
    }


    public int NumSwimBait
    {
        get
        {
            var ptr = FishingEventHandler;
            if (ptr == null)
                return 0;

            return (ptr->SwimBaitId1 != 0 ? 1 : 0)
              + (ptr->SwimBaitId2 != 0 ? 1 : 0)
              + (ptr->SwimBaitId3 != 0 ? 1 : 0);
        }
    }

    public FishingState FishingState
    {
        get
        {
            var ptr = FishingEventHandler;
            return ptr != null ? ptr->State : FishingState.NotFishing;
        }
    }
}
