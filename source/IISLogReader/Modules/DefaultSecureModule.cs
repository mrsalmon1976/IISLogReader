using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.Security;
using Nancy.Authentication.Forms;

namespace IISLogReader.Modules
{
    public class DefaultSecureModule : DefaultModule
    {
        public DefaultSecureModule()
        {
            this.RequiresAuthentication();
        }

    }
}
