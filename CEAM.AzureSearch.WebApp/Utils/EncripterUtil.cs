using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CEAM.AzureSearch.WebApp.Utils
{
    public class EncripterUtil
    {
        //public static string EncryptString(string key, string plainText)
        //{
        //    byte[] iv = new byte[16];
        //    byte[] array;

        //    using (Aes aes = Aes.Create())
        //    {
        //        aes.Key = Encoding.UTF8.GetBytes(key);
        //        aes.IV = iv;

        //        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        //        using (MemoryStream memoryStream = new MemoryStream())
        //        {
        //            using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
        //            {
        //                using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
        //                {
        //                    streamWriter.Write(plainText);
        //                }

        //                array = memoryStream.ToArray();
        //            }
        //        }
        //    }

        //    return Convert.ToBase64String(array);
        //}

        //public static string DecryptString(string key, string cipherText)
        //{
        //    byte[] iv = new byte[16];
        //    byte[] buffer = Convert.FromBase64String(cipherText);

        //    using (Aes aes = Aes.Create())
        //    {
        //        aes.Key = Encoding.UTF8.GetBytes(key);
        //        aes.IV = iv;
        //        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        //        using (MemoryStream memoryStream = new MemoryStream(buffer))
        //        {
        //            using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
        //            {
        //                using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
        //                {
        //                    return streamReader.ReadToEnd();
        //                }
        //            }
        //        }
        //    }
        //}


        //public static class Global
        //{
        //    // set password
        //    public const string strPassword = "LetMeIn99$";

        //    // set permutations
        //    public const String strPermutation = "ouiveyxaqtd";
        //    public const Int32 bytePermutation1 = 0x19;
        //    public const Int32 bytePermutation2 = 0x59;
        //    public const Int32 bytePermutation3 = 0x17;
        //    public const Int32 bytePermutation4 = 0x41;
        //}


        ////// The console window
        ////public static void Main(String[] args)
        ////{

        ////    Console.Title = "Secure Password v2";
        ////    Console.WriteLine("Output---");
        ////    Console.WriteLine("");

        ////    Console.WriteLine("Password:  " + Global.strPassword);

        ////    string strEncrypted = (Encrypt(Global.strPassword));
        ////    Console.WriteLine("Encrypted: " + strEncrypted);

        ////    string strDecrypted = (Decrypt(strEncrypted));
        ////    Console.WriteLine("Decrypted: " + strDecrypted);

        ////    Console.ReadKey();
        ////}


        //public static string Encriptar(string strData)
        //{
        //    byte[] data = Encoding.UTF8.GetBytes(strData);
        //    string b64 = Convert.ToBase64String(data);
        //    return b64;
        //}

        //public static string Desencriptar(string strData)
        //{
        //    byte[] data2 = Convert.FromBase64String(strData);
        //    string o64 = Encoding.UTF8.GetString(data2);
        //    return o64;
        //}

        ////// encoding
        //public static string Encrypt(string strData)
        //{



        //    return Convert.ToBase64String(Encrypt(Encoding.UTF32.GetBytes(strData)));
        //    // reference https://msdn.microsoft.com/en-us/library/ds4kkd55(v=vs.110).aspx

        //}


        //// decoding
        //public static string Decrypt(string strData)
        //{
        //    return Encoding.UTF32.GetString(Decrypt(Convert.FromBase64String(strData)));
        //    // reference https://msdn.microsoft.com/en-us/library/system.convert.frombase64string(v=vs.110).aspx

        //}

        //// encrypt
        //public static byte[] Encrypt(byte[] strData)
        //{
        //    PasswordDeriveBytes passbytes =
        //    new PasswordDeriveBytes(Global.strPermutation,
        //    new byte[] { Global.bytePermutation1,
        //                 Global.bytePermutation2,
        //                 Global.bytePermutation3,
        //                 Global.bytePermutation4
        //    });

        //    MemoryStream memstream = new MemoryStream();
        //    Aes aes = new AesManaged();
        //    aes.Key = passbytes.GetBytes(aes.KeySize / 8);
        //    aes.IV = passbytes.GetBytes(aes.BlockSize / 8);

        //    CryptoStream cryptostream = new CryptoStream(memstream,
        //    aes.CreateEncryptor(), CryptoStreamMode.Write);
        //    cryptostream.Write(strData, 0, strData.Length);
        //    cryptostream.Close();
        //    return memstream.ToArray();
        //}

        //// decrypt
        //public static byte[] Decrypt(byte[] strData)
        //{
        //    PasswordDeriveBytes passbytes =
        //    new PasswordDeriveBytes(Global.strPermutation,
        //    new byte[] { Global.bytePermutation1,
        //                 Global.bytePermutation2,
        //                 Global.bytePermutation3,
        //                 Global.bytePermutation4
        //    });

        //    MemoryStream memstream = new MemoryStream();
        //    Aes aes = new AesManaged();
        //    aes.Key = passbytes.GetBytes(aes.KeySize / 8);
        //    aes.IV = passbytes.GetBytes(aes.BlockSize / 8);

        //    CryptoStream cryptostream = new CryptoStream(memstream,
        //    aes.CreateDecryptor(), CryptoStreamMode.Write);
        //    cryptostream.Write(strData, 0, strData.Length);
        //    cryptostream.Close();
        //    return memstream.ToArray();
        //}
    }
}
