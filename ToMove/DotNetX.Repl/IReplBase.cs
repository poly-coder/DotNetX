using System.Threading.Tasks;

namespace DotNetX.Repl
{
    public interface IReplBase
    {
        Task<string> Prompt { get; }

        bool CanPersistState { get; }

        byte[] PersistState();

        void LoadState(byte[] persistedState);
    }
}
