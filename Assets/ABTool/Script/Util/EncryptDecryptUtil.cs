using UnityEngine;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System;

/// <summary>
/// Encrypt-Decrypt Utility
/// </summary>
public class EncryptDecryptUtil
{
    /// <summary>
    /// Encrypts the string to string.
    /// </summary>
    /// <returns>The string to string.</returns>
    /// <param name="toEncrypt">String needed to encrypt.</param>
    /// <param name="password">Password.</param>
    public static string EncryptStringToString (string toEncrypt, string password)
    {
        byte[] keyArray = UTF8Encoding.UTF8.GetBytes (password);
        byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes (toEncrypt);
        RijndaelManaged rDel = new RijndaelManaged ();
        rDel.KeySize = 256;
        using (var md5 = MD5.Create ()) {
            rDel.Key = md5.ComputeHash (keyArray);
        }
        rDel.Mode = CipherMode.ECB;
        // http://msdn.microsoft.com/en-us/library/system.security.cryptography.ciphermode.aspx
        rDel.Padding = PaddingMode.PKCS7;
        // better lang support
        ICryptoTransform cTransform = rDel.CreateEncryptor ();
        byte[] resultArray = cTransform.TransformFinalBlock (toEncryptArray, 0, toEncryptArray.Length);
        return Convert.ToBase64String (resultArray, 0, resultArray.Length);
    }

    /// <summary>
    /// Encrypts the bytes to string.
    /// </summary>
    /// <returns>The bytes to string.</returns>
    /// <param name="toEncryptArray">Bytes[] needed to encrypt.</param>
    /// <param name="password">Password.</param>
    public static string EncryptBytesToString (byte[] toEncryptArray, string password)
    {
        byte[] keyArray = UTF8Encoding.UTF8.GetBytes (password);
        RijndaelManaged rDel = new RijndaelManaged ();
        rDel.KeySize = 256;
        using (var md5 = MD5.Create ()) {
            rDel.Key = md5.ComputeHash (keyArray);
        }
        rDel.Mode = CipherMode.ECB;
        // http://msdn.microsoft.com/en-us/library/system.security.cryptography.ciphermode.aspx
        rDel.Padding = PaddingMode.PKCS7;
        // better lang support
        ICryptoTransform cTransform = rDel.CreateEncryptor ();
        byte[] resultArray = cTransform.TransformFinalBlock (toEncryptArray, 0, toEncryptArray.Length);
        return Convert.ToBase64String (resultArray, 0, resultArray.Length);
    }

    /// <summary>
    /// Encrypts the bytes to bytes.
    /// </summary>
    /// <returns>The bytes to bytes.</returns>
    /// <param name="toEncryptArray">Bytes[] needed to encrypt.</param>
    /// <param name="password">Password.</param>
    public static byte[] EncryptBytesToBytes (byte[] toEncryptArray, string password)
    {
        byte[] keyArray = UTF8Encoding.UTF8.GetBytes (password);
        RijndaelManaged rDel = new RijndaelManaged ();
        rDel.KeySize = 256;
        using (var md5 = MD5.Create ()) {
            rDel.Key = md5.ComputeHash (keyArray);
        }
        rDel.Mode = CipherMode.ECB;
        // http://msdn.microsoft.com/en-us/library/system.security.cryptography.ciphermode.aspx
        rDel.Padding = PaddingMode.PKCS7;
        // better lang support
        ICryptoTransform cTransform = rDel.CreateEncryptor ();
        byte[] resultArray = cTransform.TransformFinalBlock (toEncryptArray, 0, toEncryptArray.Length);
        return resultArray;
    }

    /// <summary>
    /// Encrypts the file to file.
    /// </summary>
    /// <param name="filepathInput">Filepath input.</param>
    /// <param name="filepathOutput">Filepath output.</param>
    /// <param name="password">Password.</param>
    public static void EncryptFileToFile (string filepathInput, string filepathOutput, string password)
    {
        byte[] keyArray = UTF8Encoding.UTF8.GetBytes (password);
        byte[] toEncryptArray = File.ReadAllBytes (filepathInput);
        RijndaelManaged rDel = new RijndaelManaged ();
        rDel.KeySize = 256;
        using (var md5 = MD5.Create ()) {
            rDel.Key = md5.ComputeHash (keyArray);
        }
        rDel.Mode = CipherMode.ECB;
        // http://msdn.microsoft.com/en-us/library/system.security.cryptography.ciphermode.aspx
        rDel.Padding = PaddingMode.PKCS7;
        // better lang support
        ICryptoTransform cTransform = rDel.CreateEncryptor ();
        byte[] resultArray = cTransform.TransformFinalBlock (toEncryptArray, 0, toEncryptArray.Length);

        FileStream outStream = new FileStream (filepathOutput, FileMode.OpenOrCreate, FileAccess.Write);
        outStream.Write (resultArray, 0, resultArray.Length);
        outStream.Close ();
    }

    /// <summary>
    /// Decrypts the string to string.
    /// </summary>
    /// <returns>The string to string.</returns>
    /// <param name="toDecrypt">String needed to decrypt.</param>
    /// <param name="password">Password.</param>
    public static string DecryptStringToString (string toDecrypt, string password)
    {
        byte[] keyArray = UTF8Encoding.UTF8.GetBytes (password);
        byte[] toDecryptArray = Convert.FromBase64String (toDecrypt);
        RijndaelManaged rDel = new RijndaelManaged ();
        rDel.KeySize = 256;
        using (var md5 = MD5.Create ()) {
            rDel.Key = md5.ComputeHash (keyArray);
        }
        rDel.Mode = CipherMode.ECB;
        // http://msdn.microsoft.com/en-us/library/system.security.cryptography.ciphermode.aspx
        rDel.Padding = PaddingMode.PKCS7;
        // better lang support
        ICryptoTransform cTransform = rDel.CreateDecryptor ();
        byte[] resultArray = cTransform.TransformFinalBlock (toDecryptArray, 0, toDecryptArray.Length);
        return UTF8Encoding.UTF8.GetString (resultArray);
    }

    /// <summary>
    /// Decrypts the bytes to string.
    /// </summary>
    /// <returns>The bytes to string.</returns>
    /// <param name="toDecryptArray">Bytes[] needed to decrypt.</param>
    /// <param name="password">Password.</param>
    public static string DecryptBytesToString (byte[] toDecryptArray, string password)
    {
        byte[] keyArray = UTF8Encoding.UTF8.GetBytes (password);
        RijndaelManaged rDel = new RijndaelManaged ();
        rDel.KeySize = 256;
        using (var md5 = MD5.Create ()) {
            rDel.Key = md5.ComputeHash (keyArray);
        }
        rDel.Mode = CipherMode.ECB;
        // http://msdn.microsoft.com/en-us/library/system.security.cryptography.ciphermode.aspx
        rDel.Padding = PaddingMode.PKCS7;
        // better lang support
        ICryptoTransform cTransform = rDel.CreateDecryptor ();
        byte[] resultArray = cTransform.TransformFinalBlock (toDecryptArray, 0, toDecryptArray.Length);
        return UTF8Encoding.UTF8.GetString (resultArray);
    }

    /// <summary>
    /// Decrypts the bytes to bytes.
    /// </summary>
    /// <returns>The bytes to bytes.</returns>
    /// <param name="toDecryptArray">Bytes[] needed to decrypt.</param>
    /// <param name="password">Password.</param>
    public static byte[] DecryptBytesToBytes (byte[] toDecryptArray, string password)
    {
        byte[] keyArray = UTF8Encoding.UTF8.GetBytes (password);
        RijndaelManaged rDel = new RijndaelManaged ();
        rDel.KeySize = 256;
        using (var md5 = MD5.Create ()) {
            rDel.Key = md5.ComputeHash (keyArray);
        }
        rDel.Mode = CipherMode.ECB;
        // http://msdn.microsoft.com/en-us/library/system.security.cryptography.ciphermode.aspx
        rDel.Padding = PaddingMode.PKCS7;
        // better lang support
        ICryptoTransform cTransform = rDel.CreateDecryptor ();
        byte[] resultArray = cTransform.TransformFinalBlock (toDecryptArray, 0, toDecryptArray.Length);
        return resultArray;
    }

    /// <summary>
    /// Decrypts the file to file.
    /// </summary>
    /// <param name="filepathInput">Filepath input.</param>
    /// <param name="filepathOutput">Filepath output.</param>
    /// <param name="password">Password.</param>
    public static void DecryptFileToFile (string filepathInput, string filepathOutput, string password)
    {
        byte[] keyArray = UTF8Encoding.UTF8.GetBytes (password);
        byte[] toDecryptArray = File.ReadAllBytes (filepathInput);
        RijndaelManaged rDel = new RijndaelManaged ();
        rDel.KeySize = 256;
        using (var md5 = MD5.Create ()) {
            rDel.Key = md5.ComputeHash (keyArray);
        }
        rDel.Mode = CipherMode.ECB;
        // http://msdn.microsoft.com/en-us/library/system.security.cryptography.ciphermode.aspx
        rDel.Padding = PaddingMode.PKCS7;
        // better lang support
        ICryptoTransform cTransform = rDel.CreateDecryptor ();
        byte[] resultArray = cTransform.TransformFinalBlock (toDecryptArray, 0, toDecryptArray.Length);

        FileStream outStream = new FileStream (filepathOutput, FileMode.OpenOrCreate, FileAccess.Write);
        outStream.Write (resultArray, 0, resultArray.Length);
        outStream.Close ();
    }

    private static System.Random random = new System.Random ();
    /// <summary>
    /// Randoms the string with length
    /// </summary>
    /// <returns>The string.</returns>
    /// <param name="length">Length.</param>
    public static string RandomString (int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789#$%&*!";
        return new string (Enumerable.Repeat (chars, length)
            .Select (s => s [random.Next (s.Length)]).ToArray ());
    }

    /// <summary>
    /// Computes the hash for asset.
    /// </summary>
    /// <returns>The hash for asset.</returns>
    /// <param name="assetPath">Asset path.</param>
    public static string ComputeHashForAsset (string assetPath)
    {
        string retString = "";

        string assetFullPath = assetPath;
        using (var md5 = MD5.Create ()) {
            using (var stream = File.OpenRead (assetFullPath)) {
                retString += Encoding.Default.GetString (md5.ComputeHash (stream));
            }
        }

        string assetMetaFile = Path.GetFileNameWithoutExtension (assetFullPath) + ".meta";
        if (File.Exists (assetMetaFile)) {
            using (var md5 = MD5.Create ()) {
                using (var stream = File.OpenRead (assetFullPath)) {
                    retString += Encoding.Default.GetString (md5.ComputeHash (stream));
                }
            }
        }
        return retString;
    }
}
