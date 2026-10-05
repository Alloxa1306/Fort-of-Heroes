public class DummyPlayerConfigProvider : IPlayerConfig
{
    public int HeroHealth => 999;
    public float MoveSpeed => 12f;
    public float FireRate => 0.15f;
    public float DashDistance => 10f;
    public float TurnSpeed => 10f;
}