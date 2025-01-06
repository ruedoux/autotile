using System.Collections.Concurrent;
using GameCore;

namespace AutoTile;

public sealed record TileData(int TileId, byte Bitmask, Vector2Int AtlasCoords);

public class AutoTiler
{
  public static readonly Vector2Int[] CELL_SURROUNDING_DIRECTIONS = new Vector2Int[] {
      Vector2Int.TopLeft, Vector2Int.Top, Vector2Int.TopRight, Vector2Int.Right, Vector2Int.BottomRight, Vector2Int.Bottom, Vector2Int.BottomLeft, Vector2Int.Left };

  private readonly AutoTileData[] tileIdToAutotileData;
  private readonly ConcurrentDictionary<Vector2Int, TileData>[] data;

  public AutoTiler(int layerCount, AutoTileData[] tileIdToAutotileData)
  {
    if (layerCount < 1)
      throw new ArgumentException($"Layer count must be higher than 1, given: {layerCount}");

    data = new ConcurrentDictionary<Vector2Int, TileData>[layerCount];
    for (int layer = 0; layer < data.Length; layer++)
      data[layer] = new();

    this.tileIdToAutotileData = tileIdToAutotileData;
  }

  public void Clear()
  {
    for (int i = 0; i < data.Length; i++)
      data[i].Clear();
  }

  public int GetLayerCount()
    => data.Length;

  public Vector2Int[] GetAllPositions(int layer)
  {
    ValidateLayer(layer);
    return data[layer].Keys.ToArray();
  }

  public TileData? GetTile(int layer, Vector2Int position)
  {
    ValidateLayer(layer);
    return GetTileDataAt(layer, position);
  }

  public void PlaceTile(int layer, Vector2Int position, int tileId)
  {
    ValidateLayer(layer);
    ValidateTileId(tileId);

    if (tileId < 0)
      data[layer].TryRemove(position, out _);
    else
    {
      data[layer][position] = new(tileId, new(), Vector2Int.Zero);

      var bitmask = GetTileBitmask(layer, position);
      data[layer][position] = new(
        tileId, bitmask, tileIdToAutotileData[tileId].GetAtlasCoords(Bitmask.Parse(bitmask)));
    }

    for (int i = 0; i < CELL_SURROUNDING_DIRECTIONS.Length; i++)
      UpdateTileBitmaskRelative(layer, position, (Bitmask.SurroundingDirection)i);
  }

  private void UpdateTileBitmaskRelative(
    int layer, Vector2Int centerPosition, Bitmask.SurroundingDirection updateDirection)
  {
    Vector2Int updatePosition = centerPosition - CELL_SURROUNDING_DIRECTIONS[(int)updateDirection];
    if (GetTileDataAt(layer, updatePosition) is not TileData tileToUpdate)
      return;

    TileData? centerTile = GetTileDataAt(layer, centerPosition);
    bool canConnect = centerTile is not null && tileIdToAutotileData[tileToUpdate.TileId].CanConnectTo(centerTile.TileId);
    var bitmask = Bitmask.UpdateBitmask(tileToUpdate.Bitmask, updateDirection, canConnect);
    data[layer][updatePosition] = new(
      tileToUpdate.TileId,
      bitmask,
      tileIdToAutotileData[tileToUpdate.TileId].GetAtlasCoords(Bitmask.Parse(bitmask)));
  }

  private byte GetTileBitmask(int layer, Vector2Int position)
  {
    TileData? centerTile = GetTileDataAt(layer, position);
    if (centerTile is null)
      return Bitmask.DEFAULT;

    var bitmaskArr = new bool[8];
    for (int i = 0; i < CELL_SURROUNDING_DIRECTIONS.Length; i++)
    {
      var surroundingPosition = position + CELL_SURROUNDING_DIRECTIONS[i];
      TileData? tileData = GetTileDataAt(layer, surroundingPosition);
      if (tileData is null)
        continue;

      bitmaskArr[i] = tileIdToAutotileData[centerTile.TileId].CanConnectTo(tileData.TileId);
    }

    return Bitmask.FromArray(bitmaskArr);
  }

  private TileData? GetTileDataAt(int layer, Vector2Int position)
  {
    if (data[layer].TryGetValue(position, out TileData? tileData))
      return tileData;
    return null;
  }

  private void ValidateTileId(int tileId)
  {
    if (tileIdToAutotileData.Length <= tileId)
      throw new ArgumentException($"Tile of id does not exist: {tileId}, max tile id is: {tileIdToAutotileData.Length}");
  }

  private void ValidateLayer(int layer)
  {
    if (data.Length < layer - 1 || layer < 0)
      throw new ArgumentException($"AutoTiler does not contain layer: {layer}");
  }
}