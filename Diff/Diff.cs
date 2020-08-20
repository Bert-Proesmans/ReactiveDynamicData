using System;
using System.Collections.Generic;
using System.Linq;

namespace ReactiveDynamicData.Diff
{
    /// <summary>
    /// Encoding of a comparison between two lists.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    // SOURCE; https://github.com/danielearwicker/ListDiff
    public class Diff<T> : IEquatable<Diff<T>>
    {
        #region Constructors
        public Diff(Operation operation, IReadOnlyList<T> items)
        {
            Operation = operation;
            Items = items;
        }
        #endregion

        #region Properties
        public Operation Operation { get; }

        public IReadOnlyList<T> Items { get; set; }
        #endregion

        #region Object
        public override string ToString()
        {
            var prettyText = string.Join("", Items.Select(t => t.ToString())).Replace('\n', '\u00b6');
            return "Diff(" + Operation + ",\"" + prettyText + "\")";
        }

        public override bool Equals(object obj)
        {
            // If parameter cannot be cast to Diff return false.
            if (!(obj is Diff<T> p))
            {
                return false;
            }

            // Return true if the fields match.
            return Equals(p);
        }

        public override int GetHashCode()
        {
            return Items.GetHashCode() ^ Operation.GetHashCode();
        }
        #endregion

        #region IEquatable
        public bool Equals(Diff<T> obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // Return true if the fields match.
            return obj.Operation == Operation &&
                   obj.Items.SequenceEqual(Items);
        }
        #endregion
    }
}