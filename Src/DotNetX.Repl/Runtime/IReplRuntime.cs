using System.Threading.Tasks;

namespace DotNetX.Repl.Runtime
{
    public interface IReplRuntime
    {
        object CreateEmptyState();
        
        Task<string> GetPrompt(object state);
        void PrintVersion(object state);
        void PrintInformation(object state);
        void PrintHelp(object state);
        void PrintCommandHelp(object state, string command);
        void PrintOptionHelp(object state, string command, string option);
    }
}
