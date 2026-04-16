using csc8208Maui.ViewModels.Verifier;
using csc8208Maui.Models;
using ZXing.Net.Maui;
using System.Diagnostics;

namespace csc8208Maui.Views.Verifier;

public partial class QRCodeScanner : ContentPage
{
	QRCodeScannerViewModel vm;
	public QRCodeScanner(QRCodeScannerViewModel viewModel)
	{
		InitializeComponent();
		cameraBarcodeReaderView.Options = new BarcodeReaderOptions
			{
			Formats = BarcodeFormats.TwoDimensional,
			AutoRotate = true,
			Multiple = false
			};
		vm = viewModel;
		this.BindingContext= vm;
	}

	protected void BarcodeReader(object sender, BarcodeDetectionEventArgs e)
	{
		vm.ScanQRCode(sender,e);
	}
}
