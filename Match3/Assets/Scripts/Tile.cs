using System.Collections;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    private Cell cell;
    private Color color;

    private const float GRAVITY_FORCE = 6.0f;

    private bool falling;

    private void Start()
    {
        falling = true;
    }

    private void Update()
    {
        if (falling)
        {
            // Fake Gravity Effect
            transform.position += Vector3.down * GRAVITY_FORCE * Time.deltaTime;
            if (transform.position.y < cell.GetPosition().y)
            {
                transform.position = cell.GetPosition();
                falling = false;
            }
        }
    }

    public void SetColor(Color color)
    {
        this.color = color;
        spriteRenderer.color = this.color;
    }

    public void AppearWhenReachsY(float y)
    {
        StartCoroutine(CoroutineAppearWhenY(y));
    }

    private IEnumerator CoroutineAppearWhenY(float y)
    {
        yield return new WaitUntil(() => transform.position.y < y);
        float animationTime = 1.0f;
        float timer = 0.0f;
        while (true)
        {
            timer += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }
    }

    public void SetCell(Cell cell)
    {
        this.cell = cell;
    }

    public Cell GetCell()
    {
        return cell;
    }

}