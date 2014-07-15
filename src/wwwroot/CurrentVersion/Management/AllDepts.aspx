<%@ Page Language="C#" AutoEventWireup="true" Inherits="Core.Web.Management_AllDepts" %>

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
			width: 100px;
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
				parent.Core.AccountData.FireDataChangedEvent("DeptDataChanged", {DeptID: state.DeptID, DeptInfo: state.DeptInfo});
			}
			else if (state.Action == "Error")
			{
				parent.Core.Utility.ShowError(state.Exception.Message);
			}
			delete state["Action"];
		}

		function Delete(id, nickname)
		{	
			if (confirm(String.format("您确定要删除部门\"{0}\"", nickname)))
			{
				CurrentWindow.Waiting("正在删除部门...");
				DoCommand("Delete", id);
			}
		}

		function match(reg,str)
		{
			reg.lastIndex=0;
			var ft = reg.exec(str);
			return (ft!=null && ft.length==1 && ft[0]==str)
		}

		function NewDept()
		{
			var nickname = document.getElementById("new_dept_nickname").value;
			if (nickname == "")
			{
				alert("请输入部门名称！");
				document.getElementById("new_dept_nickname").focus();
				return;
			}

			CurrentWindow.Waiting("正在创建部门...");
			DoCommand("NewDept", { Nickname: nickname });
			return;
		}
	</script>

</head>
<body>
	<form id="form1" runat="server">
	<div class="table_red">
		<table cellpadding="0" cellspacing="0">
			<tr>
				<td class='nickname'>“<%= DeptInfo["Name"].ToString() %>” <%= DeptID == 1 ? "" : "子"%>部门管理</td>
			</tr>
		</table>
	</div>
	<br />
	<div class="table_blue">
		<table cellpadding="0" cellspacing="0">
			<tr class='header'>
				<td class='headimg'>&nbsp;</td>
				<td class='nickname'>部门名称</td>
				<td class='registerTime'>所属部门</td>
				<td class='operation'>操作</td>
			</tr>
			<%= RenderDeptList()%>
			<tr>
				<td class='headimg'>&nbsp;</td>
				<td class='nickname'><input type="text" id="new_dept_nickname" name="new_dept_nickname" /></td>
				<td class='registerTime'>&nbsp;</td>
				<td class='operation'><a href='javascript:NewDept();'>新建部门</a></td>
			</tr>
		</table>
	</div>
	<uc1:CommandCtrl ID="CommandCtrl" runat="server" />
	</form>
</body>
</html>
