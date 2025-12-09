using UnityEngine;

[System.Serializable]
public class BossMemento
{
    public Vector3 position;
    public int health;
    public BossMemento(Vector3 pos, int hp)
    {
        position = pos;
        health = hp;
    }
}
