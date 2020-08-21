using System.Threading.Tasks;

namespace DotNetX.Repl
{
    public class ReplBase<TState> : IReplBase
        where TState : class, new()
    {
        public TState State { get; set; }

        public virtual Task<string> Prompt => Task.FromResult("");

        public virtual bool CanPersistState => false;

        public ReplBase()
        {
            this.State = new TState();
        }

        public virtual byte[] PersistState()
        {
            throw new System.NotImplementedException();
        }

        public virtual void LoadState(byte[] persistedState)
        {
            throw new System.NotImplementedException();
        }
    }
}
