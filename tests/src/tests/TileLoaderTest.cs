using Autotile;
using GameCore;
using SimpleTest;

namespace AutotileTests;


[SimpleTestClass]
public class TileLoaderTest
{
  const string BITMASK_NAME = "bitmask";
  const string TILE1_NAME = "name1";
  const string TILE2_NAME = "name2";
  const int TILE1_LAYER = 0;
  const int TILE2_LAYER = 1;
  const int TILE1_ID = 0;
  const int TILE2_ID = 1;

  private readonly TileSetConfig tileSetConfig = TileSetConfig.Construct(
      16,
      new() { { TILE1_NAME, new(Layer: TILE1_LAYER, BitmaskName: BITMASK_NAME, ImageFileName: TILE1_NAME) }, { TILE2_NAME, new(Layer: TILE2_LAYER, BitmaskName: BITMASK_NAME, ImageFileName: TILE2_NAME) } },
      new() { { BITMASK_NAME, new() { { Vector2Int.Zero, 0 } } } });

  [SimpleTestMethod]
  public void LoadTiles_ShouldCorrectlyLoadTiles_WhenCalled()
  {
    // Given
    using SimpleTestDirectory testDirectory = new();
    File.Create(testDirectory.GetRelativePath(TILE1_NAME + ".jpg"));
    File.Create(testDirectory.GetRelativePath(TILE2_NAME + ".jpg"));

    Dictionary<string, int> tileNameToids = new() { { TILE1_NAME, TILE1_ID }, { TILE2_NAME, TILE2_ID } };
    TileLoader tileLoader = new(testDirectory.AbsolutePath, tileSetConfig, tileNameToids);

    // When
    var tiles = tileLoader.LoadTiles();

    // Then
    Assertions.AssertEqual(2, tiles.Count);
    Assertions.AssertTrue(tiles.ContainsKey(new(TILE1_ID, TILE1_NAME)));
    Assertions.AssertTrue(tiles.ContainsKey(new(TILE2_ID, TILE2_NAME)));
    Assertions.AssertEqual(testDirectory.AbsolutePath + "/" + TILE1_NAME + ".jpg", tiles[new(TILE1_ID, TILE1_NAME)].ImagePath);
    Assertions.AssertEqual(testDirectory.AbsolutePath + "/" + TILE2_NAME + ".jpg", tiles[new(TILE2_ID, TILE2_NAME)].ImagePath);
  }
}