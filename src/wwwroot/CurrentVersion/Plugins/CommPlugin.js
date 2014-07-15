if (window.Core == undefined) window.Core = {};
if (window.Core.PluginsNS == undefined) window.Core.PluginsNS = {};

(function()
{
	window.Core.PluginsNS.CommPlugin = {};

	var m_CommManForm = null;

	Core.PluginsNS.CommPlugin.ShowCommManForm = function()
	{
		if (m_CommManForm == null)
		{
			var config = {
				Left: 0,
				Top: 0,
				Width: 700,
				Height: 550,
				MinWidth: 700,
				MinHeight: 550,
				Title: {
					InnerHTML: "注册用户/群组管理"
				},
				Resizable: true,
				HasMaxButton: ClientMode,
				HasMinButton: true,
				AnchorStyle: Core.WindowAnchorStyle.Left | Core.WindowAnchorStyle.Bottom
			}
			m_CommManForm = Core.CreateWindow(config);
			m_CommManForm.OnClosed.Attach(function() { m_CommManForm = null; });
			m_CommManForm.MoveEx('MIDDLE|BOTTOM', 0, -30, true);
			setTimeout(function() { m_CommManForm.Show(); }, 10);
			var url = Core.GetUrl("CommManForm.htm");
			url += "?random=" + (new Date()).getTime();
			m_CommManForm.Load(url, null);
		}
		else
		{
			setTimeout(function() { m_CommManForm.Show(); }, 10);
		}

		return m_CommManForm;
	}

})();