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
    public class TaskMiddlewaresTests
    {
        #region [ Combine ]

        [Test]
        public async Task CombineAFuncAndAMiddlewareShouldReturnAFuncThatCombinesThemTask()
        {
            // Given
            var calls = new List<string>();

            TaskMiddlewareFunc<string> func = async (str, _) =>
            {
                calls.Add($"Func: {str}");
                await Task.CompletedTask;
            };

            TaskMiddleware<string> middleware = async (str, next, ct) =>
            {
                calls.Add($"Before: {str}");
                await next(str, ct);
                calls.Add($"After: {str}");
            };

            var context = "Hello";

            // When
            await func.Combine(middleware)(context, default);

            // Then
            calls.Should().Equal("Before: Hello", "Func: Hello", "After: Hello");
        }

        [Test]
        public async Task CombineAMiddlewareAndAFuncShouldReturnAFuncThatCombinesThemTask()
        {
            // Given
            var calls = new List<string>();

            TaskMiddlewareFunc<string> func = async (str, _) =>
            {
                calls.Add($"Func: {str}");
                await Task.CompletedTask;
            };

            TaskMiddleware<string> middleware = async (str, next, ct) =>
            {
                calls.Add($"Before: {str}");
                await next(str, ct);
                calls.Add($"After: {str}");
            };

            var context = "Hello";

            // When
            await middleware.Combine(func)(context, default);

            // Then
            calls.Should().Equal("Before: Hello", "Func: Hello", "After: Hello");
        }

        [Test]
        public async Task CombineASimpleFuncAndAMiddlewareShouldReturnAFuncThatCombinesThemTask()
        {
            // Given
            var calls = new List<string>();

            SimpleTaskMiddlewareFunc<string> func = async str =>
            {
                calls.Add($"Func: {str}");
                await Task.CompletedTask;
            };

            TaskMiddleware<string> middleware = async (str, next, ct) =>
            {
                calls.Add($"Before: {str}");
                await next(str, ct);
                calls.Add($"After: {str}");
            };

            var context = "Hello";

            // When
            await func.Combine(middleware)(context, default);

            // Then
            calls.Should().Equal("Before: Hello", "Func: Hello", "After: Hello");
        }

        [Test]
        public async Task CombineAMiddlewareAndASimpleFuncShouldReturnAFuncThatCombinesThemTask()
        {
            // Given
            var calls = new List<string>();

            SimpleTaskMiddlewareFunc<string> func = async str =>
            {
                calls.Add($"Func: {str}");
                await Task.CompletedTask;
            };

            TaskMiddleware<string> middleware = async (str, next, ct) =>
            {
                calls.Add($"Before: {str}");
                await next(str, ct);
                calls.Add($"After: {str}");
            };

            var context = "Hello";

            // When
            await middleware.Combine(func)(context, default);

            // Then
            calls.Should().Equal("Before: Hello", "Func: Hello", "After: Hello");
        }

        #endregion [ Combine ]

        #region [ Compose ]

        [Test]
        public async Task ComposingMiddlewaresShouldProducesACompositeMiddlewareTask()
        {
            // Given
            var calls = new List<string>();

            TaskMiddlewareFunc<string> func = async (str, _) =>
            {
                calls.Add("Func");
                await Task.CompletedTask;
            };

            TaskMiddleware<string> MakeMiddleware(int count)
            {
                return async (str, next, ct) =>
                {
                    calls!.Add("Before" + count);
                    await next(str, ct);
                    calls.Add("After" + count);
                };
            }

            var middleware1 = MakeMiddleware(1);
            var middleware2 = MakeMiddleware(2);
            var middleware3 = MakeMiddleware(3);

            var context = "Hello world!";

            // When
            var middleware = middleware1.Compose(middleware2, middleware3);
            await func.Combine(middleware)(context, default);

            // Then
            calls.Should().Equal("Before1", "Before2", "Before3", "Func", "After3", "After2", "After1");
        }

        #endregion [ Compose ]

        #region [ Switch ]

        [TestCase(1)]
        [TestCase(2)]
        public async Task SwitchShouldCallTheMiddlewareThatWasSelected(int input)
        {
            // Given
            var calls = new List<int>();
            var middleware = TaskMiddleware.Switch<int>(
                input =>
                    input switch
                    {
                        var i when i % 2 != 0 => async (input, next, ct) => await next(i + 1, ct),
                        var i => async (input, next, ct) => await next(i, ct),
                    });

            // When
            var func = middleware.Combine(async i => calls.Add(i));
            await func(input, default);

            // Then 
            if (input % 2 != 0)
            {
                calls.Should().Equal(input + 1);
            }
            else
            {
                calls.Should().Equal(input);
            }
        }

        [TestCase(1)]
        [TestCase(2)]
        public async Task SwitchShouldCallTheMiddlewareThatWasSelectedTask(int input)
        {
            // Given
            var calls = new List<int>();
            var middleware = TaskMiddleware.Switch<int>(
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
            var func = middleware.Combine(async i => calls.Add(i));
            await func(input, default);

            // Then 
            if (input % 2 != 0)
            {
                calls.Should().Equal(input + 1);
            }
            else
            {
                calls.Should().Equal(input);
            }
        }

        [TestCase(1)]
        [TestCase(2)]
        public async Task SwitchShouldCallTheMiddlewareThatWasSelectedTaskWithCT(int input)
        {
            // Given
            var calls = new List<int>();
            var middleware = TaskMiddleware.Switch<int>(
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
            var func = middleware.Combine(async i => calls.Add(i));
            await func(input, default);

            // Then 
            if (input % 2 != 0)
            {
                calls.Should().Equal(input + 1);
            }
            else
            {
                calls.Should().Equal(input);
            }
        }

        #endregion [ Switch ]

        #region [ Choose ]

        class IntContext
        {
            public int Value;
            public bool Accepted;
        }

        [TestCase(1)]
        [TestCase(2)]
        public async Task ChooseShouldCallTheMiddlewareThatIsChosen(int input)
        {
            // Given
            var calls = new List<string>();
            var context = new IntContext { Value = input };

            var middleware =
                new TaskMiddleware<IntContext>[]
                {
                    // middleware1
                    async (ctx, next, ct) =>
                    {
                        calls.Add("Before1");
                        if (ctx.Value % 2 == 1)
                        {
                            await next(ctx, ct);
                            calls.Add("After1");
                        }
                    },
                    // middleware2
                    async (ctx, next, ct) =>
                    {
                        calls.Add("Before2");
                        if (ctx.Value % 2 == 0)
                        {
                            await next(ctx, ct);
                            calls.Add("After2");
                        }
                    },
                }
                .Choose(
                    wasChosen: ctx => ctx.Accepted,
                    defaultAction: async (ctx, next, ct) =>
                    {
                        await Task.CompletedTask;
                        calls.Add("Default");
                    });

            // When
            await middleware
                .Combine(async (ctx, ct) =>
                {
                    await Task.CompletedTask;
                    calls.Add("Func");
                    ctx.Accepted = true;
                })
                (context, default);

            // Then
            context.Accepted.Should().BeTrue();
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
            var context = new IntContext { Value = input };

            var middleware =
                new TaskMiddleware<IntContext>[]
                {
                    // middleware1
                    async (ctx, next, ct) =>
                    {
                        calls.Add("Before1");
                    },
                    // middleware2
                    async (ctx, next, ct) =>
                    {
                        calls.Add("Before2");
                    },
                }
                .Choose(
                    wasChosen: ctx => ctx.Accepted,
                    defaultAction: async (ctx, next, ct) =>
                    {
                        await Task.CompletedTask;
                        calls.Add("Default");
                    });

            // When
            await middleware
                .Combine(async (ctx, ct) =>
                {
                    await Task.CompletedTask;
                    calls.Add("Func");
                    ctx.Accepted = true;
                })
                (context, default);

            // Then
            context.Accepted.Should().BeFalse();
            calls.Should().Equal("Before1", "Before2", "Default");
        }

        [TestCase(1)]
        [TestCase(2)]
        public async Task ChooseShouldCallTheMiddlewareThatIsChosenTask(int input)
        {
            // Given
            var calls = new List<string>();
            var context = new IntContext { Value = input };

            var middleware =
                new TaskMiddleware<IntContext>[]
                {
                    // middleware1
                    async (ctx, next, ct) =>
                    {
                        calls.Add("Before1");
                        if (ctx.Value % 2 == 1)
                        {
                            await next(ctx, ct);
                            calls.Add("After1");
                        }
                    },
                    // middleware2
                    async (ctx, next, ct) =>
                    {
                        calls.Add("Before2");
                        if (ctx.Value % 2 == 0)
                        {
                            await next(ctx, ct);
                            calls.Add("After2");
                        }
                    },
                }
                .Choose(
                    wasChosen: async ctx => ctx.Accepted,
                    defaultAction: async (ctx, next, ct) =>
                    {
                        await Task.CompletedTask;
                        calls.Add("Default");
                    });

            // When
            await middleware
                .Combine(async (ctx, ct) =>
                {
                    await Task.CompletedTask;
                    calls.Add("Func");
                    ctx.Accepted = true;
                })
                (context, default);

            // Then
            context.Accepted.Should().BeTrue();
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
        public async Task ChooseShouldCallDefaultActionWhenNoChoiceIsValidTask(int input)
        {
            // Given
            var calls = new List<string>();
            var context = new IntContext { Value = input };

            var middleware =
                new TaskMiddleware<IntContext>[]
                {
                    // middleware1
                    async (ctx, next, ct) =>
                    {
                        calls.Add("Before1");
                    },
                    // middleware2
                    async (ctx, next, ct) =>
                    {
                        calls.Add("Before2");
                    },
                }
                .Choose(
                    wasChosen: async ctx => ctx.Accepted,
                    defaultAction: async (ctx, next, ct) =>
                    {
                        await Task.CompletedTask;
                        calls.Add("Default");
                    });

            // When
            await middleware
                .Combine(async (ctx, ct) =>
                {
                    await Task.CompletedTask;
                    calls.Add("Func");
                    ctx.Accepted = true;
                })
                (context, default);

            // Then
            context.Accepted.Should().BeFalse();
            calls.Should().Equal("Before1", "Before2", "Default");
        }

        #endregion [ Choose ]
    }
}
