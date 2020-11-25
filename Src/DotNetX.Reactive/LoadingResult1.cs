using System;

namespace DotNetX.Reactive
{
    public record LoadingResult<TValue, TError>(
        TValue? Value,
        bool HasValue,
        TError? Error,
        bool HasError,
        bool IsLoading)
    {
        public LoadingResult<TValue, TError> Reset() =>
            LoadingResult.Create<TValue, TError>();

        public LoadingResult<TValue, TError> StartLoading() =>
            this with { IsLoading = true };

        public LoadingResult<TValue, TError> SetValue(TValue? value) =>
            this with { IsLoading = false, Value = value, HasValue = true };

        public LoadingResult<TValue, TError> SetError(TError? error) =>
            this with { IsLoading = false, Error = error, HasError = true };

        public LoadingResult<TValue, TError> ResetValue() =>
            this with { Value = default, HasValue = false };

        public LoadingResult<TValue, TError> ResetError() =>
            this with { Error = default, HasError = false };

        public LoadingResult<TNewValue, TError> Bind<TNewValue>(Func<TValue?, LoadingResult<TNewValue, TError>> func) =>
            HasValue 
            ? func(Value) 
            : new LoadingResult<TNewValue, TError>(default, false, Error, HasError, IsLoading);

        public LoadingResult<TNewValue, TError> Map<TNewValue>(Func<TValue?, TNewValue?> func) =>
            HasValue 
            ? new LoadingResult<TNewValue, TError>(func(Value), true, Error, HasError, IsLoading)
            : new LoadingResult<TNewValue, TError>(default, false, Error, HasError, IsLoading);

        public LoadingResult<TValue, TNewError> MapError<TNewError>(Func<TError?, TNewError?> func) =>
            HasValue 
            ? new LoadingResult<TValue, TNewError>(Value, HasValue, func(Error), true, IsLoading)
            : new LoadingResult<TValue, TNewError>(Value, HasValue, default, false, IsLoading);
    }

}
