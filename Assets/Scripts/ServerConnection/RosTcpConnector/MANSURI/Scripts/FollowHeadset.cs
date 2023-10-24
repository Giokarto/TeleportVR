using UnityEngine;

public class FollowHeadset : MonoBehaviour
{
    public Transform headTransform;
    public float speed = 0.3f;
    private RectTransform rectTransform;
    public float movementThreshold = 2f;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        if (headTransform == null)
        {
            headTransform = GameObject.Find("Head")?.transform;

            if (headTransform == null)
            {
                Debug.LogWarning("Head object not found in the scene!");
                return;
            }
        }

        // Ensuring the z-axis rotation starts from zero
        Vector3 eulerRotation = rectTransform.localEulerAngles;
        eulerRotation.z = 0;
        rectTransform.localRotation = Quaternion.Euler(eulerRotation);
    }

    void FixedUpdate()
    {
        if (headTransform == null || rectTransform == null)
        {
            return;
        }

        // Creating the target rotation based on the headTransform's y-axis rotation
        Quaternion targetRotation = Quaternion.Euler(0, 0, -headTransform.eulerAngles.y);

        // Check the difference in rotations
        if (Quaternion.Angle(rectTransform.localRotation, targetRotation) < movementThreshold)
        {
            return;
        }
        Quaternion newRotation = Quaternion.Slerp(rectTransform.localRotation, targetRotation, speed * Time.fixedDeltaTime);
        rectTransform.localRotation = newRotation;
    }
}