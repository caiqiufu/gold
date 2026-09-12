using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    internal static class InitializationHelper
    {
        /*
         * internal delegate void Initiliaze<T>(T value, SqlDataReader reader);
        internal static T Create<T>(SqlDataReader reader, Initiliaze<T> initailize)
            where T : new()
        {
            T value = new T();
            while (reader.Read())
            {
                initailize(value, reader);
            }
            reader.NextResult();
            return value;
        }

        internal static T[] CreateArray<T>(SqlDataReader reader, Initiliaze<T> initailize)
            where T : new()
        {
            List<T> valueList = new List<T>();
            while (reader.Read())
            {
                T value = new T();
                initailize(value, reader);
                valueList.Add(value);
            }
            reader.NextResult();
            return valueList.ToArray();
        }

        internal static T GetItemValue<T>(this SqlDataReader reader, string name, T defaul)
        {
            return reader[name] == DBNull.Value ? defaul : (T)reader[name];
        }
    }
    public static class SettingsSetHelper
    {
        public static object DataTypeId { get; private set; }

        public static void Initialize(this SettingSet settingSet, SqlDataReader dr)
        {
            settingSet.TradeDay = InitializationHelper.Create<TradeDay>(dr, Initialize);
            settingSet.SystemParameter = InitializationHelper.Create<SystemParameter>(dr, Initialize);
            settingSet.QuotePolicies = InitializationHelper.CreateArray<QuotePolicy>(dr, Initialize);
            settingSet.QuotePolicyDetails = InitializationHelper.CreateArray<QuotePolicyDetail>(dr, Initialize);
            settingSet.InstrumentGroups = InitializationHelper.CreateArray<Manager.Common.ExchangeEntities.InstrumentGroup>(dr, Initialize);
            settingSet.Instruments = InitializationHelper.CreateArray<Instrument>(dr, Initialize);
            settingSet.OverridedQuotations = InitializationHelper.CreateArray<Manager.Common.ExchangeEntities.OverridedQuotation>(dr, Initialize);
            settingSet.TradingTimes = InitializationHelper.CreateArray<TradingTime>(dr, Initialize);
            settingSet.Currencys = InitializationHelper.CreateArray<Currency>(dr, Initialize);
            settingSet.CurrencyRates = InitializationHelper.CreateArray<CurrencyRate>(dr, Initialize);
            settingSet.DealingPolicyDetails = InitializationHelper.CreateArray<DealingPolicyDetail>(dr, Initialize);

        }

        public static void InitializePermisstionData(this PermisstionDataSet settingSet, SqlDataReader dr)
        {
            settingSet.UserGroups = InitializationHelper.CreateArray<UserGroup>(dr, Initialize);
            settingSet.PermisstionDatas = InitializationHelper.CreateArray<PermisstionData>(dr, Initialize);
        }

        public static void InitializeTradingData(this SettingSet settingSet, SqlDataReader dr)
        {
            settingSet.Customers = InitializationHelper.CreateArray<Customer>(dr, Initialize);
            settingSet.AccountGroups = InitializationHelper.CreateArray<AccountGroup>(dr, Initialize);
            settingSet.Accounts = InitializationHelper.CreateArray<Account>(dr, Initialize);
            settingSet.TradePolicies = InitializationHelper.CreateArray<TradePolicy>(dr, Initialize);
            settingSet.TradePolicyDetails = InitializationHelper.CreateArray<TradePolicyDetail>(dr, Initialize);
            settingSet.Orders = SettingsSetHelper.InitializeOrders(dr);
        }

        public static SettingSet GetExchangeDataChangeByAccountChange(SqlDataReader dr)
        {
            SettingSet settingSet = new SettingSet();
            settingSet.TradeDay = InitializationHelper.Create<TradeDay>(dr, Initialize);
            settingSet.Customers = InitializationHelper.CreateArray<Customer>(dr, Initialize);
            settingSet.AccountGroups = InitializationHelper.CreateArray<AccountGroup>(dr, Initialize);
            settingSet.Accounts = InitializationHelper.CreateArray<Account>(dr, Initialize);
            settingSet.QuotePolicyDetails = InitializationHelper.CreateArray<QuotePolicyDetail>(dr, Initialize);
            return settingSet;
        }

        public static SettingSet GetExchangeDataChangeByInstrumentChange(SqlDataReader dr)
        {
            SettingSet settingSet = new SettingSet();
            settingSet.TradeDay = InitializationHelper.Create<TradeDay>(dr, Initialize);
            settingSet.QuotePolicies = InitializationHelper.CreateArray<QuotePolicy>(dr, Initialize);
            settingSet.QuotePolicyDetails = InitializationHelper.CreateArray<QuotePolicyDetail>(dr, Initialize);
            settingSet.TradePolicies = InitializationHelper.CreateArray<TradePolicy>(dr, Initialize);
            settingSet.TradePolicyDetails = InitializationHelper.CreateArray<TradePolicyDetail>(dr, Initialize);
            settingSet.Instruments = InitializationHelper.CreateArray<Instrument>(dr, Initialize);
            settingSet.OverridedQuotations = InitializationHelper.CreateArray<Manager.Common.ExchangeEntities.OverridedQuotation>(dr, Initialize);
            return settingSet;
        }

        #region 权限数据初始化
        private static void Initialize(UserGroup userGroup, SqlDataReader dr)
        {
            userGroup.Id = (Guid)dr["Id"];
            userGroup.Code = (string)dr["Code"];
        }
        private static void Initialize(PermisstionData item, SqlDataReader dr)
        {
            item.Id = (Guid)dr["Id"];
            item.RoleId = (int)dr["RoleId"];
            item.PrincipalDataId = (string)dr["PrincipalDataId"];
            item.OperationId = (int)dr["OperationId"];
            item.Permisstion = (int)dr["Permisstion"];
            item.DataType = dr["DataType"] == DBNull.Value ? DataTypeId.Other : (DataTypeId)Enum.Parse(typeof(DataTypeId), dr["DataType"].ToString());
            item.DataFiledId = dr["DataFieldId"] == DBNull.Value ? string.Empty : (string)dr["DataFieldId"];
        }
        #endregion

        private static void Initialize(TradeDay tradeDay, SqlDataReader dr)
        {
            tradeDay.CurrentDay = (DateTime)dr["TradeDay"];
            tradeDay.BeginTime = (DateTime)dr["BeginTime"];
            tradeDay.EndTime = (DateTime)dr["EndTime"];
        }

        private static void Initialize(SystemParameter systemParameter, SqlDataReader dr)
        {
            systemParameter.IsCustomerVisibleToDealer = (bool)dr["IsCustomerVisibleToDealer"];
            systemParameter.MooMocAcceptDuration = (int)dr["MooMocAcceptDuration"];
            systemParameter.MooMocCancelDuration = (int)dr["MooMocCancelDuration"];
            systemParameter.QuotePolicyDetailID = (Guid)dr["QuotePolicyDetailID"];
            systemParameter.CanDealerViewAccountInfo = (bool)dr["CanDealerViewAccountInfo"];
            systemParameter.LotDecimal = (int)dr["LotDecimal"];
            systemParameter.DealerUsingAccountPermission = (bool)dr["DealerUsingAccountPermission"];
            systemParameter.HighBid = (bool)dr["HighBid"];
            systemParameter.LowBid = (bool)dr["LowBid"];
            systemParameter.EnableTrendSheetChart = (bool)dr["EnableTrendSheetChart"];
            systemParameter.AllowModifyOrderLot = (bool)dr["AllowModifyOrderLot"];
            systemParameter.EnquiryOutTime = (int)dr["EnquiryOutTime"];
        }

        private static void Initialize(Customer customer, SqlDataReader dr)
        {
            customer.Id = (Guid)dr["ID"];
            customer.Code = (string)dr["Code"];
            customer.PrivateQuotePolicyId = (Guid)dr["PrivateQuotePolicyID"];
            customer.PublicQuotePolicyId = (Guid)dr["PublicQuotePolicyID"];
            if (dr["DealingPolicyID"] != DBNull.Value)
            {
                customer.DealingPolicyId = (Guid)dr["DealingPolicyID"];
            }
        }

        private static void Initialize(AccountGroup accountGroup, SqlDataReader dr)
        {
            accountGroup.Id = (Guid)dr["GroupID"];
            accountGroup.Code = (string)dr["GroupCode"];
        }

        private static void Initialize(Account account, SqlDataReader dr)
        {
            account.Id = (Guid)dr["ID"];
            account.Code = (string)dr["Code"];
            account.Name = dr.GetItemValue<string>("Name", null);
            account.CustomerId = (Guid)dr["CustomerID"];
            account.TradePolicyId = (Guid)dr["TradePolicyID"];
            account.GroupId = (Guid)dr["GroupId"];
            account.GroupCode = (string)dr["GroupCode"];
            account.AccountType = (AccountType)dr["Type"];
            account.CurrencyId = (Guid)dr["CurrencyID"];
            account.IsMultiCurrency = (bool)dr["IsMultiCurrency"];
        }

        private static void Initialize(QuotePolicy quotePolicy, SqlDataReader dr)
        {
            quotePolicy.Id = (Guid)dr["ID"];
            quotePolicy.Code = (string)dr["Code"];
            quotePolicy.Description = dr.GetItemValue<string>("Description", null);
            quotePolicy.IsDefault = (bool)dr["IsDefault"];
        }

        private static void Initialize(QuotePolicyDetail quotePolicyDetail, SqlDataReader dr)
        {
            quotePolicyDetail.InstrumentId = (Guid)dr["InstrumentID"];
            quotePolicyDetail.QuotePolicyId = (Guid)dr["QuotePolicyID"];
            quotePolicyDetail.PriceType = (Manager.Common.QuotationEntities.PriceType)(byte)dr["PriceType"];
            quotePolicyDetail.AutoAdjustPoints = (int)dr["AutoAdjustPoints"];
            quotePolicyDetail.AutoAdjustPoints2 = (string)dr["AutoAdjustPoints2"];
            quotePolicyDetail.AutoAdjustPoints3 = (string)dr["AutoAdjustPoints3"];
            quotePolicyDetail.AutoAdjustPoints4 = (string)dr["AutoAdjustPoints4"];
            quotePolicyDetail.SpreadPoints = (int)dr["SpreadPoints"];
            quotePolicyDetail.SpreadPoints2 = (string)dr["SpreadPoints2"];
            quotePolicyDetail.SpreadPoints3 = (string)dr["SpreadPoints3"];
            quotePolicyDetail.SpreadPoints4 = (string)dr["SpreadPoints4"];
            quotePolicyDetail.IsOriginHiLo = (bool)dr["IsOriginHiLo"];
            quotePolicyDetail.MaxAutoAdjustPoints = (int)dr["MaxAutoAdjustPoints"];
            quotePolicyDetail.MaxSpreadPoints = (int)dr["MaxSpreadPoints"];
            quotePolicyDetail.InstrumentId = (Guid)dr["InstrumentID"];
            quotePolicyDetail.BuyLot = (decimal)dr["BuyLot"];
            quotePolicyDetail.SellLot = (decimal)dr["SellLot"];
        }

        private static void Initialize(TradePolicy tradePolicy, SqlDataReader dr)
        {
            tradePolicy.Id = (Guid)dr["ID"];
            tradePolicy.Code = (string)dr["Code"];
            tradePolicy.Description = dr.GetItemValue<string>("Description", null);

            if (dr["AlertLevel1"] != null)
            {
                tradePolicy.AlertLevel1 = dr.GetItemValue<decimal>("AlertLevel1", 0);
            }
            if (dr["AlertLevel2"] != null)
            {
                tradePolicy.AlertLevel2 = dr.GetItemValue<decimal>("AlertLevel2", 0);
            }
            if (dr["AlertLevel3"] != null)
            {
                tradePolicy.AlertLevel3 = dr.GetItemValue<decimal>("AlertLevel3", 0);
            }
        }

        private static void Initialize(TradePolicyDetail tradePolicyDetail, SqlDataReader dr)
        {
            tradePolicyDetail.InstrumentId = (Guid)dr["InstrumentID"];
            tradePolicyDetail.TradePolicyId = (Guid)dr["TradePolicyID"];
            tradePolicyDetail.ContractSize = (decimal)dr["ContractSize"];
            tradePolicyDetail.QuotationMask = (int)dr["QuotationMask"];
            if (dr["BOPolicyID"] != DBNull.Value) tradePolicyDetail.BOPolicyID = (Guid)dr["BOPolicyID"];
        }
        private static void Initialize(Manager.Common.ExchangeEntities.InstrumentGroup instrumentGroup, SqlDataReader dr)
        {
            instrumentGroup.Id = (Guid)dr["GroupID"];
            instrumentGroup.Code = (string)dr["GroupCode"];
        }
        private static void Initialize(Instrument instrument, SqlDataReader dr)
        {
            instrument.Id = (Guid)dr["ID"];
            instrument.OriginCode = (string)dr["OriginCode"];
            instrument.Code = (string)dr["Code"];
            instrument.Category = (InstrumentCategory)(int)dr["Category"];
            instrument.IsActive = (bool)dr["IsActive"];
            instrument.BeginTime = (DateTime)dr["BeginTime"];
            instrument.EndTime = (DateTime)dr["EndTime"];
            instrument.Denominator = (int)dr["Denominator"];
            instrument.NumeratorUnit = (int)dr["NumeratorUnit"];
            instrument.IsSinglePrice = (bool)dr["IsSinglePrice"];
            instrument.IsNormal = (bool)dr["IsNormal"];
            instrument.OriginType = (OriginType)(byte)dr["OriginType"];
            instrument.AllowedSpotTradeOrderSides = (byte)dr["AllowedSpotTradeOrderSides"];
            instrument.OriginInactiveTime = (int)dr["OriginInactiveTime"];
            instrument.AlertVariation = (int)dr["AlertVariation"];
            instrument.NormalWaitTime = (int)dr["NormalWaitTime"];
            instrument.AlertWaitTime = (int)dr["AlertWaitTime"];
            instrument.MaxDQLot = dr.GetItemValue<decimal>("MaxDQLot", 0);
            instrument.MaxOtherLot = dr.GetItemValue<decimal>("MaxOtherLot", 0);
            instrument.DqQuoteMinLot = dr.GetItemValue<decimal>("DQQuoteMinLot", 0);
            instrument.AutoDQMaxLot = dr.GetItemValue<decimal>("AutoDQMaxLot", 0);
            instrument.AutoLmtMktMaxLot = dr.GetItemValue<decimal>("AutoLmtMktMaxLot", 0);
            instrument.AcceptDQVariation = (int)dr["AcceptDQVariation"];
            instrument.AcceptLmtVariation = (int)dr["AcceptLmtVariation"];
            instrument.AcceptCloseLmtVariation = (int)dr["AcceptCloseLmtVariation"];
            instrument.CancelLmtVariation = (int)dr["CancelLmtVariation"];
            instrument.MaxMinAdjust = (int)dr["MaxMinAdjust"];
            instrument.IsBetterPrice = (bool)dr["IsBetterPrice"];
            instrument.HitTimes = (short)dr["HitTimes"];
            instrument.PenetrationPoint = (int)dr["PenetrationPoint"];
            instrument.PriceValidTime = (int)dr["PriceValidTime"];
            instrument.DailyMaxMove = (int)dr["DailyMaxMove"];
            instrument.LastAcceptTimeSpan = TimeSpan.FromMinutes((int)dr["LastAcceptTimeSpan"]);
            instrument.OrderTypeMask = (int)dr["OrderTypeMask"];
            instrument.PreviousClosePrice = dr.GetItemValue<string>("Close", string.Empty);
            instrument.AutoCancelMaxLot = dr.GetItemValue<decimal>("AutoCancelMaxLot", 0);
            instrument.AutoAcceptMaxLot = dr.GetItemValue<decimal>("AutoAcceptMaxLot", 0);
            instrument.AllowedNewTradeSides = Convert.ToInt16(dr["AllowedNewTradeSides"]);
            instrument.Mit = (bool)dr["Mit"];
            instrument.IsAutoEnablePrice = (bool)dr["IsAutoEnablePrice"];
            instrument.IsAutoFill = (bool)dr["IsAutoFill"];
            instrument.IsPriceEnabled = (bool)dr["IsPriceEnabled"];
            instrument.IsAutoEnablePrice = (bool)dr["IsAutoEnablePrice"];
            instrument.NextDayOpenTime = dr.GetItemValue<DateTime?>("NextDayOpenTime", null);
            instrument.MOCTime = dr.GetItemValue<DateTime?>("MOCTime", null);
            if (dr["DayOpenTime"] != DBNull.Value)
            {
                instrument.DayOpenTime = DateTime.Parse(dr["DayOpenTime"].ToString());
            }
            if (dr["DayCloseTime"] != DBNull.Value)
            {
                instrument.DayCloseTime = DateTime.Parse(dr["DayCloseTime"].ToString());
            }
            instrument.AutoDQDelay = (short)dr["AutoDQDelay"];
            instrument.SummaryGroupId = dr.GetItemValue<Guid?>("SummaryGroupId", null);
            instrument.SummaryGroupCode = dr.GetItemValue<string>("SummaryGroupCode", null);
            instrument.SummaryUnit = (decimal)((double)dr["SummaryUnit"]);
            instrument.SummaryQuantity = (decimal)dr["SummaryQuantity"];
            instrument.BuyLot = (decimal)dr["BuyLot"];
            instrument.SellLot = (decimal)dr["SellLot"];
            instrument.HitPriceVariationForSTP = (int)dr["HitPriceVariationForSTP"];
            instrument.CurrencyId = (Guid)dr["CurrencyID"];
            instrument.GroupCode = (string)dr["GroupCode"];
            instrument.GroupId = (Guid)dr["GroupID"];
        }

        public static Order[] InitializeOrders(SqlDataReader reader)
        {
            List<Order> orders = new List<Order>();
            while (reader.Read())
            {
                Order order = new Order();
                order.EndTime = ((DateTime)reader["EndTime"]).ToString("yyyy-MM-dd HH:mm:ss");
                double totalSeconds = ((DateTime)reader["EndTime"] - DateTime.Now).TotalSeconds;
                if (totalSeconds > 0)
                {
                    order.OrderValidDuration = totalSeconds >= int.MaxValue ? int.MaxValue : (int)totalSeconds;
                }
                //else
                //{
                //    continue;
                //}

                order.Id = (Guid)reader["ID"];
                order.Code = (string)reader["Code"];
                order.Phase = (OrderPhase)(byte)reader["Phase"];
                order.TransactionId = (Guid)reader["TransactionId"];
                order.TransactionCode = (string)reader["TransactionCode"];
                order.TransactionType = (TransactionType)(byte)reader["TransactionType"];
                order.TransactionSubType = (TransactionSubType)(byte)reader["TransactionSubType"];
                order.TradeOption = (TradeOption)(byte)reader["TradeOption"];
                order.OrderType = (OrderType)(int)reader["OrderTypeID"];
                order.IsOpen = (bool)reader["IsOpen"];
                order.IsBuy = (bool)reader["IsBuy"];
                order.AccountId = (Guid)reader["AccountID"];
                order.InstrumentId = (Guid)reader["InstrumentID"];
                order.ContractSize = (decimal)reader["ContractSize"];
                order.SetPrice = reader.GetItemValue<string>("SetPrice", string.Empty);
                order.ExecutePrice = reader.GetItemValue<string>("ExecutePrice", string.Empty);
                order.Lot = (decimal)reader["Lot"];
                order.LotBalance = (decimal)reader["LotBalance"];
                order.MinLot = reader.GetItemValue<decimal>("MinLot", decimal.Zero);
                order.MaxShow = reader.GetItemValue<string>("MaxShow", string.Empty);
                order.BeginTime = ((DateTime)reader["BeginTime"]).ToString("yyyy-MM-dd HH:mm:ss");
                order.SubmitTime = ((DateTime)reader["SubmitTime"]).ToString("yyyy-MM-dd HH:mm:ss");
                if (reader["ExecuteTime"] != DBNull.Value)
                {
                    order.ExecuteTime = ((DateTime)reader["ExecuteTime"]).ToString("yyyy-MM-dd HH:mm:ss");
                }
                order.HitCount = (short)reader["HitCount"];
                order.BestPrice = reader.GetItemValue<string>("BestPrice", string.Empty);
                if (reader["BestTime"] != DBNull.Value) order.BestTime = ((DateTime)reader["BestTime"]).ToString("yyyy-MM-dd HH:mm:ss");
                order.ApproverID = reader.GetItemValue<Guid>("ApproverID", Guid.Empty);
                order.SubmitorID = (Guid)reader["SubmitorID"];
                order.DQMaxMove = (int)reader["DQMaxMove"];
                order.ExpireType = (ExpireType)(int)reader["ExpireType"];
                order.SetPrice2 = reader.GetItemValue<string>("SetPrice2", string.Empty);
                order.AssigningOrderID = reader.GetItemValue<Guid>("AssigningOrderID", Guid.Empty);
                order.BlotterCode = reader.GetItemValue<string>("BlotterCode", string.Empty);
                order.SetPriceForDoneLimit = reader.GetItemValue<string>("SetPriceForDoneLimit", string.Empty);
                order.SetPriceForDoneStop = reader.GetItemValue<string>("SetPriceForDoneStop", string.Empty);
                if (reader["BOBetTypeID"] != DBNull.Value) order.BOBetTypeID = (Guid)reader["BOBetTypeID"];
                if (reader["BOFrequency"] != DBNull.Value) order.BOFrequency = (int)reader["BOFrequency"];
                if (reader["BOOdds"] != DBNull.Value) order.BOOdds = (decimal)reader["BOOdds"];
                if (reader["BOBetOption"] != DBNull.Value) order.BOBetOption = (long)reader["BOBetOption"];

                orders.Add(order);
            }
            return orders.ToArray();
        }

        private static void Initialize(Manager.Common.ExchangeEntities.OverridedQuotation overridedQuotations, SqlDataReader dr)
        {
            overridedQuotations.InstrumentId = (Guid)dr["InstrumentID"];
            overridedQuotations.QuotePolicyId = (Guid)dr["QuotePolicyID"];
            overridedQuotations.Timestamp = (DateTime)dr["Timestamp"];
            overridedQuotations.Ask = dr["Ask"].ToString();
            overridedQuotations.Bid = dr["Bid"].ToString();
            overridedQuotations.High = dr["High"].ToString();
            overridedQuotations.Low = dr["Low"].ToString();
            overridedQuotations.Origin = dr["Origin"].ToString();
        }

        private static void Initialize(TradingTime tradingTime, SqlDataReader dr)
        {
            tradingTime.InstrumentId = (Guid)dr["InstrumentID"];
            tradingTime.BeginTime = (DateTime)dr["BeginTime"];
            tradingTime.EndTime = (DateTime)dr["EndTime"];
        }

        private static void Initialize(Currency currency, SqlDataReader dr)
        {
            currency.Id = (Guid)dr["ID"];
            currency.Code = dr["Code"].ToString();
        }
        private static void Initialize(CurrencyRate currencyRate, SqlDataReader dr)
        {
            currencyRate.SourceCurrencyId = (Guid)dr["SourceCurrencyId"];
            currencyRate.TargetCurrencyId = (Guid)dr["TargetCurrencyId"];
            currencyRate.RateIn = (double)dr["RateIn"];
            currencyRate.RateOut = (double)dr["RateOut"];
            currencyRate.DependingInstrumentId = dr["DependingInstrumentId"] is DBNull ? Guid.Empty : (Guid)dr["DependingInstrumentId"];
            currencyRate.Inverted = (bool)dr["Inverted"];
        }

        private static void Initialize(DealingPolicyDetail dealingPolicyDetail, SqlDataReader dr)
        {
            dealingPolicyDetail.DealingPolicyID = (Guid)dr["DealingPolicyID"];
            dealingPolicyDetail.InstrumentID = (Guid)dr["InstrumentID"];
            dealingPolicyDetail.DealingPolicyCode = (string)dr["DealingPolicyCode"];
            dealingPolicyDetail.InstrumentCode = (string)dr["InstrumentCode"];
            dealingPolicyDetail.MaxDQLot = (decimal)dr["MaxDQLot"];
            dealingPolicyDetail.MaxOtherLot = (decimal)dr["MaxOtherLot"];
            dealingPolicyDetail.DQQuoteMinLot = (decimal)dr["DQQuoteMinLot"];
            dealingPolicyDetail.AutoDQMaxLot = (decimal)dr["AutoDQMaxLot"];
            dealingPolicyDetail.AutoLmtMktMaxLot = (decimal)dr["AutoLmtMktMaxLot"];
            dealingPolicyDetail.AcceptDQVariation = (int)dr["AcceptDQVariation"];
            dealingPolicyDetail.AcceptLmtVariation = (int)dr["AcceptLmtVariation"];
            dealingPolicyDetail.CancelLmtVariation = (int)dr["CancelLmtVariation"];
            dealingPolicyDetail.AutoDQDelay = (short)dr["AutoDQDelay"];
            dealingPolicyDetail.AutoAcceptMaxLot = (decimal)dr["AutoAcceptMaxLot"];
            dealingPolicyDetail.AutoCancelMaxLot = (decimal)dr["AutoCancelMaxLot"];
            dealingPolicyDetail.HitPriceVariationForSTP = (int)dr["HitPriceVariationForSTP"];
            dealingPolicyDetail.AcceptCloseLmtVariation = (int)dr["AcceptCloseLmtVariation"];

        }
         */

    }
}
