using System;
using System.Collections.Generic;
using Libigl;
using UI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class InputManager : MonoBehaviour
{
    public static InputManager get;

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
    
    // Tools & Input State
    public int ActiveTool { get; private set; } = ToolType.Select;
    public UiInputHintsData[] toolInputHintsL;
    public UiInputHintsData[] toolInputHintsR;

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
        InitializeController(leftHandChar, out LeftHand, leftHandPrefab, leftHandRig, out _leftHandAnimator, out LeftHandHints, toolInputHintsL);
        InitializeController(rightHandChar, out RightHand, rightHandPrefab, rightHandRig, out _rightHandAnimator, out RightHandHints, toolInputHintsR);
        
        _leftHandTeleportReticle.SetActive(false);
        _rightHandTeleportReticle.SetActive(false);
        
        _leftHandLineRenderer.material = filledLineMat;
        _rightHandLineRenderer.material = filledLineMat;
    }

    /// <summary>
    /// Gets the XR InputDevice and sets the correct model to display.
    /// </summary>
    private void InitializeController(InputDeviceCharacteristics c, out InputDevice inputDevice, GameObject handPrefab,
        XRController modelParent, out Animator handAnimator, out UiInputHints inputHints, IReadOnlyList<UiInputHintsData> toolInputHints)
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
            if(inputHints && ActiveTool < toolInputHints.Count)
                inputHints.SetData(toolInputHints[ActiveTool]);
        }
        else
        {
            inputDevice = default;
            inputHints = null;
            // Debug.LogWarning("No hand controller found with characteristic: " + c);
        }
    }


    private bool _prevSecondaryBtnPressedL;
    private bool _prevAxisClickPressedL;
    private bool _prevAxisClickPressedR;
    private void Update()
    {
        if (!LeftHand.isValid)
            InitializeController(leftHandChar, out LeftHand, leftHandPrefab, leftHandRig, out _leftHandAnimator,
                out LeftHandHints, toolInputHintsL);
        else
        {
            // Teleport ray
            var success = leftHandRig.inputDevice.IsPressed(InputHelpers.Button.PrimaryAxis2DUp, out var teleportPressed, 0.2f);
            _leftHandTeleportReticle.SetActive(teleportPressed);
            _leftHandLineRenderer.material = teleportPressed ? filledLineMat : dottedLineMat;
            
            // Changing Active Tool
            leftHandRig.inputDevice.IsPressed(InputHelpers.Button.SecondaryButton, out var secondaryBtnPressed, 0.2f);
            if(secondaryBtnPressed && ! _prevSecondaryBtnPressedL)
                SetActiveTool((ActiveTool + 1) % ToolType.Size);
            _prevSecondaryBtnPressedL = secondaryBtnPressed;
            
            // Toggling UI hints
            leftHandRig.inputDevice.IsPressed(InputHelpers.Button.Primary2DAxisClick, out var axisClickPressed, 0.2f);
            if(axisClickPressed && !_prevAxisClickPressedL)
                LeftHandHints.gameObject.SetActive(!LeftHandHints.gameObject.activeSelf);
            _prevAxisClickPressedL = axisClickPressed;
        }

        if (!RightHand.isValid)
            InitializeController(rightHandChar, out RightHand, rightHandPrefab, rightHandRig, out _rightHandAnimator,
                out RightHandHints, toolInputHintsR);
        else
        {
            // Teleport ray
            rightHandRig.inputDevice.IsPressed(InputHelpers.Button.Grip, out var gripRPressed, 0.2f);
            _rightHandTeleportReticle.SetActive(gripRPressed);
            _rightHandLineRenderer.material = gripRPressed ? filledLineMat : dottedLineMat;
            
            // Toggling UI hints
            rightHandRig.inputDevice.IsPressed(InputHelpers.Button.Primary2DAxisClick, out var axisClickPressed, 0.2f);
            if(axisClickPressed && !_prevAxisClickPressedR)
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
        if(ActiveTool == value) return;
        if(value < toolInputHintsL.Length)
            LeftHandHints.SetData(toolInputHintsL[value]);
        if(value < toolInputHintsR.Length)
            RightHandHints.SetData(toolInputHintsR[value]);

        ActiveTool = value;
    }

    #endregion
}