using System.Collections;
using System.Collections.Generic;
using InputDevices.VRControllers;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// A pointer used with the SenseGlove, where a ray is coming from the gloves position and orientation.
/// </summary>
public class PointerController : Pointer
{
    private XRBaseController teleport;
    private InputDevice right;

    /// <summary>
    /// Send the ray the sense glove is pointing at.
    /// </summary>
    public override void GetPointerPosition()
    {
        //PushPointerPosition(teleport.pointerOriginZ.position, teleport.pointerOriginZ.rotation.eulerAngles);
        PushPointerPosition(teleport.transform.position, teleport.transform.eulerAngles);
    }

    /// <summary>
    /// At the start, this method sets the reference to the first found SenseGlove_Teleport script.
    /// </summary>
    public override void SubclassStart()
    {
        Object[] objects = Resources.FindObjectsOfTypeAll(typeof(XRBaseController));
        if (objects.Length > 0)
        {
            teleport = (XRBaseController) objects[0];
        }

        right = VRControllerInputSystem.controllerRight;
        
        VRControllerInputSystem.OnTriggerChange += (l, r) =>
        {
            //CurvedUIInputModule.CustomControllerButtonState = r > 0.5;
        }; // TODO: remove in ondestroy
    }

    /// <summary>
    /// Update the ray the sense glove is pointing at.
    /// </summary>
    void Update()
    {
        GetPointerPosition();
        var pressed = (right.TryGetFeatureValue(CommonUsages.primaryButton, out var btn) && btn) ;
        CurvedUIInputModule.CustomControllerButtonState = pressed;
    }
}
