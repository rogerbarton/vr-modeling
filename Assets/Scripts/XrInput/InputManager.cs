using System;
using System.Collections.Generic;
using UI;
using UI.Hints;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace XrInput
{
    /// <summary>
    /// This script handles the controller input and is based on the Unity XR Interaction Toolkit.
    /// An important part is that this is where the <see cref="InputState"/> can be accessed and is updated.
    /// </summary>
    public partial class InputManager : MonoBehaviour
    {
        /// <summary>
        /// The singleton instance.
        /// </summary>
        public static InputManager get;

        /// <summary>
        /// The current input state shared between all meshes.
        /// </summary>
        public static InputState State;

        /// <summary>
        /// Input at the last frame (main thread).
        /// </summary>
        public static InputState StatePrev;

        /// <summary>
        /// Get the XR Rig Transform, to determine world positions of controllers.
        /// </summary>
        public Transform xrRig;

        /// <summary>
        /// Called just after the active tool has changed
        /// </summary>
        public static event Action OnActiveToolChanged = delegate { };

        [Tooltip("Show animated hands or the controller? Needs to be set before the controller is detected")]
        [SerializeField]
        private bool useHands = false;

        [SerializeField] private GameObject handPrefabL = default;
        [SerializeField] private GameObject handPrefabR = default;
        [SerializeField] private List<GameObject> controllerPrefabs = default;

        // -- Left Hand
        public InputDeviceCharacteristics handCharL =
            InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left;

        [SerializeField] private XRController handRigL = default;
        [SerializeField] private XRRayInteractor handInteractorL = default;
        [NonSerialized] public InputDevice HandL;
        [NonSerialized] public UiInputHints HandHintsL;
        [NonSerialized] public XrBrush BrushL;
        private readonly List<XRBaseInteractable> _rayHoverTargetsL = new List<XRBaseInteractable>();

        // -- Right Hand
        public InputDeviceCharacteristics handCharR =
            InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right;

        [SerializeField] private XRController handRigR = default;
        [SerializeField] private XRRayInteractor handInteractorR = default;
        [NonSerialized] public InputDevice HandR;
        [NonSerialized] public UiInputHints HandHintsR;
        [NonSerialized] public XrBrush BrushR;
        private readonly List<XRBaseInteractable> _rayHoverTargetsR = new List<XRBaseInteractable>();

        // -- Hand animation
        private Animator _handAnimatorL;
        private Animator _handAnimatorR;
        private static readonly int TriggerAnimId = Animator.StringToHash("Trigger");
        private static readonly int GripAnimId = Animator.StringToHash("Grip");

        // -- Teleporting
        /// <summary>
        /// The ray interactor for teleporting
        /// </summary>
        [SerializeField] private XRRayInteractor teleportRayL = default;

        private void Awake()
        {
            if (get)
            {
                Debug.LogWarning("InputManager instance already exists.");
                enabled = false;
                return;
            }

            get = this;
            if (!xrRig)
                xrRig = transform;
        }

        private void Start()
        {
            State = InputState.GetInstance();
            StatePrev = State;

            // Setup rays/teleporting
            teleportRayL.gameObject.SetActive(false);

            // Set active tool once everything required is initialized
            SetActiveTool(ToolType.Select);
        }

        private bool _prevAxisClickPressedL;
        private bool _prevAxisClickPressedR;

        private void Update()
        {
            UpdateSharedState();

            if (!HandL.isValid && !HandHintsL)
                InitializeController(false, handCharL, out HandL, handPrefabL, handRigL, out _handAnimatorL,
                    out HandHintsL, out BrushL);
            else
            {
                // Toggling UI hints
                handRigL.inputDevice.IsPressed(InputHelpers.Button.Primary2DAxisClick, out var axisClickPressed, 0.2f);
                if (axisClickPressed && !_prevAxisClickPressedL)
                    HandHintsL.gameObject.SetActive(!HandHintsL.gameObject.activeSelf);
                _prevAxisClickPressedL = axisClickPressed;
            }

            if (!HandR.isValid && !HandHintsR)
                InitializeController(true, handCharR, out HandR, handPrefabR, handRigR,
                    out _handAnimatorR, out HandHintsR, out BrushR);
            else
            {
                // Toggling UI hints
                handRigR.inputDevice.IsPressed(InputHelpers.Button.Primary2DAxisClick, out var axisClickPressed, 0.2f);
                if (axisClickPressed && !_prevAxisClickPressedR)
                    HandHintsR.gameObject.SetActive(!HandHintsR.gameObject.activeSelf);
                _prevAxisClickPressedR = axisClickPressed;
            }

            if (useHands)
                UpdateHandAnimators();
        }

        #region Controllers

        /// <summary>
        /// Gets the XR InputDevice and sets the correct model to display.
        /// This is where a controller is detected and initialized.
        /// </summary>
        /// <returns>True if successful</returns>
        private bool InitializeController(bool isRight, InputDeviceCharacteristics characteristics,
            out InputDevice inputDevice,
            GameObject handPrefab,
            XRController modelParent, out Animator handAnimator, out UiInputHints inputHints,
            out XrBrush brush)
        {
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(characteristics, devices);

            foreach (var item in devices)
                Debug.Log($"Detected {item.name}: {item.characteristics}");

            handAnimator = null;
            if (devices.Count > 0)
            {
                inputDevice = devices[0];

                var deviceName = inputDevice.name;
                var prefab = useHands ? handPrefab : controllerPrefabs.Find(p => deviceName.StartsWith(p.name));

                if (!prefab)
                {
                    //TODO: find correct names for Rift CV1 and Quest
                    Debug.LogWarning(
                        $"Could not find controller model with name {deviceName}, using default controllers.");
                    prefab = controllerPrefabs[isRight ? 1 : 0];
                }

                var go = Instantiate(prefab, modelParent.modelTransform);
                if (useHands)
                    handAnimator = go.GetComponent<Animator>();

                inputHints = go.GetComponentInChildren<UiInputHints>();
                inputHints.Initialize();
                if (inputHints) RepaintInputHints(!isRight, isRight);

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
        
        
        /// <summary>
        /// Enables and disables certain rays and UI interaction for performance.
        /// Raycasting the UI is one of the most expensive operations currently. See profile EventSystem
        /// </summary>
        private void UpdateRayInteractors()
        {
            if (!handInteractorL) return;

            if (State.IsTeleporting && !StatePrev.IsTeleporting) // on pressed
            {
                teleportRayL.gameObject.SetActive(true);
                handInteractorL.gameObject.SetActive(false);
                if (handInteractorR)
                    handInteractorR.gameObject.SetActive(false);
            }
            else if (!State.IsTeleporting && StatePrev.IsTeleporting) // on depressed
            {
                teleportRayL.gameObject.SetActive(false);
                handInteractorL.gameObject.SetActive(true);
                if (handInteractorR)
                    handInteractorR.gameObject.SetActive(true);
            }
            else if (!State.IsTeleporting)
            {
                // Only allow UI interaction if we are idle, not grabbing anything and hovering over a UI object
                var allowUiInteraction =
                    (State.ActiveTool == ToolType.Transform && State.ToolTransformMode == ToolTransformMode.Idle
                     || State.ActiveTool == ToolType.Select && State.ToolSelectMode == ToolSelectMode.Idle);

                handInteractorL.enableUIInteraction =
                    allowUiInteraction &&
                    (!handInteractorL.selectTarget ||
                     handInteractorL.selectTarget.gameObject.layer == UiManager.UiLayer) &&
                    _rayHoverTargetsL.Exists(t => t.gameObject.layer == UiManager.UiLayer);

                if (handInteractorR)
                    handInteractorR.enableUIInteraction =
                        allowUiInteraction &&
                        (!handInteractorR.selectTarget ||
                         handInteractorR.selectTarget.gameObject.layer == UiManager.UiLayer) &&
                        _rayHoverTargetsR.Exists(t => t.gameObject.layer == UiManager.UiLayer);
            }
        }

        #endregion

        #region Tools

        /// <summary>
        /// Sets the active tool and updates the UI
        /// </summary>
        public void SetActiveTool(ToolType value)
        {
            State.ActiveTool = value;

            RepaintInputHints();

            if (BrushL) BrushL.OnActiveToolChanged();
            if (BrushR) BrushR.OnActiveToolChanged();

            OnActiveToolChanged();
        }

        /// <summary>
        /// Safely repaint the input hints on the controllers, specify which hands should be repainted.
        /// </summary>
        public void RepaintInputHints(bool left = true, bool right = true)
        {
            if (left && HandHintsL)
                HandHintsL.Repaint();
            if (right && HandHintsR)
                HandHintsR.Repaint();
        }

        #endregion

        #region Updating the InputState

        /// <summary>
        /// Updates the <see cref="InputState"/> <see cref="State"/>.
        /// Implementation note: The hands may not be initialized/detected yet.
        /// </summary>
        private void UpdateSharedState()
        {
            StatePrev = State;

            HandL.TryGetFeatureValue(CommonUsages.primaryButton, out State.PrimaryBtnL);
            HandL.TryGetFeatureValue(CommonUsages.secondaryButton, out State.SecondaryBtnL);
            HandL.TryGetFeatureValue(CommonUsages.primary2DAxis, out State.PrimaryAxisL);

            HandR.TryGetFeatureValue(CommonUsages.primaryButton, out State.PrimaryBtnR);
            HandR.TryGetFeatureValue(CommonUsages.secondaryButton, out State.SecondaryBtnR);
            HandR.TryGetFeatureValue(CommonUsages.primary2DAxis, out State.PrimaryAxisR);


            // Read values and then convert to world space
            HandL.TryGetFeatureValue(CommonUsages.grip, out State.GripL);
            HandL.TryGetFeatureValue(CommonUsages.trigger, out State.TriggerL);
            HandL.TryGetFeatureValue(CommonUsages.devicePosition, out State.HandPosL);
            HandL.TryGetFeatureValue(CommonUsages.deviceRotation, out State.HandRotL);

            HandR.TryGetFeatureValue(CommonUsages.grip, out State.GripR);
            HandR.TryGetFeatureValue(CommonUsages.trigger, out State.TriggerR);
            HandR.TryGetFeatureValue(CommonUsages.devicePosition, out State.HandPosR);
            HandR.TryGetFeatureValue(CommonUsages.deviceRotation, out State.HandRotR);

            // Convert to world space
            var xrRigRotation = xrRig.rotation;
            State.HandPosL = xrRig.TransformPoint(State.HandPosL);
            State.HandRotL = xrRigRotation * State.HandRotL;
            State.HandPosR = xrRig.TransformPoint(State.HandPosR);
            State.HandRotR = xrRigRotation * State.HandRotR;
            
            // Update hover targets of rays
            State.IsTeleporting = State.PrimaryAxisL.y > 0.1f; // Threshold should be lower than teleportRayL's
            if (!State.IsTeleporting)
            {
                if (handInteractorL)
                    handInteractorL.GetHoverTargets(_rayHoverTargetsL);
                if (handInteractorR)
                    handInteractorR.GetHoverTargets(_rayHoverTargetsR);
            }

            // Ray Interactor & Teleportation
            UpdateRayInteractors();

            // Input conflict with interactables
            if (handInteractorL)
                State.GripL *= handInteractorL.selectTarget ? 0f : 1f;
            if (handInteractorR)
                State.GripR *= handInteractorR.selectTarget ? 0f : 1f;
            State.TriggerL *= !State.IsTeleporting && _rayHoverTargetsL.Count == 0 ? 1f : 0f;
            State.TriggerR *= !State.IsTeleporting && _rayHoverTargetsR.Count == 0 ? 1f : 0f;

            // Changing Active Tool
            if (State.SecondaryBtnL && !StatePrev.SecondaryBtnL)
                SetActiveTool((ToolType)
                    ((State.ActiveTool.GetHashCode() + 1) % Enum.GetNames(typeof(ToolType)).Length));

            // Brush Resizing
            if (Mathf.Abs(State.PrimaryAxisR.y) > XrBrush.ResizeDeadZone)
            {
                State.BrushRadius = Mathf.Clamp(
                    State.BrushRadius + XrBrush.ResizeSpeed * Time.deltaTime * State.PrimaryAxisR.y,
                    XrBrush.RadiusRange.x, XrBrush.RadiusRange.y);

                if (BrushL)
                    BrushL.SetRadius(State.BrushRadius);
                BrushR.SetRadius(State.BrushRadius);
            }

            // Changing the Active Mesh
            if (State.ActiveTool == ToolType.Transform &&
                State.TriggerR > 0.1f && StatePrev.TriggerR < 0.1f)
            {
                BrushR.SetActiveMesh();
            }
        }

        #endregion
    }
}