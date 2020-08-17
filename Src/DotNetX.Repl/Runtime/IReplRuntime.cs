using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotNetX.Repl.Runtime
{
    public interface IReplRuntime
    {
        object CreateEmptyState();
        Task<string> GetPrompt();
    }
}
