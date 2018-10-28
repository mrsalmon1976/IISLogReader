using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.BLL.Data.Models
{

    /// <summary>
    /// Stores detail of a single request log.
    /// Refer: https://www.loganalyzer.net/log-analyzer/w3c-extended.html
    /// </summary>
    public class RequestModel
    {
        public int Id { get; set; }

        public int LogFileId { get; set; }

        /// <summary>
        /// Request date/time (date + time)
        /// </summary>
        public DateTime RequestDateTime { get; set; }

        /// <summary>
        /// The IP address of the client that accessed your server (c-ip)
        /// </summary>
        public string ClientIp { get; set; }

        /// <summary>
        /// The name of the authenticated user who accessed your server.This does not include anonymous users, who are represented by a 
        /// hyphen(-). (cs-username)
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The Internet service and instance number that was accessed by a client. (s-sitename)
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// The name of the server on which the log entry was generated. (s-computername)
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// The IP address of the server on which the log entry was generated. (s-ip)
        /// </summary>
        public string ServerIp { get; set; }

        /// <summary>
        /// The port number the client is connected to. (s-port)
        /// </summary>
        public string ServerPort { get; set; }

        /// <summary>
        /// The action the client was trying to perform(for example, a GET method). (cs-method)
        /// </summary>
        public string Method { get; set; }


        /// <summary>
        /// The resource accessed; for example, Default.htm. (cs-uri-stem)
        /// </summary>
        public string UriStem { get; set; }

        /// <summary>
        /// The query, if any, the client was trying to perform. (cs-uri-query)
        /// </summary>
        public string UriQuery { get; set; }

        /// <summary>
        /// The status of the action, in HTTP or FTP terms. (sc-status)
        /// </summary>
        public string ProtocolStatus { get; set; }

        /// <summary>
        /// The number of bytes sent by the server. (sc-bytes)
        /// </summary>
        public int? BytesSent { get; set; }

        /// <summary>
        /// The number of bytes received by the server. (cs-bytes)
        /// </summary>
        public int? BytesReceived { get; set; }

        /// <summary>
        /// The duration of time, in milliseconds, that the action consumed. (time-taken)
        /// </summary>
        public int? TimeTaken { get; set; }

        /// <summary>
        /// The protocol(HTTP, FTP) version used by the client.For HTTP this will be either HTTP 1.0 or HTTP 1.1. (cs-version)
        /// </summary>
        public string ProtocolVersion { get; set; }

        /// <summary>
        /// Displays the content of the host header. (cs-host)
        /// </summary>
        public string Host { get; set; }


        /// <summary>
        /// The browser used on the client. (cs(User-Agent))
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// The content of the cookie sent or received, if any. (cs(Cookie))
        /// </summary>
        public string Cookie { get; set; }

        /// <summary>
        /// The previous site visited by the user.This site provided a link to the current site. (cs(Referer) )
        /// </summary>
        public string Referer { get; set; } 

    }
}
