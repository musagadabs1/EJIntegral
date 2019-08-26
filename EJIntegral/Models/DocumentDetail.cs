using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EJIntegral.Models
{
    public class DocumentDetail
    {
        public string DocName { get; set; }
        public string Description { get; set; }
        public string DocPath { get; set; }

        public virtual Document Document { get; set; }

    }
}