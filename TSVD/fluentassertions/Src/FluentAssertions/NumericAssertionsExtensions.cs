using System;
using FluentAssertions.Execution;
using FluentAssertions.Numeric;

namespace FluentAssertions
{
    /// <summary>
    /// Contains a number of extension methods for floating point <see cref="NumericAssertions{T}"/>.
    /// </summary>
    public static class NumericAssertionsExtensions
    {
        /// <summary>
        /// Asserts an integral value is close to another value within a specified value.
        /// </summary>
        /// <param name="parent">The <see cref="NumericAssertions{T}"/> object that is being extended.</param>
        /// <param name="nearbyValue">
        /// The nearby value to compare the actual value with.
        /// </param>
        /// <param name="delta">
        /// The maximum amount of which the two values may differ.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public static AndConstraint<NumericAssertions<sbyte>> BeCloseTo(this NumericAssertions<sbyte> parent,
            sbyte nearbyValue, byte delta, string because = "",
            params object[] becauseArgs)
        {
            sbyte actualValue = (sbyte)parent.Subject;
            sbyte minValue = (sbyte)(nearbyValue - delta);
            if (minValue > nearbyValue)
            {
                minValue = sbyte.MinValue;
            }

            sbyte maxValue = (sbyte)(nearbyValue + delta);
            if (maxValue < nearbyValue)
            {
                maxValue = sbyte.MaxValue;
            }

            Execute.Assertion
                .ForCondition(minValue <= actualValue && actualValue <= maxValue)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:value} to be within {0} from {1}{reason}, but found {2}.",
                    delta, nearbyValue, actualValue);

            return new AndConstraint<NumericAssertions<sbyte>>(parent);
        }

        /// <summary>
        /// Asserts an integral value is close to another value within a specified value.
        /// </summary>
        /// <param name="parent">The <see cref="NumericAssertions{T}"/> object that is being extended.</param>
        /// <param name="nearbyValue">
        /// The nearby value to compare the actual value with.
        /// </param>
        /// <param name="delta">
        /// The maximum amount of which the two values may differ.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public static AndConstraint<NumericAssertions<byte>> BeCloseTo(this NumericAssertions<byte> parent,
            byte nearbyValue, byte delta, string because = "",
            params object[] becauseArgs)
        {
            byte actualValue = (byte)parent.Subject;
            byte minValue = (byte)(nearbyValue - delta);
            if (minValue > nearbyValue)
            {
                minValue = byte.MinValue;
            }

            byte maxValue = (byte)(nearbyValue + delta);
            if (maxValue < nearbyValue)
            {
                maxValue = byte.MaxValue;
            }

            Execute.Assertion
                .ForCondition(minValue <= actualValue && actualValue <= maxValue)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:value} to be within {0} from {1}{reason}, but found {2}.",
                    delta, nearbyValue, actualValue);

            return new AndConstraint<NumericAssertions<byte>>(parent);
        }

        /// <summary>
        /// Asserts an integral value is close to another value within a specified value.
        /// </summary>
        /// <param name="parent">The <see cref="NumericAssertions{T}"/> object that is being extended.</param>
        /// <param name="nearbyValue">
        /// The nearby value to compare the actual value with.
        /// </param>
        /// <param name="delta">
        /// The maximum amount of which the two values may differ.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public static AndConstraint<NumericAssertions<short>> BeCloseTo(this NumericAssertions<short> parent,
            short nearbyValue, ushort delta, string because = "",
            params object[] becauseArgs)
        {
            short actualValue = (short)parent.Subject;
            short minValue = (short)(nearbyValue - delta);
            if (minValue > nearbyValue)
            {
                minValue = short.MinValue;
            }

            short maxValue = (short)(nearbyValue + delta);
            if (maxValue < nearbyValue)
            {
                maxValue = short.MaxValue;
            }

            Execute.Assertion
                .ForCondition(minValue <= actualValue && actualValue <= maxValue)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:value} to be within {0} from {1}{reason}, but found {2}.",
                    delta, nearbyValue, actualValue);

            return new AndConstraint<NumericAssertions<short>>(parent);
        }

        /// <summary>
        /// Asserts an integral value is close to another value within a specified value.
        /// </summary>
        /// <param name="parent">The <see cref="NumericAssertions{T}"/> object that is being extended.</param>
        /// <param name="nearbyValue">
        /// The nearby value to compare the actual value with.
        /// </param>
        /// <param name="delta">
        /// The maximum amount of which the two values may differ.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public static AndConstraint<NumericAssertions<ushort>> BeCloseTo(this NumericAssertions<ushort> parent,
            ushort nearbyValue, ushort delta, string because = "",
            params object[] becauseArgs)
        {
            ushort actualValue = (ushort)parent.Subject;
            ushort minValue = (ushort)(nearbyValue - delta);
            if (minValue > nearbyValue)
            {
                minValue = ushort.MinValue;
            }

            ushort maxValue = (ushort)(nearbyValue + delta);
            if (maxValue < nearbyValue)
            {
                maxValue = ushort.MaxValue;
            }

            Execute.Assertion
                .ForCondition(minValue <= actualValue && actualValue <= maxValue)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:value} to be within {0} from {1}{reason}, but found {2}.",
                    delta, nearbyValue, actualValue);

            return new AndConstraint<NumericAssertions<ushort>>(parent);
        }

        /// <summary>
        /// Asserts an integral value is close to another value within a specified value.
        /// </summary>
        /// <param name="parent">The <see cref="NumericAssertions{T}"/> object that is being extended.</param>
        /// <param name="nearbyValue">
        /// The nearby value to compare the actual value with.
        /// </param>
        /// <param name="delta">
        /// The maximum amount of which the two values may differ.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public static AndConstraint<NumericAssertions<int>> BeCloseTo(this NumericAssertions<int> parent,
            int nearbyValue, uint delta, string because = "",
            params object[] becauseArgs)
        {
            int actualValue = (int)parent.Subject;
            int minValue = (int)(nearbyValue - delta);
            if (minValue > nearbyValue)
            {
                minValue = int.MinValue;
            }

            int maxValue = (int)(nearbyValue + delta);
            if (maxValue < nearbyValue)
            {
                maxValue = int.MaxValue;
            }

            Execute.Assertion
                .ForCondition(minValue <= actualValue && actualValue <= maxValue)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:value} to be within {0} from {1}{reason}, but found {2}.",
                    delta, nearbyValue, actualValue);

            return new AndConstraint<NumericAssertions<int>>(parent);
        }

        /// <summary>
        /// Asserts an integral value is close to another value within a specified value.
        /// </summary>
        /// <param name="parent">The <see cref="NumericAssertions{T}"/> object that is being extended.</param>
        /// <param name="nearbyValue">
        /// The nearby value to compare the actual value with.
        /// </param>
        /// <param name="delta">
        /// The maximum amount of which the two values may differ.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public static AndConstraint<NumericAssertions<uint>> BeCloseTo(this NumericAssertions<uint> parent,
            uint nearbyValue, uint delta, string because = "",
            params object[] becauseArgs)
        {
            uint actualValue = (uint)parent.Subject;
            uint minValue = nearbyValue - delta;
            if (minValue > nearbyValue)
            {
                minValue = uint.MinValue;
            }

            uint maxValue = nearbyValue + delta;
            if (maxValue < nearbyValue)
            {
                maxValue = uint.MaxValue;
            }

            Execute.Assertion
                .ForCondition(minValue <= actualValue && actualValue <= maxValue)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:value} to be within {0} from {1}{reason}, but found {2}.",
                    delta, nearbyValue, actualValue);

            return new AndConstraint<NumericAssertions<uint>>(parent);
        }

        /// <summary>
        /// Asserts an integral value is close to another value within a specified value.
        /// </summary>
        /// <param name="parent">The <see cref="NumericAssertions{T}"/> object that is being extended.</param>
        /// <param name="nearbyValue">
        /// The nearby value to compare the actual value with.
        /// </param>
        /// <param name="delta">
        /// The maximum amount of which the two values may differ.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public static AndConstraint<NumericAssertions<long>> BeCloseTo(this NumericAssertions<long> parent,
            long nearbyValue, ulong delta, string because = "",
            params object[] becauseArgs)
        {
            long actualValue = (long)parent.Subject;
            long minValue = GetMinValue(nearbyValue, delta);
            long maxValue = GetMaxValue(nearbyValue, delta);

            Execute.Assertion
                .ForCondition(minValue <= actualValue && actualValue <= maxValue)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:value} to be within {0} from {1}{reason}, but found {2}.",
                    delta, nearbyValue, actualValue);

            return new AndConstraint<NumericAssertions<long>>(parent);
        }

        /// <summary>
        /// Asserts an integral value is close to another value within a specified value.
        /// </summary>
        /// <param name="parent">The <see cref="NumericAssertions{T}"/> object that is being extended.</param>
        /// <param name="nearbyValue">
        /// The nearby value to compare the actual value with.
        /// </param>
        /// <param name="delta">
        /// The maximum amount of which the two values may differ.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public static AndConstraint<NumericAssertions<ulong>> BeCloseTo(this NumericAssertions<ulong> parent,
            ulong nearbyValue, ulong delta, string because = "",
            params object[] becauseArgs)
        {
            ulong actualValue = (ulong)parent.Subject;
            ulong minValue = nearbyValue - delta;
            if (minValue > nearbyValue)
            {
                minValue = ulong.MinValue;
            }

            ulong maxValue = nearbyValue + delta;
            if (maxValue < nearbyValue)
            {
                maxValue = ulong.MaxValue;
            }

            Execute.Assertion
                .ForCondition(minValue <= actualValue && actualValue <= maxValue)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:value} to be within {0} from {1}{reason}, but found {2}.",
                    delta, nearbyValue, actualValue);

            return new AndConstraint<NumericAssertions<ulong>>(parent);
        }

        /// <summary>
        /// Asserts an integral value is not within another value by a specified value.
        /// </summary>
        /// <param name="parent">The <see cref="NumericAssertions{T}"/> object that is being extended.</param>
        /// <param name="distantValue">
        /// The nearby value to compare the actual value with.
        /// </param>
        /// <param name="delta">
        /// The maximum amount of which the two values may differ.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public static AndConstraint<NumericAssertions<sbyte>> NotBeCloseTo(this NumericAssertions<sbyte> parent,
            sbyte distantValue, byte delta, string because = "",
            params object[] becauseArgs)
        {
            sbyte actualValue = (sbyte)parent.Subject;
            sbyte minValue = (sbyte)(distantValue - delta);
            if (minValue > distantValue)
            {
                minValue = sbyte.MinValue;
            }

            sbyte maxValue = (sbyte)(distantValue + delta);
            if (maxValue < distantValue)
            {
                maxValue = sbyte.MaxValue;
            }

            Execute.Assertion
                .ForCondition(!(minValue <= actualValue && actualValue <= maxValue))
                .BecauseOf(because, becauseArgs)
                .FailWith("Did not expect {context:value} to be within {0} from {1}{reason}, but found {2}.",
                    delta, distantValue, actualValue);

            return new AndConstraint<NumericAssertions<sbyte>>(parent);
        }

        /// <summary>
        /// Asserts an integral value is not within another value by a specified value.
        /// </summary>
        /// <param name="parent">The <see cref="NumericAssertions{T}"/> object that is being extended.</param>
        /// <param name="distantValue">
        /// The nearby value to compare the actual value with.
        /// </param>
        /// <param name="delta">
        /// The maximum amount of which the two values may differ.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public static AndConstraint<NumericAssertions<byte>> NotBeCloseTo(this NumericAssertions<byte> parent,
            byte distantValue, byte delta, string because = "",
            params object[] becauseArgs)
        {
            byte actualValue = (byte)parent.Subject;
            byte minValue = (byte)(distantValue - delta);
            if (minValue > distantValue)
            {
                minValue = byte.MinValue;
            }

            byte maxValue = (byte)(distantValue + delta);
            if (maxValue < distantValue)
            {
                maxValue = byte.MaxValue;
            }

            Execute.Assertion
                .ForCondition(!(minValue <= actualValue && actualValue <= maxValue))
                .BecauseOf(because, becauseArgs)
                .FailWith("Did not expect {context:value} to be within {0} from {1}{reason}, but found {2}.",
                    delta, distantValue, actualValue);

            return new AndConstraint<NumericAssertions<byte>>(parent);
        }

        /// <summary>
        /// Asserts an integral value is not within another value by a specified value.
        /// </summary>
        /// <param name="parent">The <see cref="NumericAssertions{T}"/> object that is being extended.</param>
        /// <param name="distantValue">
        /// The nearby value to compare the actual value with.
        /// </param>
        /// <param name="delta">
        /// The maximum amount of which the two values may differ.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public static AndConstraint<NumericAssertions<short>> NotBeCloseTo(this NumericAssertions<short> parent,
            short distantValue, ushort delta, string because = "",
            params object[] becauseArgs)
        {
            short actualValue = (short)parent.Subject;
            short minValue = (short)(distantValue - delta);
            if (minValue > distantValue)
            {
                minValue = short.MinValue;
            }

            short maxValue = (short)(distantValue + delta);
            if (maxValue < distantValue)
            {
                maxValue = short.MaxValue;
            }

            Execute.Assertion
                .ForCondition(!(minValue <= actualValue && actualValue <= maxValue))
                .BecauseOf(because, becauseArgs)
                .FailWith("Did not expect {context:value} to be within {0} from {1}{reason}, but found {2}.",
                    delta, distantValue, actualValue);

            return new AndConstraint<NumericAssertions<short>>(parent);
        }

        /// <summary>
        /// Asserts an integral value is not within another value by a specified value.
        /// </summary>
        /// <param name="parent">The <see cref="NumericAssertions{T}"/> object that is being extended.</param>
        /// <param name="distantValue">
        /// The nearby value to compare the actual value with.
        /// </param>
        /// <param name="delta">
        /// The maximum amount of which the two values may differ.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public static AndConstraint<NumericAssertions<ushort>> NotBeCloseTo(this NumericAssertions<ushort> parent,
            ushort distantValue, ushort delta, string because = "",
            params object[] becauseArgs)
        {
            ushort actualValue = (ushort)parent.Subject;
            ushort minValue = (ushort)(distantValue - delta);
            if (minValue > distantValue)
            {
                minValue = ushort.MinValue;
            }

            ushort maxValue = (ushort)(distantValue + delta);
            if (maxValue < distantValue)
            {
                maxValue = ushort.MaxValue;
            }

            Execute.Assertion
                .ForCondition(!(minValue <= actualValue && actualValue <= maxValue))
                .BecauseOf(because, becauseArgs)
                .FailWith("Did not expect {context:value} to be within {0} from {1}{reason}, but found {2}.",
                    delta, distantValue, actualValue);

            return new AndConstraint<NumericAssertions<ushort>>(parent);
        }

        /// <summary>
        /// Asserts an integral value is not within another value by a specified value.
        /// </summary>
        /// <param name="parent">The <see cref="NumericAssertions{T}"/> object that is being extended.</param>
        /// <param name="distantValue">
        /// The nearby value to compare the actual value with.
        /// </param>
        /// <param name="delta">
        /// The maximum amount of which the two values may differ.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public static AndConstraint<NumericAssertions<int>> NotBeCloseTo(this NumericAssertions<int> parent,
            int distantValue, uint delta, string because = "",
            params object[] becauseArgs)
        {
            int actualValue = (int)parent.Subject;
            int minValue = (int)(distantValue - delta);
            if (minValue > distantValue)
            {
                minValue = int.MinValue;
            }

            int maxValue = (int)(distantValue + delta);
            if (maxValue < distantValue)
            {
                maxValue = int.MaxValue;
            }

            Execute.Assertion
                .ForCondition(!(minValue <= actualValue && actualValue <= maxValue))
                .BecauseOf(because, becauseArgs)
                .FailWith("Did not expect {context:value} to be within {0} from {1}{reason}, but found {2}.",
                    delta, distantValue, actualValue);

            return new AndConstraint<NumericAssertions<int>>(parent);
        }

        /// <summary>
        /// Asserts an integral value is not within another value by a specified value.
        /// </summary>
        /// <param name="parent">The <see cref="NumericAssertions{T}"/> object that is being extended.</param>
        /// <param name="distantValue">
        /// The nearby value to compare the actual value with.
        /// </param>
        /// <param name="delta">
        /// The maximum amount of which the two values may differ.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public static AndConstraint<NumericAssertions<uint>> NotBeCloseTo(this NumericAssertions<uint> parent,
            uint distantValue, uint delta, string because = "",
            params object[] becauseArgs)
        {
            uint actualValue = (uint)parent.Subject;
            uint minValue = distantValue - delta;
            if (minValue > distantValue)
            {
                minValue = uint.MinValue;
            }

            uint maxValue = distantValue + delta;
            if (maxValue < distantValue)
            {
                maxValue = uint.MaxValue;
            }

            Execute.Assertion
                .ForCondition(!(minValue <= actualValue && actualValue <= maxValue))
                .BecauseOf(because, becauseArgs)
                .FailWith("Did not expect {context:value} to be within {0} from {1}{reason}, but found {2}.",
                    delta, distantValue, actualValue);

            return new AndConstraint<NumericAssertions<uint>>(parent);
        }

        /// <summary>
        /// Asserts an integral value is not within another value by a specified value.
        /// </summary>
        /// <param name="parent">The <see cref="NumericAssertions{T}"/> object that is being extended.</param>
        /// <param name="distantValue">
        /// The nearby value to compare the actual value with.
        /// </param>
        /// <param name="delta">
        /// The maximum amount of which the two values may differ.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public static AndConstraint<NumericAssertions<long>> NotBeCloseTo(this NumericAssertions<long> parent,
            long distantValue, ulong delta, string because = "",
            params object[] becauseArgs)
        {
            long actualValue = (long)parent.Subject;
            long minValue = GetMinValue(distantValue, delta);
            long maxValue = GetMaxValue(distantValue, delta);

            Execute.Assertion
                .ForCondition(!(minValue <= actualValue && actualValue <= maxValue))
                .BecauseOf(because, becauseArgs)
                .FailWith("Did not expect {context:value} to be within {0} from {1}{reason}, but found {2}.",
                    delta, distantValue, actualValue);

            return new AndConstraint<NumericAssertions<long>>(parent);
        }

        /// <summary>
        /// Asserts an integral value is not within another value by a specified value.
        /// </summary>
        /// <param name="parent">The <see cref="NumericAssertions{T}"/> object that is being extended.</param>
        /// <param name="distantValue">
        /// The nearby value to compare the actual value with.
        /// </param>
        /// <param name="delta">
        /// The maximum amount of which the two values may differ.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public static AndConstraint<NumericAssertions<ulong>> NotBeCloseTo(this NumericAssertions<ulong> parent,
            ulong distantValue, ulong delta, string because = "",
            params object[] becauseArgs)
        {
            ulong actualValue = (ulong)parent.Subject;
            ulong minValue = distantValue - delta;
            if (minValue > distantValue)
            {
                minValue = ulong.MinValue;
            }

            ulong maxValue = distantValue + delta;
            if (maxValue < distantValue)
            {
                maxValue = ulong.MaxValue;
            }

            Execute.Assertion
                .ForCondition(!(minValue <= actualValue && actualValue <= maxValue))
                .BecauseOf(because, becauseArgs)
                .FailWith("Did not expect {context:value} to be within {0} from {1}{reason}, but found {2}.",
                    delta, distantValue, actualValue);

            return new AndConstraint<NumericAssertions<ulong>>(parent);
        }

        /// <summary>
        /// Asserts a floating point value approximates another value as close as possible.
        /// </summary>
        /// <param name="parent">The <see cref="NumericAssertions{T}"/> object that is being extended.</param>
        /// <param name="expectedValue">
        /// The expected value to compare the actual value with.
        /// </param>
        /// <param name="precision">
        /// The maximum amount of which the two values may differ.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public static AndConstraint<NullableNumericAssertions<float>> BeApproximately(this NullableNumericAssertions<float> parent,
            float expectedValue, float precision, string because = "",
            params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(parent.Subject != null)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:value} to approximate {0} +/- {1}{reason}, but it was <null>.", expectedValue, precision);

            var nonNullableAssertions = new NumericAssertions<float>((float)parent.Subject);
            nonNullableAssertions.BeApproximately(expectedValue, precision, because, becauseArgs);

            return new AndConstraint<NullableNumericAssertions<float>>(parent);
        }

        /// <summary>
        /// Asserts a floating point value approximates another value as close as possible.
        /// </summary>
        /// <param name="parent">The <see cref="NumericAssertions{T}"/> object that is being extended.</param>
        /// <param name="expectedValue">
        /// The expected value to compare the actual value with.
        /// </param>
        /// <param name="precision">
        /// The maximum amount of which the two values may differ.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public static AndConstraint<NumericAssertions<float>> BeApproximately(this NumericAssertions<float> parent,
            float expectedValue, float precision, string because = "",
            params object[] becauseArgs)
        {
            float actualDifference = Math.Abs(expectedValue - (float)parent.Subject);

            Execute.Assertion
                .ForCondition(actualDifference <= precision)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:value} to approximate {1} +/- {2}{reason}, but {0} differed by {3}.",
                    parent.Subject, expectedValue, precision, actualDifference);

            return new AndConstraint<NumericAssertions<float>>(parent);
        }

        /// <summary>
        /// Asserts a double value approximates another value as close as possible.
        /// </summary>
        /// <param name="parent">The <see cref="NumericAssertions{T}"/> object that is being extended.</param>
        /// <param name="expectedValue">
        /// The expected value to compare the actual value with.
        /// </param>
        /// <param name="precision">
        /// The maximum amount of which the two values may differ.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public static AndConstraint<NullableNumericAssertions<double>> BeApproximately(this NullableNumericAssertions<double> parent,
            double expectedValue, double precision, string because = "",
            params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(parent.Subject != null)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:value} to approximate {0} +/- {1}{reason}, but it was <null>.", expectedValue, precision);

            var nonNullableAssertions = new NumericAssertions<double>((double)parent.Subject);
            BeApproximately(nonNullableAssertions, expectedValue, precision, because, becauseArgs);

            return new AndConstraint<NullableNumericAssertions<double>>(parent);
        }

        /// <summary>
        /// Asserts a double value approximates another value as close as possible.
        /// </summary>
        /// <param name="parent">The <see cref="NumericAssertions{T}"/> object that is being extended.</param>
        /// <param name="expectedValue">
        /// The expected value to compare the actual value with.
        /// </param>
        /// <param name="precision">
        /// The maximum amount of which the two values may differ.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public static AndConstraint<NumericAssertions<double>> BeApproximately(this NumericAssertions<double> parent,
            double expectedValue, double precision, string because = "",
            params object[] becauseArgs)
        {
            double actualDifference = Math.Abs(expectedValue - (double)parent.Subject);

            Execute.Assertion
                .ForCondition(actualDifference <= precision)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:value} to approximate {1} +/- {2}{reason}, but {0} differed by {3}.",
                    parent.Subject, expectedValue, precision, actualDifference);

            return new AndConstraint<NumericAssertions<double>>(parent);
        }

        /// <summary>
        /// Asserts a decimal value approximates another value as close as possible.
        /// </summary>
        /// <param name="parent">The <see cref="NumericAssertions{T}"/> object that is being extended.</param>
        /// <param name="expectedValue">
        /// The expected value to compare the actual value with.
        /// </param>
        /// <param name="precision">
        /// The maximum amount of which the two values may differ.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public static AndConstraint<NullableNumericAssertions<decimal>> BeApproximately(this NullableNumericAssertions<decimal> parent,
            decimal expectedValue, decimal precision, string because = "",
            params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(parent.Subject != null)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:value} to approximate {0} +/- {1}{reason}, but it was <null>.", expectedValue, precision);

            var nonNullableAssertions = new NumericAssertions<decimal>((decimal)parent.Subject);
            BeApproximately(nonNullableAssertions, expectedValue, precision, because, becauseArgs);

            return new AndConstraint<NullableNumericAssertions<decimal>>(parent);
        }

        /// <summary>
        /// Asserts a decimal value approximates another value as close as possible.
        /// </summary>
        /// <param name="parent">The <see cref="NumericAssertions{T}"/> object that is being extended.</param>
        /// <param name="expectedValue">
        /// The expected value to compare the actual value with.
        /// </param>
        /// <param name="precision">
        /// The maximum amount of which the two values may differ.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public static AndConstraint<NumericAssertions<decimal>> BeApproximately(this NumericAssertions<decimal> parent,
            decimal expectedValue, decimal precision, string because = "",
            params object[] becauseArgs)
        {
            decimal actualDifference = Math.Abs(expectedValue - (decimal)parent.Subject);

            Execute.Assertion
                .ForCondition(actualDifference <= precision)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:value} to approximate {1} +/- {2}{reason}, but {0} differed by {3}.",
                    parent.Subject, expectedValue, precision, actualDifference);

            return new AndConstraint<NumericAssertions<decimal>>(parent);
        }

        /// <summary>
        /// Asserts a floating point value does not approximate another value by a given amount.
        /// </summary>
        /// <param name="parent">The <see cref="NumericAssertions{T}"/> object that is being extended.</param>
        /// <param name="unexpectedValue">
        /// The unexpected value to compare the actual value with.
        /// </param>
        /// <param name="precision">
        /// The minimum exclusive amount of which the two values should differ.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public static AndConstraint<NullableNumericAssertions<float>> NotBeApproximately(this NullableNumericAssertions<float> parent,
            float unexpectedValue, float precision, string because = "",
            params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(parent.Subject != null)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:value} to not approximate {0} +/- {1}{reason}, but it was <null>.", unexpectedValue, precision);

            var nonNullableAssertions = new NumericAssertions<float>((float)parent.Subject);
            nonNullableAssertions.NotBeApproximately(unexpectedValue, precision, because, becauseArgs);

            return new AndConstraint<NullableNumericAssertions<float>>(parent);
        }

        /// <summary>
        /// Asserts a floating point value does not approximate another value by a given amount.
        /// </summary>
        /// <param name="parent">The <see cref="NumericAssertions{T}"/> object that is being extended.</param>
        /// <param name="unexpectedValue">
        /// The unexpected value to compare the actual value with.
        /// </param>
        /// <param name="precision">
        /// The minimum exclusive amount of which the two values should differ.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public static AndConstraint<NumericAssertions<float>> NotBeApproximately(this NumericAssertions<float> parent,
            float unexpectedValue, float precision, string because = "",
            params object[] becauseArgs)
        {
            float actualDifference = Math.Abs(unexpectedValue - (float)parent.Subject);

            Execute.Assertion
                .ForCondition(actualDifference > precision)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:value} to not approximate {1} +/- {2}{reason}, but {0} only differed by {3}.",
                    parent.Subject, unexpectedValue, precision, actualDifference);

            return new AndConstraint<NumericAssertions<float>>(parent);
        }

        /// <summary>
        /// Asserts a double value does not approximate another value by a given amount.
        /// </summary>
        /// <param name="parent">The <see cref="NumericAssertions{T}"/> object that is being extended.</param>
        /// <param name="unexpectedValue">
        /// The unexpected value to compare the actual value with.
        /// </param>
        /// <param name="precision">
        /// The minimum exclusive amount of which the two values should differ.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public static AndConstraint<NullableNumericAssertions<double>> NotBeApproximately(this NullableNumericAssertions<double> parent,
            double unexpectedValue, double precision, string because = "",
            params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(parent.Subject != null)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:value} to not approximate {0} +/- {1}{reason}, but it was <null>.", unexpectedValue, precision);

            var nonNullableAssertions = new NumericAssertions<double>((double)parent.Subject);
            NotBeApproximately(nonNullableAssertions, unexpectedValue, precision, because, becauseArgs);

            return new AndConstraint<NullableNumericAssertions<double>>(parent);
        }

        /// <summary>
        /// Asserts a double value does not approximate another value by a given amount.
        /// </summary>
        /// <param name="parent">The <see cref="NumericAssertions{T}"/> object that is being extended.</param>
        /// <param name="unexpectedValue">
        /// The unexpected value to compare the actual value with.
        /// </param>
        /// <param name="precision">
        /// The minimum exclusive amount of which the two values should differ.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public static AndConstraint<NumericAssertions<double>> NotBeApproximately(this NumericAssertions<double> parent,
            double unexpectedValue, double precision, string because = "",
            params object[] becauseArgs)
        {
            double actualDifference = Math.Abs(unexpectedValue - (double)parent.Subject);

            Execute.Assertion
                .ForCondition(actualDifference > precision)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:value} to not approximate {1} +/- {2}{reason}, but {0} only differed by {3}.",
                    parent.Subject, unexpectedValue, precision, actualDifference);

            return new AndConstraint<NumericAssertions<double>>(parent);
        }

        /// <summary>
        /// Asserts a decimal value does not approximate another value by a given amount.
        /// </summary>
        /// <param name="parent">The <see cref="NumericAssertions{T}"/> object that is being extended.</param>
        /// <param name="unexpectedValue">
        /// The unexpected value to compare the actual value with.
        /// </param>
        /// <param name="precision">
        /// The minimum exclusive amount of which the two values should differ.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public static AndConstraint<NullableNumericAssertions<decimal>> NotBeApproximately(this NullableNumericAssertions<decimal> parent,
            decimal unexpectedValue, decimal precision, string because = "",
            params object[] becauseArgs)
        {
            Execute.Assertion
                .ForCondition(parent.Subject != null)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:value} to not approximate {0} +/- {1}{reason}, but it was <null>.", unexpectedValue, precision);

            var nonNullableAssertions = new NumericAssertions<decimal>((decimal)parent.Subject);
            NotBeApproximately(nonNullableAssertions, unexpectedValue, precision, because, becauseArgs);

            return new AndConstraint<NullableNumericAssertions<decimal>>(parent);
        }

        /// <summary>
        /// Asserts a decimal value does not approximate another value by a given amount.
        /// </summary>
        /// <param name="parent">The <see cref="NumericAssertions{T}"/> object that is being extended.</param>
        /// <param name="unexpectedValue">
        /// The unexpected value to compare the actual value with.
        /// </param>
        /// <param name="precision">
        /// The minimum exclusive amount of which the two values should differ.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <see cref="because"/>.
        /// </param>
        public static AndConstraint<NumericAssertions<decimal>> NotBeApproximately(this NumericAssertions<decimal> parent,
            decimal unexpectedValue, decimal precision, string because = "",
            params object[] becauseArgs)
        {
            decimal actualDifference = Math.Abs(unexpectedValue - (decimal)parent.Subject);

            Execute.Assertion
                .ForCondition(actualDifference > precision)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:value} to not approximate {1} +/- {2}{reason}, but {0} only differed by {3}.",
                    parent.Subject, unexpectedValue, precision, actualDifference);

            return new AndConstraint<NumericAssertions<decimal>>(parent);
        }

        private static long GetMinValue(long value, ulong delta)
        {
            long minValue = (delta <= ulong.MaxValue / 2)
                            ? (value - (long)delta)
                            : ((value < 0)
                                ? long.MinValue
                                : (-(long)(delta - (ulong)value)));

            if (minValue > value)
            {
                minValue = long.MinValue;
            }

            return minValue;
        }

        private static long GetMaxValue(long value, ulong delta)
        {
            long maxValue = (delta <= ulong.MaxValue / 2)
                ? (value + (long)delta)
                : ((value >= 0)
                    ? long.MaxValue
                    : ((long)((ulong)value + delta)));

            if (maxValue < value)
            {
                maxValue = long.MaxValue;
            }

            return maxValue;
        }
    }
}
