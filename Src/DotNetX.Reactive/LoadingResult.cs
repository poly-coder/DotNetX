namespace DotNetX.Reactive
{
    public static class LoadingResult
    {
        public static LoadingResult<TValue, TError> Create<TValue, TError>() =>
            new LoadingResult<TValue, TError>(default, false, default, false, false);
    }

}
