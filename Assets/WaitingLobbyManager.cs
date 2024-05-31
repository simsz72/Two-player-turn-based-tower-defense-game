using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using System.Collections.Generic;

public class WaitingLobbyManager : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI roomCodeText;
    public TextMeshProUGUI roomNameText;
    public Button readyButton;
    public Button kickButton;
    private bool isReady = false;
    public Button privacyButton;

    private Transform playerListContent;
    public GameObject playerListItemPrefab;
    private Dictionary<int, TextMeshProUGUI> playerReadyTexts = new Dictionary<int, TextMeshProUGUI>();

    private void Start()
    {
        kickButton.gameObject.SetActive(false);
        privacyButton.gameObject.SetActive(false);
        roomNameText.text = "Room Name: " + (string)PhotonNetwork.CurrentRoom.CustomProperties["RoomName"];
        playerListContent = GameObject.Find("PlayerListContent").transform;
        object isReadyValue;
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Ready", out isReadyValue))
        {
            isReady = (bool)isReadyValue;
        }

        UpdateReadyButton();
        UpdatePlayerList();
        UpdatePrivacyButtonText();
    }
    public void OnPrivacyButtonClicked()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        bool newVisibility = !PhotonNetwork.CurrentRoom.IsVisible;
        PhotonNetwork.CurrentRoom.IsVisible = newVisibility;

        UpdatePrivacyButtonText();

        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable()
        {
            { "IsVisibleInLobby", PhotonNetwork.CurrentRoom.IsVisible }
        });

        PhotonNetwork.LeaveLobby();
        PhotonNetwork.JoinLobby();
    }

    private void UpdatePrivacyButtonText()
    {
        string newText = PhotonNetwork.CurrentRoom.IsVisible ? "Make Private" : "Make Public";
        privacyButton.GetComponentInChildren<TextMeshProUGUI>().text = newText;

    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 0 && PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;

        }
        UpdatePlayerList();
    }
    public void OnReadyButtonClicked()
    {
        isReady = !isReady;
        UpdateReadyButton();
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "Ready", isReady } });
    }
    private void UpdatePlayerList()
    {
        kickButton.gameObject.SetActive(PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount == 2);
        privacyButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        foreach (Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }
        Vector3 startingPosition = playerListContent.localPosition;
        startingPosition.y -= 40f;
        startingPosition.x += 325f;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject playerItem = Instantiate(playerListItemPrefab, playerListContent);
            playerItem.transform.localPosition = startingPosition;
            startingPosition.y -= 80f;

            TextMeshProUGUI playerNameText = playerItem.GetComponentInChildren<TextMeshProUGUI>(true);
            if (playerNameText == null)
            {
                Debug.LogError("PlayerNameText component not found in playerListItemPrefab.");
                continue;
            }
            playerNameText.text = player.NickName;


            TextMeshProUGUI readyText = playerItem.transform.Find("ReadyText")?.GetComponent<TextMeshProUGUI>();
            if (readyText != null)
            {
                playerReadyTexts[player.ActorNumber] = readyText;
                UpdatePlayerReadyStatus(player);
            }
            else
            {
                Debug.LogWarning("ReadyText component not found in playerListItemPrefab.");
            }
        }
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("Ready"))
        {
            UpdatePlayerReadyStatus(targetPlayer);
            if (PhotonNetwork.IsMasterClient)
            {
                CheckIfAllPlayersReady();
            }
        }
    }

    private void UpdatePlayerReadyStatus(Player player)
    {
        if (playerReadyTexts.TryGetValue(player.ActorNumber, out TextMeshProUGUI readyText))
        {
            readyText.text = player.CustomProperties.ContainsKey("Ready") && (bool)player.CustomProperties["Ready"] ? "Ready" : "Not Ready";
        }
    }
    private void CheckIfAllPlayersReady()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            return;
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            object isPlayerReady;
            if (player.CustomProperties.TryGetValue("Ready", out isPlayerReady))
            {
                if (!(bool)isPlayerReady)
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable()
        {
            { "IsVisibleInLobby", PhotonNetwork.CurrentRoom.IsVisible }
        });
        PhotonNetwork.LoadLevel("GameScene");
    }
    private void UpdateReadyButton()
    {
        readyButton.GetComponentInChildren<TextMeshProUGUI>().text = isReady ? "Cancel" : "Ready";
    }
    public void OnKickPlayerButtonClicked()
    {
        Player playerToKick = null;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player != PhotonNetwork.LocalPlayer)
            {
                playerToKick = player;
                break;
            }
        }
        if (playerToKick != null)
        {
            PhotonNetwork.CloseConnection(playerToKick);
            Debug.Log("Kicked player: " + playerToKick.NickName);
        }
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
           PhotonNetwork.LeaveRoom();
    }
    public void OnLeaveLobbyButtonClicked()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    }
    public override void OnLeftRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        SceneManager.LoadScene("MainMenuScene");
    }
}
