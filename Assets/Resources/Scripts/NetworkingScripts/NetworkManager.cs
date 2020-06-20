using System;
using System.Collections;


using UnityEngine;
using UnityEngine.SceneManagement;


using Photon.Pun;
using Photon.Realtime;


public class NetworkManager : MonoBehaviourPunCallbacks
{
    //manages networking for networking scene

    /// <summary>
    /// Called when the local player left the room. We need to load the launcher scene.
    /// </summary>
    /// 

    public GameObject playerPrefab;

    public GameObject p1Spawn;
    public GameObject p2Spawn;

    public static NetworkManager networkManager;

    public bool gameStarted = false;

    private int playersNeededToStart = 2;

    private void Awake()
    {
        networkManager = this;
    }

    private void Start()
    {

        if (playerPrefab == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
        }
        
        else if(PlayerMovementScript.LocalPlayerInstance == null)
        {
            Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManager.GetActiveScene());
            // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
            GameObject newPlayer = PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 0f, 0f), Quaternion.identity, 0);

            //Sets player at spawn position
            if (PhotonNetwork.PlayerList[0] == PhotonNetwork.LocalPlayer)
            {
                newPlayer.GetComponent<PlayerNetworkingScript>().SpawnPlayer(p1Spawn.transform.position);
            }
            else
            {
                newPlayer.GetComponent<PlayerNetworkingScript>().SpawnPlayer(p2Spawn.transform.position);
            }

            //Starts game if local client is second player
            if (PhotonNetwork.CurrentRoom.PlayerCount == playersNeededToStart && !PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("StartGame", RpcTarget.AllViaServer);
            }
        }
        
        else
        {
            Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
        }

        print("Res: " + Screen.width + " " + Screen.height);

    }


    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting
        Debug.Log("Ping: " + PhotonNetwork.GetPing());
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
        }
    }
    [PunRPC]

    private void StartGame()
    {
        StartCoroutine(StartGameMain());
    }
    IEnumerator StartGameMain()
    {
        OptionsUIScript.optionsUIScript.GameStartedUI();
        yield return new WaitForSeconds(5);
        gameStarted = true;
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("RichochetMainMenu");
    }


    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        gameStarted = false;
        PlayerMovementScript.LocalPlayerInstance.layer = 11;
        OptionsUIScript.optionsUIScript.ShowPlayerDisconnectButton();
    }
    public void OnClickOpponentDisconnected()
    {
        LeaveRoom();
        OnLeftRoom();
    }
    public void ResetLevel(string levelName)
    {
        photonView.RPC("ResetLevelMain", RpcTarget.All, levelName);
    }
    [PunRPC]
    public void ResetLevelMain(string levelName)
    {
        PhotonNetwork.LoadLevel(levelName);
    }

    public void PlayerDeath(string name)
    {
        photonView.RPC("PlayerDeathMain", RpcTarget.All, name);
        StartCoroutine(OnPlayerDeath());
    }

    IEnumerator OnPlayerDeath()
    {
        yield return new WaitForSeconds(5);
        ResetLevel("TestMap");
    }
    [PunRPC]
    private void PlayerDeathMain(string name)
    {
        gameStarted = false;
        PlayerMovementScript.LocalPlayerInstance.layer = 11;
        PlayerMovementScript.LocalPlayerInstance.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        OptionsUIScript.optionsUIScript.ShowDeathMessage(name);
    }
}
