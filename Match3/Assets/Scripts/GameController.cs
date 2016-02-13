using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private Grid grid;

    [SerializeField]
    private TileSpawner tileSpawner;

    private void Start()
    {
        tileSpawner.Assign(grid);
        FillGridWithTiles();
    }

    private void FillGridWithTiles()
    {
        int pipesCount = tileSpawner.PipesCount();
        for (int i = 0; i < pipesCount; i++)
        {
            while (tileSpawner.SpaceAvailableAtPipe(i))
            {
                tileSpawner.SpawnAtPipe(i);
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Cell touchedCell = grid.GetCellAtPosition(Input.mousePosition);
            if (touchedCell != null)
            {
                Tile touchedTile = touchedCell.GetAttachedTile();
                if (inputSelectedTile != null)
                {
                    if (touchedCell.GetAttachedTile() != inputSelectedTile)
                    {
                        if (AreCrossNeighbors(inputSelectedTile, touchedTile))
                        {
                            // Do Move
                        }
                        else
                        {
                            // Select new first
                            inputSelectedTile = touchedTile;
                        }
                    }
                }
                else
                {
                    // Select new first
                    inputSelectedTile = touchedTile;
                }
            }
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 v = Input.mousePosition - inputSelectedTile.GetCell().GetPosition();
            float sqrDistance = Vector3.SqrMagnitude(v);
            const float SQRDISTANCE_TO_CONSIDER_MOVE = 1.0f;
            if (sqrDistance > SQRDISTANCE_TO_CONSIDER_MOVE)
            {
                // Move (left, up, down or right?)
            }
        }

    }

    private bool AreCrossNeighbors(Tile tileA, Tile tileB)
    {
        return (tileA.GetCell().xIndex == tileB.GetCell().xIndex && Mathf.Abs(tileA.GetCell().yIndex - tileB.GetCell().yIndex) == 1)
            || (tileA.GetCell().yIndex == tileB.GetCell().yIndex && Mathf.Abs(tileA.GetCell().xIndex - tileB.GetCell().xIndex) == 1);
    }

    private Tile inputSelectedTile;

#if UNITY_EDITOR
    [ContextMenu("OD")]
    private void OD()
    {
        // Always re assign the grid, to update the settings
        // Editor purposes
        tileSpawner.Assign(grid);

        int c = tileSpawner.PipesCount();
        for (int i = 0; i < c; i++)
        {
            while (tileSpawner.SpaceAvailableAtPipe(i))
            {
                tileSpawner.SpawnAtPipe(i);
            }
        }
        //tileSpawner.EditorUpdateSpawn();
    }
#endif

}