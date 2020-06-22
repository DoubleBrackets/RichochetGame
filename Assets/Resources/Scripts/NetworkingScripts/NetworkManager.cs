using System;
using System.Collections;


using UnityEngine;
using UnityEngine.SceneManagement;


using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;


public class NetworkManager : MonoBehaviourPunCallbacks
{

    /// <summary>
    /// Called when the local player left the room. We need to load the launcher scene.
    /// </summary>
    /// 

    public GameObject playerPrefab;

    public GameObject opponentGameObject;

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
        TryToStartLocalGame();
        print("Res: " + Screen.width + " " + Screen.height);
    }

    [PunRPC]
    private bool TryToStartLocalGame()
    {
        if (PlayerNetworkingScript.LocalPlayerInstance == null && PhotonNetwork.CurrentRoom.PlayerCount == playersNeededToStart)
        {
            Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManager.GetActiveScene());
            // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
            GameObject newPlayer = PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 0f, 0f), Quaternion.identity, 0);
            PlayerNetworkingScript.LocalPlayerInstance = newPlayer;
            SetPlayersBackToSpawn();
            //Starts game if local client is second player
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("StartGame", RpcTarget.AllBufferedViaServer);
            }
            //Sends player gameobject to other client
            photonView.RPC("StorePlayerGameobject", RpcTarget.OthersBuffered, newPlayer.GetPhotonView().ViewID);
            return true;
        }
        return false;
    }

    private void SetPlayersBackToSpawn()
    {
        //Sets player at spawn position
        if (PhotonNetwork.PlayerList[0] == PhotonNetwork.LocalPlayer)
        {
            PlayerNetworkingScript.LocalPlayerInstance.GetComponent<PlayerNetworkingScript>().SpawnPlayerRPC(p1Spawn.transform.position);
        }
        else
        {
            PlayerNetworkingScript.LocalPlayerInstance.GetComponent<PlayerNetworkingScript>().SpawnPlayerRPC(p2Spawn.transform.position);
        }
    }

    [PunRPC]

    private void StorePlayerGameobject(int viewId)
    {
        opponentGameObject = PhotonNetwork.GetPhotonView(viewId).gameObject;
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting
        Debug.Log("Ping: " + PhotonNetwork.GetPing());
        TryToStartLocalGame();
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
        PlayerNetworkingScript.LocalPlayerInstance.layer = 11;
        OptionsUIScript.optionsUIScript.ShowPlayerDisconnectButton();
    }
    public void OnClickOpponentDisconnected()
    {
        LeaveRoom();
        OnLeftRoom();
    }
    //Local method, run when local player dies
    public void PlayerDeathRPC(string name)
    {
        photonView.RPC("PlayerDeath", RpcTarget.All, name);
    }

    [PunRPC]
    private void PlayerDeath(string name)
    {
        gameStarted = false;
        PlayerNetworkingScript.LocalPlayerInstance.layer = 11;
        PlayerNetworkingScript.LocalPlayerInstance.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        StartCoroutine(PrepareToResetMap());
    }
    IEnumerator PrepareToResetMap()
    {
        yield return new WaitForSeconds(6);
        ResetLevel("TestMap");
    }

    public void ResetLevel(string levelName)
    {
        //Cleans up projectiles
        GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Projectile");
        foreach(GameObject p in projectiles)
        {
            Destroy(p);
        }
        SetPlayersBackToSpawn();
        PlayerNetworkingScript.LocalPlayerInstance.GetComponent<PlayerShootingScript>().ResetAmmo();
        photonView.RPC("StartGame", RpcTarget.AllBufferedViaServer);
        OptionsUIScript.optionsUIScript.ShowDeathMessageRPC(PhotonNetwork.LocalPlayer.NickName, false);
        PlayerNetworkingScript.LocalPlayerInstance.layer = 9;
        PlayerNetworkingScript.LocalPlayerInstance.GetComponent<SpriteRenderer>().enabled = true;
        opponentGameObject.GetComponent<SpriteRenderer>().enabled = true;
    }


}
