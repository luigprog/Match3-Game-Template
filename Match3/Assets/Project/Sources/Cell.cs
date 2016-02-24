using UnityEngine;

/// <summary>
/// The cell is a piece of the grid.
/// A cell can be empty or hold one Tile.
/// </summary>
public class Cell
{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Imutable data, initialized by constructor, called by Grid when Initializing
    // The cell can be referenced either by (id) or by (xIndex, yIndex), depending
    // on the case, one manner will be more appropriate than the other.
    ///////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// The id of the cell, resulted by a calculation using the xIndex and the yIndex.
    /// The id formula is: (yIndex * Grid.Width) + xIndex
    /// </summary>
    public readonly int id;
    /// <summary>
    /// The x index of the cell, based on the Grid.cells[,] coordinates.
    /// </summary>
    public readonly int xIndex;
    /// <summary>
    /// The y index of the cell, based on the Grid.cells[,] coordinates.
    /// </summary>
    public readonly int yIndex;
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// The position of the cell in world space.
    /// Set by the Grid on initialization.
    /// </summary>
    private Vector3 position;

    private Tile attachedTile;

    public Tile AttachedTile { get { return attachedTile; } }

    /// <summary>
    /// The position of the cell in world space.
    /// Set by the Grid on initialization.
    /// </summary>
    public Vector3 Position { get { return position; } set { position = value; } }

    public Cell(int id, int xIndex, int yIndex)
    {
        attachedTile = null;
        position = Vector3.zero;
        this.id = id;
        this.xIndex = xIndex;
        this.yIndex = yIndex;
    }

    /// <summary>
    /// Return if cell is full or empty.
    /// If there is a tile attached, the cell is full.
    /// </summary>
    public bool IsFull()
    {
        return attachedTile != null;
    }

    public void AttachTile(Tile tile)
    {
        attachedTile = tile;
    }

    public void DetachTile()
    {
        attachedTile = null;
    }
}