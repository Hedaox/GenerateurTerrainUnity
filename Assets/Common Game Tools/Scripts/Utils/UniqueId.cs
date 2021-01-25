using UnityEngine;
using System.Collections.Generic;
using System;
using System.Security.Cryptography;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace CGT
{
    [ExecuteInEditMode]
    public class UniqueId : MonoBehaviour
    {        
        public static Dictionary<string, UniqueId> uniqueIds = new Dictionary<string, UniqueId>();

        public string uniqueId="";

        // This prevent to run this code on our runtime version
#if UNITY_EDITOR
        
        void Update()
        {            
            if (Application.isPlaying)
                return;

            //You need to save the scene to get uniqueId
            if (gameObject.scene.name.Length == 0)
                return;

            String uid = uniqueId;

            //Scene has been 'saved as...' and all uids are dupicated
            if (uniqueId.Length > 0 && !uid.StartsWith(gameObject.scene.name+"."))
            {
                uniqueId = gameObject.scene.name + "." + GenerateHash(Guid.NewGuid().ToString());
                EditorUtility.SetDirty(this);
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            } 
            //Collision!!
            else if (uniqueId.Length > 0 && uniqueIds.ContainsKey(uniqueId) && uniqueIds[uniqueId] != this)
            {
                uniqueId = gameObject.scene.name + "." + GenerateHash(Guid.NewGuid().ToString());
                EditorUtility.SetDirty(this);
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
            else if(uniqueId.Length == 0)
                uniqueId = gameObject.scene.name+"."+GenerateHash(Guid.NewGuid().ToString());

            //Assure the uniqueId is registered
            if (!uniqueIds.ContainsKey(uniqueId))
            {
                uniqueIds.Add(uniqueId, this);
                UniqueIdManager.instance.objects = new List<UniqueId>(uniqueIds.Values);
            }
            if(!uid.Equals(uniqueId))
            {
                //Broadcast an 'update uid' event
                gameObject.SendMessage("UpdateUID", uid.ToString(), SendMessageOptions.DontRequireReceiver);
            }
        }

        //Remove IDs from list
        void OnDestroy()
        {
            uniqueIds.Remove(uniqueId);            
        }

        //16^10 ids should be enough to make collisions very rare
        public string GenerateHash(string str)
        {
            using (var md5Hasher = MD5.Create())
            {
                var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(str));
                return BitConverter.ToString(data).Replace("-", "").Substring(0, 10);
            }
        }
#endif
    }
}