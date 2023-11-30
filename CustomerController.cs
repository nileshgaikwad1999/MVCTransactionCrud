using Microsoft.Ajax.Utilities;
using MLCS.Data;
using MLCS.ViewModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MLCS.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CustomerController()
        {
            _context = new ApplicationDbContext();
        }

        // GET: Customer
        public ActionResult Index(int? page)
        {
            int pageSize = 5;
            int pageNumber = (page ?? 1);
            var customer = _context.customers.OrderBy(e => e.Id);
            var customerList = customer.ToPagedList(pageNumber, pageSize);
            return View(customerList);
        }

        [HttpGet]
        public ActionResult Create()
        {
            CustomerViewModel customerView = new CustomerViewModel();
            return View(customerView);
        }

        [HttpPost]
        public ActionResult Create(CustomerViewModel customerModel)
        {
            if (ModelState.IsValid)
            {
                Customer customer = customerModel.Customer;
                _context.customers.Add(customer);
                _context.SaveChanges();

                ProductBill productBill = customerModel.ProductBill;
                productBill.CustomerId = customer.Id;
                _context.productsBill.Add(productBill);
                _context.SaveChanges();

                if (customerModel.ProductillDetailList.Count() > 0)
                {
                    customerModel.ProductillDetailList.ForEach(e =>
                    {
                        e.ProductBillId = productBill.Id;
                    });
                    _context.productsBillDetails.AddRange(customerModel.ProductillDetailList);
                    _context.SaveChanges();
                }


                Bill bill = customerModel.Bill;
                bill.ProductBillId = productBill.Id;
                _context.Bill.Add(bill);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View();
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var customerViewss = (from c in _context.customers
                                  join p in _context.productsBill on c.Id equals p.CustomerId
                                  join pd in _context.productsBillDetails on p.Id equals pd.ProductBillId
                                  join pb in _context.Bill on p.Id equals pb.ProductBillId
                                  select
                                  new
                                  {
                                      Bill = pb,
                                      Customer = c,
                                      ProductBill = p,
                                      ProductillDetails = pd
                                  }
                              ).GroupBy(result => result.Customer)
                              .AsEnumerable()
                              .Select(group => new CustomerViewModel()
                              {
                                  Bill = group.FirstOrDefault().Bill,
                                  Customer = group.Key,
                                  ProductBill = group.FirstOrDefault().ProductBill,
                                  ProductillDetailList = group.Select(e => new ProductillDetails()
                                  {
                                      price = e.ProductillDetails.price,
                                      ProductId = e.ProductillDetails.ProductId,
                                      ProductName = e.ProductillDetails.ProductName,
                                      Quentity = e.ProductillDetails.Quentity,
                                      Total = e.ProductillDetails.Total,
                                      Id = e.ProductillDetails.Id
                                  }).ToList()

                              }).FirstOrDefault(e=>e.Customer.Id==id);

            return View("Create",customerViewss);
        }

        [HttpPost]
        public ActionResult Edit(CustomerViewModel model)
        {
            if (ModelState.IsValid)
            {
                var customer = model.Customer;
                _context.Entry(customer).State=System.Data.Entity.EntityState.Modified;
                _context.SaveChanges();
                

                var productbill=model.ProductBill;
                _context.Entry(productbill).State = System.Data.Entity.EntityState.Modified;
                _context.SaveChanges();

                foreach (var item in model.ProductillDetailList.Where(e=>e.Id!=0))
                {
                    item.ProductBillId = productbill.Id;

                    _context.Entry(productbill).State = System.Data.Entity.EntityState.Modified;
                    _context.SaveChanges();
                }
                foreach (var item in model.ProductillDetailList.Where(e => e.Id == 0))
                {
                    item.ProductBillId = productbill.Id;
                    _context.productsBillDetails.Add(item);
                    _context.SaveChanges();

                }
                var bill = model.Bill;
                _context.Entry(bill).State = System.Data.Entity.EntityState.Modified;
                _context.SaveChanges();

                return RedirectToAction("Index");
            }
            return View("Create",model);
        }

        public ActionResult Delete(int id) 
        {

            var cust=_context.customers.Find(id);
            _context.customers.Remove(cust);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AddProductInBill(CustomerViewModel customerModel)
        {
            if (customerModel.ProductillDetails != null)
            {
                customerModel.ProductillDetailList.Add(customerModel.ProductillDetails);
            }
            return PartialView("~/Views/Customer/_ProductBillList.cshtml", customerModel);
        }
        [HttpGet]
        public JsonResult GetProduct(string SearchText)
        {
            var result = _context.products.Where(e => e.ProductName.Contains(SearchText)).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetProductById(int id)
        {
            var result = _context.products.Where(e => e.Id == id).FirstOrDefault();
            return Json(result, JsonRequestBehavior.AllowGet);

        }
    }
}