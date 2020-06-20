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

    public bool gameStarted = true;

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
            if (PhotonNetwork.PlayerList.Length == 1)
            {
                newPlayer.GetComponent<PlayerNetworkingScript>().SpawnPlayer(p1Spawn.transform.position);
            }
            else
            {
                newPlayer.GetComponent<PlayerNetworkingScript>().SpawnPlayer(p2Spawn.transform.position);
            }

            //Starts game if local client is second player
            if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            {
                gameStarted = true;
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

        //Starts game if 2nd player joins
        if(PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            gameStarted = true;
        }
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
        }
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
        //ScoreboardScript.scoreboardScript.ShowOpponentDisconnected();
    }
    public void OnClickOpponentDisconnected()
    {
        LeaveRoom();
        OnLeftRoom();
    }
}
