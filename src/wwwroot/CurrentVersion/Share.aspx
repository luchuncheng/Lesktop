<%@ Page Language="C#" AutoEventWireup="true" Inherits="Lesktop_Management_Share" %>

<%@ Register src="Ctrl/CommandCtrl.ascx" tagname="CommandCtrl" tagprefix="uc1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title></title>
	<link href='Themes/Default/Table.css' rel='stylesheet' type='text/css' />
	
	<style type="text/css">
		body
		{
			margin: 8px;
		}
		.filename
		{
		}
		.size
		{
			width: 80px;
			text-align: right;
		}
		.operation
		{
			width: 100px;
			text-align: right;
		}
		.filename input
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
			if (parent.ClientMode)
			{
				document.getElementById("upload_file").style.display = "none";
			}
			var state = GetState();
			
			if (state.Action == "Error")
			{
				parent.Core.Utility.ShowError(state.Exception.Message);
			}
		}

		function Upload()
		{
			var f = document.getElementById("upload_file").value;
			if (f == "")
			{
				alert("请选择要上传的文件！");
				return;
			}
			
			CurrentWindow.Waiting("正在上传文件，请稍候...");
			DoCommand("Upload", {});
			return;
		}

		function Delete(filename)
		{
			if (confirm(String.format("您确定要删除文件\"{0}\"", filename)))
			{
				CurrentWindow.Waiting("正在删除文件，请稍候...");
				DoCommand("Delete", { FileName: filename });
			}
		}
	</script>
</head>
<body>
	<form id="form1" runat="server" enctype="multipart/form-data">
	<div class="table_blue">
		<table cellpadding="0" cellspacing="0">
			<tr class='header'>
				<td class='filename'>文件名</td>
				<td class='size'>大小</td>
				<td class='operation'>操作</td>
			</tr>
			<% = RenderList() %>
			<asp:PlaceHolder ID="PlaceHolder1" runat="server">
			<tr>
				<td class='filename'><input type="file" id="upload_file" name="upload_file" /></td>
				<td class='size'>&nbsp;</td>
				<td class='operation'><a href='javascript:Upload();'>上传文件</a></td>
			</tr>
			</asp:PlaceHolder>
		</table>
	</div>
	<uc1:CommandCtrl ID="CommandCtrl" runat="server" />
	</form>
</body>
</html>
