using System;
using System.Collections.Generic;
using System.Text;
using M4.Common.Classes;
using M4.IBroker;

namespace M4.Demo {
	public static class Formatter {
		private static readonly Dictionary<string, string> AccountInfoFormation = new Dictionary<string, string> {
			["Currency"] = "货币单位",
			["Balance"] = "新结存",
			["RealizedPL"] = "实际盈/亏",
			["Profit"] = "浮动盈/亏",
			["Equity"] = "资产净值",
			["UsedMargin"] = "按金需求",
			["UsableMargin"] = "有效按金",
			["UsableMargin2"] = "可用按金"
		};

		private static readonly Dictionary<string, string> QuoteFormation = new Dictionary<string, string> {
			["DisplayName"] = "合约显示名称",
			["ContractName"] = "合约名称",
			["Symbol"] = "合约软件内部符号",
			["AskPrice"] = "卖出价格",
			["BidPrice"] = "买进价格",
			["Time"] = "报价时间"
		};

		private static readonly Dictionary<string, string> OrderFormation = new Dictionary<string, string> {
			["ContractName"] = "合约名称",
			["Symbol"] = "合约软件内部符号"
		};

		private static readonly Dictionary<string, string> PositionFormation = new Dictionary<string, string> {
			["ServerPositionRef"] = "编号",
			["ContractName"] = "合约",
			["BuySell"] = "卖出/买进",
			["Lot"] = "手数",
			["Amount"] = "合约单位",
			["OpenPrice"] = "开仓价",
			["Profit"] = "浮动盈/亏",
			["CreationTime"] = "交易日期"
		};

		public static string Format(AccountInfo accountInfo) {
			return Format(accountInfo, AccountInfoFormation);
		}

		public static string Format(Quote quote) {
			return Format(quote, QuoteFormation);
		}

		public static string Format(Order order) {
			return Format(order, OrderFormation);
		}

		public static string Format(Position position) {
			return Format(position, PositionFormation);
		}

		private static string Format<T>(T value, Dictionary<string, string> formation) {
			var sb1 = new StringBuilder();
			var sb2 = new StringBuilder();
			foreach (var property in typeof(T).GetProperties()) {
				string name = property.Name;
				if (formation.TryGetValue(name, out string newName))
					name = newName;
				(name != property.Name ? sb1 : sb2).AppendLine($"{name}: {property.GetValue(value, null)}");
			}
			sb1.Append(sb2.ToString());
			sb1.Remove(sb1.Length - Environment.NewLine.Length, Environment.NewLine.Length);
			return sb1.ToString();
		}
	}
}
