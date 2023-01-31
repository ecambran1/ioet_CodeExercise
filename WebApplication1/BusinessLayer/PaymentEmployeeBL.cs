using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using WebApplication1.Models;
using Resources;
using Newtonsoft.Json;

namespace WebApplication1.BusinessLayer
{
    public class PaymentEmployeeBL
    {
        public bool processPaymentFile(ResultPaymentFileModel PayEmployeeSheet) 
        {
            List<EmployeeWorkingTime> EmplData = new List<EmployeeWorkingTime>();
            bool blnrsp = true;
            try
            {
                //Load Payment schedule
                string strScheduleBase = Resources.ConfigRes.PaymentSchedule;
                PaymentRulesModel PayRulesBase = JsonConvert.DeserializeObject<PaymentRulesModel>(strScheduleBase);

                if ((PayEmployeeSheet.ItemsDetail != null) &&
                    (PayEmployeeSheet.ItemsDetail.Count > 0))
                {
                    foreach (EmployeeFile_Detail LineFile in PayEmployeeSheet.ItemsDetail)
                    {
                        string[] arrEmplLine = LineFile.Line.Split('=');
                        if (!string.IsNullOrEmpty(arrEmplLine[0]))
                        {
                            EmployeeWorkingTime ItemEmpleado = new EmployeeWorkingTime()
                            {
                                EmployeeName = arrEmplLine[0],
                                EmployeePaymentBalance = new List<PaymentDetail>()
                            };

                            if (arrEmplLine.Length > 1)
                            {
                                string[] arrEmpSched = arrEmplLine[1].Split(',');
                                if (arrEmpSched != null)
                                {
                                    var regex = @"^(MO|TU|WE|TH|FR|SA|SU)\d{2}:\d{2}-\d{2}:\d{2}$";
                                    foreach (string strItemDia in arrEmpSched)
                                    {
                                        Match match = Regex.Match(strItemDia, regex, RegexOptions.IgnoreCase);
                                        if ((!String.IsNullOrEmpty(strItemDia)) &&
                                            match.Success)
                                        {
                                            PaymentDetail dtlpaymEmpl = new PaymentDetail()
                                            {
                                                Day = strItemDia.Substring(0, 2),
                                                InitHour = Convert.ToInt32(strItemDia.Substring(2, 2)),
                                                InitMinutes = Convert.ToInt32(strItemDia.Substring(5, 2)),
                                                EndHour = Convert.ToInt32(strItemDia.Substring(8, 2)),
                                                EndMinutes = Convert.ToInt32(strItemDia.Substring(11, 2)),
                                                DailyPayment = 0
                                            };
                                            //Search which day has to be evaluated
                                            PayRule DayToEvaluate = PayRulesBase.schedule.Find(x => x.day.ToUpper() == dtlpaymEmpl.Day.ToUpper());
                                            if (DayToEvaluate != null)
                                            {
                                                bool blnIsFileTimeValid = true;
                                                DateTime dtIni = new DateTime();
                                                DateTime dtFin = new DateTime();
                                                //try to create Datetime objects from time in file
                                                try
                                                {
                                                    dtIni = new DateTime(DateTime.Now.Year,
                                                                            DateTime.Now.Month,
                                                                            DateTime.Now.Day,
                                                                            dtlpaymEmpl.InitHour,
                                                                            dtlpaymEmpl.InitMinutes,
                                                                            0);
                                                    //when in file end hour is 00 we have to add 1 day
                                                    //and the minutes won't be taken in count, because these minutes
                                                    //should be added in the next day report

                                                    if (dtlpaymEmpl.EndHour == 0)
                                                    {
                                                        dtFin = new DateTime(DateTime.Now.Year,
                                                                                DateTime.Now.Month,
                                                                                DateTime.Now.Day,
                                                                                0, 0, 0);
                                                        dtFin = dtFin.AddDays(1);
                                                    }
                                                    else
                                                    {
                                                        dtFin = new DateTime(DateTime.Now.Year,
                                                                                DateTime.Now.Month,
                                                                                DateTime.Now.Day,
                                                                                dtlpaymEmpl.EndHour,
                                                                                dtlpaymEmpl.EndMinutes,
                                                                                0);
                                                    }
                                                    //check if end hour is bigger than init hour
                                                    TimeSpan ts = dtFin - dtIni;
                                                    if (ts.TotalHours < 0)
                                                    {
                                                        blnIsFileTimeValid = false;
                                                    }
                                                }
                                                catch (Exception exDtm)
                                                {
                                                    blnIsFileTimeValid = false;
                                                };
                                                //if times in file are ok then
                                                //we evaluate in the ranges of the base payment
                                                if (blnIsFileTimeValid)
                                                {
                                                    foreach (PayRange rngPay in DayToEvaluate.timetable)
                                                    {
                                                        bool blnRngBaseTimeValid = true;
                                                        DateTime dtInitRng = new DateTime();
                                                        DateTime dtEndRng = new DateTime();
                                                        try
                                                        {
                                                            dtInitRng = new DateTime(DateTime.Now.Year,
                                                                            DateTime.Now.Month,
                                                                            DateTime.Now.Day,
                                                                            rngPay.hrIni,
                                                                            rngPay.minIni,
                                                                            0);
                                                            //when in base range config, end hour is 24 we have to add 1 day
                                                            //and the minutes will be 0
                                                            if (rngPay.hrFin == 24)
                                                            {
                                                                dtEndRng = new DateTime(DateTime.Now.Year,
                                                                                        DateTime.Now.Month,
                                                                                        DateTime.Now.Day,
                                                                                        0, 0, 0);
                                                                dtEndRng = dtEndRng.AddDays(1);
                                                            }
                                                            else
                                                            {
                                                                dtEndRng = new DateTime(DateTime.Now.Year,
                                                                                DateTime.Now.Month,
                                                                                DateTime.Now.Day,
                                                                                rngPay.hrFin,
                                                                                rngPay.minFin,
                                                                                0);
                                                            }

                                                        }
                                                        catch (Exception ex1)
                                                        {
                                                            blnRngBaseTimeValid = false;
                                                        };
                                                        //check the times on the base range are valid
                                                        if (blnRngBaseTimeValid)
                                                        {
                                                            //check if the file time is completely within this range
                                                            if ((dtInitRng <= dtIni) &&
                                                                (dtIni <= dtEndRng) &&
                                                                (dtFin <= dtEndRng))
                                                            {
                                                                //getting the time in hours and calculating payment
                                                                TimeSpan tsRng = dtFin - dtIni;
                                                                dtlpaymEmpl.DailyPayment += (tsRng.TotalHours * rngPay.hrPay);
                                                            }
                                                            else
                                                            {
                                                                //only initial part of the time is in the range
                                                                if ((dtInitRng <= dtIni) &&
                                                                    (dtIni <= dtEndRng) &&
                                                                    (dtFin > dtEndRng))
                                                                {
                                                                    //getting the time in hours and calculating payment
                                                                    TimeSpan tsRng = dtEndRng - dtIni;
                                                                    dtlpaymEmpl.DailyPayment += (tsRng.TotalHours * rngPay.hrPay);
                                                                }
                                                                else 
                                                                {
                                                                    //only final part of the time is in the range
                                                                    if ((dtIni<= dtInitRng) &&
                                                                        (dtFin >= dtInitRng) &&
                                                                        (dtFin < dtEndRng))
                                                                    {
                                                                        //getting the time in hours and calculating payment
                                                                        TimeSpan tsRng = dtFin - dtInitRng;
                                                                        dtlpaymEmpl.DailyPayment += (tsRng.TotalHours * rngPay.hrPay);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                            }
                                            ItemEmpleado.EmployeePaymentBalance.Add(dtlpaymEmpl);
                                        }
                                    }
                                }
                            }
                            EmplData.Add(ItemEmpleado);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                blnrsp = false;
            }
            finally 
            {
                PayEmployeeSheet.Items = EmplData;
            };
            return blnrsp;
        }

        public ResultPaymentFileModel readPaymentFile(string FilePath)
        {
            ResultPaymentFileModel resultFileProcess = new ResultPaymentFileModel();
            List<EmployeeWorkingTime> EmplData = new List<EmployeeWorkingTime>();
            List<EmployeeFile_Detail> FileDataDetail = new List<EmployeeFile_Detail>();
            try
            {
                
                FileInfo existingFile = new FileInfo(FilePath);
                
                // Read file using StreamReader. Reads file line by line    
                using (StreamReader file = new StreamReader(FilePath))
                {
                    string strLine;

                    while ((strLine = file.ReadLine()) != null)
                    {
                        EmployeeFile_Detail empfldt = new EmployeeFile_Detail()
                        {
                            Line = strLine,
                            Status=true
                        };
                        FileDataDetail.Add(empfldt);
                    }
                    file.Close();
                }

                resultFileProcess.Status = true;
                resultFileProcess.Items = new List<EmployeeWorkingTime>();
                resultFileProcess.ItemsDetail = FileDataDetail;

                
                
            }
            catch (Exception ex)
            {
                resultFileProcess.Status = false;
                resultFileProcess.Items = new List<EmployeeWorkingTime>();
                resultFileProcess.ItemsDetail = new List<EmployeeFile_Detail>();
                
            }

            return resultFileProcess;
        }
    }
}