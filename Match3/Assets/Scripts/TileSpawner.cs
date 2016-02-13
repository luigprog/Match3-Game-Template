using UnityEngine;

public class TileSpawner : MonoBehaviour
{
    [SerializeField]
    private TileFactory tileFactory;

    [SerializeField]
    private Transform tileHolder;

    private Grid grid;

    private Cell[] spawnCells;
    private int[] pipeCursor;
    private int pipesCount;

    private static readonly Vector3 PIPE_SPAWN_POSITION_OFFSET = Vector3.up * 0.8f;

    public void Assign(Grid grid)
    {
        this.grid = grid;
        spawnCells = grid.GetSpawnCells();
        pipesCount = spawnCells.Length;
        pipeCursor = new int[pipesCount];
    }

    public void SpawnAtPipe(int index)
    {
        Tile tile = tileFactory.MakeColoredTile(tileFactory.RandomColor());
        tile.AppearWhenReachsY(spawnCells[index].GetPosition().y);
        tile.transform.parent = tileHolder;

        tile.gameObject.transform.position = Application.isPlaying ?
             GetSpawnPositionOfPipe(index) + (Vector3.up * 0.7f * pipeCursor[index]) :
            spawnCells[index].GetPosition();

        pipeCursor[index]++;

        Cell targetCell = grid.GetTargetCellAtColumn(spawnCells[index].xIndex);
        tile.SetCell(targetCell); // temp
        targetCell.AttachTile(tile); // temp

    }

    public Vector3 GetSpawnPositionOfPipe(int index)
    {
        return spawnCells[index].GetPosition() + PIPE_SPAWN_POSITION_OFFSET;
    }

    public bool SpaceAvailableAtPipe(int index)
    {
        return grid.QuantityOfEmptyCellsAtColumn(spawnCells[index].xIndex) > 0;
    }

    public int PipesCount()
    {
        return pipesCount;
    }
}