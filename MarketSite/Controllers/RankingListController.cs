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
    public class RankingListController : BaseController
    {
        // GET: RankingList       
        public ActionResult Index(string searchBrand = "", string searchCom = "", string startDate = "", string endDate = "")
        {
            /*-------------------搜尋條件----------------------*/
            //沒有使用者ID就不能瀏覽
            if (String.IsNullOrEmpty(UserId))
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                return new HttpStatusCodeResult(401);
            }

            //第一層選單:取得使用者可以瀏覽的品牌
            ViewBag.searchBrand = UserBrand;

            //第二層選單:取得使用者可以瀏覽的代理商
            var comList = getAreaList(searchBrand);
            ViewBag.searchCom = comList;

            //第三層選單:取得可以瀏覽的代理商底下的店家
            var comId = comList.FirstOrDefault().Value;
            if (!String.IsNullOrEmpty(searchCom)) comId = searchCom;

            /*-------------------搜尋條件----------------------*/

            //判斷搜尋條件：日期跟店家是否為空值
            RankingListViewModel model = new RankingListViewModel();
            if (String.IsNullOrEmpty(searchCom))
            {
                model = null;
                return View(model);
            }
            
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
                    //if (totalDay > 182) //查詢日期區間最多6個月
                    if (totalDay > 31)
                    {
                        //Response.Write("<script>alert('查詢日期區間最多6個月')</script>");
                        return View();
                    }
                }

                //統計
                /*
                var dayRetail = db.StoreDayRetail
                    .Where(d => UserStoreListByStoreNo.Contains(d.StoreNo))
                    .Where(d => d.RetailDate >= sd && d.RetailDate <= ed)
                    .GroupBy(d => d.StoreNo)
                    .Join(db.StoreData, d => d.Key, s => s.StoreNo, (d, s) => new
                    Ranking
                    {
                        StoreNo = d.Key,
                        OrderCount = d.Sum(t => t.OrderCount),
                        TotalRetail = d.Sum(t => t.TotalRetail),
                        StoreName = s.StoreName,
                        CurrencyID = s.CurrencyID,
                        StoreCom = s.StoreCom
                    })
                    .Where(ds => ds.StoreCom == comId)
                    .OrderByDescending(ds => ds.TotalRetail);
                     
                    model.RankingList = dayRetail;
                   */
                //篩選出日期區間杯數統計
                List<string> keyword = db.ExtendKeyWord
                    .Select(k => k.keyword).ToList();
                
                var product = db.RetailSub
                    .Where(p => UserStoreListByStoreNo.Contains(p.StoreNo))
                    .Where(p => p.RetailDate >= sd && p.RetailDate <= ed)
                    .Where(p => p.IsCancel == 0);
                
                foreach (var item in keyword)
                {
                    product = product.Where(p1 => !p1.ProductName.Contains(item));
                }

                var productCup = product
                    .GroupBy(p => p.StoreNo)
                    .Select(p => new
                    {
                        storeid = p.Key,
                        pdcount = p.Sum(t => t.ProductCount)
                    });

                ////篩選出日期區間訂單與銷售額統計
                var productOrderAmount = db.StoreDayRetail
                    .Where(o => UserStoreListByStoreNo.Contains(o.StoreNo))
                    .Where(o => o.RetailDate >= sd && o.RetailDate <= ed)
                    .GroupBy(o => o.StoreNo)
                    .Join(db.StoreData, o => o.Key, c => c.StoreNo, (o, c) => new
                    {
                        storeno = c.StoreNo,
                        ordercount = o.Sum(t => t.OrderCount),
                        totalretail = o.Sum(t => t.TotalRetail),
                        storename = c.StoreName,
                        currencyid = c.CurrencyID,
                        storecom = c.StoreCom,
                    })
                    .Where(os => os.storecom == comId);

                ////連接join
                var dayRetail = productOrderAmount
                    .Join(productCup, a => a.storeno, b => b.storeid, (a, b) => new
                    Ranking
                    {
                        StoreNo = a.storeno,
                        OrderCount = a.ordercount,
                        TotalRetail = a.totalretail,
                        ProdCount = b.pdcount,
                        StoreName = a.storename,
                        CurrencyID = a.currencyid,
                        StoreCom = a.storecom
                    })
                    .OrderByDescending(ds => ds.TotalRetail);
                
                model.RankingList = dayRetail;
            }
            return View(model);
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
