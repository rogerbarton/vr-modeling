using System;
using System.Collections.Generic;
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

    // Right Hand
    public InputDeviceCharacteristics rightHandChar =
        InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right;

    [SerializeField] private XRController rightHandRig = default;
    [NonSerialized] public InputDevice RightHand;

    // Hand animation
    private Animator _leftHandAnimator;
    private Animator _rightHandAnimator;
    private static readonly int TriggerAnimId = Animator.StringToHash("Trigger");
    private static readonly int GripAnimId = Animator.StringToHash("Grip");


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
    }

    private void Start()
    {
        InitializeController(leftHandChar, out LeftHand, leftHandPrefab, leftHandRig, out _leftHandAnimator);
        InitializeController(rightHandChar, out RightHand, rightHandPrefab, rightHandRig, out _rightHandAnimator);
    }

    /// <summary>
    /// Gets the XR InputDevice and sets the correct model to display.
    /// </summary>
    private void InitializeController(InputDeviceCharacteristics c, out InputDevice inputDevice, GameObject handPrefab,
        XRController modelParent, out Animator handAnimator)
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
        }
        else
        {
            inputDevice = default;
            // Debug.LogWarning("No hand controller found with characteristic: " + c);
        }
    }

    private void Update()
    {
        if (!LeftHand.isValid)
            InitializeController(leftHandChar, out LeftHand, leftHandPrefab, leftHandRig, out _leftHandAnimator);

        if (!RightHand.isValid)
            InitializeController(rightHandChar, out RightHand, rightHandPrefab, rightHandRig, out _rightHandAnimator);

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
        // else
        // {
        //     _leftHandAnimator = leftHandRig.modelTransform.GetComponentInChildren<Animator>();
        // }

        if (_rightHandAnimator)
        {
            if (RightHand.TryGetFeatureValue(CommonUsages.trigger, out var triggerRVal))
                _rightHandAnimator.SetFloat(TriggerAnimId, triggerRVal);
            if (RightHand.TryGetFeatureValue(CommonUsages.grip, out var gripRVal))
                _rightHandAnimator.SetFloat(GripAnimId, gripRVal);
        }
        // else
        // {
        //     _rightHandAnimator = rightHandRig.modelTransform.GetComponentInChildren<Animator>();
        // }
    }
}