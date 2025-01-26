using UnityEngine;
using TMPro;
public class ControlHintItem : MonoBehaviour
{
    public TextMeshProUGUI label;
    public TextMeshProUGUI keys;

    public void UpdateSpriteAsset(TMP_SpriteAsset spriteAsset, int[] spriteIds) {
        keys.spriteAsset = spriteAsset;
        string keysText = "";
        foreach (int id in spriteIds) {
            keysText += $"<sprite index={id}>";
        }
        keys.text = keysText;

        // Set width based on number of sprite IDs
        RectTransform keysRect = keys.GetComponent<RectTransform>();
        keysRect.sizeDelta = new Vector2(spriteIds.Length > 1 ? 125f : 40f, keysRect.sizeDelta.y);
    }
}
