using System.Collections;
using System.Collections.Generic;
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
    private float healthRecovered = 0.0f;
    private Coroutine cooldownBeforeStartHealRunner;

    public PhotonView photonView;

    private static Dictionary<string, float> damageDealt = new();
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

        damageDealt.TryAdd(photonView.Controller.NickName, 0.0f);
        damageDealt[photonView.Controller.NickName] += damage;

        RefreshHealth();
        OnTickled.Invoke();

        if (photonView.IsMine)
        {
            if (health < 0.0f)
                Die();

            if (cooldownBeforeStartHealRunner != null)
                StopCoroutine(cooldownBeforeStartHealRunner);

            cooldownBeforeStartHealRunner = StartCoroutine(CooldownBeforeHeal());
        }
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
        fill.localPosition = new Vector3(0, -0.5f + (healthPercent / 2f), 0);

        renderer.color = Color.Lerp(startColor, endColor, healthPercent); // Interpolate color from white to orange
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(healthRecovered);
            healthRecovered = 0;
            stream.SendNext(damageDealt.Count);
            foreach(var pair in damageDealt)
            {
                stream.SendNext(pair.Key);
                stream.SendNext(pair.Value);
            }
            damageDealt.Clear();
        } 
        else
        {
            health += (float)stream.ReceiveNext();

            var players = GameObject.FindGameObjectsWithTag("Player");
            int numDamagedPlayers = (int)stream.ReceiveNext();
            for(int i = 0; i < numDamagedPlayers; i++)
            {
                var name = (string)stream.ReceiveNext();
                var damage = (float)stream.ReceiveNext();
                foreach( var p in players)
                {
                    if(p.GetComponent<PhotonView>().Controller.NickName == name)
                    {
                        p.GetComponentInChildren<ManageHealth>().Damage(damage);
                    }
                }
            }
        }
    }

    private IEnumerator CooldownBeforeHeal()
    {
        yield return new WaitForSeconds(cooldownBeforeStartHeal);
        
        //Health regen loop
        while (health < maxHealth)
        {
            float healAmount = Time.deltaTime * regenSpeed;

            health += healAmount;
            healthRecovered += healAmount;
            RefreshHealth();

            yield return null; // Wait for next frame
        }
    }
}
