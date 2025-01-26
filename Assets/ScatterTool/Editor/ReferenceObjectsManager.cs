using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScatterTool
{
    internal class ReferenceObjData
    {
        internal GameObject ObjRef { get; private set; }
        internal Rigidbody rb = null;
        internal bool wasKinematic = false;
        internal bool wasKinematicOverriden = false;

        internal ReferenceObjData(GameObject obj)
        {
            ObjRef = obj;
            if (PrefabUtility.IsPartOfPrefabInstance(obj))
                ObjRef = PrefabUtility.GetNearestPrefabInstanceRoot(ObjRef);
        }

        internal void UpdateData()
        {
            rb = ObjRef.GetComponent<Rigidbody>();
            wasKinematic = rb != null && rb.isKinematic;
            wasKinematicOverriden = rb != null && Utils.IsKinematicOverriden(rb);
        }
    }

    public class ReferenceObjectsManager
    {
        List<ReferenceObjData> referenceObjects = new List<ReferenceObjData>();

        internal int NumReferenceObjects { get { return referenceObjects.Count; } private set { } }
        internal bool HasReferenceObjects { get { return referenceObjects.Count > 0; } private set { } }

        int selObjIndex = 0;

        internal ReferenceObjData GetNextObject(bool randomized)
        {
            ReferenceObjData obj = null;
            while(obj == null && HasReferenceObjects)
            {
                if(randomized)
                    selObjIndex = Random.Range(0, referenceObjects.Count);
                else
                    selObjIndex = (selObjIndex + 1) % referenceObjects.Count;

                obj = referenceObjects[selObjIndex];
                if(obj == null || obj.ObjRef == null)
                {
                    obj = null;
                    referenceObjects.RemoveAt(selObjIndex);
                }
            }

            return obj;
        }

        /// <param name="targets">The collection returned by querying the selection</param>
        /// <param name="errorText">Will contain the last error in the selected list</param>
        internal void UpdateFromSelection(IEnumerable<Object> targets, ref string errorText)
        {
            referenceObjects.Clear();
            foreach (var selection in targets)
            {
                if (selection != null && selection.GetType() == typeof(GameObject))
                {
                    var obj = selection as GameObject;

                    if (!IsObjectValid(obj, ref errorText))
                        continue;

                    var data = new ReferenceObjData(obj);
                    referenceObjects.Add(data);
                }
            }
        }

        private bool IsObjectValid(GameObject obj, ref string errorText)
        {
            // Non-Convex mesh collider
            var mesh = obj.GetComponent<MeshCollider>();
            if (mesh != null && !mesh.convex)
            {
                errorText = "Ignored object with non-convex mesh collider. Cannot simulate it.";
                return false;
            }

            // Rigidbodies inside the root of the object
            bool hasRb = obj.GetComponent<Rigidbody>() != null;
            int nRbs = obj.GetComponentsInChildren<Rigidbody>().Length;
            if ((hasRb && nRbs > 1) || (!hasRb && nRbs > 0))
            {
                errorText = "Object contains a rigidbody not in root object. Not supported and discouraged.";
                return false;
            }

            // Instanced Prefabs with added objects as overrides and more nested added overrides...
            if(Utils.IsPrefab(obj) && !PrefabUtility.IsPartOfPrefabAsset(obj))
            {
                var overrides = PrefabUtility.GetAddedGameObjects(obj);
                for(int i = 0; i < overrides.Count; i++)
                {
                    var ovr = overrides[i].instanceGameObject;
                    if(Utils.IsPrefab(ovr) && PrefabUtility.GetAddedGameObjects(ovr).Count > 0)
                    {
                        errorText = "Ignored prefab with several levels of add object overrides. Not supported.";
                        return false;
                    }
                }
            }

            return true;
        }

        internal void SelectObjects()
        {
            List<GameObject> objects = new List<GameObject>(referenceObjects.Count);
            foreach (var obj in referenceObjects)
            {
                if (obj != null && obj.ObjRef != null)
                    objects.Add(obj.ObjRef);
            }

            Selection.objects = objects.ToArray();
            objects.Clear();
        }
    }
}
