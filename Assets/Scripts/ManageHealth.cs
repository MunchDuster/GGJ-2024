using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// For players
/// </summary>
public class ManageHealth : MonoBehaviour, IPunObservable
{
    public float regenSpeed = 1.0f;
    public float cooldownBeforeStartHeal = 3;
    public float maxHealth = 5.0f;
    public Transform fill;
    public SpriteRenderer renderer;
    public Color startColor = Color.white;
    public Color endColor = new Color(1.0f, 0.5f, 0.0f);

    private float health;
    private bool dead;
    private bool healthChanged;
    private Coroutine cooldownBeforeStartHealRunner;

    public UnityEvent OnTickled;

    void Start()
    {
        health = maxHealth;
        RefreshHealth();
    }

    public void Damage(float damage)
    {
        if (dead)
            return;

        health -= damage;
        RefreshHealth();
        OnTickled.Invoke();

        healthChanged = true;

        if (health < 0.0f)
            Die();

        if(cooldownBeforeStartHealRunner != null)
            StopCoroutine(cooldownBeforeStartHealRunner);

        cooldownBeforeStartHealRunner = StartCoroutine(CooldownBeforeHeal());
    }

    private void Die()
    {
        PhotonNetwork.LeaveRoom();
        Debug.Log("DEAD");
    }

    private void RefreshHealth()
    {
        float healthPercent = 1 - Mathf.Clamp01(health / maxHealth); // More health = lower bar
        
        // Make the fill the correct size and placement
        fill.localScale = new Vector3(1, healthPercent, 1);
        fill.localPosition = new Vector3(0,  -0.5f + (healthPercent / 2f), 0);

        renderer.color = Color.Lerp(startColor, endColor, healthPercent); // Interpolate color from white to orange
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsReading && stream.Count > 0)
        {
            health = (float)stream.ReceiveNext();
            RefreshHealth();
        }
        else
        {
            if (healthChanged)
            {
                stream.SendNext(health);
                healthChanged = false;
            }
        }
    }

    private IEnumerator CooldownBeforeHeal()
    {
        yield return new WaitForSeconds(cooldownBeforeStartHeal);
        
        //Health regen loop
        while (health < maxHealth)
        {
            health += Time.deltaTime * regenSpeed;
            RefreshHealth();
            yield return null; // Wait for next frame
        }
    }
}
