using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;


namespace WebApplication1.Models
{
    [DataContract]
    [Serializable]
    public class PaymentDetail
    {
        [DataMember]
        public string Day { get; set; }
        [DataMember]
        public int InitHour { get; set; }
        [DataMember]
        public int InitMinutes { get; set; }
        [DataMember]
        public int EndHour { get; set; }
        [DataMember]
        public int EndMinutes { get; set; }
        [DataMember]
        public double DailyPayment { get; set; }
    }

    [DataContract]
    [Serializable]
    public class EmployeeWorkingTime
    {
        [DataMember]
        public string EmployeeName { get; set; }
        [DataMember]
        public List<PaymentDetail> EmployeePaymentBalance { get; set; }
        
    }

    public class EmployeeFile_Detail
    {
        public string Line { get; set; }
        public bool Status { get; set; }
        
    }
    public class ResultPaymentFileModel
    {
        public bool Status { get; set; } = true;
        public List<EmployeeWorkingTime> Items { get; set; }
        public List<EmployeeFile_Detail> ItemsDetail { get; set; }
    }
}