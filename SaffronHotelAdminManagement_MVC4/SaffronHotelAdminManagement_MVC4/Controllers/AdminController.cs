using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using PagedList;
using SaffronHotelAdminManagement_MVC4;

namespace SaffronHotelAdminManagement_MVC4.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin

        //Saffron_DBEntities db = new Saffron_DBEntities(); 
        //SaffronMo db = new Saffron_DBEntities();
        SaffronModel db = new SaffronModel();

        clsMail mail = new clsMail();

        public AdminController()
        {
            ViewData["TempData"] = db.ItemMasters.Where(a => a.ReOrderLevel != null && a.ReOrderLevel > 0 && (((a.PurchaseDetails.Count > 0) ? a.PurchaseDetails.Sum(am => am.Quantity) : 0) - ((a.ItemUsedStocks.Count > 0) ? a.ItemUsedStocks.Sum(am => am.Quantity) : 0)) <= a.ReOrderLevel).ToList();
            ViewData["PendingOrderNotificationData"] = db.OrderMasters.Where(a => a.IsCancle.Value == false && a.IsComplete.Value == false && a.IsProcess.Value == false).ToList();
            ViewData["ProcessOrderNotificationData"] = db.OrderMasters.Where(a => a.IsProcess.Value==true).ToList();
            ViewData["PaymentRemaingOfOrderNotification"] = db.Payments.Where(a => a.OrderMaster.OrderDetailMasters.Sum(am => am.Amount.Value) - a.OrderMaster.Payments.Sum(am => am.Amount.Value) > 0 && a.OrderMaster.IsComplete.Value==true).ToList();
            //ViewData["PaymentRemaingOfOrderNotification"] = db.OrderMasters.Where(a=>a.Payments.f)
        }

        public ActionResult Index()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            ViewBag.TotalCustomers = db.CustomerDetails.Count();
            ViewBag.TotalOrders = db.OrderMasters.Count();
            ViewBag.TotalTabels = db.TableMasters.Count();
            ViewBag.TotalVendors = db.Vendors.Count();
            ViewBag.TotalWaiters = db.WaiterMasters.Count();
            ViewBag.TotalPendingOrders = db.OrderMasters.Where(a => a.IsCancle.Value == false && a.IsComplete.Value == false && a.IsProcess.Value == false && a.Date == DateTime.Now).Count();
            ViewBag.TotalProcessOrders = db.OrderMasters.Where(a => a.IsProcess.Value && a.Date == DateTime.Now).Count();
            ViewBag.TotalCompleteOrders = db.OrderMasters.Where(a => a.IsComplete.Value && a.Date == DateTime.Now).Count();
            ViewBag.TotalCencelOrders = db.OrderMasters.Where(a => a.IsCancle.Value && a.Date == DateTime.Now).Count();
            ViewBag.TotalPurchases = db.Purchases.Count();
            ViewBag.TotalMenuItems = db.ItemMasters.Where(a => a.ItemCategory.IsMenuItem.Value).ToList().Count;
            ViewBag.TotalAssests = db.ItemMasters.Where(a => a.ItemCategory.IsAssets.Value).ToList().Count;
            ViewBag.TotalRawMetirial = db.ItemMasters.Where(a => a.ItemCategory.IsRawMetirial.Value).ToList().Count;

            return View();
        }
        [HttpPost]
        public JsonResult TopSale_Item()
        {
            var list1 = db.OrderDetailMasters.Take(10).OrderByDescending(a => a.Quantity).ToList();
            var list = list1.Select(a => new
            {
                a.Quantity,
                a.ItemMaster.ItemName
            }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public string Encryption(string password)
        {
            string enPassword = "";
            try
            {
                var plainTextBytes = Encoding.UTF8.GetBytes(password);
                enPassword = Convert.ToBase64String(plainTextBytes);
            }
            catch { }
            return enPassword;
        }
        public string Decryption(string cyphertext)
        {
            string dePassword = "";
            try
            {
                var encodedBytes = Convert.FromBase64String(cyphertext);
                dePassword = Encoding.UTF8.GetString(encodedBytes);
            }
            catch
            { }
            return dePassword;
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            try
            {
                // string dec = Decryption(password);
                Admin ad = db.Admins.Where(a => a.Email == username && a.Password == password).FirstOrDefault();
                if (ad != null)
                {
                    Session["Admin_Id"] = ad.Admin_ID;
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.errorMsg = "Username or password is invalid";
                }
            }
            catch
            { ViewBag.errorMsg = "Technical error"; }
            return View();
        }
        public ActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ForgotPassword(string username)
        {
            try
            {
                Admin ad = db.Admins.Where(a => a.Email == username).FirstOrDefault();
                if (ad != null)
                {
                    string msg = "Your password is " + Decryption(ad.Password.ToString());
                    mail.mailSend(username, "Saffron Hotel Admin Password", msg);
                    return RedirectToAction("Login");
                }
                else
                {
                    ViewBag.errorMsg = "Email id is in valid";
                }
            }
            catch { }
            return View();
        }
        public ActionResult AdminInformation()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["Admin_Id"]);
            return View(db.Admins.Where(a => a.Admin_ID == id).ToList());
        }
        public ActionResult AdminCreate()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }
        [HttpPost]
        public ActionResult AdminCreate(string AdminName, string Email, string Password, string RPassword)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                if (Password.Equals(RPassword))
                {
                    Admin ad = db.Admins.Where(a => a.Email == Email).FirstOrDefault();
                    if (ad != null)
                    {
                        ViewBag.errorMsg = "Email id is already exist.";
                    }
                    else
                    {
                        ad = new Admin();
                        ad.Name = AdminName;
                        ad.Email = Email;
                        ad.Password = Encryption(Password);
                        ad.IsActive = true;
                        db.Admins.Add(ad);
                        int i = db.SaveChanges();
                        if (i > 0)
                        {
                            return RedirectToAction("AdminInformation");
                        }
                        else
                        { ViewBag.errorMsg = "Technical error | failed to create admin"; }
                    }
                }
                else
                {
                    ViewBag.errorMsg = "Password does not matched.";
                }
            }
            catch { ViewBag.errorMsg = "Technical error"; }
            return View();
        }
        public ActionResult AdminView(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                Admin ad = db.Admins.Find(id);
                if (ad != null)
                {
                    Session["Admin_Id_Update"] = id;
                    return View(ad);
                }
                else
                {
                    ViewBag.error = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        [HttpPost]
        public ActionResult AdminView(string Email, string AdminName)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["Admin_Id_Update"]);
            try
            {

                Admin ad = db.Admins.Where(a => a.Email == Email && a.Admin_ID != id).FirstOrDefault();
                if (ad != null)
                {
                    ViewBag.errorMsg = "Email is already exist";
                }
                else
                {
                    ad = db.Admins.Find(id);
                    ad.Email = Request["Email"];
                    ad.Name = Request["AdminName"];
                    db.Entry(ad).State = System.Data.Entity.EntityState.Modified;
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        Session["Admin_Id_Update"] = null;
                        return RedirectToAction("AdminInformation");
                    }
                    else
                    {
                        ViewBag.errorMsg = "Technical error | Failed to update";
                    }
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View(db.Admins.Find(id));
        }
        public ActionResult AdminDelete(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                Admin ad = db.Admins.Find(id);
                if (ad != null)
                {
                    Session["Admin_Id_Delete"] = id;
                    return View(ad);
                }
                else
                {
                    ViewBag.error = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        [HttpPost]
        public ActionResult AdminDelete()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["Admin_Id_Delete"]);
            try
            {

                Admin ad = db.Admins.Find(id);
                if (ad != null)
                {
                    db.Admins.Remove(ad);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        Session["Admin_Id_Delete"] = null;
                        return RedirectToAction("AdminInformation");
                    }
                }
                else
                {
                    ViewBag.error = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View(db.Admins.Find(id));
        }
        public ActionResult AdminChangePassword()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                int id = Convert.ToInt32(Session["Admin_Id"]);
                Admin ad = db.Admins.Find(id);
                if (ad == null)
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
                else
                {
                    Session["Admin_Id_ChangePassword_Update"] = id;
                    return View(ad);
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        [HttpPost]
        public ActionResult AdminChangePassword(string chpassword)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["Admin_Id_ChangePassword_Update"]);
            try
            {
                Admin ad = db.Admins.Find(id);
                if (ad != null)
                {
                    ad.Password = chpassword;
                    db.Entry(ad).State = System.Data.Entity.EntityState.Modified;
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        Session["Admin_Id_ChangePassword_Update"] = null;
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ViewBag.errorMsg = "Technical error | Failed to change password";
                    }
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        public ActionResult FloorInformation()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            return View(db.Floors.ToList());
        }
        public ActionResult FloorCreate()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }
        [HttpPost]
        public ActionResult FloorCreate(string floorno)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {

                Floor f = db.Floors.Where(a => a.FloorNo == floorno).FirstOrDefault();
                if (f == null)
                {
                    f = new Floor();
                    f.FloorNo = floorno;
                    db.Floors.Add(f);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        return RedirectToAction("FloorInformation");
                    }
                    else
                    { ViewBag.errorMsg = "Technical error | Filed to add floor"; }
                }
                else
                {
                    ViewBag.errorMsg = "This floor is already available";
                }
            }
            catch
            { ViewBag.errorMsg = "Technical error"; }
            return View();
        }
        public ActionResult FloorView(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                Floor f = db.Floors.Find(id);
                if (f != null)
                {
                    Session["Floor_Id_Update"] = id;
                    return View(f);
                }
                else
                {
                    ViewBag.error = "Technical error | Data not found";
                }
            }
            catch
            { ViewBag.errorMsg = "Technical error"; }
            return View();
        }
        [HttpPost]
        public ActionResult FloorView(string floorno)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["Floor_Id_Update"]);
            try
            {
                Floor f = db.Floors.Where(a => a.FloorNo == floorno && a.Floor_ID != id).FirstOrDefault();
                if (f != null)
                {
                    ViewBag.errorMsg = "Floor is already exixst";
                }
                else
                {
                    f = db.Floors.Find(id);
                    f.FloorNo = floorno;
                    db.Entry(f).State = System.Data.Entity.EntityState.Modified;
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        Session["Floor_Id_Update"] = null;
                        return RedirectToAction("FloorInformation");
                    }
                    else
                    {
                        ViewBag.errorMsg = "Technical error | Failed to update";
                    }
                }

            }
            catch
            { ViewBag.errorMsg = "Technical error"; }
            return View(db.Floors.Find(id));
        }
        public ActionResult FloorDelete(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                Floor f = db.Floors.Find(id);
                if (f != null)
                {
                    Session["Floor_Id_Delete"] = id;
                    return View(f);
                }
                else
                {
                    ViewBag.error = "Technical error | Data not found";
                }
            }
            catch
            { ViewBag.errorMsg = "Technical error"; }
            return View();
        }
        [HttpPost]
        public ActionResult FloorDelete()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["Floor_Id_Delete"]);
            try
            {
                Floor f = db.Floors.Find(id);
                if (f != null)
                {
                    db.Floors.Remove(f);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        Session["Floor_Id_Delete"] = null;
                        return RedirectToAction("FloorInformation");
                    }
                    else
                    {
                        ViewBag.error = "Technical error | Filed to delete";
                    }
                }
                else
                {
                    ViewBag.error = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View(db.Floors.Find(id));
        }
        public ActionResult ItemCategoryInformation(int? page)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page.HasValue) ? page.Value : 1;
            var list = db.ItemCategories.ToList();
            return View(list.ToPagedList(pageIndex, pageSize));
        }
        public ActionResult ItemCategoryCreate()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }
        [HttpPost]
        public ActionResult ItemCategoryCreate(string categoryName, string MenuItem, string Assets, string Service, string KitchenRawMetiral)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                ItemCategory ic = db.ItemCategories.Where(a => a.ItemCategoryName == categoryName).FirstOrDefault();
                if (ic != null)
                {
                    ViewBag.errorMsg = "This Item Category is already available";
                }
                else
                {
                    ic = new ItemCategory();
                    ic.ItemCategoryName = categoryName;
                    if (MenuItem == "on")
                    { ic.IsMenuItem = true; }
                    else
                    { ic.IsMenuItem = false; }

                    if (Assets == "on")
                    { ic.IsAssets = true; }
                    else
                    { ic.IsAssets = false; }

                    if (Service == "on")
                    { ic.IsService = true; }
                    else
                    { ic.IsService = false; }

                    if (KitchenRawMetiral == "on")
                    { ic.IsRawMetirial = true; }
                    else
                    { ic.IsRawMetirial = false; }
                    db.ItemCategories.Add(ic);
                    int i = db.SaveChanges();
                    if (i > 0)
                    { return RedirectToAction("ItemCategoryInformation"); }
                    else
                    { ViewBag.errorMsg = "Technical error | Filed to add Item Category"; }


                }
            }
            catch { ViewBag.errorMsg = "Technical error"; }
            return View();
        }
        public ActionResult ItemCategoryView(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                ItemCategory ic = db.ItemCategories.Find(id);
                if (ic != null)
                {
                    Session["ItemCategory_Id_Update"] = id;
                    return View(ic);
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch { ViewBag.errorMsg = "Technical error"; }
            return View();
        }
        [HttpPost]
        public ActionResult ItemCategoryView(string categoryName, string MenuItem, string Assets, string Service, string KitchenRawMetiral)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["ItemCategory_Id_Update"]);
            try
            {
                ItemCategory ic = db.ItemCategories.Where(a => a.ItemCategoryName == categoryName && a.ItemCategory_ID != id).FirstOrDefault();
                if (ic != null)
                {
                    ViewBag.errorMsg = "Itemcategory is already exist";
                    //Session["errorMsg"] = "Itemcategory is already exist";
                }
                else
                {
                    ic = db.ItemCategories.Find(id);
                    ic.ItemCategoryName = categoryName;
                    if (MenuItem == "on")
                    { ic.IsMenuItem = true; }
                    else
                    { ic.IsMenuItem = false; }
                    if (Assets == "on")
                    { ic.IsAssets = true; }
                    else
                    { ic.IsAssets = false; }
                    if (Service == "on")
                    { ic.IsService = true; }
                    else
                    { ic.IsService = false; }
                    if (KitchenRawMetiral == "on")
                    { ic.IsRawMetirial = true; }
                    else
                    { ic.IsRawMetirial = false; }
                    db.Entry(ic).State = System.Data.Entity.EntityState.Modified;
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        Session["ItemCategory_Id_Update"] = null;
                        return RedirectToAction("ItemCategoryInformation");
                    }
                    else
                    { ViewBag.errorMsg = "Technical error | Failed to update"; }
                }
            }
            catch
            { ViewBag.errorMsg = "Technical error"; }
            return View(db.ItemCategories.Find(id));
        }
        public ActionResult ItemCategoryDelete(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                ItemCategory ic = db.ItemCategories.Find(id);
                if (ic != null)
                {
                    Session["ItemCategory_Id_Delete"] = id;
                    return View(ic);
                }
                else
                {
                    ViewBag.error = "Technical error | Data not found";
                }

            }
            catch
            { ViewBag.errorMsg = "Technical error"; }
            return View();
        }
        [HttpPost]
        public ActionResult ItemCategoryDelete()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["ItemCategory_Id_Delete"]);
            try
            {
                ItemCategory ic = db.ItemCategories.Find(id);
                if (ic != null)
                {
                    db.ItemCategories.Remove(ic);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        Session["ItemCategory_Id_Delete"] = null;
                        return RedirectToAction("ItemCategoryInformation");
                    }
                    else
                    {
                        ViewBag.error = "Technical error | Filed to delete";
                    }
                }
                else
                {
                    ViewBag.error = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.error = "Technical error";
            }
            return View(db.ItemCategories.Find(id));
        }
        public ActionResult ItemMasterInformation()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            ViewBag.ItemCategory = db.ItemCategories.OrderBy(a => a.ItemCategoryName).ToList();
            return View(db.ItemMasters.ToList());
        }
        [HttpPost]
        public JsonResult ItemMasterInformation(int ItemCategory_Id, string search_ItemName, string search_GST, int page = 0)
        {
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page > 0) ? page : 1;
            var list1 = db.ItemMasters.Where(a => a.ItemCategory_ID == ItemCategory_Id).ToList();
            if (search_ItemName == "" && search_GST == "")
            {
                list1 = db.ItemMasters.Where(a => a.ItemCategory_ID == ItemCategory_Id).ToList();
            }
            else if (search_ItemName != "" && search_GST == "")
            {
                list1 = list1.Where(a => a.ItemName.ToLower().Contains(search_ItemName.ToLower())).ToList();
            }
            else if (search_ItemName == "" && search_GST != "")
            {
                list1 = list1.Where(a => a.CGST.Value.Equals(Convert.ToDouble(search_GST))).ToList();
            }
            else if (search_ItemName != "" && search_GST != "")
            {
                list1 = list1.Where(a => a.ItemName.ToLower().Contains(search_ItemName) && a.CGST.Value.Equals(Convert.ToDouble(search_GST))).ToList();
            }
            var list = list1.Select(a => new
            {
                a.ItemCategory_ID.Value,
                a.ItemMaster_ID,
                a.ItemName,
                Veg = (a.IsVeg == null) ? "-" : (a.IsVeg.Value) ? "Veg" : "Non-Veg",
                a.Rate,
                CGST = (a.CGST == null) ? 0 : a.CGST,
                SGST = (a.SGST == null) ? 0 : a.SGST,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordCount = list1.Count,
            }).ToPagedList(pageIndex, pageSize);
            ViewBag.PageList = list;
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        //public JsonResult ItemMasterInformation_FilterItemName(int ItemCategory_Id,string search_ItemName, int page = 0)
        //{
        //    int pageSize = 10;
        //    int pageIndex = 1;
        //    pageIndex = (page > 0) ? page : 1;
        //    var list1=db.ItemMasters.Where(a => a.ItemCategory_ID == ItemCategory_Id)
        //}
        public ActionResult ItemMasterCreate()
        {
            try
            {
                if (Session["Admin_Id"] == null)
                {
                    return RedirectToAction("Login");
                }
            }
            catch
            {

            }
            ViewBag.ItemCategory = db.ItemCategories.OrderBy(a => a.ItemCategoryName).ToList();
            return View();
        }
        [HttpPost]
        public ActionResult ItemMasterCreate(string ItemName, string veg, string Rate, string cgst, string sgst, string ROLevel, string MinQty)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                ItemMaster im = new ItemMaster();
                im.ItemName = ItemName;
                im.Rate = Convert.ToDouble(Rate);
                im.CGST = Convert.ToDouble(cgst);
                im.SGST = Convert.ToDouble(sgst);
                im.ReOrderLevel = Convert.ToInt32(ROLevel);
                im.MinimumQuantatity = Convert.ToInt32(MinQty);
                if (veg == "on")
                {
                    im.IsVeg = true;
                }
                else if (veg == "on")
                {
                    im.IsVeg = false;
                }
                else
                {
                    im.IsVeg = null;
                }
                im.ItemCategory_ID = Convert.ToInt32(Request["ItemCategoryId"]);
                db.ItemMasters.Add(im);
                int i = db.SaveChanges();
                if (i > 0)
                {
                    return RedirectToAction("ItemMasterInformation");
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Failed to add Item";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        public JsonResult ItemMasterCreate_CheckMenuItem(int ItemCategory_Id)
        {
            var list1 = db.ItemCategories.Find(ItemCategory_Id).IsMenuItem.Value;
            ViewBag.ItemCategory = db.ItemCategories.OrderBy(a => a.ItemCategoryName).ToList();
            return Json(list1, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ItemMasterView(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                ViewBag.ItemCategory = db.ItemCategories.OrderBy(a => a.ItemCategoryName).ToList();
                ItemMaster im = db.ItemMasters.Find(id);
                if (im != null)
                {

                    Session["ItemMaster_Id_Update"] = id;
                    return View(im);
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        [HttpPost]
        public ActionResult ItemMasterView(string ItemName, string veg, string Rate, string cgst, string sgst, string ROLevel, string MinQty)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["ItemMaster_Id_Update"]);
            try
            {
                ItemMaster im = db.ItemMasters.Find(id);
                if (im != null)
                {
                    im.ItemName = ItemName;
                    im.Rate = Convert.ToDouble(Rate);
                    im.CGST = Convert.ToDouble(cgst);
                    im.SGST = Convert.ToDouble(sgst);
                    im.ReOrderLevel = Convert.ToInt32(ROLevel);
                    im.MinimumQuantatity = Convert.ToInt32(MinQty);
                    if (veg == "on")
                    {
                        im.IsVeg = true;
                    }
                    else if (veg == "on")
                    {
                        im.IsVeg = false;
                    }
                    else
                    {
                        im.IsVeg = null;
                    }
                    im.ItemCategory_ID = Convert.ToInt32(Request["ItemCategoryId"]);
                    db.Entry(im).State = System.Data.Entity.EntityState.Modified;
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        Session["ItemMaster_Id_Update"] = null;
                        return RedirectToAction("ItemMasterInformation");
                    }
                    else
                    {
                        ViewBag.errorMsg = "Technical error | Failed to update Item";
                    }
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error | Failed to update Item";
            }
            return View(db.ItemMasters.Find(id));
        }
        public ActionResult ItemMasterDelete(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            ViewBag.ItemCategory = db.ItemCategories.OrderBy(a => a.ItemCategoryName).ToList();
            try
            {
                ItemMaster im = db.ItemMasters.Find(id);
                if (im != null)
                {

                    Session["ItemMaster_Id_Delete"] = id;
                    return View(im);
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        [HttpPost]
        public ActionResult ItemMasterDelete()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["ItemMaster_Id_Delete"]);
            try
            {
                ItemMaster im = db.ItemMasters.Find(id);
                if (im != null)
                {
                    db.ItemMasters.Remove(im);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        Session["ItemMaster_Id_Delete"] = null;
                        return RedirectToAction("ItemMasterInformation");
                    }
                    else
                    {
                        ViewBag.errorMsg = "Technical error | Failed to delete item";
                    }
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View(db.ItemMasters.Find(id));
        }
        public ActionResult TableTypeInformation()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            return View(db.TableTypes.ToList());
        }
        public ActionResult TableTypeCreate()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }
        [HttpPost]
        public ActionResult TableTypeCreate(string tableType)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                TableType tp = db.TableTypes.Where(a => a.TableName == tableType).FirstOrDefault();
                if (tp != null)
                {
                    ViewBag.errorMsg = "This Table type is already available";
                }
                else
                {
                    tp = new TableType();
                    tp.TableName = tableType;
                    db.TableTypes.Add(tp);
                    int i = db.SaveChanges();
                    if (i > 0)
                    { return RedirectToAction("TableTypeInformation"); }
                    else
                    { ViewBag.errorMsg = "Technical error | Filed to add table type"; }
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        public ActionResult TableTypeView(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                TableType tp = db.TableTypes.Find(id);
                if (tp != null)
                {
                    Session["TableType_Id_Update"] = id;
                    return View(tp);
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        [HttpPost]
        public ActionResult TableTypeView(string tableType)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["TableType_Id_Update"]);
            try
            {
                TableType tp = db.TableTypes.Where(a => a.TableName == tableType && a.TableType_ID != id).FirstOrDefault();
                if (tp != null)
                {
                    ViewBag.errorMsg = "This Table type is already available";
                }
                else
                {
                    tp = db.TableTypes.Find(id);
                    tp.TableName = tableType;
                    db.Entry(tp).State = System.Data.Entity.EntityState.Modified;
                    int i = db.SaveChanges();
                    if (i > 0)
                    { return RedirectToAction("TableTypeInformation"); }
                    else
                    { ViewBag.errorMsg = "Technical error | Filed to update table type"; }
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View(db.TableTypes.Find(id));
        }
        public ActionResult TableTypeDelete(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                TableType tp = db.TableTypes.Find(id);
                if (tp != null)
                {
                    Session["TableType_Id_Delete"] = id;
                    return View(tp);
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        [HttpPost]
        public ActionResult TableTypeDelete(string tableType)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["TableType_Id_Update"]);
            try
            {
                TableType tp = db.TableTypes.Find(id);
                if (tp != null)
                {
                    db.TableTypes.Remove(tp);
                    int i = db.SaveChanges();
                    if (i > 0)
                    { return RedirectToAction("TableTypeInformation"); }
                    else
                    { ViewBag.errorMsg = "Technical error | Filed to delete table type"; }

                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View(db.TableTypes.Find(id));
        }
        public ActionResult TableMasterInformation(int? page)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int pageSize = 10;
            int pageIndex = 1;
            ViewBag.TableTypeList = db.TableTypes.OrderBy(a => a.TableName).ToList();
            ViewBag.FloorList = db.Floors.OrderBy(a => a.FloorNo).ToList();
            pageIndex = (page.HasValue) ? page.Value : 1;
            var list = db.TableMasters.OrderBy(a => a.TableNo).ToList();
            return View(list.ToPagedList(pageIndex, pageSize));
        }
        public JsonResult TableMasterInformation_FilterTableType(int TableTypeId, int FloorId, int page = 0)
        {
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page > 0) ? page : 1;
            var list1 = db.TableMasters.Where(a => a.TableType_ID == TableTypeId && a.Floor_ID == FloorId).OrderBy(a => a.TableNo).ToList();
            var list = list1.Select(a => new
            {
                a.TableType_ID.Value,
                a.Table_ID,
                a.TableNo,
                a.TableCapacity,
                a.Floor.FloorNo,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordCount = list1.Count,
            }).ToPagedList(pageIndex, pageSize);
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public ActionResult TableMasterCreate()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                ViewBag.TableType = db.TableTypes.OrderBy(a => a.TableName).ToList();
                ViewBag.Floor = db.Floors.OrderBy(a => a.FloorNo).ToList();
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        [HttpPost]
        public ActionResult TableMasterCreate(string tableCapacity, string tableNo)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                int fid = Convert.ToInt32(Request["floor_Id"]);
                int tabletype_id = Convert.ToInt32(Request["tableType_Id"]);
                TableMaster tm = db.TableMasters.Where(a => a.TableNo == tableNo && a.Floor_ID == fid && a.TableType_ID == tabletype_id).FirstOrDefault();
                if (tm != null)
                {
                    ViewBag.errorMsg = "This table is already exist";
                }
                else
                {
                    tm = new TableMaster();
                    tm.Floor_ID = Convert.ToInt32(Request["floor_Id"]);
                    tm.TableType_ID = Convert.ToInt32(Request["tableType_Id"]);
                    tm.TableCapacity = Convert.ToInt32(tableCapacity);
                    tm.TableNo = tableNo;
                    db.TableMasters.Add(tm);
                    int i = db.SaveChanges();
                    if (i > 0)
                    { return RedirectToAction("TableMasterInformation"); }
                    else
                    { ViewBag.errorMsg = "Technical error | Filed to add Table"; }
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            ViewBag.TableType = db.TableTypes.OrderBy(a => a.TableName).ToList();
            ViewBag.Floor = db.Floors.OrderBy(a => a.FloorNo).ToList();
            return View();
        }
        public ActionResult TableMasterView(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                ViewBag.TableType = db.TableTypes.OrderBy(a => a.TableName).ToList();
                ViewBag.Floor = db.Floors.OrderBy(a => a.FloorNo).ToList();
                TableMaster tm = db.TableMasters.Find(id);
                if (tm != null)
                {
                    Session["TableMaster_Id_Update"] = id;
                    return View(tm);
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        [HttpPost]
        public ActionResult TableMasterView(string tableCapacity, string tableNo)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["TableMaster_Id_Update"]);
            try
            {
                //TableMaster tm=db.TableMasters.Where(a=>a.Table_ID!=id && a.)
                int fid = Convert.ToInt32(Request["floor_Id"]);
                int tabletype_id = Convert.ToInt32(Request["tableType_Id"]);
                TableMaster tm = db.TableMasters.Where(a => a.TableNo == tableNo && a.Floor_ID == fid && a.TableType_ID == tabletype_id).FirstOrDefault();
                if (tm != null)
                {
                    ViewBag.errorMsg = "This table is already exist";
                }
                else
                {
                    tm = db.TableMasters.Find(id);
                    if (tm != null)
                    {
                        tm.Floor_ID = Convert.ToInt32(Request["floor_Id"]);
                        tm.TableType_ID = Convert.ToInt32(Request["tableType_Id"]);
                        tm.TableCapacity = Convert.ToInt32(tableCapacity);
                        tm.TableNo = tableNo;
                        db.Entry(tm).State = System.Data.Entity.EntityState.Modified;
                        int i = db.SaveChanges();
                        if (i > 0)
                        {
                            Session["TableMaster_Id_Update"] = null;
                            return RedirectToAction("TableMasterInformation");
                        }
                        else
                        { ViewBag.errorMsg = "Technical error | Filed to update Table"; }
                    }
                    else
                    {
                        ViewBag.errorMsg = "Technical error | Data not found";
                    }
                }

            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            ViewBag.TableType = db.TableTypes.OrderBy(a => a.TableName).ToList();
            ViewBag.Floor = db.Floors.OrderBy(a => a.FloorNo).ToList();
            return View(db.TableMasters.Find(id));
        }
        public ActionResult TableMasterDelete(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                ViewBag.TableType = db.TableTypes.OrderBy(a => a.TableName).ToList();
                ViewBag.Floor = db.Floors.OrderBy(a => a.FloorNo).ToList();
                TableMaster tm = db.TableMasters.Find(id);
                if (tm != null)
                {
                    Session["TableMaster_Id_Delete"] = id;
                    return View(tm);
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        [HttpPost]
        public ActionResult TableMasterDelete()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["TableMaster_Id_Delete"]);
            try
            {
                TableMaster tm = db.TableMasters.Find(id);
                if (tm != null)
                {
                    db.TableMasters.Remove(tm);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        Session["TableMaster_Id_Delete"] = null;
                        return RedirectToAction("TableMasterInformation");
                    }
                    else
                    { ViewBag.errorMsg = "Technical error | Filed to delete Table"; }
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            ViewBag.TableType = db.TableTypes.OrderBy(a => a.TableName).ToList();
            ViewBag.Floor = db.Floors.OrderBy(a => a.FloorNo).ToList();
            return View(db.TableMasters.Find(id));
        }
        public ActionResult PurchaseInformation(int? page)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page.HasValue) ? page.Value : 1;
            ViewBag.ItemCategoryList = db.ItemCategories.Where(a => a.IsRawMetirial.Value || a.IsAssets.Value).OrderBy(a => a.ItemCategoryName).ToList();
            ViewBag.Date = DateTime.Now.ToString("yyyy-MM-dd");
            var list = db.Purchases.ToList();
            return View(list.ToPagedList(pageIndex, pageSize));
        }
        [HttpPost]
        public JsonResult PurchaseInformation_ItemCategoryWise_Bind(int ItemCategory_Id, string stDate, string enDate, int page = 0)
        {
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page > 0) ? page : 1;
            DateTime stDate1 = Convert.ToDateTime(stDate);
            DateTime enDate1 = Convert.ToDateTime(enDate);
            Session["stDate_PurchaseInformation_Print"] = stDate1;
            Session["enDate_PurchaseInformation_Print"] = enDate1;
            var list1 = db.Purchases.Where(a => a.PurchaseDetails.Select(m1 => m1.ItemMaster.ItemCategory_ID).Contains(ItemCategory_Id) && a.Date >= stDate1 && a.Date <= enDate1).OrderBy(a => a.Date).ToList();
            Session["itemCategory_Name_PurchaseInformation_Print"] = db.ItemCategories.Where(a => a.ItemCategory_ID == ItemCategory_Id).Select(a => a.ItemCategoryName).FirstOrDefault();
            Session["itemCategory_Id_PurchaseInformation_Print"] = ItemCategory_Id;
            var list = list1.Select(a => new
            {
                a.Purchase_ID,
                a.BillNo,
                Amount = a.PurchaseDetails.Sum(m => m.Amount),
                date = a.Date.Value.ToString("dd-MM-yyyy"),
                ItemCategory_Id = ItemCategory_Id,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordCount = list1.Count,
            }).ToPagedList(pageIndex, pageSize);
            ViewBag.PageList = list;
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        //-----------------------------------------------------------------
        public ActionResult PurchaseInformation_Print()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            if (Session["itemCategory_Id_PurchaseInformation_Print"] == null || Session["itemCategory_Name_PurchaseInformation_Print"] == null || Session["stDate_PurchaseInformation_Print"] == null || Session["enDate_PurchaseInformation_Print"] == null)
            {
                return RedirectToAction("Login");
            }
            string TitleData = "Category :- " + Convert.ToString(Session["itemCategory_Name_PurchaseInformation_Print"]) + " " + " : " + " From  " + Convert.ToDateTime(Session["stDate_PurchaseInformation_Print"]).ToString("dd-MM-yyyy") + "  To  " + Convert.ToDateTime(Session["enDate_PurchaseInformation_Print"]).ToString("dd-MM-yyyy");
            ViewData["TitleData_PurchaseInformationPrint"] = TitleData;
            return View();
        }
        public ActionResult PurchaseInformation_Details(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            ViewBag.PurchaseDetails_Id = id;
            Session["Purchase_Id_Common"] = id;
            string BillNo = db.Purchases.Find(id).BillNo;
            ViewBag.BillNo = BillNo;
            string vendorname=db.Purchases.Find(id).Vendor.VendorName;
            ViewBag.VendorName = vendorname;
            string city=db.Purchases.Find(id).Vendor.City;
            ViewBag.City = city;
            Session["BillNo_PurchaseInformation_Details_Print"] = BillNo;
            Session["vendorname_PurchaseInformation_Details_Print"] = vendorname;
            Session["city_PurchaseInformation_Details_Print"] = city;
            Session["Purchase_Id_PurchaseInformation_Details_Print"] = id;
            Session["Date__PurchaseInformation_Details_Print"] = db.Purchases.Find(id).Date;
            return View(db.PurchaseDetails.Where(a => a.Purchase_ID == id).ToList());
        }

        public ActionResult PurchaseInformation_Details_Print()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            if (Session["BillNo_PurchaseInformation_Details_Print"] == null || Session["vendorname_PurchaseInformation_Details_Print"] == null || Session["city_PurchaseInformation_Details_Print"] == null || Session["Purchase_Id_PurchaseInformation_Details_Print"] == null || Session["Date__PurchaseInformation_Details_Print"]==null)
            {
                return RedirectToAction("Login");
            }
            string TitleData = "Bill Number : " + Convert.ToString(Session["BillNo_PurchaseInformation_Details_Print"]) + "   Date : " + Convert.ToDateTime(Session["Date__PurchaseInformation_Details_Print"]).ToString("dd-MM-yyyy") + "  " + "Vendor Name : " + Convert.ToString(Session["vendorname_PurchaseInformation_Details_Print"]) + "  From  " + Convert.ToString(Session["city_PurchaseInformation_Details_Print"]);
            ViewData["TitleData_PurchaseInformation_Details_Print"] = TitleData;
            return View();
        }
        public ActionResult PurchaseInformation_Vendor(int? page)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page.HasValue) ? page.Value : 1;
            ViewBag.VendorList = db.Vendors.OrderBy(a => a.VendorName).ToList();
            ViewBag.Date = DateTime.Now.ToString("yyyy-MM-dd");
            var list = db.Purchases.ToList();
            return View(list.ToPagedList(pageIndex, pageSize));
        }
        [HttpPost]
        public JsonResult PurchaseInformation_VendorNameWise_Bind(int Vendor_Id, string stDate, string enDate, int page = 0)
        {
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page > 0) ? page : 1;
            DateTime stDate1 = Convert.ToDateTime(stDate);
            DateTime enDate1 = Convert.ToDateTime(enDate);
            var list1 = db.PurchaseDetails.Where(a => a.Purchase.Vendor_ID == Vendor_Id && a.Purchase.Date > stDate1 && a.Purchase.Date < enDate1).OrderBy(a => a.ItemMaster.ItemName).ToList();
            var list = list1.Select(a => new
            {
                a.Purchase_ID.Value,
                a.ItemMaster.ItemName,
                a.Purchase.BillNo,
                a.Amount,
                date = a.Purchase.Date.Value.ToString("dd-MM-yyyy"),
                Vendor_Id = Vendor_Id,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordCount = list1.Count,
            }).ToPagedList(pageIndex, pageSize);
            ViewBag.PageList = list;
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PurchaseInformation_Details_View(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                PurchaseDetail pd = db.PurchaseDetails.Find(id);
                if (pd != null)
                {
                    ViewBag.ItemCategoryName = db.ItemCategories.Where(a => a.IsRawMetirial.Value || a.IsAssets.Value).OrderBy(a => a.ItemCategoryName).ToList();
                    ViewBag.ItemName = db.ItemMasters.OrderBy(a => a.ItemName).ToList();
                    Session["PurchaseInformation_Details_View"] = id;
                    return View(pd);
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }

            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        [HttpPost]
        public ActionResult PurchaseInformation_Details_View(string qty, string rate, string amt)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["PurchaseInformation_Details_View"]);
            try
            {
                PurchaseDetail pd = db.PurchaseDetails.Find(id);
                if (pd != null)
                {
                    pd.Quantity = Convert.ToInt32(qty);
                    pd.Rate = Convert.ToDouble(rate);
                    pd.Amount = Convert.ToInt32(qty) * Convert.ToDouble(rate);
                    pd.ItemMaster_ID = Convert.ToInt32(Request["ItemMasterId"]);
                    db.Entry(pd).State = System.Data.Entity.EntityState.Modified;
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        Session["PurchaseInformation_Details_View"] = null;
                        return RedirectToAction("PurchaseInformation_Details", new { id = Convert.ToInt32(Session["Purchase_Id_Common"]) });
                    }
                    else
                    {
                        ViewBag.errorMsg = "Technical error | Fiiled to update data";
                    }
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }

            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View(db.PurchaseDetails.Find(id));
        }
        public ActionResult PurchaseInformation_Details_Delete(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                PurchaseDetail pd = db.PurchaseDetails.Find(id);
                if (pd != null)
                {
                    ViewBag.ItemCategoryName = db.ItemCategories.Where(a => a.IsRawMetirial.Value || a.IsAssets.Value).OrderBy(a => a.ItemCategoryName).ToList();
                    ViewBag.ItemName = db.ItemMasters.OrderBy(a => a.ItemName).ToList();
                    db.PurchaseDetails.Remove(pd);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        return RedirectToAction("PurchaseInformation_Details", new { id = Convert.ToInt32(Session["Purchase_Id_Common"]) });
                    }
                    else
                    {
                        ViewBag.errorMsg = "Technical error | Failed to delete record";
                    }
                    //Session["PurchaseInformation_Details_Delete"] = id;
                    //return View(pd);
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }

            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }

        public ActionResult PurchaseInformation_Return_Details(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            ViewBag.PurchaseDetails_Id = id;
            ViewBag.BillNo = db.Purchases.Find(id).BillNo;
            ViewBag.VendorName = db.Purchases.Find(id).Vendor.VendorName;
            ViewBag.City = db.Purchases.Find(id).Vendor.City;
            ViewBag.Date = DateTime.Now.ToString("yyyy-MM-dd");
            Session["PurchaseInformation_Return_Add"] = id;
            var list = db.PurchaseDetails.Where(a => a.Purchase_ID == id).ToList();
            Session["PurchaseInformation_Return_purchaseDetails_count"] = list.Count;
            return View(list);
        }
        [HttpPost]
        public ActionResult PurchaseInformation_Return_Details(string date)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["PurchaseInformation_Return_Add"]);
            ViewBag.Date = DateTime.Now.ToString("yyyy-MM-dd");
            try
            {
                var check = db.PurchaseReturns.Where(a => a.Purchase_ID == id).FirstOrDefault();
                PurchaseReturn pr;
                PurchaseReturnDetail prd;
                if (check != null)
                {
                    pr = db.PurchaseReturns.Find(check.PurchaseReturn_ID);
                    pr.Purchase_ID = id;
                    pr.Date = Convert.ToDateTime(date);
                    db.Entry(pr).State = System.Data.Entity.EntityState.Modified;
                    int i = db.SaveChanges();
                    for (int j = 1; j <= Convert.ToInt32(Session["PurchaseInformation_Return_purchaseDetails_count"]); j++)
                    {
                        prd = new PurchaseReturnDetail();
                        prd.PurchaseReturn_ID = pr.PurchaseReturn_ID;
                        prd.ItemMaster_ID = Convert.ToInt32(Request["item_" + j]);
                        prd.Quatntity = Convert.ToInt32(Request["returnQty_" + j]);
                        prd.Remarks = Request["remak_" + j];
                        db.PurchaseReturnDetails.Add(prd);
                        db.SaveChanges();
                    }
                }
                else
                {
                    pr = new PurchaseReturn();
                    pr.Purchase_ID = id;
                    pr.Date = Convert.ToDateTime(date);
                    db.PurchaseReturns.Add(pr);
                    int i = db.SaveChanges();

                    for (int j = 1; j <= Convert.ToInt32(Session["PurchaseInformation_Return_purchaseDetails_count"]); j++)
                    {
                        prd = new PurchaseReturnDetail();
                        prd.PurchaseReturn_ID = pr.PurchaseReturn_ID;
                        prd.ItemMaster_ID = Convert.ToInt32(Request["item_" + j]);
                        prd.Quatntity = Convert.ToInt32(Request["returnQty_" + j]);
                        prd.Remarks = Request["remak_" + j];
                        db.PurchaseReturnDetails.Add(prd);
                        db.SaveChanges();
                    }
                }

                return RedirectToAction("PurchaseInformation");
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            ViewBag.BillNo = db.Purchases.Find(id).BillNo;
            ViewBag.VendorName = db.Purchases.Find(id).Vendor.VendorName;
            ViewBag.City = db.Purchases.Find(id).Vendor.City;
            return View(db.PurchaseDetails.Where(a => a.Purchase_ID == id).ToList());
        }
        public ActionResult PurchaseCreate(int Vendr_Id = 0)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            Session["Vendor_Id"] = null;
            if (Vendr_Id > 0)
            {
                Session["Vendor_Id"] = Vendr_Id;
            }
            ViewBag.VendorName = db.Vendors.OrderBy(a => a.VendorName).ToList();
            ViewBag.Date = DateTime.Now.ToString("yyyy-MM-dd");
            return View();
        }
        [HttpPost]
        public ActionResult PurchaseCreate(string number, string VendorId)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                Purchase p = new Purchase();
                p.Vendor_ID = Convert.ToInt32(VendorId);
                p.Date = Convert.ToDateTime(Request["date"]);
                p.BillNo = number;
                db.Purchases.Add(p);
                int i = db.SaveChanges();
                if (i > 0)
                {
                    return RedirectToAction("PurchaseDetails", new { id = p.Purchase_ID });
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Fiiled to save";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        public ActionResult PurchaseView(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            ViewBag.VendorName = db.Vendors.OrderBy(a => a.VendorName).ToList();
            try
            {
                Purchase p = db.Purchases.Find(id);
                if (p != null)
                {
                    Session["Purchase_Id_Update"] = id;
                    return View(p);
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        [HttpPost]
        public ActionResult PurchaseView(string number, string VendorId)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["Purchase_Id_Update"]);
            try
            {
                Purchase p = db.Purchases.Find(id);
                if (p != null)
                {
                    p.Vendor_ID = Convert.ToInt32(VendorId);
                    p.Date = Convert.ToDateTime(Request["date"]);
                    p.BillNo = number;
                    db.Entry(p).State = System.Data.Entity.EntityState.Modified;
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        return RedirectToAction("PurchaseInformation");
                    }
                    else
                    {
                        ViewBag.errorMsg = "Technical error | Failed to update";
                    }
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View(db.Purchases.Find(id));
        }
        public ActionResult PurchaseDelete(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            ViewBag.VendorName = db.Vendors.OrderBy(a => a.VendorName).ToList();
            try
            {
                Purchase p = db.Purchases.Find(id);
                if (p != null)
                {
                    Session["Purchase_Id_Delete"] = id;
                    return View(p);
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        [HttpPost]
        public ActionResult PurchaseDelete()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["Purchase_Id_Delete"]);
            try
            {
                Purchase p = db.Purchases.Find(id);
                foreach (var item in p.PurchaseDetails.ToList())
                {
                    db.PurchaseDetails.Remove(item);
                    db.SaveChanges();

                }
                db.Purchases.Remove(p);
                int i = db.SaveChanges();
                if (i > 0)
                {
                    return RedirectToAction("PurchaseInformation");
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Failed to delete";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View(db.Purchases.Find(id));
        }
        public ActionResult PurchaseDetails(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                ViewBag.ItemCategoryName = db.ItemCategories.Where(a => a.IsRawMetirial.Value || a.IsAssets.Value).OrderBy(a => a.ItemCategoryName).ToList();
                ViewBag.ItemName = db.ItemMasters.OrderBy(a => a.ItemName).ToList();
                var pd = db.PurchaseDetails.Where(a => a.Purchase_ID == id).ToList();
                return View(pd);
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        [HttpPost]
        public JsonResult PurchaseDetails_ItemBind(int ItemCategory_Id)
        {
            var list1 = db.ItemMasters.Where(a => a.ItemCategory_ID == ItemCategory_Id).ToList();
            var list = list1.Select(a => new
           {
               a.ItemMaster_ID,
               a.ItemName
           }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult PurchaseDetails(string qty, string rate, string amt, int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                PurchaseDetail pd = new PurchaseDetail();
                pd.Quantity = Convert.ToInt32(qty);
                pd.Rate = Convert.ToDouble(rate);
                pd.Amount = Convert.ToInt32(qty) * Convert.ToDouble(rate);
                pd.ItemMaster_ID = Convert.ToInt32(Request["ItemMasterId"]);
                pd.Purchase_ID = id;
                db.PurchaseDetails.Add(pd);
                int i = db.SaveChanges();
                if (i > 0)
                {
                    return RedirectToAction("PurchaseDetails", new { id = id });
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Fiiled to save";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        public ActionResult PurchaseDetailsDelete(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                PurchaseDetail pd = db.PurchaseDetails.Find(id);
                int pid = pd.Purchase_ID.Value;
                if (pd != null)
                {
                    db.PurchaseDetails.Remove(pd);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        return RedirectToAction("PurchaseDetails", new { id = pid });
                    }
                    else
                    {
                        ViewBag.errorMsg = "Technical error | Fiiled to delete this record";
                    }
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        public ActionResult VendorInformation(int? page)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page.HasValue) ? page.Value : 1;
            var list = db.Vendors.OrderBy(a => a.VendorName).ToList();
            return View(list.ToPagedList(pageIndex, pageSize));
        }
        public ActionResult VendorInformation_Search_Name_MobileNUmber(string vname, string mobno, int page = 0)
        {
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page > 0) ? page : 1;

            var list1 = db.Vendors.ToList();
            if (vname != "" && mobno == "")
            {
                list1 = db.Vendors.Where(a => a.VendorName.Contains(vname)).ToList();
            }
            else if (mobno != "" && vname == "")
            {
                list1 = db.Vendors.Where(a => a.MobileNo.Contains(mobno)).ToList();
            }
            else if (mobno == "" && vname == "")
            {
                list1 = db.Vendors.ToList();
            }
            else if (vname != "" && mobno != "")
            {
                list1 = db.Vendors.Where(a => a.MobileNo.Contains(mobno) && a.VendorName.Contains(vname)).ToList();
            }
            var list = list1.Select(a => new
            {
                a.Vendor_ID,
                a.VendorName,
                a.Email,
                a.MobileNo,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordCount = list1.Count,
            }).ToPagedList(pageIndex, pageSize);
            ViewBag.PageList = list;
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public ActionResult VendorCreate()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }
        [HttpPost]
        public ActionResult VendorCreate(string name, string city, string address, string mnumber, string gnumber, string email)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                Vendor v = db.Vendors.Where(a => a.Email == email && a.GstNo == gnumber && a.MobileNo == mnumber).FirstOrDefault();
                if (v == null)
                {
                    v = new Vendor();
                    v.VendorName = name;
                    v.Address = address;
                    v.MobileNo = mnumber;
                    v.GstNo = gnumber;
                    v.Email = email;
                    v.City = city;
                    db.Vendors.Add(v);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        return RedirectToAction("VendorInformation");
                    }
                    else
                    {
                        ViewBag.errorMsg = "Technical error | Failed to save Vendor";
                    }
                }
                else
                {
                    ViewBag.errorMsg = "This information is already exsit";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        public ActionResult VendorView(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                Vendor v = db.Vendors.Find(id);
                if (v != null)
                {
                    Session["Vendor_Id_Update"] = id;
                    return View(v);
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        [HttpPost]
        public ActionResult VendorView(string name, string city, string address, string mnumber, string gnumber, string email)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["Vendor_Id_Update"]);
            try
            {
                Vendor v = db.Vendors.Where(a => a.Email == email && a.GstNo == gnumber && a.MobileNo == mnumber && a.Vendor_ID != id).FirstOrDefault();
                if (v == null)
                {
                    v = db.Vendors.Find(id);
                    if (v != null)
                    {
                        v.VendorName = name;
                        v.Address = address;
                        v.MobileNo = mnumber;
                        v.GstNo = gnumber;
                        v.Email = email;
                        v.City = city;
                        db.Entry(v).State = System.Data.Entity.EntityState.Modified;
                        int i = db.SaveChanges();
                        if (i > 0)
                        {
                            return RedirectToAction("VendorInformation");
                        }
                        else
                        {
                            ViewBag.errorMsg = "Technical error | Failed to update detaills";
                        }
                    }
                    else
                    {
                        ViewBag.errorMsg = "Technical error | Data not found";
                    }
                }
                else
                {
                    ViewBag.errorMsg = "This information is already exsit";
                }

            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View(db.Vendors.Find(id));
        }
        public ActionResult VendorDelete(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                Vendor v = db.Vendors.Find(id);
                if (v != null)
                {
                    Session["Vendor_Id_Delete"] = id;
                    return View(v);
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        [HttpPost]
        public ActionResult VendorDelete()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["Vendor_Id_Delete"]);
            try
            {
                Vendor v = db.Vendors.Find(id);
                if (v != null)
                {
                    db.Vendors.Remove(v);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        return RedirectToAction("VendorInformation");
                    }
                    else
                    {
                        ViewBag.errorMsg = "Technical error | Failed to delete Vendor";
                    }
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View(db.Vendors.Find(id));
        }
        public ActionResult CustomerInformation(int? page)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page.HasValue) ? page.Value : 1;
            var list = db.CustomerDetails.OrderBy(a => a.CustomerName).ToList();
            return View(list.ToPagedList(pageIndex, pageSize));
        }
        public ActionResult CustomerCreate(int? page)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page.HasValue) ? page.Value : 1;
            var list = db.CustomerDetails.OrderBy(a => a.CustomerName).ToList();
            return View(list.ToPagedList(pageIndex, pageSize));
        }
        [HttpPost]
        public ActionResult CustomerCreate(string cstName, string address, string mobile, string email)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                CustomerDetail cd = db.CustomerDetails.Where(a => a.Email == email).FirstOrDefault();
                if (cd != null)
                {
                    ViewBag.errorMsg = "This email id is already exist";
                }
                else
                {
                    cd = new CustomerDetail();
                    cd.CustomerName = cstName;
                    cd.Address = address;
                    cd.MobileNo = mobile;
                    cd.Email = email;
                    db.CustomerDetails.Add(cd);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        //Session["Customer_ID_OrderCreate"] = cd.Customer_ID;
                        return RedirectToAction("OrderCreate", new { id = cd.Customer_ID });
                    }
                    else
                    {
                        ViewBag.errorMsg = "Technical error | Failed to save Customer";
                    }
                }

            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        public JsonResult CustomerCreate_Search_Name_MobileNUmber(string cname, string mobno, int page = 0)
        {

            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page > 0) ? page : 1;

            var list1 = db.CustomerDetails.ToList();
            if (cname != "" && mobno == "")
            {
                list1 = db.CustomerDetails.Where(a => a.CustomerName.Contains(cname)).ToList();
            }
            else if (mobno != "" && cname == "")
            {
                list1 = db.CustomerDetails.Where(a => a.MobileNo.Contains(mobno)).ToList();
            }
            else if (mobno == "" && cname == "")
            {
                list1 = db.CustomerDetails.ToList();
            }
            else if (cname != "" && mobno != "")
            {
                list1 = db.CustomerDetails.Where(a => a.MobileNo.Contains(mobno) && a.CustomerName.Contains(cname)).ToList();
            }
            var list = list1.Select(a => new
            {
                a.Customer_ID,
                a.CustomerName,
                a.Email,
                a.MobileNo,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordCount = list1.Count,
            }).ToPagedList(pageIndex, pageSize);
            ViewBag.PageList = list;
            return Json(list, JsonRequestBehavior.AllowGet);

        }
        public ActionResult CustomerView(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                CustomerDetail cd = db.CustomerDetails.Find(id);
                if (cd != null)
                {
                    Session["CustomerDetail_Id_Update"] = id;
                    return View(cd);
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        [HttpPost]
        public ActionResult CustomerView(string cstName, string address, string mobile, string email)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["CustomerDetail_Id_Update"]);
            try
            {
                CustomerDetail cd = db.CustomerDetails.Where(a => a.Email == email && a.Customer_ID != id).FirstOrDefault();
                if (cd != null)
                {
                    ViewBag.errorMsg = "This email id is already exist";
                }
                else
                {
                    cd = db.CustomerDetails.Find(id);
                    if (cd != null)
                    {
                        cd.CustomerName = cstName;
                        cd.Address = address;
                        cd.MobileNo = mobile;
                        cd.Email = email;
                        db.Entry(cd).State = System.Data.Entity.EntityState.Modified;
                        int i = db.SaveChanges();
                        if (i > 0)
                        {
                            Session["CustomerDetail_Id_Update"] = null;
                            return RedirectToAction("CustomerInformation");
                        }
                        else
                        {
                            ViewBag.errorMsg = "Technical error | Failed to update Customer";
                        }
                    }
                    else
                    {
                        ViewBag.errorMsg = "Technical error | Data not found";
                    }
                }

            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View(db.CustomerDetails.Find(id));
        }
        public ActionResult CustomerDelete(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                CustomerDetail cd = db.CustomerDetails.Find(id);
                if (cd != null)
                {
                    Session["CustomerDetail_Id_Delete"] = id;
                    return View(cd);
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        [HttpPost]
        public ActionResult CustomerDelete()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["CustomerDetail_Id_Delete"]);
            try
            {
                CustomerDetail cd = db.CustomerDetails.Find(id);
                if (cd != null)
                {
                    db.CustomerDetails.Remove(cd);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        Session["CustomerDetail_Id_Delete"] = null;
                        return RedirectToAction("CustomerInformation");
                    }
                    else
                    {
                        ViewBag.errorMsg = "Technical error | Failed to delete Customer";
                    }
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View(db.CustomerDetails.Find(id));
        }
        public ActionResult OrderCreate(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            ViewBag.FloorNumber = db.Floors.ToList();
            ViewBag.TableType = db.TableTypes.OrderBy(a => a.TableName).ToList();
            ViewBag.TableNumber = db.TableMasters.ToList();
            ViewBag.WaiterName = db.WaiterMasters.OrderBy(a => a.WaiterName).ToList();
            ViewBag.srno = db.OrderMasters.Max(a => a.BillNo).Value + 1;
            Session["Customer_ID_OrderCreate"] = id;
            ViewBag.Date = DateTime.Now.ToString("yyyy-MM-dd");
            //int cid = Convert.ToInt32(Session["Customer_ID_OrderCreate"]);
            return View(db.CustomerDetails.Find(id));
        }
        [HttpPost]
        public JsonResult OrderCreate_TableNumber_Bind(int Floor_Id, int TableType_Id)
        {
            var list1 = db.TableMasters.Where(a => a.Floor_ID == Floor_Id && a.TableType_ID == TableType_Id).ToList();
            var list = list1.Select(a => new
            {
                a.Table_ID,
                a.TableNo
            });
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult OrderCreate_TableCapacity_Bind(int Table_Id)
        {
            var tblCapacity = db.TableMasters.Find(Table_Id).TableCapacity;
            return Json(tblCapacity, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult OrderCreate(string number, string OrderType, string Table_Id, string date)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                OrderMaster om = new OrderMaster();
                om.Customer_ID = Convert.ToInt32(Session["Customer_ID_OrderCreate"]);
                if (OrderType == "Dining")
                {
                    om.OrderType = "Dining";
                    om.Table_ID = Convert.ToInt32(Table_Id);
                    om.WaiterMaster_Id = Convert.ToInt32(Request["WaiterMaster_Id"]);
                }
                else
                {
                    om.OrderType = "Take away";

                }
                om.IsComplete = false;
                om.IsCancle = false;
                om.IsProcess = false;
                om.Date = Convert.ToDateTime(date);
                om.BillNo = Convert.ToInt32(number);
                db.OrderMasters.Add(om);
                int i = db.SaveChanges();
                if (i > 0)
                {
                    Session["Customer_ID_OrderCreate"] = null;
                    return RedirectToAction("OrderDetailCreate", new { id = om.OrderMaster_ID });
                }
                else
                {
                    ViewBag.erroMsg = "Technical error | Failed to create bill";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            ViewBag.FloorNumber = db.Floors.ToList();
            ViewBag.TableType = db.TableTypes.OrderBy(a => a.TableName).ToList();
            ViewBag.TableNumber = db.TableMasters.ToList();
            ViewBag.WaiterName = db.WaiterMasters.OrderBy(a => a.WaiterName).ToList();
            return View();
        }
        public ActionResult OrderDetailCreate(int id, int? page)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            Session["OrderMaster_Id"] = id;
            ViewBag.ItemCategoryName = db.ItemCategories.OrderBy(a => a.ItemCategoryName).Where(a => a.IsMenuItem == true).ToList();
            ViewBag.ItemName = db.ItemMasters.OrderBy(a => a.ItemName).ToList();
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page.HasValue) ? page.Value : 1;
            var OrderDetailList = db.OrderDetailMasters.Where(a => a.OrderMaster_ID == id).ToList();
            return View(OrderDetailList.ToPagedList(pageIndex, pageSize));
        }
        [HttpPost]
        public JsonResult OrderDetailCreate_Bind(int ItemCategory_Id)
        {

            var list1 = db.ItemMasters.Where(a => a.ItemCategory_ID == ItemCategory_Id).ToList();
            var list = list1.Select(a => new
            {
                a.ItemCategory_ID.Value,
                a.ItemMaster_ID,
                a.ItemName,
                Veg = (a.IsVeg == null) ? "" : (a.IsVeg.Value) ? "Veg" : "Non-Veg",
                a.Rate

            }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult OrderDetailCreate_Bind_Rate(int ItemMaster_Id)
        {
            var list1 = db.ItemMasters.Find(ItemMaster_Id).Rate.Value;
            return Json(list1, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult OrderDetailCreate(string qty, string rate, string amt, int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                OrderDetailMaster odm = new OrderDetailMaster();
                odm.OrderMaster_ID = id;
                odm.ItemMaster_ID = Convert.ToInt32(Request["ItemMasterId"]);
                odm.Quantity = Convert.ToInt32(qty);
                odm.Rate = Convert.ToDouble(rate);
                odm.Amount = Convert.ToInt32(qty) * Convert.ToDouble(rate);
                odm.IsKOT = false;
                odm.IsKOTComplete = false;
                odm.IsKOTProcess = false;
                db.OrderDetailMasters.Add(odm);
                int i = db.SaveChanges();
                if (i > 0)
                {
                    Session["OrderMaster_Id"] = id;
                    return RedirectToAction("OrderDetailCreate", new { id = id });
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Failed to save data";
                }

            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            ViewBag.ItemCategoryName = db.ItemCategories.OrderBy(a => a.ItemCategoryName).ToList();
            ViewBag.ItemName = db.ItemMasters.OrderBy(a => a.ItemName).ToList();
            var OrderDetailList = db.OrderDetailMasters.Where(a => a.OrderMaster_ID == id).ToList();
            return View(OrderDetailList);
        }

        public ActionResult OrderDetailsAddQuantity(int temp)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                OrderDetailMaster odm = db.OrderDetailMasters.Find(temp);
                if (odm != null)
                {
                    int qty = odm.Quantity.Value + 1;
                    odm.Quantity = qty;
                    odm.Amount = qty * Convert.ToDouble(odm.Rate.Value);
                    db.Entry(odm).State = System.Data.Entity.EntityState.Modified;
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        return RedirectToAction("OrderDetailCreate", new { id = Convert.ToInt32(Session["OrderMaster_Id"]) });
                    }
                    else
                    {
                        ViewBag.errorMsg = "Failed to increse qunatity quantity";
                    }
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        public ActionResult OrderDetailsSingleDeleteQuantity(int temp)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                OrderDetailMaster odm = db.OrderDetailMasters.Find(temp);
                if (odm != null)
                {
                    if (odm.Quantity.Value > 1)
                    {
                        int qty = odm.Quantity.Value - 1;
                        odm.Quantity = qty;
                        odm.Amount = qty * Convert.ToDouble(odm.Rate.Value);
                        db.Entry(odm).State = System.Data.Entity.EntityState.Modified;
                        int i = db.SaveChanges();
                        if (i > 0)
                        {
                            return RedirectToAction("OrderDetailCreate", new { id = Convert.ToInt32(Session["OrderMaster_Id"]) });
                        }
                        else
                        {
                            ViewBag.errorMsg = "Failed to decrese quantity";
                        }
                    }
                    else
                    {
                        ViewBag.errorMsg = "Quantity must be more than zero";
                    }
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return RedirectToAction("OrderDetailCreate", new { id = Convert.ToInt32(Session["OrderMaster_Id"]) });
        }
        public ActionResult OrderDetailsDeleteOrderItem(int temp)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                OrderDetailMaster odm = db.OrderDetailMasters.Find(temp);
                if (odm != null)
                {
                    db.OrderDetailMasters.Remove(odm);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        return RedirectToAction("OrderDetailCreate", new { id = Convert.ToInt32(Session["OrderMaster_Id"]) });
                    }
                    else
                    {
                        ViewBag.errorMsg = "Failed to delete qunatity quantity";
                    }
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        //------------------------------------------------------------------------------********************************************
        public ActionResult OrderMasterInformation(int? page)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page.HasValue) ? page.Value : 1;
            var list = db.OrderMasters.OrderByDescending(a => new { a.BillNo, a.Date }).ToList();
            ViewBag.date = DateTime.Now.ToString("yyyy-MM-dd");

            Session["OrderStatus"] = "Panding Order";

            return View(list.ToPagedList(pageIndex, pageSize));
        }

        [HttpPost]
        public ActionResult OrderMasterInformation(string status_order, string stDate, string enDate)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            if (status_order == "0")
            {
                Session["OrderStatus"] = "Panding Order";
            }
            else if (status_order == "1") { Session["OrderStatus"] = "Process"; }
            else if (status_order == "2") { Session["OrderStatus"] = "Complete"; }
            else if (status_order == "3") { Session["OrderStatus"] = "Cancel"; }

            Session["stDate"] = stDate;
            Session["enDate"] = enDate;
            return RedirectToAction("OrderMasterInformation_Print");
        }
        [HttpPost]
        public JsonResult OrderMasterInformation_StatusCheck(int StatusId, string stDate, string enDate, int page = 0)
        {
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page > 0) ? page : 1;
            DateTime stDate1 = Convert.ToDateTime(stDate);
            DateTime enDate1 = Convert.ToDateTime(enDate);

            var list1 = db.OrderMasters.OrderBy(a => new { a.Date, a.BillNo }).Where(a => a.Date >= stDate1 && a.Date <= enDate1).ToList();
            if (StatusId == 0)
            {
                list1 = list1.Where(a => a.IsProcess == false && a.IsComplete == false && a.IsCancle == false).ToList();
                Session["OrderStatus"] = "Panding Order";
            }
            else if (StatusId == 1)
            {
                list1 = list1.Where(a => a.IsProcess == true && a.IsComplete == false && a.IsCancle == false).ToList();
                Session["OrderStatus"] = "Process";
            }
            else if (StatusId == 2)
            {
                list1 = list1.Where(a => a.IsProcess == false && a.IsComplete == true && a.IsCancle == false).ToList();
                Session["OrderStatus"] = "Complete";
            }
            else if (StatusId == 3)
            {
                list1 = list1.Where(a => a.IsProcess == false && a.IsComplete == false && a.IsCancle == true).ToList();
                Session["OrderStatus"] = "Cancel";
            }
            Session["stDate"] = stDate1;
            Session["enDate"] = enDate1;
            var list = list1.Select(a => new
            {
                a.OrderType,
                a.CustomerDetail.CustomerName,
                a.BillNo,
                Date = a.Date.Value.ToString("dd-MM-yyyy"),
                a.OrderMaster_ID,
                StausId = StatusId,
                TableNo = (a.Table_ID == null) ? "-" : a.TableMaster.TableNo,
                TotalAmount = a.OrderDetailMasters.Sum(am => am.Amount.Value),
                TotalItem = a.OrderDetailMasters.Count,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordCount = list1.Count,
            }).ToPagedList(pageIndex, pageSize);
            ViewBag.PageList = list;
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult OrderMasterInformation_Print()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            if (Session["OrderStatus"] == null || Session["stDate"] == null || Session["enDate"] == null)
            {
                return RedirectToAction("Login");
            }
            string TitleData = "OrderStatus :- " + Convert.ToString(Session["OrderStatus"]) + " " + " : " + " From  " + Convert.ToDateTime(Session["stDate"]).ToString("dd-MM-yyyy") + "  To  " + Convert.ToDateTime(Session["enDate"]).ToString("dd-MM-yyyy");
            ViewData["TitleData"] = TitleData;
            return View();
        }

        public ActionResult OrderMasterIsProcess(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                OrderMaster om = db.OrderMasters.Find(id);
                if (om != null)
                {
                    om.IsProcess = true;
                    om.IsCancle = false;
                    om.IsComplete = false;
                    db.Entry(om).State = System.Data.Entity.EntityState.Modified;
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        return RedirectToAction("OrderMasterInformation");
                    }
                    else
                    {
                        ViewBag.errorMsg = "Technical error | Failed to change Order Status";
                    }
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error ";
            }
            return View();
        }
        public ActionResult OrderMasterIsComplete(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                OrderMaster om = db.OrderMasters.Find(id);
                if (om != null)
                {
                    om.IsComplete = true;
                    om.IsCancle = false;
                    om.IsProcess = false;
                    db.Entry(om).State = System.Data.Entity.EntityState.Modified;
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        return RedirectToAction("OrderMasterInformation");
                    }
                    else
                    {
                        ViewBag.errorMsg = "Technical error | Failed to change Order Status";
                    }
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error ";
            }
            return View();
        }
        public ActionResult OrderMasterIsCancle(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                OrderMaster om = db.OrderMasters.Find(id);
                if (om != null)
                {
                    om.IsCancle = true;
                    om.IsProcess = false;
                    om.IsComplete = false;
                    db.Entry(om).State = System.Data.Entity.EntityState.Modified;
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        return RedirectToAction("OrderMasterInformation");
                    }
                    else
                    {
                        ViewBag.errorMsg = "Technical error | Failed to change Order Status";
                    }
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error ";
            }
            return View();
        }
        public ActionResult OrderMasterInformation_Waiter(int? page)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page.HasValue) ? page.Value : 1;
            var list = db.OrderMasters.OrderByDescending(a => new { a.BillNo, a.Date }).ToList();
            ViewBag.date = DateTime.Now.ToString("yyyy-MM-dd");
            ViewBag.WaiterList = db.WaiterMasters.OrderBy(a => a.WaiterName).ToList();
            Session["OrderStatus"] = "Panding Order";
            return View(list.ToPagedList(pageIndex, pageSize));
        }
        [HttpPost]
        public JsonResult OrderMasterInformation_WaiterCheck(int StatusId, string WaiterId, string stDate, string enDate, int page = 0)
        {
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page > 0) ? page : 1;
            DateTime stDate1 = Convert.ToDateTime(stDate);
            DateTime enDate1 = Convert.ToDateTime(enDate);
            int WID = Convert.ToInt32(WaiterId);
            var list1 = db.OrderMasters.OrderByDescending(a => new { a.Date, a.BillNo }).Where(a => a.Date > stDate1 && a.Date < enDate1 && a.WaiterMaster_Id == WID).ToList();

            if (StatusId == 0)
            {
                list1 = list1.Where(a => a.IsProcess == false && a.IsComplete == false && a.IsCancle == false).ToList();
                Session["OrderStatus"] = "Panding Order";
            }
            else if (StatusId == 1)
            {
                list1 = list1.Where(a => a.IsProcess == true && a.IsComplete == false && a.IsCancle == false).ToList();
                Session["OrderStatus"] = "Process";
            }
            else if (StatusId == 2)
            {
                list1 = list1.Where(a => a.IsProcess == false && a.IsComplete == true && a.IsCancle == false).ToList();
                Session["OrderStatus"] = "Complete";
            }
            else if (StatusId == 3)
            {
                list1 = list1.Where(a => a.IsProcess == false && a.IsComplete == false && a.IsCancle == true).ToList();
                Session["OrderStatus"] = "Cancel";
            }
            Session["stDate"] = stDate1;
            Session["enDate"] = enDate1;
            Session["WaiterId_Print"] = WID;
            Session["WaiterName_Print"] = db.WaiterMasters.Find(WID).WaiterName;
            var list = list1.Select(a => new
            {
                a.OrderType,
                a.CustomerDetail.CustomerName,
                a.BillNo,
                Date = a.Date.Value.ToString("dd-MM-yyyy"),
                a.OrderMaster_ID,
                StausId = StatusId,
                TableNo = (a.Table_ID == null) ? "-" : a.TableMaster.TableNo,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordCount = list1.Count,
            }).ToPagedList(pageIndex, pageSize);
            ViewBag.PageList = list;
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public ActionResult OrderMasterInformation_Waiter_Print()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            if (Session["OrderStatus"] == null || Session["WaiterName_Print"] == null || Session["WaiterId_Print"] == null || Session["stDate"] == null || Session["enDate"] == null)
            {
                return RedirectToAction("Login");
            }
            string TitleData = "Waiter Name : " + Session["WaiterName_Print"] + "       OrderStatus : " + Convert.ToString(Session["OrderStatus"]) + "      From  " + Convert.ToDateTime(Session["stDate"]).ToString("dd-MM-yyyy") + "  To  " + Convert.ToDateTime(Session["enDate"]).ToString("dd-MM-yyyy");
            ViewData["TitleData"] = TitleData;
            return View();
        }
        public ActionResult OrderMasterDeatil(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                Session["OrderMasterDeatil_AddNewItem"] = null;
                OrderMaster om = db.OrderMasters.Find(id);
                if (om != null)
                {
                    Session["OrderMasterDeatil_AddNewItem"] = id;
                    return View(om);
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error ";
            }
            return View();
        }
        public ActionResult PaymentProcess(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                OrderMaster om = db.OrderMasters.Find(id);
                if (om != null)
                {
                    ViewBag.Date = DateTime.Now.ToString("yyyy-MM-dd");
                    Session["OrderMaster_Id_PaymentProcess"] = id;
                    ViewBag.PaymentDtl = db.Payments.Where(a => a.OrderMaster_Id == id).ToList();
                    return View(om);
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        [HttpPost]
        public ActionResult PaymentProcess(string mode, string amount, string date, string remark, string Pay)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {

                Payment p = new Payment();
                p.OrderMaster_Id = Convert.ToInt32(Session["OrderMaster_Id_PaymentProcess"]);
                p.PaymentMode = mode;
                p.Amount = Convert.ToDouble(amount);
                p.PayementDate = Convert.ToDateTime(date);
                p.Remarks = remark;
                db.Payments.Add(p);
                int i = db.SaveChanges();
                if (i > 0)
                {
                    if (Pay == "Pay")
                    {
                        return RedirectToAction("PaymentProcess", new { id = Convert.ToInt32(Session["OrderMaster_Id_PaymentProcess"]) });
                    }
                    else if (Pay == "Pay And Print")
                    {
                        return RedirectToAction("BillGenerate", new { id = Convert.ToInt32(Session["OrderMaster_Id_PaymentProcess"]) });
                    }
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | failed payment";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        public ActionResult PaymentProcess_View(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                Payment p = db.Payments.Find(id);
                if (p != null)
                {
                    Session["PaymentProcess_PaymentId_Update"] = id;
                    return View(p);
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        [HttpPost]
        public ActionResult PaymentProcess_View(string mode, string amount, string date, string remark)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["PaymentProcess_PaymentId_Update"]);
            try
            {
                Payment p = db.Payments.Find(id);
                if (p != null)
                {
                    p.PaymentMode = mode;
                    p.Amount = Convert.ToDouble(amount);
                    p.PayementDate = Convert.ToDateTime(date);
                    p.Remarks = remark;
                    db.Entry(p).State = System.Data.Entity.EntityState.Modified;
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        return RedirectToAction("PaymentProcess", new { id = Convert.ToInt32(Session["OrderMaster_Id_PaymentProcess"]) });
                    }
                    else
                    {
                        ViewBag.errorMsg = "Technical error | failed to payment";
                    }
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View(db.Payments.Find(id));
        }
        public ActionResult PaymentProcess_Delete(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                Payment p = db.Payments.Find(id);
                if (p != null)
                {
                    db.Payments.Remove(p);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        return RedirectToAction("PaymentProcess", new { id = Convert.ToInt32(Session["OrderMaster_Id_PaymentProcess"]) });
                    }
                    else
                    {

                    }
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        public ActionResult CustomerList(int? page)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page.HasValue) ? page.Value : 1;
            var list = db.CustomerDetails.OrderBy(a => a.CustomerName).ToList();
            return View(list.ToPagedList(pageIndex, pageSize));
        }
        public ActionResult CustomerDetails(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                CustomerDetail cd = db.CustomerDetails.Find(id);
                if (cd != null)
                {
                    return View(cd);
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        public ActionResult WaiterInformation(int? page)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page.HasValue) ? page.Value : 1;
            var list = db.WaiterMasters.OrderBy(a => a.WaiterName).ToList();
            return View(list.ToPagedList(pageIndex, pageSize));
        }
        public ActionResult WaiterAttendenceCreate()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            //ViewBag.WaiterName = db.WaiterMasters.OrderBy(a => a.WaiterName).ToList();
            ViewBag.Date = DateTime.Now.ToString("yyyy-MM-dd");
            var list = db.WaiterMasters.OrderBy(a => a.WaiterName).ToList();
            Session["WaiterAttendence_Count"] = list.Count;
            return View(list);
        }
        [HttpPost]
        public ActionResult WaiterAttendenceCreate(string date)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                WaiterAttendence wa;
                for (int i = 1; i <= Convert.ToInt32(Session["WaiterAttendence_Count"]); i++)
                {
                    if (Request["Chk_" + i] == "on")
                    {
                        wa = new WaiterAttendence();
                        wa.WaiterMaster_Id = Convert.ToInt32(Request["Hd_" + i]);
                        wa.DateTime = Convert.ToDateTime(date);
                        db.WaiterAttendences.Add(wa);
                        int temp = db.SaveChanges();
                    }
                }
                //wa.WaiterMaster_Id = Convert.ToInt32(WaiterMaster_Id);
                //wa.DateTime = Convert.ToDateTime(date);
                //db.WaiterAttendences.Add(wa);
                //int i = db.SaveChanges();
                //if (i > 0)
                //{
                //    return RedirectToAction("");
                //}
                //else
                //{
                //    ViewBag.errorMsg = "Technical error | filed to put attendence";
                //}
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            ViewBag.Date = DateTime.Now.ToString("yyyy-MM-dd");
            return View(db.WaiterMasters.OrderBy(a => a.WaiterName).ToList());
        }
        public ActionResult WaiterAttendenceView(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                WaiterAttendence wa = db.WaiterAttendences.Find(id);
                if (wa != null)
                {
                    Session["WaiterAttendence_Update"] = id;
                    return View(wa);
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        [HttpPost]
        public ActionResult WaiterAttendenceView(string WaiterMaster_Id, string date)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["WaiterAttendence_Update"]);
            try
            {
                WaiterAttendence wa = db.WaiterAttendences.Find(id);
                if (wa != null)
                {
                    wa.WaiterMaster_Id = Convert.ToInt32(WaiterMaster_Id);
                    wa.DateTime = Convert.ToDateTime(date);
                    db.Entry(wa).State = System.Data.Entity.EntityState.Modified;
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        Session["WaiterAttendence_Update"] = null;
                        return RedirectToAction("");
                    }
                    else
                    {
                        ViewBag.errorMsg = "Technical error | filed to update attendence";
                    }
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View(db.WaiterAttendences.Find(id));
        }
        public ActionResult WaiterAttendenceDelete(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                WaiterAttendence wa = db.WaiterAttendences.Find(id);
                if (wa != null)
                {
                    Session["WaiterAttendence_Delete"] = id;
                    return View(wa);
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        [HttpPost]
        public ActionResult WaiterAttendenceDelete()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["WaiterAttendence_Delete"]);
            try
            {
                WaiterAttendence wa = db.WaiterAttendences.Find(id);
                if (wa != null)
                {
                    db.WaiterAttendences.Remove(wa);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        Session["WaiterAttendence_Delete"] = null;
                        return RedirectToAction("");
                    }
                    else
                    {
                        ViewBag.errorMsg = "Technical error | filed to delete attendence";
                    }
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View(db.WaiterAttendences.Find(id));
        }
        public ActionResult WaiterCreate()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }
        [HttpPost]
        public ActionResult WaiterCreate(string name, string mnumber, string email, string password, string anumber, string address)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                WaiterMaster wm = db.WaiterMasters.Where(a => a.Email == email).FirstOrDefault();
                if (wm != null)
                {
                    ViewBag.errorMsg = "Email is already exist";
                }
                else
                {
                    wm = new WaiterMaster();
                    wm.WaiterName = name;
                    wm.ContctNo = mnumber;
                    wm.Email = email;
                    wm.Address = address;
                    wm.Password = password;
                    wm.AdharCardNo = anumber;
                    db.WaiterMasters.Add(wm);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        return RedirectToAction("WaiterInformation");
                    }
                    else
                    {
                        ViewBag.errorMsg = "Technical error | filed to add new waiter";
                    }
                }

            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        public ActionResult WaiterView(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                WaiterMaster wm = db.WaiterMasters.Find(id);
                if (wm != null)
                {
                    Session["WaiterMaster_Id_Update"] = id;
                    return View(wm);
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        [HttpPost]
        public ActionResult WaiterView(string name, string mnumber, string email, string password, string anumber, string address)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["WaiterMaster_Id_Update"]);
            try
            {
                WaiterMaster wm = db.WaiterMasters.Where(a => a.Email == email && a.WaiterMaster_Id != id).FirstOrDefault();
                if (wm != null)
                {
                    ViewBag.errorMsg = "Email is already exist";
                }
                else
                {
                    wm = db.WaiterMasters.Find(id);
                    if (wm != null)
                    {
                        wm.WaiterName = name;
                        wm.ContctNo = mnumber;
                        wm.Email = email;
                        wm.Address = address;
                        wm.Password = password;
                        wm.AdharCardNo = anumber;
                        db.Entry(wm).State = System.Data.Entity.EntityState.Modified;
                        int i = db.SaveChanges();
                        if (i > 0)
                        {
                            Session["WaiterMaster_Id_Update"] = null;
                            return RedirectToAction("WaiterInformation");
                        }
                        else
                        {
                            ViewBag.errorMsg = "Technical error | filed to update information";
                        }
                    }
                    else
                    {
                        ViewBag.errorMsg = "Technical error | data not found";
                    }

                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View(db.WaiterMasters.Find(id));
        }
        public ActionResult WaiterDelete(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                WaiterMaster wm = db.WaiterMasters.Find(id);
                if (wm != null)
                {
                    Session["WaiterMaster_Id_Delete"] = id;
                    return View(wm);
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        [HttpPost]
        public ActionResult WaiterDelete()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["WaiterMaster_Id_Delete"]);
            try
            {
                WaiterMaster wm = db.WaiterMasters.Find(id);
                if (wm != null)
                {
                    db.WaiterMasters.Remove(wm);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        Session["WaiterMaster_Id_Delete"] = null;
                        return RedirectToAction("WaiterInformation");
                    }
                    else
                    {
                        ViewBag.errorMsg = "Technical error | filed to delete information";
                    }
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View(db.WaiterMasters.Find(id));
        }
        public ActionResult BillGenerate(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                OrderMaster om = db.OrderMasters.Find(id);
                if (om != null)
                {
                    return View(om);
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        public ActionResult ItemUsedStockInformation()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            //date must in decending order
            //if (Session["Admin_Id"] == null)
            //{
            //    return RedirectToAction("Login");
            //}
            ViewBag.Date = DateTime.Now.ToString("yyyy-MM-dd");
            ViewBag.ItemCategory = db.ItemCategories.OrderBy(a => a.ItemCategoryName).Where(a => a.IsRawMetirial.Value == true || a.IsAssets.Value).ToList();
            return View(db.ItemUsedStocks.OrderByDescending(a => a.Date).ToList());
        }
        public JsonResult ItemUsedStockInformation_ItemBind(int ItemCategory_Id, string stDate, string enDate, int page = 0)
        {
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page > 0) ? page : 1;
            DateTime sDate = Convert.ToDateTime(stDate);
            DateTime eDate = Convert.ToDateTime(enDate);
            Session["sDate_ItemUsedStock"] = sDate;
            Session["eDate_ItemUsedStock"] = eDate;
            var list1 = db.ItemUsedStocks.Where(a => a.ItemMaster.ItemCategory_ID == ItemCategory_Id && a.Date >= sDate && a.Date <= eDate).OrderBy(a => a.Date).ToList();
            Session["itemcategory_Name"] = db.ItemCategories.Where(a => a.ItemCategory_ID == ItemCategory_Id).Select(a => a.ItemCategoryName).FirstOrDefault();
            Session["itemcategory_Id"] = ItemCategory_Id;
            var list = list1.Select(a => new
            {
                a.Stock_ID,
                a.ItemMaster_ID.Value,
                a.ItemMaster.ItemName,
                a.Quantity,
                date = a.Date.Value.ToString("dd-MM-yyyy"),
                itemcategory = a.ItemMaster.ItemCategory.ItemCategoryName,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordCount = list1.Count,
            }).ToPagedList(pageIndex, pageSize);
            ViewBag.PageList = list;

            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ItemUsedStockInformation_Print()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            if (Session["itemcategory_Id"] == null || Session["itemcategory_Name"] == null || Session["sDate_ItemUsedStock"] == null || Session["eDate_ItemUsedStock"] == null)
            {
                return RedirectToAction("Login");
            }
            string TitleData = "Category :- " + Convert.ToString(Session["itemcategory_Name"]) + " " + " : " + " From  " + Convert.ToDateTime(Session["sDate_ItemUsedStock"]).ToString("dd-MM-yyyy") + "  To  " + Convert.ToDateTime(Session["eDate_ItemUsedStock"]).ToString("dd-MM-yyyy");
            ViewData["TitleData_ItemUsedStockPrint"] = TitleData;
            return View();
        }
        public ActionResult ItemUsedStockCreate()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            ViewBag.ItemCategoryName = db.ItemCategories.OrderBy(a => a.ItemCategoryName).Where(a => a.IsRawMetirial == true || a.IsAssets.Value).ToList();
            ViewBag.Date = DateTime.Now.ToString("yyyy-MM-dd");
            return View();
        }
        [HttpPost]
        public JsonResult ItemUsedStockView_ItemBind(int ItemCategory_Id)
        {

            var list1 = db.ItemMasters.Where(a => a.ItemCategory_ID == ItemCategory_Id).ToList();
            var list = list1.Select(a => new
            {
                a.ItemCategory_ID.Value,
                a.ItemMaster_ID,
                a.ItemName

            }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult ItemUsedStockCreate(string ItemMasterId, string date, string qty)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                ItemUsedStock ius = new ItemUsedStock();
                ius.ItemMaster_ID = Convert.ToInt32(ItemMasterId);
                ius.Date = Convert.ToDateTime(date);
                ius.Quantity = Convert.ToInt32(qty);
                db.ItemUsedStocks.Add(ius);
                int i = db.SaveChanges();
                if (i > 0)
                {
                    return RedirectToAction("ItemUsedStockInformation");
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | fail to save data";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        public ActionResult ItemUsedStockView(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                ItemUsedStock ius = db.ItemUsedStocks.Find(id);
                if (ius != null)
                {
                    ViewBag.ItemCategoryName = db.ItemCategories.OrderBy(a => a.ItemCategoryName).Where(a => a.IsRawMetirial == true || a.IsAssets.Value).ToList();
                    ViewBag.Date = ius.Date.Value.ToString("yyyy-MM-dd");
                    Session["ItemUsedStock_Id_Update"] = id;
                    return View(ius);
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        [HttpPost]
        public ActionResult ItemUsedStockView(string ItemMasterId, string date, string qty)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["ItemUsedStock_Id_Update"]);
            try
            {
                ItemUsedStock ius = db.ItemUsedStocks.Find(id);
                if (ius != null)
                {
                    ius.ItemMaster_ID = Convert.ToInt32(ItemMasterId);
                    ius.Date = Convert.ToDateTime(date);
                    ius.Quantity = Convert.ToInt32(qty);
                    db.Entry(ius).State = System.Data.Entity.EntityState.Modified;
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        Session["ItemUsedStock_Id_Update"] = null;
                        return RedirectToAction("ItemUsedStockInformation");
                    }
                    else
                    {
                        ViewBag.errorMsg = "Technical error | fail to save data";
                    }
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View(db.ItemUsedStocks.Find(id));
        }
        public ActionResult ItemUsedStockDelete(int id)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            try
            {
                ItemUsedStock ius = db.ItemUsedStocks.Find(id);
                if (ius != null)
                {
                    ViewBag.ItemCategoryName = db.ItemCategories.OrderBy(a => a.ItemCategoryName).Where(a => a.IsRawMetirial == true || a.IsAssets.Value).ToList();
                    ViewBag.Date = ius.Date.Value.ToString("yyyy-MM-dd");
                    Session["ItemUsedStock_Id_Delete"] = id;
                    return View(ius);
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View();
        }
        [HttpPost]
        public ActionResult ItemUsedStockDelete()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int id = Convert.ToInt32(Session["ItemUsedStock_Id_Delete"]);
            try
            {
                ItemUsedStock ius = db.ItemUsedStocks.Find(id);
                if (ius != null)
                {
                    db.ItemUsedStocks.Remove(ius);
                    int i = db.SaveChanges();
                    if (i > 0)
                    {
                        Session["ItemUsedStock_Id_Delete"] = null;
                        return RedirectToAction("ItemUsedStockInformation");
                    }
                    else
                    {
                        ViewBag.errorMsg = "Technical error | fail to save data";
                    }
                }
                else
                {
                    ViewBag.errorMsg = "Technical error | Data not found";
                }
            }
            catch
            {
                ViewBag.errorMsg = "Technical error";
            }
            return View(db.ItemUsedStocks.Find(id));
        }
        public ActionResult ItemUsedStockNotification()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            ViewBag.ItemCategory = db.ItemCategories.Where(a => a.IsRawMetirial.Value || a.IsAssets.Value).OrderBy(a => a.ItemCategoryName).ToList();
            ViewBag.Date = DateTime.Now.ToString("yyyy-MM-dd");
            return View(db.ItemMasters.ToList());
        }
        //[HttpPost]
        //public JsonResult ItemUsedStockNotification(int ItemCategory_Id, string search_ItemName, string search_GST)
        //{
        //    //int pageSize = 10;
        //    //int pageIndex = 1;
        //    //pageIndex = (page > 0) ? page : 1;
        //    var list1 = db.ItemMasters.ToList();
        //    //if (search_ItemName == "" && search_GST == "")
        //    //{
        //    //    list1 = db.ItemMasters.Where(a => a.ItemCategory_ID == ItemCategory_Id).ToList();
        //    //}
        //    //else if (search_ItemName != "" && search_GST == "")
        //    //{
        //    //    list1 = list1.Where(a => a.ItemName.ToLower().Contains(search_ItemName.ToLower())).ToList();
        //    //}
        //    //else if (search_ItemName == "" && search_GST != "")
        //    //{
        //    //    list1 = list1.Where(a => a.CGST.Value.Equals(Convert.ToDouble(search_GST))).ToList();
        //    //}
        //    //else if (search_ItemName != "" && search_GST != "")
        //    //{
        //    //    list1 = list1.Where(a => a.ItemName.ToLower().Contains(search_ItemName) && a.CGST.Value.Equals(Convert.ToDouble(search_GST))).ToList();
        //    //}
        //    var list = list1.Select(a => new
        //    {
        //        a.ItemCategory_ID.Value,
        //        a.ItemMaster_ID,
        //        a.ItemName,
        //        Veg = (a.IsVeg == null) ? "-" : (a.IsVeg.Value) ? "Veg" : "Non-Veg",
        //        a.Rate,
        //        CGST = (a.CGST == null) ? 0 : a.CGST,
        //        SGST = (a.SGST == null) ? 0 : a.SGST,
        //        PageIndex = 0,
        //        PageSize = 0,
        //        RecordCount = list1.Count,
        //        PurchaseValue=(a.PurchaseDetails.Count>0)?a.PurchaseDetails.Sum(am=>am.Quantity):0,
        //        ItemUsedStock=(a.ItemUsedStocks.Count>0)?a.ItemUsedStocks.Sum(am=>am.Quantity):0,
        //        ClosingBalance = ((a.PurchaseDetails.Count > 0) ? a.PurchaseDetails.Sum(am => am.Quantity) : 0) - ((a.ItemUsedStocks.Count>0)?a.ItemUsedStocks.Sum(am=>am.Quantity):0),
        //        ReorderLevel=(a.ReOrderLevel!=null)?a.ReOrderLevel.Value:0,
        //        MinimumQuantity = (a.MinimumQuantatity!=null)?a.MinimumQuantatity.Value:0
        //    }).ToList();
        //  var  listTemp=list.Where(tm=>tm.ClosingBalance<=tm.ReorderLevel && tm.ReorderLevel>0).ToList();
        //  ViewBag.PageList = listTemp;
        //  return Json(listTemp, JsonRequestBehavior.AllowGet);
        //}
        public ActionResult Notification()
        {
            return View();
        }
        public ActionResult PendingOrderNotification()
        {
            return View();
        }
        public ActionResult ProcessOrderNotification()
        {
            return View();
        }
        public ActionResult PaymentRemainNotification()
        {
            return View();
        }
        public ActionResult OurMenu(int? page)
        {

            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            ViewBag.ItemCategory = db.ItemCategories.Where(a => a.IsMenuItem.Value).OrderBy(a => a.ItemCategoryName).ToList();
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page.HasValue) ? page.Value : 1;
            var list = db.ItemMasters.ToList();
            return View(list.ToPagedList(pageIndex, pageSize));
        }
        public JsonResult Item_Filter(int ItemCategory_Id, string search_ItemName, string search_GST, int page = 0)
        {
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page > 0) ? page : 1;
            var list1 = db.ItemMasters.Where(a => a.ItemCategory_ID == ItemCategory_Id).ToList();
            if (search_ItemName == "" && search_GST == "")
            {
                list1 = db.ItemMasters.Where(a => a.ItemCategory_ID == ItemCategory_Id).ToList();
            }
            else if (search_ItemName != "" && search_GST == "")
            {
                list1 = list1.Where(a => a.ItemName.ToLower().Contains(search_ItemName.ToLower())).ToList();
            }
            else if (search_ItemName == "" && search_GST != "")
            {
                list1 = list1.Where(a => a.CGST.Value.Equals(Convert.ToDouble(search_GST))).ToList();
            }
            else if (search_ItemName != "" && search_GST != "")
            {
                list1 = list1.Where(a => a.ItemName.ToLower().Contains(search_ItemName) && a.CGST.Value.Equals(Convert.ToDouble(search_GST))).ToList();
            }
            var list = list1.Select(a => new
            {
                a.ItemCategory_ID.Value,
                a.ItemMaster_ID,
                a.ItemName,
                Veg = (a.IsVeg == null) ? "-" : (a.IsVeg.Value) ? "Veg" : "Non-Veg",
                a.Rate,
                CGST = (a.CGST == null) ? 0 : a.CGST,
                SGST = (a.SGST == null) ? 0 : a.SGST,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordCount = list1.Count,
            }).ToPagedList(pageIndex, pageSize);
            ViewBag.PageList = list;
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult dsItemList()
        {
            return View();
        }

        public ActionResult RawMetirialsList(int? page)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            ViewBag.ItemCategory = db.ItemCategories.Where(a => a.IsRawMetirial.Value).OrderBy(a => a.ItemCategoryName).ToList();
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page.HasValue) ? page.Value : 1;
            var list = db.ItemMasters.ToList();
            return View(list.ToPagedList(pageIndex, pageSize));
        }
        public ActionResult AssetsList(int? page)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            ViewBag.ItemCategory = db.ItemCategories.Where(a => a.IsAssets.Value).OrderBy(a => a.ItemCategoryName).ToList();
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page.HasValue) ? page.Value : 1;
            var list = db.ItemMasters.ToList();
            return View(list.ToPagedList(pageIndex, pageSize));
        }
        public ActionResult TotalOrders(int? page)
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page.HasValue) ? page.Value : 1;
            var list = db.OrderMasters.OrderByDescending(a => new { a.BillNo, a.Date }).ToList();
            //ViewBag.date = DateTime.Now.ToString("yyyy-MM-dd");

            Session["OrderStatus"] = "Panding Order";

            return View(list.ToPagedList(pageIndex, pageSize));
        }
        public JsonResult TotalOrderFilter(int StatusId, int page = 0)
        {
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page > 0) ? page : 1;

            var list1 = db.OrderMasters.OrderBy(a => new { a.Date, a.BillNo }).ToList();
            if (StatusId == 0)
            {
                list1 = list1.Where(a => a.IsProcess == false && a.IsComplete == false && a.IsCancle == false).ToList();
                Session["OrderStatus_TotalOrder"] = "Panding Order";
            }
            else if (StatusId == 1)
            {
                list1 = list1.Where(a => a.IsProcess == true && a.IsComplete == false && a.IsCancle == false).ToList();
                Session["OrderStatus_TotalOrder"] = "Process";
            }
            else if (StatusId == 2)
            {
                list1 = list1.Where(a => a.IsProcess == false && a.IsComplete == true && a.IsCancle == false).ToList();
                Session["OrderStatus_TotalOrder"] = "Complete";
            }
            else if (StatusId == 3)
            {
                list1 = list1.Where(a => a.IsProcess == false && a.IsComplete == false && a.IsCancle == true).ToList();
                Session["OrderStatus_TotalOrder"] = "Cancel";
            }
            Session["OrderStatus_TotalOrder_Count"] = list1.Count;
            var list = list1.Select(a => new
            {
                a.OrderType,
                a.CustomerDetail.CustomerName,
                a.BillNo,
                Date = a.Date.Value.ToString("dd-MM-yyyy"),
                a.OrderMaster_ID,
                StausId = StatusId,
                TableNo = (a.Table_ID == null) ? "-" : a.TableMaster.TableNo,
                TotalAmount = a.OrderDetailMasters.Sum(am => am.Amount.Value),
                TotalItem = a.OrderDetailMasters.Count,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordCount = list1.Count,
            }).ToPagedList(pageIndex, pageSize);
            ViewBag.PageList = list;
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public ActionResult TotalOrders_Print()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            if (Session["OrderStatus_TotalOrder"] == null || Session["OrderStatus_TotalOrder_Count"]==null)
            {
                return RedirectToAction("Login");
            }

            string TitleData = "Order Status : " + Convert.ToString(Session["OrderStatus_TotalOrder"]) + "    Total Orders : " + Convert.ToInt32(Session["OrderStatus_TotalOrder_Count"]);
            ViewData["TitleData_TotalOrders_Print"] = TitleData;
            return View();
        }

        public ActionResult TotalCompleteOrders()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }
        [HttpPost]
        public JsonResult TotalCompleteOrders(int page = 0)
        {
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page > 0) ? page : 1;
            //DateTime stDate1 = Convert.ToDateTime(stDate);
            //DateTime enDate1 = Convert.ToDateTime(enDate);

            var list1 = db.OrderMasters.OrderByDescending(a => new { a.Date, a.BillNo }).Where(a => a.IsComplete == true && a.IsProcess == false && a.IsCancle == false).ToList();

            var list = list1.Select(a => new
            {
                a.OrderType,
                a.CustomerDetail.CustomerName,
                a.BillNo,
                Date = a.Date.Value.ToString("dd-MM-yyyy"),
                a.OrderMaster_ID,
                TableNo = (a.Table_ID == null) ? "-" : a.TableMaster.TableNo,
                TotalAmount = a.OrderDetailMasters.Sum(am => am.Amount.Value),
                TotalItem = a.OrderDetailMasters.Count,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordCount = list1.Count,
            }).ToPagedList(pageIndex, pageSize);
            ViewBag.PageList = list;
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        //-----------------------------------------------------------
        public ActionResult TotalCencelOrders()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }
        [HttpPost]
        public JsonResult TotalCencelOrders(int page = 0)
        {
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page > 0) ? page : 1;
            //DateTime stDate1 = Convert.ToDateTime(stDate);
            //DateTime enDate1 = Convert.ToDateTime(enDate);

            var list1 = db.OrderMasters.OrderByDescending(a => new { a.Date, a.BillNo }).Where(a => a.IsComplete == false && a.IsProcess == false && a.IsCancle == true).ToList();

            var list = list1.Select(a => new
            {
                a.OrderType,
                a.CustomerDetail.CustomerName,
                a.BillNo,
                Date = a.Date.Value.ToString("dd-MM-yyyy"),
                a.OrderMaster_ID,
                TableNo = (a.Table_ID == null) ? "-" : a.TableMaster.TableNo,
                TotalAmount = a.OrderDetailMasters.Sum(am => am.Amount.Value),
                TotalItem = a.OrderDetailMasters.Count,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordCount = list1.Count,
            }).ToPagedList(pageIndex, pageSize);
            ViewBag.PageList = list;
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        //--------------------

        //-----------------------------------------------------------
        public ActionResult TotalPendingOrders()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }
        [HttpPost]
        public JsonResult TotalPendingOrders(int page = 0)
        {
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page > 0) ? page : 1;
            //DateTime stDate1 = Convert.ToDateTime(stDate);
            //DateTime enDate1 = Convert.ToDateTime(enDate);

            var list1 = db.OrderMasters.OrderByDescending(a => new { a.Date, a.BillNo }).Where(a => a.IsComplete == false && a.IsProcess == false && a.IsCancle == false).ToList();

            var list = list1.Select(a => new
            {
                a.OrderType,
                a.CustomerDetail.CustomerName,
                a.BillNo,
                Date = a.Date.Value.ToString("dd-MM-yyyy"),
                a.OrderMaster_ID,
                TableNo = (a.Table_ID == null) ? "-" : a.TableMaster.TableNo,
                TotalAmount = a.OrderDetailMasters.Sum(am => am.Amount.Value),
                TotalItem = a.OrderDetailMasters.Count,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordCount = list1.Count,
            }).ToPagedList(pageIndex, pageSize);
            ViewBag.PageList = list;
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        //--------------------
        //-----------------------------------------------------------
        public ActionResult TotalProcessOrders()
        {
            if (Session["Admin_Id"] == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }
        [HttpPost]
        public JsonResult TotalProcessOrders(int page = 0)
        {
            int pageSize = 10;
            int pageIndex = 1;
            pageIndex = (page > 0) ? page : 1;
            //DateTime stDate1 = Convert.ToDateTime(stDate);
            //DateTime enDate1 = Convert.ToDateTime(enDate);

            var list1 = db.OrderMasters.OrderByDescending(a => new { a.Date, a.BillNo }).Where(a => a.IsComplete == false && a.IsProcess == true && a.IsCancle == false).ToList();

            var list = list1.Select(a => new
            {
                a.OrderType,
                a.CustomerDetail.CustomerName,
                a.BillNo,
                Date = a.Date.Value.ToString("dd-MM-yyyy"),
                a.OrderMaster_ID,
                TableNo = (a.Table_ID == null) ? "-" : a.TableMaster.TableNo,
                TotalAmount = a.OrderDetailMasters.Sum(am => am.Amount.Value),
                TotalItem = a.OrderDetailMasters.Count,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordCount = list1.Count,
            }).ToPagedList(pageIndex, pageSize);
            ViewBag.PageList = list;
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        //--------------------
        public ActionResult Logout()
        {
            Session["Admin_Id"] = null;
            return RedirectToAction("Login");
        }
    }
}