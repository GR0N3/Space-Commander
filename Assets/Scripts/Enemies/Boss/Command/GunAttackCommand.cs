public class GunAttackCommand : ICommand
{
    private Boss boss;
    public GunAttackCommand(Boss boss)
    {
        this.boss = boss;
    }
    public void Execute()
    {
        boss.GunAttack();
    }
}
