using System.Collections.Generic;
using libigl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MeshManager : MonoBehaviour
{
    public GameObject[] meshPrefabs;
    public GameObject uiListItemPrefab;
    public Transform uiListItemParent;
    
    public static LibiglMesh activeMesh;

    private void Start()
    {
        activeMesh = Instantiate(0);
        
        // Setup UI
        if (!uiListItemParent)
            uiListItemParent = transform;
        
        // Create listitem foreach 
        for (int i = 0; i < meshPrefabs.Length; i++)
        {
            // Parenting, layout, ui
            var go = Instantiate(uiListItemPrefab, uiListItemParent);
            var textField = go.GetComponentInChildren<TMP_Text>();
            textField.text = meshPrefabs[i].name;
            
            // setup callbacks/events
            var button = go.GetComponent<Button>();
            var i1 = i;
            button.onClick.AddListener(() => Instantiate(i1));
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            Instantiate(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            Instantiate(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            Instantiate(2);
    }

    #region Mesh Creation
    /// <summary>
    /// Instantiate a meshPrefab of a given index
    /// </summary>
    /// <param name="prefabIndex">Index in meshPrefabs to be created</param>
    /// <param name="setAsActiveMesh">Set this mesh as the active one</param>
    /// <returns>Index in the meshInstances. Set to -1 if the index is invalid.</returns>
    public LibiglMesh Instantiate(int prefabIndex, Vector3 pos = default, Quaternion rot = default, bool setAsActiveMesh = true)
    {
        if (prefabIndex >= meshPrefabs.Length)
        {
            Debug.LogWarning("Invalid index for MeshManager.Instantiate(). " +
                             $"Given index {prefabIndex} and length {meshPrefabs.Length}");
            return null;
        }
        
        var go = Instantiate(meshPrefabs[prefabIndex], pos, rot);
        go.transform.parent = transform;

        var mesh = go.GetComponent<LibiglMesh>();
        if (!mesh)
            mesh = go.AddComponent<LibiglMesh>();

        return mesh;
    }

    public void InstantiateFromUI(int prefabIndex)
    {
        Instantiate(prefabIndex);
    }
    #endregion
}
