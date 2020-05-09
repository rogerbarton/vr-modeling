using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class InputManager : MonoBehaviour
{
    public static InputManager get;

    [SerializeField] private List<GameObject> controllerPrefabs;

    public InputDeviceCharacteristics rightHandChar =
        InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right;

    [SerializeField] private Transform rightHandRig;

    [NonSerialized] public InputDevice RightHand;
    [NonSerialized] public GameObject RightHandModel;

    private void Awake()
    {
        if (get)
        {
            Debug.LogWarning("InputManager instance already exists.");
            enabled = false;
            return;
        }

        get = this;
    }

    private void Start()
    {
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(rightHandChar, devices);

        foreach (var item in devices)
            Debug.Log(item.name + ": " + item.characteristics);

        if (devices.Count > 0)
        {
            RightHand = devices[0];

            var prefab = controllerPrefabs.Find(go => RightHand.name.StartsWith(go.name));
            if (prefab)
                RightHandModel = Instantiate(prefab, rightHandRig);
            else
            {
                //TODO: find correct names for Rift CV1 and Quest
                Debug.LogWarning("Could not find controller model, using default controller.");
                RightHandModel = Instantiate(controllerPrefabs[0], rightHandRig);
            }
        }
        else
            Debug.LogWarning("No right hand controller found.");
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