using System;
using Autotile;
using GameCore;
using SimpleTest;

namespace AutotileTests;

[SimpleTestClass]
public class AutoTileDataTest
{
  [SimpleTestMethod]
  public void CanConnectTo_CorrectlyReturnsConnections_WhenSetup()
  {
    // Given
    AutoTileData autoTileData = new(new bool[] { false, true }, new());

    // When
    var shouldNotConnect = autoTileData.CanConnectTo(0);
    var shouldConnect = autoTileData.CanConnectTo(1);

    // Then
    Assertions.AssertTrue(shouldConnect);
    Assertions.AssertFalse(shouldNotConnect);
  }

  [SimpleTestMethod]
  public void GetAtlasCoords_CorrectlyReturnCoords_WhenGivenCorrectPosition()
  {
    // Given
    AutoTileData autoTileData = new(Array.Empty<bool>(), new() { { Vector2Int.One, 2 } });

    // When
    var shouldBeOne = autoTileData.GetAtlasCoords(2);
    var shouldBeDefault = autoTileData.GetAtlasCoords(123);

    // Then
    Assertions.AssertEqual(Vector2Int.One, shouldBeOne);
    Assertions.AssertEqual(Vector2Int.Zero, shouldBeDefault);
  }
}