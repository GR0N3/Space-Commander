public class BossStateMachine
{
    public IBossState Current { get; private set; }
    public void ChangeState(IBossState next, Boss boss)
    {
        Current?.Exit(boss);
        Current = next;
        Current?.Enter(boss);
    }
    public void Update(Boss boss)
    {
        Current?.Update(boss);
    }
}
