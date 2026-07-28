using System;
using Dalamud.Game;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

namespace GatherBuddy.Utility;

public readonly struct MultiString(string en, string de, string fr, string jp, string chs)
{
    public static string ParseSeStringLumina(ReadOnlySeString? luminaString)
        => luminaString?.ExtractText() ?? string.Empty;

    public readonly string English = en;
    public readonly string German = de;
    public readonly string French = fr;
    public readonly string Japanese = jp;
    public readonly string ChineseSimplified = chs;

    public string this[ClientLanguage lang]
        => Name(lang);

    public override string ToString()
        => Name((ClientLanguage)4);

    public string ToWholeString()
        => $"{English}|{German}|{French}|{Japanese}|{ChineseSimplified}";

    public static MultiString FromPlaceName(IDataManager gameData, uint id)
    {
        var en = ParseSeStringLumina(gameData.GetExcelSheet<PlaceName>(ClientLanguage.English).GetRowOrDefault(id)?.Name);
        var de = ParseSeStringLumina(gameData.GetExcelSheet<PlaceName>(ClientLanguage.German).GetRowOrDefault(id)?.Name);
        var fr = ParseSeStringLumina(gameData.GetExcelSheet<PlaceName>(ClientLanguage.French).GetRowOrDefault(id)?.Name);
        var jp = ParseSeStringLumina(gameData.GetExcelSheet<PlaceName>(ClientLanguage.Japanese).GetRowOrDefault(id)?.Name);
        var chs = ParseSeStringLumina(gameData.GetExcelSheet<PlaceName>((ClientLanguage)4).GetRowOrDefault(id)?.Name);
        return new MultiString(en, de, fr, jp, chs);
    }

    public static MultiString FromItem(IDataManager gameData, uint id)
    {
        var en = ParseSeStringLumina(gameData.GetExcelSheet<Item>(ClientLanguage.English).GetRowOrDefault(id)?.Name);
        var de = ParseSeStringLumina(gameData.GetExcelSheet<Item>(ClientLanguage.German).GetRowOrDefault(id)?.Name);
        var fr = ParseSeStringLumina(gameData.GetExcelSheet<Item>(ClientLanguage.French).GetRowOrDefault(id)?.Name);
        var jp = ParseSeStringLumina(gameData.GetExcelSheet<Item>(ClientLanguage.Japanese).GetRowOrDefault(id)?.Name);
        var chs = ParseSeStringLumina(gameData.GetExcelSheet<Item>((ClientLanguage)4).GetRowOrDefault(id)?.Name);
        return new MultiString(en, de, fr, jp, chs);
    }

    // porting-note(api13): api13's ClientLanguage grew Korean=6 and TraditionalChinese=7 on top of
    // ChineseSimplified=4 / ChineseTraditional=5, and the TC client now reports TraditionalChinese
    // where api12 reported ChineseSimplified. That fell straight into the old `throw`, which killed
    // GatherBuddy at construction (Identificator.CreateGatherableDictionary -> ArgumentException).
    // All Chinese/Korean values map to the single non-Western slot the sheets were actually read
    // with, and an unrecognised future value falls back to English instead of throwing -- a name
    // lookup must never be able to fail plugin load.
    // RUNTIME-VERIFY: the From* helpers still read that slot with the literal (ClientLanguage)4,
    // which is the value proven to work on the TC client under api12. If in-game names come out
    // empty or English, switch those reads to the client's own IDataManager.Language.
    private string Name(ClientLanguage lang)
        => lang switch
        {
            ClientLanguage.English  => English,
            ClientLanguage.German   => German,
            ClientLanguage.Japanese => Japanese,
            ClientLanguage.French   => French,
            ClientLanguage.ChineseSimplified or ClientLanguage.ChineseTraditional
                or ClientLanguage.Korean or ClientLanguage.TraditionalChinese => ChineseSimplified,
            _                       => English,
        };

    public static readonly MultiString Empty = new(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
}
