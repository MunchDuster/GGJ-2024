using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpecialAttackManager : MonoBehaviour
{
    WigglyArmV2 wiggleArm;

    ISpecialAttack currentAttack = null;
    float retractTime = 0.0f;
    float attackTime = 0.0f;
    float currentAttackTime = 0.0f;
    float currentRetractTime = 0.0f;
    Vector2 lastPosition;
    Vector2 lastDirection;

    int finalLength = 0;

    // Start is called before the first frame update
    void Start()
    {
        wiggleArm = GetComponent<WigglyArmV2>();
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
            percent = Mathf.Clamp01(percent);

            Debug.Log($"Doing retract {percent * 100}% ({currentRetractTime} / {retractTime}");
            int targetItems = (int)((1.0f - percent) * finalLength);
            RetractAttack(targetItems);

            if(targetItems == 0)
            {
                currentAttack = null;
            }

        } 
        else
        {
            Debug.Log("Doing attack");
            currentAttackTime += Time.deltaTime;
            float percent = currentAttackTime / attackTime;
            percent = Mathf.Clamp01(percent);
            lastPosition = wiggleArm.hand.position;
            lastDirection = wiggleArm.hand.transform.up.normalized;
            var next = currentAttack.NextPosition(lastPosition, lastDirection, percent);
            LinkAttackToNext(next);
        }
    }

    private void RetractAttack(int targetItems)
    {
        Debug.Log($"Retracting to {targetItems} from {wiggleArm.actualExtensionItems}");
        if (targetItems >= wiggleArm.actualExtensionItems)
        {
            return;
        }
        var newItems = wiggleArm.extension.Take(targetItems).Select(e => e.position);
        wiggleArm.UpdateExtensionLinkage(targetItems, newItems.ToArray(), wiggleArm.extension[targetItems].position);

        Debug.Log($"Retracted list is {wiggleArm.extension}");
    }

    private void LinkAttackToNext(Vector2 next)
    {
        Vector2 diff = next - lastPosition;
        var distance = diff.magnitude;
        var dir = diff.normalized;
        if( distance < wiggleArm.sectionLength )
        {
            return; // Already close enough
        }

        var numSections = Mathf.CeilToInt(distance / wiggleArm.sectionLength);
        Debug.Log($"Linking attack from {lastPosition} to {next} in {numSections}");

        if (finalLength + numSections >= wiggleArm.maxExtensionItems)
        {
            currentAttack.MaxReached();
            numSections = wiggleArm.maxExtensionItems - finalLength;
        }
        finalLength += numSections;

        
        Vector2[] points = new Vector2[numSections];
        for(int i = 0; i < numSections; i++)
        {
            points[i] = lastPosition + dir * wiggleArm.sectionLength * i;
        }
        next = lastPosition + dir * wiggleArm.sectionLength * numSections;

        lastDirection = wiggleArm.hand.transform.up;
        lastPosition = next;

        Vector2[] allPoints = wiggleArm.extension.Select(e => e.position).Concat(points).ToArray();
        wiggleArm.UpdateExtensionLinkage(finalLength, allPoints, next);
        wiggleArm.UpdateLineRenderer();
    }


    public bool LaunchSpecialAttack(ISpecialAttack attack)
    {
        if(currentAttack != null)
        {
            return false;
        }

        currentAttack = attack;
        currentAttackTime = 0.0f;
        attackTime = currentAttack.GetTotalAttackTime();
        retractTime = currentAttack.GetRetractTime();
        finalLength = 0;
        lastPosition = wiggleArm.hand.position;
        lastDirection = wiggleArm.hand.transform.up.normalized;

        currentAttack.StartAttack(lastPosition, lastDirection);
        return true;
    }



    class FistAttack : ISpecialAttack
    {
        float totalDistance = 10;
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
            Debug.Log($"Lerping between {startPosition}  and {targetPosition}, currently at {currentPosition} at percentage {time * 100}");
            this.currentPosition = Vector2.Lerp(startPosition, targetPosition, time );
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
