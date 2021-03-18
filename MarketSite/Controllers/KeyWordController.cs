using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MarketSite.Models;

namespace MarketSite.Controllers
{
    [Authorize(Roles = "Manage")]
    public class KeyWordController : BaseController
    {
        //private InkismMISEntities db = new InkismMISEntities();

        // GET: KeyWord
        public ActionResult Index()
        {
            return View(db.ExtendKeyWord.ToList());
        }

        // GET: KeyWord/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: KeyWord/Create
        // 若要免於過量張貼攻擊，請啟用想要繫結的特定屬性，如需
        // 詳細資訊，請參閱 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,category,keyword,status")] ExtendKeyWord extendKeyWord)
        {
            if (ModelState.IsValid)
            {
                extendKeyWord.category = "remove";
                extendKeyWord.status = 0;
                db.ExtendKeyWord.Add(extendKeyWord);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(extendKeyWord);
        }

        // GET: KeyWord/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExtendKeyWord extendKeyWord = db.ExtendKeyWord.Find(id);
            if (extendKeyWord == null)
            {
                return HttpNotFound();
            }
            return View(extendKeyWord);
        }

        // POST: KeyWord/Edit/5
        // 若要免於過量張貼攻擊，請啟用想要繫結的特定屬性，如需
        // 詳細資訊，請參閱 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,category,keyword,status")] ExtendKeyWord extendKeyWord)
        {
            if (ModelState.IsValid)
            {
                db.Entry(extendKeyWord).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(extendKeyWord);
        }

        // GET: KeyWord/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ExtendKeyWord extendKeyWord = db.ExtendKeyWord.Find(id);
            if (extendKeyWord == null)
            {
                return HttpNotFound();
            }
            return View(extendKeyWord);
        }

        // POST: KeyWord/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ExtendKeyWord extendKeyWord = db.ExtendKeyWord.Find(id);
            db.ExtendKeyWord.Remove(extendKeyWord);
            db.SaveChanges();
            return RedirectToAction("Index");
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
