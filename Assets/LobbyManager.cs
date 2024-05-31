using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField roomCodeInput;
    public Button createGameButton;
    public Button joinGameButton;
    public TextMeshProUGUI creatingRoomText;
    public TextMeshProUGUI joiningRoomText;
    public TextMeshProUGUI failedToCreateText;
    public TextMeshProUGUI failedToJoinText;
    public TextMeshProUGUI generatedCodeText;
    public TMP_InputField nicknameInput;
    public TMP_InputField roomNameInput;
    public SettingsManager settingsManager;

    public Transform roomListContent;
    public GameObject roomListItemPrefab;
    private bool isConnecting;

    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            isConnecting = true;
            PhotonNetwork.ConnectUsingSettings();
        }
        settingsManager = GameObject.Find("LobbyManager").GetComponent<SettingsManager>();
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.EnableCloseConnection = true;
    }

    private void CreateGame()
    {
        if (!PhotonNetwork.IsConnectedAndReady || isConnecting)
        {
            return;
        }

        settingsManager.SavePlayerName();
        SetNickname();

        string roomName = roomNameInput.text;
        string generatedCodeText = GenerateRoomCode();

        if (string.IsNullOrEmpty(roomName))
        {
            return;
        }

        if (roomName.Length < 1 || roomName.Length > 15)
        {
            return;
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
    {
        { "Player1Name", PhotonNetwork.NickName },
        { "RoomName", roomName + generatedCodeText},
        { "IsVisibleInLobby", true }
    };
        roomOptions.MaxPlayers = 2;
        roomOptions.IsVisible = true;
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "RoomName", "IsVisibleInLobby" };
        PhotonNetwork.CreateRoom(roomName + generatedCodeText, roomOptions, TypedLobby.Default);


        createGameButton.gameObject.SetActive(false);
        creatingRoomText.gameObject.SetActive(true);
        failedToCreateText.gameObject.SetActive(false);
    }

    private void JoinGame()
    {
        if (!PhotonNetwork.IsConnectedAndReady || isConnecting)
        {
            return;
        }

        string roomCode = roomCodeInput.text;
        if (string.IsNullOrEmpty(roomCode))
        {
            return;
        }

        settingsManager.SavePlayerName();
        SetNickname();

        PhotonNetwork.JoinRoom(roomCode);
    }
    private void JoinGameWithoutCode(string roomName)
    {
        if (!PhotonNetwork.IsConnectedAndReady || isConnecting)
        {
            return;
        }

        settingsManager.SavePlayerName();
        SetNickname();

        PhotonNetwork.JoinRoom(roomName);
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateRoomList(roomList);
    }

    private void UpdateRoomList(List<RoomInfo> roomList)
    {
        foreach (Transform child in roomListContent)
        {
            Destroy(child.gameObject);
        }
        Vector3 startingPosition = roomListContent.localPosition;
        startingPosition.y -= 30f;
        startingPosition.x += 250f;
        foreach (RoomInfo room in roomList)
        {
            if (room.IsOpen && room.IsVisible
                 && room.CustomProperties.ContainsKey("IsVisibleInLobby")
                 && (bool)room.CustomProperties["IsVisibleInLobby"]
                 && !string.IsNullOrEmpty(room.Name))
            {
                GameObject roomItem = Instantiate(roomListItemPrefab, roomListContent);
                roomItem.transform.localPosition = startingPosition;
                startingPosition.y -= 40f;
                TextMeshProUGUI roomNameText = roomItem.GetComponentInChildren<TextMeshProUGUI>();

                if (roomNameText != null)
                {
                    roomNameText.text = (string)room.CustomProperties["RoomName"];
                    roomNameText.text = room.Name.Substring(0, room.Name.Length - 4);
                }
                else
                {
                    Debug.LogWarning("text component not found in playerListItemPrefab");
                }

                Button joinButton = roomItem.GetComponentInChildren<Button>();
                if (joinButton != null)
                {
                    joinButton.onClick.AddListener(() => JoinGameWithoutCode(room.Name));
                }
            }
        }
    }
    public override void OnCreatedRoom()
    {
        SceneManager.LoadScene("WaitingScene");
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        creatingRoomText.gameObject.SetActive(false);
        failedToCreateText.gameObject.SetActive(true);
        createGameButton.gameObject.SetActive(true);
    }
    public override void OnJoinedRoom()
    {
        SceneManager.LoadScene("WaitingScene");
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        joiningRoomText.gameObject.SetActive(false);
        failedToJoinText.gameObject.SetActive(true);
        joinGameButton.gameObject.SetActive(true);
        createGameButton.gameObject.SetActive(true);
        generatedCodeText.gameObject.SetActive(false);
    }


    private string GenerateRoomCode()
    {
        return UnityEngine.Random.Range(1000, 9999).ToString();
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master!");
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "Ready", false } });
        isConnecting = false;
        PhotonNetwork.JoinLobby();
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetNickname()
    {
        if (nicknameInput.text != "")
        {
            PhotonNetwork.NickName = nicknameInput.text;
        }
    }

    public void OnRefreshButtonClicked()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        PhotonNetwork.JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"Disconnected: {cause}");
        isConnecting = false;
    }

}
