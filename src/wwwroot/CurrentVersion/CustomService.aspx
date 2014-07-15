<%@ Page Language="C#" AutoEventWireup="true" Inherits="Core.Web.Lesktop_CustomService" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>客服平台</title>
	<style type="text/css"">
	body
	{
		margin: 0px;
		padding: 0px;
	}
	</style>
	<link href="Themes/Default/CommonCtrl.css" rel="stylesheet" type="text/css" />
	<link href="Themes/Default/TreeView.css" rel="stylesheet" type="text/css" />
	<link href="Themes/Default/Control.css" rel="stylesheet" type="text/css" />
	<link href="Themes/Default/MsgCtrl.css" rel="stylesheet" type="text/css" />
	<link href="Themes/Default/ChatPanel.css" rel="stylesheet" type="text/css" />
	<link href="Themes/Default/ChatForm.css" rel="stylesheet" type="text/css" />
	<link href="Themes/Default/RichEditor.css" rel="stylesheet" type="text/css" />
	<script language="javascript" type="text/javascript">
		window.CustomServiceMode = true;
	</script>
	<script src="Core/Config.ashx" type="text/javascript"></script>
	<script src="Core/Common.js" type="text/javascript"></script>
	<script src="Core/Extent.js" type="text/javascript"></script>
	<script src="Core/Plugins.js" type="text/javascript"></script>
	<script src="Core/Main.js" type="text/javascript"></script>
	<script src="UI/CommonCtrl.js" type="text/javascript"></script>
	<script src="UI/TreeView.js" type="text/javascript"></script>
	<script src="UI/Control.js" type="text/javascript"></script>
	<script src="UI/MsgCtrl.js" type="text/javascript"></script>
	<script src="UI/RichEditor.js" type="text/javascript"></script>
	<script src="UI/ChatPanel.js" type="text/javascript"></script>
	<script src="UI/ChatForm.js" type="text/javascript"></script>
	<script language="javascript" type="text/javascript">

		window.CSWindow = {};
		Core.IWindow.call(window.CSWindow);

		window.CSWindow.GetClientWidth = function ()
		{
			return window.document.documentElement.clientWidth;
		}
		window.CSWindow.GetClientHeight = function ()
		{
			return window.document.documentElement.clientHeight;
		}
		window.CSWindow.GetClientCoord = function ()
		{
		}

		function OnServiceStart()
		{
			var csdata = Core.Utility.ParseJson(document.getElementById("data_json").value);
			Core.UI.PagePanel.Create('');
			var config = {
				Peer: csdata.Peer,
				User: csdata.User
			};
			window.CSData = csdata;
			window.CSChatPanel = new Core.UI.VisitorChatPanel(config);
			if (csdata.Peer.Type == 0 && csdata.Peer.State == "Leave")
			{
				window.CSChatPanel.MarkLeaveStatus();
			}
		}
		window.onload = function ()
		{
			StartService(OnServiceStart);
		}
		window.onresize = function ()
		{
			window.CSWindow.OnResize.Call()
		}
	</script>
</head>
<body>
	<form id="form1" runat="server">
	<input id="data_json" type="hidden" runat="server" />
	</form>
</body>
</html>
