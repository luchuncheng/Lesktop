<%@ Page Language="C#" AutoEventWireup="true" Inherits="Core.Web.Management_GroupMembers" %>

<%@ Register src="../Ctrl/CommandCtrl.ascx" tagname="CommandCtrl" tagprefix="uc1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
	<title></title>
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
			width: 80px;
			text-align: right;
		}
		.operation
		{
			width: 100px;
			text-align: right;
		}
		.operation_bottom
		{
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
		.operation_bottom a,
		.operation a
		{
			text-decoration: none;
			color:Blue;
			margin-left:4px;
		}
		.operation_bottom a:hover,
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
			if (state.Action == "Error")
			{
				parent.Core.Utility.ShowError(state.Exception.Message);
			}
		}

		function Remove(id, nickname, name)
		{	
			if (confirm(String.format("您将员工\"{0}\"({1})从本群组中移除？", nickname, name)))
			{
				CurrentWindow.Waiting("正在移除用户...");
				DoCommand("Remove", id);
			}
		}

		function match(reg,str)
		{
			reg.lastIndex=0;
			var ft = reg.exec(str);
			return (ft!=null && ft.length==1 && ft[0]==str)
		}
		
		function AddUsers()
		{
			var config = {
				Left: 0, Top: 0,
				Width: 450, Height: 500,
				MinWidth: 450, MinHeight: 500,
				Title: {
					InnerHTML: "添加员工"
				},
				Resizable: false,
				HasMaxButton: false,
				HasMinButton: false,
				AnchorStyle: parent.Core.WindowAnchorStyle.Left |  parent.Core.WindowAnchorStyle.Top
			}

			var form = parent.Core.CreateWindow(config);
			form.SetTag({ IDS: "" });
			form.MoveEx('CENTER', 0, 0, true);
			form.ShowDialog(
				CurrentWindow, 'CENTER', 0, 0, true, 
				function()
				{
					if (form.GetTag().IDS != "")
					{
						setTimeout(
							function()
							{
								CurrentWindow.Waiting("正在添加群组成员...");
								DoCommand("AddUsers", { IDS: form.GetTag().IDS });
							}, 10
						);
					}
				}
			);
			var url =  parent.Core.GetUrl("SelectUsersForm.htm");
			form.Load(url, null);
		}
	</script>

</head>
<body>
	<form id="form1" runat="server">
	<div class="table_blue">
		<table cellpadding="0" cellspacing="0">
			<tr class='header'>
				<td class='headimg'>&nbsp;</td>
				<td class='name'>登录名</td>
				<td class='nickname'>姓名</td>
				<td class='email'>电子邮件</td>
				<td class='registerTime'>注册时间</td>
				<td class='operation'>操作</td>
			</tr>
			<%= RenderAllUsersList()%>
			<asp:PlaceHolder ID="BottomOpe" runat="server">
			<tr>
				<td colspan="6" class='operation_bottom'>
					<a href='javascript:AddUsers();'>添加群组成员</a>
				</td>
			</tr>
			</asp:PlaceHolder>
		</table>
	</div>
	<uc1:CommandCtrl ID="CommandCtrl" runat="server" />
	</form>
</body>
</html>
