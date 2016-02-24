using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Tiles are the main gameplay object of a match 3 game. A tile is an object that can fill a cell.
/// Tiles are created(1:crude, 2: finalization), moved(by player), matched and cleared(destroyed).
/// The tile class represents the characteristics  of a tile, and behaviors related to itself only.
/// For behaviors that need or affect other tiles, a Tile must communicate with TileManager to
/// perform it. Tile.cs is attached to each tile object in the game. A tile is direct in
/// communication with TileManager. Tile.cs also hold some static methods that are common for all tiles.
/// </summary>
public class Tile : MonoBehaviour
{
    #region Fields

    /// <summary>
    /// Cell that this tile is attached to. 
    /// </summary>
    private Cell cell;

    private Color color;

    /// <summary>
    /// The spawnPipe index from where this tile was spawned.
    /// </summary>
    private int spawnPipeIndex;

    private SpriteRenderer spriteRenderer;

    private bool gravityActing;

    /// <summary>
    /// Once a tile is fully created, its creation properties like color, become immutable.
    /// </summary>
    private bool isFullyCreated;

    private const float GRAVITY_FORCE = 6.0f;

    #endregion

    #region Properties

    public bool IsFullyCreated { get { return isFullyCreated; } }

    public Cell Cell { get { return cell; } set { cell = value; } }

    public Color Color
    {
        get { return color; }
        set
        {
            if (!isFullyCreated)
            {
                color = value;
            }
        }
    }

    public int SpawnPipeIndex
    {
        get { return spawnPipeIndex; }
        set
        {
            if (!isFullyCreated)
            {
                spawnPipeIndex = value;
            }
        }
    }

    #endregion

    private void Awake()
    {
        TileManager.Instance.OnApplyGravityEffectToTiles += OnApplyGravityEffectToTiles;
    }

    /// <summary>
    /// Finalize the creation of the tile, applying the setup of critical properties(color), adding
    /// and painting the sprite, becoming visible. Once this method is called, some data cannot be
    /// mutated any more.
    /// </summary>
    public void FinalizeCreation()
    {
        if (!isFullyCreated)
        {
            // Create and add the sprite
            GameObject spriteObject = TileManager.Instance.CreateColoredTileSpriteObject();
            spriteObject.transform.parent = transform;
            spriteObject.transform.localPosition = Vector3.zero;
            spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
            spriteRenderer.color = color;

            // Apply the entrance effect
            StartCoroutine(CoroutineFadeEntranceEffect());

            // Fully created, become imutable
            isFullyCreated = true;
        }
    }

    private void OnDestroy()
    {
        TileManager.Instance.OnApplyGravityEffectToTiles -= OnApplyGravityEffectToTiles;
    }

    private void Update()
    {
        if (gravityActing)
        {
            // Gravity Effect
            transform.position += Vector3.down * GRAVITY_FORCE * Time.deltaTime;
            if (transform.position.y < cell.Position.y)
            {
                // Land in the desired position/cell position
                transform.position = cell.Position;
                gravityActing = false;
                // Tell the TileManager that we are done
                TileManager.Instance.TellThatTileGravityEffectIsDone();
            }
        }
    }

    /// <summary>
    /// Apply the clearing effect of the tile. Called after the tile is matched, and is about to be destroyed.
    /// </summary>
    public void Clear()
    {
        Destroy(gameObject);

        TileManager.Instance.TellThatTileWasCleared(this);
    }

    /// <summary>
    /// Callback of the delegate TileManager.Instance.OnApplyGravityEffectToTiles, applying the
    /// gravity effect to this tile, if needed.
    /// </summary>
    private void OnApplyGravityEffectToTiles()
    {
        if (Vector3.SqrMagnitude(transform.position - cell.Position) > Mathf.Epsilon)
        {
            // Not in the cell position, need to fall
            gravityActing = true;
        }
        else
        {
            // Already in the desired position, there is no need to fall
            gravityActing = false;
            // Tell the TileManager that we are done
            TileManager.Instance.TellThatTileGravityEffectIsDone();
        }
    }

    /// <summary>
    /// Coroutine that apply the effect of fade in alpha, when the tile is about to fall(gravity)
    /// and cross its spawnPipe position.
    /// </summary>
    private IEnumerator CoroutineFadeEntranceEffect()
    {
        // Maintain the color, but change the alpha so it becomes transparent
        Color colorWithAlphaZero = spriteRenderer.color;
        colorWithAlphaZero.a = 0.0f;
        spriteRenderer.color = colorWithAlphaZero;
        spriteRenderer.gameObject.SetActive(true);

        float pipeY = TileManager.Instance.GetSpawnPositionOfPipe(spawnPipeIndex).y;

        // Wait until tile cross the spawnPipe position
        yield return new WaitUntil(() => transform.position.y < pipeY);

        // Then, apply the fade
        const float FADE_IN_TIME = 0.2f;
        float timer = 0.0f;
        bool animationDone = false;
        while (!animationDone)
        {
            timer += Time.deltaTime;
            float ratio = Mathf.Clamp01(timer / FADE_IN_TIME);
            if (Mathf.Approximately(ratio, 1.0f))
            {
                ratio = 1.0f;
                animationDone = true;
            }
            Color tempColor = color;
            tempColor.a = ratio;
            spriteRenderer.color = tempColor;
            yield return new WaitForEndOfFrame();
        }
    }

    /// <summary>
    /// Will move the tile linearly to its cell position, in the given time.
    /// </summary>
    /// <param name="animationTime">The duration of the animation.</param>
    /// <param name="onDone">Delegate triggered when the animation is done.</param>
    public void AnimateMovementToCellPosition(float animationTime, Action onDone = null)
    {
        StartCoroutine(CoroutineAnimateMovementToCellPosition(animationTime, onDone));
    }

    private IEnumerator CoroutineAnimateMovementToCellPosition(float animationTime, Action onDone)
    {
        Vector3 from = transform.position;
        Vector3 to = cell.Position;

        float timer = 0.0f;
        bool animationDone = false;
        while (!animationDone)
        {
            timer += Time.deltaTime;
            float ratio = Mathf.Clamp01(timer / animationTime);
            if (Mathf.Approximately(ratio, 1.0f))
            {
                ratio = 1.0f;
                animationDone = true;
            }

            transform.position = Vector3.Lerp(from, to, ratio);

            yield return new WaitForEndOfFrame();
        }

        if (onDone != null)
        {
            onDone();
        }
    }

    public static void AttachTileToCell(Tile tile, Cell cell)
    {
        tile.Cell = cell;
        cell.AttachTile(tile);
    }

    public static void DettachTileFromCell(Tile tile)
    {
        tile.Cell.DetachTile();
        tile.Cell = null;
    }

    public static void SwapTiles(Tile tileA, Tile tileB)
    {
        Cell oldCellOfTileA = tileA.Cell;
        AttachTileToCell(tileA, tileB.Cell);
        AttachTileToCell(tileB, oldCellOfTileA);
    }

    /// <summary>
    /// Returns if tileA and tileB are cross neighbors.
    /// O X O
    /// X X X      <== This type of cross +
    /// O X O
    /// </summary>
    public static bool AreCrossNeighbors(Tile tileA, Tile tileB)
    {
        return (tileA.cell.xIndex == tileB.cell.xIndex && Mathf.Abs(tileA.cell.yIndex - tileB.cell.yIndex) == 1)
            || (tileA.cell.yIndex == tileB.cell.yIndex && Mathf.Abs(tileA.cell.xIndex - tileB.cell.xIndex) == 1);
    }

    /// <summary>
    /// Returns if tileA and tileB can be matched.
    /// </summary>
    public static bool AreMatchCompatible(Tile tileA, Tile tileB)
    {
        return tileA.Color == tileB.Color;
    }
}