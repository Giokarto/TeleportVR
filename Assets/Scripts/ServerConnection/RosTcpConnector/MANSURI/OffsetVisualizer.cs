using UnityEngine;

public class OffsetVisualizer : MonoBehaviour
{
    public Transform offset;
    public RectTransform minimapRawImageRectTransform;

    void Update()
    {
        // Match the position and rotation of the offset transform
        transform.position = offset.position;
        transform.rotation = offset.rotation;

        // Adjust the position on the UI RawImage based on the minimapRawImageRectTransform
        Vector2 rawImageSize = minimapRawImageRectTransform.rect.size;
        Vector2 rawImagePivot = minimapRawImageRectTransform.pivot;
        Vector2 offsetNormalizedPosition = new Vector2((offset.position.x + 1f) / 2f, (offset.position.z + 1f) / 2f);
        Vector2 offsetPixelPosition = new Vector2(offsetNormalizedPosition.x * rawImageSize.x, offsetNormalizedPosition.y * rawImageSize.y);
        Vector2 offsetUIPosition = new Vector2(offsetPixelPosition.x - rawImageSize.x * rawImagePivot.x, offsetPixelPosition.y - rawImageSize.y * rawImagePivot.y);
        transform.localPosition = new Vector3(offsetUIPosition.x, offsetUIPosition.y, 0f);
    }
}