using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MarketSite.Models;
using Microsoft.AspNet.Identity;

namespace MarketSite.Controllers
{
    public class StoreProductController : BaseController
    {
        //private InkismMISTestEntities db = new InkismMISTestEntities();

        // GET: StoreProduct
        //public ActionResult Index(string searchBrand, string searchStore, string startDate, string endDate)
        public ActionResult Index(string searchBrand = "", string searchCom = "", string searchStore = "", string startDate = "", string endDate = "")
        {
            StoreProductViewModel model = new StoreProductViewModel();

            /*//品牌列表
            var brandList = new List<string>();
            var storeBrand = from b in db.StoreBrand
                        select b;
            foreach(var item in storeBrand)
            {
                brandList.Add(item.BrandName);
            }
            ViewBag.searchBrand = new SelectList(brandList);

            //取得品牌ID
            var brandId = storeBrand.First().BrandID; //品牌ID預設值
            if (!String.IsNullOrEmpty(searchBrand))
            {
                brandId = storeBrand
                          .Where(i => i.BrandName == searchBrand)
                          .Select(i => i.BrandID)
                          .Single();
            }*/

            //沒有使用者ID就不能瀏覽
            if (String.IsNullOrEmpty(UserId))
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                return new HttpStatusCodeResult(401);
            }

            //第一層選單:取得使用者可以瀏覽的品牌
            ViewBag.searchBrand = UserBrand;

            //第二層選單:取得使用者可以瀏覽的代理商
            var brandId = UserBrand.FirstOrDefault().Value;
            if (!String.IsNullOrEmpty(searchBrand))
            {
                brandId = searchBrand;
            }
            var comList = UserCom
                .Where(u => u.ComBrand.Contains(brandId))
                .Select(u => new SelectListItem() { Text = u.ComName, Value = u.ComID })
                .ToList();
            ViewBag.searchCom = comList;

            //第三層選單:取得可以瀏覽的代理商底下的店家
            var comId = comList.FirstOrDefault().Value;
            if (!String.IsNullOrEmpty(searchCom))
            {
                comId = searchCom;
            }

            //取得使用者可以瀏覽的店家
            //ViewBag.searchStore = new SelectList(UserStoreList);
            var storeList = new List<string>();
            //取得代理商底下的店家
            var comStores = StoreByUserCom
                .Where(x => x.StoreCom.Contains(comId))
                .OrderBy(x => x.StoreCom)
                .Select(x => x.StoreNo + " " + x.StoreName);

            storeList.AddRange(comStores.Distinct());
            var userStores = new HashSet<string>(StoreByUser.StoreData.Where(c => c.StoreCom.Contains(comId)).Select(c => c.StoreNo + " " + c.StoreName));
            storeList.AddRange(userStores.Distinct());
            ViewBag.searchStore = new SelectList(storeList);

            if (String.IsNullOrEmpty(searchStore) && String.IsNullOrEmpty(startDate) && String.IsNullOrEmpty(endDate))
            {
                model = null;
                return View(model);
            }

            //篩選條件 StoreNo , RetailDate(between) 
            var retailSub = db.RetailSub
                .Where(r => UserStoreListByStoreNo.Contains(r.StoreNo))
                .Select(r => r);

            if (!String.IsNullOrEmpty(searchStore))
            {
                string[] storeItem = searchStore.Split(' ');
                var storeId = storeItem[0];
                retailSub = retailSub.Where(r => r.StoreNo == storeId);
            }

            /*//預設起始日期
            var sd = Convert.ToDateTime(DateTime.Today);
            //預設結束日期
            var ed = Convert.ToDateTime(DateTime.Today);*/

            if (!String.IsNullOrEmpty(startDate) && !String.IsNullOrEmpty(endDate))
            {
                var sd = Convert.ToDateTime(startDate);
                var ed = Convert.ToDateTime(endDate);

                if (DateTime.Compare(sd, ed) > 0) //起始日期不能大於結束日期
                {
                    //Response.Write("<script>alert('起始日期不能大於結束日期')</script>");
                    return View();
                }
                else
                {
                    var totalDay = new TimeSpan(ed.Ticks - sd.Ticks).Days;
                    if (totalDay > 182) //查詢日期區間最多6個月
                    {
                        //Response.Write("<script>alert('查詢日期區間最多6個月')</script>");
                        return View();
                    }
                }

                retailSub = retailSub.Where(r => r.RetailDate >= sd && r.RetailDate <= ed);
            }

            //排序
            var singleRetail = retailSub
                .Where(r => r.IsCancel == 0)
                .Join(db.StoreData, r => r.StoreNo, s => s.StoreNo, (r, s) => new
                {
                    Order = 0,
                    ProductName = r.ProductName,
                    ProductCount = r.ProductCount,
                    TotalPrice = r.TotalPrice,
                    CurrencyName = s.CurrencyID,
                    StoreCom = s.StoreCom
                }).ToList();

            var retailOrder = singleRetail
                .Where(r => r.StoreCom.Contains(comId))
                .GroupBy(r => new { p = r.ProductName, c = r.CurrencyName })
                .Select(r => new StoreProductViewModel
                {
                    Order = 0,
                    ProductName = r.Key.p,
                    ProductCount = r.Sum(x => x.ProductCount),
                    TotalPrice = r.Sum(x => x.TotalPrice),
                    CurrencyName = r.Key.c
                })
                .OrderByDescending(x => x.ProductCount)
                .ToList();

            decimal countAll = 0;
            decimal totalAll = 0;
            //序號
            int num = 1;
            foreach (var item in retailOrder)
            {
                //加總數量、金額
                countAll += item.ProductCount;
                totalAll += item.TotalPrice;
                //序號
                item.Order = num;
                num++;
            }
            ViewBag.countAll = countAll;
            ViewBag.totalAll = totalAll;

            return View(retailOrder);
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
