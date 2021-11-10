using BioIK;
using UnityEngine;
using UnityEngine.UI;

public class CollisionLogger : MonoBehaviour
{
    public Text LogCollsiionEnter;
    public Text LogCollisionStay;
    public Text LogCollisionExit;
    private BioSegment hand_segment;
    public EnableControlManager controlManager;

    private void Start()
    {
        //hand_segment = GetComponent<BioSegment>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        LogCollsiionEnter.text = "On Collision Enter: " + collision.collider.name;
    }

    private void OnCollisionStay(Collision collision)
    {
        LogCollisionStay.text = "On Collision stay: " + collision.collider.name;
    }

    private void OnCollisionExit(Collision collision)
    {
        LogCollisionExit.text = "On Collision exit: " + collision.collider.name;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        //controlManager.isColliding(0, true);
        //controlManager.isColliding(1, true);
        //for (int i = 0; i < hand_segment.Objectives.Length; i++)
        //{
        //    hand_segment.Objectives[i].enabled = false;
        //}

        if (other.name == "MyCube")
        {
            this.GetComponent<Rigidbody>().isKinematic = false;
            Debug.Log(name + "is not kinematic");
        }
            
        //GetComponent<Rigidbody>().velocity = Vector3.zero;
    }
    private void OnTriggerStay(Collider other)
    {
        //GetComponent<Rigidbody>().velocity = Vector3.zero;
    }
    private void OnTriggerExit(Collider other)
    {
        Debug.Log("exisiting " + other.name);
        //controlManager.isColliding(0, false);
        //controlManager.isColliding(1, false);
        //for (int i = 0; i < hand_segment.Objectives.Length; i++)
        //{
        //    hand_segment.Objectives[i].enabled = true;
        //}

        if (other.name == "MyCube")
        {
            GetComponent<Rigidbody>().isKinematic = true;
            Debug.Log(name + " is kinematic");
        }
    }
    
}