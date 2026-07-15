using Cysharp.Threading.Tasks;

namespace _Project.Develop.Editing
{
    public interface IEditState
    {
        public UniTask Enter();
        public UniTask Exit();
    }
}