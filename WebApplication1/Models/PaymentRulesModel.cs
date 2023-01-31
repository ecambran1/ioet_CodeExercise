using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class PayRange 
    {
        public int hrIni { get; set; }
        public int minIni { get; set; }
        public int hrFin { get; set; }
        public int minFin { get; set; }
        public double hrPay { get; set; }
    }

    public class PayRule 
    {
        public string day { get; set; }
        public List<PayRange> timetable { get; set; }
    }

    public class PaymentRulesModel
    {
        public List<PayRule> schedule { get; set; }
    }
}