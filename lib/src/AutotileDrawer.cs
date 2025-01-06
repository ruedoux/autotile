using GameCore;

namespace AutoTile;

public interface ITileMapDrawer
{
  public void Clear();
  public void DrawTiles(int tileLayer, KeyValuePair<Vector2Int, TileData?>[] positionsToTileData);
}

public class AutotileDrawer
{
  private readonly ITileMapDrawer tileMapDrawer;
  private readonly AutoTiler autoTiler;
  private readonly HashSet<Task> tasks = new();

  public AutotileDrawer(ITileMapDrawer tileMapDrawer, AutoTiler autoTiler)
  {
    this.tileMapDrawer = tileMapDrawer;
    this.autoTiler = autoTiler;
  }

  public void Clear()
    => autoTiler.Clear();

  public void Wait()
  {
    Task.WhenAll(tasks).Wait();
    ClearFinishedTasks();
  }

  public void DrawTilesAsync(int layer, KeyValuePair<Vector2Int, int>[] positionToTileIds)
  {
    ClearFinishedTasks();
    tasks.Add(Task.Run(() => DrawTiles(layer, positionToTileIds)));
  }

  public void DrawTiles(int layer, KeyValuePair<Vector2Int, int>[] positionToTileIds)
  {
    List<Vector2Int> positions = new();
    foreach (var (position, tileId) in positionToTileIds)
    {
      autoTiler.PlaceTile(layer, position, tileId);
      positions.Add(position);
    }

    UpdateTiles(layer, positions.ToArray());
  }

  public void UpdateTiles(int tileLayer, Vector2Int[] positions)
  {
    List<KeyValuePair<Vector2Int, TileData?>> tileLayerToData = new();
    foreach (var position in positions)
      tileLayerToData.Add(new(position, autoTiler.GetTile(tileLayer, position)));

    tileMapDrawer.DrawTiles(tileLayer, tileLayerToData.ToArray());
  }

  private void ClearFinishedTasks()
  {
    List<Task> tasksToRemove = new();
    foreach (var task in tasks)
      if (task.IsCompleted)
        tasksToRemove.Add(task);

    foreach (var task in tasksToRemove)
      tasks.Remove(task);
  }
}