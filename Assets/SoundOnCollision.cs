using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SoundOnCollision : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip defaultSound, collisionSound;
    public string requiredCollisionTag;
    //public TelemedicineTraining.UserActionType actionType;
    private Renderer renderer;
    private Color originalColor;
    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<MeshRenderer>();
        originalColor = renderer.material.GetColor("_Color");
        //col.a += 0.5f;
        //renderer.material.SetColor("_Color", col);
        //renderer.material.color = new Color(47.0f, 175.0f, 186.0f,1.0f);//, 0.1f);

    }

    // Update is called once per frame
    void Update()
    {
        //if (Keyboard.current.spaceKey.isPressed)
        //{
        //    //renderer.material.SetColor("_Color", new Color(170.0f, 246.0f, 131.0f, 0.01f));
        //    var col = originalColor;// renderer.material.GetColor("_Color");
        //    col.a += 4.0f;
        //    renderer.material.SetColor("_Color", col);

        //}
        //if (Keyboard.current.aKey.isPressed)
        //{
        //    //renderer.material.SetColor("_Color", new Color(47.0f, 175.0f, 186.0f, 0.01f));
        //    //var col = renderer.material.GetColor("_Color");
        //    //col.a -= 4.0f;
        //    renderer.material.SetColor("_Color", originalColor);

        //}
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(requiredCollisionTag))
        {
            //Debug.Log("colliding with " + other.name);
            audioSource.clip = collisionSound;
            audioSource.loop = true;
            audioSource.Play();
            var col = originalColor;// renderer.material.GetColor("_Color");
            col.a += 4.0f;
            renderer.material.SetColor("_Color", col);
            //TelemedicineTraining.MarkUserActionComplete(actionType);
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(requiredCollisionTag))
        {
            //Debug.Log("exiting " + other.name);
            audioSource.clip = defaultSound;
            audioSource.Play();
            var col = renderer.material.GetColor("_Color");
            col.a -= 0.5f;
            renderer.material.SetColor("_Color", originalColor);
        }
    }
}
