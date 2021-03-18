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
    public class CustomerPriceController : BaseController
    {
        // GET: CustomerPrice
        public ActionResult Index(string searchBrand = "", string searchCom = "", string searchStore = "", string startDate = "", string endDate = "")
        {
            //沒有使用者ID就不能瀏覽
            if (String.IsNullOrEmpty(UserId))
            {   
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
            SelectList storeList = getStoreNameList(comId);
            ViewBag.searchStore = storeList;

            if (String.IsNullOrEmpty(startDate) && String.IsNullOrEmpty(endDate))
            {
                string model = null;
                return View(model);
            }

            //篩選條件 StoreNo , RetailDate(between) 
            var storeDayRetail = db.StoreDayRetail
                                 .Where(s => UserStoreListByStoreNo.Contains(s.StoreNo))
                                 .Select(s => s);

            if (!String.IsNullOrEmpty(searchStore))
            {
                //Response.Write(searchStore);
                string[] storeItem = searchStore.Split(' ');
                var storeId = storeItem[0];
                storeDayRetail = storeDayRetail.Where(s => s.StoreNo == storeId);
            }

            /*//預設起始日期
            var sd = DateTime.Today;
            //預設結束日期
            var ed = DateTime.Today;*/

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

                storeDayRetail = storeDayRetail.Where(s => s.RetailDate >= sd && s.RetailDate <= ed);
            }

            var singleList = storeDayRetail
                .Where(d => d.OrderCount > 0)
                .Join(db.StoreData, d => d.StoreNo, s => s.StoreNo, (d, s) => new {
                    ShowDate = d.RetailDate.ToString(),
                    Total = d.TotalRetail,
                    Customer = d.OrderCount,
                    Price = Math.Round(((d.DayRetail + d.NightRetail) / d.OrderCount), 0),
                    CurrencyName = s.CurrencyID,
                    StoreCom = s.StoreCom
                }).ToList();
            //再依幣別, 日期分類統計金額(for 全部 不分店家)
            var customerList = singleList
                .Where(c => c.StoreCom.Contains(comId))
                .GroupBy(c => new { r = c.CurrencyName, d = c.ShowDate })
                .Select(c => new CustomerPriceViewModel
                {
                    CurrencyName = c.Key.r,
                    ShowDate = c.Key.d,
                    Total = c.Sum(x => x.Total),
                    Customer = c.Sum(x => x.Customer),
                    Price = Math.Round((c.Sum(x =>x.Total) / c.Sum(x => x.Customer)), 0)
                })
                .ToList();

            //依搜尋的日期區間合計
            CustomerPriceViewModel arr = new CustomerPriceViewModel();
            arr.ShowDate = "合計(Total)";
            foreach (var list in customerList)
            {
                arr.Total += list.Total;
                arr.Customer += list.Customer;
                arr.CurrencyName = list.CurrencyName;
            }
            if(arr.Total != 0 && arr.Customer != 0)
                arr.Price = Math.Round((arr.Total / arr.Customer), 0);

            customerList.Add(arr);

            return View(customerList);
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
