namespace DotNetX.Repl.Builder
{
    public abstract class ReplCommandParameterBuilder<TBuilder> :
        NamedBuilder<TBuilder>
        where TBuilder : ReplCommandParameterBuilder<TBuilder>
    {
    }
}
