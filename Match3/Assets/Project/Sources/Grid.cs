using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A grid is a structured arrangement of cells. The grid properties can be configured by inspector.
/// The grid design is almost the design of a level. It tells how big is the game field, from where
/// tiles are spawned, where tiles are placed. The grid main responsibilities are:
/// - Initialize and Create the cells;
/// - Provide ways to locate and access cells;
/// - Provide information about the state of a cell, so the game logic can react to it.
/// </summary>
#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class Grid : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private int width;

    [SerializeField]
    private int height;

    [SerializeField]
    private float spacement;

    /// <summary>
    /// Cells used as reference "points" for spawnPipes in the TileManager Tile Spawner
    /// functionality. There will be one spawnPipe per cell.
    /// </summary>
    [SerializeField]
    private int[] tileSpawnerCellsSetup;

    [SerializeField]
    private int[] nullCells;

    [SerializeField]
    private int[] pipePathCells;

    [SerializeField]
    private Color gridBorderColor;

    [SerializeField]
    private Color pipeBorderColor;

    /// <summary>
    /// The multidimensinal array that hold all the cells of the grid.
    /// PS: The pivot of this array is top_left, for example:
    ///  => [x]
    /// ||       (0,0) (1,0) (2,0)
    /// \/       (0,1) (1,1) (2,1)
    /// [y]      (0,2) (1,2) (2,2)
    /// </summary>
    private Cell[,] cells;

    private Cell[] tileSpawnerCells;

    #endregion

    #region Properties

    public int Width { get { return width; } }

    public int Height { get { return height; } }

    public Cell[,] Cells { get { return cells; } }

    public float Spacement { get { return spacement; } }

    #endregion

    private void Awake()
    {
        if (Application.isPlaying)
        {
            CreateCells();
            PlaceCells();

            // Draw outlines
            DrawOutlineOfCells(gridBorderColor);
            DrawOutlineOfCells(pipeBorderColor, cell => pipePathCells.Contains(CalculateCellId(cell.xIndex, cell.yIndex)));
        }
    }

    /// <summary>
    /// Create the cells of the grid in the form of a multidimensional array, and configure its id
    /// and indexers, which are used to locate cells in the whole code.
    /// </summary>
    private void CreateCells()
    {
        cells = new Cell[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // The cell's id formula is (y * width) + x
                // x = i, y = j
                int id = CalculateCellId(i, j);
                if (!nullCells.Contains(id))
                {
                    cells[i, j] = new Cell(id: j * width + i, xIndex: i, yIndex: j);
                }
                else
                {
                    // Null/Ignored cell, doesnt exist in the game
                    cells[i, j] = null;
                }
            }
        }

        // Store spawner cells array
        if (tileSpawnerCellsSetup != null)
        {
            tileSpawnerCells = new Cell[tileSpawnerCellsSetup.Length];
            for (int i = 0; i < tileSpawnerCellsSetup.Length; i++)
            {
                Cell spawnerCell = GetCellById(tileSpawnerCellsSetup[i]);
                if (spawnerCell != null)
                {
                    tileSpawnerCells[i] = spawnerCell;
                }
            }
        }
    }


    /// <summary>
    /// Iterate through all cells and set its position in the world. Called in the awake of the Grid
    /// and by the Update function(Edit mode).
    /// </summary>
    private void PlaceCells()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (cells[i, j] != null)
                {
                    cells[i, j].Position = new Vector3(transform.position.x + (i * (TileManager.TILE_SIZE + spacement)),
                                                       transform.position.y - (j * (TileManager.TILE_SIZE + spacement)));
                }
            }
        }
    }

    private int CalculateCellId(int xIndex, int yIndex)
    {
        return (yIndex * width) + xIndex;
    }

    public Cell GetCellById(int id)
    {
        // The cell's id formula is (y * width) + x
        // To find the xIndex from a given id, we do xIndex = id%width
        // To find the yIndex from a given id, we do yIndex = id/width
        return cells[id % width, id / width];
    }

    /// <summary>
    /// Find a cell that is closest to a given position, and return it. If there is no cell close to
    /// the position(the position is too far away), the function will return null.
    /// </summary>
    public Cell GetCellCloseToWorldPosition(Vector3 position)
    {
        // Make sure the given position's z is zero, so it doesn't interfere in the distance calculation
        position.z = 0.0f;

        // Maintain the closest
        Cell closestCell = null;
        float closestSqrDistance = Mathf.Infinity;

        float sqrDistanceConsideredTooFar = ((TileManager.TILE_SIZE / 2) + (spacement / 2)) * ((TileManager.TILE_SIZE / 2) + (spacement / 2));

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (cells[i, j] != null)
                {
                    // PS: Using sqrMagnitude instead of magnitude/distance because it is more performant
                    float currentSqrDistance = Vector3.SqrMagnitude(position - cells[i, j].Position);
                    if (currentSqrDistance < sqrDistanceConsideredTooFar && currentSqrDistance < closestSqrDistance)
                    {
                        closestCell = cells[i, j];
                        closestSqrDistance = currentSqrDistance;
                    }
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
            if (cells[xIndex, i] != null && !cells[xIndex, i].IsFull())
            {
                spacesCounter++;
            }
        }
        return spacesCounter;
    }

    /// <summary>
    /// Find and return the empty cell that is lower located in a given column, iterating the cells
    /// from down to up. Will return null if the column is full(no empty cells).
    /// </summary>
    public Cell GetTheLowerEmptyCellAtColumn(int xIndex)
    {
        Cell lowerEmptyCell = null;
        for (int i = height - 1; i >= 0; i--)
        {
            if (cells[xIndex, i] != null && !cells[xIndex, i].IsFull())
            {
                lowerEmptyCell = cells[xIndex, i];
                break;
            }
        }
        return lowerEmptyCell;
    }

    /// <summary>
    /// Get the array of tiles used as reference in the TileManager tile spawner functionality.
    /// </summary>
    public Cell[] GetTileSpawnerCells()
    {
        return tileSpawnerCells;
    }

    public Cell GetCellNeighborAtDirection(Cell cell, Vector2 neighborDirection)
    {
        Cell neighbor = null;

        if (cell != null)
        {
            if (neighborDirection == Vector2.up && cell.yIndex > 0)
            {
                neighbor = cells[cell.xIndex, cell.yIndex - 1];
            }
            else if (neighborDirection == Vector2.down && cell.yIndex < height - 1)
            {
                neighbor = cells[cell.xIndex, cell.yIndex + 1];
            }
            else if (neighborDirection == Vector2.left && cell.xIndex > 0)
            {
                neighbor = cells[cell.xIndex - 1, cell.yIndex];
            }
            else if (neighborDirection == Vector2.right && cell.xIndex < width - 1)
            {
                neighbor = cells[cell.xIndex + 1, cell.yIndex];
            }
        }

        return neighbor;
    }

    private void DrawOutlineOfCells(Color borderColor, Predicate<Cell> PredicateCellFilter = null)
    {
        List<LineDrawer.LineDrawInfo> outlines = new List<LineDrawer.LineDrawInfo>();
        if (PredicateCellFilter == null)
        {
            // If no filter defined, then lets just create a predicate that always return true, so
            // it wont filter anything
            PredicateCellFilter = cell => true;
        }
        Counter2D counter2d = new Counter2D(width - 1, height - 1);

        for (int i = 0; i < 4; i++)
        {
            #region Setup Test State

            SpriteAlignment drawLineFromCellVertex = SpriteAlignment.TopLeft;
            SpriteAlignment drawLineToCellVertex = SpriteAlignment.TopRight;
            Vector2 directionOfNeighborToLookFor = Vector2.zero;
            // i here is also the index of the state(ITERATE_ROW_DRAW_UP, ITERATE_ROW_DRAW_DOWN...)
            switch (i)
            {
                case 0: // ITERATE_ROW_DRAW_UP:
                    drawLineFromCellVertex = SpriteAlignment.TopLeft;
                    drawLineToCellVertex = SpriteAlignment.TopRight;
                    directionOfNeighborToLookFor = Vector2.up;
                    break;
                case 1: // ITERATE_ROW_DRAW_DOWN:
                    drawLineFromCellVertex = SpriteAlignment.BottomLeft;
                    drawLineToCellVertex = SpriteAlignment.BottomRight;
                    directionOfNeighborToLookFor = Vector2.down;
                    break;
                case 2: // ITERATE_COLUMN_DRAW_LEFT:
                    drawLineFromCellVertex = SpriteAlignment.TopLeft;
                    drawLineToCellVertex = SpriteAlignment.BottomLeft;
                    directionOfNeighborToLookFor = Vector2.left;
                    break;
                case 3: // ITERATE_COLUMN_DRAW_RIGHT:
                    drawLineFromCellVertex = SpriteAlignment.TopRight;
                    drawLineToCellVertex = SpriteAlignment.BottomRight;
                    directionOfNeighborToLookFor = Vector2.right;
                    break;
            }

            // Invert the outer loop when iterating the columns, instead of the rows(default/first case)
            // ITERATE_COLUMN_DRAW_LEFT aka 2 || ITERATE_COLUMN_DRAW_RIGHT aka 3
            counter2d.InvertDimensions(i == 2 || i == 3);
            counter2d.Reset();

            #endregion

            #region The real test, Algorithm that analyze the cells and create the outline info

            LineDrawer.LineDrawInfo currentLine = null;
            Cell lastCellThatDidSomething = null;

            while (!counter2d.Completed())
            {
                int x = counter2d.I;
                int y = counter2d.J;
                Cell neighborCell = GetCellNeighborAtDirection(cells[x, y], directionOfNeighborToLookFor);

                if (cells[x, y] != null &&
                    PredicateCellFilter(cells[x, y]) &&
                    (neighborCell == null || !PredicateCellFilter(neighborCell)))
                {
                    if (currentLine == null)
                    {
                        currentLine = new LineDrawer.LineDrawInfo();
                        currentLine.From = cells[x, y].GetVertex(drawLineFromCellVertex);
                        currentLine.To = cells[x, y].GetVertex(drawLineToCellVertex);
                    }
                    else
                    {
                        currentLine.To = cells[x, y].GetVertex(drawLineToCellVertex);
                    }
                    lastCellThatDidSomething = cells[x, y];
                }
                else
                {
                    if (currentLine != null && lastCellThatDidSomething != null)
                    {
                        currentLine.To = lastCellThatDidSomething.GetVertex(drawLineToCellVertex);
                        outlines.Add(currentLine);
                        currentLine = null;
                    }
                }

                if (counter2d.SecondaryDimensionCompleted() && currentLine != null)
                {
                    outlines.Add(currentLine);
                    currentLine = null;
                }

                counter2d.Advance();
            }
            #endregion
        }

        // Finally, actually draw the outline/lines
        for (int i = 0; i < outlines.Count; i++)
        {
            LineDrawer.Instance.DrawLine(outlines[i], borderColor, 0.03f);
        }
    }

    #region Editor
#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying)
        {
            CreateCells();
            PlaceCells();
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
                        if (cells[i, j] != null)
                        {
                            Color color = Color.blue;
                            color.a = 0.5f;
                            Gizmos.color = color;
                            Gizmos.DrawCube(cells[i, j].Position, Vector3.one * TileManager.TILE_SIZE);
                        }
                    }
                }

                // Tile Spawner
                for (int i = 0; i < tileSpawnerCellsSetup.Length; i++)
                {
                    Color color = Color.red;
                    color.a = 0.5f;
                    Gizmos.color = color;
                    Gizmos.DrawCube(GetCellById(tileSpawnerCellsSetup[i]).Position, Vector3.one * TileManager.TILE_SIZE * 0.2f);
                }

                // Pipe Path
                for (int i = 0; i < pipePathCells.Length; i++)
                {
                    Color color = Color.green;
                    color.a = 0.5f;
                    Gizmos.color = color;
                    Gizmos.DrawCube(GetCellById(pipePathCells[i]).Position, Vector3.one * TileManager.TILE_SIZE * 0.5f);
                }
            }
        }
    }
#endif 
    #endregion
}