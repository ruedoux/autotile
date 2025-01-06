using GameCore;

namespace AutoTile;

public class AutoTileData
{
  private readonly bool[] mappedValues = new bool[byte.MaxValue + 1];
  private readonly Vector2Int[] computedBitmaskToAtlasCoords = new Vector2Int[byte.MaxValue + 1];
  private readonly bool[] tileIdConnections;

  public AutoTileData(
    bool[] tileIdConnections, Dictionary<Vector2Int, byte> bitmaskSet)
  {
    this.tileIdConnections = tileIdConnections;
    foreach (var (position, computedBitmask) in bitmaskSet)
    {
      computedBitmaskToAtlasCoords[computedBitmask] = position;
      mappedValues[computedBitmask] = true;
    }
  }

  public bool CanConnectTo(int tileId)
    => tileIdConnections[tileId];

  public Vector2Int GetAtlasCoords(byte computedBitmask)
    => mappedValues[computedBitmask] ?
      computedBitmaskToAtlasCoords[computedBitmask] : Vector2Int.Zero;
}