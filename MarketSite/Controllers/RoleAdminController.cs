using MarketSite.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;

using System;
using System.Data.Entity;

namespace MarketSite.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleAdminController : BaseController
    {
        public RoleAdminController()
        {
        }

        //
        // GET: /Role/
        public ActionResult Index()
        {
            return View(db.ExtendRole.ToList());
        }

        //
        // GET: /Role/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //取得角色名稱
            var role = db.ExtendRole.Find(id);
            // Get the list of Users in this Role
            var userByRole = new HashSet<Guid>(role.ExtendUser.Select(u => u.ExAccountUUID));
            var users = db.ExtendUser
                .Where(u => userByRole.Contains(u.ExAccountUUID))
                .Select(u => u);


            // Get the list of Users in this Role
            /*var userByRole = db.ExtendUserRole
                .Where(r => r.ExRoleUUID == id)
                .Select(r => r);
            //var users = new List<ExtendUser>();
            //foreach (var user in userByRole)
            //{
            //    var data = db.ExtendUser
            //    .Where(u => u.ExAccountUUID == user.ExAccountUUID)
            //    .Single();
            //    if (data != null)
            //    {
            //        users.Add(data);
            //    }
            //}*/
            ViewBag.Users = users;
            ViewBag.UserCount = users.Count();
            
            return View(role);
        }

        //
        // GET: /Role/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Role/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ExRoleUUID,ExRoleName")] ExtendRole role)
        {
            if (ModelState.IsValid)
            {
                role.ExRoleUUID = Guid.NewGuid();
                db.ExtendRole.Add(role);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(role);
        }

        // GET: Role/Edit/Admin
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExtendRole role = db.ExtendRole.Find(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            return View(role);
        }

        // POST: Role/Edit/Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ExRoleUUID,ExRoleName")] ExtendRole role)
        {
            if (ModelState.IsValid)
            {
                db.Entry(role).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(role);
        }

        // GET: Role/Delete/Admin
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExtendRole role = db.ExtendRole.Find(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            return View(role);
        }

        // POST: Role/Delete/Admin
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            ExtendRole role = db.ExtendRole.Find(id);
            db.ExtendRole.Remove(role);
            db.SaveChanges();

            //UserRole的mapping表也要刪除

            return RedirectToAction("Index");
        }


        //    //
        //    // POST: /Roles/Delete/5
        //    [HttpPost, ActionName("Delete")]
        //    [ValidateAntiForgeryToken]
        //    public ActionResult DeleteConfirmed(string id, string deleteUser)
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            if (id == null)
        //            {
        //                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //            }
        //            var role = await RoleManager.FindByIdAsync(id);
        //            if (role == null)
        //            {
        //                return HttpNotFound();
        //            }
        //            IdentityResult result;
        //            if (deleteUser != null)
        //            {
        //                result = await RoleManager.DeleteAsync(role);
        //            }
        //            else
        //            {
        //                result = await RoleManager.DeleteAsync(role);
        //            }
        //            if (!result.Succeeded)
        //            {
        //                ModelState.AddModelError("", result.Errors.First());
        //                return View();
        //            }
        //            return RedirectToAction("Index");
        //        }
        //        return View();
        //    }
    }
}
