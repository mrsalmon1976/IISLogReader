using IISLogReader.BLL.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SystemWrapper.IO;

namespace IISLogReader.BLL.Utils
{
    public interface IFileUtils
    {
        FileDetail GetFileHash(string filePath);
    }

    public class FileUtils : IFileUtils
    { 
        private readonly IFileWrap _fileWrapper;

        public FileUtils(IFileWrap fileWrapper)
        {
            _fileWrapper = fileWrapper;
        }

        public FileDetail GetFileHash(string filePath)
        {
            FileDetail fileDetail = new FileDetail();
            fileDetail.Name = Path.GetFileName(filePath);

            IFileStreamWrap fileStream = _fileWrapper.OpenRead(filePath);
            fileDetail.Length = fileStream.Length;

            using (var md5 = MD5.Create())
            {
                fileStream.Seek(0, SeekOrigin.Begin);
                var hash = md5.ComputeHash(fileStream.StreamInstance);
                fileDetail.Hash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }

            return fileDetail;
        }
    }
}
