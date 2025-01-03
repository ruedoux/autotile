using GameCore;

namespace Autotile;

public record TileIdentificator(int TileId, string TileName);
public record TileResource(
  string ImagePath,
  int Layer,
  Dictionary<Vector2Int, byte> BitmaskMap,
  int AutoTileGroup);


public class TileLoader
{
  private static readonly string[] IMAGE_EXTENSIONS = new string[2] { "png", "jpg" };

  private readonly string imageDirectoryPath;
  private readonly TileSetConfig tileSetConfig;
  private readonly Dictionary<string, int> tileNameToIds;

  public TileLoader(
    string imageDirectoryPath,
    TileSetConfig tileSetConfig,
    Dictionary<string, int> tileNameToIds)
  {
    this.imageDirectoryPath = imageDirectoryPath;
    this.tileSetConfig = tileSetConfig;
    this.tileNameToIds = tileNameToIds;
  }

  public Dictionary<TileIdentificator, TileResource> LoadTiles()
  {
    if (tileSetConfig.TileDefinitions.Count != tileNameToIds.Count)
      throw new ArgumentException($"TileSetConfig defined tiles count should be equal to passed name to id dict: {tileSetConfig.TileDefinitions.Count} != {tileNameToIds.Count}");

    if (!Directory.Exists(imageDirectoryPath))
      throw new DirectoryNotFoundException($"Directory does not exist: {imageDirectoryPath}");

    Dictionary<TileIdentificator, TileResource> tiles = new();
    Dictionary<string, string> imageNamesToPaths = GetImageNamesToPaths(imageDirectoryPath);
    foreach (var (tileName, tileDefinition) in tileSetConfig.TileDefinitions)
    {
      if (!imageNamesToPaths.TryGetValue(tileDefinition.ImageFileName, out string? imagePath))
        throw new ArgumentException($"Missing required tile set image: '{tileDefinition.ImageFileName}' in path: '{imageDirectoryPath}'");

      if (!tileSetConfig.BitmaskSets.TryGetValue(tileDefinition.BitmaskName, out var bitmask))
        throw new ArgumentException($"Missing required bitmask: '{tileDefinition.BitmaskName}' in tileSetConfig");

      tiles[new(tileNameToIds[tileName], tileName)] = new(
        imagePath, tileDefinition.Layer, new(bitmask), tileDefinition.AutoTileGroup);
    }

    return tiles;
  }

  private static Dictionary<string, string> GetImageNamesToPaths(string imageDirectoryPath)
  {
    var imagePaths = FileSystem.GetFilesFromDirectory(imageDirectoryPath, IMAGE_EXTENSIONS, true);
    return imagePaths.ToDictionary(
      p => Path.GetFileNameWithoutExtension(p), p => p);
  }
}