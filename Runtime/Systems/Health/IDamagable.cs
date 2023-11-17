namespace Konfus.Systems.Health
{
    public interface IDamagable : IHasHealth
    {
        void TakeDamage(float damage);
    }
}