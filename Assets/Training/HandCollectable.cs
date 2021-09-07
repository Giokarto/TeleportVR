using System.Linq;
using UnityEngine;

namespace Training
{
    public class HandCollectable : MonoBehaviour
    {
        [SerializeField] private string[] requiredTags;
        [SerializeField] private GameObject objectToRecolor;
        [SerializeField] private TutorialSteps.TrainingStep[] requiredSteps;
        [SerializeField] private Color newColor;
        [SerializeField] private Gradient _gradient;
        [SerializeField] private AudioClip sound;
        private MeshRenderer _renderer;

        private static int collectedSpheres;
   
        private float timer;
        [SerializeField] private float dwellTime;

        private void Start()
        {
            _renderer = objectToRecolor.GetComponent<MeshRenderer>();
        }

        /// <summary>
        /// Collect the sphere if the correct tag/part of the user collides with it
        /// </summary>
        /// <param name="other">The other colliding collider</param>
        private void OnTriggerEnter(Collider other)
        {
            if (requiredSteps.Contains(TutorialSteps.Instance.currentState) && AnyTagMatches(other))
            {
                collectedSpheres++;
                if (sound != null)
                    TutorialSteps.Instance.audioManager.ScheduleAudioClip(sound);
                gameObject.SetActive(false);
                Debug.Log($"Object collected {other}. Moving on.");
                TutorialSteps.Instance.Next();
            }
        }

        private bool AnyTagMatches(Collider other )
        {
            foreach(var tag in requiredTags)
            {
                if (other.CompareTag(tag))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
