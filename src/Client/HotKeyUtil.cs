using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Client
{
	public enum HotKeyModifiers : uint
	{
		MOD_ALT = 0x1,
		MOD_CONTROL = 0x2,
		MOD_SHIFT = 0x4,
		MOD_WIN = 0x8,
	}

	public class HotKey : Core.IRenderJson
	{
		public bool CtrlKey, ShiftKey, AltKey;
		public uint KeyCode;

		public HotKey(bool ctrl, bool shift, bool alt, uint vk)
		{
			CtrlKey = ctrl;
			ShiftKey = shift;
			AltKey = alt;
			KeyCode = vk;
		}

		public HotKey(string json)
		{
			Hashtable json_val = Utility.ParseJson(json) as Hashtable;
			if (json_val == null) return;

			CtrlKey = Convert.ToBoolean(json_val["CtrlKey"]);
			ShiftKey = Convert.ToBoolean(json_val["ShiftKey"]);
			AltKey = Convert.ToBoolean(json_val["AltKey"]);
			KeyCode = Convert.ToUInt32(json_val["KeyCode"]);
		}

		public override string ToString()
		{
			string text = "";
			if (CtrlKey) text += "Ctrl + ";
			if (ShiftKey) text += "Shift + ";
			if (AltKey) text += "Alt + ";
			text += Convert.ToChar(KeyCode);
			return text;
		}

		void Core.IRenderJson.RenderJson(StringBuilder builder)
		{
			Utility.RenderHashJson(
				builder,
				"CtrlKey", CtrlKey,
				"ShiftKey", ShiftKey,
				"AltKey", AltKey,
				"KeyCode", KeyCode
			);
		}
	}

	public class HotKeyUtil
	{
		[DllImport("kernel32.dll")]
		internal static extern IntPtr GlobalAddAtom(string lpString);
		[DllImport("user32.dll")]
		internal static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
		[DllImport("user32.dll")]
		internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		static Hashtable m_HotKeys = new Hashtable();

		static bool RegisterHotKey(HotKey hk)
		{
			if (FindHotKeyAtom(hk) != 0) return true;

			IntPtr atom = GlobalAddAtom(Guid.NewGuid().ToString());
			int hotKeyId = atom.ToInt32();
			if (hotKeyId == 0) return false;

			HotKeyModifiers fsModifiers = 0;
			if (hk.CtrlKey) fsModifiers |= HotKeyModifiers.MOD_CONTROL;
			if (hk.ShiftKey) fsModifiers |= HotKeyModifiers.MOD_SHIFT;
			if (hk.AltKey) fsModifiers |= HotKeyModifiers.MOD_ALT;

			if (!RegisterHotKey(Global.Desktop.Handle, hotKeyId, (uint)fsModifiers, hk.KeyCode)) return false;

			lock (m_HotKeys)
			{
				m_HotKeys[hotKeyId] = hk;
			}

			return true;
		}

		static Int32 FindHotKeyAtom(HotKey hk)
		{
			foreach (DictionaryEntry ent in m_HotKeys)
			{
				if (ent.Value.ToString() == hk.ToString())
				{
					return Convert.ToInt32(ent.Key);
				}
			}
			return 0;
		}

		static bool UnregisterHotKey(HotKey hk)
		{
			lock (m_HotKeys)
			{
				Int32 atom = FindHotKeyAtom(hk);
				UnregisterHotKey(Global.Desktop.Handle, atom);
				m_HotKeys.Remove(atom);
				return true;
			}
		}

		public static HotKey FindHotKey(int atom)
		{
			lock (m_HotKeys)
			{
				return m_HotKeys[atom] as HotKey;
			}
		}

		static Hashtable m_HotKeyActions = new Hashtable();

		public static bool RegisterHotKeyAction(int action, HotKey hk)
		{
			lock (m_HotKeyActions)
			{
				if (m_HotKeyActions.ContainsKey(action))
				{
					if (m_HotKeyActions[action].ToString() != hk.ToString())
					{
						UnregisterHotKey(m_HotKeyActions[action] as HotKey);
						m_HotKeyActions.Remove(action);
					}
					else
					{
						return true;
					}
				}
				if (!RegisterHotKey(hk)) return false;
				m_HotKeyActions[action] = hk;
				return true;
			}
		}

		public static int FindHotKeyAction(HotKey hk)
		{
			lock (m_HotKeyActions)
			{
				foreach (DictionaryEntry ent in m_HotKeyActions)
				{
					if (ent.Value.ToString() == hk.ToString()) return Convert.ToInt32(ent.Key);
				}
				return 0;
			}
		}

		public static void RegisterAllHotKey()
		{
			try
			{
				string hkUF_json = Config.Instance.GetValue("HotKeyUF");
				HotKey hkUF = String.IsNullOrEmpty(hkUF_json) ? new HotKey(true, true, false, 90) : new HotKey(hkUF_json);
				RegisterHotKeyAction(1, hkUF);
				
				string hkMB_json = Config.Instance.GetValue("HotKeyMB");
				HotKey hkMB = String.IsNullOrEmpty(hkMB_json) ? new HotKey(true, true, false, 90) : new HotKey(hkMB_json);
				RegisterHotKeyAction(2, hkMB);
			}
			catch
			{
			}
		}

		public static void UnregisterAllHotKey()
		{
			try
			{
				lock (m_HotKeys)
				{
					foreach (DictionaryEntry ent in m_HotKeys)
					{
						UnregisterHotKey(Global.Desktop.Handle, Convert.ToInt32(ent.Key));
					}
					m_HotKeys.Clear();
				}
			}
			catch
			{
			}
		}
	}
}
