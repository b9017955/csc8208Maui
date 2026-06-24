using System;
using System.Data;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using csc8208Maui.Models;
using csc8208Maui.Services;
using csc8208Maui.Views.Verifier;
using Newtonsoft.Json;
using ZXing.Net.Maui;

namespace csc8208Maui.ViewModels.Verifier;

public partial class QRCodeScannerViewModel : ObservableObject
{
    [ObservableProperty]
    Event selectedEvent;

    public QRCodeScannerViewModel(Event selectedEvent)
    {
        this.selectedEvent = selectedEvent;
        Console.WriteLine($"THE PAGE TITLE SHOULD BE: {selectedEvent.Artist}");
        
    }

    
    public async void ScanQRCode(object sender, BarcodeDetectionEventArgs e)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        string scannedEncodedQRCodeData = e.Results?.FirstOrDefault().Value;
        if (scannedEncodedQRCodeData is null)
        {
            await Shell.Current.Navigation.PushAsync(new QRCodeDecisionPage(SelectedEvent, 0, "ERROR SCANNING QR CODE"));
            return;
        }
        Console.WriteLine($"Barcode Data: {scannedEncodedQRCodeData}");
        // QR Code Data "{serverSignedTicket},{appTimeStamp},{appSignedTimeStamp}"
        //=========================================================================================================================================================
        // TODO CODE FOR VERIFYING TICKET AND TIMESTAMP
        
        //await Shell.Current.Navigation.PushAsync(new QRCodeDecisionPage(SelectedEvent, overallDecision, overallDecisionDetails));
    }

}
