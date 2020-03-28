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
    public List<int> selectedMeshes = new List<int>();
    
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
            raycast
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
        meshInstances.Add(go);

        var instanceIndex = meshInstances.IndexOf(go);
        if (setAsActiveMesh)
            SetSelection(new []{instanceIndex});
        
        return instanceIndex;
    }
    #endregion


    #region Mesh Selection
    /// <summary>
    /// Set which mesh is the active mesh
    /// </summary>
    /// <param name="value">Mesh index in meshInstances</param>
    /// <param name="resetSelection"></param>
    public void SetActiveMesh(int value, bool resetSelection = true)
    {
        if(resetSelection)
            SetSelection(new []{value});
        else
        {
            activeMesh = value;
            if(!selectedMeshes.Contains(value))
                Debug.LogWarning($"MeshManager: selectedMeshes does not contain the activeMesh {activeMesh}.");
        }
    }

    /// <summary>
    /// Overwrites the current selection.
    /// </summary>
    /// <param name="newSelection">Indices of the newly selected meshes in meshInstances</param>
    /// <param name="setActiveAsFirst">If true the active mesh with be the first in <paramref name="newSelection"/></param>
    public void SetSelection(int[] newSelection, bool setActiveAsFirst = true)
    {
        if(setActiveAsFirst) activeMesh = newSelection[0];
        selectedMeshes.Clear();
        selectedMeshes.AddRange(newSelection);
    }
    
    /// <summary>
    /// Resets the selection and activeMesh
    /// </summary>
    public void ResetSelection()
    {
        activeMesh = -1;
        selectedMeshes.Clear();
    }
    #endregion
}
