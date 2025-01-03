using Qwaitumin.AutoTile;
using Qwaitumin.GameCore;


namespace Qwaitumin.AutoTileTests;


[SimpleTestClass]
public class AutoTileDrawerTest
{
  private static AutoTiler GetMockedAutoTiler(uint layerCount)
  {
    AutoTileData autoTileData = new(new bool[] { true }, new Dictionary<byte, Vector2Int>[] { new() });
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
    Vector2Int[] positions = GetVector2Rectangle(Vector2Int.Zero, new(16, 16));

    AutoTiler autoTiler = GetMockedAutoTiler(1);
    MockedTileMapDrawer testTileMapDrawer = new(1);
    AutoTileDrawer autoTileDrawer = new(testTileMapDrawer, autoTiler);

    // When
    autoTileDrawer.DrawTiles(0, GetMockedPositionsToTileIds(positions, tileId));

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
    Vector2Int[] positions = GetVector2Rectangle(Vector2Int.Zero, new(16, 16));

    AutoTiler autoTiler = GetMockedAutoTiler(1);
    MockedTileMapDrawer testTileMapDrawer = new(1);
    AutoTileDrawer autoTileDrawer = new(testTileMapDrawer, autoTiler);

    // When
    autoTileDrawer.DrawTilesAsync(0, GetMockedPositionsToTileIds(positions, tileId));

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
    Vector2Int[] positions = GetVector2Rectangle(Vector2Int.Zero, new(16, 16));

    AutoTiler autoTiler = GetMockedAutoTiler(1);
    MockedTileMapDrawer testTileMapDrawer = new(1);
    AutoTileDrawer autoTileDrawer = new(testTileMapDrawer, autoTiler);

    // When
    autoTileDrawer.DrawTilesAsync(0, GetMockedPositionsToTileIds(positions, tileId));
    autoTileDrawer.Wait();

    // Then
    foreach (var position in positions)
      Assertions.AssertEqual(
        tileId, testTileMapDrawer.Data[0][new(position.X, position.Y)].TileId);
  }

  public static Vector2Int[] GetVector2Rectangle(Vector2Int at, Vector2Int size)
  {
    Vector2Int[] arr = new Vector2Int[size.X * size.Y];

    int index = 0;
    for (int x = 0; x < size.X; x++)
      for (int y = 0; y < size.Y; y++)
        arr[index++] = at + new Vector2Int(x, y);

    return arr;
  }
}