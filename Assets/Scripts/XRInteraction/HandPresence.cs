using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HandPresence : MonoBehaviour
{
    public List<GameObject> controllerPrefabs;
    public InputDeviceCharacteristics characteristics;
    private InputDevice _rightHand;
    private GameObject _controllerModel;

    private void Start()
    {
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(characteristics, devices);

        foreach (var item in devices)
        {
            Debug.Log(item.name + ": " + item.characteristics);
        }

        if (devices.Count > 0)
        {
            _rightHand = devices[0];

            var prefab = controllerPrefabs.Find(go => _rightHand.name.StartsWith(go.name));
            if (prefab)
                _controllerModel = Instantiate(prefab, transform);
            else
            {
                //TODO: find correct names for Rift CV1 and Quest
                Debug.LogWarning("Could not find controller model, using default controller.");
                _controllerModel = Instantiate(controllerPrefabs[0], transform);
            }
        }
        else
            Debug.LogWarning("No right hand controllers found.");
    }

    private void Update()
    {
        if (_rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out var primaryBtnValue) && primaryBtnValue)
        {
        }

        if (_rightHand.TryGetFeatureValue(CommonUsages.trigger, out var triggerValue) && triggerValue > 0)
        {
        }
    }
}
