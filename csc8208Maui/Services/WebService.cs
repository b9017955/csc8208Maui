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

namespace csc8208Maui.Services
{
    public static class WebService
    {
        public static string BaseURL= "https://18.169.193.100/";
        static HttpClient client;
        public static Account account;
        public static string connectionFailureMessage = "Failed to connect to server, check network settings. Some features on this app will be unavailable until you reconnect to the internet.";
        public static string registrationSuccessMessage = "Registration Successful";
        public static string registrationFailureUsernameTakenMessage = "That email is assigned to an existing account";
        public static string logoutErrorMessage = "There was an error whilst attempting to sign out.";
        //==========================================================================
        // Apps digital signature data
        static ECDsaSigner ecdsa = new ECDsaSigner();
        static string curveName = "secp256r1";
        static X9ECParameters curve= Org.BouncyCastle.Asn1.Sec.SecNamedCurves.GetByName(curveName);
        static ECDomainParameters parameters;
        //--------------------------------------------------------------------------
        // Used by user
        public static ECPrivateKeyParameters AppSignaturePrivateKey;
        static ECPublicKeyParameters AppSignaturePublicKey;
        static bool newAppSignatureKeyMaterialRequired = false;
        //--------------------------------------------------------------------------
        // Used by verifier
        static ECPublicKeyParameters DownloadedPublicKey;
        public static string loginSuccessMessage = "Login successful";
        public static string invalidCredentialsMessage = "Username or password is incorrect.";
        //==========================================================================


        static WebService()
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;//In production we would have our certificate signed by a CA
            client = new HttpClient(handler)
            {
                BaseAddress = new Uri(BaseURL)
            };
            //debug
            newAppSignatureKeyMaterialRequired = true;
            //Check if secure storage contains key material for app signature
            parameters = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);
            if (SecureStorage.GetAsync("serialisedPrivateKeyInfo").Result != null && SecureStorage.GetAsync("serialisedPublicKeyInfo").Result != null)
            {
                Console.WriteLine("Found key material");
                string serialisedPrivateKeyInfo = SecureStorage.GetAsync(nameof(serialisedPrivateKeyInfo)).Result;
                AppSignaturePrivateKey = new ECPrivateKeyParameters(new BigInteger(Convert.FromBase64String(serialisedPrivateKeyInfo)), parameters);
                var serialisedPublicKeyInfo = Convert.FromBase64String(SecureStorage.GetAsync("serialisedPublicKeyInfo").Result);
                ECPoint publicKeyPoint = parameters.Curve.DecodePoint(serialisedPublicKeyInfo);
                AppSignaturePublicKey = new ECPublicKeyParameters(publicKeyPoint, parameters);
            }
            else
            {
                //Signature scheme must be initialised
                newAppSignatureKeyMaterialRequired = true;
            }
        }

        //For debugging purposes
        public static (byte[] r, byte[] s) DebugSignature()
        {
            InitialiseNewAppSignature();
            
            var binaryPrivateKeyInfo = Convert.FromBase64String(SecureStorage.GetAsync("serialisedPrivateKeyInfo").Result);
            var decodedPrivateKey = new ECPrivateKeyParameters(new BigInteger(binaryPrivateKeyInfo), parameters);

            var message = Encoding.UTF8.GetBytes("Test Message");
            ecdsa.Init(true, decodedPrivateKey);
            var signature = ecdsa.GenerateSignature(message);
            ecdsa.Init(false, AppSignaturePublicKey);
            var isSignatureValid = ecdsa.VerifySignature(message, signature[0], signature[1]);
            Console.WriteLine($"£Input Message:{message}");
            Console.WriteLine($"£StoredPrivateKey: {decodedPrivateKey.D}");
            Console.WriteLine($"£GeneratedPrivateKey: {AppSignaturePrivateKey.D}");
            Console.WriteLine($"£PublicKey: {SecureStorage.GetAsync("serialisedPublicKeyInfo").Result}");
            Console.WriteLine($"£Signature {signature}, Checking validity of generated signature:{isSignatureValid}");
            Console.WriteLine($"£SIGNATURE FORMAT: {signature}, Length={signature.Length}");
            return (signature[0].ToByteArray(), signature[1].ToByteArray());
        }

        public static (byte[] serialisedTimeStamp, byte[] r, byte[] s) GenerateSignedTimeStamp()
        {
            var newTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var serialisedTimeStamp = BitConverter.GetBytes(newTimeStamp);
            ecdsa.Init(true, AppSignaturePrivateKey);
            //Console.WriteLine($"£ENCRYPTION PUBLIC KEY:{Convert.ToBase64String(AppSignaturePublicKey.Q.GetEncoded())}");
            var signature = ecdsa.GenerateSignature(serialisedTimeStamp);
            return (serialisedTimeStamp, signature[0].ToByteArray(), signature[1].ToByteArray());
        }

        public static (bool decision, string decisionDetails) VerifyTimeStamp((byte[] serialisedTimeStamp, byte[] r, byte[] s) serialisedSignature,  string base64EncodedSerialisedPublicKeyInfo)
        {
            var serialisedPublicKeyInfo = Convert.FromBase64String(base64EncodedSerialisedPublicKeyInfo);
            ECPoint publicKeyPoint = parameters.Curve.DecodePoint(serialisedPublicKeyInfo);
            DownloadedPublicKey = new ECPublicKeyParameters(publicKeyPoint, parameters);
            //Console.WriteLine($"£DECRYPTION PUBLIC KEY:{Convert.ToBase64String(DownloadedPublicKey.Q.GetEncoded())}");
            BigInteger[] signature = {new BigInteger(serialisedSignature.r), new BigInteger(serialisedSignature.s)}; 
            ecdsa.Init(false, DownloadedPublicKey);
            var isSignatureValid = ecdsa.VerifySignature(serialisedSignature.serialisedTimeStamp, signature[0], signature[1]);

            string decisionDetails;
            bool decision;
            if (isSignatureValid)
            {
                long timeStampFromQRCode = BitConverter.ToInt64(serialisedSignature.serialisedTimeStamp, 0);
                long currentTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var timeStampAge = currentTimeStamp - timeStampFromQRCode;
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

        public static void InitialiseNewAppSignature()
        {
            //var curve = Org.BouncyCastle.Asn1.Sec.SecNamedCurves.GetByName(curveName);
            parameters = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);
            var keyParameters = new ECKeyGenerationParameters(parameters, new SecureRandom());
            var generator = new ECKeyPairGenerator();
            generator.Init(keyParameters);
            var keyPair = generator.GenerateKeyPair();

            /*PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keyPair.Private);
            string serialisedPrivateKeyInfo = Convert.ToBase64String(privateKeyInfo.ToAsn1Object().GetDerEncoded());*/
            
            byte[] privateKeyInfo = ((ECPrivateKeyParameters)keyPair.Private).D.ToByteArray();
            string serialisedPrivateKeyInfo = Convert.ToBase64String(privateKeyInfo);

            byte[] binaryPublicKeyInfo = ((ECPublicKeyParameters)keyPair.Public).Q.GetEncoded();
            string serialisedPublicKeyInfo = Convert.ToBase64String(binaryPublicKeyInfo);
            Console.WriteLine($"New Public Key: {serialisedPublicKeyInfo}");
            SecureStorage.SetAsync(nameof(serialisedPrivateKeyInfo), serialisedPrivateKeyInfo);
            SecureStorage.SetAsync(nameof(serialisedPublicKeyInfo), serialisedPublicKeyInfo);

            AppSignaturePrivateKey = (ECPrivateKeyParameters) keyPair.Private;
            AppSignaturePublicKey = (ECPublicKeyParameters) keyPair.Public;

            newAppSignatureKeyMaterialRequired = false;
        }

        public static (bool success, string message) Register(string emailAddress, string password, string firstName, string secondName, bool verifier)
        {
            string serialisedPublicKey = SecureStorage.GetAsync("serialisedPublicKeyInfo").Result;
            var registrationDataPayload = new { 
                email = emailAddress, 
                password = password, 
                first_name = firstName, 
                second_name = secondName, 
                account_type = verifier,
            };
            var registrationJSON = JsonConvert.SerializeObject(registrationDataPayload);
            var content = new StringContent(registrationJSON, Encoding.UTF8, "application/json");
            try
            {
                var response = client.PostAsync("CreateAccount", content).Result;
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

        public static (bool success, string message) Login(string username, string password)
        {
            bool online = CheckConnectionToServer();
            if (!online)
            {
                return (false, connectionFailureMessage);
            }

            var authDataPayload = new { email = username, password = password };
            var authDataJSON = JsonConvert.SerializeObject(authDataPayload);
            var content = new StringContent(authDataJSON, Encoding.UTF8, "application/json");

            try
            {
                var response = client.PostAsync("SignIn", content).Result;
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    string JWT = "";
                    SecureStorage.SetAsync("JWT", JWT);
                    SetHTTPHeaders(JWT);
                    // User may be loging in to a new device
                    if (newAppSignatureKeyMaterialRequired)
                    {
                        InitialiseNewAppSignature();
                        var serialisedPublicKey = SecureStorage.GetAsync("serialisedPublicKeyInfo").Result;
                        var keyMaterial = new { app_public_key = serialisedPublicKey };
                        var keyMaterialJSON = JsonConvert.SerializeObject(keyMaterial);
                        var keyUpdateContent = new StringContent(keyMaterialJSON, Encoding.UTF8, "application/json");
                        var keyUpdateResponse = client.PostAsync("UpdatePublicKey", keyUpdateContent);
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

        public static (bool success, string message) UpdatePublicKey()
        {
            var publicKeyPayload = new { app_public_key = AppSignaturePublicKey };
            var publicKeyPayloadJSON = JsonConvert.SerializeObject(publicKeyPayload);
            var content = new StringContent(publicKeyPayloadJSON, Encoding.UTF8, "application/json");
            var response = client.PostAsync("UpdatePublicKey", content).Result;
            if (response.IsSuccessStatusCode)
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
                    ecdsa = new ECDsaSigner();
                    newAppSignatureKeyMaterialRequired = true;
                    AppSignaturePrivateKey = null;
                    AppSignaturePublicKey = null;
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

        public static bool CheckConnectionToServer()
        {
            var pingServer = new Ping();
            var pingOptions = new PingOptions { DontFragment = true };
            var buffer = Encoding.ASCII.GetBytes("ping");
            var timeout = 120;
            var reply = pingServer.SendPingAsync(WebService.BaseURL, timeout, buffer, pingOptions).Result;
            //App is online
            return reply.Status == IPStatus.Success;
        }

        public static Account GetAccountInfo()
        {
            var response = client.GetAsync("GetAccount").Result;
            if (response.IsSuccessStatusCode)
            {
                var responseBody = response.Content.ReadAsStringAsync().Result;
                Account deserialisedAccount = JsonConvert.DeserializeObject<Account>(responseBody);
                return deserialisedAccount;
            }
            else
            {
                //Invalid JWT
                return null;
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
            catch(Exception e)
            {
                return null;
            }
        }

        public static async Task<(int id, int[] signedTicket)> BuyTicket(string id)
        {
            //return "TICKET";//DEBUG DELETE THIS //Whoops hopefully they glossed over this
            var orderPayload = new { email = "a.vann1@ncl.ac.uk", token = "eyJhbGciOiJIUzI1NiJ9.dGVzdF9lbWFpbEBlbWFpbC5jb20xNjc5MzI1MjQ0NzQ0dGVzdF9wYXNzd29yZA.1zOjjtMWzmwUkTlWbVdc99-LZm7BgvDnfMT90vjrBG4", event_id = id};
            var orderInfoJSON = JsonConvert.SerializeObject(orderPayload);
            var content = new StringContent(orderInfoJSON, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("BuyTicket", content);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = response.Content.ReadAsStringAsync().Result;// Needs deserialising <-------------------------
                Console.WriteLine($"Recieved New Ticket: {responseBody}");
                //(int ticket_id, int[] signed_ticket_id) returnJson = JsonConvert.DeserializeObject<(int ticket_id, int[] signed_ticket_id)>(responseBody);
                Dictionary<string, object> returnJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);
                int ticket_id = int.Parse(returnJson["ticket_id"].ToString());
                string[] signed_ticket_id_str = returnJson["signed_ticket_id"].ToString().Replace("[", "").Replace("]", "").Split(',');
                int[] signed_ticket_id = new int[signed_ticket_id_str.Length];
                for (int i = 0; i < signed_ticket_id.Length; i++)
                {
                    signed_ticket_id[i] = int.Parse(signed_ticket_id_str[i]);
                }
                //(int, int[]) tuple = ((int) returnJson["ticket_id"], returnJson["signed_ticket_id"].ToString());
                Console.WriteLine($"RETURN JSON: {ticket_id},{string.Join(",", signed_ticket_id)}");
                //var actualJson = JsonConvert.DeserializeObject<(int ticket_id, int[] signed_ticket_id)>(returnJson);
                //JObject deserialisedData = JObject.Parse((string)JObject.Parse(responseBody).GetValue("return"));
                //Console.WriteLine($"JOBJECT: {deserialisedData}");
                //return ((int) deserialisedData["ticket_id"], (string) deserialisedData["signed_ticket_id"]);
                //return (actualJson.ticket_id, actualJson.signed_ticket_id.ToString());
                //return (ticket_id, $"[{string.Join(",", signed_ticket_id)}]");
                return (ticket_id, signed_ticket_id);
            }
            else
            {
                return (-1,null);
            }
        }

        public static async Task<UserTickets> GetTickets()
        {
            try
            {
                var response = await client.GetAsync("GetTickets");
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = response.Content.ReadAsStringAsync().Result;// Needs deserialising <-------------------------
                    return new UserTickets();
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
        
        public static async Task<(Account accountInfo, Ticket ticketInfo)> VerifyTicket((int ticket_id, int[] signed_ticket_id) serverSignatureOnTicketHash)
        {
            /*var account = new Account("Joe", "Blogs", "joeblogs@email.com", false);
            account.appPublicKey = SecureStorage.GetAsync("DEBUGPUBLICKEY").Result;
            return (null, null);
            return (account, new Ticket("0", "The Hunna", Genre.Rock, "Hunna Fever", "Watford", "19:00", null, null));//REMOVE THIS*/
            var ticketPayload = new { ticket_id = serverSignatureOnTicketHash.ticket_id, serverSignatureOnTicketHash.signed_ticket_id };
            var ticketPayloadJSON = JsonConvert.SerializeObject(ticketPayload);
            var content = new StringContent(ticketPayloadJSON, Encoding.UTF8, "application/json");
            Console.WriteLine($"SENDING FOR VALIDATION: {ticketPayloadJSON}");
            try
            {
                var response = await client.PostAsync("ValidateTicket", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = response.Content.ReadAsStringAsync().Result;// Needs deserialising <-------------------------
                    Console.WriteLine($"VALIDATION RESPONSE: {responseBody}");
                    return (null,null);
                }
                else
                {
                    return (null, null);
                }
            }
            catch
            {
                return (null, null);
            }
        }

        public static void SetHTTPHeaders(string JWT)
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", JWT);
        }
    }
}
