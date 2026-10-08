using UnityEngine;
using TMPro;

public class WorldTextScroller : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;
    [SerializeField] private float scrollSpeed = 2f;
    [SerializeField] private float activationDistance = 2f;

    private float startingY;

    private void Start()
    {
        startingY = text.transform.localPosition.y;
    }

    private void Update()
    {
        float scroll = Input.mouseScrollDelta.y;

        var mouseLocation = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var distance = Vector2.Distance(transform.position, mouseLocation);

        if (Mathf.Abs(scroll) > 0.01f && distance < activationDistance)
        {
            Vector3 position = text.transform.localPosition;

            position.y += scroll * scrollSpeed;

            // Don't allow the text to scroll above its starting position.
            position.y = Mathf.Max(position.y, startingY);

            text.transform.localPosition = position;
        }
    }

    private void OnDrawGizmos()
    {
        var mouseLocation = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var distance = Vector2.Distance(transform.position, mouseLocation);

        if (distance < activationDistance)
        //if (text.bounds.Contains(mouseLocation))
            Gizmos.color = Color.blue;
        else
            Gizmos.color = Color.red;

        Gizmos.DrawLine(Camera.main.ScreenToWorldPoint(Input.mousePosition), transform.position);
    }
}