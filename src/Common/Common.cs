using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Net.Json;
using System.Drawing;
using System.Drawing.Imaging;

namespace Core
{
	public class Common
	{
		static DateTime BaseDateTime = new DateTime(1970, 1, 1, 0, 0, 0);

		public static String RenderJson(object obj)
		{
			StringBuilder builder = new StringBuilder();
			RenderJson(builder, obj);
			return builder.ToString();
		}

		public static void RenderJson(StringBuilder builder, object obj)
		{
			if (obj == null)
			{
				builder.Append("null");
			}
			else if (obj is IRenderJson)
			{
				(obj as IRenderJson).RenderJson(builder);
			}
			else if (obj is Exception)
			{
				builder.AppendFormat("{{\"__DataType\":\"Exception\",\"__Value\":{{\"Name\":\"{0}\",\"Message\":\"{1}\"}}}}", obj.GetType().Name, Common.TransferCharJavascript((obj as Exception).Message));
			}
			else if (obj.GetType() == typeof(DateTime))
			{
				DateTime val = (DateTime)obj;
				RenderHashJson(
					builder,
					"__DataType", "Date",
					"__Value", (val - BaseDateTime).TotalMilliseconds
				);
			}
			else if (obj is IDictionary)
			{
				int count = 0;
				builder.Append("{");
				foreach (DictionaryEntry ent in (obj as IDictionary))
				{
					if (count > 0) builder.Append(",");
					builder.AppendFormat("\"{0}\":", Common.TransferCharJavascript(ent.Key.ToString()));
					RenderJson(builder, ent.Value);
					count++;
				}
				builder.Append("}");
			}
			else if (obj is IList)
			{
				IList list = obj as IList;
				builder.Append("[");
				for (int i = 0; i < list.Count; i++)
				{
					if (i > 0) builder.Append(",");
					RenderJson(builder, list[i]);

				}
				builder.Append("]");
			}
			else if (obj is ICollection)
			{
				ICollection list = obj as ICollection;
				builder.Append("[");
				int count = 0;
				foreach (object val in list)
				{
					if (count > 0) builder.Append(",");
					RenderJson(builder, val);
					count++;
				}
				builder.Append("]");
			}
			else if (obj is DataRow)
			{
				DataRow row = obj as DataRow;
				builder.Append("{");

				int count = 0;
				foreach (DataColumn column in row.Table.Columns)
				{
					if (count > 0) builder.Append(",");
					builder.AppendFormat("\"{0}\":", column.ColumnName);
					RenderJson(builder, row[column.ColumnName]);
					count++;
				}

				builder.Append("}");
			}
			else if (obj is System.Drawing.Rectangle)
			{
				System.Drawing.Rectangle rect = (System.Drawing.Rectangle)obj;

				RenderHashJson(
					builder,
					"Left", rect.Left,
					"Top", rect.Top,
					"Width", rect.Width,
					"Height", rect.Height
				);
			}
			else if (obj is UInt16 || obj is UInt32 || obj is UInt64 || obj is Int16 || obj is Int32 || obj is Int64 || obj is Double || obj is Decimal || obj is long)
			{
				builder.Append(obj.ToString());
			}
			else if (obj is bool)
			{
				builder.Append((bool)obj ? "true" : "false");
			}
			else
			{
				builder.Append("\"");
				builder.Append(TransferCharJavascript(obj.ToString()));
				builder.Append("\"");
			}
		}

		public static String RenderHashJson(params object[] ps)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("{");
			for (int i = 0; i < ps.Length; i += 2)
			{
				if (i > 0) builder.Append(",");
				builder.AppendFormat("\"{0}\":", ps[i].ToString());
				RenderJson(builder, ps[i + 1]);
			}
			builder.Append("}");
			return builder.ToString();
		}

		public static void RenderHashJson(StringBuilder builder, params object[] ps)
		{
			builder.Append("{");
			for (int i = 0; i < ps.Length; i += 2)
			{
				if (i > 0) builder.Append(",");
				builder.AppendFormat("\"{0}\":", ps[i].ToString());
				RenderJson(builder, ps[i + 1]);
			}
			builder.Append("}");
		}

		public static string TransferCharJavascript(string s)
		{
			StringBuilder ret = new StringBuilder();
			foreach (char c in s)
			{
				switch (c)
				{
				case '\r':
				case '\t':
				case '\n':
				case '\f':
				case '\v':
				case '\"':
				case '\\':
				case '\'':
				case '<':
				case '>':
				case '&':
				case '\0':
					ret.AppendFormat("\\u{0:X4}", (int)c);
					break;
				default:
					ret.Append(c);
					break;
				}
			}
			return ret.ToString();
		}

		public static string TransferCharForXML(string s)
		{
			StringBuilder ret = new StringBuilder();
			foreach (char c in s)
			{
				switch (c)
				{
				case '\r':
				case '\t':
				case '\n':
				case '\f':
				case '\v':
				case '\"':
				case '\\':
				case '\'':
				case '<':
				case '>':
					ret.AppendFormat("&#{0};", (int)c);
					break;
				default:
					ret.Append(c);
					break;
				}
			}
			return ret.ToString();
		}

		public static string Decode(string s)
		{
			Regex r = new Regex("\\\\u[0-9a-fA-F]{4}|\\\\x[0-9a-fA-F]{2}");
			MatchEvaluator eval = new MatchEvaluator(ReplaceChar);
			return r.Replace(s, eval);
		}

		public static string ReplaceChar(Match match)
		{
			ushort ascii;
			if (match.Length == 4)
			{
				ascii = ushort.Parse(match.Value.Substring(2, 2), NumberStyles.HexNumber);
			}
			else
			{
				ascii = ushort.Parse(match.Value.Substring(2, 4), NumberStyles.HexNumber);
			}
			char c = Convert.ToChar(ascii);
			return c.ToString();
		}

		public static string MD5(string str)
		{
			MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
			byte[] data = md5.ComputeHash(UTF8Encoding.Default.GetBytes(str));
			StringBuilder sBuilder = new StringBuilder();
			for (int i = 0; i < data.Length; i++)
			{
				sBuilder.Append(data[i].ToString("X2"));
			}
			return sBuilder.ToString();
		}

		static public Bitmap ToGray(Bitmap bmp, int mode)
		{
			if (bmp == null)
			{
				return null;
			}

			int w = bmp.Width;
			int h = bmp.Height;
			try
			{
				byte newColor = 0;
				BitmapData srcData = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
				unsafe
				{
					byte* p = (byte*)srcData.Scan0.ToPointer();
					for (int y = 0; y < h; y++)
					{
						for (int x = 0; x < w; x++)
						{

							if (mode == 0)　// 加权平均
							{
								newColor = (byte)((float)p[0] * 0.114f + (float)p[1] * 0.587f + (float)p[2] * 0.299f);
							}
							else　　　　// 算数平均
							{
								newColor = (byte)((float)(p[0] + p[1] + p[2]) / 3.0f);
							}
							p[0] = newColor;
							p[1] = newColor;
							p[2] = newColor;

							p += 3;
						}
						p += srcData.Stride - w * 3;
					}
					bmp.UnlockBits(srcData);
					return bmp;
				}
			}
			catch
			{
				return null;
			}

		}

		public static Object ParseJson(string jsonText)
		{
			if (jsonText == "{}") return new Hashtable();
			if (!string.IsNullOrEmpty(jsonText))
			{
				JsonTextParser parser = new JsonTextParser();
				JsonObject obj = parser.Parse(jsonText);
				return ParseJson(obj);
			}
			else
			{
				return null;
			}
		}

		private static Object ParseJson(JsonObject jsonObject)
		{
			Type type = jsonObject.GetType();
			if (type == typeof(JsonObjectCollection))
			{
				Hashtable val = new Hashtable();
				foreach (JsonObject subObj in (jsonObject as JsonObjectCollection))
				{
					val.Add(subObj.Name, ParseJson(subObj));
				}
				if (val.ContainsKey("__DataType"))
				{
					if (val["__DataType"] as string == "Date")
					{
						return BaseDateTime.AddMilliseconds((Double)val["__Value"]);
					}
					else if (val["__DataType"] as string == "Exception")
					{
						return new Exception((val["__Value"] as Hashtable)["Message"] as string);
					}
					else
					{
						return val;
					}
				}
				else
				{
					return val;
				}
			}
			else if (type == typeof(JsonArrayCollection))
			{
				List<object> val = new List<object>();
				foreach (JsonObject subObj in (jsonObject as JsonArrayCollection))
				{
					val.Add(ParseJson(subObj));
				}
				return val.ToArray();
			}
			else if (type == typeof(JsonBooleanValue))
			{
				return jsonObject.GetValue();
			}
			else if (type == typeof(JsonStringValue))
			{
				return jsonObject.GetValue();
			}
			else if (type == typeof(JsonNumericValue))
			{
				return jsonObject.GetValue();
			}
			else
				return null;
		}
	}

	public interface IRenderJson
	{
		void RenderJson(StringBuilder builder);
	}

	public class JsonText : IRenderJson
	{
		static JsonText m_EmptyObject = new JsonText("{}");

		static public JsonText EmptyObject
		{
			get { return m_EmptyObject; }
		}

		static JsonText m_EmptyArray = new JsonText("[]");

		static public JsonText EmptyArray
		{
			get { return m_EmptyArray; }
		}

		static JsonText m_Null = new JsonText("null");

		static public JsonText Null
		{
			get { return m_Null; }
		}

		String m_Value;

		public JsonText(String value)
		{
			m_Value = value;
		}

		void IRenderJson.RenderJson(StringBuilder builder)
		{
			builder.Append(m_Value == null ? "null" : m_Value);
		}
	}

	public static class HtmlUtil
	{
		static Hashtable AllowHtmlTag = new Hashtable();
		static Regex HtmlTagRegex = new Regex(@"<(\/|)([^ \f\n\r\t\v\<\>\/]+)(\s[^\<\>]*|)(\/|)>");
		static Regex ImpermitWordRegex = new Regex("([^a-zA-Z1-9_])(on|expression|javascript)", RegexOptions.IgnoreCase);

		static HtmlUtil()
		{
			AllowHtmlTag.Add("I", "I");
			AllowHtmlTag.Add("B", "B");
			AllowHtmlTag.Add("U", "U");
			AllowHtmlTag.Add("P", "P");
			AllowHtmlTag.Add("TH", "TH");
			AllowHtmlTag.Add("TD", "TD");
			AllowHtmlTag.Add("TR", "TR");
			AllowHtmlTag.Add("OL", "OL");
			AllowHtmlTag.Add("UL", "UL");
			AllowHtmlTag.Add("LI", "LI");
			AllowHtmlTag.Add("BR", "BR");
			AllowHtmlTag.Add("H1", "H1");
			AllowHtmlTag.Add("H2", "H2");
			AllowHtmlTag.Add("H3", "H3");
			AllowHtmlTag.Add("H4", "H4");
			AllowHtmlTag.Add("H5", "H5");
			AllowHtmlTag.Add("H6", "H6");
			AllowHtmlTag.Add("H7", "H7");
			AllowHtmlTag.Add("EM", "EM");
			AllowHtmlTag.Add("PRE", "PRE");
			AllowHtmlTag.Add("DIV", "DIV");
			AllowHtmlTag.Add("IMG", "IMG");
			AllowHtmlTag.Add("CITE", "CITE");
			AllowHtmlTag.Add("SPAN", "SPAN");
			AllowHtmlTag.Add("FONT", "FONT");
			AllowHtmlTag.Add("CODE", "CODE");
			AllowHtmlTag.Add("TABLE", "TABLE");
			AllowHtmlTag.Add("TBODY", "TBODY");
			AllowHtmlTag.Add("SMALL", "SMALL");
			AllowHtmlTag.Add("THEAD", "THEAD");
			AllowHtmlTag.Add("CENTER", "CENTER");
			AllowHtmlTag.Add("STRONG", "STRONG");
			AllowHtmlTag.Add("BLOCKQUOTE", "BLOCKQUOTE");
		}

		static public String ReplaceHtml(String text)
		{
			return HtmlTagRegex.Replace(text, ReplaceHtmlTag);
		}

		static public string ReplaceHtmlTag(Match match)
		{
			if (match.Groups[1].Value == "/")
			{
				if (AllowHtmlTag.ContainsKey(match.Groups[2].Value.ToUpper()))
				{
					return match.Value;
				}
				else
				{
					return "&lt;/" + match.Groups[2].Value + "&gt;";
				}
			}
			else
			{
				if (AllowHtmlTag.ContainsKey(match.Groups[2].Value.ToUpper()))
				{
					return ImpermitWordRegex.Replace(match.Value, ReplaceImpermitWord);
				}
				else
				{
					return "&lt;" + match.Groups[2].Value + "&gt;";
				}
			}
		}

		static public string ReplaceImpermitWord(Match match)
		{
			return match.Groups[1].Value + "_" + match.Groups[2].Value;
		}
	}
}