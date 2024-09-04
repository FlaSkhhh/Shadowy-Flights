using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using TMPro;


public class ConnectionRelay : NetworkBehaviour
{
    [SerializeField] TMP_Text text;
    [SerializeField] Text waitText;

    [SerializeField] GameObject startScreen;
    [SerializeField] GameObject loadScreen;

    ulong clId;
    string joinCode;

    async void Start()
    {
        await UnityServices.InitializeAsync();

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        //if(IsHost){} to remove exception at clients
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
    }

    void OnClientConnectedCallback(ulong clientId)
    {
        if (!IsHost) return;
        if (NetworkManager.ConnectedClients[clientId] == NetworkManager.LocalClient) return; //return on client and proceed on host 
        //clId = clientId;
        loadScreen.transform.GetChild(4).gameObject.SetActive(true);
        loadScreen.transform.GetChild(1).gameObject.SetActive(false);
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
    }

    void OnClientDisconnectCallback(ulong clientId)
    {
        startScreen.SetActive(true);
        loadScreen.SetActive(true);
        loadScreen.transform.GetChild(0).gameObject.SetActive(true);
        for(int i=1;i< loadScreen.transform.childCount;i++)
        {
            loadScreen.transform.GetChild(i).gameObject.SetActive(false);
        }
        waitText.text = "Opponent disconnected. Quitting App...";
        Invoke("GameOver", 2f);
    }
    void GameOver()
    {
        Application.Quit();
    }

    public async void CreateRelay()
    {
        try
        {
            loadScreen.SetActive(true);
            loadScreen.transform.GetChild(2).gameObject.SetActive(false);
            waitText.text = "Please Wait ...";
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);

            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            text.gameObject.SetActive(true);
            text.text = "Lobby code is: \n"  + "<color=red>"+ joinCode+"</color>";
            waitText.text = "Waiting for connection...";
            loadScreen.transform.GetChild(3).gameObject.SetActive(true);
            Debug.Log(joinCode);

            RelayServerData data = new RelayServerData(allocation,"dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(data);
            HostStarted();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinRelay(string code)
    {
        try
        {
            JoinAllocation joined =await RelayService.Instance.JoinAllocationAsync(code);

            RelayServerData data = new RelayServerData(joined, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(data);
            waitText.text = "Connected. Waiting for host to start...";
            loadScreen.transform.GetChild(2).gameObject.SetActive(false);
            loadScreen.transform.GetChild(3).gameObject.SetActive(false);
            ClientStart();
        }
        catch(RelayServiceException e)
        {
            if (code != joinCode) NetworkManagerUI.instance.WrongCode();
            Debug.Log(e);
        }
    }

    void HostStarted()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void GameStart()
    {
        NetworkManager.LocalClient.PlayerObject.GetComponent<MultiplayerSetup>().MultiplayerEstablishedClientRpc();
        //NetworkManager.ConnectedClients[clId].PlayerObject.GetComponent<MultiplayerSetup>().MultiplayerEstablishedClientRpc();
    }

    void ClientStart()
    {
        NetworkManager.Singleton.StartClient();
    }

    public void HostShutdown() 
    {
        NetworkManager.Singleton.Shutdown(true);
    }

    //[ClientRpc]
    //public void RemoveStartScreenClientRpc()
    //{
    //    startScreen.SetActive(false);
    //    loadScreen.SetActive(false);
    //}
}
