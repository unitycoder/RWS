﻿using System;
using TMPro;
using UnityEngine;

namespace RWS
{
    public class LapTime : MonoBehaviour
    {
        [SerializeField]
        TextMeshProUGUI timeText = null;

        [SerializeField]
        TextMeshProUGUI bestTimeText = null;

        [SerializeField]
        GameObject bestTimeGameObject = null;

        [SerializeField]
        float maxTime = 120f;

        [SerializeField]
        float updateRate = 30f;

        //----------------------------------------------------------------------------------------------------

        public void Init( float bestTime )
        {
            this.bestTime = bestTime;

            if( bestTime < 0f )
            {
                bestTimeText.text = $"N/A";
            }
            else
            {
                bestTimeText.text = TimeSpan.FromSeconds( bestTime ).ToString( timeFormat );
            }
        }

        public Action<float> OnNewBestTime;
        public Action OnTimeOut;

        public bool Started => lapStarted;

        public void StartNewTime()
        {
            lapTime = 0f;
            lapStarted = true;

            if( !timeVisible )
            {
                ShowUI();
            }
        }

        public void CompareTime()
        {
            if( bestTime < 0f || lapTime < bestTime )
            {
                bestTime = lapTime;
                bestTimeText.text = TimeSpan.FromSeconds( bestTime ).ToString( timeFormat );

                OnNewBestTime?.Invoke( bestTime );
            }
        }

        public void Show()
        {
            ShowUI();
        }

        public void Hide()
        {
            HideUI();
        }

        public void Reset()
        {
            lapTime = 0f;
            lapStarted = false;
        }

        //----------------------------------------------------------------------------------------------------

        readonly string timeFormat = @"mm\:ss\.ff";

        bool lapStarted;
        float lapTime;
        float bestTime;
        float lastUpdateTime;
        bool timeVisible;


        void Update()
        {
            if( !lapStarted )
            {
                return;
            }

            lapTime += Time.deltaTime;

            if( Time.time - lastUpdateTime > 1f / updateRate )
            {
                lastUpdateTime = Time.time;
                timeText.text = TimeSpan.FromSeconds( lapTime ).ToString( timeFormat );
            }

            if( lapTime > maxTime )
            {
                lapTime = 0f;
                lapStarted = false;
                HideUI();

                OnTimeOut?.Invoke();
            }
        }


        void ShowUI()
        {
            timeVisible = true;
            timeText.enabled = true;
            bestTimeGameObject.SetActive( true );
        }

        void HideUI()
        {
            timeVisible = false;
            timeText.enabled = false;
            bestTimeGameObject.SetActive( false );
        }
    }
}