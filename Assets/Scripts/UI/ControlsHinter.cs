using UnityEngine;
using UnityEngine.TextCore.Text;
using System.Collections.Generic;
using TMPro;

public class ControlsHinter : MonoBehaviour
{
    [Header("Sprite Assets")]
    public TMP_SpriteAsset kbmAsset;
    public TMP_SpriteAsset xboxAsset;
    public TMP_SpriteAsset playstationAsset;

    public GameObject hintItemPrefab;

    private List<ControlHintItem> hintItems = new List<ControlHintItem>();

    private ControlsHint moveHint = new ControlsHint {
        label = "Move",
        kbmSpriteIds = new int[] { 227, 21, 195, 90 },
        xboxSpriteIds = new int[] { 78 },
        playstationSpriteIds = new int[] { 72 }
    };

    private ControlsHint shootHint = new ControlsHint {
        label = "Shoot",
        kbmSpriteIds = new int[] { 15 },
        xboxSpriteIds = new int[] { 76 },
        playstationSpriteIds = new int[] { 21 }
    };

    private ControlsHint interactHint = new ControlsHint {
        label = "Interact",
        kbmSpriteIds = new int[] { 94 },
        xboxSpriteIds = new int[] { 42 },
        playstationSpriteIds = new int[] { 21 }
    };

    private ControlsHint teleportHint = new ControlsHint {
        label = "Teleport",
        kbmSpriteIds = new int[] { 79 },
        xboxSpriteIds = new int[] { 13 },
        playstationSpriteIds = new int[] { 40 }
    };

    private ControlsHint cancelHint = new ControlsHint {
        label = "Cancel",
        kbmSpriteIds = new int[] { 107 },
        xboxSpriteIds = new int[] { 15 },
        playstationSpriteIds = new int[] { 38 }
    };

    private void Awake() {
        // Subscribe to pause menu event to hide hints

        
    }

    // Hint show after idle time
    // Switch icons based on keyboard/controller
    
    // WASD movement
    // Shoot
    // Teleport
    // Cancel
    // Interact
}

public struct ControlsHint {
    public string label;
    public int[] kbmSpriteIds;
    public int[] xboxSpriteIds;
    public int[] playstationSpriteIds;
}