using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScatterTool
{
    /// <summary>
    /// Manages a list of objects that are simulated or waiting to be simulated.
    /// Also takes care of disabling rbs in the scene so that only these objects are simulated
    /// </summary>
    public class SimulationManager
    {        
        internal struct DisabledRBData
        {
            internal Rigidbody rb;
            internal bool wasOverriden; // We set isKinematic to true, but it could be false and overriden!

            internal DisabledRBData(Rigidbody _rb, bool _wasOverriden)
            {
                rb = _rb;
                wasOverriden = _wasOverriden;
            }
        }

        List<Rigidbody> allSceneRbs = new List<Rigidbody>();
        List<DisabledRBData> disabledRbs = new List<DisabledRBData>();
        List<ScatteredObject> managedObjs = new List<ScatteredObject>();

        internal bool IsHierarchyDirty { get; set; }
        internal bool KeepParent { get; set; }
        internal int NumManagedObjs { get { return managedObjs.Count; } }

        public bool Simulating { get => simulating; private set => simulating = value; }
        public float YExtents_ws { get => yExtents_ws; }

        bool simulating = false;
        float timeAccum = 0;
        int maxTimeSteps = 3;
        float yExtents_ws = 0;

        internal SimulationManager() {}

        internal void Initialize()
        {
            Dispose();
            IsHierarchyDirty = true;
        }

        // Call to clear before destruction
        internal void Dispose()
        {
            DeleteAll();
            allSceneRbs.Clear();
            disabledRbs.Clear();
        }

        internal bool StartSimulation()
        {
            if (Simulating)
                return false;

            UpdateManagedObjects();
            
            timeAccum = 0;
            GatherRigidbodies();
            DisableRigidbodies();
            Simulating = true;

            return true;
        }

        internal void UpdateManagedObjects()
        {
            var newList = new List<ScatteredObject>(managedObjs.Count);

            foreach (var obj in managedObjs)
            {
                if (obj.IsValid)
                {
                    obj.UpdateReferenceData();
                    newList.Add(obj);
                }
            }

            managedObjs.Clear();
            managedObjs = newList;
            UpdateMinimumObjectsHeight();
        }

        internal bool StopSimulation(bool consolidateSimulatingObjects)
        {
            if (!Simulating)
                return false;

            Simulating = false;
            if (consolidateSimulatingObjects)
            {
                ConsolidateAll();
                if (managedObjs.Count > 0)
                    Undo.SetCurrentGroupName("Consolidated " + managedObjs.Count + " dropped objects.");
            }
            DeleteAll();
            EnableDisabledRigidbodies();

            return true;
        }

        internal void UpdateMinimumObjectsHeight()
        {
            yExtents_ws = 0;
            foreach (var obj in managedObjs)
                yExtents_ws = Mathf.Max(yExtents_ws, obj.YExtents_ws);
        }

        internal void MoveObjects(Vector3 offset)
        {
            if (simulating)
                return;

            foreach (var obj in managedObjs)
                obj.Move(offset);
        }

        internal bool AddObject(ReferenceObjData refObj, Vector3 position, bool randomRotation)
        {
            var newObj = new ScatteredObject(ref refObj);

            if (newObj.IsValid)
                managedObjs.Add(newObj);
            else
            {
                newObj.Destroy();
                return false;
            }

            if (randomRotation)
                newObj.Rotate(Random.rotation);

            UpdateMinimumObjectsHeight();

            newObj.SetPosition(position);

            return true;
        }

        internal void ConsolidateAll()
        {
            foreach (var obj in managedObjs)
            {
                var finalObj = obj.Consolidate(KeepParent);
                if (finalObj == null)
                    continue;

                //Add the new rigidbodies by hand to the list. Otherwise the object to drop will be also considered
                // IMPORTANT We take for granted objects do not have any more rbs, because it is not supported
                var rb = finalObj.GetComponent<Rigidbody>();
                if (rb != null)
                    allSceneRbs.Add(rb);
            }
        }

        internal void DeleteAll()
        {
            foreach (var obj in managedObjs)
                obj.Destroy();

            managedObjs.Clear();

            yExtents_ws = 0;
        }

        internal bool AreAllAsleep()
        {
            foreach (var obj in managedObjs)
                if (!obj.IsSleeping)
                    return false;

            return true;
        }

        internal bool IsLastAsleep()
        {
            if (managedObjs.Count == 0)
                return true;

            return managedObjs[managedObjs.Count - 1].IsSleeping;
        }

        private void GatherRigidbodies()
        {
            if (!IsHierarchyDirty)
                return;

            // Disable all rbs from all managed objects (created and hidden in hierarchy)
            List<ScatteredObject> disabledList = new List<ScatteredObject>();
            foreach (var obj in managedObjs)
            {
                if (obj.DisableRigidbody())
                    disabledList.Add(obj);
            }

            // Get all enabled rigidbodies in the scene (our objects created will be ignored)
            allSceneRbs.Clear();
            allSceneRbs.AddRange(UnityEngine.Object.FindObjectsOfType<Rigidbody>(false));
            allSceneRbs.TrimExcess();

            foreach (var obj in disabledList)
                obj.EnableRigidbody();

            IsHierarchyDirty = false;
        }
       
        private void DisableRigidbodies()
        {
            disabledRbs.Clear();
            disabledRbs.Capacity = allSceneRbs.Count;
            foreach (var rb in allSceneRbs)
            {
                if (!rb.isKinematic)
                {
                    disabledRbs.Add(new DisabledRBData(rb, Utils.IsKinematicOverriden(rb)));
                    rb.isKinematic = true;
                }
            }
        }

        private void EnableDisabledRigidbodies()
        {
            foreach (var disabledRb in disabledRbs)
            {
                Rigidbody rb = disabledRb.rb;
                if (rb == null) // was deleted due to Undo while simulating
                    continue;

                if (PrefabUtility.IsPartOfAnyPrefab(rb))
                {
                    if (rb.isKinematic == false)
                        return;

                    rb.isKinematic = false;

                    // Revert override unless it was already overriden
                    if (!disabledRb.wasOverriden)
                        Utils.RevertIsKinematic(rb);
                }
                else
                    rb.isKinematic = false;
            }

            disabledRbs.Clear();
        }

        internal void SimulationStep()
        {
            timeAccum += Time.deltaTime;

            int counter = 0;
            while (timeAccum > Time.fixedDeltaTime && counter < maxTimeSteps)
            {
                counter++;
                timeAccum -= Time.fixedDeltaTime;

                Physics.autoSimulation = false;
                Physics.Simulate(Time.fixedDeltaTime);
                Physics.autoSimulation = true;
            }
        }

        internal bool IsPartOfAnyObject(Transform transform)
        {
            foreach (var obj in managedObjs)
            {
                if (obj.HasChild(transform))
                    return true;
            }
            return false;
        }

        internal void DrawBounds()
        {
            foreach (var obj in managedObjs)
                obj.DrawBounds();
        }
    }
}