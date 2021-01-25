using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CGT
{
    [ExecuteInEditMode]
    public class UniqueIdManager : MonoBehaviour
    {
        [Header("Active Objects In Scene (Read Only)")]
        public List<UniqueId> objects;

        private static UniqueIdManager _instance = null;
        public static UniqueIdManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (UniqueIdManager)FindObjectOfType(typeof(UniqueIdManager));
                    if (_instance == null)
                    {
                        GameObject managers = GameObject.Find("/CGTManagers");
                        if(managers==null)
                            managers = new GameObject("CGTManagers");
                        _instance = (new GameObject("UniqueIdManager")).AddComponent<UniqueIdManager>();
                        _instance.transform.parent = managers.transform;
                    }                    
                }
                return _instance;
            }
        }

        public void Initialize()
        {
            Debug.Log("Initializing UniqueIdManager");
        }

#if UNITY_EDITOR
        private void Update()
        {
            objects = new List<UniqueId>(UniqueId.uniqueIds.Values);
        }
#endif
    }
}