namespace ReactiveDynamicData.Diff
{
    // SOURCE; https://github.com/danielearwicker/ListDiff
    public enum Operation
    {
        /// <summary>
        /// Fragment was deleted
        /// </summary>
        Delete,
        /// <summary>
        /// Fragment was inserted
        /// </summary>
        Insert,
        /// <summary>
        /// Fragment was equal
        /// </summary>
        Equal
    }
}
