using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISpecialAttack
{
    float GetTotalAttackTime();
 
    void StartAttack(Vector2 startPosition, Vector2 startDirection);
    Vector2 NextPosition(Vector2 currentPosition, Vector2 currentDirection, float time);

    bool AttackIsDone();

    float GetRetractTime();

    void MaxReached();
    void Finished();
}
