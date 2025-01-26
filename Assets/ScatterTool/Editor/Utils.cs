using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScatterTool
{
    internal static class Utils
    {
        internal static bool IsPrefab(GameObject prefab)
        {
            // There is an occasional Null exception when this call doesn't work.
            try
            {
                var type = PrefabUtility.GetPrefabAssetType(prefab);
                return type == PrefabAssetType.Regular || type == PrefabAssetType.Variant;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Scatter Tool - Probable rare null exception when testing if game object is a prefab when Undoing. Please, delete the dropped objects by hand. Erro: " + e);
                return false;
            }
        }

        internal static Scene GetObjScene(GameObject obj)
        {
            return EditorUtility.IsPersistent(obj) ? SceneManager.GetActiveScene() : obj.scene;
        }

        /// <summary>
        /// The only way of copying a prefab and all of the overrides (components and objects included)
        /// is by using a shady method in the Unsupported library. Hopefully this will change eventually to a more standard method
        /// </summary>
        internal static GameObject DuplicatePrefabInstance(GameObject objectToDuplicate)
        {
            var previousSelection = Selection.objects;
            Selection.objects = null; // Necessary to avoid problems with copy and selected objs
            Selection.activeObject = objectToDuplicate; // Just one object selected
            var nameBackup = Undo.GetCurrentGroupName(); // To avoid confusion, restore the undo group name that is changed to Paste X.
            Unsupported.DuplicateGameObjectsUsingPasteboard();
            Undo.SetCurrentGroupName(nameBackup);
            var duplicate = Selection.activeObject as GameObject;

            // In some Unity versions after duplicating the prefab it is not selected!.
            // It happens specifically when the object contains added objects overrides
            if(duplicate.GetInstanceID() == objectToDuplicate.GetInstanceID())
            {
                // Remove numbers at the end of the object name
                var baseName = GetBaseName(objectToDuplicate.name);

                // The duplicated prefab will not appear in the scene root objects for some reason
                // Sadly we have to gather all objects
                var newRootObjs = Resources.FindObjectsOfTypeAll<GameObject>();

                // Sometimes the first object in the list is the duplicated base.
                duplicate = newRootObjs[0]; 
                if(duplicate == null || !duplicate.name.StartsWith(baseName))
                {
                    // In some unexpected occasions, the first objects are the overrides duplicated, then the root object at the end.
                    var nAddedObjOverrides = PrefabUtility.GetAddedGameObjects(objectToDuplicate).Count;
                    duplicate = newRootObjs[nAddedObjOverrides];

                    if(duplicate == null || !duplicate.name.StartsWith(baseName))
                    {
                        Debug.LogWarning("The duplication of Prefabs in the scene uses a method that is not valid for your prefab setup." +
                        " Please, avoid using too many nested prefab overrides, and if the problem persists, please notify the developer to get this fixed!");
                    }
                }
            }

            Selection.objects = previousSelection;
            return duplicate;
        }

        internal static string GetBaseName(string objectName)
        {
            // Check if object name ends on " (38)" which is the default unity adds to an object
            // If it ends with that, the duplicated object will grab that and increment the number
            Regex rx = new Regex(@"^(.+)(\s\()(\d+)(\))$");
            if(rx.IsMatch(objectName))
            {
                var splits = objectName.Split(' ');
                var lastPartLength = splits[splits.Length - 1].Length;
                return objectName.Substring(0, objectName.Length - lastPartLength);
            }
            return objectName;
        }

        internal static void RevertIsKinematic(Rigidbody rb)
        {
            SerializedObject sObject = new SerializedObject(rb);
            SerializedProperty sProperty = sObject.FindProperty("m_IsKinematic");

            if (sProperty == null)
                Debug.LogError("Cannot find and revert IsKinematic in the rigidbody on object " + rb.gameObject.name);
            else
                PrefabUtility.RevertPropertyOverride(sProperty, InteractionMode.AutomatedAction);
        }

        internal static bool IsKinematicOverriden(Rigidbody rb)
        {
            if (!PrefabUtility.IsPartOfPrefabInstance(rb))
                return false;

            foreach (var mod in PrefabUtility.GetPropertyModifications(rb))
            {
                if (mod.propertyPath == "m_IsKinematic")
                {
                    // Old dirty modifications can exist in the serialized data to non-existent components! Check reference
                    if (mod.target == null)
                        return false;

                    return true;
                }
            }

            return false;
        }

        internal static Bounds GetBoundsFromObject(GameObject go)
        {
            var colliders = go.GetComponentsInChildren<Collider>();

            if (colliders.Length == 0)
                return new Bounds(go.transform.position, Vector3.zero);

            Bounds fullBounds = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++)
            {
                fullBounds.Encapsulate(colliders[i].bounds.min);
                fullBounds.Encapsulate(colliders[i].bounds.max);
            }
            return fullBounds;
        }

        internal static void DrawBounds(Bounds b, float delay = 0)
        {
            // bottom
            var p1 = new Vector3(b.min.x, b.min.y, b.min.z);
            var p2 = new Vector3(b.max.x, b.min.y, b.min.z);
            var p3 = new Vector3(b.max.x, b.min.y, b.max.z);
            var p4 = new Vector3(b.min.x, b.min.y, b.max.z);

            Debug.DrawLine(p1, p2, Color.blue, delay);
            Debug.DrawLine(p2, p3, Color.red, delay);
            Debug.DrawLine(p3, p4, Color.yellow, delay);
            Debug.DrawLine(p4, p1, Color.magenta, delay);

            // top
            var p5 = new Vector3(b.min.x, b.max.y, b.min.z);
            var p6 = new Vector3(b.max.x, b.max.y, b.min.z);
            var p7 = new Vector3(b.max.x, b.max.y, b.max.z);
            var p8 = new Vector3(b.min.x, b.max.y, b.max.z);

            Debug.DrawLine(p5, p6, Color.blue, delay);
            Debug.DrawLine(p6, p7, Color.red, delay);
            Debug.DrawLine(p7, p8, Color.yellow, delay);
            Debug.DrawLine(p8, p5, Color.magenta, delay);

            // sides
            Debug.DrawLine(p1, p5, Color.white, delay);
            Debug.DrawLine(p2, p6, Color.gray, delay);
            Debug.DrawLine(p3, p7, Color.green, delay);
            Debug.DrawLine(p4, p8, Color.cyan, delay);
        }
    }
}
