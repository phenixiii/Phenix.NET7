using System;
using System.IO;
using System.Threading.Tasks;

namespace Phenix.Services.Business.Inout
{
    /// <summary>
    /// 文件块信息
    /// </summary>
    [Serializable]
    public class FileChunkInfo
    {
        /// <summary>
        /// 初始化
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        public FileChunkInfo(string fileName, int chunkCount, int chunkNumber, int chunkSize, int maxChunkSize, byte[] chunkBody)
        {
            _fileName = Path.GetFileName(fileName);
            _chunkCount = chunkCount;
            _chunkNumber = chunkNumber;
            _chunkSize = chunkSize;
            _maxChunkSize = maxChunkSize;
            _chunkBody = chunkBody;
        }

        #region 工厂

        /// <summary>
        /// 构造文件块
        /// </summary>
        /// <param name="sourceStream">数据源</param>
        /// <param name="fileName">文件名</param>
        /// <param name="chunkNumber">块号</param>
        /// <param name="maxChunkSize">块最大值</param>
        /// <returns>文件块信息</returns>
        public static async Task<FileChunkInfo> CreateAsync(Stream sourceStream, string fileName, int chunkNumber, int maxChunkSize)
        {
            if (sourceStream == null)
                throw new ArgumentNullException(nameof(sourceStream));
            if (maxChunkSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxChunkSize));

            int chunkCount = (int) Math.Ceiling(sourceStream.Length * 1.0 / maxChunkSize);
            if (chunkNumber > chunkCount)
                return null;

            int chunkSize = chunkNumber < chunkCount ? maxChunkSize : (int) (sourceStream.Length - maxChunkSize * (chunkCount - 1));
            byte[] chunkBody = new byte[chunkSize];
            sourceStream.Seek(maxChunkSize * (chunkNumber - 1), SeekOrigin.Begin);
            await sourceStream.ReadAsync(chunkBody, 0, chunkSize);
            return new FileChunkInfo(fileName, chunkCount, chunkNumber, chunkSize, maxChunkSize, chunkBody);
        }

        #endregion
        
        #region 属性

        private readonly string _fileName;

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
        }

        private readonly int _chunkCount;

        /// <summary>
        /// 块数
        /// </summary>
        public int ChunkCount
        {
            get { return _chunkCount; }
        }

        private readonly int _chunkNumber;

        /// <summary>
        /// 块号
        /// </summary>
        public int ChunkNumber
        {
            get { return _chunkNumber; }
        }

        private readonly int _chunkSize;

        /// <summary>
        /// 块大小
        /// </summary>
        public int ChunkSize
        {
            get { return _chunkSize; }
        }

        private readonly int _maxChunkSize;

        /// <summary>
        /// 块最大值
        /// </summary>
        public int MaxChunkSize
        {
            get { return _maxChunkSize; }
        }

        private readonly byte[] _chunkBody;

        /// <summary>
        /// 块体
        /// </summary>
        public byte[] ChunkBody
        {
            get { return _chunkBody; }
        }

        /// <summary>
        /// 是否终止
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool Stop
        {
            get { return _chunkCount <= 0 || _chunkNumber <= 0 || _chunkSize <= 0; }
        }

        /// <summary>
        /// 是否完成
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool Done
        {
            get { return _chunkCount <= _chunkNumber; }
        }

        #endregion

        #region 方法

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="targetStream">目的流</param>
        public async Task WriteAsync(Stream targetStream)
        {
            if (targetStream == null)
                throw new ArgumentNullException(nameof(targetStream));

            targetStream.Seek(MaxChunkSize * (ChunkNumber - 1), SeekOrigin.Begin);
            await targetStream.WriteAsync(ChunkBody);
            await targetStream.FlushAsync();
        }

        #endregion
    }
}