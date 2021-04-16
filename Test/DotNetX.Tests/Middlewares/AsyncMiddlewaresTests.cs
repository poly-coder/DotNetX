using DotNetX.Middlewares;
using FluentAssertions;
using FsCheck;
using FsCheck.NUnit;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PropertyAttribute = FsCheck.NUnit.PropertyAttribute;

namespace DotNetX.Tests.Middlewares
{
    [TestFixture]
    public class AsyncMiddlewaresTests
    {
        #region [ Constants ]

        [TestCase(1, "One")]
        public async Task ConstantFuncWithAResultShouldAlwaysReturnTheSameGivenAnyInputAsync(int input, string result)
        {
            // Given
            var middlewareFunc = AsyncMiddleware.ConstantFunc<int, string>(result);

            // When
            var actualResult = await middlewareFunc(input, default(CancellationToken));

            // Then 
            actualResult.Should().Be(result);
        }

        [TestCase(1, "One", "Fake")]
        public async Task ConstantShouldAlwaysReturnTheSameGivenAnyInputAndNextAsync(int input, string result, string otherResult)
        {
            // Given
            var middleware = AsyncMiddleware.Constant<int, string>(result);

            // When
            var actualResult = await middleware(input, (_, _) => Task.FromResult(otherResult), default(CancellationToken));

            // Then 
            actualResult.Should().Be(result);
        }

        #endregion [ Constants ]

        #region [ Combine ]

        [Test]
        public async Task CombineAFuncAndAMiddlewareShouldReturnAFuncThatCombinesThemAsync()
        {
            // Given
            var calls = new List<string>();

            AsyncMiddlewareFunc<string, int> func = async (str, _) =>
            {
                calls.Add("Func");
                await Task.CompletedTask;
                return str.Length;
            };

            AsyncMiddleware<string, int> middleware = async (str, next, ct) =>
            {
                calls.Add("Before");
                var result = await next(str, ct);
                calls.Add("After");
                return result;
            };

            var context = "Hello world!";

            // When
            var result = await func.Combine(middleware)(context, default);

            // Then
            result.Should().Be(context.Length);
            calls.Should().Equal("Before", "Func", "After");
        }

        [Test]
        public async Task CombineAMiddlewareAndAFuncShouldReturnAFuncThatCombinesThemAsync()
        {
            // Given
            var calls = new List<string>();

            AsyncMiddlewareFunc<string, int> func = async (str, _) =>
            {
                calls.Add("Func");
                await Task.CompletedTask;
                return str.Length;
            };

            AsyncMiddleware<string, int> middleware = async (str, next, ct) =>
            {
                calls.Add("Before");
                var result = await next(str, ct);
                calls.Add("After");
                return result;
            };

            var context = "Hello world!";

            // When
            var result = await middleware.Combine(func)(context, default);

            // Then
            result.Should().Be(context.Length);
            calls.Should().Equal("Before", "Func", "After");
        }

        [Test]
        public async Task CombineASimpleFuncAndAMiddlewareShouldReturnAFuncThatCombinesThemAsync()
        {
            // Given
            var calls = new List<string>();

            SimpleAsyncMiddlewareFunc<string, int> func = async str =>
            {
                calls.Add("Func");
                await Task.CompletedTask;
                return str.Length;
            };

            AsyncMiddleware<string, int> middleware = async (str, next, ct) =>
            {
                calls.Add("Before");
                var result = await next(str, ct);
                calls.Add("After");
                return result;
            };

            var context = "Hello world!";

            // When
            var result = await func.Combine(middleware)(context, default);

            // Then
            result.Should().Be(context.Length);
            calls.Should().Equal("Before", "Func", "After");
        }

        [Test]
        public async Task CombineAMiddlewareAndASimpleFuncShouldReturnAFuncThatCombinesThemAsync()
        {
            // Given
            var calls = new List<string>();

            SimpleAsyncMiddlewareFunc<string, int> func = async str =>
            {
                calls.Add("Func");
                await Task.CompletedTask;
                return str.Length;
            };

            AsyncMiddleware<string, int> middleware = async (str, next, ct) =>
            {
                calls.Add("Before");
                var result = await next(str, ct);
                calls.Add("After");
                return result;
            };

            var context = "Hello world!";

            // When
            var result = await middleware.Combine(func)(context, default);

            // Then
            result.Should().Be(context.Length);
            calls.Should().Equal("Before", "Func", "After");
        }

        #endregion [ Combine ]

        #region [ Compose ]

        [Test]
        public async Task ComposingMiddlewaresShouldProducesACompositeMiddlewareAsync()
        {
            // Given
            var calls = new List<string>();

            AsyncMiddlewareFunc<string, int> func = async (str, _) =>
            {
                calls.Add("Func");
                await Task.CompletedTask;
                return str.Length;
            };

            AsyncMiddleware<string, int> MakeMiddleware(int count)
            {
                return async (str, next, ct) =>
                {
                    calls!.Add("Before" + count);
                    var result = await next(str, ct);
                    calls.Add("After" + count);
                    return result;
                };
            }

            var middleware1 = MakeMiddleware(1);
            var middleware2 = MakeMiddleware(2);
            var middleware3 = MakeMiddleware(3);

            var context = "Hello world!";

            // When
            var middleware = middleware1.Compose(middleware2, middleware3);
            var result = await func.Combine(middleware)(context, default);

            // Then
            result.Should().Be(context.Length);
            calls.Should().Equal("Before1", "Before2", "Before3", "Func", "After3", "After2", "After1");
        }

        #endregion [ Compose ]

        #region [ Switch ]

        [TestCase(1)]
        [TestCase(2)]
        public async Task SwitchShouldCallTheMiddlewareThatWasSelected(int input)
        {
            // Given
            var middleware = AsyncMiddleware.Switch<int, int>(
                input =>
                    input switch
                    {
                        var i when i % 2 != 0 => async (input, next, ct) => await next(i + 1, ct),
                        var i => async (input, next, ct) => await next(i, ct),
                    });

            // When
            var func = middleware.Combine(i => Task.FromResult(i));
            var result = await func(input, default);

            // Then 
            (result % 2).Should().Be(0);
        }
        
        [TestCase(1)]
        [TestCase(2)]
        public async Task SwitchShouldCallTheMiddlewareThatWasSelectedAsync(int input)
        {
            // Given
            var middleware = AsyncMiddleware.Switch<int, int>(
                async input =>
                {
                    await Task.CompletedTask;
                    return input switch
                    {
                        var i when i % 2 != 0 => async (input, next, ct) => await next(i + 1, ct),
                        var i => async (input, next, ct) => await next(i, ct),
                    };
                });

            // When
            var func = middleware.Combine(i => Task.FromResult(i));
            var result = await func(input, default);

            // Then 
            (result % 2).Should().Be(0);
        }

        [TestCase(1)]
        [TestCase(2)]
        public async Task SwitchShouldCallTheMiddlewareThatWasSelectedAsyncWithCT(int input)
        {
            // Given
            var middleware = AsyncMiddleware.Switch<int, int>(
                async (input, _cancellationToken) =>
                {
                    await Task.CompletedTask;
                    return input switch
                    {
                        var i when i % 2 != 0 => async (input, next, ct) => await next(i + 1, ct),
                        var i => async (input, next, ct) => await next(i, ct),
                    };
                });

            // When
            var func = middleware.Combine(i => Task.FromResult(i));
            var result = await func(input, default);

            // Then 
            (result % 2).Should().Be(0);
        }

        #endregion [ Switch ]

        #region [ Choose ]

        [TestCase(1)]
        [TestCase(2)]
        public async Task ChooseShouldCallTheMiddlewareThatIsChosen(int input)
        {
            // Given
            var calls = new List<string>();

            var middleware =
                new AsyncMiddleware<int, string>[]
                {
                    // middleware1
                    async (input, next, ct) =>
                    {
                        calls.Add("Before1");
                        if (input % 2 == 1)
                        {
                            var result = await next(input, ct);
                            calls.Add("After1");
                            return result;
                        }
                        return "";
                    },
                    // middleware2
                    async (input, next, ct) =>
                    {
                        calls.Add("Before2");
                        if (input % 2 == 0)
                        {
                            var result = await next(input, ct);
                            calls.Add("After2");
                            return result;
                        }
                        return "";
                    },
                }
                .Choose(
                    wasChosen: (result, _) => !string.IsNullOrEmpty(result),
                    defaultAction: async (input, next, ct) =>
                    {
                        await Task.CompletedTask;
                        calls.Add("Default");
                        return "Default";
                    });

            // When
            var result = await middleware
                .Combine(async (input, ct) =>
                {
                    await Task.CompletedTask;
                    calls.Add("Func");
                    return "Func";
                })
                (input, default);

            // Then
            result.Should().Be("Func");
            if (input % 2 == 1)
            {
                calls.Should().Equal("Before1", "Func", "After1");
            }
            else
            {
                calls.Should().Equal("Before1", "Before2", "Func", "After2");
            }
        }

        [TestCase(1)]
        [TestCase(2)]
        public async Task ChooseShouldCallDefaultActionWhenNoChoiceIsValid(int input)
        {
            // Given
            var calls = new List<string>();

            var middleware =
                new AsyncMiddleware<int, string>[]
                {
                    // middleware1
                    async (input, next, ct) =>
                    {
                        await Task.CompletedTask;
                        calls.Add("Before1");
                        return "";
                    },
                    // middleware2
                    async (input, next, ct) =>
                    {
                        await Task.CompletedTask;
                        calls.Add("Before2");
                        return "";
                    },
                }
                .Choose(
                    wasChosen: (result, _) => !string.IsNullOrEmpty(result),
                    defaultAction: async (input, next, ct) =>
                    {
                        calls.Add("BeforeDefault");
                        var result = await next(input, ct);
                        calls.Add("AfterDefault");
                        return result;
                    });

            // When
            var result = await middleware
                .Combine(async (input, ct) =>
                {
                    await Task.CompletedTask;
                    calls.Add("Func");
                    return "Func";
                })
                (input, default);

            // Then
            result.Should().Be("Func");
            calls.Should().Equal("Before1", "Before2", "BeforeDefault", "Func", "AfterDefault");
        }

        [TestCase(1)]
        [TestCase(2)]
        public async Task ChooseShouldCallTheMiddlewareThatIsChosenAsync(int input)
        {
            // Given
            var calls = new List<string>();

            var middleware =
                new AsyncMiddleware<int, string>[]
                {
                    // middleware1
                    async (input, next, ct) =>
                    {
                        calls.Add("Before1");
                        if (input % 2 == 1)
                        {
                            var result = await next(input, ct);
                            calls.Add("After1");
                            return result;
                        }
                        return "";
                    },
                    // middleware2
                    async (input, next, ct) =>
                    {
                        calls.Add("Before2");
                        if (input % 2 == 0)
                        {
                            var result = await next(input, ct);
                            calls.Add("After2");
                            return result;
                        }
                        return "";
                    },
                }
                .Choose(
                    wasChosen: async (result, _) => !string.IsNullOrEmpty(result),
                    defaultAction: async (input, next, ct) =>
                    {
                        await Task.CompletedTask;
                        calls.Add("Default");
                        return "Default";
                    });

            // When
            var result = await middleware
                .Combine(async (input, ct) =>
                {
                    await Task.CompletedTask;
                    calls.Add("Func");
                    return "Func";
                })
                (input, default);

            // Then
            result.Should().Be("Func");
            if (input % 2 == 1)
            {
                calls.Should().Equal("Before1", "Func", "After1");
            }
            else
            {
                calls.Should().Equal("Before1", "Before2", "Func", "After2");
            }
        }

        [TestCase(1)]
        [TestCase(2)]
        public async Task ChooseShouldCallDefaultActionWhenNoChoiceIsValidAsync(int input)
        {
            // Given
            var calls = new List<string>();

            var middleware =
                new AsyncMiddleware<int, string>[]
                {
                    // middleware1
                    async (input, next, ct) =>
                    {
                        await Task.CompletedTask;
                        calls.Add("Before1");
                        return "";
                    },
                    // middleware2
                    async (input, next, ct) =>
                    {
                        await Task.CompletedTask;
                        calls.Add("Before2");
                        return "";
                    },
                }
                .Choose(
                    wasChosen: async (result, _) => !string.IsNullOrEmpty(result),
                    defaultAction: async (input, next, ct) =>
                    {
                        calls.Add("BeforeDefault");
                        var result = await next(input, ct);
                        calls.Add("AfterDefault");
                        return result;
                    });

            // When
            var result = await middleware
                .Combine(async (input, ct) =>
                {
                    await Task.CompletedTask;
                    calls.Add("Func");
                    return "Func";
                })
                (input, default);

            // Then
            result.Should().Be("Func");
            calls.Should().Equal("Before1", "Before2", "BeforeDefault", "Func", "AfterDefault");
        }

        #endregion [ Choose ]
    }
}
