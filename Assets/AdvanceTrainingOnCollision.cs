using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvanceTrainingOnCollision : MonoBehaviour
{
    public string requiredCollisionTag;
    public TelemedicineTraining.UserActionType actionType;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(requiredCollisionTag))
        {
            TelemedicineTraining.MarkUserActionComplete(actionType);
        }

    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(requiredCollisionTag))
        {
            TelemedicineTraining.MarkUserActionComplete(actionType);
        }
    }
}
