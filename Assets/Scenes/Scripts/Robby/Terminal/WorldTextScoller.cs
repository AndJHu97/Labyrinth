using TMPro;
using UnityEngine;

public class WorldTextScroller : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;
    [Tooltip("The visible area, usually the terminal's background sprite.")]
    [SerializeField] private SpriteRenderer viewport;
    [SerializeField] private float scrollSpeed = 2f;
    [SerializeField] private float activationDistance = 2f;
    [Tooltip("Off: wheel down shows later text (normal). On: reversed.")]
    [SerializeField] private bool invertScroll = false;

    private float startingY;
    private Camera cam;

    private void Awake()
    {
        startingY = text.transform.localPosition.y;
        cam = Camera.main;
    }

    private void Update()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) < 0.01f) return;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Vector3 mouseLocation = cam.ScreenToWorldPoint(Input.mousePosition);
        float distance = Vector2.Distance(transform.position, mouseLocation);
        if (distance >= activationDistance) return;

        float delta = (invertScroll ? scroll : -scroll) * scrollSpeed;
        SetOffset(CurrentOffset() + delta);
    }

    // Jump to the newest text (called by the terminal after every change)
    public void ScrollToBottom()
    {
        float max = MaxOffset();
        if (float.IsInfinity(max)) return;   // no viewport assigned
        ApplyOffset(max);
    }

    // How far the text has been moved up from its starting position
    private float CurrentOffset()
    {
        return text.transform.localPosition.y - startingY;
    }

    private void SetOffset(float offset)
    {
        // Can't scroll above the start or past the end of the text
        ApplyOffset(Mathf.Clamp(offset, 0f, MaxOffset()));
    }

    private void ApplyOffset(float offset)
    {
        Vector3 position = text.transform.localPosition;
        position.y = startingY + offset;
        text.transform.localPosition = position;
    }

    // How much taller the text is than the visible area, in the text's parent space.
    // Assumes the text is anchored at the top and grows downward.
    private float MaxOffset()
    {
        if (viewport == null) return float.PositiveInfinity;

        text.ForceMeshUpdate();   // make sure preferredHeight matches the current text

        float parentScale = text.transform.parent != null ? text.transform.parent.lossyScale.y : 1f;
        float textHeightWorld = text.preferredHeight * text.transform.lossyScale.y;
        float viewHeightWorld = viewport.bounds.size.y;

        float overflowWorld = Mathf.Max(0f, textHeightWorld - viewHeightWorld);
        return overflowWorld / Mathf.Max(parentScale, 0.0001f);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Camera c = Camera.main;
        if (c == null) return;

        Vector3 mouseLocation = c.ScreenToWorldPoint(Input.mousePosition);
        float distance = Vector2.Distance(transform.position, mouseLocation);

        Gizmos.color = distance < activationDistance ? Color.blue : Color.red;
        Gizmos.DrawLine(mouseLocation, transform.position);
    }
}