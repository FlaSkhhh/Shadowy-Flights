using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] Animator animator;
    Vector2 rawInput;

    float actionDelay = 0.7f;
    float timeToWaitForNextAction=0f;

    NetworkVariable<bool> blocking = new NetworkVariable<bool>(false, 
                                     NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    NetworkVariable<bool> lowBlock = new NetworkVariable<bool>(false,
                                     NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    bool inAction = false;
    
    public override void OnNetworkSpawn()
    {
        if (IsHost && IsOwner)
        {
            transform.position = new Vector3(-4f, -3.29f, 0f);
            transform.name = "Local Player";
            ActivateChara(0);
            DeactivateChara(1);
        }
        else if(IsHost && !IsOwner)
        {
            transform.name = "Client Player";
            ActivateChara(1);
            DeactivateChara(0);
        }
        else if (!IsHost && IsOwner)
        {
            transform.position = new Vector3(4f, -3.29f, 0f);
            transform.localScale = new Vector3(-1, 1, 1);
            transform.name = "Local Player";
            ActivateChara(1);
            DeactivateChara(0);
        }
        else if (!IsHost && !IsOwner)
        {
            transform.name = "Client Player";
            ActivateChara(0);
            DeactivateChara(1);
        }
    }

    void ActivateChara(int i)
    {
        transform.GetChild(i).gameObject.SetActive(true);
        transform.GetChild(i).name = "The Man";
    }
    void DeactivateChara(int i)
    {
        transform.GetChild(i).gameObject.SetActive(false);
        transform.GetChild(i).name = "The Man_1";
    }

    public void GameReset()
    {
        if (IsHost && IsLocalPlayer)
        {
            transform.position = new Vector3(-4f, -3.29f, 0f);
        }
        if (!IsHost && IsLocalPlayer)
        {
            transform.position = new Vector3(4f, -3.29f, 0f);
        }
    }

    void OnEnable()
    {
        inAction = false;
        timeToWaitForNextAction = 0f;

        PlayerInput playerInput = this.GetComponent<PlayerInput>();
        if (IsHost && IsLocalPlayer)//to fix p2 not working with keyboard 
        {
            playerInput.enabled = false;
            playerInput.enabled = true;
        }                          //Restarting player input for correctly subscribing to events after ui buttons are enabled 
        playerInput.ActivateInput();// to reactivate controls deactivated in health script
    }
    void OnDisable()
    {
        JumpKickStop();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;
        Move();
        Block();
    }
    //blocking getter
    public bool BlockingStatus()
    {
        return blocking.Value;
    }
    public bool LowBlockingStatus()
    {
        return lowBlock.Value;
    }

    void Move()
    {
        if (blocking.Value||lowBlock.Value) return;
        if (rawInput!= new Vector2(0f,0f)&&!inAction)
        {
            animator.SetBool("Walking", true);
            transform.Translate(1.5f * rawInput * Time.deltaTime);
            //ResetActions();
        }
        else animator.SetBool("Walking", false);
    }

    void Block()
    {
        if (blocking.Value)
        {
            animator.SetBool("Block", true);
        }
        else if (lowBlock.Value)
        {
            animator.SetBool("LowBlock", true);
        }
        else
        {
            animator.SetBool("LowBlock", false);
            animator.SetBool("Block", false);
        }
    }

    void ResetActions()
    {
        animator.ResetTrigger("Punch"); animator.ResetTrigger("Kick");
        animator.ResetTrigger("Parry"); animator.ResetTrigger("Kick2");
    }
    void InAction()
    {
        inAction=true;
    }
    void Action()
    {
        inAction = false;
    }

    public void JumpKickStop()
    {
        StopCoroutine("JumpDistanceCoroutine");
    }

    IEnumerator JumpDistanceCoroutine()
    {
        float intervalTime = 0.35f / 15f;
        while (true)
        {
            transform.position = Vector2.MoveTowards(transform.position, transform.position + transform.right / 2 * transform.localScale.x, 0.7f / 12f); 
            yield return new WaitForSeconds(intervalTime);
        }
    }

    void OnWalk(InputValue val)
    {
        rawInput=val.Get<Vector2>();
    }
    void OnPunch(InputValue val)
    {
        if (!IsOwner) return;
        if (blocking.Value || lowBlock.Value) return;
        if (inAction) return;
        if (Time.time > timeToWaitForNextAction)
        {
            animator.SetTrigger("Punch");
            timeToWaitForNextAction = Time.time + actionDelay;
            inAction = true;
        }
    }
    void OnKick(InputValue val)
    {
        if (!IsOwner) return;
        if (blocking.Value || lowBlock.Value) return;
        if (inAction) return;
        if (Time.time > timeToWaitForNextAction)
        {
            animator.SetTrigger("Kick");
            timeToWaitForNextAction = Time.time + actionDelay + 0.4f;
            inAction = true;
        }
    }
    void OnJumpKick(InputValue val)
    {
        if (!IsOwner) return;
        if (blocking.Value || lowBlock.Value) return;
        if (inAction) return;
        if (Time.time > timeToWaitForNextAction)
        {
            animator.SetTrigger("Kick2");
            timeToWaitForNextAction = Time.time + actionDelay;
            inAction = true;
        }
    }
    void StartJumpMovement()
    {
        if (!IsOwner) return;
        StartCoroutine("JumpDistanceCoroutine");
    }
    void OnBlock(InputValue val)
    {
        if (!IsOwner) return;
        blocking.Value=val.isPressed;
    }
    void OnLowBlock(InputValue val)
    {
        if (!IsOwner) return;
        lowBlock.Value = val.isPressed;
    }
    void OnParry(InputValue val)
    {
        if (!IsOwner) return;
        if (blocking.Value || lowBlock.Value) return;
        if (inAction) return;
        ParryServerRpc();
    }
    [ServerRpc]
    void ParryServerRpc()
    {
        GetComponent<NetworkAnimator>().SetTrigger("Parry");
        inAction = true;
    }
    void OnLowParry(InputValue val)
    {
        if(!IsOwner) return;
        if (blocking.Value || lowBlock.Value) return;
        if (inAction) return;
        LowParryServerRpc();
    }
    [ServerRpc]
    void LowParryServerRpc()
    {
        GetComponent<NetworkAnimator>().SetTrigger("LowParry");
        inAction = true;
    }
}
