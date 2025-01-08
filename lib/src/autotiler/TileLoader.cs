using GameCore;

namespace AutoTile;

public record TileIdentificator(int TileId, string TileName);
public record TileResource(
  string ImagePath,
  int Layer,
  Dictionary<Vector2Int, byte> BitmaskMap,
  int AutoTileGroup);


public class TileLoader
{
  private static readonly string[] filters = new string[] { "*" };
  private readonly string imageDirectoryPath;
  private readonly AutoTileConfig autoTileConfig;
  private readonly Dictionary<string, int> tileNameToIds;

  public TileLoader(
    string imageDirectoryPath,
    AutoTileConfig autoTileConfig,
    Dictionary<string, int> tileNameToIds)
  {
    this.imageDirectoryPath = imageDirectoryPath;
    this.autoTileConfig = autoTileConfig;
    this.tileNameToIds = tileNameToIds;
  }

  public Dictionary<TileIdentificator, TileResource> LoadTiles()
  {
    if (autoTileConfig.TileDefinitions.Count != tileNameToIds.Count)
      throw new ArgumentException($"TileSetConfig defined tiles count should be equal to passed name to id dict: {autoTileConfig.TileDefinitions.Count} != {tileNameToIds.Count}");

    if (!Directory.Exists(imageDirectoryPath))
      throw new DirectoryNotFoundException($"Directory does not exist: {imageDirectoryPath}");

    Dictionary<TileIdentificator, TileResource> tiles = new();
    Dictionary<string, string> fileNamesToPaths = GetFileNamesToPaths(imageDirectoryPath);
    foreach (var (tileName, tileDefinition) in autoTileConfig.TileDefinitions)
    {
      if (!fileNamesToPaths.TryGetValue(tileDefinition.ImageFileName, out string? imagePath))
        throw new ArgumentException($"Missing required image: '{tileDefinition.ImageFileName}' in path: '{imageDirectoryPath}'");

      if (!autoTileConfig.BitmaskSets.TryGetValue(tileDefinition.BitmaskName, out var bitmask))
        throw new ArgumentException($"Missing required bitmask: '{tileDefinition.BitmaskName}' in autoTileConfig");

      tiles[new(tileNameToIds[tileName], tileName)] = new(
        imagePath,
        tileDefinition.Layer,
        ShiftBitmask(new(bitmask), tileDefinition.PositionInSet),
        tileDefinition.AutoTileGroup);
    }

    return tiles;
  }

  private static Dictionary<string, string> GetFileNamesToPaths(string imageDirectoryPath)
  {
    var imagePaths = FileSystem.GetFilesFromDirectory(imageDirectoryPath, filters, true);
    return imagePaths.ToDictionary(
      p => Path.GetFileName(p), p => p);
  }

  private static Dictionary<Vector2Int, byte> ShiftBitmask(Dictionary<Vector2Int, byte> bitmask, Vector2Int positionInSet)
    => bitmask
        .Select(kv => new KeyValuePair<Vector2Int, byte>(kv.Key + positionInSet, kv.Value))
        .ToDictionary();
}