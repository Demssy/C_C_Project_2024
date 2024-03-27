using C_C_Proj_WebStore.DataAccess.Data;
using C_C_Proj_WebStore.DataAccess.Repository.IRepository;
using C_C_Proj_WebStore.Models;
using C_C_Proj_WebStore.Models.ViewModels;
using C_C_Proj_WebStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace C_C_Proj_WebStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
            
            return View(objCompanyList);
        }
        public IActionResult Upsert(int? id)
        {
            if (id == null || id== 0)
            {
                //create
                return View(new Company());
            }
            else
            {
                //update
                Company companyObj = _unitOfWork.Company.Get(u=>u.Id == id);
                return View(companyObj);
            }
        }
        [HttpPost]
        public IActionResult Upsert(Company companyObj)
        {
            if (ModelState.IsValid)
            {
                
                if (companyObj.Id != 0)
                {
                    _unitOfWork.Company.Update(companyObj);
                }
                else
                {
                    _unitOfWork.Company.Add(companyObj);
                }
                _unitOfWork.Save();
                TempData["success"] = "Company has been created successfully";
                return RedirectToAction("Index");
            }
            else
            { 
                return View(companyObj);
            }
        }

        

       



        #region API CALLS


        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
            return Json(new { data = objCompanyList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var companyToBeDeleted = _unitOfWork.Company.Get(u => u.Id == id);
            if (companyToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            } 
            _unitOfWork.Company.Remove(companyToBeDeleted);
            _unitOfWork.Save();
            
            return Json(new { success = true, message = "Delete successful" });
        }

        #endregion
    }
}
