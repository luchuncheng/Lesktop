<%@ Page Language="C#" AutoEventWireup="true" Inherits="Core.Web.Lesktop_Management_UpdateUserInfo" validateRequest="false" %>

<%@ Register src="../Ctrl/CommandCtrl.ascx" tagname="CommandCtrl" tagprefix="uc1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title></title>
	<link href='../Themes/Default/Common.css' rel='stylesheet' type='text/css' />	
	<link href='../Themes/Default/Table.css' rel='stylesheet' type='text/css' />
	
	<link rel="stylesheet" href="../../kissy_editor_1.0.0_build242/themes/default/editor-min.css" type="text/css"/>
	
	<style type="text/css">
		.col1
		{
			width:48px;
			text-align:left;
			vertical-align: top;
		}
		.col2
		{
			width: 200px;
			text-align:left;
			vertical-align: top;
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
		
		#file_headimg_con
		{
			margin-top:6px;
			background:url("../Themes/Default/Images/upload_file.png") no-repeat -3px 0px;
		}
		
		#file_headimg_con:hover
		{
			margin-top:6px;
			background:url("../Themes/Default/Images/upload_file.png") no-repeat -63px 0px;
		}

		#file_headimg
		{
			filter: alpha(opacity=0);
			-moz-opacity: 0;
			opacity: 0;
			left:0px;
			top:0px;
			height: 16px;
			width: 60px;
			cursor:pointer;
			float:left;
		}
		
		#headimg
		{
			width: 150px;
			height:150px;
		}
	</style>

	<script src="../KindEditor/3.5.2/kindeditor.js" type="text/javascript"></script>

	<script type="text/javascript" language="javascript">
			
		KE.show({
			id : 'remarkEditor',
			resizeMode : 1,
			allowPreviewEmoticons : false,
			allowUpload: false,
			linkAlwaysNew: true,
			items : [
			'fontname', 'fontsize', '|', 'textcolor', 'bgcolor', 'bold', 'italic', 'underline',
			'removeformat', '|', 'link']
		});
			
		var CurrentWindow = parent.CurrentWindow;
		var RemarkEditor = null;

		window.onload = function()
		{
			CurrentWindow.Completed();
			
			var state = GetState();
			if (state.Action == "Error")
			{
				parent.Core.Utility.ShowError(state.Exception.Message);
			}
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
			
			var img =  document.getElementById("headimg").firstChild;
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
			return Match(/[0-9a-zA-Z]{4,10}/ig, pwd);
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
			
			CurrentWindow.Waiting("正在更新群组资料...");
			DoCommand(
				"Update", 
				{
					Nickname: nickname.value,
					Remark: KE.html('remarkEditor')
				}
			);
		}
	</script>
</head>
<body>
	<form id="form1" runat="server">
	<div class="table_blue_ng">
		<table class='table' cellpadding="0" cellspacing="0">
			<tr class="tr header">
				<td colspan="3" class='td col2'>群组资料</td>
			</tr>
			<tr class="tr">
				<td class='td col1'>群名称：</td>
				<td class='td col2'><input id="nickname" type="text" value="<%= Info.Nickname%>" /></td>
				<td class='td col3'>&nbsp;</td>
			</tr>
			<tr class="tr">
				<td class='td col1'>群公告：</td>
				<td class='td col2'>
					<textarea name="remarkEditor" id="remarkEditor" rows="50" cols="152" style="width: 400px; height: 150px;"><%= Info.Remark%></textarea>
				</td>
				<td class='td col3'>&nbsp;</td>
			</tr>
			<tr class="tr">
				<td colspan="3" class='td operation_bottom'><a href='javascript:Update();'>更新群组资料</a></td>
			</tr>
		</table>
	</div>
	<uc1:CommandCtrl ID="CommandCtrl1" runat="server" />
	</form>
	
</body>
</html>
