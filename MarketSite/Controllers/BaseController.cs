using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Microsoft.AspNet.Identity;
using MarketSite.Models;
using System.Data.Entity;
using System.Threading;
using System.Globalization;
using MarketSite.ActionFilter;

namespace MarketSite.Controllers
{
    [SetCulture]
    public class BaseController : Controller
    {
        protected InkismMISEntities db = new InkismMISEntities();
                
        public BaseController()
        {
        }

        //宣告變數(從Controller傳過來)
        private string _UserId;

        //帶入從Controller傳過來的值
        //1.若未收到值id = null, 預設抓登入的ID
        //2.若有收到值,帶入收到的值(例如管理者所編輯的某一位使用者ID)
        private string getUserId(string id = null)
        {
            if (id == null)
            {
                id = User.Identity.GetUserId();
            }
            return id;
        }

        //取得使用者ID
        protected string UserId
        {
            get
            {
                var id = getUserId(this._UserId);
                return id;
            }
            set
            {
                this._UserId = value;  //從Controller傳過來的值
            }
        }

        //使用者-代理商 join
        protected ExtendUser ComByUser
        {
            get
            {
                return db.ExtendUser
                        .Include(i => i.StoreCom)
                        .Where(i => i.ExAccountUUID.ToString() == UserId)
                        .Single();
            }
        }

        //使用者-店家 join
        protected ExtendUser StoreByUser
        {
            get
            {
                return db.ExtendUser
                        .Include(i => i.StoreData)
                        .Where(i => i.ExAccountUUID.ToString() == UserId)
                        .Single();
            }
        }

        //取得代理商底下的店家(依照使用者勾選的代理商)
        protected IEnumerable<StoreData> StoreByUserCom
        {
            get
            {
                var extendUserStoreCom = new HashSet<string>(ComByUser.StoreCom.Select(c => c.ComID));
                var data = from c in db.StoreData
                           //where c.StoreBrand == "0001"
                           where extendUserStoreCom.Contains((string)c.StoreCom)
                           select c;
                return data;
            }
        }

        //取得使用者可以瀏覽的店家(店號+店名)
        protected List<string> UserStoreList
        {
            get
            {
                var storeList = new List<string>();

                //取得代理商底下的店家
                var comStores = StoreByUserCom
                    .OrderBy(x => x.StoreCom)
                    .Select(x => x.StoreNo + " " + x.StoreName);

                storeList.AddRange(comStores.Distinct());

                //加上品牌判斷
                //var userStores = new HashSet<string>(StoreByUser.StoreData.Where(c => c.StoreBrand == brandId).Select(c => c.StoreNo + " " + c.StoreName));
                var userStores = new HashSet<string>(StoreByUser.StoreData.Select(c => c.StoreNo + " " + c.StoreName));
                storeList.AddRange(userStores.Distinct());
                return storeList;
            }
        }

        //取得使用者可以瀏覽的店家(店號)
        protected List<string> UserStoreListByStoreNo
        {
            get
            {
                var storeList = new List<string>();

                //取得代理商底下的店家
                var comStores = StoreByUserCom
                    .Select(x => x.StoreNo);

                storeList.AddRange(comStores.Distinct());

                var userStores = new HashSet<string>(StoreByUser.StoreData.Select(c => c.StoreNo));
                storeList.AddRange(userStores.Distinct());
                return storeList;
            }
        }

        //選單 取得使用者可以瀏覽的代理商
        protected IEnumerable<StoreCom> UserCom
        {
            get
            {
                var ExtendUserStoreCom = new HashSet<string>(ComByUser.StoreCom.Select(c => c.ComID));
                var ExtendUserStoreData = new HashSet<string>(StoreByUser.StoreData.Select(s => s.StoreCom));
                var ComList = db.StoreCom
                    .Where(c => ExtendUserStoreCom.Contains(c.ComID) || ExtendUserStoreData.Contains(c.ComID))
                    .Select(c => c);
                return ComList;
            }
        }

        //選單 取得使用者可以瀏覽的品牌
        protected List<SelectListItem> UserBrand
        {
            get
            {
                var BrandIds = db.StoreData
                    .Where(u => UserStoreListByStoreNo.Contains(u.StoreNo))
                    .GroupBy(u => u.StoreBrand)
                    .Select(g => g.Key ).ToList();

                var BrandList = db.StoreBrand
                   .Where(b => BrandIds.Contains(b.BrandID))
                   .Select(b =>  new SelectListItem() { Text = b.BrandName, Value = b.BrandID })
                   .ToList();

                return BrandList;
            }
        }
        //選單 取得使用者可以瀏覽的飲料品牌
        //0001,0002,0006,0011 四種飲料品牌
        protected List<SelectListItem> UserDrinkBrand
        {
            get
            {
                var BrandIds = db.StoreData
                    .Where(u => UserStoreListByStoreNo.Contains(u.StoreNo))
                    .GroupBy(u => u.StoreBrand)
                    .Select(g => g.Key).ToList();
                List<string> DrinkList = new List<string>();
                DrinkList.Add("0001");
                DrinkList.Add("0002");
                DrinkList.Add("0006");
                DrinkList.Add("0011");
                var BrandList = db.StoreBrand                    
                    .Where(b => BrandIds.Contains(b.BrandID))
                    .Where(b => DrinkList.Contains(b.BrandID))
                    .Select(b => new SelectListItem() { Text = b.BrandName, Value = b.BrandID })
                    .ToList();
                return BrandList;
            }
        }

        //第二層選單:取得使用者可以瀏覽的代理商
        protected List<SelectListItem> getAreaList(string searchBrand)
        {
            var brandId = UserBrand.FirstOrDefault().Value;
            if (!String.IsNullOrEmpty(searchBrand))
            {
                brandId = searchBrand;
            }
            var list = UserCom
            .Where(u => u.ComBrand.Contains(brandId))
            .Select(u => new SelectListItem() { Text = u.ComName, Value = u.ComID })
            .ToList();
            return list;
        }

        //第三層選單:取得可以瀏覽的代理商底下的店家代號
        protected List<string> getStoreIdList(string comId)
        {
            List<string> list = new List<string>();
            //取得代理商底下的店家代號
            var comStoreIds = StoreByUserCom
                .Where(x => x.StoreCom.Contains(comId))
                .Select(x => x.StoreNo);
            list.AddRange(comStoreIds.Distinct());
            var userStoreIds = new HashSet<string>(StoreByUser.StoreData
                .Where(c => c.StoreCom.Contains(comId))
                .Select(c => c.StoreNo));
            list.AddRange(userStoreIds.Distinct());
            return list;
        }

        //第三層選單:取得可以瀏覽的代理商底下的店家
        //選單顯示用
        protected SelectList getStoreNameList(string comId)
        {
            //取得使用者可以瀏覽的店家
            //ViewBag.searchStore = new SelectList(UserStoreList);
            var storeList = new List<string>();
            //取得代理商底下的店家
            var comStores = StoreByUserCom
                .Where(x => x.StoreCom.Contains(comId))
                .OrderBy(x => x.StoreCom)
                .Select(x => x.StoreNo + " " + x.StoreName);
            storeList.AddRange(comStores.Distinct());
            var userStores = new HashSet<string>(StoreByUser.StoreData
                .Where(c => c.StoreCom.Contains(comId))
                .Select(c => c.StoreNo + " " + c.StoreName));
            storeList.AddRange(userStores.Distinct());
            SelectList list = new SelectList(storeList);           
            return list;
        }


        //切換語系
        /*public string _Lang;
        public string OnLang
        {
            set
            {
                this._Lang = "en-US";
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(this._Lang);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(this._Lang);
            }
        }
        public List<string> LangList
        {
            get
            {
                var list = new List<string>();
                return list;
            }
        }*/
        public ActionResult SetCulture(string id)
        {
            HttpCookie userCookie = Request.Cookies["Culture"];

            userCookie.Value = id;
            userCookie.Expires = DateTime.Now.AddYears(100);
            Response.SetCookie(userCookie);

            return Redirect(Request.UrlReferrer.ToString());
        }

    }    
}