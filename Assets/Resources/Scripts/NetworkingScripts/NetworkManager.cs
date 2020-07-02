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

    public GameObject opponentGameObject;

    public static NetworkManager networkManager;

    public bool gameStarted = false;

    public int playersNeededToStart;

    private int currentRound = 1;
    private int numberOfRounds = 7;

    private int numberOfRoundsWon = 0;

    public GameObject[] mapCollection;//Map prefabs to load in

    private string[] characterCollection = { "AceCharacter", "FocusPointCharacter" ,"DoubleImpactCharacter"};//Character prefabs
     
    private GameObject currentMap;


    private void Awake()
    {
        networkManager = this;
    }

    private void Start()
    {
        if (characterCollection == null)
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
            int characterToInstantiate = PlayerPrefs.GetInt("SelectedCharacter",0);
            GameObject newPlayer = PhotonNetwork.Instantiate(characterCollection[characterToInstantiate], new Vector3(0f, 0f, 0f), Quaternion.identity, 0);
            PlayerNetworkingScript.LocalPlayerInstance = newPlayer;
            //Starts game if local client is second player
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("StartRound", RpcTarget.AllBufferedViaServer,GetRandomMapIndex());
            }
            //Sends player gameobject to other client
            photonView.RPC("StorePlayerGameobject", RpcTarget.OthersBuffered, newPlayer.GetPhotonView().ViewID);
            return true;
        }
        return false;
    }

    private void SetPlayersBackToSpawn(Vector3 p1Spawn, Vector3 p2Spawn)
    {
        //Sets player at spawn position
        if (PhotonNetwork.PlayerList[0] == PhotonNetwork.LocalPlayer)
        {
            PlayerNetworkingScript.LocalPlayerInstance.GetComponent<PlayerNetworkingScript>().SpawnPlayerRPC(p1Spawn);
        }
        else
        {
            PlayerNetworkingScript.LocalPlayerInstance.GetComponent<PlayerNetworkingScript>().SpawnPlayerRPC(p2Spawn);
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
        Invoke("TryToStartLocalGame",2f);
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
        }
    }

    [PunRPC]

    private void StartRound(int nextMapIndex)
    {
        if(currentRound != 1)//Resetting level only runs if not first round
        {
            PlayerNetworkingScript.LocalPlayerInstance.GetComponent<PlayerShootingScript>().ResetAmmo();
            if(PlayerNetworkingScript.LocalPlayerInstance.GetComponent<ToggleAbilityClass>() != null)
                PlayerNetworkingScript.LocalPlayerInstance.GetComponent<ToggleAbilityClass>().ResetResourceValue();
            ScreenUIScript.screenUIScript.ShowDeathMessageRPC(PhotonNetwork.LocalPlayer.NickName, false);
            PlayerNetworkingScript.LocalPlayerInstance.layer = 9;
            PlayerNetworkingScript.LocalPlayerInstance.GetComponent<SpriteRenderer>().enabled = true;
            PlayerNetworkingScript.SetIsHit(false);
            if (opponentGameObject != null)
                opponentGameObject.GetComponent<SpriteRenderer>().enabled = true;
            Destroy(currentMap);
        }
        //Loads new map
        currentMap = Instantiate(mapCollection[nextMapIndex], Vector2.zero, Quaternion.identity);
        MapScript mScript = currentMap.GetComponent<MapScript>();
        SetPlayersBackToSpawn(mScript.p1Spawn.transform.position,mScript.p2Spawn.transform.position);
        StartCoroutine(StartRoundMain(mScript.mapName));
    }
    IEnumerator StartRoundMain(String mapName)
    {
        ScreenUIScript.screenUIScript.GameStartedUI(mapName);
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
        photonView.RPC("RoundEnded", RpcTarget.AllViaServer, name,GetRandomMapIndex());
    }

    private int GetRandomMapIndex()
    {
        int length = mapCollection.Length;
        //Randomly selects new map
        return UnityEngine.Random.Range(0, length);
    }
    [PunRPC]
    private void RoundEnded(string name,int nextMapIndex)
    {
        if (gameStarted == false)
            return;
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
        StartCoroutine(PrepareToResetMap(nextMapIndex));
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
        if (currentRound == numberOfRounds + 1 || numberOfRoundsWon > numberOfRounds / 2|| currentRound - numberOfRoundsWon > numberOfRounds / 2 + 1)
        {
            StartCoroutine(EndMatch(isWinning));
            return true;
        }
        else
        {
            return false;
        }
    }
    IEnumerator PrepareToResetMap(int nextMapIndex)
    {
        yield return new WaitForSeconds(6);
        ResetLevel(nextMapIndex);
    }

    public void ResetLevel(int nextMapIndex)
    {
        //Cleans up projectiles
        GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Projectile");
        foreach(GameObject p in projectiles)
        {
            Destroy(p);
        }

        StartRound(nextMapIndex);
        
    }

    private IEnumerator EndMatch(bool didLocalPlayerWin)
    {
        ScreenUIScript.screenUIScript.ShowGameEndScreen(didLocalPlayerWin);
        yield return new WaitForSeconds(5f);
        PhotonNetwork.LeaveRoom();
    }

    #endregion
}
