using System;
using System.IO;
using System.IO.Compression;

namespace Phenix.Core.IO
{
    /// <summary>
    /// 压缩助手
    /// </summary>
    public static class CompressHelper
    {
        /// <summary>
        /// 压缩
        /// </summary>
        public static ArraySegment<byte> Compress(ArraySegment<byte> source)
        {
            if (source == null || source.Array == null)
                throw new ArgumentNullException(nameof(source));

            using (MemoryStream targetStream = new MemoryStream())
            {
                using (DeflateStream compressStream = new DeflateStream(targetStream, CompressionMode.Compress, true))
                {
                    compressStream.Write(source.Array, 0, source.Count);
                }

                return new ArraySegment<byte>(targetStream.ToArray());
            }
        }

        /// <summary>
        /// 压缩
        /// </summary>
        public static void CompressTo(this Stream sourceStream, Stream targetStream)
        {
            if (sourceStream == null)
                throw new ArgumentNullException(nameof(sourceStream));
            if (targetStream == null)
                throw new ArgumentNullException(nameof(targetStream));

            using (DeflateStream compressStream = new DeflateStream(targetStream, CompressionMode.Compress, true))
            {
                sourceStream.CopyTo(compressStream);
            }

            targetStream.Seek(0, SeekOrigin.Begin);
        }

        /// <summary>
        /// 解压
        /// </summary>
        public static ArraySegment<byte> Decompress(ArraySegment<byte> source)
        {
            if (source == null || source.Array == null)
                throw new ArgumentNullException(nameof(source));

            using (MemoryStream sourceStream = new MemoryStream(source.Array))
            using (DeflateStream decompressStream = new DeflateStream(sourceStream, CompressionMode.Decompress, true))
            {
                return new ArraySegment<byte>(decompressStream.ToArray());
            }
        }

        /// <summary>
        /// 解压
        /// </summary>
        public static void DecompressTo(this Stream sourceStream, Stream targetStream)
        {
            if (sourceStream == null)
                throw new ArgumentNullException(nameof(sourceStream));
            if (targetStream == null)
                throw new ArgumentNullException(nameof(targetStream));

            using (DeflateStream decompressStream = new DeflateStream(sourceStream, CompressionMode.Decompress, true))
            {
                decompressStream.CopyTo(targetStream);
            }

            targetStream.Seek(0, SeekOrigin.Begin);
        }
    }
}