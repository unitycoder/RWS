﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace RWS
{
    public class AxisControl
    {
        public AxisControl()
        {
            axisDirection = 1;
            inputAction = new InputAction();
            inputAction.performed += context =>
            {
                Performed?.Invoke( context.ReadValue<float>() * axisDirection );
            };
        }

        //----------------------------------------------------------------------------------------------------
        
        public event Action<float> Performed;

        public bool Invert
        {
            get => axisDirection < 0;
            set
            {
                axisDirection = value ? -1 : 1;
                Performed?.Invoke( inputAction.ReadValue<float>() * axisDirection );
            }
        }

        public string BindingPath
        {
            get; private set;
        }

        public string BindingName
        {
            get; set;
        }

        public void SetBinding( InputControl control )
        {
            BindingPath = control.path;
            BindingName = $"{control.device.displayName}: {control.displayName}";
            
            SetBinding( BindingPath );
        }
        
        
        public void Save( string dataKey )
        {
            //Debug.Log( dataKey );
            if( string.IsNullOrEmpty( BindingPath ) )
            {
                return;
            }
            var dataString = $"{BindingPath}@{BindingName}@{axisDirection}";
            PlayerPrefs.SetString( dataKey, dataString );
        }

        public void Load( string dataKey )
        {            
            if( PlayerPrefs.HasKey( dataKey ) )
            {
                var dataString = PlayerPrefs.GetString( dataKey );
                var dataSplitted = dataString.Split( '@' );

                BindingPath = dataSplitted[ 0 ].Trim();
                BindingName = dataSplitted[ 1 ].Trim();
                axisDirection = int.Parse( dataSplitted[ 2 ].Trim() );
                
                SetBinding( BindingPath );
            }
            else
            {
                BindingPath = null;
                BindingName = null;
                axisDirection = 1;
            }
        }

        public void Disable()
        {
            inputAction.Disable();
        }
        
        //----------------------------------------------------------------------------------------------------
        
        InputAction inputAction;
        int axisDirection;
        
        void SetBinding( string path )
        {
            inputAction.Disable();
            if( inputAction.bindings.Count > 0 )
            {
                inputAction.ChangeBinding( 0 ).Erase();
            }
            inputAction.AddBinding( path );
            inputAction.Enable();
        }
    }

    public class ButtonControl
    {
        double startTime;
        
        public ButtonControl()
        {
            inputAction = new InputAction();
            inputAction.performed += context =>
            {
                Performed?.Invoke();
            };
        }
        
        //----------------------------------------------------------------------------------------------------
        
        public event Action Performed;

        public string BindingPath
        {
            get; private set;
        }

        public string BindingName
        {
            get; set;
        }

        public void SetBinding( InputControl control )
        {
            BindingPath = control.path;
            BindingName = $"{control.device.displayName}: {control.displayName}";
            
            SetBinding( BindingPath );
        }
        
        public void Save( string dataKey )
        {
            if( string.IsNullOrEmpty( BindingPath ) )
            {
                return;
            }
            var dataString = $"{BindingPath}@{BindingName}";
            PlayerPrefs.SetString( dataKey, dataString );
        }

        public void Load( string dataKey )
        {            
            if( PlayerPrefs.HasKey( dataKey ) )
            {
                var dataString = PlayerPrefs.GetString( dataKey );
                var dataSplitted = dataString.Split( '@' );

                BindingPath = dataSplitted[ 0 ].Trim();
                BindingName = dataSplitted[ 1 ].Trim();
                
                SetBinding( BindingPath );
            }
            else
            {
                BindingPath = null;
                BindingName = null;
            }
        }

        public void Disable()
        {
            inputAction.Disable();
        }
        
        //----------------------------------------------------------------------------------------------------
        
        InputAction inputAction;
        
        void SetBinding( string path )
        {
            inputAction.Disable();
            if( inputAction.bindings.Count > 0 )
            {
                inputAction.ChangeBinding( 0 ).Erase();
            }
            inputAction.AddBinding( path, "hold(duration=0.1)" );
            inputAction.Enable();
        }
    }

    
    public class InputManager : Singleton<InputManager>
    {
        public void ListenAxis( Action<InputControl> callback )
        {
            if( listenAxisCoroutine != null )
            {
                StopCoroutine( listenAxisCoroutine );
            }
            listenAxisCoroutine = ListenAxisCoroutine( callback );
            StartCoroutine( listenAxisCoroutine );
        }

        public void StopAxisListening()
        {
            if( listenAxisCoroutine != null )
            {
                StopCoroutine( listenAxisCoroutine );
                listenAxisCoroutine = null;
            }
        }

        
        public void ListenButton( Action<InputControl> callback )
        {
            listenButtonInputAction = new InputAction( type: InputActionType.PassThrough );

            listenButtonInputAction.AddBinding( "<Keyboard>/<button>" );
            listenButtonInputAction.AddBinding( "<Joystick>/<button>", "hold(duration=0.1)" );
            listenButtonInputAction.AddBinding( "<Gamepad>/<button>", "hold(duration=0.1)" );

            listenButtonInputAction.performed += context =>
            {
                if( context.control.name.Contains( "anyKey" ) || context.control.name.Contains( "escape" ) )
                {
                    return;
                }
                callback?.Invoke( context.control );
                listenButtonInputAction.Disable();
            };
            
            listenButtonInputAction.Enable();
        }

        public void StopButtonListening()
        {
            listenButtonInputAction?.Disable();
        }
        
        
        public AxisControl ThrottleControl => throttleControl;
        
        public AxisControl RollControl => rollControl;

        public AxisControl PitchControl => pitchControl;
        
        public AxisControl TrimControl => trimControl;

        public ButtonControl ViewControl => viewControl;
        
        public ButtonControl LaunchResetControl => launchResetControl;

                
        public bool AxesDisplay
        {
            get => axesDisplay;
            set
            {
                axesDisplay = value;
                
                if( axesDisplay )
                {
                    InputTelemetry.Instance.Show();
                }
                else
                {
                    InputTelemetry.Instance.Hide();
                }
                
                PlayerPrefs.SetInt( axesDisplayKey, axesDisplay ? 1 : 0 );
            }
        }
        
        public UnityAction OnEnterButton;
        public UnityAction OnEscapeButton;
        
        
        public void LoadPlayerPrefs()
        {
            throttleControl.Load( throttleControlInfoKey );
            rollControl.Load( rollControlInfoKey );
            pitchControl.Load( pitchControlInfoKey );
            trimControl.Load( trimControlInfoKey );
            viewControl.Load( viewControlInfoKey );
            launchResetControl.Load( launchResetControlInfoKey );
        }

        public void SavePlayerPrefs()
        {
            throttleControl.Save( throttleControlInfoKey );
            rollControl.Save( rollControlInfoKey );
            pitchControl.Save( pitchControlInfoKey );
            trimControl.Save( trimControlInfoKey );
            viewControl.Save( viewControlInfoKey );
            launchResetControl.Save( launchResetControlInfoKey );
        }

        //----------------------------------------------------------------------------------------------------
        
        readonly string throttleControlInfoKey = "ThrottleControlInfo";
        readonly string rollControlInfoKey = "RollControlInfo";
        readonly string pitchControlInfoKey = "PitchControlInfo";
        readonly string trimControlInfoKey = "TrimControlInfo";
        readonly string viewControlInfoKey = "ViewControlInfo";
        readonly string launchResetControlInfoKey = "LaunchResetControlInfo";
        readonly string axesDisplayKey = "AxesDisplay";
        
        AxisControl throttleControl;
        AxisControl rollControl;
        AxisControl pitchControl;
        AxisControl trimControl;
        ButtonControl viewControl;
        ButtonControl launchResetControl;
        InputAction listenButtonInputAction;
        bool axesDisplay;
        InputAction enterInputAction;
        InputAction escapeInputAction;
        
        
        IEnumerator ListenAxisCoroutine( Action<InputControl> callback, float threshold = 0.75f )
        {
            var axesDictionary = new Dictionary<string, float>();

            while( true )
            {
                foreach( var device in InputSystem.devices )
                {
                    if( device is Mouse || device is Keyboard )
                    {
                        continue;
                    }
                    
                    foreach( var control in device.allControls )
                    {
                        if( control.layout != "Axis" )
                        {
                            continue;
                        }
                        
                        if( axesDictionary.ContainsKey( control.name ) )
                        {
                            var oldAxisValue = axesDictionary[ control.name ];
                            var newAxisValue = (float)control.ReadValueAsObject();

                            if( Math.Abs( newAxisValue - oldAxisValue ) > threshold )
                            {
                                callback?.Invoke( control );
                                listenAxisCoroutine = null;
                                yield break;
                            }
                        }
                        else
                        {
                            if( control.IsActuated() )
                            {
                                axesDictionary.Add( control.name, (float) control.ReadValueAsObject() );
                            }
                        }
                    }
                }
                
                yield return null;
            }
        }
        IEnumerator listenAxisCoroutine;


        void Awake()
        {
            // Poll gamepads at 120 Hz
            //InputSystem.pollingFrequency = 120;
            
            throttleControl = new AxisControl();
            rollControl = new AxisControl();
            pitchControl = new AxisControl();
            trimControl = new AxisControl();
            viewControl = new ButtonControl();
            launchResetControl = new ButtonControl();

            //PlayerPrefs.DeleteAll();
            LoadPlayerPrefs();
            
            InputTelemetry.Instance.Init( this );
            if( PlayerPrefs.HasKey( axesDisplayKey ) )
            {
                AxesDisplay = PlayerPrefs.GetInt( axesDisplayKey ) > 0;
            }
            else
            {
                AxesDisplay = true;
            }
        }

        void OnEnable()
        {
            enterInputAction = new InputAction( type: InputActionType.Button, binding: "<Keyboard>/enter" );
            enterInputAction.performed += context => OnEnterButton?.Invoke();
            enterInputAction.Enable();

            escapeInputAction = new InputAction( type: InputActionType.Button, binding: "<Keyboard>/escape" );
            escapeInputAction.performed += context => OnEscapeButton?.Invoke();
            escapeInputAction.Enable();
        }
        
        void OnDisable()
        {
            enterInputAction.Disable();
            escapeInputAction.Disable();
            
            throttleControl.Disable();
            rollControl.Disable();
            pitchControl.Disable();
            trimControl.Disable();
            viewControl.Disable();
            launchResetControl.Disable();

            OnEnterButton = null;
            OnEscapeButton = null;
        }
    }
}