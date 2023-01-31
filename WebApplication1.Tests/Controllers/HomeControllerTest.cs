using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using WebApplication1;
using WebApplication1.BusinessLayer;
using WebApplication1.Controllers;
using WebApplication1.Models;

namespace WebApplication1.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void Index()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ProcessPaymentFileTest()
        {
            // Arrange
            PaymentEmployeeBL controller = new BusinessLayer.PaymentEmployeeBL();

            // Act
            EmployeeFile_Detail tstEmpDet = new EmployeeFile_Detail()
            {
                Line= "RENE=MO10:00-12:00,TU10:00-12:00,TH01:00-03:00,SA14:00-18:00,SU20:00-21:00",
                Status=true
            };
            List<EmployeeFile_Detail> tstLstemp = new List<EmployeeFile_Detail>();
            tstLstemp.Add(tstEmpDet);
            ResultPaymentFileModel parmFileModel = new ResultPaymentFileModel()
            {
                ItemsDetail = tstLstemp
            };
            bool result = controller.processPaymentFile(parmFileModel);

            // Assert
            Assert.AreEqual(false, result);
        }

        
    }
}
