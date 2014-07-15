<%@ Page Language="C#" AutoEventWireup="true" Inherits="Core.Web.Management_AllUsers" %>

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
			width:2px;
		}
		.name
		{
			width: 60px;
		}
		.nickname
		{
			width: 100px;
		}
		.email
		{
		}
		.dept
		{
		}
		.operation
		{
			width: 120px;
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
			if(state.Action == "ClearDeptData")
			{
				parent.Core.AccountData.ClearDeptData(state.DeptID);
				parent.Core.AccountData.FireDataChangedEvent("DeptDataChanged", {DeptID: state.DeptID, DeptInfo: state.DeptInfo });
				delete state["Action"];
			}
			else if(state.Action == "ResetDeptData")
			{
				parent.Core.AccountData.ClearDeptData(state.DeptID);
				parent.Core.AccountData.FireDataChangedEvent("DeptDataChanged", {DeptID: state.DeptID });
				delete state["Action"];
			}
			else if (state.Action == "Error")
			{
				parent.Core.Utility.ShowError(state.Exception.Message);
			}
		}

		function Remove(id, nickname, name, deptID, deptName)
		{	
			if (confirm(String.format("您将员工\"{0}\"({1})从部门 \"{2}\" 中移除？", nickname, name, deptName)))
			{
				CurrentWindow.Waiting("正在移除用户...");
				DoCommand("Remove", {DeptID: deptID, ID: id});
			}
		}

		function Delete(id, nickname, name)
		{	
			if (confirm(String.format("您确定要删除员工\"{0}\"({1})，删除后，员工所有资料将被全部删除！", nickname, name)))
			{
				CurrentWindow.Waiting("正在删除员工...");
				DoCommand("Delete", id);
			}
		}

		function match(reg,str)
		{
			reg.lastIndex=0;
			var ft = reg.exec(str);
			return (ft!=null && ft.length==1 && ft[0]==str)
		}

		function CreateUser()
		{
			var nameReg = /[a-zA-Z0-9][a-zA-Z0-9_]{3,15}/ig;
			var emailReg = /[a-zA-Z0-9._\-]+@[a-zA-Z0-9._\-]+/ig;

			var name = document.getElementById("new_user_name").value;
			if(GetState().CheckLoginName)
			{
				if (!match(nameReg, name))
				{
					alert("登录名格式不正确（账户名为4-16位字符，并且只能包含英文字符，数字和下划线，首字符必须为字母）！");
					document.getElementById("new_user_name").focus();
					return;
				}
			}
			else
			{
				if (name.length <= 0)
				{
					alert("登录名不能为空！");
					document.getElementById("new_user_name").focus();
					return;
				}
			}

			var nickname = document.getElementById("new_user_nickname").value;
			if (nickname == "")
			{
				alert("请输入员工姓名！");
				document.getElementById("new_user_nickname").focus();
				return;
			}

			var email = document.getElementById("new_user_email").value;  

			CurrentWindow.Waiting("正在创建员工...");
			DoCommand("CreateUser", { Name: name, Nickname: nickname, EMail: email });
			return;
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
								DoCommand("AddUsers", { IDS: form.GetTag().IDS });
							}, 10
						);
					}
				}
			);
			var url =  parent.Core.GetUrl(String.format("Management/SelectUsersForm.aspx?random={0}", (new Date()).getTime()));
			form.Load(url, null);
		}

		function ResetUserDepts(id, name, nickname)
		{
			var config = {
				Left: 0, Top: 0,
				Width: 400, Height: 450,
				MinWidth: 400, MinHeight: 450,
				Title: {
					InnerHTML: String.format("请选择\"{0}\"所属部门...", name)
				},
				Resizable: false,
				HasMaxButton: false,
				HasMinButton: false,
				AnchorStyle: parent.Core.WindowAnchorStyle.Left | parent.Core.WindowAnchorStyle.Top
			}

			var form = parent.Core.CreateWindow(config);
			var tag = {
				AfterSelected: function(data)
				{
					CurrentWindow.Waiting("正在修改...");
					DoCommand("ResetUserDepts", { UserID: id, NewDepts: data });
				}
			};
			form.SetTag(tag);
			form.MoveEx('CENTER', 0, 0, true);
			form.ShowDialog(CurrentWindow, 'CENTER', 0, 0, true, null);
			var url = parent.Core.GetUrl("SelectDeptForm.htm");
			form.Load(url, null);
		}
	</script>

</head>
<body>
	<form id="form1" runat="server">
	<div class="table_red">
		<table cellpadding="0" cellspacing="0">
			<tr>
				<td>“<%= DeptInfo["Name"].ToString() %>” 人员管理</td>
			</tr>
		</table>
	</div>
	<br />
	<div class="table_blue">
		<table cellpadding="0" cellspacing="0">
			<tr class='header'>
				<td class='headimg'>&nbsp;</td>
				<td class='name'>登录名</td>
				<td class='nickname'>姓名</td>
				<td class='email'>电子邮件</td>
				<td class='dept'>所属部门</td>
				<td class='operation'>操作</td>
			</tr>
			<%= RenderAllUsersList()%>
			<tr>
				<td class='headimg'>&nbsp;</td>
				<td class='name'><input type="text" id="new_user_name" name="new_user_name" /></td>
				<td class='nickname'><input type="text" id="new_user_nickname" name="new_user_nickname" /></td>
				<td class='email'><input type="text" id="new_user_email" name="new_user_email" /></td>
				<td class='dept'>&nbsp;</td>
				<td class='operation'><a href='javascript:CreateUser();'>新建员工</a></td>
			</tr>
			<tr>
				<td class='headimg'>&nbsp;</td>
				<td colspan="5" style="color:Red;">注：所有新建的员工密码均为空，第一次登录不需要输入密码。</td>
			</tr>
			<%--<tr>
				<td colspan="6" class='operation_bottom'><a href='javascript:AddUsers();'>添加员工</a></td>
			</tr>--%>
		</table>
	</div>
	<uc1:CommandCtrl ID="CommandCtrl" runat="server" />
	</form>
</body>
</html>
