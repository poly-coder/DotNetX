using System.Threading;
using System.Threading.Tasks;

namespace DotNetX.Repl.Runtime
{
    public interface IReplRuntime
    {
        bool CanPersistState { get; }
        byte[] PersistState();
        void LoadState(byte[] persistedState);

        void PrintVersion();
        void PrintInformation();
        void PrintHelp();
        void PrintCommandHelp(string command);
        void PrintOptionHelp(string command, string option);
        
        Task<string> GetPrompt(CancellationToken cancellationToken);
        Task ExecuteAsync(string line, CancellationToken cancellationToken);
    }
}
