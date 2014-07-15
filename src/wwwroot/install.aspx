<%@ Page Language="C#" AutoEventWireup="true" CodeFile="install.aspx.cs" Inherits="_lesktop_install" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title></title>
	<style type="text/css">
		.table_blue
		{
			margin-bottom:14px;
		}
		.table_blue table
		{
			border-width: 0px;
			border-style: solid;
			border-color: #C4D6EC;
			width: 100%;
		}
		.table_blue td
		{
			font-size: 12px;
			font-family: SimSun;
			padding-top: 8px;
			padding-bottom: 8px;
			padding-left: 8px;
			padding-right: 8px;
			line-height: 1.5em;
			border-style: solid;
			border-color: #C4D6EC;
		}
		.table_blue tr td
		{
			border-width: 0px 0px 1px 0px;
			border-style: solid;
			border-color: #C4D6EC;
			background-color: #F8FCFF;
			color: #074977;
			border-bottom-style:dotted;
		}
		.table_blue tr.header td
		{
			border-width: 1px 0px 1px 0px;
			background-color: #F3F7FA;
			font-weight: bold;
		}
		.style1
		{
			width: 130px;
		}
		.style2
		{
			width: 550px;
		}
		.textbox
		{
			border:solid 1px #D0D0D0;
			font-size:12px;
			font-family:SimSun;
			padding:4px;
			width: 522px;
		}
		.style3
		{
			font-family: serif;
			font-family:SimSun;
		}
	</style>
</head>
<body>
	<form id="form1" runat="server">
	<div class="table_blue">
		<table cellpadding="0" cellspacing="0">
			<tr class="header">
				<td colspan="3">
					管理员初始密码(admin)
				</td>
			</tr>
			<tr>
				<td class="style1">
					管理员密码：
				</td>
				<td class="style2">
					<asp:TextBox class="textbox" ID="tbAdminPwd" runat="server" TextMode="Password"></asp:TextBox>
				</td>
				<td>
					&nbsp;
				</td>
			</tr>
			<tr>
				<td class="style1">
					管理员密码确认：
				</td>
				<td class="style2">
					<asp:TextBox class="textbox" ID="tbAdminPwdConfirm" runat="server" TextMode="Password"></asp:TextBox>
				</td>
				<td>
					&nbsp;
				</td>
			</tr>
		</table>
	</div>
	<div class="table_blue">
		<table cellpadding="0" cellspacing="0">
			<tr class="header">
				<td colspan="3">
					数据库配置<span style="color:Red;">（请使用空的数据库，SQL Server 2005或SQL Server 2008）</span>
				</td>
			</tr>
			<tr>
				<td class="style1">
					数据库服务器：
				</td>
				<td class="style2">
					<asp:TextBox class="textbox" ID="tbDbServer" runat="server" 
						TextMode="SingleLine"></asp:TextBox>
				</td>
				<td>
					&nbsp;
				</td>
			</tr>
			<tr>
				<td class="style1">
					数据库名称：
				</td>
				<td class="style2">
					<asp:TextBox class="textbox" ID="tbDbName" runat="server" TextMode="SingleLine"></asp:TextBox>
				</td>
				<td>
					&nbsp;
				</td>
			</tr>
			<tr>
				<td class="style1">
					数据库用户名：
				</td>
				<td class="style2">
					<asp:TextBox class="textbox" ID="tbDbUser" runat="server" TextMode="SingleLine"></asp:TextBox>
				</td>
				<td>
					&nbsp;
				</td>
			</tr>
			<tr>
				<td class="style1">
					数据库密码：
				</td>
				<td class="style2">
					<asp:TextBox class="textbox" ID="tbDbPWD" runat="server" TextMode="Password"></asp:TextBox>
				</td>
				<td>
					&nbsp;
				</td>
			</tr>
			<asp:PlaceHolder ID="PlaceHolder1" runat="server">
			<tr>
				<td class="style1">
					IP数据库</td>
				<td class="style2">
					<asp:CheckBox ID="cbImportIP" runat="server" Text="导入IP数据库" />
					<span style="color:Red;">(用于在线客服定位访客)</span></td>
				<td>
					&nbsp;</td>
			</tr>
			</asp:PlaceHolder>
		</table>
	</div>
	<div class="table_blue">
		<table cellpadding="0" cellspacing="0">
			<tr class="header">
				<td colspan="3">
					用户数据</td>
			</tr>
			<tr>
				<td class="style1">
					数据文件位置：
				</td>
				<td class="style2">
					<asp:TextBox class="textbox" ID="tbFiles" runat="server" TextMode="SingleLine">App_Data</asp:TextBox>
					<br />
					<br />
					<div style="text-align:left; width:530px; line-height:1.8em; ">
						<span class="style3">
						用户的所有数据文件（头像，消息图片，附件等）默认存放在App_Data中。<br />
						您可以重新指定一个文件夹用于存放用户数据：<br />
						1、使用绝对路径，例如: <span style="color:Red;">E:\FILES</span><br />
						2、使用相对路径（相对根目录wwwroot），例如：<span style="color:Red;">..\App_Data(相对根目录wwwroot上一级目录)</span>
						</span>
					</div>
				</td>
				<td>
					&nbsp;
				</td>
			</tr>
			<tr>
				<td class="style1">
					&nbsp;
				</td>
				<td style="text-align: right;" class="style2">
					<asp:Button ID="btnInstall" runat="server" Text="安&nbsp;&nbsp;装" Width="79px" 
						onclick="btnInstall_Click" style="margin-right: 16px;"/>
				</td>
				<td>
					&nbsp;
				</td>
			</tr>
			</table>
	</div>
	</form>
</body>
</html>
