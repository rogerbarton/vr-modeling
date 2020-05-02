using System.Collections.Generic;
using libigl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MeshManager : MonoBehaviour
{
    public GameObject[] meshPrefabs;
    public Transform uiListItemParent;
    private GameObject _uiListItemPrefab;
    public Transform meshSpawnPoint;
    
    public static LibiglMesh ActiveMesh;

    private void Start()
    {
        if(meshPrefabs.Length > 0)
            ActiveMesh = LoadMesh(meshPrefabs[0]);

        // Convention: Use the first child as the prefab
        if (!_uiListItemPrefab && uiListItemParent.childCount > 0)
            _uiListItemPrefab = uiListItemParent.GetChild(uiListItemParent.childCount -1).gameObject;
        
        // Create listitem foreach 
        foreach (var mesh in meshPrefabs)
            SetupUI(mesh);
    }

    /// <summary>
    /// Generates UI for loading the <paramref name="meshPrefab"/>
    /// </summary>
    /// <param name="meshPrefab"></param>
    private void SetupUI(GameObject meshPrefab)
    {
        // Parenting, layout, ui
        var go = Instantiate(_uiListItemPrefab, uiListItemParent);
        go.SetActive(true);
        var textField = go.GetComponentInChildren<TMP_Text>();
        textField.text = meshPrefab.name;
            
        // setup callbacks/events
        var button = go.GetComponent<Button>();
        button.onClick.AddListener(() => LoadMesh(meshPrefab));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            LoadMeshById(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            LoadMeshById(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            LoadMeshById(2);
    }

    #region Mesh Creation
    /// <summary>
    /// Instantiate a mesh that can be used with libigl
    /// </summary>
    /// <param name="prefab">Prefab to be created</param>
    /// <param name="setAsActiveMesh">Set this mesh as the active one</param>
    /// <returns>LibiglMesh component on the new instance</returns>
    public LibiglMesh LoadMesh(GameObject prefab, bool setAsActiveMesh = true)
    {
        var go = Instantiate(prefab, meshSpawnPoint.position, meshSpawnPoint.transform.rotation, transform);
        go.transform.parent = transform;

        var lmesh = go.GetComponent<LibiglMesh>();
        if (!lmesh)
        {
            Debug.LogWarning($"Prefab does not have a {nameof(LibiglMesh)} component, it will be added.");
            lmesh = go.AddComponent<LibiglMesh>();
        }

        if (setAsActiveMesh)
            ActiveMesh = lmesh;

        return lmesh;
    }

    /// <param name="prefabIndex">Index in <see cref="meshPrefabs"/></param>
    public void LoadMeshById(int prefabIndex)
    {
        LoadMesh(meshPrefabs[prefabIndex]);
    }
    #endregion
}
