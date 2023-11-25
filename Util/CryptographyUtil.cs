using System.Collections.Generic;
using System.IO;
using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace PullFinanceData.Util
{
    public static class CryptographyUtil
    {
        #region DES Cryptography

        private const string s_BinaryIV = "3C22A9B9";
        private const string s_BinaryKey = "C45FA5F0";

        /// <summary>
        ///     Text Encrypt / Decrypt is different with Binary Encrypt / Decrypt. They are not using same key and will screw up if
        ///     mismatched.
        /// </summary>
        private const string s_TextIV = "QA_Awifa";

        private const string s_TextKey = "Awifapdf";

        static CryptographyUtil()
        {
            InitialTripleKey();
        }

        private static void InitialTripleKey()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(AWDEnvironment.STripleKey))
                {
                    s_TripleKey = AWDEnvironment.STripleKey;
                }
                else
                {
                    var rkey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Morningstar\Common\TDES");
                    if (rkey != null) s_TripleKey = rkey.GetValue("key") as string;
                }
            }
            catch (Exception ex)
            {
                s_TripleKey = null;
                ExceptionUtil.DefaultHandleException(ex);
            }
        }

        public static string DESEncrypt(string data)
        {
            if (string.IsNullOrEmpty(data))
                return data;
            var datas = AWDEnvironment.s_DefaultEncoding.GetBytes(data);
            var results = DESEncrypt(datas);
            return Convert.ToBase64String(results);
        }

        public static string DESDecrypt(string data)
        {
            return DESDecrypt(data, AWDEnvironment.s_DefaultEncoding);
        }

        public static bool TryDESDecrypt(string data, out string decrptedData)
        {
            try
            {
                decrptedData = DESDecrypt(data, AWDEnvironment.s_DefaultEncoding);
                return true;
            }
            catch
            {
                decrptedData = null;
                return false;
            }
        }

        public static string DESDecrypt(string data, Encoding encoding)
        {
            if (string.IsNullOrEmpty(data))
                return data;
            var datas = Convert.FromBase64String(data);
            byte[] results = null;

            try
            {
                results = DESDecrypt(datas, true);
            }
            catch (Exception)
            {
                return data;
            }

            return encoding.GetString(results);
        }

        public static byte[] DESDecrypt(byte[] datas)
        {
            byte[] results = null;
            try
            {
                results = DESDecrypt(datas, false);
            }
            catch (Exception)
            {
                results = datas;
            }

            return results;
        }

        public static byte[] DESEncrypt(byte[] datas)
        {
            if (datas == null || datas.Length == 0)
                return null;
            byte[] res;
            using (var sa = GetSymmetricAlgorithm(true, true))
            {
                using (var ict = sa.CreateEncryptor())
                {
                    res = Transform(datas, ict);
                }

                sa.Clear();
            }

            var bytes = new List<byte>();
            bytes.AddRange(s_TripleHeader);
            bytes.AddRange(res);
            return bytes.ToArray();
        }

        private static byte[] DESDecrypt(byte[] datas, bool bText)
        {
            if (datas == null || datas.Length == 0)
                return null;

            var bytes = datas;
            var bTriple = false;
            if (datas.Length > 4 && datas[0] == s_TripleHeader[0] && datas[1] == s_TripleHeader[1] &&
                datas[2] == s_TripleHeader[2] && datas[3] == s_TripleHeader[3])
            {
                bTriple = true;
                bytes = new byte[datas.Length - 4];
                Array.Copy(datas, 4, bytes, 0, bytes.Length);
            }

            byte[] res;
            using (var sa = GetSymmetricAlgorithm(bTriple, bText))
            {
                using (var ict = sa.CreateDecryptor())
                {
                    res = Transform(bytes, ict);
                }

                sa.Clear();
            }

            return res;
        }

        private static SymmetricAlgorithm GetSymmetricAlgorithm(bool bTriple, bool bText)
        {
            if (bTriple)
            {
                if (s_TripleKey == null) InitialTripleKey();
                if (s_TripleKey == null)
                    throw new Exception("Unable to get s_TripleKey from registry");
                var pdb = new PasswordDeriveBytes(s_TripleKey, CreateRandomSalt(7));
                var provider = new TripleDESCryptoServiceProvider
                {
                    Mode = CipherMode.CBC,
                    IV = Convert.FromBase64String(s_TripleIV)
                };
                provider.Key = pdb.CryptDeriveKey("TripleDES", "SHA1", 192, provider.IV);
                return provider;
            }

            return new DESCryptoServiceProvider
            {
                Key = AWDEnvironment.s_DefaultEncoding.GetBytes(bText ? s_TextKey : s_BinaryKey),
                IV = AWDEnvironment.s_DefaultEncoding.GetBytes(bText ? s_TextIV : s_BinaryIV)
            };
        }

        private static byte[] Transform(byte[] datas, ICryptoTransform ict)
        {
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, ict, CryptoStreamMode.Write))
                {
                    cs.Write(datas, 0, datas.Length);
                    cs.FlushFinalBlock();
                }

                return ms.ToArray();
            }
        }

        #endregion

        #region TripleDES Cryptography

        private const string s_TripleIV = "0E1FFE0E67F=";
        private static string s_TripleKey;

        /// <summary>
        ///     Text Encrypt / Decrypt is different with Binary Encrypt / Decrypt. They are not using same key and will screw up if
        ///     mismatched.
        /// </summary>
        //private const string s_TripleTextIV = "9A64D8871B2=";
        //private const string s_TripleTextKey = "8D9FFCC920B84007B43DF5A367E2B0E4";
        private static readonly byte[] s_TripleHeader = { 0x13, 0x14, 0x15, 0x16 };

        private static byte[] CreateRandomSalt(int len)
        {
            var rng = new RNGCryptoServiceProvider();
            var rngByets = len >= 1 ? new byte[len] : new byte[1];

            rng.GetBytes(rngByets);
            return rngByets;
        }

        #endregion

        #region AES Cryptography

        private static RijndaelManaged rijndael;

        private static RijndaelManaged Rijndael
        {
            get
            {
                if (rijndael == null) rijndael = AESCreate();
                return rijndael;
            }
        }

        private static RijndaelManaged AESCreate()
        {
            var key = s_TripleKey;
            var iv = s_TripleKey.Substring(8, 16);
            return new RijndaelManaged
            {
                Key = AWDEnvironment.s_DefaultEncoding.GetBytes(key),
                IV = AWDEnvironment.s_DefaultEncoding.GetBytes(iv)
            };
        }

        public static string AESEncrypt(string data)
        {
            if (string.IsNullOrEmpty(data)) return null;
            var datas = AWDEnvironment.s_DefaultEncoding.GetBytes(data);
            var transform = Rijndael.CreateEncryptor();
            var result = transform.TransformFinalBlock(datas, 0, datas.Length);
            return Convert.ToBase64String(result);
        }

        public static string AESDecrypt(string data)
        {
            if (string.IsNullOrEmpty(data)) return null;
            var datas = Convert.FromBase64String(data);
            var transform = Rijndael.CreateDecryptor();
            var result = transform.TransformFinalBlock(datas, 0, datas.Length);
            return AWDEnvironment.s_DefaultEncoding.GetString(result);
        }

        #endregion
    }
}