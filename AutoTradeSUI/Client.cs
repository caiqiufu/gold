using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using M4.Common;
using M4.Common.Classes;
using M4.Common.Enums;
using M4.IBroker;
using M4.Interfaces;
using M4.Interfaces.UserCode;
using mFinance.Settings;

namespace uClient.Broker
{
	/// <summary>
	/// M4客户端 新版本 1.0.0.1356
	/// </summary>
	public class Client : M4.Client.Client
	{
		public Client() : base()
		{
			Init();
		}
		public override void Init()
		{
			Core = CreateCore();
			RegisterEvents();
			ConnectionStatus = ConnectionStatus.Disconnected;
		}
        /// <summary>
        /// 当前持有仓位列表
        /// </summary>
        public override IList<Position> OpenPositions => UpdatePositions(Broker.OpenPositionsNew);
        public static new ICore CreateCore()
		{
			var core = (ICore)new M4.Core.Core();
			core.Settings.CheckAndInitialiseDefaults();
			core.IsSaveSettingRequired = false;
			core.CurrentCluture = new CultureInfo(core.Settings.CurrentCulture);
			var versionInfo = FileVersionInfo.GetVersionInfo(Application.StartupPath + "\\SUI_mF4.exe");
			core.Settings.DisclaimerVersion = new Version(versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart, versionInfo.FilePrivatePart).ToString();
			core.Settings.FxServerClientVersion = AssemblyName.GetAssemblyName("FxServerClient.dll").Version;
			return core;
		}
	}
}
