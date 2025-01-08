using AutoTile;
using GameCore;
using SimpleTest;

namespace CoreTests;

[SimpleTestClass]
public class AutoTileConfigTest
{
  const int TILE_SIZE = 16;

  const string SINGLE_TILE_NAME1 = "ExampleSingle1";
  const string SINGLE_TILE_NAME2 = "ExampleSingle2";
  const string SINGLE_TILE_FILE = "ExampleSingle.png";
  const string SINGLE_TILE_BITMASK = "Single";

  const string AUTO_TILE_NAME1 = "ExampleAutoTiled1";
  const string AUTO_TILE_NAME2 = "ExampleAutoTiled2";
  const string AUTO_TILE_FILE = "ExampleAutoTiled.jpg";
  const string AUTO_TILE_BITMASK = "AutoTiled";

  const string TILESET_MOCK_PATH = "./resources/AutoTileConfig.json";

  [SimpleTestMethod]
  public void LoadObjectFromFile_ShouldDeserialize_WhenLoadedFromFile()
  {
    // Given
    // When
    AutoTileConfig autoTileConfig = JsonSerializable.LoadObjectFromFile(
      TILESET_MOCK_PATH, AutoTileConfigJsonContext.Default.AutoTileConfig);

    // Then
    Assertions.AssertEqual(autoTileConfig.TileSize, TILE_SIZE);

    Assertions.AssertTrue(autoTileConfig.BitmaskSets.ContainsKey(AUTO_TILE_BITMASK));
    Assertions.AssertEqual(56, autoTileConfig.BitmaskSets[AUTO_TILE_BITMASK][Vector2Int.Zero]);
    Assertions.AssertTrue(autoTileConfig.BitmaskSets.ContainsKey(SINGLE_TILE_BITMASK));
    Assertions.AssertEqual(0, autoTileConfig.BitmaskSets[SINGLE_TILE_BITMASK][Vector2Int.Zero]);

    Assertions.AssertEqual(
      new(Layer: 0, ImageFileName: SINGLE_TILE_FILE, BitmaskName: SINGLE_TILE_BITMASK, PositionInSet: Vector2Int.Zero, AutoTileGroup: -1),
      autoTileConfig.TileDefinitions[SINGLE_TILE_NAME1]);
    Assertions.AssertEqual(
      new(Layer: 0, ImageFileName: SINGLE_TILE_FILE, BitmaskName: SINGLE_TILE_BITMASK, PositionInSet: new(TILE_SIZE, 0), AutoTileGroup: -1),
      autoTileConfig.TileDefinitions[SINGLE_TILE_NAME2]);
    Assertions.AssertEqual(
      new(Layer: 0, ImageFileName: AUTO_TILE_FILE, BitmaskName: AUTO_TILE_BITMASK, PositionInSet: Vector2Int.Zero, AutoTileGroup: 0),
      autoTileConfig.TileDefinitions[AUTO_TILE_NAME1]);
    Assertions.AssertEqual(
      new(Layer: 0, ImageFileName: AUTO_TILE_FILE, BitmaskName: AUTO_TILE_BITMASK, PositionInSet: new(TILE_SIZE * 6, 0), AutoTileGroup: 0),
      autoTileConfig.TileDefinitions[AUTO_TILE_NAME2]);
  }
}