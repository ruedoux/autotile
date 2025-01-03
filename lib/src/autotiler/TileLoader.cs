using Qwaitumin.GameCore;

namespace Qwaitumin.AutoTile;

public record TileIdentificator(int TileId, string TileName);
public record TileResource(
  string ImagePath,
  uint Layer,
  Dictionary<byte, Vector2Int>[] TileIdToBitmaskSets,
  int AutoTileGroup);


public class TileLoader
{
  private readonly string imageDirectoryPath;
  private readonly AutoTileConfig autoTileConfig;
  private readonly string[] tileIdToTileNames;

  public TileLoader(
    string imageDirectoryPath,
    AutoTileConfig autoTileConfig,
    string[] tileNameToIds)
  {
    this.imageDirectoryPath = imageDirectoryPath;
    this.autoTileConfig = autoTileConfig;
    this.tileIdToTileNames = tileNameToIds;
  }

  public Dictionary<TileIdentificator, TileResource> LoadTiles()
  {
    if (autoTileConfig.TileDefinitions.Count != tileIdToTileNames.Length)
      throw new ArgumentException($"TileSetConfig defined tiles count should be equal to passed tile names array: {autoTileConfig.TileDefinitions.Count} != {tileIdToTileNames.Length}");

    foreach (var (tileName, _) in autoTileConfig.TileDefinitions)
      if (Array.FindIndex(tileIdToTileNames, name => name == tileName) < 0)
        throw new ArgumentException($"TileSetConfig contains tile '{tileName}', but passes tile names array does not contain it");

    if (!Directory.Exists(imageDirectoryPath))
      throw new DirectoryNotFoundException($"Directory does not exist: {imageDirectoryPath}");

    Dictionary<string, string> fileNamesToPaths = GetFileNamesToPaths(imageDirectoryPath);
    Dictionary<TileIdentificator, TileResource> tiles = GetTileIdentificators()
      .ToDictionary(
          tileIdentificator => tileIdentificator,
          tileIdentificator => GetTileResource(tileIdentificator, fileNamesToPaths));

    return tiles;
  }

  private TileIdentificator[] GetTileIdentificators()
    => Enumerable.Range(0, tileIdToTileNames.Length)
    .Select(tileId => new TileIdentificator(tileId, tileIdToTileNames[tileId]))
    .ToArray();

  private TileResource GetTileResource(
    TileIdentificator tileIdentificator, Dictionary<string, string> fileNamesToPaths)
  {
    var tileDefinition = autoTileConfig.TileDefinitions[tileIdentificator.TileName];
    if (!fileNamesToPaths.TryGetValue(tileDefinition.ImageFileName, out string? imagePath))
      throw new ArgumentException($"Missing required image: '{tileDefinition.ImageFileName}' in path: '{imageDirectoryPath}'");

    var defaultBitmaskSet = autoTileConfig.BitmaskSets[tileDefinition.BitmaskName];
    var shiftedDefaultBitmaskSet = ShiftBitmask(new(defaultBitmaskSet), tileDefinition.PositionInSet);

    var tileIdToBitmasks = new Dictionary<byte, Vector2Int>[tileIdToTileNames.Length];
    for (int i = 0; i < tileIdToBitmasks.Length; i++)
      tileIdToBitmasks[i] = shiftedDefaultBitmaskSet;

    ApplyBitmaskOverrides(tileDefinition, tileIdToBitmasks);

    return new(
      ImagePath: imagePath,
      Layer: tileDefinition.Layer,
      TileIdToBitmaskSets: tileIdToBitmasks,
      AutoTileGroup: tileDefinition.AutoTileGroup
    );
  }

  private void ApplyBitmaskOverrides(
    TileDefinition tileDefinition, Dictionary<byte, Vector2Int>[] tileIdToBitmaskSets)
  {
    if (tileDefinition.BitmaskOverrides is null)
      return;

    foreach (var (overrideTileName, bitmaskSet) in tileDefinition.BitmaskOverrides)
    {
      int overrideTileId = Array.FindIndex(tileIdToTileNames, name => name == overrideTileName);
      foreach (var (overrideBitmask, overridePosition) in bitmaskSet)
        tileIdToBitmaskSets[overrideTileId][overrideBitmask] = overridePosition;
    }
  }

  private static Dictionary<string, string> GetFileNamesToPaths(string imageDirectoryPath)
    => Directory.GetFiles(imageDirectoryPath, $"*", SearchOption.AllDirectories)
        .ToDictionary(p => Path.GetFileName(p), p => p);

  private static Dictionary<byte, Vector2Int> ShiftBitmask(Dictionary<byte, Vector2Int> bitmaskSet, Vector2Int positionInSet)
    => bitmaskSet
        .Select(kv => new KeyValuePair<byte, Vector2Int>(kv.Key, kv.Value + positionInSet))
        .ToDictionary();
}