<%@ Page Language="C#" AutoEventWireup="true" Inherits="Management_CommGroupList" %>

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
		.operation
		{
			width: 140px;
			text-align: right;
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
		
		function Update(name, id)
		{
			var config = {
				Left: 0, Top: 0,
				Width: 630, Height: 500,
				MinWidth: 370, MinHeight: 500,
				Title: {
					InnerHTML: "修改群资料"
				},
				Resizable: false,
				HasMaxButton: false,
				HasMinButton: true,
				AnchorStyle: parent.Core.WindowAnchorStyle.Left |  parent.Core.WindowAnchorStyle.Top
			}
			
			var form =  parent.Core.CreateWindow(config);
			form.MoveEx('CENTER', 0, 0, true);
			form.ShowDialog(CurrentWindow, 'CENTER', 0, 0, true, null);
			var url =  parent.Core.GetUrl(String.format("Management/GroupManagerForm.htm?random={0}&ID={1}", (new Date()).getTime(), id));
			form.Load(url, null);
		}

		function Delete(id, nickname, name, isCreator)
		{	
			if(isCreator)
			{
				if (confirm(String.format("您确定要解散群组\"{0}\"({1})", nickname, name)))
				{
					CurrentWindow.Waiting("正在删除群组...");
					DoCommand("Delete", id);
				}
			}
			else
			{
				if (confirm(String.format("您确定要退出群组\"{0}\"({1})", nickname, name)))
				{
					CurrentWindow.Waiting("正在退出群组...");
					DoCommand("Exit", id);
				}
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

			var name = document.getElementById("new_group_name").value;
			if (!match(nameReg, name))
			{
				alert("群账户名格式不正确（账户名为4-16位字符，并且只能包含英文字符，数字和下划线）！");
				document.getElementById("new_group_name").focus();
				return;
			}

			var nickname = document.getElementById("new_group_nickname").value;
			if (nickname == "")
			{
				alert("请输入群名称！");
				document.getElementById("new_group_nickname").focus();
				return;
			}

			DoCommand("NewGroup", { Name: name, Nickname: nickname });
			return;
		}
	</script>

</head>
<body>
	<form id="form1" runat="server">
	<div class="table_blue">
		<table cellpadding="0" cellspacing="0">
			<tr class='header'>
				<td class='headimg'>&nbsp;</td>
				<td class='name'>群账号</td>
				<td class='nickname'>群名称</td>
				<td class='creator'>群创建者</td>
				<td class='operation'>操作</td>
			</tr>
			<%= RenderFriendList()%>
			<tr>
				<td class='headimg'>&nbsp;</td>
				<td class='name'><input type="text" id="new_group_name" name="new_group_name" /></td>
				<td class='nickname'><input type="text" id="new_group_nickname" name="new_group_nickname" /></td>
				<td class='creator'>&nbsp;</td>
				<td class='operation'>
					<a href='javascript:NewGroup();'>新建群组</a>
				</td>
			</tr>
		</table>
	</div>
	<uc1:CommandCtrl ID="CommandCtrl" runat="server" />
	</form>
</body>
</html>
