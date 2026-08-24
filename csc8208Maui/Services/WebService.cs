using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Utilities.Encoders;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Org.BouncyCastle.Utilities.IO.Pem;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Math;
using csc8208Maui.Models;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Microsoft.Maui.Storage;
using System.Net;
using System.Net.Http.Json;
using System.Diagnostics;
using Java.Lang;
//using System.Security.Cryptography;
using System.Runtime.Intrinsics.Arm;
using Android.Service.Controls.Actions;
using System.ComponentModel;

namespace csc8208Maui.Services
{
    public static class WebService
    {
        public static string BaseURL= "https://192.168.1.192:7288/api/";//"https://192.168.1.192:7288/api/";//"https://10.0.2.2:7288/api/";// "https://18.169.193.100/";
        static HttpClient client;
        public static Account account;
        public static string connectionFailureMessage = "Failed to connect to server, check network settings. Some features on this app will be unavailable until you reconnect to the internet.";
        public static string registrationSuccessMessage = "Registration Successful";
        public static string registrationFailureUsernameTakenMessage = "That email is assigned to an existing account";
        public static string logoutErrorMessage = "There was an error whilst attempting to sign out.";
        //==========================================================================
        // Apps digital signature data
        static ECDsaSigner ecdsa;
        static string curveName = "secp256r1";
        static X9ECParameters curve= Org.BouncyCastle.Asn1.Sec.SecNamedCurves.GetByName(curveName);
        static ECDomainParameters parameters;
        //--------------------------------------------------------------------------
        // Used by user
        public static ECPrivateKeyParameters appSignaturePrivateKey;
        static ECPublicKeyParameters appSignaturePublicKey;
        //--------------------------------------------------------------------------
        // Used by verifier
        static ECPublicKeyParameters downloadedPublicKey;
        public static string loginSuccessMessage = "Login successful";
        public static string invalidCredentialsMessage = "Username or password is incorrect.";
        //==========================================================================


        static WebService()
        {
            //Production
            // var handler = new HttpClientHandler();
            // handler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;//In production we would have our certificate signed by a CA
            // client = new HttpClient(handler)
            // {
            //     BaseAddress = new Uri(BaseURL)
            // };

            //----------------------------------------
            //Debug DO NOT USE IN PRODUCTION
            //custom handler ignores SSL error caused by server's self-signed certificate
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                {
                    if (cert != null && cert.Issuer.Equals("CN=localhost"))
                        return true;
                    return errors == System.Net.Security.SslPolicyErrors.None;
                }
            };

            client = new HttpClient(handler)
            {
                BaseAddress = new Uri(BaseURL)
            };
            
            InitialiseSignatureSigning();
        }

        //For debugging purposes
        public static (byte[] r, byte[] s) DebugSignature()
        {
            var binaryPrivateKeyInfo = Convert.FromBase64String(SecureStorage.GetAsync("serialisedPrivateKeyInfo").Result);
            var decodedPrivateKey = new ECPrivateKeyParameters(new BigInteger(binaryPrivateKeyInfo), parameters);

            var message = Encoding.UTF8.GetBytes("Test Message");
            ecdsa.Init(true, decodedPrivateKey);
            var signature = ecdsa.GenerateSignature(message);
            ecdsa.Init(false, appSignaturePublicKey);
            var isSignatureValid = ecdsa.VerifySignature(message, signature[0], signature[1]);
            Console.WriteLine($"£Input Message:{message}");
            Console.WriteLine($"£StoredPrivateKey: {decodedPrivateKey.D}");
            Console.WriteLine($"£GeneratedPrivateKey: {appSignaturePrivateKey.D}");
            Console.WriteLine($"£PublicKey: {SecureStorage.GetAsync("serialisedPublicKeyInfo").Result}");
            Console.WriteLine($"£Signature {signature}, Checking validity of generated signature:{isSignatureValid}");
            Console.WriteLine($"£SIGNATURE FORMAT: {signature}, Length={signature.Length}");
            return (signature[0].ToByteArray(), signature[1].ToByteArray());
        }

        public static BigInteger[] GenerateAppSignature(byte[] message)
        {
            ecdsa.Init(true, appSignaturePrivateKey);
            return ecdsa.GenerateSignature(message);
        }

        public static (bool decision, string decisionDetails) VerifyTimeStamp(long timeStamp, BigInteger[] signature,  string base64EncodedSerialisedPublicKeyInfo)
        {
            var serialisedPublicKeyInfo = Convert.FromBase64String(base64EncodedSerialisedPublicKeyInfo);
            ECPoint publicKeyPoint = parameters.Curve.DecodePoint(serialisedPublicKeyInfo);
            downloadedPublicKey = new ECPublicKeyParameters(publicKeyPoint, parameters);
            //Console.WriteLine($"£DECRYPTION PUBLIC KEY:{Convert.ToBase64String(DownloadedPublicKey.Q.GetEncoded())}");
            ecdsa.Init(false, downloadedPublicKey);
            var isSignatureValid = ecdsa.VerifySignature(System.Security.Cryptography.SHA256.HashData(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(timeStamp)), signature[0], signature[1]);

            string decisionDetails;
            bool decision;
            if (isSignatureValid)
            {
                long currentTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var timeStampAge = currentTimeStamp - timeStamp;
                if (timeStampAge < 0)
                {
                    decision = false;
                    decisionDetails = $"INVALID TIMESTAMP, NEGATIVE AGE ({timeStampAge}s): REQUEST ID, DETAILS MUST MATCH TICKET"; //This should never happen, either QR code is corrupt or user is malicious
                }
                else if (timeStampAge > 300) 
                {
                    decision = false;
                    decisionDetails = $"STALE TIMESTAMP ({timeStampAge}s): REQUEST ID, DETAILS MUST MATCH TICKET";
                } 
                else
                {
                    decision = true;
                    decisionDetails = $"VALID TIMESTAMP ({timeStampAge}s)";
                }
            }
            else
            {
                decision = false;
                decisionDetails = $"INVALID TIMESTAMP: SIGNATURES DO NOT MATCH; QR CODE IS CORRUPT OR COUNTERFEIT";
            }
            return (decision, decisionDetails);
        }

        private static void InitialiseSignatureSigning()
        {
            ecdsa = new ECDsaSigner();
            parameters = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);
            var keyParameters = new ECKeyGenerationParameters(parameters, new SecureRandom());
            
            byte[] serialisedPrivateKey;
            byte[] serialisedPublicKey;
            //Look for key material in secure storage
            if(SecureStorage.GetAsync("serialisedPrivateKeyInfo").Result != null && SecureStorage.GetAsync("serialisedPublicKeyInfo").Result != null)
            {
                serialisedPrivateKey = Convert.FromBase64String(SecureStorage.GetAsync("serialisedPrivateKeyInfo").Result);
                serialisedPublicKey = Convert.FromBase64String(SecureStorage.GetAsync("serialisedPublicKeyInfo").Result);
                appSignaturePrivateKey = new ECPrivateKeyParameters(new BigInteger(serialisedPrivateKey), parameters);
                appSignaturePublicKey = new ECPublicKeyParameters(parameters.Curve.DecodePoint(serialisedPublicKey), parameters);
            }
            else
            {
                var generator = new ECKeyPairGenerator();
                generator.Init(keyParameters);
                var keyPair = generator.GenerateKeyPair();
                appSignaturePrivateKey = (ECPrivateKeyParameters) keyPair.Private;
                appSignaturePublicKey = (ECPublicKeyParameters) keyPair.Public;

                serialisedPrivateKey = ((ECPrivateKeyParameters)keyPair.Private).D.ToByteArray();
                serialisedPublicKey = ((ECPublicKeyParameters)keyPair.Public).Q.GetEncoded();
                SecureStorage.SetAsync("serialisedPrivateKeyInfo", Convert.ToBase64String(serialisedPrivateKey));
                SecureStorage.SetAsync("serialisedPublicKeyInfo", Convert.ToBase64String(serialisedPublicKey));
                Console.WriteLine($"New Public Key: {Convert.ToBase64String(serialisedPublicKey)}");
            }
        }

        //Testing
        private static void SanityCheck(){
            try
            {
                var testResponse = client.GetAsync("Login/test").Result;
                Console.WriteLine(testResponse);
            }
            catch
            {
                Console.WriteLine(connectionFailureMessage);
            }
        }
        
        public static (bool success, string message) Register(string emailAddress, string password, string firstName, string secondName, bool verifier)
        {
            //string serialisedPublicKey = SecureStorage.GetAsync("serialisedPublicKeyInfo").Result;
            var accountDTO = new AccountDTO();
            accountDTO.email = emailAddress;
            accountDTO.password = password;
            accountDTO.firstName = firstName;
            accountDTO.surname = secondName;
            accountDTO.type = verifier ? AccountDTO.AccountType.ADMIN : AccountDTO.AccountType.USER;
            
            var registrationPayload = JsonConvert.SerializeObject(accountDTO);
            Console.WriteLine(registrationPayload);
            var content = new StringContent(registrationPayload, Encoding.UTF8, "application/json");
            try
            {
                var response = client.PostAsync("Login/register", content).Result;
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine($"Registration response: {responseBody}");
                    return (true, registrationSuccessMessage);
                }
                else
                {
                    return (false, registrationFailureUsernameTakenMessage);
                }
            }
            catch
            {
                return (false, connectionFailureMessage);
            }
        }

        public static async Task<(bool success, string message)> LoginAsync(string username, string password)
        {
            bool online = CheckConnectionToInternet();
            SanityCheck();
            if (!online)
            {
                return (false, connectionFailureMessage);
            }

            var authDataPayload = new { username, password };
            var authDataJSON = JsonConvert.SerializeObject(authDataPayload);
            var content = new StringContent(authDataJSON, Encoding.UTF8, "application/json");

            try
            {
                var response = await Task.Run(() => client.PostAsync("Login", content).Result);
                if (response.StatusCode.Equals(HttpStatusCode.OK))
                {
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    string JWT = responseBody;
                    Console.WriteLine($"JWT={JWT}");
                    await SecureStorage.SetAsync("JWT", JWT);
                    SetHTTPHeaders(JWT);
                    SubmitPublicKey();

                    bool accountDownloadSuccess = GetAccountInfo();
                    if (!accountDownloadSuccess)
                    {
                        //error getting accountDTO
                        return (false, "Login successful but there was an error retrieving accountDTO");
                    }
                    return (true, loginSuccessMessage);
                }
                else
                {
                    return (false, invalidCredentialsMessage);
                }
            }
            catch
            {
                return (false, connectionFailureMessage);
            }
            
        }

        public static (bool success, string message) SubmitPublicKey()
        {
            var encodedPublicKey = Convert.ToBase64String(appSignaturePublicKey.Q.GetEncoded());
            var json = JsonConvert.SerializeObject(new {encodedPublicKey});
            var payload = new StringContent(json, Encoding.UTF8, "application/json");
            var keyUpdateResponse = client.PostAsync("Login/submitClientPublicKey", payload).Result;
            if (keyUpdateResponse.IsSuccessStatusCode)
            {
                return (true, "Successfully updated public key");
            }
            else
            {
                return (false, connectionFailureMessage);
            }
        }

        public static bool Logout()
        {
            try
            {
                var response = client.GetAsync("SignOut").Result;
                if (response.IsSuccessStatusCode)
                {
                    SecureStorage.SetAsync("JWT", null);
                    SecureStorage.SetAsync("account", null);
                    SecureStorage.SetAsync("serialisedPublicKeyInfo", null);
                    SecureStorage.SetAsync("serialisedPrivateKeyInfo", null);
                    client.Dispose();
                    client = new HttpClient
                    {
                        BaseAddress = new Uri(BaseURL)
                    };
                    account = null;
                    appSignaturePrivateKey = null;
                    appSignaturePublicKey = null;
                    return true;
                }
                else
                {
                    Console.WriteLine("INCORRECT RESPONSE WHILST ATTEMPTING TO SIGN OUT");
                    return false;
                }
            }
            catch
            {
                return false;
            }
            
            
        }

        public static bool CheckConnectionToInternet()
        {
            var pingServer = new Ping();
            var pingOptions = new PingOptions { DontFragment = true };
            var buffer = Encoding.ASCII.GetBytes("ping");
            var timeout = 120;
            try
            {
                var reply = pingServer.SendPingAsync("google.com", timeout).Result;
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        public static bool GetAccountInfo()
        {
            var response = client.GetAsync("Login/GetAccount").Result;
            if (response.IsSuccessStatusCode)
            {
                var responseBody = response.Content.ReadAsStringAsync().Result;
                
                AccountDTO accountDTO = JsonConvert.DeserializeObject<AccountDTO>(responseBody);
                Console.WriteLine($"Account found! Account type {accountDTO.type} {accountDTO.firstName} {accountDTO.surname} {accountDTO.email}");
                account = new Account(accountDTO.firstName, accountDTO.surname, accountDTO.email, accountDTO.type==AccountDTO.AccountType.ADMIN);
                return true;
            }
            else
            {
                //Invalid JWT
                Console.WriteLine("Error from webserver; account not found.");
                return false;
            }
        }

        public static async Task<EventStore> GetEvents()
        {
            try
            {
                var response = await client.GetAsync("GetEvents");
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = response.Content.ReadAsStringAsync().Result;
                    EventStore eventStore = new EventStore();
                    List<Event> events = JsonConvert.DeserializeObject<List<Event>>(responseBody);
                    foreach(Event e in events)
                    {
                        eventStore.AddItemAsync(e);
                    }
                    return eventStore;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public static async Task<string> BuyTicket(int id)
        {
            //return "TICKET";//DEBUG DELETE THIS //Whoops hopefully they glossed over this
            int eventId = id;
            var response = await client.PostAsJsonAsync("Ticket/BuyTicket", eventId);//client.PostAsync("Ticket/BuyTicket", content);
            if (response.IsSuccessStatusCode)
            {
                var encodedSignedTicket = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine($"Recieved New Ticket: {encodedSignedTicket}");
                return encodedSignedTicket;
            }
            else
            {
                throw new System.Exception("BuyTicket() received a response from the server that was not a success code");
            }
        }

        public static async Task<UserTicketStore> GetTickets()
        {
            try
            {
                var response = await client.GetAsync("GetTickets");
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = response.Content.ReadAsStringAsync().Result;// Needs deserialising <-------------------------
                    return new UserTicketStore();
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
            
        }
        
        public static async Task<(AccountDTO accountInfo, EventDTO eventInfo)> GetTicketInfo(string encodedTicketHash)
        {
            var response = await client.PostAsJsonAsync("Ticket/GetTicketInfo", encodedTicketHash);
            if (response.IsSuccessStatusCode)
            {
                string encodedTicketInfo = response.Content.ReadAsStringAsync().Result;
                (AccountDTO,EventDTO) ticketInfo = JsonConvert.DeserializeObject<(AccountDTO,EventDTO)>(encodedTicketInfo);
                return ticketInfo;
            }
            else
            {
                Console.WriteLine($"Unable to retrieve ticket info, server responded with '{response.StatusCode} {response.Content}'");
                return (null,null);
            }
        }

        public static async Task<bool> VerifyTicket(byte[] ticketHash, BigInteger[] signature)
        {
            var response = client.GetAsync("Ticket/GetServerPublicKey").Result;
            if (response.IsSuccessStatusCode)
            {
                var encodedServerPublicKeyPoint = Convert.FromBase64String(response.Content.ReadAsStringAsync().Result);
                var serverPublicKeyPoint = parameters.Curve.DecodePoint(encodedServerPublicKeyPoint);
                var serverPublicKey = new ECPublicKeyParameters(serverPublicKeyPoint, parameters);
                ecdsa.Init(false, serverPublicKey);
                return ecdsa.VerifySignature(ticketHash, signature[0], signature[1]);

            }
            else
            {
                throw new InvalidOperationException("Error retrieving public key from server");
            }
            
        }
        public static void SetHTTPHeaders(string JWT)
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", JWT);
        }
    }
}
