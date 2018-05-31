using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using QBitNinja.Client;


namespace NBitcoin.SPVSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static QBitNinjaClient client = new QBitNinjaClient(Network.TestNet);
        public MainWindow()
        {
            InitializeComponent();
            root.DataContext = new MainWindowViewModel();
        }
        
        public MainWindowViewModel ViewModel
        {
            get
            {
                return root.DataContext as MainWindowViewModel;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddWalletWindow win = new AddWalletWindow();
            win.ViewModel.Name = "Wallet" + ViewModel.Wallets.Count;
            var result = win.ShowDialog();
            if (result.HasValue && result.Value)
            {
                ViewModel.CreateWallet(win.ViewModel);
            }
        }
        private void Button_Click2(object sender, RoutedEventArgs e)
        {

            MainWindowViewModel mbwallet = (root.DataContext as MainWindowViewModel);


            Script lScript = mbwallet.SelectedWallet.CurrentAddress.ScriptPubKey;
            KeyPath lKeyPath = ViewModel.SelectedWallet.Wallet.GetKeyPath(lScript);
            ExtKey lPrivKey = mbwallet.SelectedWallet.PrivateKeys[0].ExtKey.Derive(lKeyPath);
            BitcoinAddress lMyAddress = mbwallet.SelectedWallet.CurrentAddress;

            if (CheckValues.CheckIfNull(this))
            {
                return;
            }

            if (!decimal.TryParse(Value.Text, out decimal lMoney))
            {
                MessageBox.Show("This can only be numbers", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            List<ICoin> coins = GetCoins(lMoney);

            decimal total = coins.Sum(x => Convert.ToDecimal(x.Amount.ToString()));
            if (total < Convert.ToDecimal(lMoney))
            {
                MessageBox.Show("Does not have enough funds\nFunds: " + total + "\nValue to Send: " + lMoney, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            BitcoinAddress lDestination = BitcoinAddress.Create(Address.Text, App.Network);
        
            TransactionBuilder txBuilder = new TransactionBuilder();

            Transaction lTx = txBuilder
                .AddCoins(coins)
                .AddKeys(lPrivKey)
                .Send(lDestination, new Money(lMoney, MoneyUnit.BTC))
                .SendFees("0.0001")
                .SetChange(lMyAddress)
                .BuildTransaction(true);

            broad(lTx);

            List<ICoin> GetCoins(decimal sendAmount)
            {
                var txInAmount = Money.Zero;
                var coins1 = new List<ICoin>();
                foreach (var balance in client.GetBalance(lMyAddress,
            true).Result.Operations)
                {
                    var transactionId = balance.TransactionId;
                    var transactionResponse =
            client.GetTransaction(transactionId).Result;
                    var receivedCoins = transactionResponse.ReceivedCoins;
                    foreach (Coin coin in receivedCoins)
                    {
                        if (coin.TxOut.ScriptPubKey ==
                            lMyAddress.ScriptPubKey)
                        {
                            coins1.Add(coin);
                            txInAmount += (coin.Amount as Money);
                        }
                    }
                }
                return coins1;
            }
        }
        public class CheckValues
        {
            public static bool CheckIfNull(MainWindow mw)
            {
                if (String.IsNullOrEmpty(mw.Address.Text) || String.IsNullOrEmpty(mw.Value.Text))
                {
                    MessageBox.Show("Both Address and Coins to Send need to have a value", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        public async static void broad(Transaction tx)
        {

            var response = await client.Broadcast(tx);

            if (response.Success)
                MessageBox.Show("Transaction Success", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show("Transaction Failed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);


        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel._Group != null)
            {
                ConnectedNodesWindow win = new ConnectedNodesWindow();
                win.ViewModel = ViewModel.CreateConnectedNodesViewModel();
                win.Show();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            ViewModel.Dispose();
        }

        private void Hyperlink_Click_1(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedWallet != null && ViewModel.SelectedWallet.CurrentAddress != null)
            {
                Clipboard.SetText(ViewModel.SelectedWallet.CurrentAddress.ToString());
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedWallet != null)
            {
                ViewModel.SelectedWallet.Wallet.GetNextScriptPubKey();
                ViewModel.SelectedWallet.Update();
            }
        }
    }
}
