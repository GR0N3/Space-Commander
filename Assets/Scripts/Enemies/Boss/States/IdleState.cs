using UnityEngine;

public class IdleState : IBossState
{
    public void Enter(Boss boss)
    {
        var anim = boss.Animator;
        if (anim != null && !string.IsNullOrEmpty(boss.IsWalkingParamName)) anim.SetBool(boss.IsWalkingParamName, false);
    }
    public void Update(Boss boss)
    {
        if (boss.Player == null) return;
        var dist = Vector3.Distance(boss.transform.position, boss.Player.position);
        if (dist <= boss.AttackRange)
        {
            boss.StateMachine.ChangeState(new SwordAttackState(), boss);
            return;
        }
        if (dist > boss.StopDistance)
        {
            boss.StateMachine.ChangeState(new ChaseState(), boss);
            return;
        }
        if (dist >= boss.GunAttackMinDistance)
        {
            boss.StateMachine.ChangeState(new GunAttackState(), boss);
        }
    }
    public void Exit(Boss boss) { }
}
