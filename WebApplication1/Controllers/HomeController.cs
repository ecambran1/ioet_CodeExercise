using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.BusinessLayer;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public JsonResult ProcessPaymentFile()
        {
            PaymentEmployeeBL PayEmplMngr = new PaymentEmployeeBL();
            ResultPaymentFileModel PayEmployeeSheet = new ResultPaymentFileModel();
            if (Request.Files.Count > 0)
            {
                try
                {
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        HttpPostedFileBase file = files[i];
                        string fname;
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            fname = testfiles[testfiles.Length - 1];
                        }
                        else
                        {
                            fname = file.FileName;
                        }
                        var newName = fname.Split('.');
                        fname = newName[0] + "_" + DateTime.Now.Ticks.ToString() + "." + newName[1];
                        var uploadRootFolderInput = AppDomain.CurrentDomain.BaseDirectory + "\\FileUploads";
                        Directory.CreateDirectory(uploadRootFolderInput);
                        var directoryFullPathInput = uploadRootFolderInput;
                        fname = Path.Combine(directoryFullPathInput, fname);
                        file.SaveAs(fname);
                        PayEmployeeSheet = PayEmplMngr.readPaymentFile(fname);
                        System.IO.File.Delete(fname);
                    }
                    if (PayEmplMngr.processPaymentFile(PayEmployeeSheet)) 
                    {
                        if (PayEmployeeSheet.Items.Count > 0)
                        {
                            var data = new ResultPaymentFileModel()
                            {
                                Items = PayEmployeeSheet.Items,
                                ItemsDetail = PayEmployeeSheet.ItemsDetail,
                                Status = true
                            };
                            return new JsonResult()
                            {
                                Data = data,
                                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                                MaxJsonLength = Int32.MaxValue

                            };
                        }
                        else
                        {
                            var data = new ResultPaymentFileModel()
                            {
                                Items = null,
                                ItemsDetail = PayEmployeeSheet.ItemsDetail,
                                Status = false
                            };
                            return Json(data, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        var data = new ResultPaymentFileModel()
                        {
                            Items = null,
                            ItemsDetail = PayEmployeeSheet.ItemsDetail,
                            Status = false
                        };
                        return Json(data, JsonRequestBehavior.AllowGet);
                    }

                }
                catch (Exception ex)
                {
                    var data = new ResultPaymentFileModel()
                    {
                        Items = null,
                        ItemsDetail = null,
                        Status = false
                    };
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                var data = new ResultPaymentFileModel()
                {
                    Items = null,
                    ItemsDetail = null,
                    Status = false
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }

        
    }
}