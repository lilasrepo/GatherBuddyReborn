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
        // porting-note(api13): the api12 stub blanked this because that Lumina only exposed
        // WKSMissionUnit.Unknown0..20. api13's Lumina.Excel (7.3.1) names the columns, so this is
        // upstream's own line again.
        Name = MultiString.ParseSeStringLumina(Data.Name);
    }
}
