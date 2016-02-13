using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class Grid : MonoBehaviour
{
    [SerializeField]
    private int width;

    [SerializeField]
    private int height;

    [SerializeField]
    private float spacement;

    [SerializeField]
    private int[] spawnCellsIds;

    private Cell[,] cells;
    private readonly Vector3 CELL_GIZMO_SIZE = new Vector3(0.6f, 0.6f, 0.6f);

    private void Awake()
    {
        InitializeCells();
        SetupCellsPositions();
    }

    private void InitializeCells()
    {
        cells = new Cell[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // The cell id formula is (y * width) + x
                cells[i, j] = new Cell(j * width + i, i, j);
            }
        }
    }

    private void SetupCellsPositions()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                cells[i, j].SetPosition(new Vector3(transform.position.x + (i * spacement), transform.position.y - (j * spacement)));
            }
        }
    }

    public bool IsSpawnCell(Cell cell)
    {
        bool isSpawnCell = false;
        for (int i = 0; i < spawnCellsIds.Length; i++)
        {
            if (cell.id == spawnCellsIds[i])
            {
                isSpawnCell = true;
                break;
            }
        }
        return isSpawnCell;
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
        return cells[id % width, id / width];
    }

    public Cell GetCellAtPosition(Vector3 position)
    {
        position.z = 0.0f;
        Cell closestCell = null;
        float closestSqrDistance = Mathf.Infinity;
        float MAX_SQRDISANCE = 1.0f;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float currentSqrDistance = Vector3.SqrMagnitude(position - cells[i, j].GetPosition());
                if (currentSqrDistance < MAX_SQRDISANCE && currentSqrDistance < closestSqrDistance)
                {
                    closestCell = cells[i, j];
                    closestSqrDistance = currentSqrDistance;
                }
            }
        }

        return closestCell;
    }

    public int QuantityOfEmptyCellsAtColumn(int xIndex)
    {
        int spacesCounter = 0;
        for (int i = 0; i < height; i++)
        {
            if (!cells[xIndex, i].IsFull())
            {
                spacesCounter++;
            }
        }
        return spacesCounter;
    }

    public Cell GetTargetCellAtColumn(int xIndex)
    {
        Cell targetCell = null;
        for (int i = height - 1; i >= 0; i--)
        {
            if (!cells[xIndex, i].IsFull())
            {
                targetCell = cells[xIndex, i];
                break;
            }
        }
        return targetCell;
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying)
        {
            if (cells == null || cells.GetLength(0) != width || cells.GetLength(1) != height)
            {
                InitializeCells();
            }
            SetupCellsPositions();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            if (cells != null)
            {
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        Color color = IsSpawnCell(cells[i, j]) ? Color.red : Color.blue;
                        color.a = 0.5f;
                        Gizmos.color = color;
                        Gizmos.DrawCube(cells[i, j].GetPosition(), CELL_GIZMO_SIZE);
                    }
                }
            }
        }
    }
#endif

}