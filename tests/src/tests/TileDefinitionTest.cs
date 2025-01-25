using Qwaitumin.AutoTile;
using Qwaitumin.SimpleTest;

namespace CoreTests;

[SimpleTestClass]
public class TileDefinitionTest
{
  [SimpleTestMethod]
  public void VerifyEquality()
  {
    SimpleEqualsVerifier.Verify<TileDefinition>(
      new(Layer: 0),
      new(Layer: 0),
      new(Layer: 1));
    SimpleEqualsVerifier.Verify<TileDefinition>(
      new(ImageFileName: "a"),
      new(ImageFileName: "a"),
      new(ImageFileName: "b"));
    SimpleEqualsVerifier.Verify<TileDefinition>(
      new(PositionInSet: new()),
      new(PositionInSet: new()),
      new(PositionInSet: new(1, 1)));
    SimpleEqualsVerifier.Verify<TileDefinition>(
      new(BitmaskName: "a"),
      new(BitmaskName: "a"),
      new(BitmaskName: "b"));
    SimpleEqualsVerifier.Verify<TileDefinition>(
      new(AutoTileGroup: 0),
      new(AutoTileGroup: 0),
      new(AutoTileGroup: 1));
    SimpleEqualsVerifier.Verify<TileDefinition>(
      new(BitmaskOverrides: new() { { "a", new() } }),
      new(BitmaskOverrides: new() { { "a", new() } }),
      new(BitmaskOverrides: new() { { "b", new() } }));
    SimpleEqualsVerifier.Verify<TileDefinition>(
      new(BitmaskOverrides: new() { { "a", new() { { 1, new(0, 0) } } } }),
      new(BitmaskOverrides: new() { { "a", new() { { 1, new(0, 0) } } } }),
      new(BitmaskOverrides: new() { { "a", new() { { 2, new(0, 0) } } } }));
    SimpleEqualsVerifier.Verify<TileDefinition>(
      new(BitmaskOverrides: new() { { "a", new() { { 1, new(0, 0) } } } }),
      new(BitmaskOverrides: new() { { "a", new() { { 1, new(0, 0) } } } }),
      new(BitmaskOverrides: new() { { "a", new() { { 1, new(1, 1) } } } }));
  }
}