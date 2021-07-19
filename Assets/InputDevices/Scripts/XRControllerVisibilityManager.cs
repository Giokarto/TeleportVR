using UnityEngine;

public class XRControllerVisibilityManager : MonoBehaviour
{

    public GameObject leftController, rightController;
    private bool controllersHidden = false;

    // Update is called once per frame
    void Update()
    {
#if SENSEGLOVE
        if (!controllersHidden)
        {
            // try each frame until controllers are found, since they are loaded dynamically
            try
            {
                var leftRenderer = leftController.GetComponentInChildren<MeshRenderer>();
                leftRenderer.enabled = false;
                var leftLineRenderer = leftController.GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.XRRayInteractor>();
                leftLineRenderer.enabled = false;
                var rightRenderer = rightController.GetComponentInChildren<MeshRenderer>();
                rightRenderer.enabled = false;
                var rightLineRenderer = rightController.GetComponentInChildren<UnityEngine.XR.Interaction.Toolkit.XRRayInteractor>();
                rightLineRenderer.enabled = false;

                controllersHidden = true;
            }
            catch (System.NullReferenceException)
            {
            }
        }
#endif
    }
}
