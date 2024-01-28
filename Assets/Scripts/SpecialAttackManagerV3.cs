using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpecialAttackManagerV3 : MonoBehaviour
{
    WigglyArmV3 wiggleArm;

    ISpecialAttack currentAttack = null;
    float retractTime = 0.0f;
    float attackTime = 0.0f;
    float currentAttackTime = 0.0f;
    float currentRetractTime = 0.0f;

    Vector2 finalPos = Vector2.zero;

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

        if(currentAttack == null)
        {
            return;
        }

        if(currentAttack.AttackIsDone() || currentAttackTime >= attackTime)
        {
            currentRetractTime += Time.deltaTime;
            float percent = currentRetractTime / retractTime;
            percent = 1.0f - Mathf.Clamp01(percent);

            
            var targetHandPos = wiggleArm.GetTargetHandPos();
            Debug.Log($"Retracting from {finalPos} to {targetHandPos}");
            var next = Vector2.Lerp(targetHandPos, finalPos, percent);
            UpdateHand(next);

            if(currentRetractTime >= retractTime)
            {
                currentAttack = null;
                wiggleArm.LinkSection(wiggleArm.arm.Last(), wiggleArm.hand);
            }
        } 
        else
        {
            Debug.Log("Doing attack");
            currentAttackTime += Time.deltaTime;
            float percent = currentAttackTime / attackTime;
            percent = Mathf.Clamp01(percent);
            var lastPosition = wiggleArm.hand.position;
            var lastDirection = wiggleArm.hand.transform.up.normalized;
            var next = currentAttack.NextPosition(lastPosition, lastDirection, percent);
            UpdateHand(next);
            finalPos = next;
        }
    }

    private void UpdateHand(Vector2 next)
    {
        Debug.Log($"Updated hand to pos {next}");
        wiggleArm.hand.position = next;   
    }

    public void CancelAttack()
    {
        currentAttackTime = attackTime;
        currentAttack.MaxReached();
    }

    public bool LaunchSpecialAttack(ISpecialAttack attack)
    {
        if(currentAttack != null || !wiggleArm.photonView.IsMine)
        {
            return false;
        }

        currentAttack = attack;
        currentAttackTime = 0.0f;
        attackTime = currentAttack.GetTotalAttackTime();
        retractTime = currentAttack.GetRetractTime();
        finalPos = wiggleArm.hand.position;

        wiggleArm.hand.GetComponent<HingeJoint2D>().connectedBody = null;

        currentAttack.StartAttack(wiggleArm.hand.transform.position, wiggleArm.hand.transform.up);
        return true;
    }

    class FistAttack : ISpecialAttack
    {
        float totalDistance = 2;
        Vector2 startPosition;
        Vector2 targetPosition;
        Vector2 currentPosition;

        public bool AttackIsDone()
        {
            return currentPosition == targetPosition;
        }

        public void Finished()
        {
            currentPosition = startPosition;
        }

        public float GetRetractTime()
        {
            return 1.0f;
        }

        public float GetTotalAttackTime()
        {
            return 0.5f;
        }

        public void MaxReached()
        {
            // Override target with the current position 
            targetPosition = currentPosition;
        }

        public Vector2 NextPosition(Vector2 currentPosition, Vector2 currentDirection, float time)
        {
            this.currentPosition = Vector2.Lerp(startPosition, targetPosition, time);
            Debug.Log($"Lerping between {startPosition}  and {targetPosition}, currently at {this.currentPosition} at percentage {time * 100}");
            return this.currentPosition;
        }

        public void StartAttack(Vector2 startPosition, Vector2 startDirection)
        {
            this.startPosition = startPosition;
            this.targetPosition = startPosition + startDirection * totalDistance;
            this.currentPosition = startPosition;
        }
    };

    public void LaunchFistAttack()
    {
        if(!LaunchSpecialAttack(new FistAttack()))
        {
            Debug.LogError("Attack already started");
            return;
        }
        Debug.Log("Attack launched!");
    }
}
