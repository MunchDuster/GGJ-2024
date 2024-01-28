using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectPowerUp : MonoBehaviour
{
    public PhotonView photonView;
    public SpecialAttackManagerV4 leftAttackMan;
    public SpecialAttackManagerV4 rightAttackMan;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "PowerUpTag")
        {
            leftAttackMan.StartBigHand();
            rightAttackMan.StartBigHand();
            Destroy(collision.gameObject);
        }
    }
}
