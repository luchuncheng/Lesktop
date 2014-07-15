using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;

namespace Custom
{
	public class CustomServerImpl
	{
		private static byte[] Keys = { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88 };

		public static string EncryptPassword(string pwd)
		{
			return Core.Common.MD5(pwd);
		}

		public static void AddCookie(HttpContext context, string sessionId, Int64 id, string[] roles, Nullable<DateTime> expires, bool clientMode)
		{
			StringBuilder roles_string = new StringBuilder();
			foreach (string role in roles)
			{
				if (roles_string.Length > 0) roles_string.Append(',');
				roles_string.Append(role);
			}

			HttpCookie id_cookie = new HttpCookie(clientMode ? "LesktopIDC" : "LesktopID");
			id_cookie.Value = CookieEncrypt.EncryptDES(Keys, id.ToString());
			if (!clientMode && expires != null) id_cookie.Expires = expires.Value;
			if (context.Response.Cookies[id_cookie.Name] != null) context.Response.Cookies.Remove(id_cookie.Name);
			context.Response.Cookies.Add(id_cookie);
		}

		public static void RemoveCookie(HttpContext context)
		{
			if (context.Response.Cookies["LesktopID"] != null) context.Response.Cookies["LesktopID"].Expires = DateTime.Now.AddDays(-1);
		}

		public static int GetUserID(HttpContext context)
		{
			if (context.Request.Cookies["LesktopIDC"] != null) return Convert.ToInt32(CookieEncrypt.DecryptDES(Keys, context.Request.Cookies["LesktopIDC"].Value));
			if (context.Request.Cookies["LesktopID"] != null) return Convert.ToInt32(CookieEncrypt.DecryptDES(Keys, context.Request.Cookies["LesktopID"].Value));
			return 0;
		}

		public static void AfterCreateUser(object serverImpl, object accountImpl, object sessionImpl, object newUser)
		{
		}
	}

	public class CookieEncrypt
	{
		private static byte[] Keys = { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88 };
		/// <summary>  
		/// DES加密字符串  
		/// </summary>  
		/// <param name="encryptString">待加密的字符串</param>  
		/// <param name="encryptKey">加密密钥,要求为8位</param>  
		/// <returns>加密成功返回加密后的字符串，失败返回源串</returns>  
		public static string EncryptDES(byte[] rgbKey, string encryptString)
		{
			try
			{
				byte[] rgbIV = Keys;
				byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);
				DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
				MemoryStream mStream = new MemoryStream();
				CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
				cStream.Write(inputByteArray, 0, inputByteArray.Length);
				cStream.FlushFinalBlock();
				return Convert.ToBase64String(mStream.ToArray());
			}
			catch
			{
				return encryptString;
			}
		}
		/// <summary>  
		/// DES解密字符串  
		/// </summary>  
		/// <param name="decryptString">待解密的字符串</param>  
		/// <param name="decryptKey">解密密钥,要求为8位,和加密密钥相同</param>  
		/// <returns>解密成功返回解密后的字符串，失败返源串</returns>  
		public static string DecryptDES(byte[] rgbKey, string decryptString)
		{
			try
			{
				byte[] rgbIV = Keys;
				byte[] inputByteArray = Convert.FromBase64String(decryptString);
				DESCryptoServiceProvider DCSP = new DESCryptoServiceProvider();
				MemoryStream mStream = new MemoryStream();
				CryptoStream cStream = new CryptoStream(mStream, DCSP.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
				cStream.Write(inputByteArray, 0, inputByteArray.Length);
				cStream.FlushFinalBlock();
				return Encoding.UTF8.GetString(mStream.ToArray());
			}
			catch
			{
				return decryptString;
			}
		}
	}
}
