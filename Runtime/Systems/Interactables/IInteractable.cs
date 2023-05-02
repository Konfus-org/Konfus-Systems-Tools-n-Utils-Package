
namespace Konfus.Utility.Interfaces
{
    public interface IInteractable<in T>
    {
        public void Interact(T t);
    }
}