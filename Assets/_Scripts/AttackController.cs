using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class AttackController : NetworkBehaviour
{
    [SerializeField] Transform punchBoxM;
    [SerializeField] Transform kicksBoxM;

    [SerializeField] Transform punchBoxF;
    [SerializeField] Transform kicksBoxF;

    Transform punchBox;
    Transform kicksBox;

    [SerializeField] Animator animator;
              NetworkAnimator nAnimator;

    PlayerHealth hp;
    PlayerMovement pm;

    Collider2D[] col;

    bool parry=false;
    bool lowParry=false;

    public override void OnNetworkSpawn()
    {
        if (transform.GetChild(0).gameObject.activeSelf)
        {
            punchBox = punchBoxM;
            kicksBox = kicksBoxM;
        }
        else if(transform.GetChild(1).gameObject.activeSelf)
        {
            punchBox = punchBoxF;
            kicksBox = kicksBoxF;
        }
        nAnimator = GetComponent<NetworkAnimator>();
    }

    void PunchHitbox()
    {
        col = Physics2D.OverlapBoxAll(punchBox.position, new Vector2(0.65f, 0.5f),0f);
        int i = 0;
        while(i <col.Length)
        {
            if (col[i].gameObject.tag == "Player")
            {
                hp = col[i].GetComponent<PlayerHealth>();
                pm = col[i].GetComponent<PlayerMovement>();
                if (col[i].GetComponent<AttackController>().parry)
                {
                    if (!IsHost) return;
                    nAnimator.SetTrigger("Parried");
                    return;//check this interaction.Even with parry active damage being dealt
                }
                if (pm.BlockingStatus()) return;  //blocking the damage

                //damage if not blocking
                if (IsLocalPlayer && IsHost)
                {
                    hp.DamageTaken(10, true);
                    return;
                }
                else if(!IsLocalPlayer && IsHost)
                {
                    hp.DamageTaken(10, false);
                    return;
                }
            }
            i++;
        }
    }

    void HighKick()
    {
        col = Physics2D.OverlapBoxAll(kicksBox.position + transform.localScale.x * Vector3.left / 2.4f, new Vector2(1.4f, 0.7f), 0f);
        for (int i=0;i < col.Length;i++)
        {
            if (col[i].gameObject.tag == "Player")
            {
                if (this.gameObject == col[i].gameObject)
                {
                    continue;         //continue to skip to next iteration if own collider is in kick hitbos
                }
                hp = col[i].GetComponent<PlayerHealth>(); 
                pm = col[i].GetComponent<PlayerMovement>();

                if (col[i].GetComponent<AttackController>().parry)
                {
                    if (!IsHost) return;
                    nAnimator.SetTrigger("ParriedHKick");
                    return;
                }
                if (pm.BlockingStatus()) break;     //highblock

                //damage if not blocking
                if (IsLocalPlayer && IsHost)
                {
                    hp.DamageTaken(10, true);
                }
                else if (!IsLocalPlayer && IsHost)
                {
                    hp.DamageTaken(10, false);
                }
            }
        }
    }

    void LowKick()
    {
        col = Physics2D.OverlapBoxAll(kicksBox.position + transform.localScale.x * Vector3.left / 2.4f, new Vector2(1.4f, 0.7f), 0f);
        int i = 0;
        while (i < col.Length)
        {
            if (col[i].gameObject.tag == "Player")
            {
                hp = col[i].GetComponent<PlayerHealth>();
                pm = col[i].GetComponent<PlayerMovement>();

                if (col[i].GetComponent<AttackController>().lowParry)
                {
                    if (!IsHost) return;
                    nAnimator.SetTrigger("ParriedLKick");
                    return;
                }
                if (pm.LowBlockingStatus()) break;     //lowblock

                //damage if not blocking
                if (IsLocalPlayer && IsHost)
                {
                    hp.DamageTaken(10, true);
                }
                else if (!IsLocalPlayer && IsHost)
                {
                    hp.DamageTaken(10, false);
                }
            }
            i++;
        }
    }

    void ParryStart()
    {
        parry = true;
        if (IsServer) { nAnimator.ResetTrigger("Parry"); }
    }
    void ParryEnd()
    {
        parry = false;
    }
    void LowParryStart()
    {
        lowParry = true;
        if(IsServer) { nAnimator.ResetTrigger("LowParry"); }
    }
    void LowParryEnd()
    {
        lowParry = false;
    }

    void ParriedStart()
    {
        //GetComponent<PlayerMovement>().JumpKickStop(); test if needed
        gameObject.GetComponent<PlayerMovement>().enabled = false;
    }
    void ParriedEnd()
    {
        gameObject.GetComponent<PlayerMovement>().enabled = true;
    }

    void OnDrawGizmosSelected()
    {
        //Gizmos.DrawWireCube(punchBoxF.position, new Vector3(0.65f,0.5f,0));  // Punch hitbox
        //Gizmos.DrawWireCube(kicksBoxF.position + transform.localScale.x*Vector3.left/2.4f, new Vector3(1.4f,0.7f,0));  // Jump and low Kick hitbox
    }
}
