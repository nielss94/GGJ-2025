using System;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace ScatterTool
{
    /// <summary>
    /// Creates the UI, manages the creation of the objects (where and when) and events happening in the Editor
    /// </summary>
    [EditorTool("ScatterTool")]
    public class ScatterTool : EditorTool
    {
        internal class HitPoint
        {
            internal Vector3 position;
            public HitPoint() { }
            internal HitPoint(Vector3 position) { this.position = position; }
        }

        internal enum CreationType
        {
            OneByOne,
            Area
        };

        internal enum CreationSpeed
        {
            OnlyOnce,
            AfterSlept,
            AfterDelay
        };

        SimulationManager simulationManager;
        ReferenceObjectsManager refObjsManager;

        // STATUS
        bool enabled = false;
        bool mouseDown = false;

        // SETTINGS
        CreationType creationType = CreationType.OneByOne;
        CreationSpeed creationSpeed = CreationSpeed.AfterSlept;
        float minimumDelay = 0.2f;
        float delay = 1;
        float areaSize = 3;
        float distance = 5;
        bool randomized = true;
        bool rotated = false;

        // LOGIC
        // We only initialized after we enter de Scene View. But this can happen after the move event, so we need this
        bool initialized = false;
        bool ignoreHierarchyChange = false;
        float creationTimeAccum = 0;
        HitPoint hitPoint;
        Vector3 spawnPointOffset;

#if UNITY_2021_2
        bool windowContentsDrawn = false;
        int bugTestCounter;
#endif
        
        // GUI
        Rect guiRect;
        Rect guiContentRect;
        string errorText;
        GUIStyle errorMsgStyle;
        [SerializeField] Texture2D icon;
        [SerializeField] Texture2D invertedIcon;
        GUIContent iconContent;
        GUIContent invertedIconContent;
        int controlID;
        int guiYOffset = 0;

        GameObject proxyObject;
        ReferenceObjData nextObjectToSpawn;

        private void OnEnable()
        {
            simulationManager = new SimulationManager();
            refObjsManager = new ReferenceObjectsManager();

            iconContent = new GUIContent()
            {
                image = icon,
                tooltip = "ScatterTool"
            };

            invertedIconContent = new GUIContent()
            {
                image = invertedIcon,
                tooltip = "ScatterTool"
            };

            // Fallback to built-in icon
            if (icon == null)
            {
                var cont = EditorGUIUtility.IconContent("Profiler.NetworkMessages");
                iconContent.image = cont.image;
            }

            if (invertedIcon == null)
            {
                var cont = EditorGUIUtility.IconContent("Profiler.NetworkMessages");
                invertedIconContent.image = cont.image;
            }

#if !UNITY_2021_1_OR_NEWER
            // Unity 2019, 2020 have the 0,0 pos over the top toolbox.
            guiYOffset = 23;
#endif
            guiRect = new Rect(5, 5 + guiYOffset, 155, 145);

            errorMsgStyle = new GUIStyle();
            errorMsgStyle.fontStyle = FontStyle.Bold;
            errorMsgStyle.normal.textColor = new Color(0.7f, 0.2f, 0.1f);
            errorMsgStyle.wordWrap = true;
        }

        private void OnDisable()
        {
            //NOTE: Disable is the only method called after compiling. Some objects stay dirty in the scene in this case!
            DisableTool();
        }

        public override GUIContent toolbarIcon { get { if (enabled) return invertedIconContent; else return iconContent; }}

        public override void OnActivated()
        {
            EnableTool();
        }

        public override void OnWillBeDeactivated()
        {
            DisableTool();
        }

        public void EnableTool()
        {
#if UNITY_2021_2
            bugTestCounter = 0;
#endif
            enabled = true;
            mouseDown = false;
            refObjsManager.UpdateFromSelection(targets, ref errorText);
            nextObjectToSpawn = refObjsManager.GetNextObject(randomized);
            UpdateProxyObject();

            if (creationType == CreationType.OneByOne)
                areaSize = 0;

            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            Selection.selectionChanged += OnSelectionChanged;
            SceneView.duringSceneGui += OnSceneGUI;
            Undo.undoRedoPerformed += UndoRedoPerformed;
        }

        public void DisableTool()
        {
            enabled = false;
            StopSimulation(consolidateSimulatingObjects: false);
            DestroyProxyObject();
            simulationManager.Dispose();
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            Selection.selectionChanged -= OnSelectionChanged;
            SceneView.duringSceneGui -= OnSceneGUI;
            Undo.undoRedoPerformed -= UndoRedoPerformed;
        }

        private void OnHierarchyChanged()
        {
            if (Application.isPlaying)
                return;

            if (ignoreHierarchyChange)
            {
                ignoreHierarchyChange = false;
                return;
            }

            simulationManager.IsHierarchyDirty = true;
        }

        private void UndoRedoPerformed()
        {
            if (Application.isPlaying)
                return;

            simulationManager.IsHierarchyDirty = true;
            bool wasSimulating = simulationManager.Simulating;

            StopSimulation(consolidateSimulatingObjects: false);

            // If performed during simulation selection can be lost. Restore it silently
            // If not simulating it will trigger a selection change needed to update the object to drop
            if (wasSimulating)
                Selection.selectionChanged -= OnSelectionChanged;
                        
            refObjsManager.SelectObjects();
            
            if (wasSimulating)
                Selection.selectionChanged += OnSelectionChanged;

            simulationManager.UpdateManagedObjects();
            UpdateProxyObject();

            SceneView.RepaintAll();
        }

        private void OnSelectionChanged()
        {
            if (Application.isPlaying || simulationManager.Simulating)
                return;

            errorText = "";
            refObjsManager.UpdateFromSelection(targets, ref errorText);
            nextObjectToSpawn = refObjsManager.GetNextObject(randomized);

            if (!refObjsManager.HasReferenceObjects)
            {
                hitPoint = null;
                simulationManager.DeleteAll();
            }

            // If a change of selection happens while in the Scene view, if we have floating objects. Update them!
            if (simulationManager.NumManagedObjs > 0)
                simulationManager.DeleteAll();

            UpdateProxyObject();
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (!(window is SceneView) || !enabled)
                return;

            controlID = GUIUtility.GetControlID(this.GetHashCode(), FocusType.Passive);

            //NOTE: We override the size by hand to avoid size flickering by hiding controls
            guiRect.height = guiContentRect.height + 30;

            // Clamp to sceneView
            guiRect.x = Mathf.Clamp(guiRect.x, 5, Screen.width - guiRect.width - 5);
            guiRect.y = Mathf.Clamp(guiRect.y, 5 + guiYOffset, Screen.height - guiYOffset - guiRect.height - 5);

            var newGuiRect = GUILayout.Window(controlID, guiRect, DrawGUI, "= Scatter Tool =");

            // Allow dragging
            guiRect.x = newGuiRect.x;
            guiRect.y = newGuiRect.y;

#if UNITY_2021_2
            if (!windowContentsDrawn)
            {
                //Sometimes this happens once or twice before starting to draw the GUI. But if keeps happening, its a bug.
                if (++bugTestCounter > 3)
                {
                    Debug.LogWarning("Scatter Tool window cannot be drawn. There is a bug between 2021.2.1f and 2021.2.6f. Please update to a newer version or remove the tool folder.");
                    DisableTool();
                    return;
                }
            }
#endif
        }

        private void DrawGUI(int windowID)
        {
#if UNITY_2021_2
            windowContentsDrawn = true;
#endif

            if (Application.isPlaying)
                return;

            EditorGUIUtility.labelWidth = 60;
            EditorGUIUtility.fieldWidth = 80;
            int checkboxLabelSize = (int)(EditorGUIUtility.labelWidth + EditorGUIUtility.fieldWidth - 16f);

            if (simulationManager.Simulating)
                GUI.enabled = false;

#if UNITY_2021_2
            Rect newContentRect;            
            try
            {
                newContentRect = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(false));
            }
            catch (ArgumentException)
            {
                Debug.LogError("Scatter Tool received a GUI exception due to the Unity bug between 2021.2.1f and 2021.2.6f. Please upgrade or remove the tool folder. Disabling tool.");
                DisableTool();
                return;
            }
#else
            Rect newContentRect = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(false));
#endif

            distance = EditorGUILayout.FloatField("Height", distance, GUILayout.ExpandWidth(false));
            if (distance < 0.1f) distance = 0.1f;

            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUILayout.Popup("Creation", (int)creationType, Enum.GetNames(typeof(CreationType)), GUILayout.ExpandWidth(false));
            if (EditorGUI.EndChangeCheck())
            {
                Event.current.Use();
                creationType = (CreationType)newIndex;
                areaSize = creationType == CreationType.OneByOne ? 0 : areaSize;
            }

            if (creationType == CreationType.Area)
            {
                EditorGUI.BeginChangeCheck();
                areaSize = EditorGUILayout.FloatField("Area Size", areaSize, GUILayout.ExpandWidth(false));
                if (EditorGUI.EndChangeCheck())
                {
                    if (areaSize < 0f) areaSize = 0f;

                    if (creationType == CreationType.Area)
                        areaSize = Mathf.Max(0.5f, areaSize);

                    Event.current.Use();
                }
            }

            EditorGUI.BeginChangeCheck();
            newIndex = EditorGUILayout.Popup("Spawn", (int)creationSpeed, Enum.GetNames(typeof(CreationSpeed)), GUILayout.ExpandWidth(false));
            if (EditorGUI.EndChangeCheck())
            {
                Event.current.Use();
                creationSpeed = (CreationSpeed)newIndex;
            }

            if (creationSpeed == CreationSpeed.AfterDelay)
            {
                delay = EditorGUILayout.FloatField("Delay", delay, GUILayout.ExpandWidth(false));
                if (delay < minimumDelay) delay = minimumDelay;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Rotated", GUILayout.MaxWidth(checkboxLabelSize), GUILayout.ExpandWidth(false));
            EditorGUI.BeginChangeCheck();
            rotated = GUILayout.Toggle(rotated, "", GUILayout.ExpandWidth(false));
            if (EditorGUI.EndChangeCheck())
                Event.current.Use();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Keep Parent", GUILayout.MaxWidth(checkboxLabelSize), GUILayout.ExpandWidth(false));
            EditorGUI.BeginChangeCheck();
            simulationManager.KeepParent = GUILayout.Toggle(simulationManager.KeepParent, "", GUILayout.ExpandWidth(false));
            if (EditorGUI.EndChangeCheck())
                Event.current.Use();
            EditorGUILayout.EndHorizontal();

            if (refObjsManager.NumReferenceObjects > 1)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Random Order", GUILayout.MaxWidth(checkboxLabelSize), GUILayout.ExpandWidth(false));
                EditorGUI.BeginChangeCheck();
                randomized = GUILayout.Toggle(randomized, "", GUILayout.ExpandWidth(false));
                if (EditorGUI.EndChangeCheck())
                    Event.current.Use();
                EditorGUILayout.EndHorizontal();
            }

            if (simulationManager.Simulating)
                GUI.enabled = true;

            GUILayout.Space(3);

            var colorBackup = GUI.backgroundColor;
            GUI.backgroundColor = simulationManager.Simulating? Color.red : Color.gray;
            string buttonText = refObjsManager.NumReferenceObjects == 0? "No objects selected" : (simulationManager.Simulating ? "Stop Simulation" : "Not Simulating");

            if (GUILayout.Button(buttonText, GUILayout.MinWidth(140), GUILayout.ExpandWidth(false)))
            {
                if (simulationManager.Simulating)
                {
                    Event.current.Use();
                    StopSimulation(consolidateSimulatingObjects: true);
                }
            }
            GUI.backgroundColor = colorBackup;

            if (errorText != "")
            {
                colorBackup = GUI.contentColor;
                GUILayout.Label(errorText, errorMsgStyle, GUILayout.MaxWidth(guiRect.width - 10), GUILayout.ExpandWidth(false));
                GUI.contentColor = colorBackup;
            }

            EditorGUILayout.EndHorizontal();

            // Get the new size
            if (newContentRect.width != 0 && newContentRect.height != 0 && newContentRect.height > 50)
                guiContentRect = newContentRect;

            // Make the title of the window draggable
            GUI.DragWindow(new Rect(0,0, 500, 20));
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (Application.isPlaying || Event.current.type == EventType.Used)
                return;

            bool insideGUI = guiRect.Contains(Event.current.mousePosition);

            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    if (Event.current.modifiers == EventModifiers.None && Event.current.button == 0)
                    {
                        GUIUtility.hotControl = controlID;
                        if (!insideGUI)
                        {
                            StartSimulation();
                            UpdateProxyObject();
                            SpawnNewSimulatedObject();
                            mouseDown = true;
                        }
                        Event.current.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (Event.current.button == 0)
                    {
                        GUIUtility.hotControl = 0;
                        if (!insideGUI)
                            mouseDown = false;
                        Event.current.Use();
                    }
                    break;

                case EventType.MouseMove:
                case EventType.MouseDrag:
                    // Move events can happen before calling MouseEnter
                    if (!initialized)
                        return;

                    OnMouseMovementInSceneView(sceneView, Event.current.mousePosition);
                    break;

                case EventType.MouseEnterWindow:
                    initialized = true;
                    simulationManager.Initialize();
                    OnMouseMovementInSceneView(sceneView, Event.current.mousePosition);
                    break;
                    
                case EventType.MouseLeaveWindow:
                    StopSimulation(consolidateSimulatingObjects: true);
                    simulationManager.DeleteAll(); // If not simulating, no object is deleted
                    initialized = false;
                    hitPoint = null;
                    errorText = "";
                    UpdateProxyObject();
                    sceneView.Repaint();
                    break;
                    
                default:
                    break;
            }

            DrawHandles();
        }

        private void DrawHandles()
        {
            if (hitPoint == null)
                return;

            using (new Handles.DrawingScope(Color.green))
            {
                if (creationType == CreationType.Area)
                {
                    Handles.CircleHandleCap(
                        controlID, hitPoint.position + spawnPointOffset,
                        Quaternion.LookRotation(Vector3.down, Vector3.right),
                        areaSize, EventType.Repaint);
                }

                Handles.ArrowHandleCap(
                    controlID, hitPoint.position + spawnPointOffset + Vector3.down * 1.5f,
                    Quaternion.LookRotation(Vector3.down, Vector3.right),
                    1f, EventType.Repaint);

                Handles.CircleHandleCap(
                    controlID, hitPoint.position,
                    Quaternion.LookRotation(Vector3.down, Vector3.right),
                    1f, EventType.Repaint);
            }
        }

        private void OnMouseMovementInSceneView(SceneView sceneView, Vector3 mousePos)
        {
            sceneView.Repaint();

            var hitPointChanged = UpdateHitPoint(mousePos);

            if (simulationManager.Simulating)
                return;

            if (hitPointChanged && hitPoint == null)
                simulationManager.DeleteAll();

            UpdateProxyObject();
        }

        /// <returns>True if its existence status was changed</returns>
        private bool UpdateHitPoint(Vector2 mousePos)
        {
            var hitPointBackup = hitPoint;

            // When undoing and focus gets out of the window
            if (Camera.current == null || guiRect.Contains(mousePos) || !refObjsManager.HasReferenceObjects)
                hitPoint = null;
            else
            {
                mousePos.y = Camera.current.pixelHeight - mousePos.y;
                Vector3 oldPos = hitPoint == null ? Vector3.zero : hitPoint.position;
                var ray = Camera.current.ScreenPointToRay(mousePos);
                int rayCount = 6;
                
                hitPoint = null;

                while (rayCount > 0)
                {
                    if (!Physics.Raycast(ray, out RaycastHit hitInfo))
                        break;

                    // Check only floating objects. Ignore not visible objects (area colliders, and such)
                    if (!simulationManager.Simulating && simulationManager.IsPartOfAnyObject(hitInfo.transform) || hitInfo.collider.isTrigger)
                    {
                        rayCount--;
                        ray = new Ray(hitInfo.point + ray.direction * 0.05f, ray.direction);
                        continue;
                    }
                    else
                    {
                        hitPoint = new HitPoint(ray.origin + ray.direction * hitInfo.distance);
                        UpdateSpawnPointOffset();

                        // If it was previously null, no object is moved here
                        simulationManager.MoveObjects(hitPoint.position - oldPos);
                        break;
                    }
                }
            }

            return hitPoint == null ^ hitPointBackup == null;
        }

        private void UpdateSpawnPointOffset()
        {
            spawnPointOffset = Vector3.up * Mathf.Max(distance, simulationManager.YExtents_ws);
        }

        private void UpdateProxyObject()
        {
            if(hitPoint == null || simulationManager.Simulating || nextObjectToSpawn == null)
            {
                DestroyProxyObject();
                return;
            }

            if(proxyObject != null)
            {
                proxyObject.transform.position = hitPoint.position + spawnPointOffset;
            }
            else
            {
                if(CreateProxyObject())
                    proxyObject.transform.position = GetNewSpawnPointPos();
            }
        }

        private bool CreateProxyObject()
        {
            if(nextObjectToSpawn == null || nextObjectToSpawn.ObjRef == null)
                return false;

            // Add one material and as many rendererers and filters are there are in this object.
            var fakeShader = Shader.Find("Unlit/ScatterToolFakeObject");
            if(fakeShader == null)
            {
                Debug.LogError("ScatterToolFakeObject shader is not found in the Scatter Tool folder. Please, reinstall the asset.");
                proxyObject = null;
                return false;
            }

            nextObjectToSpawn.UpdateData();

            // OnHierarchyChanged will be called because the object is created right before changing the hideFlags!
            ignoreHierarchyChange = true;

            var refObjTr = nextObjectToSpawn.ObjRef.transform;
            var mat_l2w = refObjTr.localToWorldMatrix;
            var mat_w2l = refObjTr.worldToLocalMatrix;

            proxyObject = new GameObject("Proxy Object");
            proxyObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
            var proxyTr = proxyObject.transform;
            proxyTr.rotation = mat_l2w.rotation;
            proxyTr.localScale = mat_l2w.lossyScale;
            //Position is applied afterwards

            var material = new Material(fakeShader);

            var filters = nextObjectToSpawn.ObjRef.GetComponentsInChildren<MeshFilter>(includeInactive: false);
            for(int i = 0; i < filters.Length; i++)
            {
                var child = new GameObject();
                child.AddComponent<MeshRenderer>().sharedMaterial = material;
                child.AddComponent<MeshFilter>().sharedMesh = filters[i].sharedMesh;
                var childTr = child.transform;
                childTr.parent = proxyTr;
                
                // Calculate matrix from the parent object to the mesh filter transform to place the mesh properly relative to the parent
                var filterObjectToParentMat = mat_w2l * filters[i].transform.localToWorldMatrix;
                childTr.localPosition = filterObjectToParentMat.GetColumn(3);
                childTr.localRotation = filterObjectToParentMat.rotation;
                childTr.localScale = filterObjectToParentMat.lossyScale;
            }

            return true;
        }

        private void DestroyProxyObject()
        {
            if(proxyObject != null)
            {
                DestroyImmediate(proxyObject);
                proxyObject = null;
            }
        }

        private void SpawnNewSimulatedObject()
        {
            if (hitPoint == null || nextObjectToSpawn == null)
                return;

            // OnHierarchyChanged will be called because the object is created right before changing the hideFlags!
            ignoreHierarchyChange = true;
            if(simulationManager.AddObject(nextObjectToSpawn, GetNewSpawnPointPos(), rotated))
            {
                nextObjectToSpawn = refObjsManager.GetNextObject(randomized);
                DestroyProxyObject();
                UpdateProxyObject();
            }
        }

        private Vector3 GetNewSpawnPointPos()
        {
            if(hitPoint == null)
                return Vector3.zero;

            // NOTE: Area size can be 0 when only one object is Scattered
            Vector3 offsetPos = Vector3.zero;
            if(creationType == CreationType.Area)
            {
                offsetPos = UnityEngine.Random.insideUnitCircle * areaSize;
                offsetPos.z = offsetPos.y;
                offsetPos.y = 0;
            }

            UpdateSpawnPointOffset();
            return hitPoint.position + spawnPointOffset + offsetPos;
        }

        private void StartSimulation()
        {
            if (simulationManager.Simulating)
                return;

            if (simulationManager.StartSimulation())
            {
                Undo.IncrementCurrentGroup();
                Undo.SetCurrentGroupName("Scatter Object unfinished simulation");
                creationTimeAccum = 0;
                EditorApplication.update += OnEditorUpdate;
            }
        }

        private void StopSimulation(bool consolidateSimulatingObjects)
        {
            if (simulationManager.StopSimulation(consolidateSimulatingObjects))
                EditorApplication.update -= OnEditorUpdate;

            // If the user does not move the mouse, at least we need to repaint the GUI with the changes
            SceneView.RepaintAll();
            UpdateProxyObject();
        }

        private void OnEditorUpdate()
        {
            simulationManager.SimulationStep();
            
            // WARNING: Somehow the selection is not working after duplicating a prefab if mouse button is the hot control
            if (GUIUtility.hotControl == controlID)
                GUIUtility.hotControl = 0;

            if (!mouseDown)
            {
                if (simulationManager.AreAllAsleep())
                    StopSimulation(consolidateSimulatingObjects: true);
                return;
            }

            if (hitPoint == null)
                return;

            creationTimeAccum += Time.deltaTime;
            float clampedDelay = Mathf.Max(Time.deltaTime, delay);

            // Create new objects
            switch (creationSpeed)
            {
                case CreationSpeed.AfterSlept:
                    { 
                        if (creationTimeAccum > minimumDelay && simulationManager.IsLastAsleep())
                        {
                            creationTimeAccum = 0;
                            SpawnNewSimulatedObject();
                        }
                        break;
                    }
                case CreationSpeed.AfterDelay:
                    {
                        while (creationTimeAccum > clampedDelay)
                        {
                            creationTimeAccum = 0;
                            SpawnNewSimulatedObject();
                        }
                        break;
                    }
            }
        }
    }
}