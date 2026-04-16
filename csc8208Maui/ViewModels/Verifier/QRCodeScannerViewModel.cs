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
        Console.WriteLine($"Barcode Data: {scannedEncodedQRCodeData}");
        QRCode scannedQRCodeData;
        try
        {
            scannedQRCodeData = JsonConvert.DeserializeObject<QRCode>(scannedEncodedQRCodeData);
        }
        catch
        {
            await Shell.Current.Navigation.PushAsync(new QRCodeDecisionPage(SelectedEvent, 0, "ERROR SCANNING QR CODE"));

            return;
        }
        (Account accountInfo, Ticket ticketInfo) ticketInfoFromServer = WebService.VerifyTicket(scannedQRCodeData.serverSignedTicket).Result;
        (bool timeStampDecision, string timeStampDecisionDetails) timeStampVerificationOutcome = WebService.VerifyTimeStamp(scannedQRCodeData.appSignedTimeStamp, ticketInfoFromServer.accountInfo.appPublicKey);
        //(bool timeStampDecision, string timeStampDecisionDetails) timeStampVerificationOutcome = WebService.VerifyTimeStamp(scannedQRCodeData.appSignedTimeStamp, SecureStorage.GetAsync("DEBUGPUBLICKEY").Result);
        Console.WriteLine($"£ {timeStampVerificationOutcome.timeStampDecisionDetails}");
        
        int overallDecision;//0=Denied, 1=Approved, 2=Further Action Required
        string overallDecisionDetails;
        
        if (ticketInfoFromServer.accountInfo != null && ticketInfoFromServer.ticketInfo != null)
        {
            if (ticketInfoFromServer.ticketInfo.Artist.Equals(selectedEvent.Artist) &&
            ticketInfoFromServer.ticketInfo.EventLocation.Equals(selectedEvent.Location) &&
            ticketInfoFromServer.ticketInfo.DoorsOpen.Equals(selectedEvent.DoorsOpen))
            {
                overallDecision = timeStampVerificationOutcome.timeStampDecision ? 1 : 2;
                overallDecisionDetails = $"TICKET IS VALID::{timeStampVerificationOutcome.timeStampDecisionDetails}::FirstName:{ticketInfoFromServer.accountInfo.firstName}, SecondName:{ticketInfoFromServer.accountInfo.secondName}";
            }
            else
            {
                overallDecision = 0;
                overallDecisionDetails = $"TICKET IS NOT VALID FOR THIS EVENT::{timeStampVerificationOutcome.timeStampDecisionDetails}";
            }
        }
        else
        {
            overallDecision = 0;
            overallDecisionDetails = $"TICKET IS INVALID";
        }
        await Shell.Current.Navigation.PushAsync(new QRCodeDecisionPage(SelectedEvent, overallDecision, overallDecisionDetails));
        stopwatch.Stop();
        Console.WriteLine($"Time taken to verify QR Code: {stopwatch.Elapsed.TotalMilliseconds} ms");
    }

}
