using DotNetX.Reflection;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq.Expressions;

namespace DotNetX
{
    public static class OptionsExtensions
    {
        public static IConfiguration SetOption<TOption, TValue>(
            this IConfiguration configuration,
            TOption options,
            Expression<Func<TOption, TValue>> property,
            Func<string, TValue?> tryParse,
            TValue? defaultValue = null)
            where TValue : struct
        {
            var propertyInfo = property.GetPropertyInfo();

            var propertyName = propertyInfo.Name;

            var optionText = configuration[propertyName];

            if (optionText != null)
            {
                var value = tryParse(optionText);

                if (value.HasValue)
                {
                    propertyInfo.SetValue(options, value.Value);
                    return configuration;
                }
            }

            if (defaultValue.HasValue)
            {
                propertyInfo.SetValue(options, defaultValue.Value);
                return configuration;
            }

            return configuration;
        }

        public static IConfiguration SetOption<TOption, TValue>(
            this IConfiguration configuration,
            TOption options,
            Expression<Func<TOption, TValue>> property,
            Func<string, TValue> tryParse,
            TValue? defaultValue = null)
            where TValue : class
        {
            var propertyInfo = property.GetPropertyInfo();

            var propertyName = propertyInfo.Name;

            var optionText = configuration[propertyName];

            if (optionText != null)
            {
                var value = tryParse(optionText);

                if (value != null)
                {
                    propertyInfo.SetValue(options, value);
                    return configuration;
                }
            }

            if (defaultValue != null)
            {
                propertyInfo.SetValue(options, defaultValue);
                return configuration;
            }

            return configuration;
        }

        public static IConfiguration SetOption<TOption>(
            this IConfiguration configuration,
            TOption options,
            Expression<Func<TOption, string>> property,
            string? defaultValue = null) =>
            configuration.SetOption(options, property, v => v, defaultValue);

        public static IConfiguration SetOption<TOption>(
            this IConfiguration configuration,
            TOption options,
            Expression<Func<TOption, TimeSpan>> property,
            TimeSpan? defaultValue = null) =>
            configuration.SetOption(options, property, s => TimeSpan.TryParse(s, out var v) ? (TimeSpan?)v : null, defaultValue);

        public static IConfiguration SetOption<TOption>(
            this IConfiguration configuration,
            TOption options,
            Expression<Func<TOption, int>> property,
            int? defaultValue = null) =>
            configuration.SetOption(options, property, s => int.TryParse(s, out var v) ? (int?)v : null, defaultValue);
    }
}
