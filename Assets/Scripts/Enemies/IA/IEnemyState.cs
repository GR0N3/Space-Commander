using UnityEngine;

public interface IEnemyState
{
    void EnterState(EnemyController enemy);
    void UpdateState(EnemyController enemy, float deltaTime);
    void ExitState(EnemyController enemy);
}
