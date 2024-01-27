using Photon.Pun;
using UnityEngine;
using static UnityEngine.Rendering.CoreUtils;

/// <summary>
/// HA HA PHSYICS GO BRRR
/// </summary>
public class NetworkedMovement : MonoBehaviourPunCallbacks
{
    [SerializeField] private float movementForce = 10;
    [SerializeField] private float maxForce = 10;
    [SerializeField] private Inputter input;
    [SerializeField] private Rigidbody2D rb;


    Vector2 targetVelocity = new();
    bool isMoving;

    private void Update() // FOR INPUT
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }

        targetVelocity = input.move; // Get user input
        isMoving = targetVelocity.magnitude > 0.2f;
    }

    void FixedUpdate() // FOR PHYSICS
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }

        if (isMoving)
        {
            targetVelocity.Normalize();
            targetVelocity *= movementForce;
        }
        else
            targetVelocity = Vector2.zero;

        rb.AddForce(targetVelocity * Time.fixedDeltaTime, ForceMode2D.Impulse); // GOT TO MOVE IT MOVE IT
        //CalculateForce(targetVelocity, rb, maxForce);

        // Look in direction of movement
        //if (isMoving)
        //    transform.rotation = Quaternion.Euler(0, 0, Vector3.SignedAngle(Vector3.up, rb.velocity.normalized, Vector3.forward));
    }

    public static void CalculateForce(Vector2 desiredVelocity, Rigidbody2D rb, float maxForce)
    {
        Vector2 force = desiredVelocity - rb.velocity; // The missile knows where it is, because it knows where it isnt

        force = force.normalized * Mathf.Min(force.magnitude, maxForce); // Cap the force

        rb.AddForce(force, ForceMode2D.Impulse); // GOT TO MOVE IT MOVE IT
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
}
