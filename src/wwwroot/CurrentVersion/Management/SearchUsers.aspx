<%@ Page Language="C#" AutoEventWireup="true" Inherits="Core.Web.Management_SearchUsers" %>

<%@ Register src="../Ctrl/CommandCtrl.ascx" tagname="CommandCtrl" tagprefix="uc1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
	<title></title>
	<link href='../Themes/Default/Common.css' rel='stylesheet' type='text/css' />	
	<link href='../Themes/Default/Table.css' rel='stylesheet' type='text/css' />
	
	<style type="text/css">
		body
		{
			margin: 8px;
		}
		#users_table td
		{
			vertical-align:middle;
		}
		.headimg
		{
			width:20px;
		}
		.name
		{
			width: 100px;
		}
		.nickname
		{
		}
		.email
		{
		}
		.registerTime
		{
			width: 80px;
			text-align: right;
		}
		.operation
		{
			width: 100px;
			text-align: right;
		}
		.name input,
		.nickname input,
		.email input
		{
			width:90%;
			border:solid 1px #D0D0D0;
			font-family:SimSun;
			font-size:12px;
			padding:3px;
		}
		#Table1 a,
		.operation a
		{
			text-decoration: none;
			color:Blue;
			margin-left:4px;
		}
		#Table1 a:hover,
		.operation a:hover
		{
			text-decoration: none;
		}
		#search_keyword
		{
			width:90%;
			border:solid 1px #D0D0D0;
			font-family:SimSun;
			font-size:12px;
			padding:3px;
		}
	</style>

	<script type="text/javascript" language="javascript">
		var CurrentWindow = parent.CurrentWindow;

		window.onload = function()
		{
			CurrentWindow.Completed();

			var state = GetState();
			if (state.Action == "Error")
			{
				parent.Core.Utility.ShowError(state.Exception.Message);
			}
		}

		window.GetUsers = function()
		{
			var ids = [];
			var tab = document.getElementById("users_table");
			if (tab != null)
			{
				for (var i = 1; i < tab.rows.length; i++)
				{
					var cb = tab.rows[i].cells[0].firstChild;
					if (cb != null && cb.checked) ids.push(cb.value);
				}
			}
			return ids.join(',');
		}
		
		function Search()
		{
			var keyword = document.getElementById("search_keyword");
			if(keyword.value.length == 0)
			{
				parent.Core.Utility.ShowWarning("请输入关键字！");
				keyword.focus();
				return;
			}
			
			var state = GetState();
			state.IDS = GetUsers();
			ResetState(state);
			
			CurrentWindow.Waiting("正在搜索用户...");
			DoCommand("Search", { Keyword: keyword.value });
		}
		
		function search_keyword_onkeydown(evt)
		{		    
			if(evt == undefined) evt = event;
			if(evt.keyCode == 13)
			{
				Search();
			}
		}
	</script>

</head>
<body>
	<form id="form1" runat="server">
	<uc1:CommandCtrl ID="CommandCtrl" runat="server" />
	<div class="table_blue">
		<table id="Table1" cellpadding="0" cellspacing="0">
			<tr>
				<td><input runat="server" id="search_keyword" type="text" onkeydown="search_keyword_onkeydown()" /></td>
				<td style="width: 35px"><a href="javascript:Search()">搜 索</a></td>
			</tr>
		</table>
	</div>
	<asp:PlaceHolder ID="PlaceHolder1" runat="server">
		<br />
		<div class="table_blue">
		<table id="users_table" cellpadding="0" cellspacing="0">
			<tr class='header'>
				<td class='headimg'>&nbsp;</td>
				<td class='name'>登录名</td>
				<td class='nickname'>姓名</td>
				<td class='email'>所属部门</td>
			</tr>
			<%= RenderLastSelList()%>
			<%= RenderAllUsersList()%>
		</table>
	</div>
	</asp:PlaceHolder>
	<asp:PlaceHolder ID="PlaceHolder3" runat="server">
		<br />
		<div class="table_red">
			<table id="Table3" cellpadding="0" cellspacing="0">
				<tr>
					<td class='nickname'>如要选择多个员工，可执行多次搜索，下次搜索将保留本次搜索的选中项！</td>
				</tr>
			</table>
		</div>
	</asp:PlaceHolder>
	<asp:PlaceHolder ID="PlaceHolder2" runat="server">
		<br />
		<div class="table_red">
			<table id="Table2" cellpadding="0" cellspacing="0">
				<tr>
					<td class='nickname'>红色项为上次搜索选中项</td>
				</tr>
			</table>
		</div>
	</asp:PlaceHolder>
	</form>
</body>
</html>
