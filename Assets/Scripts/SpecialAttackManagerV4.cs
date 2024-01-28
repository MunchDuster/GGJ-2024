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
        if(Input.GetKeyUp(KeyCode.Space))
        {
            LaunchFistAttack();
        }
    }

    private void UpdateHand(Vector2 next)
    {
        Debug.Log($"Updated hand to pos {next}");
        wiggleArm.hand.position = next;   
    }


    public bool cancelled;
    public void CancelAttack()
    {
        cancelled = true;
    }

    public IEnumerator FistAttack(float force, float forceTime, float driftTime, float retractTime)
    {
        Rigidbody2D hand = wiggleArm.hand;
        hand.GetComponent<HingeJoint2D>().connectedBody = null;
        hand.GetComponent<NetworkedMovement>().enabled = false;
        
        float t = 0;
        while (t < forceTime && !cancelled) {
            hand.AddForce(hand.transform.up * force);
            yield return null;
            t += Time.deltaTime;
        }

        t = 0;
        while (t < driftTime && !cancelled) {
            yield return null;
            t += Time.deltaTime;
        }
        var finalPos = hand.position;

        t = 0;
        while (t < forceTime)
        {
            var targetPos = wiggleArm.GetTargetHandPos();
            hand.position = Vector2.Lerp(finalPos, targetPos, t / forceTime);
            yield return null;
            t += Time.deltaTime;
        }

        var target = wiggleArm.GetTargetHandPos();
        hand.position = target;
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
}
