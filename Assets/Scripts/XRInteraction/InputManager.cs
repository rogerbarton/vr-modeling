using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class InputManager : MonoBehaviour
{
    public static InputManager get;
    /// <summary>
    /// Get the XR Rig Transform, to determine world positions
    /// </summary>
    public Transform XRRig;

    [SerializeField] private List<GameObject> controllerPrefabs;

    // Left Hand
    public InputDeviceCharacteristics leftHandChar =
        InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left;

    [SerializeField] private Transform leftHandRig;

    [NonSerialized] public InputDevice LeftHand;
    
    // Right Hand
    public InputDeviceCharacteristics rightHandChar =
        InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right;

    [SerializeField] private Transform rightHandRig;

    [NonSerialized] public InputDevice RightHand;

    private void Awake()
    {
        if (get)
        {
            Debug.LogWarning("InputManager instance already exists.");
            enabled = false;
            return;
        }

        get = this;
        if(!XRRig)
            XRRig = transform;
    }

    private void Start()
    {
        InitializeController(leftHandChar, out LeftHand, leftHandRig);
        InitializeController(rightHandChar, out RightHand, rightHandRig);
    }

    /// <summary>
    /// Gets the XR InputDevice and sets the correct model to display.
    /// </summary>
    /// <param name="c"></param>
    /// <param name="inputDevice"></param>
    /// <param name="modelParent"></param>
    private void InitializeController(InputDeviceCharacteristics c, out InputDevice inputDevice, Transform modelParent)
    {
        
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(c, devices);

        foreach (var item in devices)
            Debug.Log(item.name + ": " + item.characteristics);

        if (devices.Count > 0)
        {
            inputDevice = devices[0];

            var deviceName = inputDevice.name;
            var prefab = controllerPrefabs.Find(go => deviceName.StartsWith(go.name));
            if (!prefab)
            {
                //TODO: find correct names for Rift CV1 and Quest
                Debug.LogWarning($"Could not find controller model with name {deviceName}, using default controller.");
                prefab = controllerPrefabs[0];
            }

            modelParent.GetComponent<XRController>().modelPrefab = prefab.transform;
        }
        else
        {
            inputDevice = default;
            Debug.LogWarning("No hand controller found with characteristic: " + c);
        }
    }

    private void Update()
    {
        if (RightHand.TryGetFeatureValue(CommonUsages.primaryButton, out var primaryBtnValue) && primaryBtnValue)
        {
        }

        if (RightHand.TryGetFeatureValue(CommonUsages.trigger, out var triggerValue) && triggerValue > 0)
        {
        }
    }
}