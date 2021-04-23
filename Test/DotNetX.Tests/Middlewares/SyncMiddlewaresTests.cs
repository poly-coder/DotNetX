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
    public class SyncMiddlewaresTests
    {
        #region [ Constants ]

        [Test]
        public void ConstantFuncWithAResultShouldAlwaysReturnTheSameGivenAnyInput()
        {
            Prop
                .ForAll<int, NonEmptyString>((input, result) =>
                {
                    // Given
                    var middlewareFunc = SyncMiddleware.ConstantFunc<int, string>(result.Get);

                    // When
                    var actualResult = middlewareFunc(input);

                    // Then 
                    actualResult.Should().Be(result.Get);
                })
                .QuickCheckThrowOnFailure();
        }

        [Test]
        public void ConstantShouldAlwaysReturnTheSameGivenAnyInputAndNext()
        {
            Prop
                .ForAll<int, NonEmptyString, string>((input, result, otherResult) =>
                {
                    // Given
                    var middleware = SyncMiddleware.Constant<int, string>(result.Get);

                    // When
                    var actualResult = middleware(input, _ => otherResult);

                    // Then 
                    actualResult.Should().Be(result.Get);
                })
                .QuickCheckThrowOnFailure();
        }

        #endregion [ Constants ]

        #region [ Combine ]

        [Test]
        public void CombineAFuncAndAMiddlewareShouldReturnAFuncThatCombinesThem()
        {
            // Given
            var calls = new List<string>();
            
            SyncMiddlewareFunc<string, int> func = str =>
            {
                calls.Add("Func");
                return str.Length;
            };

            SyncMiddleware<string, int> middleware = (str, next) =>
            {
                calls.Add("Before");
                var result = next(str);
                calls.Add("After");
                return result;
            };

            var context = "Hello world!";

            // When
            var result = func.Combine(middleware)(context);

            // Then
            result.Should().Be(context.Length);
            calls.Should().Equal("Before", "Func", "After");
        }

        [Test]
        public void CombineAMiddlewareAndAFuncShouldReturnAFuncThatCombinesThem()
        {
            // Given
            var calls = new List<string>();

            SyncMiddlewareFunc<string, int> func = str =>
            {
                calls.Add("Func");
                return str.Length;
            };

            SyncMiddleware<string, int> middleware = (str, next) =>
            {
                calls.Add("Before");
                var result = next(str);
                calls.Add("After");
                return result;
            };

            var context = "Hello world!";

            // When
            var result = middleware.Combine(func)(context);

            // Then
            result.Should().Be(context.Length);
            calls.Should().Equal("Before", "Func", "After");
        }

        #endregion [ Combine ]

        #region [ Compose ]

        [Test]
        public void ComposingMiddlewaresShouldProducesACompositeMiddleware()
        {
            // Given
            var calls = new List<string>();
            
            SyncMiddlewareFunc<string, int> func = str =>
            {
                calls.Add("Func");
                return str.Length;
            };

            SyncMiddleware<string, int> MakeMiddleware(int count)
            {
                return (str, next) =>
                {
                    calls!.Add("Before" + count);
                    var result = next(str);
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
            var result = func.Combine(middleware)(context);

            // Then
            result.Should().Be(context.Length);
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
                    var middleware = SyncMiddleware.Switch<int, int>(
                        input =>
                            input switch
                            {
                                var i when i % 2 != 0 => (input, next) => next(i + 1),
                                var i => (input, next) => next(i),
                            });

                    // When
                    var result = middleware.Combine(i => i)(input);

                    // Then 
                    (result % 2).Should().Be(0);
                })
                .QuickCheckThrowOnFailure();
        }

        #endregion [ Switch ]

        #region [ Choose ]

        [Test]
        public void ChooseShouldCallTheMiddlewareThatIsChosen()
        {
            Prop
                .ForAll<PositiveInt>(input =>
                {
                    // Given
                    var calls = new List<string>();

                    var middleware = 
                        new SyncMiddleware<int, string>[]
                        {
                            // middleware1
                            (input, next) =>
                            {
                                calls.Add("Before1");
                                if (input % 2 == 1)
                                {
                                    var result = next(input);
                                    calls.Add("After1");
                                    return result;
                                }
                                return "";
                            },
                            // middleware2
                            (input, next) =>
                            {
                                calls.Add("Before2");
                                if (input % 2 == 0)
                                {
                                    var result = next(input);
                                    calls.Add("After2");
                                    return result;
                                }
                                return "";
                            },
                        }
                        .Choose(
                            wasChosen: (result, _) => !string.IsNullOrEmpty(result),
                            defaultAction: (input, next) =>
                            {
                                calls.Add("Default");
                                return "Default";
                            });

                    // When
                    var result = middleware
                        .Combine(input =>
                        {
                            calls.Add("Func");
                            return "Func";
                        })
                        (input.Get);

                    // Then
                    result.Should().Be("Func");
                    if (input.Get % 2 == 1)
                    {
                        calls.Should().Equal("Before1", "Func", "After1");
                    }
                    else
                    {
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

                    var middleware =
                        new SyncMiddleware<int, string>[]
                        {
                            // middleware1
                            (input, next) =>
                            {
                                calls.Add("Before1");
                                return "";
                            },
                            // middleware2
                            (input, next) =>
                            {
                                calls.Add("Before2");
                                return "";
                            },
                        }
                        .Choose(
                            wasChosen: (result, _) => !string.IsNullOrEmpty(result),
                            defaultAction: (input, next) =>
                            {
                                calls.Add("BeforeDefault");
                                var result = next(input);
                                calls.Add("AfterDefault");
                                return result;
                            });

                    // When
                    var result = middleware
                        .Combine(input =>
                        {
                            calls.Add("Func");
                            return "Func";
                        })
                        (input.Get);

                    // Then
                    result.Should().Be("Func");
                    calls.Should().Equal("Before1", "Before2", "BeforeDefault", "Func", "AfterDefault");
                })
                .QuickCheckThrowOnFailure();
        }

        #endregion [ Choose ]
    }
}
