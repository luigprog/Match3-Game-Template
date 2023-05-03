using EZCameraShake;
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

    /// <summary>
    /// Once a tile is fully created, its creation properties like color, become immutable.
    /// </summary>
    private bool isFullyCreated;

    private SpriteRenderer spriteRenderer;

    private bool gravityActing;
    private bool bouncedOnce;
    private float gravityForce;
    private float bounceForce;

    private bool isBomb;

    private const float INITIAL_GRAVITY_FORCE = 2.0f;
    private const float FINAL_GRAVITY_FORCE = 7.5f;
    private const float BOUNCE_INTENSITY = 3.8f;

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

    public bool IsBomb
    {
        get { return isBomb; }
        set
        {
            if (!isFullyCreated)
            {
                isBomb = value;
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
            GameObject spriteObject;

            // Create and add the sprite
            if (!isBomb)
            {
                spriteObject = TileManager.Instance.CreateColoredTileSpriteObject();
            }
            else
            {
                // Temp
                spriteObject = TileManager.Instance.CreateBombTileSpriteObject();
            }

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
            if (gravityForce < FINAL_GRAVITY_FORCE)
            {
                gravityForce += 12.0f * Time.deltaTime;
                if (gravityForce > FINAL_GRAVITY_FORCE)
                {
                    gravityForce = FINAL_GRAVITY_FORCE;
                }
            }

            if (bounceForce > 0.0f)
            {
                bounceForce -= 12.0f * Time.deltaTime;
                if (bounceForce < 0.0f)
                {
                    bounceForce = 0.0f;
                }
            }

            Vector3 gravityVector = Vector3.down * gravityForce;
            Vector3 bounceVector = Vector3.up * bounceForce;
            transform.position += (gravityVector + bounceVector) * Time.deltaTime;

            // Tile position overpassed cell position?
            if (transform.position.y < cell.Position.y)
            {
                // Yes, lets fix the position
                // Land in the desired position(cell position)
                transform.position = cell.Position;
                if (!bouncedOnce)
                {
                    // First time reaching the cell position, not bounced yet
                    // In the gravity effect of the tile, it must bounce once
                    // Lets setup the bounce effect
                    bouncedOnce = true;
                    gravityForce = INITIAL_GRAVITY_FORCE;
                    bounceForce = BOUNCE_INTENSITY;
                }
                else
                {
                    // Landing for the second time, already bounced
                    // So, gravity effect is done!
                    gravityActing = false;
                    // Tell the TileManager that we are done
                    TileManager.Instance.TellThatTileGravityEffectIsDone();
                }
            }
        }
    }

    // Temp activate
    public void Activate(Tile activatorTile)
    {
        if (isBomb)
        {
            if (!activatorTile.isBomb)
            {
                TileManager.Instance.MatchAllTilesOfColor(activatorTile.Color);
                TileManager.Instance.AddToCacheOfMatchedTiles(this);
            }
        }
    }

    public bool IsMatchCompatibleWith(Tile otherTile)
    {
        return !isBomb && color == otherTile.Color;
    }

    /// <summary>
    /// Apply the clearing effect of the tile. Called after the tile is matched, and is about to be destroyed.
    /// </summary>
    public void Clear()
    {
        if (!isBomb)
        {
            ParticlesManager.Instance.PlayTileDestructionParticle(color, transform.position);
        }
        else
        {
            // Temp bomb effect
            CameraShaker.Instance.ShakeOnce(6.0f, 2.0f, 0.1f, 0.1f);
            ParticlesManager.Instance.PlayTileDestructionParticle(color, transform.position);
        }

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
            gravityForce = INITIAL_GRAVITY_FORCE;
            bounceForce = 0.0f;
            bouncedOnce = false;
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
}