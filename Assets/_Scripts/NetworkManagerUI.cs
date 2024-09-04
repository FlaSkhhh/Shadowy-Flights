using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkManagerUI : MonoBehaviour
{
    public InputField password;

    [SerializeField]
    GameObject joinScreen;

    [SerializeField]
    ConnectionRelay relay;

    string joinPassword;

    #region Singleton

    public static NetworkManagerUI instance;

    void Awake()
    {
        instance = this;
    }

    #endregion


    public void JoinScreen()
    {
        joinScreen.SetActive(true);
        joinScreen.transform.GetChild(0).GetComponent<Text>().text = "Submit Password";
        joinScreen.transform.GetChild(2).gameObject.SetActive(true);
        joinScreen.transform.GetChild(3).gameObject.SetActive(true);
    }

    public void PasswordChange()
    {
        joinPassword = password.text;
    }

    public void SubmitPassword()
    {
        relay.JoinRelay(joinPassword);
    }

    public void BackButton()
    {
        if (relay.NetworkManager.IsHost) {relay.HostShutdown(); }
        joinScreen.transform.GetChild(1).gameObject.SetActive(false);
        joinScreen.SetActive(false);
        joinScreen.transform.GetChild(3).gameObject.SetActive(false);
        joinScreen.transform.GetChild(4).gameObject.SetActive(false);
    }

    public void WrongCode()
    {
        joinScreen.transform.GetChild(0).GetComponent<Text>().text = "Wrong Password" +"\n"+"Try Again";
    }
}
