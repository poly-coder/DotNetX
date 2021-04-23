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
using PropertyAttribute = FsCheck.NUnit.PropertyAttribute;

namespace DotNetX.Tests.Middlewares
{
    [TestFixture]
    public class VoidSyncMiddlewaresTests
    {
        #region [ Combine ]

        [Test]
        public void CombineAFuncAndAMiddlewareShouldReturnAFuncThatCombinesThem()
        {
            // Given
            var calls = new List<string>();
            
            VoidSyncMiddlewareFunc<string> func = str =>
            {
                calls.Add($"Func: {str}");
            };

            VoidSyncMiddleware<string> middleware = (str, next) =>
            {
                calls.Add($"Before: {str}");
                next(str);
                calls.Add($"After: {str}");
            };

            var context = "Hello";

            // When
            func.Combine(middleware)(context);

            // Then
            calls.Should().Equal("Before: Hello", "Func: Hello", "After: Hello");
        }

        [Test]
        public void CombineAMiddlewareAndAFuncShouldReturnAFuncThatCombinesThem()
        {
            // Given
            var calls = new List<string>();

            VoidSyncMiddlewareFunc<string> func = str =>
            {
                calls.Add($"Func: {str}");
            };

            VoidSyncMiddleware<string> middleware = (str, next) =>
            {
                calls.Add($"Before: {str}");
                next(str);
                calls.Add($"After: {str}");
            };

            var context = "Hello";

            // When
            middleware.Combine(func)(context);

            // Then
            calls.Should().Equal("Before: Hello", "Func: Hello", "After: Hello");
        }

        #endregion [ Combine ]

        #region [ Compose ]

        [Test]
        public void ComposingMiddlewaresShouldProducesACompositeMiddleware()
        {
            // Given
            var calls = new List<string>();

            VoidSyncMiddlewareFunc<string> func = str =>
            {
                calls.Add("Func");
            };

            VoidSyncMiddleware<string> MakeMiddleware(int count)
            {
                return (str, next) =>
                {
                    calls!.Add("Before" + count);
                    next(str);
                    calls.Add("After" + count);
                };
            }

            var middleware1 = MakeMiddleware(1);
            var middleware2 = MakeMiddleware(2);
            var middleware3 = MakeMiddleware(3);

            var context = "Hello world!";

            // When
            var middleware = middleware1.Compose(middleware2, middleware3);
            func.Combine(middleware)(context);

            // Then
            calls.Should().Equal("Before1", "Before2", "Before3", "Func", "After3", "After2", "After1");
        }

        #endregion [ Compose ]

        #region [ Switch ]

        [Test]
        public void SwitchShouldCallTheMiddlewareThatWasSelected()
        {
            Prop
                .ForAll<int>(input =>
                {
                    // Given
                    var calls = new List<int>();
                    var middleware = VoidSyncMiddleware.Switch<int>(
                        input =>
                            input switch
                            {
                                var i when i % 2 != 0 => (input, next) => next(i + 1),
                                var i => (input, next) => next(i),
                            });

                    // When
                    middleware.Combine(i => calls.Add(i))(input);

                    // Then
                    if (input % 2 != 0)
                    {
                        calls.Should().Equal(input + 1);
                    }
                    else
                    {
                        calls.Should().Equal(input);
                    }
                })
                .QuickCheckThrowOnFailure();
        }

        #endregion [ Switch ]

        #region [ Choose ]

        class IntContext
        {
            public int Value;
            public bool Accepted;
        }

        [Test]
        public void ChooseShouldCallTheMiddlewareThatIsChosen()
        {
            Prop
                .ForAll<PositiveInt>(input =>
                {
                    // Given
                    var calls = new List<string>();
                    var context = new IntContext { Value = input.Get };

                    var middleware =
                        new VoidSyncMiddleware<IntContext>[]
                        {
                            // middleware1
                            (ctx, next) =>
                            {
                                calls.Add("Before1");
                                if (ctx.Value % 2 == 1)
                                {
                                    next(ctx);
                                    calls.Add("After1");
                                }
                            },
                            // middleware2
                            (ctx, next) =>
                            {
                                calls.Add("Before2");
                                if (ctx.Value % 2 == 0)
                                {
                                    next(ctx);
                                    calls.Add("After2");
                                }
                            },
                        }
                        .Choose(
                            wasChosen: ctx => ctx.Accepted,
                            defaultAction: (input, next) =>
                            {
                                calls.Add("Default");
                            });

                    // When
                    middleware
                        .Combine(ctx =>
                        {
                            calls.Add("Func");
                            ctx.Accepted = true;
                        })
                        (context);

                    // Then
                    if (input.Get % 2 == 1)
                    {
                        context.Accepted.Should().BeTrue();
                        calls.Should().Equal("Before1", "Func", "After1");
                    }
                    else
                    {
                        context.Accepted.Should().BeTrue();
                        calls.Should().Equal("Before1", "Before2", "Func", "After2");
                    }
                })
                .QuickCheckThrowOnFailure();
        }

        [Test]
        public void ChooseShouldCallDefaultActionWhenNoChoiceIsValid()
        {
            Prop
                .ForAll<PositiveInt>(input =>
                {
                    // Given
                    var calls = new List<string>();
                    var context = new IntContext { Value = input.Get };

                    var middleware =
                        new VoidSyncMiddleware<IntContext>[]
                        {
                            // middleware1
                            (ctx, next) =>
                            {
                                calls.Add("Before1");
                            },
                            // middleware2
                            (ctx, next) =>
                            {
                                calls.Add("Before2");
                            },
                        }
                        .Choose(
                            wasChosen: ctx => ctx.Accepted,
                            defaultAction: (input, next) =>
                            {
                                calls.Add("Default");
                            });

                    // When
                    middleware
                        .Combine(ctx =>
                        {
                            calls.Add("Func");
                            ctx.Accepted = true;
                        })
                        (context);

                    // Then
                    context.Accepted.Should().BeFalse();
                    calls.Should().Equal("Before1", "Before2", "Default");
                })
                .QuickCheckThrowOnFailure();
        }

        #endregion [ Choose ]
    }
}
