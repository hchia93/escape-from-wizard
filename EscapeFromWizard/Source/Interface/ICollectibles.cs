using EscapeFromWizard.Source.GameObject.Dynamic;

namespace EscapeFromWizard.Source.Interface
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
