using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpecialAttackManager : MonoBehaviour
{
    WigglyArmV2 wiggleArm;

    ISpecialAttack currentAttack = null;
    float retractTime = 0.0f;
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

        if(currentAttack.AttackIsDone())
        {
            currentRetractTime += Time.deltaTime;
            float percent = Time.deltaTime * currentRetractTime / retractTime;
            int targetItems = (int)((1.0f - percent) * finalLength);
            RetractAttack(targetItems);
        } 
        else
        {
            currentAttackTime += Time.deltaTime;
            var next = currentAttack.NextPosition(lastPosition, lastDirection, currentAttackTime);
            LinkAttackToNext(next);
        }
    }

    private void RetractAttack(int targetItems)
    {
        if(targetItems >= wiggleArm.actualExtensionItems)
        {
            return;
        }
        var newItems = wiggleArm.extension.Take(targetItems).Select(e => e.position);
        wiggleArm.UpdateExtensionLinkage(targetItems, newItems.ToArray(), wiggleArm.extension[targetItems].position);
    }

    private void LinkAttackToNext(Vector2 next)
    {
        var distance = (next - lastPosition).magnitude;
        var numSections = Mathf.CeilToInt(distance / wiggleArm.sectionLength);

        if(finalLength + numSections >= wiggleArm.maxExtensionItems)
        {
            currentAttack.MaxReached();
            numSections = wiggleArm.maxExtensionItems - finalLength;
        }
        finalLength += numSections;

        Vector2[] points = new Vector2[numSections];
        for(int i = 0; i < numSections; i++)
        {
            points[i] = Vector2.Lerp(lastPosition, next, i / numSections);
        }

        lastDirection = wiggleArm.hand.transform.up;
        lastPosition = next;

        wiggleArm.UpdateExtensionLinkage(finalLength, points, next);
    }

    public bool LaunchSpecialAttack(ISpecialAttack attack)
    {
        if(currentAttack != null)
        {
            return false;
        }

        currentAttack = attack;
        currentAttackTime = 0.0f;
        retractTime = currentAttack.GetRetractTime();
        finalLength = 0;

        currentAttack.StartAttack(wiggleArm.hand.position, wiggleArm.hand.transform.up.normalized);
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
            currentPosition = targetPosition;
        }

        public Vector2 NextPosition(Vector2 currentPosition, Vector2 currentDirection, float time)
        {
            return Vector2.Lerp(startPosition, targetPosition, time);
        }

        public void StartAttack(Vector2 startPosition, Vector2 startDirection)
        {
            this.startPosition = startPosition;
            this.targetPosition = startDirection + startDirection * totalDistance;
            currentPosition = startPosition;
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
