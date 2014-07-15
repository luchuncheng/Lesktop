<%@ Page Language="C#" AutoEventWireup="true" Inherits="Core.Web.Lesktop_MsgHistory_MessageList_Group" %>

<%@ Register src="../Ctrl/CommandCtrl.ascx" tagname="CommandCtrl" tagprefix="uc1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title></title>
	<link href='../Themes/Default/Common.css' rel='stylesheet' type='text/css' />	
	<link href='../Themes/Default/Table.css' rel='stylesheet' type='text/css' />
	
	<style type="text/css">
		.checkbox
		{
			width: 20px;
		}
		td.user
		{
			color: #AA0000;
		}
		.msgcount
		{
			width: 60px;
			text-align:right;
		}
		.renewtime
		{
			width: 120px;
			text-align:right;
		}
		.operation
		{
			width: 100px;
			text-align:right;
		}
	
		.operation a
		{
			text-decoration: none;
			color:Blue;
			margin-left:4px;
		}
		
		.operation a:hover
		{
			text-decoration: underline;
		}
		
		.operation_bottom input
		{
			padding:0px;
			margin:0px;
			border:solid 1px #D0D0D0;
			font-size:12px;
			font-family:SimSun;
		}
	</style>

	<script type="text/javascript" language="javascript">
		var CurrentWindow = parent.CurrentWindow;
	
		function Refresh()
		{
			Core.Params["random"] = (new Date()).getTime();
			
			var qs = "";
			for(var key in Core.Params)
			{
				if(qs != "") qs += "&";
				qs+= (key + "=" + Core.Params[key]);
			}
			
			parent.CurrentWindow.Waiting("正在载入聊天记录...");
			window.location = "MessageList_Group.aspx?" + qs;
		}
		
		function GetPageCount()
		{
			return GetState().PageCount
		}
		
		function GetCurrentPage()
		{
			if(Core.Params["CurrentPage"]!=undefined)
			{
				var cp = parseInt(Core.Params["CurrentPage"]);
				return cp == -1 ? GetPageCount() : cp;
			}
			else
			{
				return 1;//GetPageCount();
			}
		}
	
		function First()
		{
			Core.Params["CurrentPage"] = 1;
			Refresh();
		}
		
		function Last()
		{
			Core.Params["CurrentPage"] = -1;
			Refresh();
		}
		
		function Prior()
		{
			var cp = GetCurrentPage();
			Core.Params["CurrentPage"] = (cp == 1 ? 1 : cp - 1);
			Refresh();
		}
		
		function Next()
		{
			var cp = GetCurrentPage();
			Core.Params["CurrentPage"] = (cp == GetPageCount() ? GetPageCount() : cp + 1);
			Refresh();
		}
		
		function page_onkeydown(evt)
		{
			if(evt == undefined) evt = window.event;
			if(evt.keyCode == 13)
			{
				var tp = parseInt(document.getElementById("page").value);
				
				if(tp > GetPageCount()) tp = GetPageCount();
				if(tp < 0) tp = 0;
				
				if(tp == GetCurrentPage()) return;
				
				Core.Params["CurrentPage"]=tp;
				Refresh();
				if (evt && evt.stopPropagation) evt.stopPropagation();
				else evt.cancelBubble = true;
				
				return false;
			}
		}
		
		function onview()
		{
			CurrentWindow.Waiting("正在载入聊天记录...");
			return true;
		}

		window.onload = function()
		{
			CurrentWindow.Completed();

			var state = GetState();
			if (state.Action == "Error")
			{
				parent.Core.Utility.ShowError(state.Exception.Message);
			}
		}
		
		function SelectAll()
		{
			var tab = document.getElementById("msglist");
			var cb_all = document.getElementById("cb_all");
			for(var i = 1; i < tab.rows.length; i++)
			{
				tab.rows[i].cells[0].firstChild.checked = cb_all.checked;
			}
		}
		
		function Delete()
		{
			if(confirm("您确定要删除选中的消息记录？"))
			{
				var items = [];
				var tab = document.getElementById("msglist");
				for(var i = 1; i < tab.rows.length - 1; i++)
				{
					if(tab.rows[i].cells[0].firstChild.checked == true) items.push(tab.rows[i].cells[0].firstChild.value);
				}
				CurrentWindow.Waiting("正在删除消息...");
				DoCommand("Delete", {Items: items.join(",")});
			}
		}
		
		function ViewMessages(user, peer, return_url)
		{
			CurrentWindow.Waiting("正在载入聊天记录...");
			window.location = String.format("ViewMessages.aspx?User={0}&Peer={1}&ReturnUrl={2}", user, peer, return_url);
		}
	</script>
</head>
<body>
	<form id="form1" runat="server">
	<div class="table_blue">
		<table id="msglist" cellpadding="0" cellspacing="0">
			<tr class='header'>
				<td class='checkbox'><input id="cb_all" type="checkbox" onclick="return SelectAll();" /></td>
				<td class='user'><%= _isTemp == 1 ? "多人会话" : "群名称"%></td>
				<td class='msgcount'>消息数</td>
				<td class='renewtime'>最后对话时间</td>
				<td class='operation'>&nbsp;</td>
			</tr>
			<%= RenderList() %>
			<tr>
				<td colspan="5" class="operation_bottom">
					<div style="float:left; height:14px;">
						<a class="link" href="javascript:Delete()">删除所选</a>
					</div>
					<div style="float:right; height:12px;">
						<a class="<%= CurrentPage == 1 ? "link_invalid" : "link"%>" href="<%= CurrentPage == 1 ? "javascript:void(0);" : "javascript:First();"%>">第一页</a>
						<a class="<%= CurrentPage == 1 ? "link_invalid" : "link"%>" href="<%= CurrentPage == 1 ? "javascript:void(0);" : "javascript:Prior();"%>"> 上一页</a>
						第<input type="text" id="page" value="<%= CurrentPage %>" onkeydown="return page_onkeydown();" style="width:30px; text-align:center; margin-left: 4px; margin-right: 4px;" />页/共<%= PageCount %>页
						<a class="<%= CurrentPage == PageCount ? "link_invalid" : "link"%>" href="<%= CurrentPage == PageCount ? "javascript:void(0);" : "javascript:Next();"%>">下一页</a>
						<a class="<%= CurrentPage == PageCount ? "link_invalid" : "link"%>" href="<%= CurrentPage == PageCount ? "javascript:void(0);" : "javascript:Last();"%>">最后一页</a>
					</div>
				</td>
			</tr>
		</table>
	</div>
	<uc1:CommandCtrl ID="CommandCtrl" runat="server" />
	</form>
</body>
</html>
