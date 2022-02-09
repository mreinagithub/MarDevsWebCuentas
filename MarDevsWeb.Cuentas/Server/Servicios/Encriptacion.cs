using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace MarDevsWeb.Cuentas.Server.Servicios
{
    public static class Encriptacion
    {
        #region METODOS DE ENCRIPTACION

        private static string _SemillaEncriptacion = "_marDevs_seedEnc";

        private static string EncriptarDES(byte[] bytes, string clave)
        {
            string lClave = _SemillaEncriptacion + clave;
            DESCryptoServiceProvider miDes = new DESCryptoServiceProvider();
            System.Text.StringBuilder lDevolver = new System.Text.StringBuilder();

            #region Incrementar o truncar el tamaño de la Clave
            // La Clave para DES debe tener una longitud de 64 bits (8 bytes)
            while (lClave.Length < 8)
            {
                lClave += lClave;
            }
            if (lClave.Length > 8)
            {
                lClave = lClave.Substring(0, 8);
            }
            #endregion

            #region Create the crypto objects, with the key, as passed in
            miDes.Key = System.Text.ASCIIEncoding.ASCII.GetBytes(lClave);
            miDes.IV = System.Text.ASCIIEncoding.ASCII.GetBytes(lClave);

            MemoryStream miMS = new MemoryStream();
            CryptoStream miCS = new CryptoStream(miMS, miDes.CreateEncryptor(), CryptoStreamMode.Write);
            #endregion

            #region Write the byte array into the crypto stream (It will end up in the memory stream)
            miCS.Write(bytes, 0, bytes.Length);
            miCS.FlushFinalBlock();
            #endregion

            #region Get the data back from the memory stream, and into a string
            foreach (byte miByte in miMS.ToArray())
            {
                lDevolver.AppendFormat("{0:X2}", miByte);
            }
            #endregion

            return lDevolver.ToString();
        }
        private static string EncriptarDES(byte[] bytes)
        {
            return EncriptarDES(bytes, String.Empty);
        }
        public static string EncriptarDES(string texto, string clave)
        {
            byte[] Datos = System.Text.Encoding.UTF8.GetBytes(texto);
            return EncriptarDES(Datos, clave);
        }
        public static string EncriptarDES(string texto)
        {
            return EncriptarDES(texto, String.Empty);
        }
        private static string DecriptarDES(byte[] bytes, string clave)
        {
            string lClave = _SemillaEncriptacion + clave;
            DESCryptoServiceProvider miDes = new DESCryptoServiceProvider();
            System.Text.StringBuilder lDevolver = new System.Text.StringBuilder();

            #region Incrementar o truncar el tamaño de la Clave
            // La Clave para DES debe tener una longitud de 64 bits (8 bytes)
            while (lClave.Length < 8)
            {
                lClave += lClave;
            }
            if (lClave.Length > 8)
            {
                lClave = lClave.Substring(0, 8);
            }
            #endregion

            #region Create the crypto objects
            miDes.Key = System.Text.ASCIIEncoding.ASCII.GetBytes(lClave);
            miDes.IV = System.Text.ASCIIEncoding.ASCII.GetBytes(lClave);
            MemoryStream miMS = new MemoryStream();
            CryptoStream miCS = new CryptoStream(miMS, miDes.CreateDecryptor(), CryptoStreamMode.Write);
            #endregion

            #region Flush the data through the crypto stream into the memory stream
            miCS.Write(bytes, 0, bytes.Length);
            miCS.FlushFinalBlock();
            #endregion

            #region Get the decrypted data back from the memory stream
            foreach (byte miByte in miMS.ToArray())
            {
                lDevolver.Append((char)miByte);
            }
            #endregion

            return lDevolver.ToString();
        }
        private static string DecriptarDES(byte[] bytes)
        {
            return DecriptarDES(bytes, _SemillaEncriptacion);
        }
        public static string DecriptarDES(string texto, string clave)
        {
            if (texto == null)
            {
                return null;
            }
            int lLongitud = texto.Length / 2;
            byte[] Datos = new byte[lLongitud];

            for (int i = 0; i < lLongitud; i++)
            {
                int lIndividual = Convert.ToInt32(texto.Substring(i * 2, 2), 16);
                Datos[i] = (byte)lIndividual;
            }
            return DecriptarDES(Datos, clave);
        }
        public static string DecriptarDES(string texto)
        {
            return DecriptarDES(texto, _SemillaEncriptacion);
        }
        private static string EncriptarSHA(byte[] bytes, string clave)
        {
            string lClave = _SemillaEncriptacion + clave;
            byte[] miClave = System.Text.Encoding.UTF8.GetBytes(lClave);

            HMACSHA1 miHMAC = new HMACSHA1(miClave);
            CryptoStream cs = new CryptoStream(Stream.Null, miHMAC, CryptoStreamMode.Write);
            cs.Write(bytes, 0, bytes.Length);
            cs.Close();

            return Convert.ToBase64String(miHMAC.Hash);
        }
        private static string EncriptarSHA(byte[] bytes)
        {
            return EncriptarSHA(bytes, String.Empty);
        }
        public static string EncriptarSHA(string texto, string clave)
        {
            byte[] Datos = System.Text.Encoding.UTF8.GetBytes(texto);
            return EncriptarSHA(Datos, clave);
        }
        public static string EncriptarSHA(string texto)
        {
            return EncriptarSHA(texto, String.Empty);
        }


        #endregion   
    }
}
