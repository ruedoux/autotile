namespace AutoTile;

public class AutoTilerComposer
{
  public readonly TileLoader TileLoader;
  private readonly AutotileConfig autotileConfig;

  public AutoTilerComposer(
    string imageDirectoryPath,
    AutotileConfig autotileConfig,
    Dictionary<string, int> tileNameToIds)
  {
    this.autotileConfig = autotileConfig;

    TileLoader = new(imageDirectoryPath, autotileConfig, tileNameToIds);
  }

  public AutoTiler GetAutoTiler()
  {
    var biggestLayer = autotileConfig.TileDefinitions
      .DistinctBy(e => e.Value.Layer)
      .OrderByDescending(e => e.Value.Layer)
      .FirstOrDefault().Value.Layer;

    return new(biggestLayer + 1, GetAutoTileData(TileLoader.LoadTiles()));
  }

  private static AutoTileData[] GetAutoTileData(Dictionary<TileIdentificator, TileResource> tiles)
  {
    var autoTileIdToTileDatas = new AutoTileData[tiles.Count];
    Dictionary<int, bool[]> autotileGroupToConnections = new();

    foreach (var (_, tileResource) in tiles)
      autotileGroupToConnections[tileResource.AutoTileGroup] = new bool[tiles.Count];

    foreach (var (TileIdentificator, tileResource) in tiles)
      autotileGroupToConnections[tileResource.AutoTileGroup][TileIdentificator.TileId] = true;

    foreach (var (TileIdentificator, tileResource) in tiles)
      autoTileIdToTileDatas[TileIdentificator.TileId] = new(
        autotileGroupToConnections[tileResource.AutoTileGroup], tileResource.BitmaskMap);

    return autoTileIdToTileDatas;
  }
}