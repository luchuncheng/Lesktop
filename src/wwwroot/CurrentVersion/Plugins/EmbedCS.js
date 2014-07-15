if (window.Core == undefined) window.Core = {};
if (window.Core.PluginsNS == undefined) window.Core.PluginsNS = {};

(function()
{
	window.Core.PluginsNS.EmbedCS = {};

	var timer = null;

	Core.PluginsNS.EmbedCS.CheckComment = function()
	{
		var data = {
			Action: "HasUnreadComment"
		}

		try
		{
			Core.SendCommand(
				function(ret)
				{
					try
					{
						if (ret.Count > 0)
						{
							var notifyData = {
								Type: "UnreadCommentNotify",
								Count: ret.Count
							};
							Core.Utility.ShowFloatForm(Core.Utility.RenderJson(notifyData), "json");
						}
					}
					catch (ex)
					{
					}
					finally
					{
						if (timer != null) clearTimeout(timer);
						timer = setTimeout(Core.PluginsNS.EmbedCS.CheckComment, 10 * 60 * 1000);
					}
				},
				function(ex)
				{
					if (timer != null) clearTimeout(timer);
					timer = setTimeout(Core.PluginsNS.EmbedCS.CheckComment, 10 * 60 * 1000);
				},
				Core.Utility.RenderJson(data), "Core.Web Common_CH", false
			);
		}
		catch (ex)
		{
			if (timer != null) clearTimeout(timer);
			timer = setTimeout(Core.PluginsNS.EmbedCS.CheckComment, 10 * 60 * 1000);
		}
	}

	var m_EmbedCodeForm = null;

	Core.PluginsNS.EmbedCS.ShowEmbedCodeForm = function()
	{
		if (m_EmbedCodeForm == null)
		{
			var config = {
				Left: 0,
				Top: 0,
				Width: 700,
				Height: 550,
				MinWidth: 700,
				MinHeight: 550,
				Title: {
					InnerHTML: "客服嵌入代码管理"
				},
				Resizable: true,
				HasMaxButton: ClientMode,
				HasMinButton: true,
				AnchorStyle: Core.WindowAnchorStyle.Left | Core.WindowAnchorStyle.Bottom
			}
			m_EmbedCodeForm = Core.CreateWindow(config);
			m_EmbedCodeForm.OnClosed.Attach(function() { m_EmbedCodeForm = null; });
			m_EmbedCodeForm.MoveEx('MIDDLE|BOTTOM', 0, -30, true);
			setTimeout(function() { m_EmbedCodeForm.Show(); }, 10);
			var url = Core.GetUrl("EmbedCodeManagerForm.htm");
			url += "?random=" + (new Date()).getTime();
			m_EmbedCodeForm.Load(url, null);
		}
		else
		{
			setTimeout(function() { m_EmbedCodeForm.Show(); }, 10);
		}

		return m_EmbedCodeForm;
	}

	var m_CommentForm = null;
	var m_CommentTab = null;

	Core.PluginsNS.EmbedCS.ShowCommentForm = function(tabIndex)
	{
		if (m_CommentForm == null)
		{
			var config = {
				Left: 0,
				Top: 0,
				Width: 700,
				Height: 550,
				MinWidth: 700,
				MinHeight: 550,
				Title: {
					InnerHTML: "客户留言管理"
				},
				Resizable: true,
				HasMaxButton: ClientMode,
				HasMinButton: true,
				AnchorStyle: Core.WindowAnchorStyle.Left | Core.WindowAnchorStyle.Bottom
			}
			var tag = {
				OnCreated: function(p)
				{
					m_CommentTab = p;
					if (tabIndex != undefined) p.Select(tabIndex);
				}
			};
			m_CommentForm = Core.CreateWindow(config);
			m_CommentForm.SetTag(tag);
			m_CommentForm.OnClosed.Attach(function() { m_CommentForm = null; });
			m_CommentForm.MoveEx('MIDDLE|BOTTOM', 0, -30, true);
			setTimeout(function() { m_CommentForm.Show(); }, 10);
			var url = Core.GetUrl("CommentForm.htm");
			url += "?random=" + (new Date()).getTime();
			m_CommentForm.Load(url, null);
		}
		else
		{
			if (tabIndex != undefined && m_CommentTab != null) m_CommentTab.Select(tabIndex);
			setTimeout(function() { m_CommentForm.Show(); }, 10);
		}

		return m_CommentForm;
	}

	Core.PluginsNS.EmbedCS.AfterInit = function()
	{
	}

	Core.PluginsNS.EmbedCS.AfterInitSession = function()
	{
		Core.PluginsNS.EmbedCS.CheckComment();
	}

	Core.PluginsNS.EmbedCS.OnEmbedCodeManagement = function()
	{
		Core.PluginsNS.EmbedCS.ShowEmbedCodeForm();
	}
	
	Core.PluginsNS.EmbedCS.OnViewComment = function()
	{
		Core.PluginsNS.EmbedCS.ShowCommentForm(2);
	}

})();