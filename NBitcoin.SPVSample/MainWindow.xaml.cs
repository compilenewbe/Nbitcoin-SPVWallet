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
using QBitNinja.Client.Models;
using NBitcoin.SPV;
using NBitcoin.Crypto;
using NBitcoin;
using NBitcoin.SPVSample.Properties;
using NBitcoin.Protocol;
using System.Collections;
using System.Diagnostics;

namespace NBitcoin.SPVSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
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

            //var privKey = mbwallet.SelectedWallet.PrivateKeys.FirstOrDefault(); // <== Revisa que aqui te devuelva            
            //var privkey = mbwallet.SelectedWallet.PrivateKeys[0].PrivateKey.GetBitcoinSecret(App.Network);
            //var myaddress = privkey.PubKey.GetAddress(App.Network);
            BitcoinAddress destination = BitcoinAddress.Create("n1WkSm1uQB8ELxRDzQNKbstMJSHh5U3ixX", Network.TestNet);
            //BitcoinPubKeyAddress destination = new BitcoinPubKeyAddress("msJr3Gxdm9SBQ38ECDrgMBnFqTLoNqWLfa", Network.TestNet);
            BitcoinAddress myaddress = BitcoinAddress.Create("msJr3Gxdm9SBQ38ECDrgMBnFqTLoNqWLfa", Network.TestNet);
            BitcoinSecret privkey = new BitcoinSecret ("cTNf7qB8cnQA5JuuV4ejKi739sGZkcHpdLX3XzRPqkGfxZsfMGxU", Network.TestNet);

            decimal value = Convert.ToDecimal(Value.Text);

            List<ICoin> coins = GetCoins(value);

            TransactionBuilder txBuilder = new TransactionBuilder();

            Transaction tx = txBuilder
                .AddCoins(coins)
                .AddKeys(privkey)
                .Send(destination, new Money(value, MoneyUnit.BTC))
                .SendFees("0.0001")
                .SetChange(myaddress)
                .BuildTransaction(true);

                var hello = tx.ToHex();

            if (txBuilder.Verify(tx) == false)
                MessageBox.Show("No funciono");

            Debug.WriteLine
                (tx.ToHex());

            //.BuildTransaction(true);99
            //Console.WriteLine(txBuilder.Verify(tx));
            var txRepo = new NoSqlTransactionRepository();
            txRepo.Put(tx.GetHash(), tx);
            //Assert(txBuilder.Verify(tx)); //check fully signed


            List<ICoin> GetCoins(decimal sendAmount)
            {
                //var mbwallet = (root.DataContext as MainWindowViewModel);
                var amountMoney = new Money(sendAmount, MoneyUnit.BTC);
                var client = new QBitNinjaClient(App.Network);
                var txInAmount = Money.Zero;
                var coins1 = new List<ICoin>();
                foreach (var balance in client.GetBalance(myaddress,//MBWallet.Wallet.Address,
            true).Result.Operations)
                {
                    var transactionId = balance.TransactionId;
                    var transactionResponse =
            client.GetTransaction(transactionId).Result;
                    var receivedCoins = transactionResponse.ReceivedCoins;
                    foreach (Coin coin in receivedCoins)
                    {
                        if (coin.TxOut.ScriptPubKey ==
                            myaddress.ScriptPubKey)//mbwallet.SelectedWallet.CurrentAddress.ScriptPubKey)//MBWallet.Wallet.BitcoinPrivateKey.ScriptPubKey) // this may not be necessary
                        {
                            coins1.Add(coin);
                            txInAmount += (coin.Amount as Money);
                        }
                    }
                }
                return coins1;
            }
            //BroadcastResponse broadcastResponse = client.Broadcast(tx).Result;
            //0100000001de8402147356b475d1ccf96df5f06769b31250ecdec56a379da010973cef3af90000000048473044022006cfe3911866796672ca97c03eec373f9f05e3f3bd565839e5e8db9c8131eea702204639e6fe3ea269bb5b4f93c7fd1894884241881da41543fbed768fbe3b2a331f01ffffffff02c005d901000000002321028a447a5218514d460f5b50d32438b29b4f6f632cf0bdec3b88fd1bca2e40b9eaac002d31010000000017a914a9974100aeee974a20cda9a2f545704a0ab54fdc8700000000
            //0100000001fb2b5248de40206f9d371a517a0f0406013848874701dabeee8fc4a8b531aec3000000006a47304402202209a4f973908be45329d80a2f7fc7b8473701bcd5231ee707674bba06e0629902207ed737b81fb4c0862ddd069b24b0a93f9e06194dfedc210ac2cbd7b7ae3f2c350121028a447a5218514d460f5b50d32438b29b4f6f632cf0bdec3b88fd1bca2e40b9eaffffffff0240420f00000000001976a914a0de39ffcac8405d2c50a2a94a37226fbeb9db4a88ac80f0fa020000000017a914a9974100aeee974a20cda9a2f545704a0ab54fdc8700000000
            //"0100000001fb2b5248de40206f9d371a517a0f0406013848874701dabeee8fc4a8b531aec3000000006b483045022100a374340d1d51a4a8bcc01c28f9b7b6740702dbeaf538d34888b771558431c317022039c097783f707244bc0237199f09114ab27db56654fcde8c308fb84960932f260121028a447a5218514d460f5b50d32438b29b4f6f632cf0bdec3b88fd1bca2e40b9eaffffffff0200093d00000000001976a914a0de39ffcac8405d2c50a2a94a37226fbeb9db4a88ac80f0fa020000000017a914a9974100aeee974a20cda9a2f545704a0ab54fdc8700000000"
            //"0100000001fb2b5248de40206f9d371a517a0f0406013848874701dabeee8fc4a8b531aec3000000006b48304502210083aade394dd5c627e34c0b4d991c909210cddff46664c5323383d5b0b6e1dda002202e200ddca3f1cbf06daf34f42ba3b767e6cf57aee39fd3d72d98337d82bacd600121028a447a5218514d460f5b50d32438b29b4f6f632cf0bdec3b88fd1bca2e40b9eaffffffff0200366e01000000001976a914a0de39ffcac8405d2c50a2a94a37226fbeb9db4a88ac80c3c9010000000017a914a9974100aeee974a20cda9a2f545704a0ab54fdc8700000000"
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
