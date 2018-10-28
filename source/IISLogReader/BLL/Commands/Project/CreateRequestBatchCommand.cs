using IISLogReader.BLL.Data;
using IISLogReader.BLL.Data.Models;
using IISLogReader.BLL.Exceptions;
using IISLogReader.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tx.Windows;

namespace IISLogReader.BLL.Commands.Project
{
    public interface ICreateRequestBatchCommand
    {
        void Execute(int logFileId, IEnumerable<W3CEvent> logEvents);
    }
    public class CreateRequestBatchCommand : ICreateRequestBatchCommand
    {
        private IDbContext _dbContext;
        private IRequestValidator _requestValidator;

        public CreateRequestBatchCommand(IDbContext dbContext, IRequestValidator requestValidator)
        {
            _dbContext = dbContext;
            _requestValidator = requestValidator;
        }

        public void Execute(int logFileId, IEnumerable<W3CEvent> logEvents)
        {
            string sql = @"INSERT INTO Requests (LogFileId, RequestDateTime) VALUES (@LogFileId, @RequestDateTime)";

            foreach (W3CEvent evt in logEvents)
            {
                RequestModel model = new RequestModel();
                model.LogFileId = logFileId;
                model.RequestDateTime = evt.dateTime;

                ValidationResult result = _requestValidator.Validate(model);
                if (!result.Success)
                {
                    throw new ValidationException(result.Messages);
                }

                // insert new record
                _dbContext.ExecuteNonQuery(sql, model);

            }
        }
    }
}
