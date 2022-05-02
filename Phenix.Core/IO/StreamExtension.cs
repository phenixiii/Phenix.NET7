using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO
{
    /// <summary>
    /// 扩展
    /// </summary>
    public static class StreamExtension
    {
        private const int BufferSize = 4096;

        /// <summary>
        /// 拷贝数据
        /// </summary>
        /// <param name="sourceStream">数据源</param>
        /// <param name="targetStream">目的流</param>
        public static void CopyTo(this Stream sourceStream, Stream targetStream)
        {
            if (sourceStream == null)
                throw new ArgumentNullException(nameof(sourceStream));
            if (targetStream == null)
                throw new ArgumentNullException(nameof(targetStream));

            int i;
            byte[] sourceBuffer = new byte[BufferSize];
            while ((i = sourceStream.Read(sourceBuffer, 0, BufferSize)) > 0)
            {
                targetStream.Write(sourceBuffer, 0, i);
                targetStream.Flush();
            }
        }

        /// <summary>
        /// 拷贝数据
        /// </summary>
        /// <param name="sourceStream">数据源</param>
        /// <param name="targetStream">目的流</param>
        public static async Task CopyToAsync(this Stream sourceStream, Stream targetStream)
        {
            await CopyToAsync(sourceStream, targetStream, CancellationToken.None);
        }

        /// <summary>
        /// 拷贝数据
        /// </summary>
        /// <param name="sourceStream">数据源</param>
        /// <param name="targetStream">目的流</param>
        /// <param name="cancellationToken">取消指示，用于对应该取消操作的通知进行传播</param>
        public static async Task CopyToAsync(this Stream sourceStream, Stream targetStream, CancellationToken cancellationToken)
        {
            if (sourceStream == null)
                throw new ArgumentNullException(nameof(sourceStream));
            if (targetStream == null)
                throw new ArgumentNullException(nameof(targetStream));

            int i;
            byte[] sourceBuffer = new byte[BufferSize];
            while ((i = await sourceStream.ReadAsync(sourceBuffer, 0, BufferSize, cancellationToken)) > 0)
            {
                await targetStream.WriteAsync(sourceBuffer, 0, i, cancellationToken);
                await targetStream.FlushAsync(cancellationToken);
            }
        }

        /// <summary>
        /// 拷贝数据
        /// </summary>
        /// <param name="sourceStream">数据源</param>
        /// <returns>目的字节串</returns>
        public static byte[] ToArray(this Stream sourceStream)
        {
            if (sourceStream == null)
                throw new ArgumentNullException(nameof(sourceStream));

            List<byte> result = new List<byte>();
            int i;
            byte[] sourceBuffer = new byte[BufferSize];
            while ((i = sourceStream.Read(sourceBuffer, 0, BufferSize)) > 0)
            {
                byte[] targetBuffer = new byte[i];
                Array.Copy(sourceBuffer, targetBuffer, i);
                result.AddRange(targetBuffer);
            }

            return result.ToArray();
        }

        /// <summary>
        /// 拷贝数据
        /// </summary>
        /// <param name="sourceStream">数据源</param>
        /// <returns>目的字节串</returns>
        public static async Task<byte[]> ToArrayAsync(this Stream sourceStream)
        {
            return await ToArrayAsync(sourceStream, CancellationToken.None);
        }

        /// <summary>
        /// 拷贝数据
        /// </summary>
        /// <param name="sourceStream">数据源</param>
        /// <param name="cancellationToken">取消指示，用于对应该取消操作的通知进行传播</param>
        /// <returns>目的字节串</returns>
        public static async Task<byte[]> ToArrayAsync(this Stream sourceStream, CancellationToken cancellationToken)
        {
            if (sourceStream == null)
                throw new ArgumentNullException(nameof(sourceStream));

            List<byte> result = new List<byte>();
            int i;
            byte[] sourceBuffer = new byte[BufferSize];
            while ((i = await sourceStream.ReadAsync(sourceBuffer, 0, BufferSize, cancellationToken)) > 0)
            {
                byte[] targetBuffer = new byte[i];
                Array.Copy(sourceBuffer, targetBuffer, i);
                result.AddRange(targetBuffer);
            }

            return result.ToArray();
        }
    }
}