using System.Collections.Immutable;
using System.Text.Json.Serialization;
using GameCore;

namespace AutoTile;

[JsonSourceGenerationOptions(
  Converters = new Type[] { typeof(Vector2IntImmutableDictionaryConverter<byte>) },
  DefaultIgnoreCondition = JsonIgnoreCondition.Never,
  IncludeFields = true)]
[JsonSerializable(typeof(AutoTileConfig))]
public partial class AutoTileConfigJsonContext : JsonSerializerContext { }

public class AutoTileConfig : JsonSerializable
{
  public int TileSize { get; private set; }
  public ImmutableDictionary<string, TileDefinition> TileDefinitions { get; private set; }
  public ImmutableDictionary<string, ImmutableDictionary<Vector2Int, byte>> BitmaskSets { get; private set; }

  [JsonConstructor]
  public AutoTileConfig(
    int tileSize,
    ImmutableDictionary<string, TileDefinition> tileDefinitions,
    ImmutableDictionary<string, ImmutableDictionary<Vector2Int, byte>> bitmaskSets
  ) : base(AutoTileConfigJsonContext.Default.AutoTileConfig)
  {
    TileSize = tileSize;
    TileDefinitions = tileDefinitions;
    BitmaskSets = bitmaskSets;

    IntegrityAssertion();
  }

  public static AutoTileConfig Construct(
    int tileSize,
    Dictionary<string, TileDefinition> tileDefinitions,
    Dictionary<string, Dictionary<Vector2Int, byte>> bitmaskSets)
  {
    return new(
      tileSize,
      tileDefinitions.ToImmutableDictionary(),
      bitmaskSets.ToImmutableDictionary(entry => entry.Key, entry => entry.Value.ToImmutableDictionary()));
  }

  public static AutoTileConfig LoadFromFile(string path)
    => LoadObjectFromFile(path, AutoTileConfigJsonContext.Default.AutoTileConfig);

  private void IntegrityAssertion()
  {
    foreach (var (name, tileDefinition) in TileDefinitions)
      if (!BitmaskSets.ContainsKey(tileDefinition.BitmaskName))
        throw new ArgumentException($"Missing required bitmask: '{tileDefinition.BitmaskName}' in autoTileConfig");
  }
}