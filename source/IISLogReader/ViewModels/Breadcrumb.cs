using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISLogReader.ViewModels
{
    public class Breadcrumb
    {
        public Breadcrumb(string text) : this(text, null)
        {

        }
        public Breadcrumb(string text, string url)
        {
            this.Text = text;
            this.Url = url;
        }

        public string Text { get; set; }

        public string Url { get; set; }
    }
}
