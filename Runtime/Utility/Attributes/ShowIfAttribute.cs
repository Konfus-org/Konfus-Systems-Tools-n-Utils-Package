using UnityEngine;

namespace Konfus.Utility.Attributes
{
    /// <summary>
    /// Conditionally Show/Hide field in inspector, based on some other field or property value
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public sealed class ShowIfAttribute : PropertyAttribute
    {
        public readonly string ConditionalSourceField;
        public readonly bool ExpectedValue;

        /// <summary>
        /// Create the attribute for show a field x if field y is true or false.
        /// </summary>
        /// <param name="conditionalSourceField">name of field y type boolean </param>
        /// <param name="expectedValue"> what value should have the field y for show the field x</param>
        public ShowIfAttribute(string conditionalSourceField, bool expectedValue)
        {
            ConditionalSourceField = conditionalSourceField;
            ExpectedValue = expectedValue;
        }
    }
}
