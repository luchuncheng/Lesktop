using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Json;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;

namespace Client
{

	public class Utility : Core.Common
	{
		static public HtmlElement Find(HtmlElement elem, string classPath)
		{
			string[] cpn = classPath.Split(new char[] { ',' });
			return Find(elem, cpn, 0);
		}

		static public HtmlElement Find(HtmlElement elem, string[] cpn, int first)
		{
			for (int i = 0; i < elem.Children.Count; i++)
			{
				object dom = elem.Children[i].DomElement;

				string className = (string)dom.GetType().InvokeMember("className", BindingFlags.GetProperty, null, dom, new object[0]);

				if (String.Compare(className, cpn[first], true) == 0)
				{
					return first == cpn.Length - 1 ? elem.Children[i] : Find(elem.Children[i], cpn, first + 1);
				}
			}

			return null;
		}

		static public object GetProperty(object dom, string name)
		{
			return dom.GetType().InvokeMember(name, BindingFlags.GetProperty, null, dom, new object[0]);
		}

		static public object SetProperty(object dom, string name, object value)
		{
			return dom.GetType().InvokeMember(name, BindingFlags.SetProperty, null, dom, new object[] { value });
		}

		static public object InvokeMethod(object obj, string name, params object[] args)
		{
			if (obj == null) return null;
			return obj.GetType().InvokeMember(name, BindingFlags.InvokeMethod, null, obj, args);
		}

		static public object InvokeMethod2(object obj, string name, object[] args)
		{
			if (obj == null) return null;
			return obj.GetType().InvokeMember(name, BindingFlags.InvokeMethod, null, obj, args);
		}

		static public object CallFunc(object func, params object[] args)
		{
			List<object> ps = new List<object>();
			ps.Add(func);
			ps.AddRange(args);

			return func.GetType().InvokeMember("call", BindingFlags.InvokeMethod, null, func, ps.ToArray());
		}

		public static String GetSelectionHtml(object evt)
		{
			object srcElement = Utility.GetProperty(evt, "srcElement");

			object doc = Utility.GetProperty(srcElement, "document");
			if (doc == null) return String.Empty;

			object selection = Utility.GetProperty(doc, "selection");
			if (selection == null) return String.Empty;

			object range = Utility.InvokeMethod(selection, "createRange");
			if (range == null) return String.Empty;

			object htmlText = Utility.GetProperty(range, "htmlText");
			if (htmlText == null) return String.Empty;

			return htmlText.ToString();
		}
	}

	public static class HtmlUtil
	{
		static Hashtable AllowHtmlTag = new Hashtable();
		static Regex HtmlTagRegex = new Regex(@"<(\/|)([^ \f\n\r\t\v\<\>]+)(\s[^\<\>]*|)>");
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
