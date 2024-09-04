using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class HealthBar : NetworkBehaviour
{
    public Slider hostHP;
    public Slider clientHP;

    public Text victoryText;

    public void StartHP(int hHp, int cHp)
    {
        clientHP.value = cHp;
        hostHP.value = hHp;
        victoryText.gameObject.SetActive(false);
    }

    [ClientRpc]
    public void ChangeHPClientRpc(int Hp,bool host)
    {
        if (host)
        {
            clientHP.value = Hp;
        }
        else
        {
            hostHP.value = Hp;
        }
    }

    [ClientRpc]
    public void VictoryScreenClientRpc(bool ifHost)
    {
        victoryText.gameObject.SetActive(true);
        if (ifHost)
        {
            victoryText.color = new Color(1f, 0, 0, 1f);
            if (IsHost)
            {
                victoryText.text = "A WINNER IS YOU";
            }
            else
            {
                victoryText.text = "A LOSER IS YOU";
            }
        }
        else
        {
            victoryText.color = new Color(1f, 0.32f, 0.90f, 1f);
            if (IsHost)
            {
                victoryText.text = "A LOSER IS YOU";
            }
            else
            {
                victoryText.text = "A WINNER IS YOU";
            }
        }
    }
}
