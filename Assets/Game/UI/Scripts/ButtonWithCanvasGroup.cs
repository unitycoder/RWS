﻿using UnityEngine;
using UnityEngine.UI;

namespace RWS
{
    public class ButtonWithCanvasGroup : Button
    {
        [SerializeField]
        CanvasGroup canvasGroup = null;

        public bool Interactable
        {
            get => !canvasGroup.enabled;
            set
            {
                canvasGroup.interactable = value;
                canvasGroup.enabled = !value;
            }
        }
    }
}
