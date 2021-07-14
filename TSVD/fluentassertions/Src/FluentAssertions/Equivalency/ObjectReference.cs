using System;
using System.Linq;
using FluentAssertions.Common;

namespace FluentAssertions.Equivalency
{
    /// <summary>
    /// Represents  an object tracked by the <see cref="CyclicReferenceDetector"/> including it's location within an object graph.
    /// </summary>
    internal class ObjectReference
    {
        private readonly object @object;
        private readonly string[] path;
        private readonly bool? isComplexType;

        public ObjectReference(object @object, string path, bool? isComplexType = null)
        {
            this.@object = @object;
            this.path = path.ToLower().Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            this.isComplexType = isComplexType;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (!(obj is ObjectReference other))
            {
                return false;
            }

            return ReferenceEquals(@object, other.@object) && IsParentOf(other);
        }

        private bool IsParentOf(ObjectReference other)
        {
            return (other.path.Length > path.Length) && other.path.Take(path.Length).SequenceEqual(path);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                return (@object.GetHashCode() * 397) ^ path.GetHashCode();
            }
        }

        public override string ToString()
        {
            return $"{{\"{path}\", {@object}}}";
        }

        public bool IsComplexType => isComplexType ?? (!(@object is null) && !@object.GetType().OverridesEquals());
    }
}
