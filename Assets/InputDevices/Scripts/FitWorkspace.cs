using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FitWorkspace : MonoBehaviour
{
    public Transform leftHand, leftHandObjective, rightHand, rightHandObjective, leftArm, rightArm;

    void Update()
    {
        if (Input.GetKey(KeyCode.F))
        {
            FitLeft();
            FitRight();
        }
    }

    private void Fit(Transform hand, Transform objective, Transform target)
    {
        float factor = objective.position.x / hand.position.x;
        Debug.Log($"Scaled {target} by {factor}");
        target.localScale *= factor;
    }

    public void FitLeft()
    {
        Fit(leftHand, leftHandObjective, leftArm);
    }

    public void FitRight()
    {
        Fit(rightHand, rightHandObjective, rightArm);
    }
}
