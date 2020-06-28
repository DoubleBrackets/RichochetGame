using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Photon.Pun;
using Photon.Realtime;

public class DevTestUI : MonoBehaviourPunCallbacks
{
    public static DevTestUI devTestUI;

    public Text ping;

    private void Awake()
    {
        devTestUI = this;
    }

    private void Update()
    {
        ping.text ="Ping: "+ PhotonNetwork.GetPing() + " ms";
    }

}
