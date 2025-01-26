using StarterAssets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] private Slider masterVolume;
    [SerializeField] private Slider musicVolume;
    [SerializeField] private Slider sfxVolume;
    [SerializeField] private Slider dialogueVolume;
    [SerializeField] private Slider mouseSensitivity;
    [SerializeField] private Toggle tpRotateHorizontal;
    [SerializeField] private Toggle tpRotateVertical;

    private void Start() {
        masterVolume.value = AudioManager.Instance.MasterVolume;
        musicVolume.value = AudioManager.Instance.MusicVolume;
        sfxVolume.value = AudioManager.Instance.SfxVolume;
        dialogueVolume.value = AudioManager.Instance.DialogueVolume;
        mouseSensitivity.value = PlayerPrefs.GetFloat("mouseSensitivity", 2);
        tpRotateHorizontal.isOn = PlayerPrefs.GetInt("tpRotateHorizontal", 1) == 1;
        tpRotateVertical.isOn = PlayerPrefs.GetInt("tpRotateVertical", 0) == 1;
    }

    private void OnEnable() {
        EventSystem.current.SetSelectedGameObject(masterVolume.gameObject);
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

    public void MouseSensitivityChanged(float value) {
        PlayerPrefs.SetFloat("mouseSensitivity", value);

        if (FindFirstObjectByType<FirstPersonController>() != null) {
            FindFirstObjectByType<FirstPersonController>().RotationSpeed = value;
        }
    }

    public void TPRotateHorizontalChanged() {
        PlayerPrefs.SetInt("tpRotateHorizontal", tpRotateHorizontal.isOn ? 1 : 0);
    }

    public void TPRotateVerticalChanged() {
        PlayerPrefs.SetInt("tpRotateVertical", tpRotateVertical.isOn ? 1 : 0);
    }
}
