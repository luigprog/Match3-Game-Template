using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField]
    private int xSize;

    [SerializeField]
    private int ySize;

    [SerializeField]
    private float spacement;

    [SerializeField]
    private int[] spawnCellsIds;

    private Cell[,] cells;

    private readonly Vector3 CELL_GIZMO_SIZE = new Vector3(1.0f, 1.0f, 1.0f);

    public void F2()
    {
        cells = new Cell[xSize, ySize];

        for (int i = 0; i < xSize; i++)
        {
            for (int j = 0; j < ySize; j++)
            {
                // The cell id formula is (y * width) + x
                cells[i, j] = new Cell(j * xSize + i, i, j, new Vector2(transform.position.x + (i * spacement), transform.position.y - (j * spacement)));
            }
        }
    }

    public Cell[] GetSpawnCells()
    {
        Cell[] spawnCells = new Cell[spawnCellsIds.Length];
        for (int i = 0; i < spawnCellsIds.Length; i++)
        {
            spawnCells[i] = GetCellById(spawnCellsIds[i]);
        }
        return spawnCells;
    }

    public Cell GetCellById(int id)
    {
        return cells[id % xSize, id / xSize];
    }

#if UNITY_EDITOR
    [ContextMenu("F")]
    private void F()
    {
        F2();
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            if (cells != null)
            {
                for (int i = 0; i < xSize; i++)
                {
                    for (int j = 0; j < ySize; j++)
                    {
                        Gizmos.DrawCube(cells[i, j].position, CELL_GIZMO_SIZE);
                    }
                }
            }
        }
    }
#endif

}

public struct Cell
{
    public readonly int id;
    public readonly int xIndex;
    public readonly int yIndex;
    public readonly Vector3 position;
    private bool isFull;

    public Cell(int id, int xIndex, int yIndex, Vector3 position)
    {
        isFull = false;
        this.id = id;
        this.xIndex = xIndex;
        this.yIndex = yIndex;
        this.position = position;
    }
}