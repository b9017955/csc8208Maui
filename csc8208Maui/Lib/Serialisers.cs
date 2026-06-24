using System;
using Org.BouncyCastle.Math;

namespace csc8208Maui;

public static class Serialisers
{
    /// <summary>
    /// Takes an ECDSA signature and outputs a comma seperated string containing r and s (both Base64 encoded)
    /// </summary>
    /// <param name="signature"></param>
    /// <returns></returns>
    public static string SerialiseSignature(BigInteger[] signature)
    {
        var encodedR = Convert.ToBase64String(signature[0].ToByteArray());
        var encodedS = Convert.ToBase64String(signature[1].ToByteArray());
        return $"{encodedR},{encodedS}";
    }

    public static BigInteger[] DeserialiseSignature(string base64String)
    {
        var encodedR = base64String.Split(",")[0];
        var encodedS = base64String.Split(",")[1];
        
        var decodedR = new BigInteger(Convert.FromBase64String(encodedR));
        var decodedS = new BigInteger(Convert.FromBase64String(encodedS));

        return [decodedR,decodedS];
    }
}
