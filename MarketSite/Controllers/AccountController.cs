using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MarketSite.Models;

using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace MarketSite.Controllers
{
    public class AccountController : BaseController
    {   
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }
        //登入功能
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(string ExUserId, string ExUserPassword)
        {
            if (ModelState.IsValid)
            {
                //正則表示式  判斷輸入的帳號是否含有特殊符號
                Regex x = new Regex(@"^[a-zA-Z0-9_]+$");
                if (!x.IsMatch(ExUserId))
                {
                    ViewData["Message"] = "帳號密碼錯誤";
                    Response.Write("<script>alert('帳號密碼錯誤');</script>");
                    ExUserId = "";
                    ExUserPassword = "";
                }

                //登入驗證
                Guid? accountGuid = LoginChecked(ExUserId, ExUserPassword);
                if (accountGuid != null)
                {
                    ClaimsIdentity identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, ExUserId), }, DefaultAuthenticationTypes.ApplicationCookie, ClaimTypes.Name, ClaimTypes.Role);
                    identity.AddClaim(new Claim(ClaimTypes.Sid, accountGuid.ToString())); //OK to store userID here?
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, accountGuid.ToString()));
                    
                    //取得使用者角色
                    var user = db.ExtendUser.Find(accountGuid);
                    List<string> userRoles = user.ExtendRole.Select(r => r.ExRoleName).ToList();
                    foreach(var role in userRoles)
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, role));
                    }

                    HttpContext.GetOwinContext().Authentication.SignIn(new AuthenticationProperties { IsPersistent = true }, identity);
                    
                    //寫入log
                    ExtendLog log = new ExtendLog()
                    {
                        Refer = HttpContext.Request.UrlReferrer.AbsolutePath,
                        Destination = HttpContext.Request.Url.AbsolutePath,
                        //Method = filterContext.HttpContext.Request.HttpMethod,
                        RequestTime = DateTime.Now,
                        IPAddress = HttpContext.Request.UserHostAddress,
                        Operator = identity.GetUserName()
                        //Operator = db.Account.Find(accountGuid).RealName
                    };
                    db.ExtendLog.Add(log);
                    db.SaveChanges();

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("ExUserPassword", "Invalid username or password.");
                    
                }
            }
            // If we got this far, something failed, redisplay form
            return View();
        }

        //登出  資料清空
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            Request.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        // GET: /Account/Register
        //[AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        /*--ExAccountID 使用者代碼(8碼)--*/
        //12碼 國別, 3碼 代理(M)/加盟(F), 45碼 品牌碼, 678碼 客戶編號後三碼
        [Authorize]
        // POST: /Account/Register
        [HttpPost]
        //[AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Include = "ExUserId,ExUserPassword,ExUserName,ExUserDep")] ExtendUser model)
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
                    db.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (DataException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
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

        //登入驗證
        public Guid? LoginChecked(string ExUserId, string ExUserPassword)
        {
            string secretCode = "";
            Guid? accountGuid = null;
            var data = db.ExtendUser
                .ToList()
                .Find(u => u.ExAccountID == ExUserId);
            try
            {
                //password加密
                secretCode = Enpasswd(data.ExAccountUUID, ExUserPassword);
                if (data.ExPassword == secretCode)
                {
                    accountGuid = data.ExAccountUUID;
                }
            }
            catch
            {
                return null;
            }
            return accountGuid;
        }

        [Authorize]
        // GET: /Account/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        // POST: /Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Guid userId = Guid.Parse(User.Identity.GetUserId());            
            ExtendUser user = db.ExtendUser.Find(userId);

            model.OldPassword = Enpasswd(user.ExAccountUUID, model.OldPassword);
            if(model.OldPassword == user.ExPassword)
            { 
                    user.ExPassword = Enpasswd(user.ExAccountUUID, model.NewPassword);

                    user.ExUserId = user.ExAccountID;
                    user.ExUserName = user.ExRealName;
                    user.ExUserPassword = user.ExPassword;
                    user.ExUserDep = user.ExDep;
                    
                    db.ExtendUser.Attach(user);
                    db.Entry(user).Property(u => u.ExPassword).IsModified = true;
                    db.SaveChanges();
                

                //model.NewPassword = Enpasswd(user.ExAccountUUID, model.NewPassword);
                //ExtendUser updateUser = db.ExtendUser.Find(userId);
                //updateUser.ExPassword = model.NewPassword;
                //db.Entry(updateUser).State = EntityState.Modified;

                return RedirectToAction("Index", "Home");
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
