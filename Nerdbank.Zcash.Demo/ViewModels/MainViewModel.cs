using System;
using System.IO;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;
using Nerdbank.Bitcoin;
using ReactiveUI;

namespace Nerdbank.Zcash.Demo.ViewModels;

public class MainViewModel : ViewModelBase
{
	private readonly ZcashAccount account;
	private readonly LightWalletClient client;
	private decimal sendAmount;
	private string sendRecipient = string.Empty;
	private string? status;
	private AccountBalances balances = new();

	public MainViewModel()
		: this(new ZcashAccount(
			new Zip32HDWallet(Bip39Mnemonic.Create(256), ZcashNetwork.MainNet)))
	{
	}

	public MainViewModel(ZcashAccount account)
	{
		this.account = account;

		// Carefully construct a UA that lacks the orchard receiver until librustzcash supports it.
		this.Address = UnifiedAddress.Create(
			new SaplingAddress(this.account.DefaultAddress.GetPoolReceiver<SaplingReceiver>()!.Value, this.account.Network),
		new TransparentP2PKHAddress(this.account.DefaultAddress.GetPoolReceiver<TransparentP2PKHReceiver>()!.Value, this.account.Network));

		this.SendCommand = ReactiveCommand.CreateFromTask(this.SendAsync);

		string dataFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		this.client = new LightWalletClient(new Uri("https://zcash.mysideoftheweb.com:9067/"), account.Network, dataFile);
		this.WatchBalanceAsync().Forget();
	}

	public string Address { get; }

	public string SeedPhrase => this.account.HDDerivation!.Value.Wallet.Mnemonic!.SeedPhrase;

	public ZcashNetwork Network => this.account.Network;

	public string? Status
	{
		get => this.status;
		set => this.RaiseAndSetIfChanged(ref this.status, value);
	}

	public AccountBalances Balances
	{
		get => this.balances;
		set => this.RaiseAndSetIfChanged(ref this.balances, value);
	}

	public decimal SendAmount
	{
		get => this.sendAmount;
		set => this.RaiseAndSetIfChanged(ref this.sendAmount, value);
	}

	public string SendUnits => this.account.Network.AsSecurity().TickerSymbol;

	public string SendRecipient
	{
		get => this.sendRecipient;
		set => this.RaiseAndSetIfChanged(ref this.sendRecipient, value);
	}

	public ReactiveCommand<Unit, Unit> SendCommand { get; }

	public async Task SendAsync()
	{
		try
		{
			await this.client.SendAsync(
				this.account,
				[new Transaction.SendItem(ZcashAddress.Decode(this.SendRecipient), this.SendAmount, Memo.FromMessage("Thank you ZF!"))],
				progress: null,
				CancellationToken.None);
			this.SendRecipient = string.Empty;
			this.SendAmount = 0;
			this.Status = "Sent!";
		}
		catch (Exception ex)
		{
			this.Status = $"Error: {ex.Message}";
		}
	}

	private async Task WatchBalanceAsync()
	{
		try
		{
			if (this.account.BirthdayHeight is null)
			{
				this.account.BirthdayHeight = await this.client.GetLatestBlockHeightAsync(CancellationToken.None) - 100;
			}

			await this.client.AddAccountAsync(this.account, CancellationToken.None);
			Progress<LightWalletClient.SyncProgress> progress = new(p =>
			{
				this.Status = p.LastFullyScannedBlock == p.TipHeight ? $"Up to date ({p.LastFullyScannedBlock})" : $"Syncing {p.LastFullyScannedBlock} / {p.TipHeight}";
				this.Balances = this.client.GetBalances(this.account);
			});

			while (true)
			{
				try
				{
					await this.client.DownloadTransactionsAsync(progress, discoveredTransactions: null, continually: true, cancellationToken: CancellationToken.None);
				}
				catch (Exception ex)
				{
					this.Status = $"Error: {ex.Message}";
					await Task.Delay(5000);
				}
			}
		}
		catch (Exception ex)
		{
			this.Status = $"Error: {ex.Message}";
		}
	}
}
