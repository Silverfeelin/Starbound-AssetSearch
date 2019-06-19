using System;

namespace Pak
{
    /// <summary>
    /// Class that holds the item data for items in Pak files.
    /// </summary>
    public class PakItem
    {
        /// <summary>
        /// Item name.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Stream offset for the first byte of this item.
        /// </summary>
        public ulong Offset { get; set; }

        /// <summary>
        /// Length of this item in bytes.
        /// </summary>
        public ulong Length { get; set; }

        public PakItem() { }

        public PakItem(string path, ulong offset, ulong length)
        {
            Path = path;
            Offset = offset;
            Length = length;
        }
    }
}
