using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Qwaitumin.GameCore;


namespace Qwaitumin.AutoTile;

[JsonSourceGenerationOptions(
  Converters = new Type[] { typeof(Vector2IntConverter) },
  DefaultIgnoreCondition = JsonIgnoreCondition.Never,
  IncludeFields = true)]
[JsonSerializable(typeof(AutoTileConfig))]
public partial class AutoTileConfigJsonContext : JsonSerializerContext { }

public class AutoTileConfig : JsonSerializable
{
  public int TileSize { get; private set; }
  public ImmutableDictionary<string, TileDefinition> TileDefinitions { get; private set; }
  public ImmutableDictionary<string, ImmutableDictionary<byte, Vector2Int>> BitmaskSets { get; private set; }

  [JsonConstructor]
  public AutoTileConfig(
    int tileSize,
    ImmutableDictionary<string, TileDefinition> tileDefinitions,
    ImmutableDictionary<string, ImmutableDictionary<byte, Vector2Int>> bitmaskSets
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
    Dictionary<string, Dictionary<byte, Vector2Int>> bitmaskSets)
  {
    return new(
      tileSize,
      tileDefinitions.ToImmutableDictionary(),
      bitmaskSets.ToImmutableDictionary(entry => entry.Key, entry => entry.Value.ToImmutableDictionary()));
  }

  private void IntegrityAssertion()
  {
    foreach (var (tileName, tileDefinition) in TileDefinitions)
      if (!BitmaskSets.ContainsKey(tileDefinition.BitmaskName))
        throw new ArgumentException($"Missing required bitmask set: '{tileDefinition.BitmaskName}' in autoTileConfig, for tile definition: '{tileName}'");
  }
}