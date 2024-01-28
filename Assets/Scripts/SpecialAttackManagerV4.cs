using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpecialAttackManagerV4 : MonoBehaviour
{
    WigglyArmV3 wiggleArm;

    Coroutine runningAttack = null;

    // Start is called before the first frame update
    void Start()
    {
        wiggleArm = GetComponent<WigglyArmV3>();
        if(!wiggleArm)
        {
            Debug.LogError("Could not get wiggle arm component");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool cancelled = false;
    public void CancelAttack()
    {
        cancelled = true;
    }

    public IEnumerator FistAttack(float force, float forceTime, float driftTime, float retractTime)
    {
        Rigidbody2D hand = wiggleArm.hand;
        hand.GetComponent<HingeJoint2D>().connectedBody = null;
        hand.GetComponent<NetworkedMovement>().enabled = false;
        var direction = hand.transform.up;

        float t = 0;
        while (t < forceTime && !cancelled) {
            hand.AddForce(direction * force);
            yield return null;
            t += Time.deltaTime;
        }

        t = 0;
        while (t < driftTime && !cancelled) {
            Debug.Log($"Drift position {hand.transform.position}");
            yield return null;
            t += Time.deltaTime;
        }
        var finalPos = hand.transform.position;
        Debug.Log($"Final position {hand.transform.position}");
        t = 0;
        while (t < forceTime)
        {
            var targetPos = wiggleArm.GetTargetHandPos();
            Debug.Log($"Last position {hand.transform.position}");
            hand.transform.position = Vector2.Lerp(finalPos, targetPos, t / forceTime);
            Debug.Log($"Set position {hand.transform.position}");
            yield return null;
            t += Time.deltaTime;
        }

        var target = wiggleArm.GetTargetHandPos();
        hand.transform.position = target;
        hand.GetComponent<HingeJoint2D>().connectedBody = wiggleArm.arm.Last();
        hand.GetComponent<NetworkedMovement>().enabled = true;
        cancelled = false;
        runningAttack = null;
        yield break;
    }


    public float fistAttackStrength = 1.0f;
    public float fistAttackForceTime = 0.2f;
    public float fistAttackDriftTime = 0.3f;
    public float fistAttackRetractTime = 1.0f;
    public void LaunchFistAttack()
    {
        if(runningAttack != null)
        {
            Debug.LogError("Attack already started");
            return;
        }
        cancelled = false;
        runningAttack = StartCoroutine(FistAttack(fistAttackStrength, fistAttackForceTime, fistAttackDriftTime, fistAttackRetractTime));
    }

    public IEnumerator CoBigHand(float time, float bigness)
    {
        wiggleArm.hand.transform.localScale = Vector3.one * bigness;
        yield return new WaitForSeconds(time);
        wiggleArm.hand.transform.localScale = Vector3.one;
        yield break;
    }


    Coroutine bigHand;
    public float bigHandTime = 10.0f;
    public float bigHandBigness = 3;
    public void StartBigHand()
    {
        if(bigHand != null)
            StopCoroutine(bigHand);
        bigHand = StartCoroutine(CoBigHand(bigHandTime, bigHandBigness));
    }
}
