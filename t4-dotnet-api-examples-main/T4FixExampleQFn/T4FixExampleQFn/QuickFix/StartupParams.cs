using QuickFix.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T4FixExampleQFn.QuickFix
{

    public enum CommandType
    {
        Login,
        LoginQuiet,
        ExchangeList,
        ContractList,
        MarketList,
        MarketSingle,
        AccountList,
        NewOrderSingle,
        MarketDataFull,
        MarketDataIncremental
    };


    internal class StartupParams
    {

        public string Firm { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public bool UseStunnel { get; set; }
        public string Account { get; set; }
        public string ExchangeID { get; set; }  //  T4 Defined ExchangeID - Example: CME_Eq
        public string ContractID { get; set; }  //  T4 Defined ContractID - Example: ES
        public string MarketID { get; set; }  //  T4 Defined MarketID - XCME_Eq ES(U24)
        public string QuickFixInitiatorConfigFilePath { get; set; }
        public CommandType Command { get; set; }

        public StartupParams(string firm, string user, string password, string account, string exchangeID, string contractID, string marketID, CommandType command)
        {

            // For simplicity hardcode values here and just hit enter at first prompt.
            Firm = firm;
            User = user;
            Password = password;
            Account = account;
            ExchangeID = exchangeID;
            ContractID = contractID;
            MarketID = marketID;
            Command = command;

        }

        public StartupParams()
        {

            AppSettings settings = SettingsStore.Load();

            Console.Write("Use sTunnel: ");
            settings.UseStunnel = ReadLineWithEditing(settings.UseStunnel).ToUpper();
            UseStunnel = settings.UseStunnel == "Y";

            Console.WriteLine("Enter your firm:");
            settings.Firm = ReadLineWithEditing(settings.Firm);
            Firm = settings.Firm;

            Console.WriteLine("Enter your user:");
            settings.User = ReadLineWithEditing(settings.User);
            User = settings.User;   

            Console.WriteLine("Enter your password:");
            Password = ReadPassword();

            // Save inputs for next session.
            SettingsStore.Save(settings);

            // Set the path to the quickfix initiator file.
            QuickFixInitiatorConfigFilePath = Path.Combine(AppContext.BaseDirectory,"QuickFIX","initiator.cfg");

            Console.WriteLine("Enter one of the following commands:");
            Console.WriteLine("L - Recommended Logon");
            Console.WriteLine("Q - Quiet Logon - Disables Auto: Portfolio Refresh, Account Subscription, Account and Position Collateral Reports");
            Console.WriteLine("E - Post Logon Security Definition Request - Exchange List");
            Console.WriteLine("C - Post Logon Security Definition Request - Contract List");
            Console.WriteLine("M - Post Logon Security Definition Request - Market List");
            Console.WriteLine("M - Post Logon Security Definition Request - Single Market");
            Console.WriteLine("A - Collateral Inquiry - Account List");
            Console.WriteLine("S - Order Execution - New Order Single - Simplified Example Supports Futures Only");
            Console.WriteLine("F - Market Data Request - Simplified Example Supports Futures Only");
            Console.WriteLine("I - Market Data Request - Incremental - Simplified Example Supports Futures Only");

            // Read the command or comment out and hardcode.
            var command = Console.ReadLine() ?? "";


            switch (command)
            {

                case "C": // Contract List

                    Console.WriteLine("Enter ExchangeID:");
                    ExchangeID = Console.ReadLine() ?? "";

                    Command = CommandType.ContractList;

                    break;

                case "M": // Markets

                    Console.WriteLine("Enter ExchangeID:");
                    ExchangeID = Console.ReadLine() ?? "";

                    Console.WriteLine("Enter ContractID:");
                    ContractID = Console.ReadLine() ?? "";

                    Console.WriteLine("Enter MarketID for a specific market or blank for a market list:");
                    MarketID = Console.ReadLine() ?? "";

                    if (MarketID == "")
                    { Command = CommandType.MarketList; }
                    else
                    { Command = CommandType.MarketSingle; }


                    break;


                case "S": // Order Execution - New Order Single

                    Console.WriteLine("Enter Account:");
                    Account = Console.ReadLine() ?? "";

                    Console.WriteLine("Enter ExchangeID:");
                    ExchangeID = Console.ReadLine() ?? "";

                    Console.WriteLine("Enter ContractID:");
                    ContractID = Console.ReadLine() ?? "";

                    Console.WriteLine("Enter MarketID:");
                    MarketID = Console.ReadLine() ?? "";

                    Command = CommandType.NewOrderSingle;

                    break;

                case "F": // Market Data Request - Stream Specific Market

                    Console.WriteLine("Enter ExchangeID:");
                    ExchangeID = Console.ReadLine() ?? "";

                    Console.WriteLine("Enter ContractID:");
                    ContractID = Console.ReadLine() ?? "";

                    Console.WriteLine("Enter MarketID:");
                    MarketID = Console.ReadLine() ?? "";

                    Command = CommandType.MarketDataFull;

                    break;

                case "I": // Market Data Request - Incremental - Stream Specific Market

                    Console.WriteLine("Enter ExchangeID:");
                    ExchangeID = Console.ReadLine() ?? "";

                    Console.WriteLine("Enter ContractID:");
                    ContractID = Console.ReadLine() ?? "";

                    Console.WriteLine("Enter MarketID:");
                    MarketID = Console.ReadLine() ?? "";

                    Command = CommandType.MarketDataIncremental;

                    break;

                case "L":

                    Command = CommandType.Login;

                    break;

                case "Q":

                    Command = CommandType.LoginQuiet;

                    break;

                case "A":

                    Command = CommandType.AccountList;

                    break;

                default:
                    break;
            }

        }

        static string ReadLineWithEditing(string defaultValue)
        {
            // Show the default in the console
            Console.Write(defaultValue);

            var buffer = new StringBuilder(defaultValue);
            int cursor = buffer.Length;

            while (true)
            {
                var key = Console.ReadKey(intercept: true);

                // ENTER → finish
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return buffer.ToString();
                }

                // BACKSPACE
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (cursor > 0)
                    {
                        // Update buffer
                        buffer.Remove(cursor - 1, 1);
                        cursor--;

                        // Erase from console
                        Console.Write("\b \b");
                    }

                    continue;
                }

                // Ignore other control keys for now (arrows, etc.)
                if (char.IsControl(key.KeyChar))
                    continue;

                // Normal character: append to end
                buffer.Insert(cursor, key.KeyChar);
                cursor++;

                Console.Write(key.KeyChar);
            }
        }

        // Read the password while masking the characters.
        static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo keyInfo;

            do
            {
                keyInfo = Console.ReadKey(true); // intercept keypress
                if (keyInfo.Key != ConsoleKey.Backspace && keyInfo.Key != ConsoleKey.Enter)
                {
                    password += keyInfo.KeyChar;
                    Console.Write("*"); // print placeholder character
                }
                else if (keyInfo.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Remove(password.Length - 1); // remove last character
                    Console.Write("\b \b"); // erase previous character
                }
            }
            while (keyInfo.Key != ConsoleKey.Enter);

            Console.WriteLine(); // move to next line
            return password;
        }

    }
}
