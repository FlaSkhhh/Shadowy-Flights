using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MultiplayerSetup : NetworkBehaviour
{
    GameObject startScreen;
    GameObject loadScreen;
    //PlayerMovement pm;

    void Start()
    {
        //pm = GetComponent<PlayerMovement>();
        //if (IsHost) pm.enabled = false;
    }

    [ClientRpc]
    public void MultiplayerEstablishedClientRpc()
    {   
        RemoveUIScreens();
        StartTimeline();
    }

    void RemoveUIScreens()
    {
        startScreen = NetworkManagerUI.instance.transform.parent.gameObject;
        loadScreen = startScreen.transform.GetChild(2).gameObject; //this WORKS!!!
        if (startScreen != null) startScreen.SetActive(false);
        if (loadScreen != null) loadScreen.SetActive(false);
    }

    void StartTimeline()
    {
        TimelineController.instance.SetPlayerAnimator();
    }
}
