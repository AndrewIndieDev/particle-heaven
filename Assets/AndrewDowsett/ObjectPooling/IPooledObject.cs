namespace AndrewDowsett.ObjectPooling
{
    public interface IPooledObject
    {
        public void Spawn();
        public void Despawn();
    }
}