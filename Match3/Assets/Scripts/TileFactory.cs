using UnityEngine;

public class TileFactory : MonoBehaviour
{
    [SerializeField]
    private GameObject coloredTilePrefab;

    public Color RandomColor()
    {
        int r = Random.Range(0, 3);
        Color color = default(Color);
        switch (r)
        {
            case 0:
                color = Color.red;
                break;
            case 1:
                color = Color.green;
                break;
            case 2:
                color = Color.blue;
                break;
            case 3:
                color = Color.yellow;
                break;
        }
        return color;
    }

    public Tile MakeColoredTile(Color color)
    {
        GameObject newColoredTile = Instantiate(coloredTilePrefab);
        newColoredTile.gameObject.SetActive(true);
        Tile tile = newColoredTile.GetComponent<Tile>();
        tile.SetColor(color);
        return tile;
    }
}
