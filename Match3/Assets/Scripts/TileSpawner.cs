using UnityEngine;

public class TileSpawner : MonoBehaviour
{
    private Grid grid;
    private Vector3[] spawnPositions;

    public void Assign(Grid grid)
    {
        this.grid = grid;
        Cell[] spawnCells = grid.GetSpawnCells();
        spawnPositions = new Vector3[spawnCells.Length];
        for (int i = 0; i < spawnCells.Length; i++)
        {
            spawnPositions[i] = spawnCells[i].position + Vector3.up;
        }
    }

    public void SpawnAt(int index)
    {

    }
}
