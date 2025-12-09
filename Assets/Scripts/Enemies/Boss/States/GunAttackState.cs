public class GunAttackState : IBossState
{
    private float timer;
    public void Enter(Boss boss)
    {
        timer = 0f;
        var eq = boss.EventQueue;
        if (eq != null) eq.AddEvent(new GunAttackCommand(boss));
        else boss.GunAttack();
    }
    public void Update(Boss boss)
    {
        boss.AimCannonAtPlayer();
        timer += UnityEngine.Time.deltaTime;
        if (timer >= boss.CooldownBetweenAttacks)
        {
            boss.StateMachine.ChangeState(new ChaseState(), boss);
        }
    }
    public void Exit(Boss boss) { }
}
