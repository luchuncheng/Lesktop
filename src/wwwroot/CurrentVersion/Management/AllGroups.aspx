<%@ Page Language="C#" AutoEventWireup="true" Inherits="Core.Web.Management_AllGroups" %>

<%@ Register src="../Ctrl/CommandCtrl.ascx" tagname="CommandCtrl" tagprefix="uc1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
	<title></title>
	<link href='../Themes/Default/Common.css' rel='stylesheet' type='text/css' />	
	<link href='../Themes/Default/Table.css' rel='stylesheet' type='text/css' />
	
	<style type="text/css">
		.headimg
		{
			width:20px;
		}
		.name
		{
		}
		.nickname
		{
		}
		.name input,
		.nickname input
		{
			width:90%;
			border:solid 1px #D0D0D0;
			font-family:SimSun;
			font-size:12px;
			padding:3px;
		}
		.creator
		{
			width: 120px;
		}
		.registerTime
		{
			width: 100px;
			text-align: right;
		}
		.operation
		{
			width: 120px;
			text-align: right;
		}
		.operation a
		{
			text-decoration: none;
			color:Blue;
			margin-left:4px;
		}
		.operation a:hover
		{
			text-decoration: none;
		}
	</style>

	<script type="text/javascript" language="javascript">
		var CurrentWindow = parent.CurrentWindow;

		window.onload = function()
		{
			CurrentWindow.Completed();

			var state = GetState();
			if(state.Action == "ClearDeptData")
			{
				parent.Core.AccountData.ClearDeptData(state.DeptID);
				parent.Core.AccountData.FireDataChangedEvent("DeptDataChanged", {DeptID: state.DeptID, DeptInfo: state.DeptInfo });
				delete state["Action"];
			}
			else if (state.Action == "Error")
			{
				parent.Core.Utility.ShowError(state.Exception.Message);
			}
		}

		function Delete(id, nickname, name)
		{	
			if (confirm(String.format("您确定要删除群组\"{0}\"({1})", nickname, name)))
			{
				CurrentWindow.Waiting("正在删除群组...");
				DoCommand("Delete", id);
			}
		}

		function match(reg,str)
		{
			reg.lastIndex=0;
			var ft = reg.exec(str);
			return (ft!=null && ft.length==1 && ft[0]==str)
		}

		function NewGroup()
		{
			var nameReg = /[a-zA-Z0-9_]{4,16}/ig;

			var nickname = document.getElementById("new_group_nickname").value;
			if (nickname == "")
			{
				alert("请输入群名称！");
				document.getElementById("new_group_nickname").focus();
				return;
			}

			CurrentWindow.Waiting("正在创建群组...");
			DoCommand("NewGroup", { Nickname: nickname });
			return;
		}
	</script>

</head>
<body>
	<form id="form1" runat="server">
	<div class="table_red">
		<table cellpadding="0" cellspacing="0">
			<tr>
				<td class='nickname'>“<%= DeptInfo["Name"].ToString() %>” 群组管理</td>
			</tr>
		</table>
	</div>
	<br />
	<div class="table_blue">
		<table cellpadding="0" cellspacing="0">
			<tr class='header'>
				<td class='headimg'>&nbsp;</td>
				<td class='nickname'>群名称</td>
				<td class='creator'>群创建者</td>
				<td class='operation'>操作</td>
			</tr>
			<%= RenderFriendList()%>
			<tr>
				<td class='headimg'>&nbsp;</td>
				<td class='nickname'><input type="text" id="new_group_nickname" name="new_group_nickname" /></td>
				<td class='creator'>&nbsp;</td>
				<td class='operation'><a href='javascript:NewGroup();'>新建群组</a></td>
			</tr>
		</table>
	</div>
	<uc1:CommandCtrl ID="CommandCtrl" runat="server" />
	</form>
</body>
</html>
