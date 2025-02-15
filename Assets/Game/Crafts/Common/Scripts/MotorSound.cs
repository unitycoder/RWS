﻿using UnityEngine;

namespace RWS
{
    public class MotorSound : MonoBehaviour
    {
        [SerializeField]
        Motor motor = null;

        // Low rpm sound
        [SerializeField]
        AudioSource lowSoundSource = null;

        // High rpm sound
        [SerializeField]
        AudioSource highSoundSource = null;

        // Low sound pitch vs Transition
        [SerializeField]
        AnimationCurve lowSoundPitchCurve = AnimationCurve.Linear( 0f, 0.8f, 1f, 1.4f );

        // Low sound volume vs Transition
        [SerializeField]
        AnimationCurve lowSoundVolumeCurve = AnimationCurve.Linear( 0f, 0.3f, 1f, 0.15f );

        // High sound pitch vs Transition
        [SerializeField]
        AnimationCurve highSoundPitchCurve = AnimationCurve.Linear( 0f, 0.8f, 1f, 1.3f );

        // High sound volume vs Transition
        [SerializeField]
        AnimationCurve highSoundVolumeCurve = AnimationCurve.Linear( 0f, 0f, 1f, 1f );

        [SerializeField, Range( 0f, 1f )]
        float soundTransition = 0f;

        [SerializeField]
        float motorRpmMax = 30000f;

        [SerializeField]
        float deadzoneMin = 0.01f;

        //--------------------------------------------------------------------------------------------------------------

        public float SoundTransition
        {
            get => soundTransition;
            set
            {
                soundTransition = Mathf.Clamp01( value );
                UpdateAudioSources();
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        void OnValidate()
        {
            UpdateAudioSources();
        }

        void Update()
        {
            if( motor ) // Do not remove! Remote wing has no motor
            {
                SoundTransition = motor.rpm / motorRpmMax;
            }
        }

        //--------------------------------------------------------------------------------------------------------------

        void UpdateAudioSources()
        {
            if( soundTransition > deadzoneMin )
            {
                lowSoundSource.volume = lowSoundVolumeCurve.Evaluate( soundTransition );
                highSoundSource.volume = highSoundVolumeCurve.Evaluate( soundTransition );

                lowSoundSource.pitch = lowSoundPitchCurve.Evaluate( soundTransition );
                highSoundSource.pitch = highSoundPitchCurve.Evaluate( soundTransition );
            }
            else
            {
                if( lowSoundSource.volume > 0f )
                {
                    lowSoundSource.volume = 0f;
                }

                if( highSoundSource.volume > 0f )
                {
                    highSoundSource.volume = 0f;
                }
            }
        }
    }
}