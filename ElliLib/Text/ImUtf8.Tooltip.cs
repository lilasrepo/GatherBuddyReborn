using ElliLib.Text.EndObjects;

namespace ElliLib.Text;

public static partial class ImUtf8
{
    /// <summary> Begin a tooltip and end it on leaving scope. </summary>
    /// <returns> A disposable object. Use with using. </returns>
    public static Tooltip Tooltip()
        => new(true);
}
