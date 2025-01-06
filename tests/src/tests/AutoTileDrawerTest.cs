using AutoTile;
using GameCore;
using SimpleTest;

namespace AutotileTests;


[SimpleTestClass]
public class AutoTileDrawerTest
{
  private static AutoTiler GetMockedAutoTiler(int layerCount)
  {
    AutoTileData autoTileData = new(new bool[] { true }, new());
    return new(layerCount, new AutoTileData[] { autoTileData });
  }

  private static KeyValuePair<Vector2Int, int>[] GetMockedPositionsToTileIds(
    Vector2Int[] positions, int tileId)
  {
    List<KeyValuePair<Vector2Int, int>> positionsToTileIds = new();
    foreach (var position in positions)
      positionsToTileIds.Add(new(position, tileId));

    return positionsToTileIds.ToArray();
  }

  [SimpleTestMethod]
  public void DrawTiles_ShouldDrawTiles_WhenMockedDrawer()
  {
    // Given
    int tileId = 0;
    Vector3Int mapSize = new(16, 16, 1);
    Vector2Int[] positions = Vector2Int.GetInRectangle(Vector2Int.Zero, new(mapSize.X, mapSize.Y));

    AutoTiler autoTiler = GetMockedAutoTiler(1);
    MockedTileMapDrawer testTileMapDrawer = new(1);
    AutotileDrawer autotileDrawer = new(testTileMapDrawer, autoTiler);

    // When
    autotileDrawer.DrawTiles(0, GetMockedPositionsToTileIds(positions, tileId));

    // Then
    foreach (var position in positions)
      Assertions.AssertEqual(
        tileId, testTileMapDrawer.Data[0][new(position.X, position.Y)].TileId);
  }

  [SimpleTestMethod]
  public void DrawTilesAsync_ShouldDrawTiles_WhenMockedDrawer()
  {
    // Given
    int tileId = 0;
    Vector3Int mapSize = new(16, 16, 1);
    Vector2Int[] positions = Vector2Int.GetInRectangle(Vector2Int.Zero, new(mapSize.X, mapSize.Y));

    AutoTiler autoTiler = GetMockedAutoTiler(1);
    MockedTileMapDrawer testTileMapDrawer = new(1);
    AutotileDrawer autotileDrawer = new(testTileMapDrawer, autoTiler);

    // When
    autotileDrawer.DrawTilesAsync(0, GetMockedPositionsToTileIds(positions, tileId));

    // Then
    Assertions.AssertAwaitAtMost(100, () => // 100ms is more than enough for 16x16 tiles
    {
      foreach (var position in positions)
        Assertions.AssertEqual(
          tileId, testTileMapDrawer.Data[0][new(position.X, position.Y)].TileId);
    });
  }

  [SimpleTestMethod]
  public void DrawTilesAsync_ShouldDrawTiles_WhenMockedDrawerAndWait()
  {
    // Given
    int tileId = 0;
    Vector3Int mapSize = new(16, 16, 1);
    Vector2Int[] positions = Vector2Int.GetInRectangle(Vector2Int.Zero, new(mapSize.X, mapSize.Y));

    AutoTiler autoTiler = GetMockedAutoTiler(1);
    MockedTileMapDrawer testTileMapDrawer = new(1);
    AutotileDrawer autotileDrawer = new(testTileMapDrawer, autoTiler);

    // When
    autotileDrawer.DrawTilesAsync(0, GetMockedPositionsToTileIds(positions, tileId));
    autotileDrawer.Wait();

    // Then
    foreach (var position in positions)
      Assertions.AssertEqual(
        tileId, testTileMapDrawer.Data[0][new(position.X, position.Y)].TileId);
  }
}