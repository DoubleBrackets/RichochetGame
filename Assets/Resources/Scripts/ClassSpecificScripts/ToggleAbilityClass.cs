using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public abstract class ToggleAbilityClass : MonoBehaviourPunCallbacks
{
    protected float resourceValue = 0;
    public float maxResourceValue;
    public float toggleThreshholdValue;
    public float resourceDrainRate;

    public string playerParticleKey;

    public Color belowToggleThreshholdColor;
    public Color aboveToggleThreshholdColor;
    public Color fullColor;

    public float resourceRegenRate;
    protected bool isToggleActive = false;


    protected void UpdateResourceBar()
    {
        ScreenUIScript.screenUIScript.UpdateResourceBar(resourceValue, maxResourceValue);
        
        if (resourceValue == maxResourceValue)
            ScreenUIScript.screenUIScript.UpdateResourceBarColor(fullColor);
        else if(resourceValue < toggleThreshholdValue)
            ScreenUIScript.screenUIScript.UpdateResourceBarColor(belowToggleThreshholdColor);
        else
            ScreenUIScript.screenUIScript.UpdateResourceBarColor(aboveToggleThreshholdColor);
    }

    public void ResetResourceValue()
    {
        resourceValue = 0;
        UpdateResourceBar();
        PlayerParticleManager.playerParticleManager.StopParticle(playerParticleKey + PhotonNetwork.LocalPlayer.NickName);
    }

    protected void UpdateResourceValues()
    {       
        if (resourceValue < maxResourceValue)
        {
            resourceValue += resourceRegenRate * Time.deltaTime;
            if (resourceValue > maxResourceValue)
                resourceValue = maxResourceValue;
        }
        if (isToggleActive)
        {
            resourceValue -= resourceDrainRate * Time.deltaTime;
            if(resourceValue <= 0)
            {
                resourceValue = 0;
                photonView.RPC("ToggleOff", RpcTarget.AllViaServer);
                isToggleActive = false;
            }
        }
        UpdateResourceBar();
    }
    protected abstract void ToggleOn();
    protected abstract void ToggleOff();
}
