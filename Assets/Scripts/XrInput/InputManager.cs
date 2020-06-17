using System;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace XrInput
{
    public partial class InputManager : MonoBehaviour
    {
        public static InputManager get;
        public static SharedInputState State;
        // Input at the last frame (main thread)
        public static SharedInputState StatePrev;

        /// <summary>
        /// Get the XR Rig Transform, to determine world positions
        /// </summary>
        public Transform XRRig;

        [Tooltip("Show animated hands or the controller? Needs to be set before the controller is detected")]
        [SerializeField] private bool useHands = false;
        [SerializeField] private GameObject handPrefabL = default;
        [SerializeField] private GameObject handPrefabR = default;
        [SerializeField] private List<GameObject> controllerPrefabs = default;

        // Left Hand
        public InputDeviceCharacteristics handCharL =
            InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left;

        [SerializeField] private XRController handRigL = default;
        [NonSerialized] public InputDevice HandL;
        [NonSerialized] public UiInputHints HandHintsL; // TODO: move to UiManager

        // Right Hand
        public InputDeviceCharacteristics handCharR =
            InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right;

        [SerializeField] private XRController handRigR = default;
        [NonSerialized] public InputDevice HandR;
        [NonSerialized] public UiInputHints HandHintsR;


        // Hand animation
        private Animator _handAnimatorL;
        private Animator _handAnimatorR;
        private static readonly int TriggerAnimId = Animator.StringToHash("Trigger");
        private static readonly int GripAnimId = Animator.StringToHash("Grip");

        // Teleporting
        private LineRenderer _handLineRendererL;
        private LineRenderer _handLineRendererR;
        private GameObject _handTeleportReticleL;
        private GameObject _handTeleportReticleR;
        public Material filledLineMat;
        public Material dottedLineMat;

        public UiInputHintsData[] toolInputHintsL;
        public UiInputHintsData[] toolInputHintsR;
    
        [NonSerialized] public XrBrush BrushL;
        [NonSerialized] public XrBrush BrushR;

        private void Awake()
        {
            if (get)
            {
                Debug.LogWarning("InputManager instance already exists.");
                enabled = false;
                return;
            }

            get = this;
            if (!XRRig)
                XRRig = transform;

            _handLineRendererL = handRigL.GetComponent<LineRenderer>();
            _handTeleportReticleL = handRigL.GetComponent<XRInteractorLineVisual>().reticle;
            _handLineRendererR = handRigR.GetComponent<LineRenderer>();
            _handTeleportReticleR = handRigR.GetComponent<XRInteractorLineVisual>().reticle;
        }

        private void Start()
        {
            _handTeleportReticleL.SetActive(false);
            _handTeleportReticleR.SetActive(false);

            _handLineRendererL.material = filledLineMat;
            _handLineRendererR.material = filledLineMat;

            State = SharedInputState.GetInstance();
            StatePrev = State;

            SetActiveTool(ToolType.Select);
        }

        /// <summary>
        /// Gets the XR InputDevice and sets the correct model to display.
        /// </summary>
        /// <returns>True if successful</returns>
        private bool InitializeController(bool isRight, InputDeviceCharacteristics c, out InputDevice inputDevice,
            GameObject handPrefab,
            XRController modelParent, out Animator handAnimator, out UiInputHints inputHints,
            IReadOnlyList<UiInputHintsData> toolInputHints,
            out XrBrush brush)
        {
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(c, devices);

            foreach (var item in devices)
                Debug.Log(item.name + ": " + item.characteristics);

            handAnimator = null;
            if (devices.Count > 0)
            {
                inputDevice = devices[0];

                var deviceName = inputDevice.name;
                var prefab = useHands ? handPrefab : controllerPrefabs.Find(p => deviceName.StartsWith(p.name));

                if (!prefab)
                {
                    //TODO: find correct names for Rift CV1 and Quest
                    Debug.LogWarning($"Could not find controller model with name {deviceName}, using default controller.");
                    prefab = controllerPrefabs[0];
                }

                var go = Instantiate(prefab, modelParent.modelTransform);
                if (useHands)
                    handAnimator = go.GetComponent<Animator>();

                inputHints = go.GetComponentInChildren<UiInputHints>();
                if (inputHints && State.ActiveTool < toolInputHints.Count)
                    inputHints.SetData(toolInputHints[State.ActiveTool]);

                brush = go.GetComponentInChildren<XrBrush>();
                if (brush) brush.Initialize(isRight);

                return true;
            }

            inputDevice = default;
            inputHints = null;
            brush = null;
            // Debug.LogWarning("No hand controller found with characteristic: " + c);
            return false;
        }


        private bool _prevSecondaryBtnPressedL;
        private bool _prevAxisClickPressedL;
        private bool _prevAxisClickPressedR;

        private void Update()
        {
            UpdateSharedState();
            
            if (!HandL.isValid)
                InitializeController(false, handCharL, out HandL, handPrefabL, handRigL, out _handAnimatorL,
                    out HandHintsL, toolInputHintsL, out BrushL);
            else
            {
                // Teleport ray
                var success =
                    handRigL.inputDevice.IsPressed(InputHelpers.Button.PrimaryAxis2DUp, out var teleportPressed, 0.2f);
                _handTeleportReticleL.SetActive(teleportPressed);
                _handLineRendererL.material = teleportPressed ? filledLineMat : dottedLineMat;

                // Changing Active Tool
                handRigL.inputDevice.IsPressed(InputHelpers.Button.SecondaryButton, out var secondaryBtnPressed, 0.2f);
                if (secondaryBtnPressed && !_prevSecondaryBtnPressedL)
                    SetActiveTool((State.ActiveTool + 1) % ToolType.Size);
                _prevSecondaryBtnPressedL = secondaryBtnPressed;

                // Toggling UI hints
                handRigL.inputDevice.IsPressed(InputHelpers.Button.Primary2DAxisClick, out var axisClickPressed, 0.2f);
                if (axisClickPressed && !_prevAxisClickPressedL)
                    HandHintsL.gameObject.SetActive(!HandHintsL.gameObject.activeSelf);
                _prevAxisClickPressedL = axisClickPressed;
            }

            if (!HandR.isValid)
                InitializeController(true, handCharR, out HandR, handPrefabR, handRigR,
                    out _handAnimatorR, out HandHintsR, toolInputHintsR, out BrushR);
            else
            {
                // Teleport ray
                handRigR.inputDevice.IsPressed(InputHelpers.Button.Grip, out var gripRPressed, 0.2f);
                _handTeleportReticleR.SetActive(gripRPressed);
                _handLineRendererR.material = gripRPressed ? filledLineMat : dottedLineMat;

                // Toggling UI hints
                handRigR.inputDevice.IsPressed(InputHelpers.Button.Primary2DAxisClick, out var axisClickPressed, 0.2f);
                if (axisClickPressed && !_prevAxisClickPressedR)
                    HandHintsR.gameObject.SetActive(!HandHintsR.gameObject.activeSelf);
                _prevAxisClickPressedR = axisClickPressed;
            }

            if (useHands)
                UpdateHandAnimators();
        }

        private void UpdateHandAnimators()
        {
            if (_handAnimatorL)
            {
                if (HandL.TryGetFeatureValue(CommonUsages.trigger, out var triggerLVal))
                    _handAnimatorL.SetFloat(TriggerAnimId, triggerLVal);
                if (HandL.TryGetFeatureValue(CommonUsages.grip, out var gripLVal))
                    _handAnimatorL.SetFloat(GripAnimId, gripLVal);
            }

            if (_handAnimatorR)
            {
                if (HandR.TryGetFeatureValue(CommonUsages.trigger, out var triggerRVal))
                    _handAnimatorR.SetFloat(TriggerAnimId, triggerRVal);
                if (HandR.TryGetFeatureValue(CommonUsages.grip, out var gripRVal))
                    _handAnimatorR.SetFloat(GripAnimId, gripRVal);
            }
        }

        #region Tools

        /// <summary>
        /// Sets the active tool and updates the UI
        /// </summary>
        public void SetActiveTool(int value)
        {
            State.ActiveTool = value;

            if(HandL.isValid)
            {
                if (HandHintsL && value < toolInputHintsL.Length)
                    HandHintsL.SetData(toolInputHintsL[value]);
                if (BrushL) BrushL.OnActiveToolChanged();
            }

            if(HandR.isValid)
            {
                if (HandHintsR && value < toolInputHintsR.Length)
                    HandHintsR.SetData(toolInputHintsR[value]);
                if (BrushR) BrushR.OnActiveToolChanged();
            }
        }

        #endregion
    }
}