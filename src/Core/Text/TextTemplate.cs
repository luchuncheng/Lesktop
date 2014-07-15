using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.IO;

namespace Core.Text
{
	/// <summary>
	/// 表示一个文本模板，该类使用一个字符串作为模板，通过将模板中的标志替换为对应的值（模板中的标志用 {标志名} 表示）生成新的文本
	/// </summary>
	/// <example>以下代码用文本模板生成IMG标志
	/// <code>
	/// static class Program
	/// {
	///     [STAThread]
	///     static void Main()
	///     {
	///         TextTemplate temp = new TextTemplate("&lt;img src='{src}' alt='{alt}' /&gt;");
	///         Console.WriteLine(temp.Render("pic.bmp","Image"));
	///         Hashtable values = new Hashtable();
	///         values.Add("src", "pic.bmp");
	///         values.Add("alt", "image");
	///         Console.WriteLine(temp.Render(values));
	///     }
	/// }
	/// 
	/// 输出为：
	/// &lt;img src='pic.bmp' alt='Image' /&gt;
	/// &lt;img src='pic.bmp' alt='image' /&gt;
	/// 
	/// </code>
	/// </example>
	public class TextTemplate
	{
		TextTemplateTag[] _tags;
		String[] _contentParts;
		int _tagCount;

		private TextTemplate()
		{
			_tagCount = 0;
			_tags = null;
			_contentParts = null;
		}

		/// <summary>
		/// 用指定的模板初始化TextTemplate
		/// </summary>
		/// <param name="content">模板内容</param>
		public TextTemplate(String content)
		{
			FromString(content);
		}

		/// <summary>
		/// 用指定的模板初始化TextTemplate,模板内容重文件读入
		/// </summary>
		/// <param name="file">模板文件位置</param>
		/// <param name="encoding">文件使用的编码</param>
		public TextTemplate(string file, Encoding encoding)
		{
			StreamReader sr = new StreamReader(file, encoding);
			try
			{
				string content = sr.ReadToEnd();
				FromString(content);
			}
			catch (Exception)
			{
				sr.Close();
				throw;
			}
			sr.Close();
		}

		private void FromString(String content)
		{
			MatchCollection mc = Regex.Matches(content, "{\\w+}");
			_tagCount = mc.Count;
			_tags = new TextTemplateTag[mc.Count];
			_contentParts = new string[mc.Count + 1];
			int index = 0;
			foreach (Match m in mc)
			{
				_tags[index++] = new TextTemplateTag(m.Value.Substring(1, m.Value.Length - 2), m.Index, m.Length);
			}
			int start = 0;
			index = 0;
			foreach (TextTemplateTag con in _tags)
			{
				_contentParts[index] = content.Substring(start, con.Position - start);
				start = con.Position + con.Length;
				index++;
			}
			if (start < content.Length) _contentParts[index] = content.Substring(start);
		}

		/// <summary>
		/// 用指定的值生成文本
		/// </summary>
		/// <param name="values">各标志对应的值（用标志名作为key）</param>
		/// <returns>生成的文本</returns>
		/// <example>以下代码用文本模板生成IMG标志
		/// <code>
		/// static class Program
		/// {
		///     [STAThread]
		///     static void Main()
		///     {
		///         TextTemplate temp = new TextTemplate("&lt;img src='{src}' alt='{alt}' /&gt;");
		///         Console.WriteLine(temp.Render("pic.bmp","Image"));
		///         Hashtable values = new Hashtable();
		///         values.Add("src", "pic.bmp");
		///         values.Add("alt", "image");
		///         Console.WriteLine(temp.Render(values));
		///     }
		/// }
		/// 
		/// 输出为：
		/// &lt;img src='pic.bmp' alt='Image' /&gt;
		/// &lt;img src='pic.bmp' alt='image' /&gt;
		/// 
		/// </code>
		/// </example>
		public string Render(Hashtable values)
		{
			StringBuilder result = new StringBuilder(8 * 1024);
			int i = 0;
			for (i = 0; i < _tagCount; i++)
			{
				result.Append(_contentParts[i]);
				if (values[_tags[i].Name] != null)
					result.Append(values[_tags[i].Name]);
				else
					result.Append("{" + _tags[i].Name + "}");
			}
			result.Append(_contentParts[i]);
			return result.ToString();
		}

		/// <summary>
		/// 用指定的值生成文本
		/// </summary>
		/// <param name="args">各标志对应的值(忽略标志名，第一个标志对应第一个参数，以此类推)</param>
		/// <returns>生成的文本</returns>
		/// <example>以下代码用文本模板生成IMG标志
		/// <code>
		/// static class Program
		/// {
		///     [STAThread]
		///     static void Main()
		///     {
		///         TextTemplate temp = new TextTemplate("&lt;img src='{src}' alt='{alt}' /&gt;");
		///         Console.WriteLine(temp.Render("pic.bmp","Image"));
		///         Hashtable values = new Hashtable();
		///         values.Add("src", "pic.bmp");
		///         values.Add("alt", "image");
		///         Console.WriteLine(temp.Render(values));
		///     }
		/// }
		/// 
		/// 输出为：
		/// &lt;img src='pic.bmp' alt='Image' /&gt;
		/// &lt;img src='pic.bmp' alt='image' /&gt;
		/// 
		/// </code>
		/// </example>
		public string Render(params object[] args)
		{
			StringBuilder result = new StringBuilder(2 * 1024);
			int i = 0;
			for (i = 0; i < _tagCount; i++)
			{
				result.Append(_contentParts[i]);
				result.Append(args[i].ToString());
			}
			result.Append(_contentParts[i]);
			return result.ToString();
		}

		/// <summary>
		/// 用指定的值生成文本，并保存到文件中
		/// </summary>
		/// <param name="file">要保存的文件路径</param>
		/// <param name="encoding">文件的编码</param>
		/// <param name="values">各标志对应的值（用标志名作为key）</param>
		public void SaveAs(string file, Encoding encoding, Hashtable values)
		{
			StreamWriter sw = new StreamWriter(file, false, encoding);
			try
			{
				String content = Render(values);
				sw.Write(content);
			}
			catch (Exception)
			{
				sw.Close();
				throw;
			}
			sw.Close();
		}

		/// <summary>
		/// 用指定的值生成文本，并保存到文件中
		/// </summary>
		/// <param name="file">要保存的文件路径</param>
		/// <param name="encoding">文件的编码</param>
		/// <param name="args">各标志对应的值(忽略标志名，第一个标志对应第一个参数，以此类推)</param>
		public void SaveAs(string file, Encoding encoding, params object[] args)
		{
			StreamWriter sw = new StreamWriter(file, false, encoding);
			try
			{
				String content = Render(args);
				sw.Write(content);
			}
			catch (Exception)
			{
				sw.Close();
				throw;
			}
			sw.Close();
		}

		/// <summary>
		/// 将模板以指定的分隔标志分隔成小模板
		/// </summary>
		/// <param name="splitTag"></param>
		/// <returns></returns>
		public TextTemplate[] Split(string splitTag)
		{
			List<TextTemplate> temps = new List<TextTemplate>();
			List<string> contentParts = new List<string>();
			List<TextTemplateTag> tags = new List<TextTemplateTag>();
			int i = 0;
			foreach (string content in _contentParts)
			{
				contentParts.Add(content);
				if (i >= _tags.Length || _tags[i].Name == splitTag)
				{
					TextTemplate newTemp = new TextTemplate();
					newTemp._contentParts = contentParts.ToArray();
					newTemp._tags = tags.ToArray();
					newTemp._tagCount = tags.Count;
					temps.Add(newTemp);
					contentParts.Clear();
					tags.Clear();
				}
				else
					tags.Add(new TextTemplateTag(_tags[i].Name, _tags[i].Position, _tags[i].Length));
				i++;
			}
			return temps.ToArray();
		}
	}

	internal class TextTemplateTag
	{
		int _position, _length;
		string _name;

		public TextTemplateTag(string name, int pos, int len)
		{
			_name = name;
			_position = pos;
			_length = len;
		}

		public string Name
		{
			get { return _name; }
		}

		public int Position
		{
			get { return _position; }
		}

		public int Length
		{
			get { return _length; }
		}
	}
}