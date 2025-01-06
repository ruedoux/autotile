namespace AutoTile;

public class AutoTilerComposer
{
  public readonly TileLoader TileLoader;
  private readonly AutoTileConfig autoTileConfig;

  public AutoTilerComposer(
    string imageDirectoryPath,
    AutoTileConfig autoTileConfig,
    Dictionary<string, int> tileNameToIds)
  {
    this.autoTileConfig = autoTileConfig;

    TileLoader = new(imageDirectoryPath, autoTileConfig, tileNameToIds);
  }

  public AutoTiler GetAutoTiler()
  {
    var biggestLayer = autoTileConfig.TileDefinitions
      .DistinctBy(e => e.Value.Layer)
      .OrderByDescending(e => e.Value.Layer)
      .FirstOrDefault().Value.Layer;

    return new(biggestLayer + 1, GetAutoTileData(TileLoader.LoadTiles()));
  }

  private static AutoTileData[] GetAutoTileData(Dictionary<TileIdentificator, TileResource> tiles)
  {
    var autoTileIdToTileDatas = new AutoTileData[tiles.Count];
    Dictionary<int, bool[]> autoTileGroupToConnections = new();

    foreach (var (_, tileResource) in tiles)
      autoTileGroupToConnections[tileResource.AutoTileGroup] = new bool[tiles.Count];

    foreach (var (TileIdentificator, tileResource) in tiles)
      autoTileGroupToConnections[tileResource.AutoTileGroup][TileIdentificator.TileId] = true;

    foreach (var (TileIdentificator, tileResource) in tiles)
      autoTileIdToTileDatas[TileIdentificator.TileId] = new(
        autoTileGroupToConnections[tileResource.AutoTileGroup], tileResource.BitmaskMap);

    return autoTileIdToTileDatas;
  }
}