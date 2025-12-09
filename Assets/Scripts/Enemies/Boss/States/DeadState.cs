public class DeadState : IBossState
{
    public void Enter(Boss boss)
    {
        boss.Die();
    }
    public void Update(Boss boss) { }
    public void Exit(Boss boss) { }
}
