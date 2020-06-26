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

    private int currentRound = 1;
    private int numberOfRounds = 5;

    private int numberOfRoundsWon = 0;


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
        if(currentRound != 1)//Resetting level only runs if not first round
        {
            SetPlayersBackToSpawn();
            PlayerNetworkingScript.LocalPlayerInstance.GetComponent<PlayerShootingScript>().ResetAmmo();
            ScreenUIScript.screenUIScript.ShowDeathMessageRPC(PhotonNetwork.LocalPlayer.NickName, false);
            PlayerNetworkingScript.LocalPlayerInstance.layer = 9;
            PlayerNetworkingScript.LocalPlayerInstance.GetComponent<SpriteRenderer>().enabled = true;
            opponentGameObject.GetComponent<SpriteRenderer>().enabled = true;

        }

        StartCoroutine(StartGameMain());
    }
    IEnumerator StartGameMain()
    {
        ScreenUIScript.screenUIScript.GameStartedUI();
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
        ScreenUIScript.screenUIScript.ShowPlayerDisconnectButton();
    }
    public void OnClickOpponentDisconnected()
    {
        LeaveRoom();
        OnLeftRoom();
    }

    #region Player death and round resetting

    //Local method, run when local player dies
    public void PlayerDeathRPC(string name)
    {
        photonView.RPC("RoundEnded", RpcTarget.All, name);
    }

    [PunRPC]
    private void RoundEnded(string name)
    {
        gameStarted = false;
        PlayerNetworkingScript.LocalPlayerInstance.layer = 11;
        PlayerNetworkingScript.LocalPlayerInstance.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        bool localPlayerWonRound = name.CompareTo(PhotonNetwork.LocalPlayer.NickName) != 0;
        //Checks if game ended
        if (CheckForGameEnd(localPlayerWonRound))
        {
            return;
        }
        //Prepare to reset map and start new round otherwise
        StartCoroutine(PrepareToResetMap());
    }

    private bool CheckForGameEnd(bool wonRound)
    {
        if (wonRound)
        {
            numberOfRoundsWon++;
        }
        currentRound++;

        int enemyRoundsWon = currentRound - numberOfRoundsWon - 1;

        bool isWinning = (enemyRoundsWon < numberOfRoundsWon);

        ScreenUIScript.screenUIScript.UpdateScoreboard(numberOfRoundsWon, enemyRoundsWon);
        //end game if round limit reached or player has won Best of (rounds), otherwise start a new round
        if (currentRound == numberOfRounds + 1 || numberOfRoundsWon > numberOfRounds / 2 + 1 || currentRound - numberOfRoundsWon > numberOfRounds / 2 + 1)
        {
            StartCoroutine(EndMatch(isWinning));
            return true;
        }
        else
        {
            return false;
        }
    }
    IEnumerator PrepareToResetMap()
    {
        yield return new WaitForSeconds(6);
        ResetLevel();
    }

    public void ResetLevel()
    {
        //Cleans up projectiles
        GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Projectile");
        foreach(GameObject p in projectiles)
        {
            Destroy(p);
        }

        StartGame();
        
    }

    private IEnumerator EndMatch(bool didLocalPlayerWin)
    {
        ScreenUIScript.screenUIScript.ShowGameEndScreen(didLocalPlayerWin);
        yield return new WaitForSeconds(5f);
        PhotonNetwork.LeaveRoom();
    }

    #endregion
}
