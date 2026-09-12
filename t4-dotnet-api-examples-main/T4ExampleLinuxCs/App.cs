using T4.API;
using T4;
using Microsoft.Extensions.Logging;

namespace T4ExampleLinuxCs
{
    public class App
    {
        private readonly ILogger<App> _log;

        //T4 Api Host instance
        Host moHost;

        //  Reference to the current account.
        Account moAccount = null;

        // Reference to the current exchange.
        Exchange moExchange = null;

        // Reference to the current contract.
        Contract moContract = null;

        // Reference to the current market.
        Market moPickerMarket = null;

        List<Market> moLoadedMarkets = null;

        // References to selected markets.
        Market moMarket1 = null;

        // References to marketid's retrieved from saved settings.
        string mstrMarketID1;


        public App(ILogger<App> log)
        {
            _log = log;
        }

        EventWaitHandle ewhAccountComplete = new EventWaitHandle(false, EventResetMode.AutoReset);
        EventWaitHandle ewhMarketsComplete = new EventWaitHandle(false, EventResetMode.AutoReset);
        public async Task RunAsync()
        {
            // Event that is raised when details for an account have 
            // changed, or a new account is recieved.
            void moAccounts_AccountDetails(AccountDetailsEventArgs e)
            {
                Console.WriteLine(e.Account.AccountID);
            }
            var logged = false;
            while (!logged)
            {
                Console.Clear();
                Console.WriteLine("Welcome to the T4 API console App example:");
                Console.WriteLine("Enter your Firm:");
                var firm = Console.ReadLine();
                Console.WriteLine("Enter your Username:");
                var username = Console.ReadLine();
                Console.WriteLine("Enter your Password:");
                var password = Console.ReadLine();
                try
                {
                    moHost = new Host(APIServerType.Simulator, "T4Example", "112A04B0-5AAF-42F4-994E-FA7CB959C60B", firm, username, password);
                    logged = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    _log.LogError($"Error creating host: {ex.ToString()}");
                    Console.ReadLine();
                }
            }
                                    
            moHost.Accounts.AccountDetails += new T4.API.AccountList.AccountDetailsEventHandler(moAccounts_AccountDetails);

            await Task.Delay(2000); //Give it time to load the messages

            while (true)
            {
                Console.WriteLine("\nMAIN MENU:\n");
                Console.WriteLine("Press 1 to print the accounts for the user");
                Console.WriteLine("Press 2 to print the exchange list for the user");
                Console.WriteLine("Press any other key to exit");

                var input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        DisplayAccountList();
                        break;
                    case "2":
                        DisplayExchangeList();
                        break;
                    default:
                        Environment.Exit(42);
                        return;
                }
            }
        }

        void DisplayAccountList()
        {
            try
            {
                Console.Clear();
                // Lock the API.
                moHost.EnterLock();

                // Display the account list.
                Console.WriteLine("ACCOUNT INFORMATION:\n");
                int i = 0;
                var accountIds = new List<string>();
                foreach (Account oAccount in moHost.Accounts)
                {
                    accountIds.Add(oAccount.AccountID);
                    Console.WriteLine($"Account Index: {i}");
                    Console.WriteLine($"Account Id: {oAccount.AccountID}");
                    Console.WriteLine($"Account Name: {oAccount.AccountNumber}");
                    //Console.WriteLine($"Available Cash: {String.Format("{0:#,###,##0.00}", oAccount.AvailableCash)}");
                    Console.WriteLine("\n");
                    i++;
                }

                Console.WriteLine("Enter the index of the acccount you want to select");
                var strAccountSelected = Console.ReadLine();
                var accountIndex = int.Parse(strAccountSelected);
                var selectedAccount = accountIds[accountIndex];
                DisplayAccountInformation(selectedAccount);
            }
            catch (Exception ex)
            {
                // Trace Errors.
                _log.LogError($"PrintAccountList(): {ex.ToString()}");
            }
            finally
            {
                // Unlock the api.
                moHost.ExitLock();
            }
        }

        void DisplayExchangeList()
        {
            try
            {
                Console.Clear();
                // Lock the API.
                moHost.EnterLock();
                Console.WriteLine("EXCHANGE LIST:");
                int i = 0;
                var exchangeIds = new List<string>();
                foreach (var exchange in moHost.MasterUser.Exchanges)
                {
                    exchangeIds.Add(exchange.Exchange.ExchangeID);
                    Console.Write($"{i}. ");
                    Console.Write($"{exchange.Exchange.Description}");
                    Console.WriteLine();
                    i++;
                }
                Console.WriteLine("Enter the index of the exchange you want to select");
                var strExchangeSelected = Console.ReadLine();
                var exchangeIndex = int.Parse(strExchangeSelected);
                var selectedExchange = exchangeIds[exchangeIndex];
                moExchange = moHost.MasterUser.Exchanges.SingleOrDefault(e => e.ExchangeID == selectedExchange).Exchange;
                DisplayContracts(selectedExchange);
            }
            catch (Exception ex)
            {
                // Trace Errors.
                _log.LogError($"PrintExchangeList(): {ex.ToString()}");
            }
            finally
            {
                // Unlock the api.
                moHost.ExitLock();
            }
        }

        void DisplayContracts(string selectedExchange)
        {
            Console.Clear();
            // Eliminate any previous references.
            moContract = null;
            moPickerMarket = null;

            if (moExchange != null)
            {
                Console.WriteLine("CONTRACT LIST:");
                try
                {
                    int i = 0;
                    var contractIds = new List<string>();
                    // Add the exchanges to the dropdown list.
                    foreach (Contract oContract in moExchange.Contracts)
                    {
                        contractIds.Add(oContract.ContractID);
                        Console.Write($"{i}. ");
                        Console.Write(oContract.Description);
                        Console.WriteLine();
                        i++;
                    }
                    Console.WriteLine("Enter the index of the contract you want to select");
                    var strContractSelected = Console.ReadLine();
                    var contractIndex = int.Parse(strContractSelected);
                    var selectedContract = contractIds[contractIndex];
                    moContract = moExchange.Contracts.SingleOrDefault(c => c.ContractID == selectedContract);
                    LoadMarkets(selectedContract);
                    ewhMarketsComplete.WaitOne();
                    DisplayMarkets();
                    DisplayPosition();
                }
                catch (Exception ex)
                {
                    // Trace the error.
                    _log.LogError("PrintContracts(): " + ex.ToString());
                }
            }
        }

        private void LoadMarkets(string selectedContract)
        {
            moContract.GetMarkets(0, StrategyType.None, e2 =>
            {
                moLoadedMarkets = e2.Markets.GetSortedList();
                ewhMarketsComplete.Set();
            });
        }

        private void DisplayMarkets()
        {
            Console.Clear();
            // Populate the list of markets available for the current contract.
            // Eliminate any previous references.
            moPickerMarket = null;

            try
            {
                int i = 0;
                var marketIds = new List<string>();
                // Add the markets to the dropdown list.
                foreach (Market oMarket in moLoadedMarkets)
                {
                    marketIds.Add(oMarket.MarketID);
                    Console.Write($"{i}. ");
                    Console.Write(oMarket.Description);
                    Console.WriteLine();
                    i++;
                }
                Console.WriteLine("Enter the index of the market you want to select");
                var strMarketSelected = Console.ReadLine();
                var marketIndex = int.Parse(strMarketSelected);
                var selectedMarket = marketIds[marketIndex];
                moPickerMarket = moLoadedMarkets.SingleOrDefault(m => m.MarketID == selectedMarket);

            }
            catch (Exception ex)
            {
                // Trace the error.
                _log.LogError("DisplayMarkets(); " + ex.ToString());
            }
        }

        private void DisplayAccountInformation(string selectedAccount)
        {
            moAccount = moHost.Accounts[selectedAccount];

            // Register the account's events.
            if (moAccount != null)
            {
                OnAccountDetails(new AccountDetailsEventArgs(moAccount));
                moAccount.AccountUpdate += new T4.API.Account.AccountUpdateEventHandler(moAccounts_AccountUpdate);

                Console.WriteLine("Loading Account Information ...");
                ewhAccountComplete.WaitOne();

                // Display the current account balance.
                DisplayAccount();     
            }
        }

        private void moAccount_OrderUpdate(OrderUpdateEventArgs e)
        {

        }

        private void DisplayOrders()
        {
            Console.WriteLine("ORDERS:\n");
            try
            {
                // Lock the api.
                moHost.EnterLock();

                // Itterate through the orders, newest is first.
                foreach (Order oOrder in moAccount.Orders.GetSortedList())
                {
                    Console.WriteLine($"{oOrder.Market.Description} {oOrder.BuySell} {oOrder.TotalFillVolume}/{oOrder.CurrentVolume} @ {oOrder.Market.PriceToDisplay(oOrder.CurrentLimitPrice)} {oOrder.Status.ToString()} {oOrder.StatusDetail} {oOrder.SubmitTime}");
                }
            }
            catch (Exception ex)
            {
                // Trace the error.
                _log.LogError("DisplayOrders(): " + ex.ToString());
            }
            finally
            {
                // Unlock the api.
                moHost.ExitLock();
            }
        }

        private void DisplayAccountPositions()
        {
            Console.WriteLine("\n\nPOSITIONS:\n");
            foreach(var position in moAccount.Positions)
            {
                Console.WriteLine($"{position.Market.Description}: // Net: {position.Net} // Buys: {position.Buys} // Sells: {position.Sells}");
            }
        }

        // Event that is raised when the accounts overall balance,
        // P&L or margin details have changed.
        void moAccounts_AccountUpdate(AccountUpdateEventArgs e)
        {

        }

        void DisplayAccounts()
        {
            try
            {
                // Lock the API.
                moHost.EnterLock();

                // Display the account list.
                foreach (Account oAccount in moHost.Accounts)
                {
                    OnAccountDetails(new AccountDetailsEventArgs(oAccount));
                }
            }
            catch (Exception ex)
            {
                // Trace Errors.
                _log.LogError($"DisplayAccounts(): {ex.ToString()}");
            }
            finally
            {
                // Unlock the api.
                moHost.ExitLock();
            }

        }

        void OnAccountDetails(AccountDetailsEventArgs e)
        {
            // Check to see if the account exists prior to adding/subscribing to it.
            if (e.Account.Subscribed != true)
            {
                // Subscribe to the account.
                e.Account.Subscribe(e2 =>
                {
                    moAccounts_AccountComplete(e2);
                });
            }
        }
        void moAccounts_AccountComplete(AccountCompleteEventArgs e)
        {
            ewhAccountComplete.Set();
        }

        void DisplayAccount()
        {
            Console.Clear();
            Console.WriteLine("ACCOUNT INFORMATION:\n");
            if ((moAccount != null))
            {
                try
                {
                    Console.WriteLine($"Available cash: {String.Format("{0:#,###,##0.00}", moAccount.AvailableCash)}");
                }
                catch (Exception ex)
                {
                    // Trace the error.
                    _log.LogError("DisplayAccount(): " + ex.ToString());

                }
                Console.WriteLine();
                DisplayOrders();
                DisplayAccountPositions();
            }
        }

        void DisplayPosition()
        {
            string strNet = "";
            string strBuys = "";
            string strSells = "";

            try
            {

                if ((moPickerMarket != null) && (moAccount != null))
                {

                    // Display positions for current account and market1.
                    // Reference the market's positions.
                    Position oPosition = moAccount.Positions[moPickerMarket.MarketID];

                    if (oPosition != null)
                    {
                        // Reference the net position.
                        strNet = oPosition.Net.ToString();
                        strBuys = oPosition.Buys.ToString();
                        strSells = oPosition.Sells.ToString();
                        Console.WriteLine($"Net: {strNet} Buys: {strBuys} Sells: {strSells}");
                    }
                }

            }
            catch (Exception ex)
            {
                // Trace the error.
                _log.LogError("DisplayPosition() " + ex.ToString());
            }
        }
    }
}
