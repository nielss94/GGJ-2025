using TMPro;
using UnityEngine;

public class StatsPanel : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI eggVelocityText;

    [SerializeField]
    private TextMeshProUGUI rebirthCountText;

    private int rebirthCount = 0;

    void Awake()
    {
        Events.OnEggSpeed += SetEggVelocity;
        Events.OnRebirth += Rebirth;
    }

    void Start()
    {
        rebirthCount = 0;
        rebirthCountText.text = $"Rebirths: {rebirthCount}";
        eggVelocityText.text = $"0.00 m/s";
    }

    public void SetEggVelocity(float velocity)
    {
        eggVelocityText.text = $"{velocity:F2} m/s";
    }

    private void Rebirth()
    {
        rebirthCount++;
        rebirthCountText.text = $"Rebirths: {rebirthCount}";
    }
}
