using Photon.Pun;
using TMPro;
using UnityEngine;

public class DisplayPlayerName : MonoBehaviour
{
    public PhotonView photonView;
    public TextMeshPro nameText;

    // Start is called before the first frame update
    void Start()
    {
        nameText.text = photonView.Controller.NickName;
    }
}