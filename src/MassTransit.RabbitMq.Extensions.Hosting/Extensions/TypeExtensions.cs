using System;
using System.Text.RegularExpressions;
using Humanizer;

namespace MassTransit.RabbitMq.Extensions.Hosting.Extensions
{
    /// <summary>
    /// Extensions for <see cref="Type"/>.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Gets the snail_name of the type, including the namespace.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static string GetSnailName(this Type type)
        {
            var name = type.IsInterface && Regex.IsMatch(type.Name, "^I[A-Z]")
                           ? type.Name.Substring(1) // type is interface and looks like ISomeInterface
                           : type.Name;
            var namespaceSnail = type.Namespace.Replace(".", "").Underscore();
            return $"{namespaceSnail}_{name.Underscore()}";
        }
    }
}
