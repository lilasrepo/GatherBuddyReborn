using GatherBuddy.Utility;
using Lumina.Excel.Sheets;

namespace GatherBuddy.Classes;

public class CosmicMission
{
    public ushort Id
        => (ushort)Data.RowId;

    public WKSMissionUnit Data;
    public string         Name;

    public CosmicMission(WKSMissionUnit data)
    {
        Data = data;
        // API12 stub: WKSMissionUnit.Name is a game-7.5 Cosmic Exploration sheet column
        // not present in TC client's Lumina. Empty name; missions still indexed by RowId.
        Name = string.Empty;
    }
}
