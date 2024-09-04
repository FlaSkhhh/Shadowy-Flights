using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;

public class PlayerHealth : NetworkBehaviour
{
    //NetworkVariable<int> ownHealth=new NetworkVariable<int>(100,NetworkVariableReadPermission.Everyone, 
    //                                                            NetworkVariableWritePermission.Server);
    int ownHealth = 100;
    bool dead = false;

    HealthBar hpBar;
    Animator animator;
    OwnerNetworkAnimator nAnimator;

    GameObject[] playa;


    public override void OnNetworkSpawn() 
    {
        ownHealth = 100;
        hpBar = FindObjectOfType<HealthBar>();
        hpBar.StartHP(100, 100);
        animator = GetComponent<Animator>();
        nAnimator = GetComponent<OwnerNetworkAnimator>();
    }

    [ClientRpc]
    void ResetSetupClientRpc()
    {
        TimelineController.instance.PlayTimeline();
        hpBar.StartHP(100, 100);
        dead = false;

        hpBar.transform.parent.transform.GetChild(3).gameObject.SetActive(false);

        playa = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < playa.Length; i++)
        {
            playa[i].GetComponent<PlayerMovement>().GameReset();
            playa[i].GetComponent<PlayerHealth>().ownHealth = 100;
        }
    }

    public void DamageTaken(int hp,bool ifHost)
    {
        nAnimator.SetTrigger("GotHit");
        UpdateHealth(hp,ifHost);
    }

    void UpdateHealth(int hp,bool ifHost)
    {
        if (ifHost)
        {
            ownHealth -= hp;
            UpdateHealthBar(ownHealth, true);
            if (ownHealth <= 0&&!dead) { DeathTrigger(ifHost); }
        }
        else if (!ifHost)
        {
            ownHealth -= hp;
            UpdateHealthBar(ownHealth, false);
            if (ownHealth <= 0&&!dead) { DeathTrigger(ifHost); }
        }
    }

    void UpdateHealthBar(int hp,bool ifhost)
    {
        hpBar.ChangeHPClientRpc(hp, ifhost);
    }

    void DeathTrigger (bool ifHost)
    {
        dead = true;
        if (ifHost)
        {
            hpBar.VictoryScreenClientRpc(ifHost);
            nAnimator.SetTrigger("Death");
            DeathAnimationClientRpc();
        }
        else
        {
            hpBar.VictoryScreenClientRpc(ifHost);
            nAnimator.SetTrigger("Death");
            DeathAnimationClientRpc();
        }
        Invoke("ResetScene", 3f);
    }

    [ClientRpc]
    void DeathAnimationClientRpc()
    {
        playa = GameObject.FindGameObjectsWithTag("Player");

        if (playa[0] == this.gameObject)
        {
            playa[1].GetComponent<Animator>().SetTrigger("Victory");
        }
        else if (playa[1] == this.gameObject)
        {
            playa[0].GetComponent<Animator>().SetTrigger("Victory");
        }
        //getclientbyid only works on host
        playa[0].GetComponent<PlayerHealth>().VictoryPose();
        playa[1].GetComponent<PlayerHealth>().VictoryPose();
    }
    void VictoryPose()
    {
        this.GetComponent<PlayerMovement>().enabled = false;
        this.GetComponent<PlayerInput>().DeactivateInput();
    }

    public void VictoryUI()
    {
        hpBar.transform.parent.GetChild(0).gameObject.SetActive(false);
        hpBar.transform.parent.GetChild(1).gameObject.SetActive(false);
    }
    [ClientRpc]
    public void GameOverUIClientRpc()
    {
        GameObject go = hpBar.transform.parent.gameObject;
        for(int i=0; i<go.transform.childCount-1;i++)
        {
            go.transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    public void PositionReset()
    {
        GetComponent<PlayerMovement>().OnNetworkSpawn();
    }

    void ResetScene()
    {
        ResetSetupClientRpc();
    }
}
