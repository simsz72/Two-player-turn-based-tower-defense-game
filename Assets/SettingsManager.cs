using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider soundSlider;

    public LobbyManager lobbyManager;

    void Start()
    {
        lobbyManager = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();
        LoadSettings();
    }

    private void SetVolume(float volume)
    {
        audioMixer.SetFloat("volume", volume);
        SaveVolume();
    }

    private void LoadSettings()
    {
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        audioMixer.SetFloat("volume", savedVolume);
        soundSlider.value = savedVolume;

        string savedNickname = PlayerPrefs.GetString("PlayerName");
        Debug.Log(savedNickname);
        lobbyManager.nicknameInput.text = savedNickname;
    }

    private void SaveVolume()
    {
        float currentVolume = 0.0f;
        audioMixer.GetFloat("volume", out currentVolume);
        PlayerPrefs.SetFloat("MasterVolume", currentVolume);
        PlayerPrefs.Save();
    }

    public void SavePlayerName()
    {
        PlayerPrefs.SetString("PlayerName", lobbyManager.nicknameInput.text);
        PlayerPrefs.Save();
    }

}