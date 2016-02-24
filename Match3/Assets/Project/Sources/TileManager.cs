using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The TileManager has all the functionality in the level of a group of tiles. It can spawn tiles,
/// check for matches, communicate with tiles, change the state of tiles, an so on.
/// The TileManager is a pseudo singleton.
/// 
/// ///////////////////////////////////////////////////////////////////////////////////////////////////// 
/// // ##### Tile Spawn #####
/// // 1) Crude Spawn: Instantiate a tile, and perform the initial setup.
/// // 2) Mutation Window: After crude spawn, all tiles are with its creation properties, still mutable. 
/// // So the color of these tiles can be changed, in order to avoid spawning matches for example.
/// // 3) Finalize Spawn: All the tiles will finalize its creation, making its properties immutable,
/// // cannot be changed anymore. This is the last step of spawning a tile.
/// ///////////////////////////////////////////////////////////////////////////////////////////////////// 
/// </summary>
public class TileManager : MonoBehaviour
{
    #region Fields

    private static TileManager instance;
    public const float TILE_SIZE = 0.6f;
    private static readonly Vector3 PIPE_SPAWN_POSITION_OFFSET = Vector3.up * 0.8f;
    private static readonly Color[] COLOREDTILE_COLORS = new Color[] { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta };

    [SerializeField]
    private Grid grid;

    [SerializeField]
    private Transform tileHolder;

    [SerializeField]
    private GameObject coloredTileSpriteObject;

    /// <summary>
    /// Read from the grid, information used when creating the "spawnPipes". Each cell will generate
    /// a spawnPipe. Spawn pipe is an invisible concept.
    /// </summary>
    private Cell[] cellsReferenceForTileSpawner;

    private int spawnPipesCount;

    /// <summary>
    /// Used as reference to stack one tile upon the other, in a spawnPipe. Represents how manny
    /// tiles are being spawned in each spawnPipe, in this session/frame. Each spawnPipe owns one
    /// position of this array.
    /// </summary>
    private int[] spawnPipeCursor;

    /// <summary>
    /// The quantity of tiles existing in the current game.
    /// </summary>
    private int tilesCount;

    private List<Tile> cacheOfMatchedTiles;

    /// <summary>
    /// Tiles must subscribe to this delegate. The delegate gets triggered on
    /// ApplyGravityEffectToTiles(), to communicate all tiles that they must execute gravity effect.
    /// </summary>
    public Action OnApplyGravityEffectToTiles;

    /// <summary>
    /// Counter used to control and identify when gravity effect application finished for all tiles.
    /// </summary>
    private int gravityEffectDoneCounter;

    /// <summary>
    /// Delegate that is triggered when all tiles finished the application of the gravity effect.
    /// </summary>
    public Action OnGravityEffectDoneForAllTiles;

    #endregion

    #region Properties

    public static TileManager Instance { get { return instance; } }

    #endregion

    private void Awake()
    {
        // Pseudo singleton stuff
        instance = this;
    }

    private void Start()
    {
        // Initialize tile spawner functionality, based on the grid
        cellsReferenceForTileSpawner = grid.GetCellsReferencedByTileSpawner();
        spawnPipesCount = cellsReferenceForTileSpawner.Length;
        spawnPipeCursor = new int[spawnPipesCount];
    }

    #region Tile Spawner

    public Vector3 GetSpawnPositionOfPipe(int index)
    {
        return cellsReferenceForTileSpawner[index].Position + PIPE_SPAWN_POSITION_OFFSET;
    }

    public bool ThereIsSpaceAvailableAtSpawnPipe(int index)
    {
        return grid.QuantityOfEmptyCellsAtColumn(cellsReferenceForTileSpawner[index].xIndex) > 0;
    }

    /// <summary>
    /// Spawn(crude) tiles in order to fill all the empty spaces of the grid.
    /// </summary>
    public void FillGridWithTiles()
    {
        for (int i = 0; i < spawnPipesCount; i++)
        {
            while (ThereIsSpaceAvailableAtSpawnPipe(i))
            {
                CrudeSpawnAtPipe(i);
            }
        }
    }

    /// <summary>
    /// The first step of the TileSpawn functionality.
    /// Crude Spawn a random colored tile at a given spawnPipe, attach the tile to the right cell
    /// and place the tile in the pipe so it can fall to its cell later on.
    /// </summary>
    /// <param name="index">The index of the spawnPipe</param>
    public void CrudeSpawnAtPipe(int index)
    {
        GameObject tileGameObject = new GameObject("Tile");
        tileGameObject.transform.parent = tileHolder;

        Tile tile = tileGameObject.AddComponent<Tile>();
        tile.SpawnPipeIndex = index;
        tile.Color = COLOREDTILE_COLORS[UnityEngine.Random.Range(0, COLOREDTILE_COLORS.Length)];
        Tile.AttachTileToCell(tile, grid.GetTheLowerEmptyCellAtColumn(cellsReferenceForTileSpawner[index].xIndex));

        if (Application.isPlaying)
        {
            tile.gameObject.transform.position = GetSpawnPositionOfPipe(index) + (Vector3.up * 0.7f * spawnPipeCursor[index]);
        }
        else
        {
            tile.gameObject.transform.position = tile.Cell.Position;
        }

        spawnPipeCursor[index]++;

        tilesCount++;
    }

    /// <summary>
    /// Iterate through all tiles and randomly mutate its creation properties, for tiles that
    /// are still mutable(not fully created).
    /// </summary>
    public void RandomlyMutateTiles()
    {
        // Iterate the grid, cell by cell, looking for the tile attached to each cell
        for (int i = 0; i < grid.Width; i++)
        {
            for (int j = 0; j < grid.Height; j++)
            {
                Cell cell = grid.Cells[i, j];
                if (cell != null && cell.AttachedTile != null && !cell.AttachedTile.IsFullyCreated)
                {
                    // Tile is mutable, lets randomize its color
                    cell.AttachedTile.Color = COLOREDTILE_COLORS[UnityEngine.Random.Range(0, COLOREDTILE_COLORS.Length)];
                }
            }
        }
    }

    /// <summary>
    /// Iterate through all tiles and finalize the creation of the ones that are still mutable.
    /// Will also reset the state of the tileSpawner for further usage.
    /// </summary>
    public void FinalizeSpawn()
    {
        for (int i = 0; i < grid.Width; i++)
        {
            for (int j = 0; j < grid.Height; j++)
            {
                Tile tile = grid.Cells[i, j].AttachedTile;
                if (!tile.IsFullyCreated)
                {
                    // For tiles that are not fully created(creation properties are still mutable),
                    // finalize the creation
                    tile.FinalizeCreation();
                }
            }
        }

        // Reset tile spawner state.
        // Zero all pipe cursors so we can avoid spawning from too high(cursor always incrementing on spawn)
        for (int i = 0; i < spawnPipeCursor.Length; i++)
        {
            spawnPipeCursor[i] = 0;
        }
    }

    #endregion

    #region Factory

    /// <summary>
    /// Creates and returns a blank sprite of a colored tile. The sprited must be painted by the "client".
    /// </summary>
    public GameObject CreateColoredTileSpriteObject()
    {
        GameObject spriteObject = Instantiate(coloredTileSpriteObject);
        spriteObject.SetActive(true);
        spriteObject.name = "Sprite";
        return spriteObject;
    }

    #endregion

    /// <summary>
    /// Iterate through all tiles and find new valid cell to the ones that are "flying"(has
    /// empty cell below), resulted of a match and clearing session. This arrangement will free
    /// cells in the top of the grid, so new tiles can be spawned and placed in these cells.
    /// Note: This method just arrange the cells of the tiles, a gravity effect must still be called
    ///       later so the tiles can actually fall towards its respective cells.
    /// </summary>
    public void CollapseTiles()
    {
        for (int x = 0; x < grid.Width; x++)
        {
            // Iterate y from down to up
            // Ignore the lower line(bottom), cause its impossible that a tile is in the bottom and
            // is "flying"
            for (int y = grid.Height - 2; y >= 0; y--)
            {
                Cell cell = grid.Cells[x, y];
                if (cell.IsFull() && !grid.Cells[x, y + 1].IsFull())
                {
                    // Empty cell below the tile, lets arrange it
                    Tile tile = cell.AttachedTile;
                    Tile.DettachTileFromCell(tile);
                    Tile.AttachTileToCell(tile, grid.GetTheLowerEmptyCellAtColumn(cell.xIndex));
                }
            }
        }
    }

    /// <summary>
    /// Method called by tiles to communicate TileManager that the respective tile was cleared(destroyed).
    /// </summary>
    public void TellThatTileWasCleared(Tile clearedTile)
    {
        tilesCount--;
    }

    #region Gravity Effect

    /// <summary>
    /// Apply gravity effect for all existing tiles, and starts a counter to identify when all tiles
    /// completed the application of the gravity effect.
    /// </summary>
    public void ApplyGravityEffectToTiles()
    {
        gravityEffectDoneCounter = tilesCount;
        if (OnApplyGravityEffectToTiles != null)
        {
            OnApplyGravityEffectToTiles();
        }
    }

    /// <summary>
    /// Method called by Tiles to communicate TileManager that gravity effect for the respective
    /// tile is done.
    /// </summary>
    public void TellThatTileGravityEffectIsDone()
    {
        gravityEffectDoneCounter--;
        if (gravityEffectDoneCounter == 0)
        {
            // All tiles done!
            if (OnGravityEffectDoneForAllTiles != null)
            {
                OnGravityEffectDoneForAllTiles();
            }
        }
    }

    #endregion

    #region Matches

    /// <summary>
    /// Search for line(match3) matches in the rows and columns of the given tiles. Return the list
    /// of matched tiles.
    /// </summary>
    public List<Tile> GetLineMatchesFromTiles(Tile tileA, Tile tileB)
    {
        List<Tile> matchedTiles = new List<Tile>();

        matchedTiles.AddRange(GetVerticalLineMatches(tileA.Cell.xIndex, previouslyMatchedTiles: matchedTiles));
        matchedTiles.AddRange(GetHorizontalLineMatches(tileA.Cell.yIndex, previouslyMatchedTiles: matchedTiles));

        // Avoid searching twice in the same row or column
        if (tileB.Cell.xIndex != tileA.Cell.xIndex)
        {
            matchedTiles.AddRange(GetVerticalLineMatches(tileB.Cell.xIndex, previouslyMatchedTiles: matchedTiles));
        }

        if (tileB.Cell.yIndex != tileA.Cell.yIndex)
        {
            matchedTiles.AddRange(GetHorizontalLineMatches(tileB.Cell.yIndex, previouslyMatchedTiles: matchedTiles));
        }

        matchedTiles.Distinct();

        return matchedTiles;
    }

    /// <summary>
    /// Search for line(match3) matches in the whole grid(all rows and columns) and return it.
    /// </summary>
    public List<Tile> GetLineMatchesInTheWholeGrid()
    {
        List<Tile> matchedTiles = new List<Tile>();
        for (int i = 0; i < grid.Height; i++)
        {
            matchedTiles.AddRange(GetHorizontalLineMatches(i, previouslyMatchedTiles: matchedTiles));
        }
        for (int i = 0; i < grid.Width; i++)
        {
            matchedTiles.AddRange(GetVerticalLineMatches(i, previouslyMatchedTiles: matchedTiles));
        }
        matchedTiles.Distinct();
        return matchedTiles;
    }

    /// <summary>
    /// Search for line(match3) matches in a given row, and return the list of matched tiles.
    /// </summary>
    /// <param name="rowIndex">The index of the row, yIndex.</param>
    /// <param name="previouslyMatchedTiles">
    /// The list of previously matche tiles, so it can be ignored when looking for matches.
    /// </param>
    public List<Tile> GetHorizontalLineMatches(int rowIndex, List<Tile> previouslyMatchedTiles = null)
    {
        previouslyMatchedTiles = previouslyMatchedTiles ?? new List<Tile>();
        List<Tile> matchedTiles = new List<Tile>();

        int consecutiveCompatibilities = 0;
        // Iterate the row, but with the for loop starting from the second element(element 1), because the logic
        // always compare the current i with the previous i(i-1)
        for (int i = 1; i < grid.Width; i++)
        {
            if (!previouslyMatchedTiles.Contains(grid.Cells[i - 1, rowIndex].AttachedTile) &&
                !previouslyMatchedTiles.Contains(grid.Cells[i, rowIndex].AttachedTile) &&
                Tile.AreMatchCompatible(grid.Cells[i - 1, rowIndex].AttachedTile, grid.Cells[i, rowIndex].AttachedTile))
            {
                // Tile i and Tile i-1 are not contained in the previouslyMatched list, and are
                // match compatible!

                consecutiveCompatibilities++;

                // At least match3 to consider a match, in other words, two consecutive compatibilities
                if (consecutiveCompatibilities == 2)
                {
                    // Then add the current, and the two previous to the list of matched tiles
                    matchedTiles.Add(grid.Cells[i - 2, rowIndex].AttachedTile);
                    matchedTiles.Add(grid.Cells[i - 1, rowIndex].AttachedTile);
                    matchedTiles.Add(grid.Cells[i, rowIndex].AttachedTile);
                }
                else if (consecutiveCompatibilities > 2)
                {
                    // > than match3, lets always add the current to the list
                    matchedTiles.Add(grid.Cells[i, rowIndex].AttachedTile);
                }
            }
            else
            {
                // failed the test, restart consecutiveCompatibilities counter
                consecutiveCompatibilities = 0;
            }
        }

        return matchedTiles;
    }

    /// <summary>
    /// Search for line(match3) matches in a given column, and return the list of matched tiles.
    /// </summary>
    /// <param name="rowIndex">The index of the column, xIndex.</param>
    /// <param name="previouslyMatchedTiles">
    /// The list of previously matche tiles, so it can be ignored when looking for matches.
    /// </param>
    public List<Tile> GetVerticalLineMatches(int columnIndex, List<Tile> previouslyMatchedTiles = null)
    {
        previouslyMatchedTiles = previouslyMatchedTiles ?? new List<Tile>();
        List<Tile> matchedTiles = new List<Tile>();

        int consecutiveCompatibilities = 0;
        // Iterate the column, but with the for loop starting from the second element(element 1), because the logic
        // always compare the current i with the previous i(i-1)
        for (int i = 1; i < grid.Height; i++)
        {
            if (!previouslyMatchedTiles.Contains(grid.Cells[columnIndex, i - 1].AttachedTile) &&
                !previouslyMatchedTiles.Contains(grid.Cells[columnIndex, i].AttachedTile) &&
                Tile.AreMatchCompatible(grid.Cells[columnIndex, i - 1].AttachedTile, grid.Cells[columnIndex, i].AttachedTile))
            {
                // Tile i and Tile i-1 are not contained in the previouslyMatched list, and are
                // match compatible!

                consecutiveCompatibilities++;

                // At least match3 to consider a match, in other words, two consecutive compatibilities
                if (consecutiveCompatibilities == 2)
                {
                    // Then add the current, and the two previous to the list of matched tiles
                    matchedTiles.Add(grid.Cells[columnIndex, i - 2].AttachedTile);
                    matchedTiles.Add(grid.Cells[columnIndex, i - 1].AttachedTile);
                    matchedTiles.Add(grid.Cells[columnIndex, i].AttachedTile);
                }
                else if (consecutiveCompatibilities > 2)
                {
                    // > than match3, lets always add the current to the list
                    matchedTiles.Add(grid.Cells[columnIndex, i].AttachedTile);
                }
            }
            else
            {
                // failed the test, restart consecutiveCompatibilities counter
                consecutiveCompatibilities = 0;
            }
        }

        return matchedTiles;
    }

    /// <summary>
    /// Cache a list of matched tiles for further usage.
    /// </summary>
    public void CacheMatchedTiles(List<Tile> matchedTiles)
    {
        cacheOfMatchedTiles = matchedTiles;
    }

    public List<Tile> GetCacheOfMatchedTiles()
    {
        return cacheOfMatchedTiles;
    }

    public void CleanCacheOfMatchedTiles()
    {
        cacheOfMatchedTiles = null;
    }

    #endregion
}