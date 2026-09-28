using UnityEngine;

public class ScrollCode : MonoBehaviour
{
    public float speed = 50f;
    private RectTransform rect;

    void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        rect.anchoredPosition += Vector2.down * speed * Time.deltaTime;

        // Reset when it goes too far down
        if (rect.anchoredPosition.y < -Screen.height)
        {
            rect.anchoredPosition = new Vector2(0, Screen.height);
        }
    }
}