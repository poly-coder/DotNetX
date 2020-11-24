using System.Reactive;

namespace DotNetX.Reactive
{
    public class Command : Command<Unit>
    {
        public void Call()
        {
            CallWith(Unit.Default);
        }

        public override string ToString()
        {
            return $"Command()";
        }
    }

}
