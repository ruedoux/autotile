using System.Collections.Immutable;
using System.Text.Json.Serialization;
using GameCore;

namespace Autotile;

[JsonSourceGenerationOptions(
  Converters = new Type[] { typeof(Vector2IntImmutableDictionaryConverter<byte>) },
  DefaultIgnoreCondition = JsonIgnoreCondition.Never,
  IncludeFields = true)]
[JsonSerializable(typeof(AutotileConfig))]
public partial class AutotileConfigJsonContext : JsonSerializerContext { }

public class AutotileConfig : JsonSerializable
{
  public int TileSize { get; private set; }
  public ImmutableDictionary<string, TileDefinition> TileDefinitions { get; private set; }
  public ImmutableDictionary<string, ImmutableDictionary<Vector2Int, byte>> BitmaskSets { get; private set; }

  [JsonConstructor]
  public AutotileConfig(
    int tileSize,
    ImmutableDictionary<string, TileDefinition> tileDefinitions,
    ImmutableDictionary<string, ImmutableDictionary<Vector2Int, byte>> bitmaskSets
  ) : base(AutotileConfigJsonContext.Default.AutotileConfig)
  {
    TileSize = tileSize;
    TileDefinitions = tileDefinitions;
    BitmaskSets = bitmaskSets;

    IntegrityAssertion();
  }

  public static AutotileConfig Construct(
    int tileSize,
    Dictionary<string, TileDefinition> tileDefinitions,
    Dictionary<string, Dictionary<Vector2Int, byte>> bitmaskSets)
  {
    return new(
      tileSize,
      tileDefinitions.ToImmutableDictionary(),
      bitmaskSets.ToImmutableDictionary(entry => entry.Key, entry => entry.Value.ToImmutableDictionary()));
  }

  public static AutotileConfig LoadFromFile(string path)
    => LoadObjectFromFile(path, AutotileConfigJsonContext.Default.AutotileConfig);

  private void IntegrityAssertion()
  {
    foreach (var (name, tileDefinition) in TileDefinitions)
      if (!BitmaskSets.ContainsKey(tileDefinition.BitmaskName))
        throw new ArgumentException($"Missing required bitmask: '{tileDefinition.BitmaskName}' in autotileConfig");
  }
}