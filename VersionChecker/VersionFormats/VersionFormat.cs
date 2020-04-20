using System;

namespace Straitjacket.Utility.VersionFormats
{
    /// <summary>
    /// Provides a base class for implementations of the <see cref="IVersionFormat"/> interface.
    /// </summary>
    public abstract class VersionFormat : IVersionFormat, IComparable<VersionFormat>
    {
        /// <summary>
        /// A <see cref="string"/> representing the version number of this instance.
        /// </summary>
        public virtual string Version { get; set; }

        /// <summary>
        /// Compares the current <see cref="VersionFormat"/> to a specified <see cref="VersionFormat"/> and returns an indication of their relative values.
        /// </summary>
        /// <param name="other">A <see cref="VersionFormat"/> representing a version number.</param>
        /// <returns>An <see cref="int"/> indicating the relative values of the two version numbers.</returns>
        public abstract int CompareTo(VersionFormat other);

        /// <summary>
        /// Converts the current <see cref="VersionFormat"/> to its equivalent <see cref="string"/> representation of a version number.
        /// </summary>
        /// <returns>The <see cref="string"/> representation of the version number.</returns>
        public override string ToString() => Version;

        /// <summary>
        /// Compares two <see cref="VersionFormat"/> operands and returns <see langword="true"/> if its left-hand <see cref="VersionFormat"/> is less
        /// than its right-hand <see cref="VersionFormat"/>, <see langword="false"/> otherwise.
        /// </summary>
        /// <param name="left">The left-hand <see cref="VersionFormat"/> operand.</param>
        /// <param name="right">The right-hand <see cref="VersionFormat"/> operand.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is less than <paramref name="right"/>, <see langword="false"/> otherwise.</returns>
        public static bool operator <(VersionFormat left, VersionFormat right)
            => left.CompareTo(right) < 0 && left.Version != right.Version;

        /// <summary>
        /// Compares two <see cref="VersionFormat"/> operands and returns <see langword="true"/> if its left-hand <see cref="VersionFormat"/> is greater
        /// than its right-hand <see cref="VersionFormat"/>, <see langword="false"/> otherwise.
        /// </summary>
        /// <param name="left">The left-hand <see cref="VersionFormat"/> operand.</param>
        /// <param name="right">The right-hand <see cref="VersionFormat"/> operand.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is greater than <paramref name="right"/>, <see langword="false"/> otherwise.</returns>
        public static bool operator >(VersionFormat left, VersionFormat right)
            => left.CompareTo(right) > 0 && left.Version != right.Version;

        /// <summary>
        /// Compares two <see cref="VersionFormat"/> operands and returns <see langword="true"/> if its left-hand <see cref="VersionFormat"/> is less
        /// than or equal to its right-hand <see cref="VersionFormat"/>, <see langword="false"/> otherwise.
        /// </summary>
        /// <param name="left">The left-hand <see cref="VersionFormat"/> operand.</param>
        /// <param name="right">The right-hand <see cref="VersionFormat"/> operand.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is less than or equal to <paramref name="right"/>,
        /// <see langword="false"/> otherwise.</returns>
        public static bool operator <=(VersionFormat left, VersionFormat right)
            => left.CompareTo(right) <= 0;

        /// <summary>
        /// Compares two <see cref="VersionFormat"/> operands and returns <see langword="true"/> if its left-hand <see cref="VersionFormat"/> is greater
        /// than or equal to its right-hand <see cref="VersionFormat"/>, <see langword="false"/> otherwise.
        /// </summary>
        /// <param name="left">The left-hand <see cref="VersionFormat"/> operand.</param>
        /// <param name="right">The right-hand <see cref="VersionFormat"/> operand.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is greater than or equal to <paramref name="right"/>,
        /// <see langword="false"/> otherwise.</returns>
        public static bool operator >=(VersionFormat left, VersionFormat right)
            => left.CompareTo(right) >= 0;
    }
}
