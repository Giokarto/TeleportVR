using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Widgets
{
    public class Completion : MonoBehaviour
    {
        public string text = "Calibrating";
        [Range(0f, 1f)]
        public float progress = 0;
        public bool active = false;
        public Image image;
        public GameObject child;

        private TextMeshProUGUI tmp;

        // Start is called before the first frame update
        void Start()
        {
            tmp = GetComponentInChildren<TextMeshProUGUI>();
        }

        // Update is called once per frame
        void Update()
        {
            progress = Mathf.Clamp01(progress);
            active &= progress > 0;
            child.SetActive(active);
            image.fillAmount = progress;
            tmp.text = text;
        }

        public void Set(float progress, string text = null)
        {
            active = progress > 0;
            this.progress = Mathf.Clamp01(progress);
            if (text != null)
            {
                tmp.text = text;
            }
        }
    }

}
