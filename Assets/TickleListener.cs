using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class TickleListener : MonoBehaviour
{
    [SerializeField] private GameObject myPlayer;
    [SerializeField] private PhotonView photonView;
    [SerializeField] private string playerTag;
    [SerializeField] private float damagePerTickle = 0.2f;
    [SerializeField] private float cooldown = 0.2f;

    public UnityEvent OnTickle;

    private bool isCooldown;

    private void Start()
    {
        if (photonView && !photonView.IsMine)
            Destroy(this);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamage(collision.collider.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamage(collision.collider.gameObject);
    }

    private void TryDamage(GameObject other)
    {
        if (isCooldown)
            return;

        if (other.tag == playerTag) // Check if colliding with player
        {
            if (other == myPlayer)
                return; //Dont tickle self

            ManageHealth health = other.GetComponentInChildren<ManageHealth>(); // Find the health script
            if (!health)
            {
                Debug.LogError("NO HEALTH ON PLAYER");
                return;
            }

            if (OnTickle != null)
                OnTickle.Invoke();

            Debug.Log("TICKLE TICKLE");
            health.Damage(damagePerTickle * Time.deltaTime); //Apply damage
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
