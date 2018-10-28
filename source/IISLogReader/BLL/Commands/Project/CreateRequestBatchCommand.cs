using IISLogReader.BLL.Data;
using IISLogReader.BLL.Data.Models;
using IISLogReader.BLL.Exceptions;
using IISLogReader.BLL.Utils;
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
            string sql = @"INSERT INTO Requests (
                LogFileId
                , RequestDateTime
                , ClientIp 
                , UserName 
                , ServiceName 
                , ServerName 
                , ServerIp 
                , ServerPort 
                , Method 
                , UriStem 
                , UriQuery 
                , ProtocolStatus 
                , BytesSent 
                , BytesReceived 
                , TimeTaken 
                , ProtocolVersion 
                , Host 
                , UserAgent 
                , Cookie 
                , Referer 
                ) VALUES (
                @LogFileId
                , @RequestDateTime
                , @ClientIp 
                , @UserName 
                , @ServiceName 
                , @ServerName 
                , @ServerIp 
                , @ServerPort 
                , @Method 
                , @UriStem 
                , @UriQuery 
                , @ProtocolStatus 
                , @BytesSent 
                , @BytesReceived 
                , @TimeTaken 
                , @ProtocolVersion 
                , @Host 
                , @UserAgent 
                , @Cookie 
                , @Referer 
                )";

            foreach (W3CEvent evt in logEvents)
            {
                RequestModel model = new RequestModel();
                model.LogFileId = logFileId;
                model.RequestDateTime = evt.dateTime;
                model.ClientIp = evt.c_ip;
                model.UserName = evt.cs_username;
                model.ServiceName = evt.s_sitename;
                model.ServerName = evt.s_computername;
                model.ServerIp = evt.s_ip;
                model.ServerPort = evt.s_port;
                model.Method = evt.cs_method;
                model.UriStem = evt.cs_uri_stem;
                model.UriQuery = evt.cs_uri_query;
                model.ProtocolStatus = evt.sc_status;
                model.BytesSent = NumericUtils.TryParse(evt.sc_bytes);
                model.BytesReceived = NumericUtils.TryParse(evt.cs_bytes);
                model.TimeTaken = NumericUtils.TryParse(evt.time_taken);
                model.ProtocolVersion = evt.cs_version;
                model.Host = evt.cs_host;
                model.UserAgent = evt.cs_User_Agent;
                model.Cookie = evt.cs_Cookie;
                model.Referer = evt.cs_Referer;

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
