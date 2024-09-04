using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Timeline;
using UnityEngine.Playables;

public class TimelineController : NetworkBehaviour
{
    PlayableDirector pd;
    GameObject[] go;
    GameObject localPlayer;
    TimelineAsset timelineAsset;

    #region Singleton
    public static TimelineController instance;

    void Awake()
    {
        instance = this;
    }
    #endregion

    void Start()
    {
        pd = GetComponent<PlayableDirector>();
        timelineAsset = pd.playableAsset as TimelineAsset;
    }

    public void SetPlayerAnimator()
    {
        go = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject go in go)
        {
            go.GetComponent<PlayerMovement>().OnNetworkSpawn();
        } 
        pd.SetGenericBinding(timelineAsset.GetOutputTrack(1), go[0]);
        pd.SetGenericBinding(timelineAsset.GetOutputTrack(2), go[1]);
        //pd.Play();
        //pd.stopped += StartPlayerMovement;
        PlayTimeline();
        //pd.stopped +=(pd)=> { go[0].GetComponent<PlayerMovement>().enabled = true;
        //                      go[0].GetComponent<PlayerMovement>().enabled = true; 
        //};
    }

    public void PlayTimeline()
    {
        pd.Play();
        pd.stopped += StartPlayerMovement;
    }

    void StartPlayerMovement(PlayableDirector pd)
    {
        localPlayer=NetworkManager.LocalClient.PlayerObject.gameObject;
        localPlayer.GetComponent<PlayerMovement>().enabled = true;
        pd.stopped -= StartPlayerMovement;
    }
}
