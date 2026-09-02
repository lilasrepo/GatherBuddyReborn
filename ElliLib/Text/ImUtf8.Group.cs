using Group = ElliLib.Text.EndObjects.Group;

namespace ElliLib.Text;

public static partial class ImUtf8
{
    /// <summary> Begin a group and end it on leaving scope. </summary>
    /// <returns> A disposable object. Use with using. </returns>
    public static Group Group()
        => new(true);
}
