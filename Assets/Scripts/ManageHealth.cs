using Photon.Pun;
using UnityEngine;

/// <summary>
/// For players
/// </summary>
public class ManageHealth : MonoBehaviour, IPunObservable
{
    public float health;
    public float maxHealth = 5.0f;
    public Transform fill;
    public SpriteRenderer renderer;
    public Color startColor = Color.white;
    public Color endColor = new Color(1.0f, 0.5f, 0.0f);

    private bool hasChanged;
    private bool dead;

    void Start()
    {
        hasChanged = true;
        RefreshHealth();
    }

    public void Damage(float damage)
    {
        if (dead)
            return;

        hasChanged = true;
        health -= damage;
        RefreshHealth();

        if (health < 0.0f)
            Die();
    }

    private void Die()
    {
        Debug.Log("DEAD");
    }

    private void RefreshHealth()
    {
        float healthPercent = health / maxHealth;
        fill.localScale = new Vector3(healthPercent, 1, 1);

        float newX = -0.5f + (healthPercent / 2f);
        fill.localPosition = new Vector3(newX, 0, 0);

        renderer.color = Color.Lerp(startColor, endColor, healthPercent); // Interpolate color from white to orange
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsReading)
            health = (float)stream.ReceiveNext();
        else if(hasChanged)
        {
            stream.SendNext(health);
            hasChanged = false;
        }
    }
}
