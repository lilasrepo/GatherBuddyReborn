// API12 forward-port gap-fills for walk-back OtterGui (TC_ok era) FileSystem<T>:
// (1) TryGetValue(T data, out Leaf leaf)        — newer code does data-instance lookup
// (2) IEnumerable<KeyValuePair<T, Leaf>>        — newer code uses _fileSystem.Select(kvp => ...)
// Both walk the folder tree in DFS order.

using System.Collections;
using System.Collections.Generic;

namespace OtterGui.Filesystem;

public partial class FileSystem<T> : IEnumerable<KeyValuePair<T, FileSystem<T>.Leaf>> where T : class
{
    public bool TryGetValue(T data, out Leaf leaf)
    {
        return TryGetValueImpl(Root, data, out leaf!);
    }

    private static bool TryGetValueImpl(Folder folder, T data, out Leaf? leaf)
    {
        foreach (var child in folder.Children)
        {
            if (child is Leaf l && ReferenceEquals(l.Value, data))
            {
                leaf = l;
                return true;
            }
            if (child is Folder sub && TryGetValueImpl(sub, data, out leaf))
                return true;
        }
        leaf = null;
        return false;
    }

    public IEnumerator<KeyValuePair<T, Leaf>> GetEnumerator()
    {
        foreach (var leaf in EnumerateLeaves(Root))
            yield return new KeyValuePair<T, Leaf>(leaf.Value, leaf);
    }

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    private static IEnumerable<Leaf> EnumerateLeaves(Folder folder)
    {
        foreach (var child in folder.Children)
        {
            if (child is Leaf l)
                yield return l;
            else if (child is Folder sub)
                foreach (var inner in EnumerateLeaves(sub))
                    yield return inner;
        }
    }
}
