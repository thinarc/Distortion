using Cysharp.Threading.Tasks;

namespace _Project.Develop
{
    public interface IState
    {
        public UniTask Enter();
        public UniTask Exit();
    }
}