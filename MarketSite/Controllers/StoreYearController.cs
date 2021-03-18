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
    public class StoreYearController : BaseController
    {        
        // GET: StoreYear
        public ActionResult Index(string searchBrand = "", string searchCom = "", string searchStore = "", int searchYear = 0)
        {
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
            ViewBag.searchStore = getStoreNameList(comId);

            //年份列表
            var yearList = new List<int>();
            for (int y = DateTime.Today.Year; y >= 2014; y--)
            {
                yearList.Add(y);
            }
            ViewBag.searchYear = new SelectList(yearList);
            ViewBag.year = searchYear;

            if (String.IsNullOrEmpty(searchStore) && searchYear == 0)
            {
                string model = null;
                return View(model);
            }
            
            //店號+店名            
            var storeIdName = "";
            //店號
            string storeId = "";

            if (!String.IsNullOrEmpty(searchStore))
            {
                storeIdName = searchStore;                
                storeId = searchStore.Split(' ')[0];                
            }
            
            List<StoreYearViewModel> storeYearList = new List<StoreYearViewModel>();
            List<string> storeIdList = new List<string>();
            if (String.IsNullOrEmpty(searchStore))
            {
                //區域底下的所有店家資料
                storeIdList = getStoreIdList(comId);
            }
            else
            {
                storeIdList.Add(storeId);
            }
    
            foreach (var sId in storeIdList)
            {
                StoreYearViewModel singleData = new StoreYearViewModel();
                Store12Month data = new Store12Month() {
                    SubTotal1 = "0.00",
                    SubTotal2 = "0.00",
                    SubTotal3 = "0.00",
                    SubTotal4 = "0.00",
                    SubTotal5 = "0.00",
                    SubTotal6 = "0.00",
                    SubTotal7 = "0.00",
                    SubTotal8 = "0.00",
                    SubTotal9 = "0.00",
                    SubTotal10 = "0.00",
                    SubTotal11 = "0.00",
                    SubTotal12 = "0.00",
                    YearTotal = "0.00"
                };
                var result = db.StoreDayRetail
                    .Where(s => s.StoreNo == sId && s.RetailDate.Year == searchYear)
                    .GroupBy(s => new { y = s.RetailDate.Year, m = s.RetailDate.Month })
                    .Select(s => new StoreYearInfo { StoreId = sId, Month = s.Key.m.ToString(), SubTotal = s.Sum(x => x.TotalRetail) });

                var result2 = db.StoreMonthRetailKeyIn
                    .Where(s => s.StoreNo == sId && s.RetailDate.Year == searchYear)
                    .GroupBy(s => new { y = s.RetailDate.Year, m = s.RetailDate.Month })
                    .Select(s => new StoreYearInfo { StoreId = sId, Month = s.Key.m.ToString(), SubTotal = s.Sum(x => x.TotalRetail) });

                decimal yearTotal = 0;
                foreach (var item in result)
                {
                    data.StoreId = item.StoreId;
                    if (item.Month == "1") data.SubTotal1 = string.Format("{0:N}", item.SubTotal);
                    else if (item.Month == "2") data.SubTotal2 = string.Format("{0:N}", item.SubTotal);
                    else if (item.Month == "3") data.SubTotal3 = string.Format("{0:N}", item.SubTotal);
                    else if (item.Month == "4") data.SubTotal4 = string.Format("{0:N}", item.SubTotal);
                    else if (item.Month == "5") data.SubTotal5 = string.Format("{0:N}", item.SubTotal);
                    else if (item.Month == "6") data.SubTotal6 = string.Format("{0:N}", item.SubTotal);
                    else if (item.Month == "7") data.SubTotal7 = string.Format("{0:N}", item.SubTotal);
                    else if (item.Month == "8") data.SubTotal8 = string.Format("{0:N}", item.SubTotal);
                    else if (item.Month == "9") data.SubTotal9 = string.Format("{0:N}", item.SubTotal);
                    else if (item.Month == "10") data.SubTotal10 = string.Format("{0:N}", item.SubTotal);
                    else if (item.Month == "11") data.SubTotal11 = string.Format("{0:N}", item.SubTotal);
                    else if (item.Month == "12") data.SubTotal12 = string.Format("{0:N}", item.SubTotal);
                    yearTotal += item.SubTotal;
                }

                foreach (var item in result2)
                {
                    data.StoreId = item.StoreId;
                    if (item.Month == "1") data.SubTotal1 = string.Format("{0:N}", item.SubTotal);
                    else if (item.Month == "2") data.SubTotal2 = string.Format("{0:N}", item.SubTotal);
                    else if (item.Month == "3") data.SubTotal3 = string.Format("{0:N}", item.SubTotal);
                    else if (item.Month == "4") data.SubTotal4 = string.Format("{0:N}", item.SubTotal);
                    else if (item.Month == "5") data.SubTotal5 = string.Format("{0:N}", item.SubTotal);
                    else if (item.Month == "6") data.SubTotal6 = string.Format("{0:N}", item.SubTotal);
                    else if (item.Month == "7") data.SubTotal7 = string.Format("{0:N}", item.SubTotal);
                    else if (item.Month == "8") data.SubTotal8 = string.Format("{0:N}", item.SubTotal);
                    else if (item.Month == "9") data.SubTotal9 = string.Format("{0:N}", item.SubTotal);
                    else if (item.Month == "10") data.SubTotal10 = string.Format("{0:N}", item.SubTotal);
                    else if (item.Month == "11") data.SubTotal11 = string.Format("{0:N}", item.SubTotal);
                    else if (item.Month == "12") data.SubTotal12 = string.Format("{0:N}", item.SubTotal);
                    yearTotal += item.SubTotal;
                }

                //年總計(原幣別)
                data.YearTotal = string.Format("{0:N}", yearTotal);

                //取得店名
                data.StoreName = db.StoreData
                    .Where(s => s.StoreNo == sId)
                    .Select(s => s.StoreName).FirstOrDefault();
                //取得幣別
                data.CurrencyName = db.StoreData
                    .Where(s => s.StoreNo == sId)
                    .Select(s => s.CurrencyID).FirstOrDefault();

                /*------匯率換算  google------------------------------------------------------------*/
                /*https:'//www.google.com.tw/search?q=821348.00+HKD+to+NTD&ie=UTF-8&cr=countryTW*/
                if (data.CurrencyName == "NTD") data.CurrencyName = "TWD";
                data.URL = "https://www.google.com.tw/search?q=" + data.YearTotal + "+" + data.CurrencyName + "+to+NTD&ie=UTF-8&cr=countryTW";

                singleData.StoreMonths = data;                  
                storeYearList.Add(singleData);
            }
            return View(storeYearList);
            
                    
            /*------匯率換算  google------------------------------------------------------------*/
            /*https:'//www.google.com.tw/search?q=821348.00+HKD+to+NTD&ie=UTF-8&cr=countryTW*/
            /*if (currencyName == "NTD") {
                currencyName = "TWD";
            }
            string url = "https://www.google.com.tw/search?q=" + total + "+" + currencyName + "+to+NTD&ie=UTF-8&cr=countryTW";
            ViewBag.Url = url;*/            

            //ex.店號111001 年2015
            /*------依照月份統計------*/
            //Linq 寫法
            /*var result = from s in db.StoreDayRetail
                         group s by new { y = s.RetailDate.Year, m = s.RetailDate.Month } into g
                         select new StoreYearViewModel
                         {
                             Month = g.Key.m.ToString(),
                             SubTotal = g.Sum(x => x.TotalRetail)
                         };
            return View(result.ToList());*/

        }

        // GET: StoreYear/Details/5
        public ActionResult Details(string storeId = "", int year = 0, int month = 0)
        {
            //沒有使用者ID就不能瀏覽
            if (String.IsNullOrEmpty(UserId))
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                return new HttpStatusCodeResult(401);
            }
            if (!UserStoreListByStoreNo.Contains(storeId) || year <= 0 || month <= 0)
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                return new HttpStatusCodeResult(404);
            }

            //StoreDayRetail storeDayRetail = db.StoreDayRetail.Find(id);
            //列表
            var storeDayRetail = db.StoreDayRetail
                        .Where(s => s.StoreNo == storeId && s.RetailDate.Year == year && s.RetailDate.Month == month)
                        .Select(s => s)
                        .ToList();

/*----------------------------------------------------*/
            decimal tax = 0;
            decimal isTax = 0;
            //判斷storeId 是屬於哪個區域
            var comId = db.StoreData
                .Where(c => c.StoreNo == storeId)
                .Select(c => c.StoreCom).SingleOrDefault();

            //比對區域代碼 並抓該區域代碼的稅率計算
            foreach (var t in taxAreaList())
            {
                if (t.Com == comId)
                {
                    isTax = 1;
                    tax = t.Tax;
                    continue;
                }
            }
            foreach (var s in storeDayRetail)
            {
                s.ShortDate = s.RetailDate.ToString("yyyy-MM-dd");
                s.NoTaxTotal = (isTax == 1) ? (s.TotalRetail / tax).ToString("#.##") : "--";
            }

            

/*-------------------------------------*/
           
            if (storeDayRetail == null)
            {
                return HttpNotFound();
            }
            return View(storeDayRetail);
        }


        //稅率表
        /*  香港	0%
            日本 內用10% 外帶8%	外加
            雪梨	10%	內含
            安大略省	13%	外加
            新加坡	0%	內含
            吉隆坡	6%	內含
            卡爾加里	5%	外加
            阿德雷德	10%	內含
            紐西蘭奧克蘭	15%	內含
            柬埔寨 10%  內含*/
        public class taxArea
        {
            public string Com { get; set; } //區域
            public decimal Tax { get; set; }
            public string TaxType { get; set; } //I:稅內含, O:稅外加
        }
        public List<taxArea> taxAreaList()
        {
            List<taxArea> list = new List<taxArea>();
            list.Add(new taxArea { Com = "0001001", Tax = (decimal)1.05, TaxType = "I" }); //台灣 內含5%
            list.Add(new taxArea { Com = "0001010", Tax = (decimal)1.10, TaxType = "I" }); //雪梨 內含10%
            list.Add(new taxArea { Com = "0001012", Tax = (decimal)1.13, TaxType = "O" }); //安大略省 外加13%
            list.Add(new taxArea { Com = "0001015", Tax = (decimal)1.06, TaxType = "I" }); //吉隆坡 內含6%
            list.Add(new taxArea { Com = "0001017", Tax = (decimal)1.05, TaxType = "O" }); //卡爾加里 外加5%
            list.Add(new taxArea { Com = "0001025", Tax = (decimal)1.10, TaxType = "I" }); //阿德雷德 內含10%
            list.Add(new taxArea { Com = "0001026", Tax = (decimal)1.15, TaxType = "I" }); //紐西蘭奧克蘭 內含15%
            list.Add(new taxArea { Com = "0001028", Tax = (decimal)1.10, TaxType = "I" }); //柬埔寨 內含 10%
            return list;
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
