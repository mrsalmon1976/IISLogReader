using IISLogReader.BLL.Data;
using IISLogReader.BLL.Data.Models;
using IISLogReader.BLL.Data.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Tx.Windows;

namespace IISLogReader.BLL.Commands.Project
{
    public interface ICreateLogFileWithRequestsCommand
    {
        void Execute(int projectId, string fileName, Stream fileStream);
    }

    public class CreateLogFileWithRequestsCommand : ICreateLogFileWithRequestsCommand
    {
        private IDbContext _dbContext;
        private ILogFileRepository _logFileRepo;

        public CreateLogFileWithRequestsCommand(IDbContext dbContext, ILogFileRepository logFileRepo)
        {
            _dbContext = dbContext;
            _logFileRepo = logFileRepo;
        }

        public void Execute(int projectId, string fileName, Stream fileStream)
        {
            // load the contents of the file
            StreamReader reader = new StreamReader(fileStream);
            IEnumerable<W3CEvent> events;
            string fileHash = String.Empty;

            try
            {
                events = W3CEnumerable.FromStream(reader);
                events.First();     // peek at the first item - if the file is invalid this will cause an exception to be raised
            }
            catch (Exception)
            {
                throw new FileFormatException("File is not a valid IIS log file");
            }

            // check the hash of the file - if it is already saved against this project, throw an exception
            using (var md5 = MD5.Create())
            {
                fileStream.Seek(0, SeekOrigin.Begin);
                var hash = md5.ComputeHash(fileStream);
                fileHash = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
            LogFileModel logFile = _logFileRepo.GetByHash(projectId, fileHash);

            // save details of the file itself


            // save each event

            reader.Dispose();
        }
    }
}
