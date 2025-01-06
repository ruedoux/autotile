using AutoTile;
using GameCore;
using SimpleTest;

namespace AutoTileTests;

[SimpleTestClass]
public class AutoTilerTest
{
  [SimpleTestMethod]
  public void PlaceTile_CorrectlyPlacesTile_WhenCalled()
  {
    // Given
    AutoTileData autoTileData = new(new bool[] { true }, new());
    AutoTiler autoTiler = new(1, new AutoTileData[] { autoTileData });

    // When
    autoTiler.PlaceTile(0, Vector2Int.Zero, 0);
    var tileData = autoTiler.GetTile(0, Vector2Int.Zero);
    autoTiler.PlaceTile(0, Vector2Int.Zero, -1);
    var tileDataAfterRemoval = autoTiler.GetTile(0, Vector2Int.Zero);

    // Then
    Assertions.AssertNull(tileDataAfterRemoval);
    Assertions.AssertNotNull(tileData);
    Assertions.AssertEqual(0, tileData.TileId);
    Assertions.AssertEqual(Bitmask.DEFAULT, tileData.Bitmask);
  }

  [SimpleTestMethod]
  public void PlaceTile_CorrectlyPlacesTiles_WhenCalledAsync()
  {
    // Given
    AutoTileData autoTileData = new(new bool[] { true }, new());
    AutoTiler autoTiler = new(1, new AutoTileData[] { autoTileData });

    List<Vector2Int> positions = new();
    for (int x = 0; x < 10; x++)
      for (int y = 0; y < 10; y++)
        positions.Add(new(x, y));

    // When
    List<Task> tasks = new();
    for (int i = 0; i < 10; i++)
    {
      tasks.Add(new Task(() =>
      {
        foreach (var position in positions)
          autoTiler.PlaceTile(0, position, 0);
      }));
    }

    foreach (var task in tasks)
      task.Start();

    Task.WhenAll(tasks).Wait();

    // Then
    foreach (var position in positions)
    {
      var tileData = autoTiler.GetTile(0, position);
      Assertions.AssertNotNull(tileData);
      Assertions.AssertEqual(0, tileData.TileId);
    }
  }

  [SimpleTestMethod]
  public void PlaceTile_CorrectlyUpdatesBitmask_WhenCalled()
  {
    // Given
    byte tl = 1 << 0;
    byte tt = 1 << 1;
    byte tr = 1 << 2;
    byte rr = 1 << 3;
    byte br = 1 << 4;
    byte bb = 1 << 5;
    byte bl = 1 << 6;
    byte ll = 1 << 7;

    AutoTileData autoTileData = new(new bool[] { true }, new());
    AutoTiler autoTiler = new(1, new AutoTileData[] { autoTileData });
    HelperAutoTiler helperAutoTiler = new(autoTiler, 0, Vector2Int.Zero);

    // When
    //Then
    autoTiler.PlaceTile(0, Vector2Int.Zero, 0);
    helperAutoTiler.AssertBitmaskMatches(
      new byte[] { 0, 0, 0,
                   0, 0, 0,
                   0, 0, 0 });

    autoTiler.PlaceTile(0, Vector2Int.TopLeft, 0);
    helperAutoTiler.AssertBitmaskMatches(
      new byte[] { br, 0, 0,
                   0, tl, 0,
                   0, 0, 0 });

    autoTiler.PlaceTile(0, Vector2Int.Top, 0);
    helperAutoTiler.AssertBitmaskMatches(
      new byte[] { (byte)(br | rr), (byte)(ll | bb), 0,
                   0, (byte)(tl | tt), 0,
                   0, 0, 0 });

    autoTiler.PlaceTile(0, Vector2Int.TopRight, 0);
    helperAutoTiler.AssertBitmaskMatches(
      new byte[] { (byte)(rr | br), (byte)(ll | bb | rr), (byte)(ll | bl),
                   0, (byte)(tl | tt | tr), 0,
                   0, 0, 0 });
  }

  class HelperAutoTiler
  {
    private readonly AutoTiler autoTiler;
    private readonly int layer;
    private readonly Vector2Int atPosition;

    public HelperAutoTiler(AutoTiler autoTiler, int layer, Vector2Int atPosition)
    {
      this.autoTiler = autoTiler;
      this.layer = layer;
      this.atPosition = atPosition;
    }

    public void AssertBitmaskMatches(byte[] bitmasks)
    {
      Assertions.AssertEqual(9, bitmasks.Length);

      var middleTile = autoTiler.GetTile(layer, atPosition);
      var topLeftTile = autoTiler.GetTile(layer, atPosition + Vector2Int.TopLeft);
      var topTile = autoTiler.GetTile(layer, atPosition + Vector2Int.Top);
      var topRightTile = autoTiler.GetTile(layer, atPosition + Vector2Int.TopRight);
      var rightTile = autoTiler.GetTile(layer, atPosition + Vector2Int.Right);
      var bottomRightTile = autoTiler.GetTile(layer, atPosition + Vector2Int.BottomRight);
      var bottomTile = autoTiler.GetTile(layer, atPosition + Vector2Int.Bottom);
      var bottomLeftTile = autoTiler.GetTile(layer, atPosition + Vector2Int.BottomLeft);
      var leftTile = autoTiler.GetTile(layer, atPosition + Vector2Int.Left);

      VerifyTile(topLeftTile, bitmasks[0]);
      VerifyTile(topTile, bitmasks[1]);
      VerifyTile(topRightTile, bitmasks[2]);
      VerifyTile(leftTile, bitmasks[3]);
      VerifyTile(middleTile, bitmasks[4]);
      VerifyTile(rightTile, bitmasks[5]);
      VerifyTile(bottomLeftTile, bitmasks[6]);
      VerifyTile(bottomTile, bitmasks[7]);
      VerifyTile(bottomRightTile, bitmasks[8]);
    }

    private static void VerifyTile(TileData? tileData, byte bitmaskValue)
    {
      if (tileData is null) Assertions.AssertEqual(0, bitmaskValue);
      else Assertions.AssertEqual(bitmaskValue, tileData.Bitmask);
    }
  }
}