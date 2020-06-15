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
        public static SharedInputState Input;

        /// <summary>
        /// Get the XR Rig Transform, to determine world positions
        /// </summary>
        public Transform XRRig;

        [Tooltip("Show animated hands or the controller? Needs to be set before the controller is detected")]
        [SerializeField] private bool useHands = false;
        [SerializeField] private GameObject leftHandPrefab = default;
        [SerializeField] private GameObject rightHandPrefab = default;
        [SerializeField] private List<GameObject> controllerPrefabs = default;

        // Left Hand
        public InputDeviceCharacteristics leftHandChar =
            InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left;

        [SerializeField] private XRController leftHandRig = default;
        [NonSerialized] public InputDevice LeftHand;
        [NonSerialized] public UiInputHints LeftHandHints;

        // Right Hand
        public InputDeviceCharacteristics rightHandChar =
            InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right;

        [SerializeField] private XRController rightHandRig = default;
        [NonSerialized] public InputDevice RightHand;
        [NonSerialized] public UiInputHints RightHandHints;


        // Hand animation
        private Animator _leftHandAnimator;
        private Animator _rightHandAnimator;
        private static readonly int TriggerAnimId = Animator.StringToHash("Trigger");
        private static readonly int GripAnimId = Animator.StringToHash("Grip");

        // Teleporting
        private LineRenderer _leftHandLineRenderer;
        private LineRenderer _rightHandLineRenderer;
        private GameObject _leftHandTeleportReticle;
        private GameObject _rightHandTeleportReticle;
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

            _leftHandLineRenderer = leftHandRig.GetComponent<LineRenderer>();
            _leftHandTeleportReticle = leftHandRig.GetComponent<XRInteractorLineVisual>().reticle;
            _rightHandLineRenderer = rightHandRig.GetComponent<LineRenderer>();
            _rightHandTeleportReticle = rightHandRig.GetComponent<XRInteractorLineVisual>().reticle;
        }

        private void Start()
        {
            _leftHandTeleportReticle.SetActive(false);
            _rightHandTeleportReticle.SetActive(false);

            _leftHandLineRenderer.material = filledLineMat;
            _rightHandLineRenderer.material = filledLineMat;

            Input = SharedInputState.GetInstance();

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
                if (inputHints && Input.ActiveTool < toolInputHints.Count)
                    inputHints.SetData(toolInputHints[Input.ActiveTool]);

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
            // InputPrev = Input;
            UpdateSharedState();
            
            if (!LeftHand.isValid)
                InitializeController(false, leftHandChar, out LeftHand, leftHandPrefab, leftHandRig, out _leftHandAnimator,
                    out LeftHandHints, toolInputHintsL, out BrushL);
            else
            {
                // Teleport ray
                var success =
                    leftHandRig.inputDevice.IsPressed(InputHelpers.Button.PrimaryAxis2DUp, out var teleportPressed, 0.2f);
                _leftHandTeleportReticle.SetActive(teleportPressed);
                _leftHandLineRenderer.material = teleportPressed ? filledLineMat : dottedLineMat;

                // Changing Active Tool
                leftHandRig.inputDevice.IsPressed(InputHelpers.Button.SecondaryButton, out var secondaryBtnPressed, 0.2f);
                if (secondaryBtnPressed && !_prevSecondaryBtnPressedL)
                    SetActiveTool((Input.ActiveTool + 1) % ToolType.Size);
                _prevSecondaryBtnPressedL = secondaryBtnPressed;

                // Toggling UI hints
                leftHandRig.inputDevice.IsPressed(InputHelpers.Button.Primary2DAxisClick, out var axisClickPressed, 0.2f);
                if (axisClickPressed && !_prevAxisClickPressedL)
                    LeftHandHints.gameObject.SetActive(!LeftHandHints.gameObject.activeSelf);
                _prevAxisClickPressedL = axisClickPressed;
            }

            if (!RightHand.isValid)
                InitializeController(true, rightHandChar, out RightHand, rightHandPrefab, rightHandRig,
                    out _rightHandAnimator, out RightHandHints, toolInputHintsR, out BrushR);
            else
            {
                // Teleport ray
                rightHandRig.inputDevice.IsPressed(InputHelpers.Button.Grip, out var gripRPressed, 0.2f);
                _rightHandTeleportReticle.SetActive(gripRPressed);
                _rightHandLineRenderer.material = gripRPressed ? filledLineMat : dottedLineMat;

                // Toggling UI hints
                rightHandRig.inputDevice.IsPressed(InputHelpers.Button.Primary2DAxisClick, out var axisClickPressed, 0.2f);
                if (axisClickPressed && !_prevAxisClickPressedR)
                    RightHandHints.gameObject.SetActive(!RightHandHints.gameObject.activeSelf);
                _prevAxisClickPressedR = axisClickPressed;
            }

            if (useHands)
                UpdateHandAnimators();
        }

        private void UpdateHandAnimators()
        {
            if (_leftHandAnimator)
            {
                if (LeftHand.TryGetFeatureValue(CommonUsages.trigger, out var triggerLVal))
                    _leftHandAnimator.SetFloat(TriggerAnimId, triggerLVal);
                if (LeftHand.TryGetFeatureValue(CommonUsages.grip, out var gripLVal))
                    _leftHandAnimator.SetFloat(GripAnimId, gripLVal);
            }

            if (_rightHandAnimator)
            {
                if (RightHand.TryGetFeatureValue(CommonUsages.trigger, out var triggerRVal))
                    _rightHandAnimator.SetFloat(TriggerAnimId, triggerRVal);
                if (RightHand.TryGetFeatureValue(CommonUsages.grip, out var gripRVal))
                    _rightHandAnimator.SetFloat(GripAnimId, gripRVal);
            }
        }

        #region Tools

        /// <summary>
        /// Sets the active tool and updates the UI
        /// </summary>
        public void SetActiveTool(int value)
        {
            Input.ActiveTool = value;

            if(LeftHand.isValid)
            {
                if (LeftHandHints && value < toolInputHintsL.Length)
                    LeftHandHints.SetData(toolInputHintsL[value]);
                if (BrushL) BrushL.OnActiveToolChanged();
            }

            if(RightHand.isValid)
            {
                if (RightHandHints && value < toolInputHintsR.Length)
                    RightHandHints.SetData(toolInputHintsR[value]);
                if (BrushR) BrushR.OnActiveToolChanged();
            }
        }

        #endregion
    }
}