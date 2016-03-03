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

    public void DrawLine(LineDrawInfo lineDrawInfo, Color color, float thickness = 1.0f)
    {
        Vector2 fromTo = lineDrawInfo.To - lineDrawInfo.From;
        lines[cursor].gameObject.isStatic = false;
        lines[cursor].color = color;
        lines[cursor].gameObject.SetActive(true);
        lines[cursor].transform.right = fromTo.normalized;
        // Getting the middle point between from and to 
        lines[cursor].transform.localPosition = lineDrawInfo.From + (fromTo * 0.5f);
        Vector3 newScale = new Vector3(fromTo.magnitude, thickness, 1.0f);
        lines[cursor].transform.localScale = newScale;
        lines[cursor].gameObject.isStatic = true;

        cursor++;
        if (cursor == QUANTITY_IN_POOL)
        {
            cursor = 0;
        }
    }

    public class LineDrawInfo
    {
        private Vector2 from;
        private Vector2 to;

        public Vector2 From { get { return from; } set { from = value; } }
        public Vector2 To { get { return to; } set { to = value; } }
    }
}