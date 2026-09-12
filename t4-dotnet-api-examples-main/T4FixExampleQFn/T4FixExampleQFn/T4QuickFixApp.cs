using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickFix;
using QuickFix.Fields;
using QuickFix.Transport;
using System.Security;
using System.Numerics;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using T4FixExampleQFn.QuickFix;




internal class T4QuickFixApp : IApplication
{

    bool mLoggedIn = false;
    StartupParams mStartupParams;

    static ulong seqno = 1;
    Session session = null;
    SessionID sessionID = null;

    public T4QuickFixApp(StartupParams startupParams)
    {

        this.mStartupParams = startupParams;

    }

    public void Start()
    {

        // Be sure we have received a successfull login response.
        while (mLoggedIn == false)
        {
            Thread.Sleep(1000);
        };

        // Delay a few seconds to have processed all messages that are part of initialization.
        Thread.Sleep(3000);


        switch (mStartupParams.Command )
        {
            case CommandType.ExchangeList: // Exchange List

                seqno += 1;

                QuickFix.FIX42.SecurityDefinitionRequest msgE = new QuickFix.FIX42.SecurityDefinitionRequest();

                msgE.Header.SetField(new MsgSeqNum(seqno));

                msgE.Header.SetField(new SenderCompID("T4Example"));
                msgE.Header.SetField(new TargetCompID(mStartupParams.Firm));
                msgE.SetField(new StringField(320, Guid.NewGuid().ToString()));
                msgE.SetField(new SecurityRequestType(3));
                msgE.SetField(new SecurityType("FUT"));

                Session.SendToTarget(msgE, sessionID);

                break;

            case CommandType.ContractList : // Contract List

                    Console.WriteLine("***");
                    Console.WriteLine($"SecurityDefinitionRequest SecurityExchange={mStartupParams.ExchangeID}");
                    Console.WriteLine("***");

                    seqno += 1;

                    QuickFix.FIX42.SecurityDefinitionRequest msgC = new QuickFix.FIX42.SecurityDefinitionRequest();

                    msgC.Header.SetField(new MsgSeqNum(seqno));

                    msgC.Header.SetField(new SenderCompID("T4Example"));
                    msgC.Header.SetField(new TargetCompID(mStartupParams.Firm));
                    msgC.SetField(new StringField(320, Guid.NewGuid().ToString()));
                    msgC.SetField(new SecurityRequestType(3));
                    msgC.SetField(new SecurityType("FUT"));
                    msgC.SetField(new SecurityExchange(mStartupParams.ExchangeID));

                    Session.SendToTarget(msgC, sessionID);


                break;

            case CommandType.MarketList : // Market List

                Console.WriteLine("***");
                Console.WriteLine($"SecurityDefinitionRequest SecurityExchange={mStartupParams.ExchangeID}  Symbol={mStartupParams.ContractID}");
                Console.WriteLine("***");

                seqno += 1;

                QuickFix.FIX42.SecurityDefinitionRequest msgML = new QuickFix.FIX42.SecurityDefinitionRequest();

                msgML.Header.SetField(new MsgSeqNum(seqno));

                msgML.Header.SetField(new SenderCompID("T4Example"));
                msgML.Header.SetField(new TargetCompID(mStartupParams.Firm));
                msgML.SetField(new StringField(320, Guid.NewGuid().ToString()));
                msgML.SetField(new SecurityRequestType(3));
                msgML.SetField(new SecurityType("FUT"));
                msgML.SetField(new SecurityExchange(mStartupParams.ExchangeID));
                msgML.SetField(new Symbol(mStartupParams.ContractID));

                Session.SendToTarget(msgML, sessionID);

                break;

            case CommandType.MarketSingle:  // Market Single

                Console.WriteLine("***");
                Console.WriteLine($"SecurityDefinitionRequest SecurityExchange={mStartupParams.ExchangeID}  Symbol={mStartupParams.ContractID}  SecurityID={mStartupParams.MarketID}");
                Console.WriteLine("***");

                seqno += 1;

                QuickFix.FIX42.SecurityDefinitionRequest msgMS = new QuickFix.FIX42.SecurityDefinitionRequest();

                msgMS.Header.SetField(new MsgSeqNum(seqno));
                msgMS.Header.SetField(new SenderCompID("T4Example"));
                msgMS.Header.SetField(new TargetCompID(mStartupParams.Firm));
                msgMS.SetField(new StringField(320, Guid.NewGuid().ToString()));
                msgMS.SetField(new SecurityRequestType(3));
                msgMS.SetField(new SecurityType("FUT"));
                msgMS.SetField(new SecurityExchange(mStartupParams.ExchangeID));
                msgMS.SetField(new Symbol(mStartupParams.ContractID));
                msgMS.SetField(new SecurityID(mStartupParams.MarketID));

                Session.SendToTarget(msgMS, sessionID);

                break;

            case CommandType.AccountList:  // Collateral Inquiry - Account List

                Console.WriteLine("***");
                Console.WriteLine($"CollateralInquiry - Account List");
                Console.WriteLine("***");

                seqno += 1;

                QuickFix.FIX44.CollateralInquiry msgA = new QuickFix.FIX44.CollateralInquiry();

                msgA.Header.SetField(new MsgSeqNum(seqno));
                msgA.Header.SetField(new SenderCompID("T4Example"));
                msgA.Header.SetField(new TargetCompID(mStartupParams.Firm));

                msgA.SetField(new CollInquiryID(Guid.NewGuid().ToString()));
                msgA.SetField(new SubscriptionRequestType('0'));
                msgA.SetField(new ResponseTransportType('0'));
                msgA.SetField(new CollInquiryQualifier(0));

                msgA.Header.SetField(new BodyLength(msgA.ToString().Length));
                msgA.Header.SetField(new BeginString("FIX.4.2"));

                Session.SendToTarget(msgA, sessionID);

                break;

                case CommandType.NewOrderSingle : // Order Execution - New Order Single

                Console.WriteLine("***");
                Console.WriteLine($"Order Execution - New Order Single");
                Console.WriteLine("***");

                seqno += 1;

                QuickFix.FIX42.NewOrderSingle msgS = new QuickFix.FIX42.NewOrderSingle();

                msgS.Header.SetField(new MsgSeqNum(seqno));
                msgS.Header.SetField(new SenderCompID("T4Example"));
                msgS.Header.SetField(new TargetCompID(mStartupParams.Firm));

                msgS.SetField(new Account(mStartupParams.Account));
                msgS.SetField(new ClOrdID(DateTime.Now.Ticks.ToString()));

                msgS.SetField(new SecurityExchange(mStartupParams.ExchangeID));
                msgS.SetField(new Symbol(mStartupParams.ContractID));
                msgS.SetField(new SecurityID(mStartupParams.MarketID));

                // Submit a buy market.
                msgS.SetField(new Side('1'));
                msgS.SetField(new OrderQty(1));
                msgS.SetField(new OrdType('1'));
                msgS.SetField(new SecurityType("FUT"));

                msgS.SetField(new TimeInForce('0'));
                msgS.SetField(new TransactTime(DateTime.UtcNow));
                msgS.SetField(new HandlInst('3'));

                msgS.Header.SetField(new BodyLength(msgS.ToString().Length));
                msgS.Header.SetField(new BeginString("FIX.4.2"));

                Session.SendToTarget(msgS, sessionID);

                break;

            case CommandType.MarketDataFull: // Market Data Request - Full Snapshot - Stream Specific Market 

                Console.WriteLine("***");
                Console.WriteLine($"Market Data Request - Stream Specific Market");
                Console.WriteLine("***");

                seqno += 1;

                QuickFix.FIX42.MarketDataRequest msgD = new QuickFix.FIX42.MarketDataRequest();

                msgD.Header.SetField(new MsgSeqNum(seqno));
                msgD.Header.SetField(new SenderCompID("T4Example"));
                msgD.Header.SetField(new TargetCompID(mStartupParams.Firm));

                msgD.SetField(new MDReqID(Guid.NewGuid().ToString()));
                msgD.SetField(new SubscriptionRequestType('1'));
                msgD.SetField(new MarketDepth(10));
                msgD.SetField(new MDUpdateType(5));

                var groupD = new QuickFix.FIX42.MarketDataRequest.NoMDEntryTypesGroup();

                groupD.MDEntryType = new MDEntryType('0');
                msgD.AddGroup(groupD);
                groupD.MDEntryType = new MDEntryType('1');
                msgD.AddGroup(groupD);
                groupD.MDEntryType = new MDEntryType('2');
                msgD.AddGroup(groupD);
                groupD.MDEntryType = new MDEntryType('3');
                msgD.AddGroup(groupD);

                var groupD2 = new QuickFix.FIX42.MarketDataRequest.NoRelatedSymGroup();

                groupD2.SecurityExchange = new SecurityExchange(mStartupParams.ExchangeID);
                groupD2.Symbol = new Symbol(mStartupParams.ContractID);
                groupD2.SecurityID = new SecurityID(mStartupParams.MarketID);
                groupD2.SecurityType = new SecurityType("FUT");

                msgD.AddGroup(groupD2);


                msgD.Header.SetField(new BodyLength(msgD.ToString().Length));
                msgD.Header.SetField(new BeginString("FIX.4.2"));

                Session.SendToTarget(msgD, sessionID);

                break;

            case CommandType.MarketDataIncremental : // Market Data Request - Incremental - Stream Specific Market

                Console.WriteLine("***");
                Console.WriteLine($"Market Data Request - Stream Specific Market");
                Console.WriteLine("***");

                seqno += 1;

                QuickFix.FIX42.MarketDataRequest msgDI = new QuickFix.FIX42.MarketDataRequest();

                msgDI.Header.SetField(new MsgSeqNum(seqno));
                msgDI.Header.SetField(new SenderCompID("T4Example"));
                msgDI.Header.SetField(new TargetCompID(mStartupParams.Firm));

                msgDI.SetField(new MDReqID(Guid.NewGuid().ToString()));
                msgDI.SetField(new SubscriptionRequestType('7'));
                msgDI.SetField(new MarketDepth(10));
                msgDI.SetField(new MDUpdateType(5));

                var groupDI = new QuickFix.FIX42.MarketDataRequest.NoMDEntryTypesGroup();

                groupDI.MDEntryType = new MDEntryType('0');
                msgDI.AddGroup(groupDI);
                groupDI.MDEntryType = new MDEntryType('1');
                msgDI.AddGroup(groupDI);
                groupDI.MDEntryType = new MDEntryType('2');
                msgDI.AddGroup(groupDI);
                groupDI.MDEntryType = new MDEntryType('3');
                msgDI.AddGroup(groupDI);

                var groupDI2 = new QuickFix.FIX42.MarketDataRequest.NoRelatedSymGroup();

                groupDI2.SecurityExchange = new SecurityExchange(mStartupParams.ExchangeID);
                groupDI2.Symbol = new Symbol(mStartupParams.ContractID);
                groupDI2.SecurityID = new SecurityID(mStartupParams.MarketID);
                groupDI2.SecurityType = new SecurityType("FUT");

                msgDI.AddGroup(groupDI2);


                msgDI.Header.SetField(new BodyLength(msgDI.ToString().Length));
                msgDI.Header.SetField(new BeginString("FIX.4.2"));

                Session.SendToTarget(msgDI, sessionID);

                break;

            default:
                break;
        }


    }

    public void Stop()
    {

        seqno += 1;

        QuickFix.FIX42.Logout msg = new QuickFix.FIX42.Logout();

        msg.Header.SetField(new MsgSeqNum(seqno));

        msg.Header.SetField(new SenderCompID("T4Example"));
        msg.Header.SetField(new TargetCompID(mStartupParams.Firm));
        msg.Header.SetField(new Text("Logging Out"));

        session.Send(msg);

    }


    private void MsgLogonUpdate(Message msg)
    {

        if (msg.Header.GetString(35) == "A")
        {

            msg.Header.SetField(new MsgType("A"));

            msg.Header.SetField(new SenderCompID("T4Example"));
            msg.Header.SetField(new TargetCompID(mStartupParams.Firm));
            msg.Header.SetField(new SecureData("112A04B0-5AAF-42F4-994E-FA7CB959C60B"));
            msg.Header.SetField(new SecureDataLen(36));
            msg.Header.SetField(new SendingTime(DateTime.UtcNow, true));
            msg.Header.SetField(new MsgSeqNum(1));


            msg.SetField(new EncryptMethod(0));
            msg.SetField(new HeartBtInt(30));
            msg.SetField(new Username(mStartupParams.User));
            msg.SetField(new Password(mStartupParams.Password));
            msg.SetField(new ResetSeqNumFlag(true));

            // 	D = Enables Decimal pricing in order entry, market and chart data. This is required to use the 8 decimal place 2yr Note prices from January 2019 onwards.
            var groupD = new QuickFix.FIX42.Logon.NoMsgTypesGroup();
            groupD.RefMsgType = new RefMsgType("D");
            msg.AddGroup(groupD);

            // 	c = Enables security definitions requests.
            var groupc = new QuickFix.FIX42.Logon.NoMsgTypesGroup();
            groupc.RefMsgType = new RefMsgType("c");
            msg.AddGroup(groupc);


            // 	BG = Enables dynamic account notifications. Position and Account collateral reports will be generated automatically.
            var groupBG = new QuickFix.FIX42.Logon.NoMsgTypesGroup();
            groupBG.RefMsgType = new RefMsgType("BG");
            msg.AddGroup(groupBG);

            msg.Header.SetField(new BodyLength(msg.ToString().Length));
            msg.Header.SetField(new BeginString("FIX.4.2"));

            msg.RemoveField(141);

        }

    }


    private void MsgLogonUpdateQuiet(Message msg)
    {

        if (msg.Header.GetString(35) == "A")
        {

            msg.Header.SetField(new MsgType("A"));

            msg.Header.SetField(new SenderCompID("T4Example"));
            msg.Header.SetField(new TargetCompID(mStartupParams.Firm));
            msg.Header.SetField(new SecureData("112A04B0-5AAF-42F4-994E-FA7CB959C60B"));
            msg.Header.SetField(new SecureDataLen(36));
            msg.Header.SetField(new SendingTime(DateTime.UtcNow, true));
            msg.Header.SetField(new MsgSeqNum(1));


            msg.SetField(new EncryptMethod(0));
            msg.SetField(new HeartBtInt(30));
            msg.SetField(new Username(mStartupParams.User));
            msg.SetField(new Password(mStartupParams.Password));
            //msg.SetField(new ResetSeqNumFlag(true));

            // 	D = Enables Decimal pricing in order entry, market and chart data. This is required to use the 8 decimal place 2yr Note prices from January 2019 onwards.
            var groupD = new QuickFix.FIX42.Logon.NoMsgTypesGroup();
            groupD.RefMsgType = new RefMsgType("D");
            msg.AddGroup(groupD);

            // 	d = Disables listing of portfolio orders, positions and account details (AutoPortfolio Refresh) - upon successful login.
            var groupd = new QuickFix.FIX42.Logon.NoMsgTypesGroup();
            groupd.RefMsgType = new RefMsgType("d");
            msg.AddGroup(groupd);

            // BB = Disables automatic subscription to all accounts of the logon user.
            var groupBB = new QuickFix.FIX42.Logon.NoMsgTypesGroup();
            groupd.RefMsgType = new RefMsgType("BB");
            msg.AddGroup(groupBB);

            msg.Header.SetField(new BodyLength(msg.ToString().Length));
            msg.Header.SetField(new BeginString("FIX.4.2"));

            msg.RemoveField(141);

        }

    }


    public void OnCreate(SessionID sessionID)
    {
        Console.WriteLine("Message OnCreate  SessionID: " + sessionID.ToString());

        this.session = Session.LookupSession(sessionID);
        this.sessionID = sessionID;
        this.session.Reset("Sequence numbers reset on new session");
    }
    public void OnLogout(SessionID sessionID)
    {
        Console.WriteLine("Log out Session  SessionID: " + sessionID.ToString());
    }
    public void OnLogon(SessionID sessionID)
    {
        Console.WriteLine("OnLogon  SessionID: " + sessionID.ToString());
    }

    public void FromApp(Message msg, SessionID sessionID)
    {
        Console.WriteLine("FromApp: " + msg.GetType().ToString().Replace("QuickFix.FIX42.", "") + " - " + msg.ToString());

        switch (msg.Header.GetString(Tags.MsgType))
        {
            case "BA":

                switch (msg.GetString(Tags.QtyType))
                {

                    case "1":

                        Console.WriteLine("***");
                        Console.WriteLine($"CollateralReport - AccountList:  Account={msg.GetString(Tags.Account)}");
                        Console.WriteLine("***");

                        break;

                    case "2":

                        Console.WriteLine("***");
                        Console.WriteLine($"CollateralReport - AccountDetails:  Account={msg.GetString(Tags.Account)}  Firm={msg.GetString(3111)}");
                        Console.WriteLine("***");

                        break;

                    case "3":

                        Console.WriteLine("***");
                        Console.WriteLine($"CollateralReport - AccountUpdate:  Account={msg.GetString(Tags.Account)}  StartCash={msg.GetString(Tags.StartCash)}");
                        Console.WriteLine("***");

                        break;

                    case "4":

                        Console.WriteLine("***");
                        Console.WriteLine($"CollateralReport - AccountPositionUpdate:  Account={msg.GetString(Tags.Account)}  SecurityID={msg.GetString(Tags.SecurityID)}  Quantity={msg.GetString(Tags.Quantity)}");
                        Console.WriteLine("***");


                        break;

                }

                //  DisplayMessageFormattedJson(msg);

                break;

            case "W":

                ProcessMarketDataFullRefresh(msg);

                break;

            case "X":

                ProcessMarketDataIncremental(msg);

                break;


        }

    }

    private SortedList<int, PriceVolume> Bids = new SortedList<int, PriceVolume>();
    private SortedList<int, PriceVolume> Offers = new SortedList<int, PriceVolume>();

    private void ProcessMarketDataFullRefresh(Message msg)
    {

        NoMDEntries noMDEntries = new NoMDEntries();

        msg.GetField(noMDEntries);

        int groupCount = noMDEntries.getValue();

        QuickFix.FIX42.MarketDataSnapshotFullRefresh.NoMDEntriesGroup mdGroup = new QuickFix.FIX42.MarketDataSnapshotFullRefresh.NoMDEntriesGroup();

        for (int i = 1; i <= groupCount; i++)
        {
            msg.GetGroup(i, mdGroup);

            //SecurityID securityID = new SecurityID();
            MDEntryPx mdEntryPx = new MDEntryPx();
            MDEntryType mdEntryType = new MDEntryType();
            MDEntrySize mdEntrySize = new MDEntrySize();
            MDPriceLevel mdPriceLevel = new MDPriceLevel();

            if (mdGroup.IsSetField(mdEntryType) && mdGroup.IsSetField(mdEntrySize) && mdGroup.IsSetField(mdPriceLevel) && mdGroup.IsSetField(mdEntryPx))
            {
                mdGroup.GetField(mdEntryType);
                mdGroup.GetField(mdEntrySize);
                mdGroup.GetField(mdPriceLevel);
                mdGroup.GetField(mdEntryPx);


                // Add or Update
                var oPV = new PriceVolume { Price = mdEntryPx.getValue(), Volume = mdEntrySize.getValue() };

                if (mdEntryType.getValue() == '0')
                {
                    Bids[mdPriceLevel.getValue()] = oPV;
                }
                else if (mdEntryType.getValue() == '1')
                {
                    Offers[mdPriceLevel.getValue()] = oPV;
                }


                // Console.WriteLine($"UpdateAction:{mdUpdateAction.getValue ()}  EntryType:{mdEntryType}  EntrySize:{mdEntrySize}  PriceLevel:{mdPriceLevel}  EntryPx:{mdEntryPx}");
            }

        }


        int level = Offers.Count;

        foreach (var p in Offers.Values)
        {

            Console.WriteLine($"Offers - Level: {level}  Price:{p.Price}  Volume:{p.Volume}");
            level -= 1;
        }

        level = 0;

        foreach (var p in Bids.Values)
        {
            level += 1;
            Console.WriteLine($"Bids - Level: {level}  Price:{p.Price}  Volume:{p.Volume}");

        }




        Console.WriteLine("");


    }

    private void ProcessMarketDataIncremental(Message msg)
    {

        NoMDEntries noMDEntries = new NoMDEntries();

        msg.GetField(noMDEntries);

        int groupCount = noMDEntries.getValue();

        QuickFix.FIX42.MarketDataIncrementalRefresh.NoMDEntriesGroup mdGroup = new QuickFix.FIX42.MarketDataIncrementalRefresh.NoMDEntriesGroup();

        for (int i = 1; i <= groupCount; i++)
        {
            msg.GetGroup(i, mdGroup);

            //SecurityID securityID = new SecurityID();
            MDEntryPx mdEntryPx = new MDEntryPx();
            MDEntryType mdEntryType = new MDEntryType();
            MDEntrySize mdEntrySize = new MDEntrySize();
            MDPriceLevel mdPriceLevel = new MDPriceLevel();
            MDUpdateAction mdUpdateAction = new MDUpdateAction();

            if (mdGroup.IsSetField(mdUpdateAction) && mdGroup.IsSetField(mdEntryType) && mdGroup.IsSetField(mdPriceLevel)) // && mdGroup.IsSetField(mdEntryPx) && mdGroup.IsSetField(mdEntrySize))
            {
                mdGroup.GetField(mdUpdateAction);
                mdGroup.GetField(mdEntryType);
                mdGroup.GetField(mdPriceLevel);


                if (mdUpdateAction.getValue() == '0') // Add
                {
                    // Add would need to have price and volume.
                    mdGroup.GetField(mdEntryPx);
                    mdGroup.GetField(mdEntrySize);

                    // Add
                    var oPV = new PriceVolume { Price = mdEntryPx.getValue(), Volume = mdEntrySize.getValue(), Level = mdPriceLevel.getValue() };

                    if (mdEntryType.getValue() == '0') // Bid
                    {
                        Bids[oPV.Level] = oPV;
                    }
                    else if (mdEntryType.getValue() == '1') // Offer
                    {
                        Offers[oPV.Level] = oPV;
                    }
                }
                else if (mdUpdateAction.getValue() == '1') // Update
                {
                    // Update could either update price or volume.
                    // To be an update means we should already have had this item in order to update it.

                    PriceVolume oPV;

                    if (mdEntryType.getValue() == '0') // Bid
                    {
                        if (Bids.TryGetValue(mdPriceLevel.getValue(), out oPV))
                        {

                            if (mdGroup.IsSetField(mdEntryPx)) // Price
                            {
                                mdGroup.GetField(mdEntryPx);
                                oPV.Price = mdEntryPx.getValue();
                            }
                            if (mdGroup.IsSetField(mdEntrySize)) // Volume
                            {
                                mdGroup.GetField(mdEntrySize);
                                oPV.Volume = mdEntrySize.getValue();
                            }

                        }
                        else
                        {

                            // Ok..  Fine.. Try and Add.
                            // Oddly we don't have the item so create it.
                            // We would need price and volume.
                            if (mdGroup.IsSetField(mdEntryPx) && mdGroup.IsSetField(mdEntrySize))
                            {

                                oPV = new PriceVolume { Price = mdEntryPx.getValue(), Volume = mdEntrySize.getValue(), Level = mdPriceLevel.getValue() };

                                if (mdEntryType.getValue() == '0') // Bid
                                {
                                    Bids[oPV.Level] = oPV;
                                }

                            }

                        }

                    }
                    else if (mdEntryType.getValue() == '1') // Offer.
                    {

                        if (Offers.TryGetValue(mdPriceLevel.getValue(), out oPV))
                        {

                            if (mdGroup.IsSetField(mdEntryPx))
                            {
                                mdGroup.GetField(mdEntryPx); // Price
                                oPV.Price = mdEntryPx.getValue();
                            }
                            if (mdGroup.IsSetField(mdEntrySize))
                            {
                                mdGroup.GetField(mdEntrySize); // Volume
                                oPV.Volume = mdEntrySize.getValue();
                            }

                        }
                        else
                        {

                            // Ok..  Fine.. Try and Add.
                            // Oddly we don't have the item so create it.
                            // We would need price and volume.
                            if (mdGroup.IsSetField(mdEntryPx) && mdGroup.IsSetField(mdEntrySize))
                            {

                                oPV = new PriceVolume { Price = mdEntryPx.getValue(), Volume = mdEntrySize.getValue(), Level = mdPriceLevel.getValue() };

                                if (mdEntryType.getValue() == '1') // Offer
                                {
                                    Offers[oPV.Level] = oPV;
                                }

                            }

                        }
                    }

                }
                else if (mdUpdateAction.getValue() == 2) // Remove
                {

                    // Delete.

                    if (mdEntryType.getValue() == 0)
                    {
                        Bids.Remove(mdPriceLevel.getValue()); // Bid
                    }
                    else if (mdEntryType.getValue() == 1)
                    {
                        Offers.Remove(mdPriceLevel.getValue()); // Offer
                    }

                }


                // Console.WriteLine($"UpdateAction:{mdUpdateAction.getValue ()}  EntryType:{mdEntryType}  EntrySize:{mdEntrySize}  PriceLevel:{mdPriceLevel}  EntryPx:{mdEntryPx}");
            }

        }

        int level = Offers.Count;

        foreach (var p in Offers.Values.Reverse())
        {

            Console.WriteLine($"Offers - Level: {level}  Price:{p.Price}  Volume:{p.Volume}");
            level -= 1;
        }

        level = 0;

        foreach (var p in Bids.Values)
        {
            level += 1;
            Console.WriteLine($"Bids - Level: {level}  Price:{p.Price}  Volume:{p.Volume}");

        }

        Console.WriteLine("");


    }

    public void FromAdmin(Message msg, SessionID sessionID)
    {

        Console.WriteLine("FromAdmin: " + msg.GetType().ToString().Replace("QuickFix.FIX42.", "") + " - " + msg.ToString());

        switch (msg.Header.GetString(Tags.MsgType))
        {
            case "A":

                if (msg.GetString(Tags.DefaultCstmApplVerID) != "")
                {

                    mLoggedIn = true;

                    Console.WriteLine("***");
                    Console.WriteLine("Logged In");
                    Console.WriteLine("***");

                    // DisplayMessageFormattedJson(msg);

                }

                break;

            case "5":

                mLoggedIn = false;

                Console.WriteLine("***");
                Console.WriteLine("Logged Off");
                Console.WriteLine("***");

                //  DisplayMessageFormattedJson(msg);

                break;

            default:

                break;
        }

    }
    public void ToAdmin(Message msg, SessionID sessionID)
    {

        Console.WriteLine("ToAdmin: " + msg.GetType().ToString().Replace("QuickFix.FIX42.", "") + " - " + msg.ToString());

        // DisplayMessageFormattedJson(msg);

        if (mStartupParams.Command ==  CommandType.LoginQuiet )
        {
            // Intercept and modify the logon message prior to it being sent.
            MsgLogonUpdateQuiet(msg);
        }
        else
        {
            // Intercept and modify the logon message prior to it being sent.
            MsgLogonUpdate(msg);
        }

    }
    public void ToApp(Message msg, SessionID sessionID)
    {

        Console.WriteLine("ToApp: " + msg.GetType().ToString().Replace("QuickFix.FIX42.", "") + " - " + msg.ToString());

        //  DisplayMessageFormattedJson(msg);

    }

    private void DisplayMessageFormattedJson(Message msg)
    {

        // Deserialize the JSON string into a .NET object
        dynamic jsonObject = JsonConvert.DeserializeObject(msg.ToJSON());

        // Format the JSON for display
        string formattedJson = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);

        Console.WriteLine("");
        Console.WriteLine(formattedJson);
        Console.WriteLine("");
    }


}

