using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace SB1Util.Misc
{
    public class Crypto
    {




        /*
         * Criptografa um texto utilizando o MD5 e uma chave para criptografar.
         * A chave deverá ser utilizada para descriptografar o texto posteriormente.
         * @Return texto Criptografado ou "" em caso de erro.
         */
        public string cipherPassMD5(string pass, string key)
        {
            string ret = "";
            try
            {
                TripleDESCryptoServiceProvider cipherPass = new TripleDESCryptoServiceProvider();
                MD5CryptoServiceProvider cryptoMd5 = new MD5CryptoServiceProvider();

                byte[] byteHash, byteBuff;
                string strTempKey = key;

                byteHash = cryptoMd5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(strTempKey));
                cryptoMd5 = null;
                cipherPass.Key = byteHash;
                cipherPass.Mode = CipherMode.ECB;

                byteBuff = ASCIIEncoding.ASCII.GetBytes(pass);
                ret = Convert.ToBase64String(cipherPass.CreateEncryptor().TransformFinalBlock(byteBuff, 0, byteBuff.Length));
            }
            catch (Exception ex)
            {
                ret = "";
            }

            return ret;
        }


        /*
         * Descriptografa um texto criptografado em MD5 utilizando 
         * a mesma chave utilizada para criptografar.
         * @Return texto Descriptografado ou "" em caso de erro.
         */ 
        public string decryptPassMD5(string cipher, string key)
        {
            string pass = "";
            try
            {
                TripleDESCryptoServiceProvider decriptPass = new TripleDESCryptoServiceProvider();
                MD5CryptoServiceProvider cryptoMd5 = new MD5CryptoServiceProvider();

                byte[] byteHash, byteBuff;
                string strTempKey = key;

                byteHash = cryptoMd5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(strTempKey));
                cryptoMd5 = null;
                decriptPass.Key = byteHash;
                decriptPass.Mode = CipherMode.ECB;

                byteBuff = Convert.FromBase64String(cipher);
                string strDecrypted = ASCIIEncoding.ASCII.GetString(decriptPass.CreateDecryptor().TransformFinalBlock(byteBuff, 0, byteBuff.Length));
                decriptPass = null;

                pass = strDecrypted;
            }
            catch (Exception ex)
            {
                pass = "";
            }
            return pass;
        }


        /*
         * Criptografa o texto passado por parametro utilizando a forma tradicional do MD5.
         * @Return Texto Criptografado ou o texto passado por parametro em caso de erro.
         */ 
        public string getMD5Hash(string text)
        {
            try
            {
                MD5 md5 = MD5.Create();

                byte[] bytes = Encoding.ASCII.GetBytes(text);
                bytes = md5.ComputeHash(bytes);
                text = "";
                foreach (byte b in bytes)
                {
                    text += b.ToString("x2");
                }
            }
            catch (Exception e)
            {
            }
            return text;
        }


        /*
         * Realiza a verificao de um hash em relacao a um texto para 
         * saber o hash eh produto do texto passado.
         * @Return true caso o hash seja foi criado atraves do texto ou
         * fase caso contrario ou ocorra algum erro.
         */
        public bool verifyMD5HashText(string hash, string text)
        {
            bool ret = false;
            try
            {
                text = getMD5Hash(text);
                StringComparer comp = StringComparer.OrdinalIgnoreCase;
                if (comp.Compare(hash, text) == 0)
                {
                    ret = true;
                }
            }
            catch (Exception e)
            {
                ret = false;
            }
            return ret;
        }



    }
}
