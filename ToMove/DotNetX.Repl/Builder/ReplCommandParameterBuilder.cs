namespace DotNetX.Repl.Builder
{
    public interface IReplCommandParameterBuilder : INamedBuilder
    {

    }

    public abstract class ReplCommandParameterBuilder<TBuilder> :
        NamedBuilder<TBuilder>,
        IReplCommandParameterBuilder
        where TBuilder : ReplCommandParameterBuilder<TBuilder>
    {
    }
}
