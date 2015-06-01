using System.Text;
using System.Security.Cryptography;


public class MD5Manager
{
    public static MD5 md5Hash = MD5.Create();

    // Hash a given string into hexadecimal characters (taken from MSDN Library)
    // https://msdn.microsoft.com/en-us/library/system.security.cryptography.md5
    public static string hashPassword(string originalText)
    {
        // Convert the input string to a byte array and compute the hash. 
        byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(originalText));

        // Create a new Stringbuilder to collect the bytes and create a string.
        StringBuilder sBuilder = new StringBuilder();

        // Loop through each byte of the hashed data and format into hexadecimal string. 
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }

        // Return the hexadecimal string. 
        return sBuilder.ToString();
    }
}