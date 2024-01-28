using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ScoreManager : MonoBehaviour, IComparable, IPunObservable
{
    public static List<ScoreManager> managers = new();
    public PhotonView photonView;

    public float score;


    private void Start()
    {
        managers.Add(this);
    }

    private void OnDestroy()
    {
        managers.Remove(this);
    }

    public void AddScore()
    {
        score++;
    }

    public int CompareTo(object obj)
    {
        ScoreManager other = (ScoreManager)obj;

        if (other == null || other.score > score) return 1;
           
        return -1;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        stream.Serialize(ref score);
    }
}
