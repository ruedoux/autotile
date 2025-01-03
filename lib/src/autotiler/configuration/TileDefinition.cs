using GameCore;

namespace Autotile;

public partial record class TileDefinition(
  int Layer = -1,
  string ImageFileName = "<Null>",
  string BitmaskName = "<Null>",
  Vector2Int PositionInSet = default,
  int AutoTileGroup = -1);