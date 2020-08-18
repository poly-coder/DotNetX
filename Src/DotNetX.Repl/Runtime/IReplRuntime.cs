using System.Threading.Tasks;

namespace DotNetX.Repl.Runtime
{
    public interface IReplRuntime
    {
        Task<string> Prompt { get; }
        void PrintVersion();
        void PrintInformation();
        void PrintHelp();
        void PrintCommandHelp(string command);
        void PrintOptionHelp(string command, string option);
        Task ExecuteAsync(string line);
    }
}
