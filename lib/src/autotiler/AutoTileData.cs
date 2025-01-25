using System.Numerics;

namespace Qwaitumin.AutoTile;

// NOTE: This has to work as fast as possible
public class AutoTileData
{
  private readonly bool[] tileIdConnections;
  private readonly bool[,] tileIdToMappedValues;
  private readonly Vector2[,] tileIdToComputedBitmaskToAtlasCoords;

  public AutoTileData(
    bool[] tileIdConnections, Dictionary<byte, Vector2>[] tileIdsToBitmasks)
  {
    this.tileIdConnections = tileIdConnections;
    if (tileIdConnections.Length != tileIdsToBitmasks.Length)
      throw new ArgumentException($"Provided connection array is not equal to bitmask array {tileIdConnections.Length} != {tileIdsToBitmasks.Length}");

    tileIdToMappedValues = new bool[tileIdsToBitmasks.Length, byte.MaxValue + 1];
    tileIdToComputedBitmaskToAtlasCoords = new Vector2[tileIdsToBitmasks.Length, byte.MaxValue + 1];
    for (int tileId = 0; tileId < tileIdsToBitmasks.Length; tileId++)
    {
      foreach (var (computedBitmask, position) in tileIdsToBitmasks[tileId])
      {
        tileIdToComputedBitmaskToAtlasCoords[tileId, computedBitmask] = position;
        tileIdToMappedValues[tileId, computedBitmask] = true;
      }
    }
  }

  public bool CanConnectTo(int tileId)
    => tileIdConnections[tileId];

  public Vector2 GetAtlasCoords(int neighbourTileId, byte computedBitmask)
    => tileIdToMappedValues[neighbourTileId, computedBitmask] ?
      tileIdToComputedBitmaskToAtlasCoords[neighbourTileId, computedBitmask] : new(0, 0);
}