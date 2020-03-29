using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MeshManager : MonoBehaviour
{
    public GameObject[] meshPrefabs;
    public List<GameObject> meshInstances;

    public int activeMesh = -1;
    public LayerMask raycastLayers;
    
    void Start()
    {
        meshInstances = new List<GameObject>(transform.childCount);
        for (int i = 0; i < transform.childCount; i++)
            meshInstances.Add(transform.GetChild(i).gameObject);

        activeMesh = -1;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
            Instantiate(0);
        else if (Input.GetKeyDown(KeyCode.Alpha1))
            Instantiate(1);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            Instantiate(2);
    }

    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var raycastHit, 10f,
                raycastLayers))
            {
                var T = raycastHit.collider.transform;
                if (T.parent.GetComponent<MeshManager>())
                    activeMesh = T.GetSiblingIndex();
            }
        }
    }

    #region Mesh Creation
    /// <summary>
    /// Instantiate a meshPrefab of a given index
    /// </summary>
    /// <param name="prefabIndex">Index in meshPrefabs to be created</param>
    /// <param name="setAsActiveMesh">Set this mesh as the active one</param>
    /// <returns>Index in the meshInstances. Set to -1 if the index is invalid.</returns>
    public int Instantiate(int prefabIndex, Vector3 pos = default, Quaternion rot = default, bool setAsActiveMesh = true)
    {
        if (prefabIndex >= meshPrefabs.Length)
        {
            Debug.LogWarning("Invalid index for MeshManager.Instantiate(). " +
                             $"Given index {prefabIndex} and length {meshPrefabs.Length}");
            return -1;
        }
        
        var go = Instantiate(meshPrefabs[prefabIndex], pos, rot);
        transform.parent = transform;
        meshInstances.Add(go);

        var instanceIndex = meshInstances.IndexOf(go);
        if (setAsActiveMesh)
            activeMesh = instanceIndex;
        
        return instanceIndex;
    }

    public void InstantiateFromUI(int prefabIndex)
    {
        Instantiate(prefabIndex);
    }
    #endregion
}
