using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] private Slider masterVolume;
    [SerializeField] private Slider musicVolume;
    [SerializeField] private Slider sfxVolume;
    [SerializeField] private Slider dialogueVolume;

    private void Start() {
        masterVolume.value = AudioManager.Instance.MasterVolume;
        musicVolume.value = AudioManager.Instance.MusicVolume;
        sfxVolume.value = AudioManager.Instance.SfxVolume;
        dialogueVolume.value = AudioManager.Instance.DialogueVolume;
    }

    public void MasterVolumeChanged(float value) {
        AudioManager.Instance.SetMasterVolume(value);
    }

    public void MusicVolumeChanged(float value) {
        AudioManager.Instance.SetMusicVolume(value);
    }

    public void SfxVolumeChanged(float value) {
        AudioManager.Instance.SetSfxVolume(value);
    }

    public void DialogueVolumeChanged(float value) {
        AudioManager.Instance.SetDialogueVolume(value);
    }
}
