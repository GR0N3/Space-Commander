public class ChaseState : IBossState
{
    public void Enter(Boss boss)
    {
        var anim = boss.Animator;
        if (anim != null && !string.IsNullOrEmpty(boss.IsWalkingParamName)) anim.SetBool(boss.IsWalkingParamName, true);
    }
    public void Update(Boss boss)
    {
        boss.FollowPlayer();
        var player = boss.Player;
        if (player == null) return;
        var dist = UnityEngine.Vector3.Distance(boss.transform.position, player.position);
        if (dist <= boss.AttackRange)
        {
            boss.StateMachine.ChangeState(new SwordAttackState(), boss);
            return;
        }
        if (dist >= boss.GunAttackMinDistance)
        {
            boss.StateMachine.ChangeState(new GunAttackState(), boss);
            return;
        }
    }
    public void Exit(Boss boss) { }
}
