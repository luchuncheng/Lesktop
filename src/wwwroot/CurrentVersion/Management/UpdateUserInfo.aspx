<%@ Page Language="C#" AutoEventWireup="true" Inherits="Core.Web.Lesktop_Management_UpdateUserInfo" %>

<%@ Register Src="../Ctrl/CommandCtrl.ascx" TagName="CommandCtrl" TagPrefix="uc1" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title></title>
	<link href='../Themes/Default/Common.css' rel='stylesheet' type='text/css' />
	<link href='../Themes/Default/Table.css' rel='stylesheet' type='text/css' />
	<style type="text/css">
		.col1
		{
			width: 80px;
			text-align: right;
		}
		.col2
		{
			width: 200px;
			text-align: left;
		}
		.col3
		{
		}
		.col2 input
		{
			width: 90%;
			border: solid 1px #D0D0D0;
			font-family: SimSun;
			font-size: 12px;
			padding: 3px;
		}
		.operation_bottom
		{
			text-align: right;
		}
		a
		{
			text-decoration: none;
			color: Blue;
			margin-left: 4px;
		}
		a:hover
		{
			text-decoration: none;
		}
		
		#file_headimg_con
		{
			margin-top: 6px;
			background: url("../Themes/Default/Images/upload_file.png") no-repeat -3px 0px;
			cursor: pointer;
		}
		
		#file_headimg_con:hover
		{
			margin-top: 6px;
			background: url("../Themes/Default/Images/upload_file.png") no-repeat -3px -16px;
			cursor: pointer;
		}
		
		#file_headimg
		{
			filter: alpha(opacity=0);
			-moz-opacity: 0;
			opacity: 0;
			left: 0px;
			top: 0px;
			height: 16px;
			width: 60px;
			cursor: pointer;
			float: left;
		}
		
		#headimg
		{
			width: 150px;
			height: 150px;
		}
		
		#resetdepts
		{
			color: Blue;
			cursor: pointer;
			margin-left: 16px;
		}
		#resetdepts:hover
		{
			text-decoration: undeline;
		}
	</style>
	<script type="text/javascript" language="javascript">
		var CurrentWindow = parent.CurrentWindow;

		window.onload = function ()
		{
			CurrentWindow.Completed();

			var state = GetState();
			if (state.Action == "Error")
			{
				parent.Core.Utility.ShowError(state.Exception.Message);
			}

			document.getElementById("headimg").innerHTML = "<img/>";

			var img = document.getElementById("headimg").firstChild;
			img.onload = centerImage;
			img.src = GetHeadIMG(state.Info, 150, false);
		}

		function GetHeadIMG(info, size, gred)
		{
			return parent.Core.CreateHeadImgUrl(info.ID, size, gred, info.HeadIMG);
		}

		function centerImage()
		{
			var headimg = document.getElementById("headimg");
			var img = headimg.firstChild;

			if (img.width > 150 || img.height > 150)
			{
				if (img.width * 150 > img.height * 150)
				{
					img.height = 150 * img.height / img.width;
					img.width = 150;
				}
				else
				{
					img.width = 150 * img.width / img.height;
					img.height = 150;
				}
			}

			img.style.marginLeft = (150 - img.width) / 2 + 'px';
			img.style.marginTop = (150 - img.height) / 2 + 'px';
		}

		function file_headimg_onload()
		{
			document.getElementById("headimg").innerHTML = "<img/>";

			var img = document.getElementById("headimg").firstChild;
			img.onload = centerImage;

			if (parent.ClientMode)
			{
				var file = document.getElementById("file_headimg");
				if (file.value.indexOf("fakepath") >= 0)
				{
					file.select();
					img.src = document.selection.createRange().text;
				}
				else
				{
					img.src = file.value;
				}
			}
			else
			{
				img.src = parent.Core.GetUrl("Themes/Default/HeadIMG/head_changed_warning.png");
			}
		}

		function headimg_onload()
		{
		}

		function Match(reg, str)
		{
			reg.lastIndex = 0;
			var ft = reg.exec(str);
			return (ft != null && ft.length == 1 && ft[0] == str)
		}

		function CheckEMail(email)
		{
			return Match(/[a-zA-Z0-9._\-]+@[a-zA-Z0-9._\-]+/ig, email);
		}

		function CheckTel(tel)
		{
			return Match(/[0-9\-]{6,30}/ig, tel);
		}

		function CheckMobile(mobile)
		{
			return Match(/[0-9]{6,20}/ig, mobile);
		}

		function CheckName(name)
		{
			return Match(/[0-9a-zA-Z]+/ig, name);
		}

		function CheckPassword(pwd)
		{
			return pwd.length >= 4 && pwd.length <= 10;
		}

		function Update()
		{
			var nickname = document.getElementById("nickname");
			if (nickname.value < 2 || nickname.value > 16)
			{
				parent.Core.Utility.ShowWarning("姓名格式错误，长度为2-16个字符！");
				nickname.focus();
				return;
			}

			var tel = document.getElementById("tel");

			if (!CheckTel(tel.value))
			{
				parent.Core.Utility.ShowWarning("电话格式错误！");
				tel.focus();
				return;
			}

			var mobile = document.getElementById("mobile");
			if (!CheckMobile(mobile.value))
			{
				parent.Core.Utility.ShowWarning("手机号码格式错误!");
				mobile.focus();
				return;
			}

			var email = document.getElementById("email");
			if (!CheckEMail(email.value))
			{
				parent.Core.Utility.ShowWarning("电子邮箱格式错误!");
				email.focus();
				return;
			}

			CurrentWindow.Waiting("正在更新员工资料...");
			DoCommand("Update", {
				Nickname: nickname.value,
				EMail: email.value,
				Mobile: mobile.value,
				Tel: tel.value
			}
			);
		}

		function ResetPassword()
		{
			var password = document.getElementById("password");
			if (!CheckPassword(password.value))
			{
				parent.Core.Utility.ShowWarning("密码必须由4-10位字符组成！");
				password.focus();
				return;
			}

			var password_confirm = document.getElementById("password_confirm");
			if (password_confirm.value != password.value)
			{
				parent.Core.Utility.ShowWarning("两次输入密码不一致！");
				password_confirm.focus();
				return;
			}

			CurrentWindow.Waiting("正在重置登录密码...");

			var password_pre = document.getElementById("password_pre");
			if (password_pre == null)
			{
				DoCommand("ResetPassword", {
					Password: password.value
				}
				);
			}
			else
			{
				DoCommand("ResetSelfPassword", {
					PreviousPassword: password_pre.value,
					Password: password.value
				}
				);
			}
		}

		function ResetDepts()
		{
			var info = GetState().Info;

			var config = {
				Left: 0, Top: 0,
				Width: 400, Height: 450,
				MinWidth: 400, MinHeight: 450,
				Title: {
					InnerHTML: String.format("请选择\"{0}\"所属部门...", info.Nickname)
				},
				Resizable: false,
				HasMaxButton: false,
				HasMinButton: false,
				AnchorStyle: parent.Core.WindowAnchorStyle.Left | parent.Core.WindowAnchorStyle.Top
			}

			var form = parent.Core.CreateWindow(config);
			var tag = {
				AfterSelected: function (data)
				{
					CurrentWindow.Waiting("正在修改...");
					DoCommand("ResetUserDepts", { UserID: info.ID, NewDepts: data });
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
	<form id="form1" runat="server" enctype="multipart/form-data">
	<div class="table_blue">
		<table cellpadding="0" cellspacing="0">
			<tr class="header">
				<td colspan="3" class='col2'>
					<%= Info.ID == Core.ServerImpl.Instance.GetUserID(Context) ? "个人资料" : "员工资料"%>
				</td>
			</tr>
			<tr>
				<td class='col1'>
					登录名：
				</td>
				<td class='col2'>
					<%= Info.Name%>
				</td>
				<td class='col3'>
					&nbsp;
				</td>
			</tr>
			<tr>
				<td class='col1'>
					姓&nbsp;&nbsp;名：
				</td>
				<td class='col2'>
					<input id="nickname" type="text" value="<%= Info.Nickname%>" />
				</td>
				<td class='col3'>
					&nbsp;
				</td>
			</tr>
			<tr>
				<td class='col1'>
					电&nbsp;&nbsp;话：
				</td>
				<td class='col2'>
					<input id="tel" type="text" value="<%= Info.Tel%>" />
				</td>
				<td class='col3'>
					&nbsp;
				</td>
			</tr>
			<tr>
				<td class='col1'>
					手&nbsp;&nbsp;机：
				</td>
				<td class='col2'>
					<input id="mobile" type="text" value="<%= Info.Mobile%>" />
				</td>
				<td class='col3'>
					&nbsp;
				</td>
			</tr>
			<tr>
				<td class='col1'>
					邮&nbsp;&nbsp;箱：
				</td>
				<td class='col2'>
					<input id="email" value="<%= Info.EMail%>" type="text" />
				</td>
				<td class='col3'>
					&nbsp;
				</td>
			</tr>
			<asp:PlaceHolder runat="server" ID="PlaceHolderDepts" Visible="false">
				<tr>
					<td class='col1'>
						部&nbsp;&nbsp;门：
					</td>
					<td class='col2'>
						<%= Depts%><a id="resetdepts" href="javascript:ResetDepts();">修改所属部门</a>
					</td>
					<td class='col3'>
						&nbsp;
					</td>
				</tr>
			</asp:PlaceHolder>
			<tr>
				<td class='col1' style="vertical-align: top">
					相&nbsp;&nbsp;片：
				</td>
				<td class='col2' style="vertical-align: top">
					<div id="headimg">
					</div>
					<div id="file_headimg_con" style="width: 60px; height: 16px;">
						<input type='file' name='file_headimg' id='file_headimg' onchange="return file_headimg_onload()" /></div>
				</td>
				<td class='col3'>
					&nbsp;
				</td>
			</tr>
			<tr>
				<td colspan="3" class='operation_bottom'>
					<a href='javascript:Update();'>更新资料</a>
				</td>
			</tr>
		</table>
	</div>
	<br />
	<asp:PlaceHolder runat="server" ID="PlaceHolderResetPwd">
		<div class="table_blue">
			<table cellpadding="0" cellspacing="0">
				<tr class="header">
					<td colspan="3" class='col2'>
						重置密码
					</td>
				</tr>
				<asp:PlaceHolder runat="server" ID="PrePasswordPH">
					<tr>
						<td class='col1'>
							原密码：
						</td>
						<td class='col2'>
							<input id="password_pre" type="password" />
						</td>
						<td class='col3'>
							&nbsp;
						</td>
					</tr>
				</asp:PlaceHolder>
				<tr>
					<td class='col1'>
						新密码：
					</td>
					<td class='col2'>
						<input id="password" type="password" />
					</td>
					<td class='col3'>
						&nbsp;
					</td>
				</tr>
				<tr>
					<td class='col1'>
						密码确认：
					</td>
					<td class='col2'>
						<input id="password_confirm" type="password" />
					</td>
					<td class='col3'>
						&nbsp;
					</td>
				</tr>
				<tr>
					<td colspan="3" class='operation_bottom'>
						<a href='javascript:ResetPassword();'>重置密码</a>
					</td>
				</tr>
			</table>
		</div>
	</asp:PlaceHolder>
	<uc1:CommandCtrl ID="CommandCtrl1" runat="server" />
	</form>
</body>
</html>
