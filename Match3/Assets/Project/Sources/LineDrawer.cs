using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    private static LineDrawer instance;
    private const int QUANTITY_IN_POOL = 20;

    [SerializeField]
    private GameObject linePrefab;

    private SpriteRenderer[] lines;
    private int cursor;

    public static LineDrawer Instance { get { return instance; } }

    private void Awake()
    {
        instance = this;

        lines = new SpriteRenderer[QUANTITY_IN_POOL];
        for (int i = 0; i < QUANTITY_IN_POOL; i++)
        {
            GameObject lineObject = Instantiate(linePrefab);
            lineObject.transform.parent = transform;
            lineObject.transform.localPosition = Vector3.zero;
            lineObject.SetActive(false);
            lines[i] = lineObject.GetComponent<SpriteRenderer>();
        }
    }

    public void DrawLine(Vector2 from, Vector2 to, Color color, float thickness = 1.0f)
    {
        Vector2 fromTo = to - from;
        lines[cursor].gameObject.isStatic = false;
        lines[cursor].color = color;
        lines[cursor].gameObject.SetActive(true);
        lines[cursor].transform.right = fromTo.normalized;
        // Getting the middle point between from and to 
        lines[cursor].transform.localPosition = from + (fromTo * 0.5f);
        Vector3 newScale = new Vector3(fromTo.magnitude, thickness, 1.0f);
        lines[cursor].transform.localScale = newScale;
        lines[cursor].gameObject.isStatic = true;

        cursor++;
        if (cursor == QUANTITY_IN_POOL)
        {
            cursor = 0;
        }
    }
}