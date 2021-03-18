using MarketSite.App_GlobalResources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MarketSite.Models
{
    //public class StoreDayViewModel
    //{
    //    public IEnumerable<StoreDayRetail> RetailItem { get; set; }
    //    public IEnumerable<Retail> RetailList { get; set; }
    //}
    public class StoreDayViewModel
    {
        public IEnumerable<StoreDayInfo> AllRetailItem { get; set; }
        public StoreDayInfo RetailItem { get; set; }
        public IEnumerable<StoreDayList> RetailList { get; set; }
        public IEnumerable<StoreDayCarry> RetailCarry { get; set; }
    }

    public class StoreDayInfo
    {
        public string StoreNo { get; set; }
        [Display(Name = "TotalCount", ResourceType = typeof(Resource))]
        public int OrderCount { get; set; }
        [Display(Name = "DayCount", ResourceType = typeof(Resource))]
        public int DayCount { get; set; }
        [Display(Name = "NightCount", ResourceType = typeof(Resource))]
        public int NightCount { get; set; }
        [Display(Name = "TotalRetail", ResourceType = typeof(Resource))]
        public decimal TotalRetail { get; set; }
        [Display(Name = "NonTaxTotal", ResourceType = typeof(Resource))]
        public string NonTaxTotal { get; set; }
        public DateTime RetailDate { get; set; }
        public string StoreName { get; set; }
        [Display(Name = "Currency", ResourceType = typeof(Resource))]
        public string CurrencyID { get; set; }
    }
    public class StoreDayList
    {
        public string StoreNo { get; internal set; }
        [Display(Name = "OrderNum", ResourceType = typeof(Resource))]
        public string OrderNo { get; internal set; }
        [Display(Name = "Subtotal", ResourceType = typeof(Resource))]
        public decimal SubPrice { get; internal set; }
        [Display(Name = "ServiceFee", ResourceType = typeof(Resource))]
        public decimal ServiceCharge { get; internal set; }
        [Display(Name = "Total", ResourceType = typeof(Resource))]
        public decimal TotalPrice { get; internal set; }
        [Display(Name = "OrderTime", ResourceType = typeof(Resource))]
        public DateTime OrderDateTime { get; internal set; }
        public DateTime? RetailDate { get; internal set; }
        [Display(Name = "Orderer", ResourceType = typeof(Resource))]
        public string CreateBy { get; internal set; }
        [Display(Name = "Currency", ResourceType = typeof(Resource))]
        public string CurrencyID { get; internal set; }
        [Display(Name = "CarryType", ResourceType = typeof(Resource))]
        public string OrderType { get; internal set; }
    }

    public class StoreDayCarry
    {
        public string CarryType { get; set; }
        public int CarryNum { get; set; }
        public decimal CarryMoney { get; set; }
    }



    public class StoreYearViewModel
    {
        public StoreYearInfo StoreYear { get; set; }
        public Store12Month StoreMonths {get; set;}
       
    }
    public class StoreYearInfo
    {
        public string StoreId { get; set; }
        [Display(Name = "Month", ResourceType = typeof(Resource))]
        public string Month { get; set; }
        [Display(Name = "Revenue", ResourceType = typeof(Resource))]
        public decimal SubTotal { get; set; }

        public string CurrencyName { get; set; }
        public decimal SubTotalRate { get; set; }

    }
    public class Store12Month
    {
        public string StoreId { get; set; }
        public string StoreName { get; set; }
        
        //public string JAN { get; set; }
        //public string FEB { get; set; }
        //public string MAR { get; set; }
        //public string APR { get; set; }
        //public string MAY { get; set; }
        //public string JUN { get; set; }
        //public string JUL { get; set; }
        //public string AUG { get; set; }
        //public string SEP { get; set; }
        //public string OCT { get; set; }
        //public string NOV { get; set; }
        //public string DEC { get; set; }

        public string SubTotal1 { get; set; }
        public string SubTotal2 { get; set; }
        public string SubTotal3 { get; set; }
        public string SubTotal4 { get; set; }
        public string SubTotal5 { get; set; }
        public string SubTotal6 { get; set; }
        public string SubTotal7 { get; set; }
        public string SubTotal8 { get; set; }
        public string SubTotal9 { get; set; }
        public string SubTotal10 { get; set; }
        public string SubTotal11 { get; set; }
        public string SubTotal12 { get; set; }
        public string YearTotal { get; set; }
        public string CurrencyName { get; set; }
        public decimal SubTotalRate { get; set; }
        public string URL { get; set; }

    }


    public class StoreProductViewModel
    {
        [Display(Name = "Ranking", ResourceType = typeof(Resource))]
        public int Order { get; set; }
        [Display(Name = "Items", ResourceType = typeof(Resource))]
        public string ProductName { get; set; }
        [Display(Name = "Quantity", ResourceType = typeof(Resource))]
        public int ProductCount { get; set; }
        [Display(Name = "Total", ResourceType = typeof(Resource))]
        public decimal TotalPrice { get; set; }
        [Display(Name = "Currency", ResourceType = typeof(Resource))]
        public string CurrencyName { get; set; }
    }

    public class CustomerPriceViewModel
    {
        [Display(Name = "Date", ResourceType = typeof(Resource))]
        public string ShowDate { get; set; }
        [Display(Name = "Revenue", ResourceType = typeof(Resource))]
        public decimal Total { get; set; }
        [Display(Name = "Visitors", ResourceType = typeof(Resource))]
        public int Customer { get; set; }
        [Display(Name = "SinglePrice", ResourceType = typeof(Resource))]
        public decimal Price { get; set; }
        [Display(Name = "Currency", ResourceType = typeof(Resource))]
        public string CurrencyName { get; set; }

    }

    public class SizeSugarViewModel
    {
        public string StoreNo { get; set; }
        public string RetailDate { get; set; }
        [Display(Name = "Items", ResourceType = typeof(Resource))]
        public string ProductName { get; set; }
        [Display(Name = "TotalCups", ResourceType = typeof(Resource))]
        public int ProductCount { get; set; }
        [Display(Name = "LargeCold", ResourceType = typeof(Resource))]
        public int LIce { get; set; } // 大冷 
        [Display(Name = "LargeHot", ResourceType = typeof(Resource))]
        public int LHot { get; set; } // 大熱 
        [Display(Name = "MediumCold", ResourceType = typeof(Resource))]
        public int MIce { get; set; } // 中冷
        [Display(Name = "MediumHot", ResourceType = typeof(Resource))]
        public int MHot { get; set; } // 中熱
        [Display(Name = "NormalSugar", ResourceType = typeof(Resource))]
        public int S { get; set; } // 正常甜
        [Display(Name = "Sugar23", ResourceType = typeof(Resource))]
        public int S23 { get; set; } // 2/3糖
        [Display(Name = "Sugar12", ResourceType = typeof(Resource))]
        public int S12 { get; set; } // 1/2糖
        [Display(Name = "Sugar13", ResourceType = typeof(Resource))]
        public int S13 { get; set; } // 1/3糖
        [Display(Name = "Sugar14", ResourceType = typeof(Resource))]
        public int S14 { get; set; } // 1/4糖
        [Display(Name = "Microsugar", ResourceType = typeof(Resource))]
        public int SLittle { get; set; } // 甜一點
        [Display(Name = "SugarFree", ResourceType = typeof(Resource))]
        public int NoS { get; set; } //無糖
    }

    public partial class StoreDayRetail
    {
        public string ShortDate { get; set; } 
        public string NoTaxTotal { get; set; }
    }

    public class RankingListViewModel
    {
        public IEnumerable<Ranking> RankingList { get; set; }        
    }
    public class Ranking
    {   
        public int RankingNum { get; set; } 
        public string StoreNo { get; set; }
        public string StoreName { get; set; }
        [Display(Name = "TotalCount", ResourceType = typeof(Resource))]
        public int OrderCount { get; set; }
        [Display(Name = "DayCount", ResourceType = typeof(Resource))]
        public decimal TotalRetail { get; set; }
        public DateTime RetailDate { get; set; }
        [Display(Name = "Currency", ResourceType = typeof(Resource))]
        public string CurrencyID { get; set; }
        public string StoreCom { get; set; }
        public int ProdCount { get; set; }
        public string ProdName { get; set; }
    }
    
}