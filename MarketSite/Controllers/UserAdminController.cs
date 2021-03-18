using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MarketSite.Models;
using System.Text;
using System.Security.Cryptography;

namespace MarketSite.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserAdminController : BaseController
    {
        // GET: UserAdmin
        public ActionResult Index(string searchId)
        {
            if (!String.IsNullOrEmpty(searchId))
            {
                var searchUser = db.ExtendUser
                                .Where(u => u.ExAccountID == searchId)
                                .Select(u => u);
                return View(searchUser);
            }
            return View(db.ExtendUser.ToList());
        }

        // GET: UserAdmin/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExtendUser account = db.ExtendUser.Find(id);
            if (account == null)
            {
                return HttpNotFound();
            }

            ViewBag.RoleNames = account.ExtendRole.Select(r => r.ExRoleName).ToList();

            return View(account);
        }

        // GET: UserAdmin/Create
        public ActionResult Create()
        {
            ViewBag.RoleId = new SelectList(db.ExtendRole.ToList(), "ExRoleName", "ExRoleName");
            return View();
        }

        /*--ExAccountID 使用者代碼(8碼)--*/
        //12碼 國別, 3碼 代理(M)/加盟(F), 45碼 品牌碼, 678碼 客戶編號後三碼
        // POST: UserAdmin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ExUserId,ExUserPassword,ExUserName,ExUserDep")] ExtendUser model, string[] selectedRole)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.ExAccountUUID = Guid.NewGuid();
                    model.ExAccountID = model.ExUserId;
                    model.ExRealName = model.ExUserName;
                    model.ExDep = model.ExUserDep;
                    //password加密
                    model.ExPassword = Enpasswd(model.ExAccountUUID, model.ExUserPassword);

                    db.ExtendUser.Add(model);
                    //db.SaveChanges();
                    
                    //角色權限
                    UpdateUserRoles(selectedRole, model);
                    db.SaveChanges();
                    
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (DataException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            return View(model);
        }
        //密碼加密
        public string Enpasswd(Guid uid, string upasswd)
        {
            string ret = "";
            string passwdAddSalt = uid + upasswd;
            byte[] inputByteArray = Encoding.UTF8.GetBytes(passwdAddSalt);
            SHA256CryptoServiceProvider csp = new SHA256CryptoServiceProvider();
            ret = BitConverter.ToString(csp.ComputeHash(inputByteArray)).Replace("-", string.Empty);

            return ret;
        }

        // GET: UserAdmin/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExtendUser account = db.ExtendUser.Find(id);
            if (account == null)
            {
                return HttpNotFound();
            }
            
            var userRoles = new HashSet<string>(account.ExtendRole.Select(r => r.ExRoleName));

            //品牌頁籤
            ViewBag.BrandTab = db.StoreBrand.Select(b => b).ToList();


            //代理商選單
            UserId = account.ExAccountUUID.ToString();
            PopulateAssignedStoreCom(ComByUser);

            return View(new EditUserViewModel()
            {
                Id = account.ExAccountUUID,
                AccountId = account.ExAccountID,
                UserName = account.ExRealName,
                UserDep = account.ExDep,
                RolesList = db.ExtendRole.ToList().Select(x => new SelectListItem()
                {
                    Selected = userRoles.Contains(x.ExRoleName),
                    Text = x.ExRoleName,
                    Value = x.ExRoleName
                }),
                AgentList = ComByUser
            });
        }
        private void PopulateAssignedStoreCom(ExtendUser ComByUser)
        {
            var agentList = db.StoreCom
                            //.Where(c => c.ComBrand == "0001")
                            .Select(c => c);
            var extendUserStoreCom = new HashSet<string>(ComByUser.StoreCom
                //.Where(c => c.ComBrand == "0001")
                .Select(c => c.ComID));
            var viewModel = new List<AssignedAgent>();
            foreach (var agent in agentList)
            {
                viewModel.Add(new AssignedAgent
                {
                    ComBrand = agent.ComBrand,
                    ComID = agent.ComID,
                    ComName = agent.ComName,
                    Assigned = extendUserStoreCom.Contains(agent.ComID)
                });
            }
            ViewBag.Agents = viewModel;
        }

        //
        // POST: /Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AccountId,UserName,UserDep,Id")] EditUserViewModel editUser, string storeSubmit, string[] selectedRole, params string[] selectedAgents)
        {
            if (ModelState.IsValid)
            {
                //必填欄位必須帶入值才能儲存                
                db.Configuration.ValidateOnSaveEnabled = false;

                var user = db.ExtendUser.Find(editUser.Id);

                /*忽略必填欄位
                db.Entry(user).State = EntityState.Modified;
                */

                if (user == null)
                {
                    return HttpNotFound();
                }

                user.ExAccountID = editUser.AccountId;
                user.ExRealName = editUser.UserName;
                user.ExDep = editUser.UserDep;

                //角色權限
                var userRoles = new HashSet<string>(user.ExtendRole.Select(r => r.ExRoleName));
                selectedRole = selectedRole ?? new string[] { };
                UpdateUserRoles(selectedRole, user);
                db.SaveChanges();

                //總部/代理商選單
                UserId = user.ExAccountUUID.ToString();
                UpdateUserAgents(selectedAgents, ComByUser);
                db.SaveChanges();

                PopulateAssignedStoreCom(ComByUser);

                //修改AspNetUserStoreData，如果是已勾選代理商底下的店家，這些店家就刪除
                //1.取得已勾選代理商
                var extendUserStoreCom = new HashSet<string>(ComByUser.StoreCom
                    //.Where(c => c.ComBrand == "0001")
                    .Select(c => c.ComID));
                //2.取得已勾選代理商的店家列表 StoreByUserCom
                //3.依照帳號取得 AspNetUserStoreDate的店家列表
                var extendUserStoreData = new HashSet<string>(StoreByUser.StoreData.Select(c => c.StoreNo));
                //4.刪除已勾選代理商底下的店家
                foreach (var i in StoreByUserCom)
                {
                    if (extendUserStoreData.Contains(i.StoreNo))
                    {
                        StoreByUser.StoreData.Remove(i);
                    }
                }
                db.SaveChanges();
                
                //連結到店家列表
                if (!String.IsNullOrEmpty(storeSubmit))
                {
                    return RedirectToAction("StoreEdit", new { id = user.ExAccountUUID });
                }

                return RedirectToAction("Edit");
            }
            ModelState.AddModelError("", "Something failed.");

            return View();
        }
        private void UpdateUserRoles(string[] selectedRole, ExtendUser user)
        {
            if (selectedRole == null)
            {
                user.ExtendRole = new List<ExtendRole>();
                return;
            }

            var selectedRoleHS = new HashSet<string>(selectedRole);
            var extendUserRole = new HashSet<Guid>(user.ExtendRole
                .Select(r => r.ExRoleUUID));
            foreach (var role in db.ExtendRole)
            {
                if (selectedRoleHS.Contains(role.ExRoleName))
                {
                    if (!extendUserRole.Contains(role.ExRoleUUID))
                    {
                        user.ExtendRole.Add(role);
                    }
                }
                else
                {
                    if (extendUserRole.Contains(role.ExRoleUUID))
                    {
                        user.ExtendRole.Remove(role);
                    }
                }
            }
        }
        private void UpdateUserAgents(string[] selectedAgents, ExtendUser ComByUser)
        {
            if (selectedAgents == null)
            {
                ComByUser.StoreData = new List<StoreData>();
                return;
            }

            var selectedAgentsHS = new HashSet<string>(selectedAgents);
            var extendUserStoreCom = new HashSet<string>(ComByUser.StoreCom
                //.Where(c => c.ComBrand == "0001")
                .Select(c => c.ComID));
            foreach (var agent in db.StoreCom)
            {
                if (selectedAgentsHS.Contains(agent.ComID))
                {
                    if (!extendUserStoreCom.Contains(agent.ComID))
                    {
                        ComByUser.StoreCom.Add(agent);
                    }
                }
                else
                {
                    if (extendUserStoreCom.Contains(agent.ComID))
                    {
                        ComByUser.StoreCom.Remove(agent);
                    }
                }
            }
        }

        // GET: /Users/StoreEdit/5
        public ActionResult StoreEdit(Guid? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = db.ExtendUser.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            //品牌頁籤
            ViewBag.BrandTab = db.StoreBrand.Select(b => b).ToList();


            //店家選單
            UserId = user.ExAccountUUID.ToString();
            PopulateAssignedStoreData(StoreByUser, id);

            return View(new StoreEditViewModel()
            {
                Id = user.ExAccountUUID,
                StoreList = StoreByUser
            });
        }
        private void PopulateAssignedStoreData(ExtendUser StoreByUser, Guid? id)
        {
            //排除已勾選代理底下的店家
            //1.撈出已勾選的代理商
            var extendUserStoreCom = new HashSet<string>(ComByUser.StoreCom
                //.Where(c => c.ComBrand == "0001")
                .Select(c => c.ComID));
            //2.排除已勾選代理商底下的店家
            var extendCom = from c in db.StoreData
                            //where c.StoreBrand == "0001"
                            where !extendUserStoreCom.Contains((string)c.StoreCom)
                            select c;

            var extendUserStoreData = new HashSet<string>(StoreByUser.StoreData.Select(c => c.StoreNo));
            var viewModel = new List<AssignedStoreData>();
            foreach (var store in extendCom)
            {
                viewModel.Add(new AssignedStoreData
                {
                    StoreBrand = store.StoreBrand,
                    StoreId = store.StoreNo,
                    StoreName = store.StoreName,
                    Assigned = extendUserStoreData.Contains(store.StoreNo)
                });
            }
            ViewBag.Stores = viewModel;
        }

        // POST: /Users/StoreEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StoreEdit([Bind(Include = "Id")] StoreEditViewModel editUser, params string[] selectedStores)
        {
            if (ModelState.IsValid)
            {
                var user = db.ExtendUser.Find(editUser.Id);
                if (user == null)
                {
                    return HttpNotFound();
                }

                //店家選單
                UserId = user.ExAccountUUID.ToString();
                UpdateUserStores(selectedStores, StoreByUser);
                db.SaveChanges();

                PopulateAssignedStoreData(StoreByUser, user.ExAccountUUID);
                //return View(userStoreToUpdate);

                return RedirectToAction("StoreEdit");
            }
            ModelState.AddModelError("", "Something failed.");
            return View();

        }
        private void UpdateUserStores(string[] selectedStores, ExtendUser StoreByUser)
        {
            if (selectedStores == null)
            {
                StoreByUser.StoreData = new List<StoreData>();
                return;
            }

            var selectedStoresHS = new HashSet<string>(selectedStores);
            var extendUserStoreData = new HashSet<string>(StoreByUser.StoreData.Select(c => c.StoreNo));
            //var yifangStore = db.StoreData.Where(s => s.StoreBrand == "0001").Select(s => s);
            var Store = db.StoreData.Select(s => s);

            //foreach (var store in yifangStore)
            foreach (var store in Store)
            {
                if (selectedStoresHS.Contains(store.StoreNo.ToString()))
                {
                    if (!extendUserStoreData.Contains(store.StoreNo))
                    {
                        StoreByUser.StoreData.Add(store);
                    }
                }
                else
                {
                    if (extendUserStoreData.Contains(store.StoreNo))
                    {
                        StoreByUser.StoreData.Remove(store);
                    }
                }
            }
        }


        //
        // GET: /Users/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = db.ExtendUser.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        //
        // POST: /Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid? id)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var user = db.ExtendUser.Find(id);
                if (user == null)
                {
                    return HttpNotFound();
                }
                

                //var userRoles = new HashSet<Guid>(user.ExtendRole.Select(r => r.ExRoleUUID));
                db.ExtendUser.Remove(user);
                db.SaveChanges();


                //var result = await UserManager.DeleteAsync(user);
                //if (!result.Succeeded)
                //{
                //    ModelState.AddModelError("", result.Errors.First());
                //    return View();
                //}
                return RedirectToAction("Index");
            }
            return View();
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
