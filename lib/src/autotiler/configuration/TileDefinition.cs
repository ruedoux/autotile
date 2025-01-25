using System.Numerics;

namespace Qwaitumin.AutoTile;

public record class TileDefinition(
    uint Layer = 0,
    string ImageFileName = "<NONE>",
    Vector2 PositionInSet = default,
    string BitmaskName = "<NONE>",
    int AutoTileGroup = 0,
    Dictionary<string, Dictionary<byte, Vector2>>? BitmaskOverrides = null)
{
  public virtual bool Equals(TileDefinition? other)
  {
    if (other is null) return false;
    if (ReferenceEquals(this, other)) return true;

    return Layer == other.Layer &&
           ImageFileName == other.ImageFileName &&
           PositionInSet.Equals(other.PositionInSet) &&
           BitmaskName == other.BitmaskName &&
           AutoTileGroup == other.AutoTileGroup &&
           CompareDictionaries(BitmaskOverrides, other.BitmaskOverrides);
  }

  public override int GetHashCode()
    => HashCode.Combine(
      Layer,
      ImageFileName,
      PositionInSet,
      BitmaskName,
      AutoTileGroup, ComputeDictionaryHash(BitmaskOverrides));

  private static bool CompareDictionaries(
      Dictionary<string, Dictionary<byte, Vector2>>? first,
      Dictionary<string, Dictionary<byte, Vector2>>? second)
  {
    if (first == null || second == null)
      return first == second;

    if (first.Count != second.Count)
      return false;

    foreach (var pair in first)
      if (!second.TryGetValue(pair.Key, out var secondInnerDict) || !pair.Value.SequenceEqual(secondInnerDict))
        return false;

    return true;
  }

  private static int ComputeDictionaryHash(
    Dictionary<string, Dictionary<byte, Vector2>>? dict)
  {
    if (dict == null) return 0;

    int hash = 0;
    foreach (var kvp in dict)
    {
      int innerHash = HashCode.Combine(
          kvp.Key,
          kvp.Value.Aggregate(0, (acc, pair) => HashCode.Combine(acc, pair.Key, pair.Value)));
      hash = HashCode.Combine(hash, innerHash);
    }
    return hash;
  }
}