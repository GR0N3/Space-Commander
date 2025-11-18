using UnityEngine;

public class AlertState : IEnemyState
{
    private float rotationSpeed;
    private float alertDuration;
    private float alertTimer;
    private bool hasRotatedToPlayer;
    
    public AlertState() { }
    
    public void EnterState(EnemyController enemy)
    {
        enemy.SetMaterial(enemy.yellowMaterial);
        rotationSpeed = enemy.rotationSpeed;
        alertDuration = enemy.alertDuration;
        alertTimer = 0f;
        hasRotatedToPlayer = false;
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
            Vector3 directionToPlayer = (enemy.player.position - enemy.transform.position).normalized;
            directionToPlayer.y = 0;
            
            float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.player.position);
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            enemy.transform.rotation = Quaternion.Slerp(
                enemy.transform.rotation, 
                targetRotation, 
                rotationSpeed * deltaTime
            );
            if (distanceToPlayer > enemy.stopDistance)
            {
                Vector3 newPosition = enemy.transform.position + directionToPlayer * enemy.chaseSpeed * deltaTime;
                if (enemy.canLeavePatrolAreaToChase || !enemy.restrictToPatrolArea || enemy.IsPositionInPatrolArea(newPosition))
                {
                    enemy.transform.position = newPosition;
                }
            }
            float angleToPlayer = Vector3.Angle(enemy.transform.forward, directionToPlayer);
            if (angleToPlayer < 5f)
            {
                hasRotatedToPlayer = true;
            }
        }
        alertTimer += deltaTime;
        if (hasRotatedToPlayer && alertTimer >= alertDuration)
        {
            enemy.SwitchState(new AttackingState());
        }
    }
    
    public void ExitState(EnemyController enemy) { }
}

