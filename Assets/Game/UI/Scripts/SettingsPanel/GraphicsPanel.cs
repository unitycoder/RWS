﻿using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RWS
{
    public class GraphicsPanel : MonoBehaviour
    {
        [SerializeField]
        GraphicsManager graphicsManager = null;

        [SerializeField]
        InputManager inputManager = null;
        
        [SerializeField]
        TMP_Dropdown resolutionDropdown = null;

        [SerializeField]
        TMP_Dropdown qualityDropdown = null;

        [SerializeField]
        TMP_Dropdown grassDropdown = null;

        [SerializeField]
        Toggle postProcessToggle = null;

        [SerializeField]
        TMP_InputField displayInputField = null;

        [SerializeField]
        Toggle vSyncToggle = null;

        [SerializeField]
        TMP_InputField fpsLimitInputField = null;

        [SerializeField]
        Button backButton = null;

        [SerializeField]
        Button applyButton = null;

        //----------------------------------------------------------------------------------------------------

        public Action OnBackButtonClicked;
        
        public void Show()
        {
            gameObject.SetActive( true );

            resolutionDropdown.value = graphicsManager.GetResolutions().IndexOf( graphicsManager.GetResolution() );
            qualityDropdown.value = graphicsManager.QualityLevel;
            grassDropdown.value = graphicsManager.GrassQualityLevel;
            postProcessToggle.isOn = graphicsManager.PostProcess;

            if( graphicsManager.DisplayCount > 1 )
            {
                SetInteractable( displayInputField, true );
                displayInputField.text = ( graphicsManager.TargetDisplay + 1 ).ToString();
            }
            else
            {
                SetInteractable( displayInputField, false );
                displayInputField.text = "1";
            }

            vSyncToggle.isOn = graphicsManager.VSync;

            fpsLimitInputField.text = graphicsManager.FpsLimit.ToString();
            SetInteractable( fpsLimitInputField, !vSyncToggle.isOn );

            applyButton.gameObject.SetActive( false );
        }

        public void Hide()
        {
            gameObject.SetActive( false );
        }

        //----------------------------------------------------------------------------------------------------
        
        int targetDisplay;
        int fpsLimit;


        void OnValidate()
        {
            if( !graphicsManager )
            {
                graphicsManager = GraphicsManager.Instance;
            }
            if( !inputManager )
            {
                inputManager = InputManager.Instance;
            }
        }

        void Awake()
        {
            void updateResolutionDropdown()
            {
                var allResolutions = graphicsManager.GetResolutions();
                var currentResolution = graphicsManager.GetResolution();

                resolutionDropdown.ClearOptions();
                resolutionDropdown.AddOptions( new List<string>( allResolutions.Select( resolution => $"{resolution.width} x {resolution.height}" ) ) );
                resolutionDropdown.value = allResolutions.IndexOf( currentResolution );
                resolutionDropdown.onValueChanged.AddListener( OnResolutionDropdownChanged );
            }
            
            updateResolutionDropdown();
            graphicsManager.OnTargetDisplayChenged += newDisplay => { updateResolutionDropdown(); };

            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions( new List<string>( graphicsManager.QualityNames ) );
            qualityDropdown.value = graphicsManager.QualityLevel;
            qualityDropdown.onValueChanged.AddListener( OnQualityDropdownChanged );

            grassDropdown.value = graphicsManager.GrassQualityLevel;
            grassDropdown.onValueChanged.AddListener( OnGrassDropdownChanged );

            postProcessToggle.isOn = graphicsManager.PostProcess;
            postProcessToggle.onValueChanged.AddListener( OnPostProcessToggleChanged );

            if( graphicsManager.DisplayCount > 1 )
            {
                SetInteractable( displayInputField, true );
                displayInputField.text = ( graphicsManager.TargetDisplay + 1 ).ToString();
            }
            else
            {
                SetInteractable( displayInputField, false );
                displayInputField.text = "1";
            }

            displayInputField.onEndEdit.AddListener( OnDisplayInputFieldChanged );

            vSyncToggle.isOn = graphicsManager.VSync;
            vSyncToggle.onValueChanged.AddListener( OnVSyncToggleChanged );

            fpsLimit = graphicsManager.FpsLimit;
            fpsLimitInputField.text = fpsLimit.ToString();
            SetInteractable( fpsLimitInputField, !vSyncToggle.isOn );
            fpsLimitInputField.onEndEdit.AddListener( OnFpsLimitInputFieldChanged );

            backButton.onClick.AddListener( OnBackButton );

            applyButton.onClick.AddListener( OnApplyButton );
            applyButton.gameObject.SetActive( false );
        }

        void OnEnable()
        {
            inputManager.OnEscapeButton += OnEscapeButton;
        }

        void OnDisable()
        {
            inputManager.OnEscapeButton -= OnEscapeButton;
        }
        

        void OnResolutionDropdownChanged( int value )
        {
            applyButton.gameObject.SetActive( true );
        }

        void OnQualityDropdownChanged( int value )
        {
            applyButton.gameObject.SetActive( true );
        }

        void OnGrassDropdownChanged( int value )
        {
            applyButton.gameObject.SetActive( true );
        }

        void OnPostProcessToggleChanged( bool value )
        {
            applyButton.gameObject.SetActive( true );
        }

        void OnDisplayInputFieldChanged( string value )
        {
            var newTargetDisplay = int.Parse( value ) - 1;
            newTargetDisplay = Mathf.Clamp( newTargetDisplay, 0, graphicsManager.DisplayCount - 1 );

            if( newTargetDisplay == targetDisplay )
            {
                return;
            }

            targetDisplay = newTargetDisplay;

            displayInputField.text = ( targetDisplay + 1 ).ToString();
            applyButton.gameObject.SetActive( true );
        }

        void OnVSyncToggleChanged( bool value )
        {
            SetInteractable( fpsLimitInputField, !value );
            applyButton.gameObject.SetActive( true );
        }

        void OnFpsLimitInputFieldChanged( string value )
        {
            var newFpsLimit = int.Parse( value );
            newFpsLimit = Mathf.Clamp( newFpsLimit, 10, 1000 );

            if( newFpsLimit == fpsLimit )
            {
                return;
            }

            fpsLimit = newFpsLimit;

            fpsLimitInputField.text = fpsLimit.ToString();
            applyButton.gameObject.SetActive( true );
        }


        void OnBackButton()
        {
            OnBackButtonClicked?.Invoke();
        }

        void OnApplyButton()
        {
            graphicsManager.SetResolution( graphicsManager.GetResolutions()[ resolutionDropdown.value ] );
            graphicsManager.QualityLevel = qualityDropdown.value;
            graphicsManager.GrassQualityLevel = grassDropdown.value;
            graphicsManager.PostProcess = postProcessToggle.isOn;
            graphicsManager.TargetDisplay = targetDisplay;
            graphicsManager.VSync = vSyncToggle.isOn;
            graphicsManager.FpsLimit = fpsLimit;
            graphicsManager.SavePlayerPrefs();

            applyButton.gameObject.SetActive( false );
        }

        void OnEscapeButton()
        {
            if( resolutionDropdown.IsExpanded || qualityDropdown.IsExpanded || grassDropdown.IsExpanded || displayInputField.isFocused || fpsLimitInputField.isFocused )
            {
                return;
            }

            OnBackButton();
        }


        static void SetInteractable( Component target, bool interactable )
        {
            var canvasGroup = target.GetComponent<CanvasGroup>();
            canvasGroup.interactable = interactable;
            canvasGroup.enabled = !interactable;
        }
    }
}