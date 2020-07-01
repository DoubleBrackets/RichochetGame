using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class ScreenUIScript : MonoBehaviourPunCallbacks
{

    public static ScreenUIScript screenUIScript;
    public GameObject disconnectButton;

    public GameObject playerDisconnectedButton;

    public GameObject waitingForOpponentImage;

    public Text countDownText;
    public Text mapnameText;

    //Resource bar

    public Image resourceBar;

    //Death UI
    public GameObject deathScreen;
    public Text deathMessage;

    //Score
    public Text localPlayerScoreText;
    public Text otherPlayerScoreText;

    //Game end screen
    public GameObject gameEndScreen;
    public Text gameEndText;

    //in case of lag
    private bool hasPlayedDeathScreen = false;

    bool isOptionsMenuActive = false;
    void Awake()
    {
        screenUIScript = this;
        disconnectButton.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            isOptionsMenuActive = !isOptionsMenuActive;
            disconnectButton.SetActive(isOptionsMenuActive);
        }
    }

    public void ShowPlayerDisconnectButton()
    {
        playerDisconnectedButton.SetActive(true);
    }

    public void GameStartedUI(string mapName)
    {
        waitingForOpponentImage.SetActive(false);
        StartCoroutine(CountDown(mapName));
    }

    private IEnumerator CountDown(string mapName)
    {
        mapnameText.text ="Map: " + mapName;
        for(int x = 5;x > 0;x--)
        {
            countDownText.text = "" + x;
            yield return new WaitForSeconds(1);
        }
        countDownText.text = "";
        mapnameText.text = "";
    }
    public void ShowDeathMessageRPC(string name,bool val)
    {
        photonView.RPC("ShowDeathMessage", RpcTarget.All, name,val);
    }
    [PunRPC]
    private void ShowDeathMessage(string name,bool val)
    {
        if (val == true)
        {
            if (hasPlayedDeathScreen)
                return;
            hasPlayedDeathScreen = true;
        }
        else
            hasPlayedDeathScreen = false;
        deathMessage.text = name + " Got SMACKED";
        deathScreen.SetActive(val);
    }

    public void UpdateScoreboard(int localPlayerScore, int opposingPlayerScore)
    {
        localPlayerScoreText.text = "" + localPlayerScore + " <";
        otherPlayerScoreText.text = "> " + opposingPlayerScore;
    }

    public void ShowGameEndScreen(bool didLocalPlayerWin)
    {
        gameEndScreen.SetActive(true);
        if (didLocalPlayerWin)
        {
            gameEndText.text = "U Win :>";
        }
        else
        {
            gameEndText.text = "U Lose :<";
        }
    }


    public void UpdateResourceBar(float val, float max)
    {
        resourceBar.transform.localScale = new Vector2(1,val / max);
    }


    public void UpdateResourceBarColor(Color c)
    {
        resourceBar.color = c;
    }
}
