<%@ Page Language="C#" AutoEventWireup="true" Inherits="Core.Web.Lesktop_Management_UpdateDeptInfo" %>

<%@ Register src="../Ctrl/CommandCtrl.ascx" tagname="CommandCtrl" tagprefix="uc1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title></title>
	<link href='../Themes/Default/Common.css' rel='stylesheet' type='text/css' />	
	<link href='../Themes/Default/Table.css' rel='stylesheet' type='text/css' />
	<style type="text/css">
		.col1
		{
			width:80px;
			text-align:right;
		}
		.col2
		{
			width: 200px;
			text-align:left;
		}
		.col3
		{
		}
		.col2 input
		{
			width:90%;
			border:solid 1px #D0D0D0;
			font-family:SimSun;
			font-size:12px;
			padding:3px;
		}
		.operation_bottom
		{
			text-align:right;
		}
		a
		{
			text-decoration: none;
			color:Blue;
			margin-left:4px;
		}
		a:hover
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
				parent.Core.AccountData.FireDataChangedEvent("DeptDataChanged", {DeptID: state.DeptID, DeptInfo: state.DeptInfo});
			}
			else if(state.Action == "ResetDeptData")
			{
				parent.Core.AccountData.ResetDeptData(state.DeptID, state.DeptInfo);
				parent.Core.AccountData.FireDataChangedEvent("DeptDataChanged", {DeptID: state.DeptID, DeptInfo: state.DeptInfo});
			}
			else if (state.Action == "Error")
			{
				parent.Core.Utility.ShowError(state.Exception.Message);
			}
			delete state["Action"];
		}
		
		function Update()
		{
			var nickname = document.getElementById("nickname");
			if(nickname.value < 2 || nickname.value > 16 )
			{
				parent.Core.Utility.ShowWarning("姓名格式错误，长度为2-16个字符！");
				nickname.focus();
				return;
			}
			
			CurrentWindow.Waiting("正在更新部门资料...");
			
			DoCommand("Update", {
					Name: nickname.value
				}
			);
		}
	</script>
</head>
<body>
	<form id="form1" runat="server">
	<div class="table_blue">
		<table cellpadding="0" cellspacing="0">
			<tr class="header">
				<td colspan="3" class='col2'><%= DeptID == 1 ? "公司信息" : "部门信息"%></td>
			</tr>
			<tr>
				<td class='col1'><%= DeptID == 1 ? "公司" : "部门"%>名称：</td>
				<td class='col2'><input id="nickname" type="text" value="<%= Data["Name"].ToString() %>" /></td>
				<td class='col3'>&nbsp;</td>
			</tr>
			<tr>
				<td colspan="3" class='operation_bottom'><a href='javascript:Update();'>更新<%= DeptID == 1 ? "公司" : "部门"%>资料</a></td>
			</tr>
		</table>
	</div>
	<uc1:CommandCtrl ID="CommandCtrl1" runat="server" />
	</form>
</body>
</html>
