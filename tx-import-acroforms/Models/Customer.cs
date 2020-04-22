using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace tx_import_acroforms.Models
{
    public class Customer
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Firstname { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Postalcode { get; set; }
        public DateTime DOB { get; set; }
        public bool Tax { get; set; }
    }
}