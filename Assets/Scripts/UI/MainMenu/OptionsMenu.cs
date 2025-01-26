using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] private Slider masterVolume;
    [SerializeField] private Slider musicVolume;
    [SerializeField] private Slider sfxVolume;
    [SerializeField] private Slider dialogueVolume;
    [SerializeField] private Toggle tpRotateHorizontal;
    [SerializeField] private Toggle tpRotateVertical;

    private void Start() {
        masterVolume.value = AudioManager.Instance.MasterVolume;
        musicVolume.value = AudioManager.Instance.MusicVolume;
        sfxVolume.value = AudioManager.Instance.SfxVolume;
        dialogueVolume.value = AudioManager.Instance.DialogueVolume;

        tpRotateHorizontal.isOn = PlayerPrefs.GetInt("tpRotateHorizontal", 1) == 1;
        tpRotateVertical.isOn = PlayerPrefs.GetInt("tpRotateVertical", 1) == 1;
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

    public void TPRotateHorizontalChanged(bool value) {
        PlayerPrefs.SetInt("tpRotateHorizontal", value ? 1 : 0);
    }

    public void TPRotateVerticalChanged(bool value) {
        PlayerPrefs.SetInt("tpRotateVertical", value ? 1 : 0);
    }
}
