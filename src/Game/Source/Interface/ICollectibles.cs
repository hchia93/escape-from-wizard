using Game.Source.GameObject.Dynamic;

namespace Game.Source.Interface
{
    internal interface ICollectibles
    {
        void OnCollect(Player player);
    }

    internal interface IReset
    {
        void Reset();
    }
}
