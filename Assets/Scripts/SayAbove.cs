using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SayAbove : MonoBehaviour
{
    public GameObject player;
    public float yOffset = 0f;
    public float xOffset = 0.75f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(player.transform.localPosition.x + xOffset, player.transform.localPosition.y + yOffset, player.transform.localPosition.z);
    }
}
 