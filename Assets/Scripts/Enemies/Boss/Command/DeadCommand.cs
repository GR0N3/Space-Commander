public class DeadCommand : ICommand
{
    private Boss boss;
    public DeadCommand(Boss boss)
    {
        this.boss = boss;
    }
    public void Execute()
    {
        boss.Die();
    }
}
