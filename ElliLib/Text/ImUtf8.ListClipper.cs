using ElliLib.Text.EndObjects;

namespace ElliLib.Text;

public static partial class ImUtf8
{
    public static ListClipper ListClipper(int itemsCount, float itemsHeight = -1f)
        => new(itemsCount, itemsHeight);
}
