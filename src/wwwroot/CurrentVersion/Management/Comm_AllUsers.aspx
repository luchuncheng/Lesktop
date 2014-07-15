<%@ Page Language="C#" AutoEventWireup="true" Inherits="Management_CommAllUsers" %>

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
		.email
		{
		}
		.registerTime
		{
			width: 100px;
			text-align: right;
		}
		.operation
		{
			width: 100px;
			text-align: right;
		}
	</style>

	<script type="text/javascript" language="javascript">
		var CurrentWindow = parent.CurrentWindow;

		window.onload = function()
		{
			CurrentWindow.Completed();

			var state = GetState();
			if (state.Action == "")
			{
			}
		}

		function Delete(id, nickname, name)
		{	
			if (confirm(String.format("您确定要删除用户\"{0}\"({1})", nickname, name)))
			{
				CurrentWindow.Waiting("正在删除用户...");
				DoCommand("Delete", id);
			}
		}
	</script>

</head>
<body>
	<form id="form1" runat="server">
	<div class="table_blue">
		<table cellpadding="0" cellspacing="0">
			<tr class='header'>
				<td class='headimg'>&nbsp;</td>
				<td class='name'>用户名</td>
				<td class='nickname'>昵称</td>
				<td class='email'>电子邮件</td>
				<td class='registerTime'>注册时间</td>
				<td class='operation'>操作</td>
			</tr>
			<%= RenderAllUsersList()%>
		</table>
	</div>
	<uc1:CommandCtrl ID="CommandCtrl" runat="server" />
	</form>
</body>
</html>
