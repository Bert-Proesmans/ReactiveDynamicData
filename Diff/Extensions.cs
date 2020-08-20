using System.Collections.Generic;
using System.Linq;

namespace ReactiveDynamicData.Diff
{
    internal static class Extensions
    {
        /// <summary>
        /// Take a selection from the current list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static List<T> Substring<T>(this IReadOnlyList<T> target, int start, int length = -1)
        {
            if (length == -1)
            {
                length = target.Count - start;
            }
            var list = target as List<T>;
            return list?.GetRange(start, length) ?? target.Skip(start).Take(length).ToList();
        }

        /// <summary>
        /// Test if objects at specified ranges from both lists are equal.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listA"></param>
        /// <param name="offsetA"></param>
        /// <param name="listB"></param>
        /// <param name="offsetB"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static bool CompareRange<T>(IReadOnlyList<T> listA, int offsetA, IReadOnlyList<T> listB, int offsetB, int count, IEqualityComparer<T> comparer)
        {
            for (var j = 0; j < count; j++)
            {
                if (!comparer.Equals(listA[offsetA + j], listB[offsetB + j]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Find the starting index of another list within the current one.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="other"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static int IndexOf<T>(this IReadOnlyList<T> target, IReadOnlyList<T> other, IEqualityComparer<T> comparer, int start = 0)
        {
            var end = target.Count - other.Count;
            for (var i = start; i < end; i++)
            {
                if (CompareRange(target, i, other, 0, other.Count, comparer))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Test if two objects, both on seperate lists at specified index, are equal.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="targetPos"></param>
        /// <param name="other"></param>
        /// <param name="otherPos"></param>
        /// <returns></returns>
        public static bool EqualsAt<T>(this IReadOnlyList<T> target, int targetPos, IReadOnlyList<T> other, int otherPos, IEqualityComparer<T> comparer)
        {
            return comparer.Equals(target[targetPos], other[otherPos]);
        }

        /// <summary>
        /// Insert an enumerable of element into the current list.
        /// </summary>
        /// <remarks>
        /// Note that <paramref name="count"/> isn't derived from array length of new objects!
        /// </remarks>
        /// <typeparam name="T">The list content type</typeparam>
        /// <param name="input">The current list</param>
        /// <param name="start">The start position of the insert</param>
        /// <param name="count">The amount of items to remove after startposition</param>
        /// <param name="objects">The objects to insert.</param>
        /// <returns></returns>
        public static List<T> Splice<T>(this List<T> input, int start, int count, params T[] objects)
        {
            var deletedRange = input.GetRange(start, count);
            input.RemoveRange(start, count);
            input.InsertRange(start, objects);
            return deletedRange;
        }

        /// <summary>
        /// Test if the provided items are the same as the ones at the start of the current list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool StartsWith<T>(this IReadOnlyList<T> target, IReadOnlyList<T> other, IEqualityComparer<T> comparer)
        {
            return target.Count >= other.Count && target.Take(other.Count).SequenceEqual(other, comparer);
        }

        /// <summary>
        /// Test if the provided items are the same as the ones at the end of the current list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool EndsWith<T>(this IReadOnlyList<T> target, IReadOnlyList<T> other, IEqualityComparer<T> comparer)
        {
            return target.Count >= other.Count &&
                target
                .Skip(target.Count - other.Count)
                .Take(other.Count)
                .SequenceEqual(other, comparer);
        }
    }
}
