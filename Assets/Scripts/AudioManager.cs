using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public float MasterVolume { get; private set; } = 1;
    public float MusicVolume { get; private set; } = 1;
    public float SfxVolume { get; private set; } = 1;
    public float DialogueVolume { get; private set; } = 1;
    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

    }

    private void Start() {
        MasterVolume = PlayerPrefs.GetFloat("masterVolume", 0.7f);
        MusicVolume = PlayerPrefs.GetFloat("musicVolume", 0.7f);
        SfxVolume = PlayerPrefs.GetFloat("sfxVolume", 0.7f);
        DialogueVolume = PlayerPrefs.GetFloat("dialogueVolume", 0.7f);
    }

    public void SetMasterVolume(float value) {
        MasterVolume = value;
        PlayerPrefs.SetFloat("masterVolume", MasterVolume);
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("MasterVolume", MasterVolume);
    }

    public void SetMusicVolume(float value) {
        MusicVolume = value;
        PlayerPrefs.SetFloat("musicVolume", MusicVolume);
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("AmbianceVolume", MusicVolume);
    }

    public void SetSfxVolume(float value) {
        SfxVolume = value;
        PlayerPrefs.SetFloat("sfxVolume", SfxVolume);
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("SoundFXVolume", SfxVolume);
    }

    public void SetDialogueVolume(float value) {
        DialogueVolume = value;
        PlayerPrefs.SetFloat("dialogueVolume", DialogueVolume);
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("DialogueVolume", DialogueVolume);
    }
}
