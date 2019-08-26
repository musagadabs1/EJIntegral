using System.Collections.Generic;
using System.Web.Mvc;

namespace EJIntegral.Models
{
    public class LGAAndDAModel
    {
        public LGAAndDAModel()
        {
            LGAs = new List<SelectListItem>();
            DAs = new List<SelectListItem>();
        }
        public List<SelectListItem> LGAs { get; set; }
        public List<SelectListItem> DAs { get; set; }

        public int LGAId { get; set; }
        public string DAName { get; set; }
    }
}