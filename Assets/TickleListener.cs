using System.Collections;
using Photon.Pun;
using UnityEngine;

public class TickleListener : MonoBehaviour
{
    [SerializeField] private GameObject myPlayer;
    [SerializeField] private PhotonView photonView;
    [SerializeField] private string playerTag;
    [SerializeField] private float damagePerTickle = 0.2f;
    [SerializeField] private float cooldown = 0.2f;

    private bool isCooldown;

    private void Start()
    {
        if (!photonView.IsMine)
            Destroy(this);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isCooldown)
            return;

        GameObject other = collision.collider.gameObject;
        if (other.tag == playerTag) // Check if colliding with player
        {
            if (other == myPlayer)
                return; //Dont tickle self

            ManageHealth health = other.GetComponent<ManageHealth>(); // Find the health script
            if (!health)
                Debug.LogError("NO HEALTH ON PLAYER");

            health.Damage(damagePerTickle); //Apply damage
            StartCoroutine(RunCooldown()); //Begin cooldown
        }
    }

    private IEnumerator RunCooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldown);
        isCooldown = false;
    }
}
