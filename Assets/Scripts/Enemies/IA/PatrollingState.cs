using UnityEngine;

public class PatrollingState : IEnemyState
{
    public void EnterState(EnemyController enemy)
    {
        enemy.SetMaterial(enemy.greenMaterial);
        enemy.PickRandomPatrolPoint();
        enemy.ScheduleNextRandomRotation();
    }

    public void UpdateState(EnemyController enemy, float deltaTime)
    {
        if (enemy.IsPlayerInSight())
        {
            enemy.SwitchState(new AlertState());
            return;
        }

        if (enemy.ShouldDoRandomRotation())
        {
            enemy.StartRandomRotation();
        }
        if (enemy.UpdateRandomRotation(deltaTime))
        {
            return;
        }

        Transform target = enemy.GetCurrentPatrolTarget();
        if (target != null)
        {
            Vector3 rawDir = target.position - enemy.transform.position;
            rawDir.y = 0;

            Vector3 direction = Vector3.zero;
            if (rawDir.sqrMagnitude > 0.0001f)
            {
                direction = rawDir.normalized;

                Quaternion targetRotation = Quaternion.LookRotation(direction);
                enemy.transform.rotation = Quaternion.Slerp(
                    enemy.transform.rotation,
                    targetRotation,
                    enemy.randomRotationSpeed * deltaTime
                );
            }

            Vector3 newPosition = enemy.transform.position + direction * enemy.patrolSpeed * deltaTime;
            
            if (enemy.restrictToPatrolArea)
            {
                if (enemy.IsPositionInPatrolArea(newPosition))
                {
                    enemy.transform.position = newPosition;
                }
                else
                {
                    enemy.PickRandomPatrolPoint();
                }
            }
            else
            {
                enemy.transform.position = newPosition;
            }

            if (Vector3.Distance(enemy.transform.position, target.position) < 0.5f)
            {
                enemy.PickRandomPatrolPoint();
            }
        }
    }

    public void ExitState(EnemyController enemy) { }
}

