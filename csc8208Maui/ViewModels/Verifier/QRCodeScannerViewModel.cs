using System;
using System.Data;
using System.Diagnostics;
using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using csc8208Maui.Models;
using csc8208Maui.Services;
using csc8208Maui.Views.Verifier;
using Java.Lang;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Modes;
using ZXing.Net.Maui;

namespace csc8208Maui.ViewModels.Verifier;

public partial class QRCodeScannerViewModel : ObservableObject
{
    [ObservableProperty]
    int timestampExpirySeconds=30;
    [ObservableProperty]
    Event selectedEvent;
    public Command GoBackCommand {get;}
    private async void OnBackClicked(object obj)
    {
        await Shell.Current.Navigation.PopModalAsync();
        Console.WriteLine("Go Back Clicked");
    }

    public QRCodeScannerViewModel(Event selectedEvent)
    {
        this.selectedEvent = selectedEvent;
        Console.WriteLine($"THE PAGE TITLE SHOULD BE: {selectedEvent.Artist}");
        GoBackCommand = new Command(OnBackClicked);
        
    }
    

    
    public async void ScanQRCode(object sender, BarcodeDetectionEventArgs e)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        string rawQRCodeData = e.Results?.FirstOrDefault().Value;
        if (rawQRCodeData is null)
        {
            await Shell.Current.Navigation.PushAsync(new QRCodeDecisionPage(SelectedEvent, 0, "ERROR SCANNING QR CODE"));
            return;
        }
        Console.WriteLine($"Barcode Data: {rawQRCodeData}");
        // QR Code Data "{tickethash,r,s},{appTimeStamp},{appSignedTimeStampR appSignedTimeStampS}"
        //=========================================================================================================================================================
        // TODO CODE FOR VERIFYING TICKET AND TIMESTAMP
        try
        {
            string[] QRCodeComponents = rawQRCodeData.Split(',');
            string encodedTicketHash = QRCodeComponents[0];
            string encodedServerSignature = $"{QRCodeComponents[1]},{QRCodeComponents[2]},";
            string encodedTimeStamp = QRCodeComponents[3];
            string encodedAppSignature = $"{QRCodeComponents[4]},{QRCodeComponents[5]},";

            //VERIFY SERVERSIGNED TICKET USING SERVER PUBLIC KEY
            byte[] ticketHash = Convert.FromBase64String(encodedTicketHash);
            var serverSignature = Serialisers.DeserialiseSignature(encodedServerSignature);
            bool ticketIsAuthentic = WebService.VerifyTicket(ticketHash, serverSignature).Result;
            if (ticketIsAuthentic)
            {
                //SEND TICKETHASH TO SERVER AND RECEIVE EVENTINFO
                var ticketInfo = WebService.GetTicketInfo(rawQRCodeData.Split(',')[0]).Result;
                //ENSURE TICKET DETAILS MATCH THIS EVENT
                if(ticketInfo.eventInfo is not null && ticketInfo.eventInfo.id == SelectedEvent.ID)
                {
                    //VERIFY APP TIMESTAMP
                    byte[] serialisedTimeStamp = Convert.FromBase64String(encodedTimeStamp);
                    long timeStamp = BitConverter.ToInt64(serialisedTimeStamp);
                    long timeStampAge = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timeStamp;
                    if(timeStampAge < TimestampExpirySeconds)
                    {
                        await Shell.Current.Navigation.PushAsync(new QRCodeDecisionPage(SelectedEvent, 1, "APPROVED"));
                    }
                    else
                    {
                        await Shell.Current.Navigation.PushAsync(new QRCodeDecisionPage(SelectedEvent, 2, "TIMESTAMP IS STALE OR INVALID, REQUEST CUSTOMER'S ID"));
                    }
                }
                else
                {
                    await Shell.Current.Navigation.PushAsync(new QRCodeDecisionPage(SelectedEvent, 0, "TICKET NOT VALID FOR EVENT"));
                }
            }
            else
            {
                await Shell.Current.Navigation.PushAsync(new QRCodeDecisionPage(SelectedEvent, 0, "TICKET IS NOT VALID"));
            }
        }
        catch(System.Exception barcodeReadException)
        {
            Console.WriteLine($"Error whilst extracting ticket from barcode with exception {barcodeReadException}");
        }
        
    }

}
