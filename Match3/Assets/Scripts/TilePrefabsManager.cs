using System;
using System.Collections.Generic;
using UnityEngine;

public class TilePrefabsManager : MonoBehaviour
{
    private static TilePrefabsManager instance;

    [SerializeField]
    private InspectorSetupStruct[] prefabsInspectorSetup;

    private Dictionary<IdEnum, GameObject> prefabs;

    public static TilePrefabsManager Instance { get { return instance; } }

    private void Awake()
    {
        instance = this;

        prefabs = new Dictionary<IdEnum, GameObject>();
        Array.ForEach(prefabsInspectorSetup, element => prefabs.Add(element.id, element.prefab));
    }

    private enum IdEnum
    {
        TILE_RED,
        TILE_GREEN,
        TILE_BLUE,
        TILE_YELLOW
    }

    [Serializable]
    private struct InspectorSetupStruct
    {
        public IdEnum id;
        public GameObject prefab;
    }
}
