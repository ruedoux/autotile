using Godot;
using Qwaitumin.AutoTile.GUI.Core;
using Qwaitumin.AutoTile.GUI.Core.GodotBindings;
using Qwaitumin.AutoTile.GUI.Core.Signals;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Qwaitumin.AutoTile.GUI.Scenes.Editor.Tiles;

public partial class EditorTiles : MarginContainer, IState
{
  private static readonly PackedScene tileScene = ResourceLoader.Load<PackedScene>(
    "uid://1pokk3kl6wjp");

  public readonly GodotInputListener InputListener = new();
  public readonly EventNotifier<GuiTile> ChangedActiveTile = new();
  public readonly EventNotifier<GuiTile> TileDeleted = new();
  public readonly EventNotifier<Color> TileColorChanged = new();
  public GuiTile? ActiveTile { private set; get; } = null;
  public readonly HashSet<GuiTile> CreatedTiles = [];

  private Control tileList = null!;
  private Button addTileButton = null!;


  public override void _Ready()
  {
    tileList = GetNode<Control>("V/ScrollContainer/List");
    addTileButton = GetNode<Button>("V/Add");

    addTileButton.Pressed += AddTile;

    AddTile();
    ActiveTile = CreatedTiles.First();
    ChangeActiveTile(CreatedTiles.First());
  }

  public void ClearAll()
  {
    Editor.Logger.Log("> Starting clearing tiles");
    foreach (var guiTile in CreatedTiles)
      RemoveTile(guiTile.TileName);
    ActiveTile = null;
    Editor.Logger.Log("> Finished clearing tiles");
  }

  public Dictionary<string, Color> GetTileNamesToColors()
    => CreatedTiles.ToDictionary(
      guiTile => guiTile.TileName,
      guiTile => guiTile.ColorPickerButton.Color);

  public void EndState()
    => Hide();

  public void InitializeState()
    => Show();

  public void AddTile(int tileId, string tileName, Color color)
  {
    if (CreatedTiles.Any(guiTile => guiTile.TileId == tileId))
    {
      Editor.Logger.LogError($"Cannot create tile with already taken id: '{tileId}'");
      return;
    }
    if (CreatedTiles.Any(guiTile => guiTile.TileName == tileName))
    {
      Editor.Logger.LogError($"Cannot create tile with already taken name: '{tileName}'");
      return;
    }

    GuiTile tileInstance = tileScene.Instantiate<GuiTile>();
    tileList.AddChild(tileInstance);

    tileInstance.TileId = tileId;
    tileInstance.TileName = tileName;
    tileInstance.TileNameEdit.Text = tileName;
    tileInstance.ColorPickerButton.Color = color;

    tileInstance.TryDeleteNotifier.AddObserver(RemoveTile);
    tileInstance.TryChangeTileNameNotifier.AddObserver(TryChangeTileName);
    tileInstance.SelectActiveTileNotifier.AddObserver(ChangeActiveTile);
    tileInstance.ColorPickerButton.ColorChanged += TileColorChanged.NotifyObservers;

    CreatedTiles.Add(tileInstance);
    Editor.Logger.Log($"Added new tile: {tileName}");
  }

  private void AddTile()
  {
    Random random = new();
    AddTile(
      GetNextFreeTileId(),
      GetNewTileName(new(CreatedTiles.Select(x => x.TileName))),
      new Color(
        r: (float)random.NextDouble(),
        g: (float)random.NextDouble(),
        b: (float)random.NextDouble(),
        a: 0.7f));
  }

  private void ChangeActiveTile(GuiTile tile)
  {
    if (ActiveTile is not null)
      ActiveTile.SelectButton.Modulate = Colors.White;
    tile.SelectButton.Modulate = new(r: 0, g: 2, b: 0);
    ActiveTile = tile;
    ChangedActiveTile.NotifyObservers(tile);
    Editor.Logger.Log($"Changed active tile: {tile.TileName}");
  }

  private void TryChangeTileName(Tuple<GuiTile, string> tileAndName)
  {
    GuiTile tile = tileAndName.Item1;
    string newTileName = tileAndName.Item2;
    string oldTileName = tile.TileName;

    Dictionary<string, GuiTile> tileNamesToGuiTiles = new(
      CreatedTiles.ToDictionary(guiTile => guiTile.TileName, guiTile => guiTile));
    if (tileNamesToGuiTiles.ContainsKey(newTileName))
    {
      if (tileNamesToGuiTiles[newTileName] == tile) return;
      newTileName = GetNewTileName(new(tileNamesToGuiTiles.Keys), newTileName + "-copy");
    }

    tile.TileName = newTileName;
    tile.TileNameEdit.Text = newTileName;
    Editor.Logger.Log($"Changed tile name from {oldTileName} to {newTileName}");
  }

  private void RemoveTile(string tileName)
  {
    var tileToDelete = CreatedTiles.FirstOrDefault(guiTile => guiTile.TileName == tileName);
    if (tileToDelete is null)
    {
      Editor.Logger.Log($"Tile cannot be deleted, tile name not found: '{tileName}'");
      return;
    }

    tileList.RemoveChild(tileToDelete);
    TileDeleted.NotifyObservers(tileToDelete);
    CreatedTiles.Remove(tileToDelete);
    tileToDelete.ColorPickerButton.ColorChanged -= TileColorChanged.NotifyObservers;
    tileToDelete.QueueFree();

    if (tileToDelete == ActiveTile)
      ActiveTile = null;
    Editor.Logger.Log($"Removed tile {tileName}");
  }

  private int GetNextFreeTileId()
  {
    var assignedIdsSet = CreatedTiles
      .Select(guiTile => guiTile.TileId)
      .ToHashSet();

    int nextFreeId = 0;
    while (assignedIdsSet.Contains(nextFreeId))
      nextFreeId++;

    return nextFreeId;
  }

  private static string GetNewTileName(HashSet<string> names, string defaultName = "Tile")
  {
    string newName = defaultName;
    int index = 0;
    while (names.Contains(newName))
      newName = defaultName + index++.ToString();
    return newName;
  }
}
