namespace Autotile;

public class AutoTilerComposer
{
  public readonly TileLoader TileLoader;
  private readonly TileSetConfig tileSetConfig;

  public AutoTilerComposer(
    string imageDirectoryPath,
    TileSetConfig tileSetConfig,
    Dictionary<string, int> tileNameToIds)
  {
    this.tileSetConfig = tileSetConfig;

    TileLoader = new(imageDirectoryPath, tileSetConfig, tileNameToIds);
  }

  public AutoTiler GetAutoTiler()
  {
    var biggestLayer = tileSetConfig.TileDefinitions
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