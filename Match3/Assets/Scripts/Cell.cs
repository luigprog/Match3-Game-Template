using UnityEngine;

public class Cell
{
    public readonly int id;
    public readonly int xIndex;
    public readonly int yIndex;

    private Vector3 position;
    private Tile attachedTile;

    public Cell(int id, int xIndex, int yIndex)
    {
        attachedTile = null;
        position = Vector3.zero;
        this.id = id;
        this.xIndex = xIndex;
        this.yIndex = yIndex;
    }

    public void SetPosition(Vector3 position)
    {
        this.position = position;
    }

    public Vector3 GetPosition()
    {
        return position;
    }

    public bool IsFull()
    {
        return attachedTile != null;
    }

    public void AttachTile(Tile tile)
    {
        attachedTile = tile;
    }

    public Tile GetAttachedTile()
    {
        return attachedTile;
    }
}