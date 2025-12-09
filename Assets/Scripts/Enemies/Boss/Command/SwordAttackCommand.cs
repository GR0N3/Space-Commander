public class SwordAttackCommand : ICommand
{
    private Boss boss;
    public SwordAttackCommand(Boss boss)
    {
        this.boss = boss;
    }
    public void Execute()
    {
        boss.SwordAttack();
    }
}
