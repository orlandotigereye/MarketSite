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
    public class StoreDayController : BaseController
    {

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
        
        //private InkismMISTestEntities db = new InkismMISTestEntities();

        // GET: StoreDay
        public ActionResult Index(string searchBrand = "", string searchCom = "", string searchStore = "", string searchDate = "")
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
            //取得使用者可以瀏覽的店家

            SelectList storeList = getStoreNameList(comId);
            ViewBag.searchStore = storeList;

            var singleStore = storeList.FirstOrDefault().ToString();
            string[] storeItem = singleStore.Split(' ');

            //店號+店名
            var storeIdName = "";
            //店號
            var storeId = "";

            //判斷搜尋條件：日期跟店家是否為空值
            StoreDayViewModel model = new StoreDayViewModel();
            if (String.IsNullOrEmpty(searchDate))
            {
                model = null;
                return View(model);
            }
            if (!String.IsNullOrEmpty(searchStore))
            {
                storeItem = searchStore.Split(' ');
                storeId = storeItem[0];
                storeIdName = searchStore;
            }
            ViewBag.storeId = storeId;
            ViewBag.storeIdName = storeIdName;
/*-------------------搜尋條件----------------------*/

            //比對區域代碼 並抓該區域代碼的稅率計算
            decimal tax = 1;
            decimal isTax = 0;  //判斷該區域是否有列在稅率表 0:無; 1:有            
            foreach (var t in taxAreaList())
            {
                if (t.Com == comId)
                {
                    isTax = 1;
                    tax = t.Tax;
                    continue;
                }
            }

            //統計
            var dayRetail = db.StoreDayRetail
                .Join(db.StoreData, d => d.StoreNo, s => s.StoreNo, (d, s) => new
                StoreDayInfo
                {
                    StoreNo = d.StoreNo,
                    OrderCount = d.OrderCount,
                    DayCount = d.DayCount,
                    NightCount = d.NightCount,
                    //NonTaxTotal = d.TotalRetail / (decimal)1.05,
                    NonTaxTotal = (isTax == 1) ? (d.TotalRetail / tax).ToString() : "--",
                    TotalRetail = d.TotalRetail,
                    RetailDate = d.RetailDate,
                    StoreName = s.StoreName,
                    CurrencyID = s.CurrencyID
                })
                .Where(d => UserStoreListByStoreNo.Contains(d.StoreNo));


            //單一店家資料
            var singleDayRetail = dayRetail.Where(i => i.StoreNo == storeId && i.RetailDate.ToString() == searchDate)
                .Select(i => i).SingleOrDefault();
            
            //單一店家底下的列表
            var retails = db.Retail
                .Where(r => r.IsCancel == 0) //排除取消的訂單
                .Join(db.StoreData, r => r.StoreNo, s => s.StoreNo, (r, s) => new
                StoreDayList
                {
                    StoreNo = r.StoreNo,
                    OrderNo = r.OrderNo,
                    SubPrice = r.SubPrice,
                    ServiceCharge = r.ServiceCharge,
                    TotalPrice = r.TotalPrice,
                    CreateBy = r.CreateBy,
                    OrderDateTime = r.OrderDateTime,
                    RetailDate = r.RetailDate,
                    CurrencyID = s.CurrencyID,
                    OrderType = r.OrderType
                })
                .Where(r => r.StoreNo == storeId && r.RetailDate.ToString() == searchDate);

            //取餐方式
            var carryGoods = retails
                .GroupBy(r => r.OrderType)
                .Select(r => new StoreDayCarry
                {
                    CarryType = r.Key,
                    CarryNum = r.Count(),
                    CarryMoney = r.Sum(x => x.TotalPrice)
                }).ToList();
            List<StoreDayCarry> carryList = new List<StoreDayCarry>();
            carryList.Add(new StoreDayCarry { CarryType = "內用", CarryNum = 0, CarryMoney = 0 });
            carryList.Add(new StoreDayCarry { CarryType = "自取", CarryNum = 0, CarryMoney = 0 });
            carryList.Add(new StoreDayCarry { CarryType = "外帶", CarryNum = 0, CarryMoney = 0 });
            carryList.Add(new StoreDayCarry { CarryType = "外送", CarryNum = 0, CarryMoney = 0 });

            foreach (var goods in carryGoods)
            {
                foreach (var list in carryList)
                {
                    if (goods.CarryType == list.CarryType)
                    {
                        list.CarryNum = goods.CarryNum;
                        list.CarryMoney = goods.CarryMoney;
                    }
                }
            }
            
            //區域底下的所有店家資料
            if (String.IsNullOrEmpty(searchStore))
            {
                List<string> storeIdList = getStoreIdList(comId);

                var dayRetailByAll = dayRetail
                    .Where(i => storeIdList.Contains(i.StoreNo) && i.RetailDate.ToString() == searchDate);

                model.AllRetailItem = dayRetailByAll;
                model.RetailItem = null;
                model.RetailCarry = null;
                model.RetailList = null;
                return View(model);
            }

            model.AllRetailItem = null;
            model.RetailItem = singleDayRetail;
            model.RetailList = retails;
            model.RetailCarry = carryList;

            return View(model);
        }

        // GET: StoreDay/Details/5
        public ActionResult Details(string storeId = "", string id = "")
        {
            //沒有使用者ID就不能瀏覽
            if (String.IsNullOrEmpty(UserId))
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                return new HttpStatusCodeResult(401);
            }
            if (!UserStoreListByStoreNo.Contains(storeId) || String.IsNullOrEmpty(id))
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                return new HttpStatusCodeResult(404);
            }


            //單一筆資料時
            var orderItem = db.RetailSub.Where(r => r.StoreNo == storeId).FirstOrDefault(r => r.OrderNo == id);
            ViewBag.orderItem = orderItem;

            //顯示多筆資料
            var retailSub = db.RetailSub.Where(r => r.StoreNo == storeId && r.OrderNo == id).OrderBy(r => r.OrderNo);

            //訂單金額
            decimal money = 0;
            foreach (var item in retailSub)
            {
                money += item.Price;
            }
            ViewBag.money = money;

            if (retailSub == null)
            {
                return HttpNotFound();
            }
            return View(retailSub.ToList());
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
