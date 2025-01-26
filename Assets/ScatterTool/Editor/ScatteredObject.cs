using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScatterTool
{
    /// <summary>
    /// An object that is managed and simulated and keeps track of the reference object from which it was created from
    /// </summary>
    public class ScatteredObject
    {
        ReferenceObjData refData;

        //associated hidden obj data which is a copy of the reference one
        GameObject obj;
        Rigidbody rb;
        float yExtents_ws;

        internal bool IsValid { get => obj != null; }
        internal bool IsSleeping { get => IsValid && rb.IsSleeping(); }
        public float YExtents_ws { get => yExtents_ws; }

        internal ScatteredObject(ref ReferenceObjData reference)
        {
            refData = reference;
            obj = InstantiateReferenceObject();
            if (!IsValid)
                return;

            obj.name = string.Format("Spawned Object ({0})", refData.ObjRef.name);
            obj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
            obj.transform.position = Vector3.zero;
            UpdateData();
        }

        private void UpdateData()
        {
            if (!IsValid)
                return;

            rb = obj.GetComponent<Rigidbody>();
            if (rb == null)
                rb = Undo.AddComponent(obj, typeof(Rigidbody)) as Rigidbody;

            // Avoid overrides by not changing to the same kinematic value, even though we later on reset it
            if (rb.isKinematic)
                rb.isKinematic = false;

            UpdateHeight();
        }

        /// <summary>
        /// Y size to calculate a minimum height to drop objects
        /// </summary>
        private void UpdateHeight()
        {
            yExtents_ws = 0;
            var bounds = Utils.GetBoundsFromObject(obj);
            if (bounds.size.y == 0)
                return;

            yExtents_ws = Mathf.Max(0, bounds.extents.y);
        }

        private GameObject InstantiateReferenceObject()
        {
            GameObject objCopy;
            if (Utils.IsPrefab(refData.ObjRef))
            {
                if (PrefabUtility.IsPartOfPrefabAsset(refData.ObjRef))
                    objCopy = PrefabUtility.InstantiatePrefab(refData.ObjRef, Utils.GetObjScene(refData.ObjRef)) as GameObject;
                else
                {
                    objCopy = Utils.DuplicatePrefabInstance(refData.ObjRef);
                    if (objCopy != null && objCopy.transform.parent != null)
                        Undo.SetTransformParent(objCopy.transform, null, Undo.GetCurrentGroupName());
                }
            }
            else
            {
                objCopy = Object.Instantiate(refData.ObjRef);
                SceneManager.MoveGameObjectToScene(objCopy, Utils.GetObjScene(refData.ObjRef));
            }
            return objCopy;
        }

        internal void UpdateReferenceData()
        {
            refData.UpdateData();
        }

        internal bool HasChild(Transform transform)
        {
            return IsValid && transform.IsChildOf(obj.transform);
        }

        /// <summary>
        /// Convert this object into a final object in the scene with Undo-Redo enabled
        /// The original object will not be removed unless we do it manually afterwards.
        /// </summary>
        internal GameObject Consolidate(bool keepParent)
        {            
            GameObject finalObj = InstantiateReferenceObject();
            if (finalObj == null)
                return null;

            Undo.RegisterCreatedObjectUndo(finalObj, "Consolidate scattered object");
            
            // Apply changes to the serialized object
            SerializedObject serializedObj = new SerializedObject(finalObj.transform);
            serializedObj.FindProperty("m_LocalPosition").vector3Value = obj.transform.position;
            serializedObj.FindProperty("m_LocalRotation").quaternionValue = obj.transform.rotation;
            serializedObj.ApplyModifiedProperties();

            // If there was a rb, enable the kinematic status, removing the possible override
            if (refData.rb != null && !refData.wasKinematic)
            {
                var newRb = finalObj.GetComponent<Rigidbody>();

                SerializedObject serializedRb = new SerializedObject(newRb);
                SerializedProperty isKinematicProp = serializedRb.FindProperty("m_IsKinematic");
                isKinematicProp.boolValue = false;
                serializedRb.ApplyModifiedProperties();

                if (!refData.wasKinematicOverriden && Utils.IsPrefab(refData.ObjRef))
                    PrefabUtility.RevertPropertyOverride(isKinematicProp, InteractionMode.AutomatedAction);
            }

            if (keepParent)
                Undo.SetTransformParent(finalObj.transform, refData.ObjRef.transform.parent, "ScatterTool: Parent object to the orignal parent");

            return finalObj;
        }

        internal void SetPosition(Vector3 position)
        {
            // WARNING: Move event can happen while deleting the objects!
            if (IsValid)
                obj.transform.position = position;
        }

        internal void Rotate(Quaternion offset)
        {
            // WARNING: Move event can happen while deleting the objects!
            if (IsValid)
            {
                obj.transform.rotation *= offset;
                //Note: Bounds are not updated after the rotation. But in the future, if more objects are created at the same time, do this outside
                Physics.SyncTransforms();
                UpdateHeight();
            }
        }

        internal void Move(Vector3 offset)
        {
            // WARNING: Move event can happen while deleting the objects!
            if (IsValid)
                obj.transform.Translate(offset, Space.World);
        }

        internal void Destroy()
        {
            //Since it is hidden, no hierarchy event is emitted
            if (IsValid)
                Object.DestroyImmediate(obj);
        }

        /// <returns>True if it had a non disabled rb</returns>
        internal bool DisableRigidbody()
        {
            if (!IsValid)
                return false;

            // If the objects takes time to delete, this can result in exceptions
            if (rb == null)
                UpdateData();

            //Objects with rigidbody in the main object are the only ones allowed!
            if (rb != null && rb.gameObject.activeSelf)
            {
                rb.gameObject.SetActive(false);
                return true;
            }

            return false;
        }

        /// <summary> Warning: Call this only if this rb has been disabled before </summary>
        internal void EnableRigidbody()
        {
            if (IsValid)
                rb.gameObject.SetActive(true);
        }

        internal void DrawBounds()
        {
            var bounds = Utils.GetBoundsFromObject(obj);
            if (bounds.size.y == 0)
                return;
            Utils.DrawBounds(bounds);
        }
    }
}
