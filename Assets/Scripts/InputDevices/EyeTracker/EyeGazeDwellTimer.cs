﻿#if TOBII
using Tobii.G2OM;
#endif
using System;
using OperatorUserInterface;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InputDevices.EyeGaze
{
    [Obsolete(
        "Not used anymore, was used to activate widgets with eye gaze. " +
        "Code kept here for possible reimplementation.")]
#if TOBII
public class EyeGazeDwellTimer : MonoBehaviour, IGazeFocusable, IPointerEnterHandler, IPointerExitHandler
#else
    public class EyeGazeDwellTimer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
#endif
    {
        public float duration = 2.0f;
        Timer timer;
        public bool hasFocus = false;

        Image dwellTimeImage;

        private void Awake()
        {
            timer = new Timer();
            dwellTimeImage = GetComponentInParent<Image>();
        }

        public void GazeFocusChanged(bool hasFocus)
        {
            if (hasFocus)
            {
                OnPointerEnter(null);
            }
            else
            {
                OnPointerExit(null);
            }
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (hasFocus)
            {
                timer.LetTimePass(Time.deltaTime);
                dwellTimeImage.fillAmount = timer.GetFraction();
            }
        }

        public void GoToConstruct()
        {
            SceneManager.Instance.SwitchScene();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hasFocus = true;
            timer.SetTimer(duration, GoToConstruct);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hasFocus = false;

            timer.ResetTimer();
            dwellTimeImage.fillAmount = timer.GetFraction();
        }
    }
}