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
            PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 0f, 0f), Quaternion.identity, 0);
        }
        
        else
        {
            Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
        }

        //Updates player names
        int c = 0;
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            c++;
            
        }

        print("Res: " + Screen.width + " " + Screen.height);

    }


    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting
        Debug.Log("Ping: " + PhotonNetwork.GetPing());
        //Updates player names when a new player joins
        int c = 0;
        foreach(Player p in PhotonNetwork.PlayerList)
        {
            c++;
            //ScoreboardScript.scoreboardScript.UpdateNameText(c, p.NickName);
        }


        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
        }
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("MainMenu");
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
