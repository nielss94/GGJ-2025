using UnityEngine;
using UnityEngine.TextCore.Text;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;

public class ControlsHinter : MonoBehaviour
{
    [Header("Sprite Assets")]
    public TMP_SpriteAsset kbmAsset;
    public TMP_SpriteAsset xboxAsset;
    public TMP_SpriteAsset playstationAsset;

    [Space]
    [SerializeField] private GameObject container;

    [Header("Hint Items")]
    [SerializeField] private ControlHintItem moveHintItem;
    [SerializeField] private ControlHintItem shootHintItem;
    [SerializeField] private ControlHintItem teleportHintItem;
    [SerializeField] private ControlHintItem cancelHintItem;
    [SerializeField] private ControlHintItem interactHintItem;

    [Header("Visibility")]
    [SerializeField] private float showDuration = 5f;
    [SerializeField] private float idleThreshold = 5f; // Time before showing hints when idle
    
    private float idleTimer = 0f;
    private Vector3 lastPosition;
    private bool isShowing = false;
    private float currentShowTimer = 0f;
    private CharacterController characterController;

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
        xboxSpriteIds = new int[] { 17 },
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
        characterController = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterController>();
        UpdateControlsType(GetCurrentControlsType());
        HideHints(); // Ensure hints are hidden initially
    }

    private void OnEnable() {
        // Subscribe to input device changes
        InputSystem.onDeviceChange += OnInputDeviceChange;
    }

    private void OnDisable() {
        // Unsubscribe from input device changes
        InputSystem.onDeviceChange -= OnInputDeviceChange;
    }

    private void OnInputDeviceChange(InputDevice device, InputDeviceChange change) {
        if (change == InputDeviceChange.Added || change == InputDeviceChange.Removed) {
            UpdateControlsType(GetCurrentControlsType());
        }
    }

    private ControlsType GetCurrentControlsType() {
        Gamepad gamepad = Gamepad.current;
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (gamepad != null) {
            // Check if it's a PlayStation controller
            if (gamepad.name.ToLower().Contains("playstation") || 
                gamepad.name.ToLower().Contains("ps4") || 
                gamepad.name.ToLower().Contains("ps5")) {
                return ControlsType.PlayStation;
            }
            // Default to Xbox layout for other gamepads
            return ControlsType.Xbox;
        }

        if (keyboard != null || mouse != null) {
            return ControlsType.KeyboardMouse;
        }

        return ControlsType.KeyboardMouse; // Default to keyboard/mouse
    }

    private void UpdateControlsType(ControlsType controlsType) {
        TMP_SpriteAsset spriteAsset = controlsType switch {
            ControlsType.KeyboardMouse => kbmAsset,
            ControlsType.Xbox => xboxAsset,
            ControlsType.PlayStation => playstationAsset,
            _ => kbmAsset
        };

        // Update all hint items with the new sprite asset
        moveHintItem?.UpdateSpriteAsset(spriteAsset, GetSpritesForType(moveHint, controlsType));
        shootHintItem?.UpdateSpriteAsset(spriteAsset, GetSpritesForType(shootHint, controlsType));
        teleportHintItem?.UpdateSpriteAsset(spriteAsset, GetSpritesForType(teleportHint, controlsType));
        cancelHintItem?.UpdateSpriteAsset(spriteAsset, GetSpritesForType(cancelHint, controlsType));
        interactHintItem?.UpdateSpriteAsset(spriteAsset, GetSpritesForType(interactHint, controlsType));
    }

    private int[] GetSpritesForType(ControlsHint hint, ControlsType type) {
        return type switch {
            ControlsType.KeyboardMouse => hint.kbmSpriteIds,
            ControlsType.Xbox => hint.xboxSpriteIds,
            ControlsType.PlayStation => hint.playstationSpriteIds,
            _ => hint.kbmSpriteIds
        };
    }

    private void Start()
    {
        lastPosition = characterController.transform.position;
        ShowHints(); // Show hints at game start
    }

    private void Update()
    {
        // Check if player is moving
        Vector3 currentPosition = characterController.transform.position;
        if (currentPosition != lastPosition)
        {
            idleTimer = 0f;
            lastPosition = currentPosition;
        }
        else
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleThreshold && !isShowing)
            {
                ShowHints();
            }
        }

        // Handle auto-hide timer
        if (isShowing)
        {
            currentShowTimer += Time.deltaTime;
            if (currentShowTimer >= showDuration)
            {
                HideHints();
            }
        }
    }

    private void ShowHints()
    {
        container.SetActive(true);
        isShowing = true;
        currentShowTimer = 0f;
    }

    private void HideHints()
    {
        container.SetActive(false);
        isShowing = false;
    }
}

public enum ControlsType {
    KeyboardMouse,
    Xbox,
    PlayStation
}

public struct ControlsHint {
    public string label;
    public int[] kbmSpriteIds;
    public int[] xboxSpriteIds;
    public int[] playstationSpriteIds;
}