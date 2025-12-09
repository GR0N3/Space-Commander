public class SwordAttackState : IBossState
{
    private float timer;
    public void Enter(Boss boss)
    {
        timer = 0f;
        var eq = boss.EventQueue;
        if (eq != null) eq.AddEvent(new SwordAttackCommand(boss));
        else boss.SwordAttack();
    }
    public void Update(Boss boss)
    {
        timer += UnityEngine.Time.deltaTime;
        if (timer >= boss.CooldownBetweenAttacks)
        {
            boss.StateMachine.ChangeState(new ChaseState(), boss);
        }
    }
    public void Exit(Boss boss) { }
}
