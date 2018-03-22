using System;
using System.Collections.Generic;
using System.Linq;

namespace MassTransit.RabbitMq.Extensions.Hosting.Extensions
{
    /// <summary>
    /// Extensions for <see cref="string"/>.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Converts the specified CamelCase string to snail_case.
        /// </summary>
        /// <param name="s">The CamelCased string.</param>
        /// <returns></returns>
        public static string ToSnailCase(this string s) => new string(s.SelectMany(YieldSnailSeparatedCamels).ToArray());

        private static IEnumerable<char> YieldSnailSeparatedCamels(char c, int i) => YieldSeparatedCamels(c, i, '_');

        private static IEnumerable<char> YieldSeparatedCamels(char c, int i, char separator)
        {
            if (char.IsLower(c))
            {
                yield return c;
                yield break;
            }

            var lower = char.ToLower(c);
            if (i > 0)
            {
                yield return separator;
            }

            yield return lower;
        }
    }
}
