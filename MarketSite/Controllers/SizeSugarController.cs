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
using System.IO;
using Microsoft.AspNet.Identity;
//using Microsoft.Report.Viewer;
//using Microsoft.ReportViewer.WebFor;

namespace MarketSite.Controllers
{
    public class SizeSugarController : BaseController
    {
        //private InkismMISTestEntities db = new InkismMISTestEntities();

        // GET: SizeSugar
        public ActionResult Index(string searchBrand = "", string searchCom = "", string searchStore = "", string startDate = "", string endDate = "")
        {

            //沒有使用者ID就不能瀏覽
            if (String.IsNullOrEmpty(UserId))
            {
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                return new HttpStatusCodeResult(401);
            }
            //第一層選單:取得使用者可以瀏覽的飲料品牌            
            ViewBag.searchBrand = UserDrinkBrand;            

            //第二層選單:取得使用者可以瀏覽的代理商
            var comList = getAreaList(searchBrand);
            ViewBag.searchCom = comList;

            //第三層選單:取得可以瀏覽的代理商底下的店家
            var comId = comList.FirstOrDefault().Value;
            if (!String.IsNullOrEmpty(searchCom)) comId = searchCom;
            SelectList storeList = getStoreNameList(comId);
            ViewBag.searchStore = storeList;

            if (String.IsNullOrEmpty(searchStore) && String.IsNullOrEmpty(startDate) && String.IsNullOrEmpty(endDate))
            {
                string model = null;
                return View(model);
            }
            //篩選條件 StoreNo , RetailDate(between)
            //取得可以瀏覽的店家
            var stores = db.StoreData
                         .Where(s => s.StoreBrand == searchBrand)
                         .Where(s => s.StoreCom == searchCom)
                         .Select(s => s.StoreNo);

            var retailSub = db.RetailSub
                            .Where(r => r.IsCancel == 0) //過濾取消的訂單
                            .Where(r => UserStoreListByStoreNo.Contains(r.StoreNo))
                            .Where(r => stores.Contains(r.StoreNo))
                            .Select(r => r);
            if (!String.IsNullOrEmpty(searchStore))
            {
                string[] storeItem = searchStore.Split(' ');
                var storeId = storeItem[0];
                retailSub = retailSub.Where(r => r.StoreNo == storeId);
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
                    if (totalDay > 182) //查詢日期區間最多6個月
                    {
                        //Response.Write("<script>alert('查詢日期區間最多6個月')</script>");
                        return View();
                    }
                }

                retailSub = retailSub.Where(r => r.RetailDate >= sd && r.RetailDate <= ed);
            }

            /*篩選條件
              只能選擇飲料品牌
              過濾取消的訂單*/

            //過濾取消的訂單
            //retailSub = retailSub
            //    .Where(r => r.IsCancel == 0)
            //    .Select(r => r);

            /*Options 分類: 空字串, PCS, 大(L), 小(M), x冰(溫), 熱, 
                            正常糖, 2/3, 1/2, 1/3, 1/4, 甜一點, 無糖 */
            var groupRetailSub = retailSub                            
                            .GroupBy(r => r.ProductName)
                            .Select(r => new SizeSugarViewModel
                            {
                                StoreNo = null,
                                ProductName = r.Key,
                                ProductCount = 0,
                                LIce = 0,
                                LHot = 0,
                                MIce = 0,
                                MHot = 0,
                                S = 0,
                                S23 = 0,
                                S12 = 0,
                                S13 = 0,
                                S14 = 0,
                                SLittle = 0,
                                NoS = 0
                            }).ToList();

            foreach (var item in retailSub)
            {
                //分品項
                //一筆中同大小、甜度可能會有兩杯以上
                //所以比對字串後以ProductCount來累加
                //int LIce = 0, LHot = 0, // 大冷 大熱 
                //    MIce = 0, MHot = 0, // 中冷 中熱
                //    S = 0, S23 = 0, // 正常甜 2/3糖
                //    S12 = 0, S13 = 0, // 1/2糖 1/3糖
                //    S14 = 0, SLittle = 0, // 1/4糖 甜一點
                //    NoS = 0 ; //無糖
                
                item.Options = item.ProductName + item.Options;
                foreach (var g in groupRetailSub)
                {
                    if (g.ProductName == item.ProductName)
                    {
                        //g.productName = item.ProductName;
                        g.StoreNo = item.StoreNo;
                        g.RetailDate = item.RetailDate.ToString();
                            
                        //袋子                            
                        if (item.Options.Contains("PCS"))
                        {
                            g.ProductCount += item.ProductCount;
                        }
                        else if (item.Options.Contains("M"))  //大小 冷熱
                        {
                            if (item.Options.Contains("熱") || item.Options.Contains("溫"))
                            {
                                g.MHot += item.ProductCount;
                                g.ProductCount += item.ProductCount;
                                g.S += item.ProductCount;
                            }
                            else
                            {                                    
                                g.MIce += item.ProductCount;
                                g.ProductCount += item.ProductCount;
                                g.S += item.ProductCount;
                            }
                        }
                        else
                        {
                            if (item.Options.Contains("熱") || item.Options.Contains("溫"))
                            {
                                g.LHot += item.ProductCount;
                                g.ProductCount += item.ProductCount;
                                g.S += item.ProductCount;
                            }
                            else
                            {
                                g.LIce += item.ProductCount;
                                g.ProductCount += item.ProductCount;
                                g.S += item.ProductCount;
                            }
                        }
                        
                        //甜度
                        if (item.Options.Contains("2/3") || item.Options.Contains("少糖"))
                        {
                            g.S23 += item.ProductCount;
                            g.S -= item.ProductCount;
                        }
                        else if (item.Options.Contains("1/2") || item.Options.Contains("半糖"))
                        {
                            g.S12 += item.ProductCount;
                            g.S -= item.ProductCount;
                        }
                        else if (item.Options.Contains("1/3") || item.Options.Contains(" 微糖"))
                        {
                            g.S13 += item.ProductCount;
                            g.S -= item.ProductCount;
                        }
                        else if (item.Options.Contains("1/4") || item.Options.Contains("微微糖"))
                        {
                            g.S14 += item.ProductCount;
                            g.S -= item.ProductCount;
                        }
                        else if (item.Options.Contains("甜一點"))
                        {
                            g.SLittle += item.ProductCount;
                            g.S -= item.ProductCount;
                        }
                        else if (item.Options.Contains("無糖"))
                        {
                            g.NoS += item.ProductCount;
                            g.S -= item.ProductCount;
                        }
                    }
                }
            }
            var groupList = groupRetailSub.OrderByDescending(r => r.ProductCount);

            SizeSugarViewModel arr = new SizeSugarViewModel();
            foreach(var list in groupList)
            {
                arr.ProductCount += list.ProductCount;
                arr.LIce += list.LIce;
                arr.LHot += list.LHot;               
                arr.MHot += list.MHot;
                arr.MIce += list.MIce;                
                arr.S += list.S;
                arr.S23 += list.S23;
                arr.S12 += list.S12;
                arr.S13 += list.S13;
                arr.S14 += list.S14;               
                arr.SLittle += list.SLittle;
                arr.NoS += list.NoS;       
            }
            ViewBag.Total = arr;

            return View(groupList);
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
