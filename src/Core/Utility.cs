using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
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
	public class Utility : Core.Common
	{
		public static void CreateDirecotry(string file)
		{
			try
			{
				string dir = Core.IO.Path.GetDirectoryName(file);
				if (!Core.IO.Directory.Exists(dir))
				{
					Core.IO.Directory.CreateDirectory(dir);
				}
			}
			catch
			{
			}
		}

		public static Hashtable GetRelUsers(List<Message> msgs)
		{
			Hashtable infos = new Hashtable();
			foreach (Message msg in msgs)
			{
				if (msg.IsValid)
				{
					infos[msg.Sender.ID] = msg.Sender.DetailsJson;
					infos[msg.Receiver.ID] = msg.Receiver.DetailsJson;
				}
			}
				return infos;
		}

		public static Hashtable GetRelUsers(Message msg)
		{
			Hashtable infos = new Hashtable();
			if (msg.IsValid)
			{
				infos[msg.Sender.ID] = msg.Sender.DetailsJson;
				infos[msg.Receiver.ID] = msg.Receiver.DetailsJson;
			}
			return infos;
		}

		public static void FilterUserInfoHtml(Hashtable info)
		{
			if (info.ContainsKey("Nickname")) info["Nickname"] = HtmlUtil.ReplaceHtml(info["Nickname"].ToString());
			if (info.ContainsKey("Tel")) info["Tel"] = HtmlUtil.ReplaceHtml(info["Tel"].ToString());
			if (info.ContainsKey("Mobile")) info["Mobile"] = HtmlUtil.ReplaceHtml(info["Mobile"].ToString());
			if (info.ContainsKey("HomePage")) info["HomePage"] = HtmlUtil.ReplaceHtml(info["HomePage"].ToString());
			if (info.ContainsKey("Remark")) info["Remark"] = HtmlUtil.ReplaceHtml(info["Remark"].ToString());
		}

		public static int[] ParseIntArray(string ids)
		{
			List<int> all = new List<int>();
			foreach (string s in ids.Split(new char[] { ',' }))
			{
				all.Add(int.Parse(s));
			}
			return all.ToArray();
		}

		public static byte[] ToBytes(int[] ids)
		{
			byte[] buffer = new byte[ids.Length * 4];
			for (int s = 0, d = 0; s < ids.Length; s++, d++)
			{
				uint id = (uint)ids[s];
				buffer[d] = (byte)((id & 0xFF000000) >> 24);
				buffer[++d] = (byte)((id & 0x00FF0000) >> 16);
				buffer[++d] = (byte)((id & 0x0000FF00) >> 8);
				buffer[++d] = (byte)(id & 0x000000FF);
			}
			return buffer;
		}
	}
}