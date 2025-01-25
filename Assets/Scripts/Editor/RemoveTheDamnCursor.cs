using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class RemoveTheDamnCursor
{
    // Static constructor that gets called when unity fires up.
    static RemoveTheDamnCursor()
    {
        EditorApplication.playModeStateChanged += (PlayModeStateChange state) =>
        {
            // If we're about to run the scene...
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                var gameWindow = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
                gameWindow.Focus();
                gameWindow.SendEvent(new Event
                {
                    button = 0,
                    clickCount = 1,
                    type = EventType.MouseDown,
                    mousePosition = gameWindow.rootVisualElement.contentRect.center
                });

            }
        };
    }
}