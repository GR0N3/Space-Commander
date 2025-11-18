using UnityEngine;

public class AttackingState : IEnemyState
{
    public void EnterState(EnemyController enemy)
    {
        enemy.SetMaterial(enemy.redMaterial);
    }

    public void UpdateState(EnemyController enemy, float deltaTime)
    {
        if (!enemy.IsPlayerInSight())
        {
            enemy.SwitchState(new PatrollingState());
            return;
        }

        if (enemy.player != null)
        {
            Vector3 direction = (enemy.player.position - enemy.transform.position).normalized;
            direction.y = 0;
            
            float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            enemy.transform.rotation = Quaternion.Slerp(
                enemy.transform.rotation,
                targetRotation,
                enemy.rotationSpeed * deltaTime
            );
            if (distanceToPlayer > enemy.stopDistance)
            {
                Vector3 newPosition = enemy.transform.position + direction * enemy.chaseSpeed * deltaTime;
                if (enemy.canLeavePatrolAreaToChase || !enemy.restrictToPatrolArea || enemy.IsPositionInPatrolArea(newPosition))
                {
                    enemy.transform.position = newPosition;
                }
            }
            enemy.Shoot();
        }
    }

    public void ExitState(EnemyController enemy) { }
}
