<%@ Page Language="C#" AutoEventWireup="true" Inherits="Core.Web.Lesktop_Management_EmbedCode" %>

<%@ Register src="../Ctrl/CommandCtrl.ascx" tagname="CommandCtrl" tagprefix="uc1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title></title>
	<link href='../Themes/Default/Common.css' rel='stylesheet' type='text/css' />	
	<link href='../Themes/Default/Table.css' rel='stylesheet' type='text/css' />
	<style type="text/css">
		div
		{
			scrollbar-arrow-color: #AAAAAA;
			scrollbar-face-color: #f3f3f5;
			scrollbar-darkshadow-color: #ffffff;
			scrollbar-base-color: #f3f3f5;
			scrollbar-highlight-color: #ffffff;
			scrollbar-shadow-color: #d4d4d4;
			scrollbar-track-color: #f3f3f5;
			scrollbar-3dlight-color: #d4d4d4;
		}
		.embed_cb
		{
			width: 16px;
			height: 16px;
		}
		.td_left
		{
			width: 100px;
			height: 16px;
		}
		.td_right
		{
			width: 120px;
			height: 16px;
		}
		.td_right input
		{
			width: 90%;
			border: solid 1px #D0D0D0;
			font-family: SimSun;
			font-size: 12px;
			padding: 3px;
		}
		td
		{
			font-family:SimSun;
			font-size:12px;
		}
		a
		{
			text-decoration: none;
			color: Blue;
			margin-left: 4px;
			cursor: pointer;
		}
		a:hover
		{
			text-decoration: underline;
			cursor: pointer;
		}
	</style>

	<script language="javascript" type="text/javascript">
		window.onload = function()
		{
			try { parent.CurrentWindow.Completed(); } catch (ex) { }
			
			var state = GetState();
			if (state.Action == "Error")
			{
				parent.Core.Utility.ShowError(state.Exception.Message);
			}
			else if(state.Action == "Edit")
			{
				Edit(state.ID, true);
				window.location = "#ec_" + state.ID;
			}
			else if(state.Action == "Scroll")
			{
				window.location = "#ec_" + state.ID;
			}
		}
		
		function check(prefix, data)
		{
			if(document.getElementById(prefix + '_ec_name').value == "")
			{
				alert("请输入嵌入代码名称！");
				document.getElementById(prefix + '_ec_name').focus();
				return false;
			}
			
			if(document.getElementById(prefix + '_ec_showwin').checked)
			{
				var num = parseInt(document.getElementById(prefix + '_ec_winwidth').value);
				if(isNaN(num))
				{
					alert("请输入正确的客服窗口宽度！");
					document.getElementById(prefix + '_ec_winwidth').focus();
					return false;
				}
				if(num < 120)
				{
					alert("客服窗口宽度不能小于120！");
					document.getElementById(prefix + '_ec_winwidth').focus();
					return false;
				}
				
				num = parseInt(document.getElementById(prefix + '_ec_winheight').value);
				if(isNaN(num))
				{
					alert("请输入正确的客服窗口高度！");
					document.getElementById(prefix + '_ec_winheight').focus();
					return false;
				}
				if(num < 250)
				{
					alert("客服窗口高度不能小于250！");
					document.getElementById(prefix + '_ec_winheight').focus();
					return false;
				}
			}
			
			if(document.getElementById(prefix + '_ec_users').value == "")
			{
				alert("请至少选择一个客服人员！");
				return false;
			}
			
			data.Name = document.getElementById(prefix + '_ec_name').value;
			if(document.getElementById(prefix + '_ec_showwin').checked)
			{
				data.ShowWindow = true;
				data.Width = parseInt(document.getElementById(prefix + '_ec_winwidth').value);
				data.Height = parseInt(document.getElementById(prefix + '_ec_winheight').value);
			}
			else
			{
				data.ShowWindow = false;
				data.Width = 120;
				data.Height = 250;
			}
			data.NewWindow = document.getElementById(prefix + '_ec_newwin').checked;
			data.Users = document.getElementById(prefix + '_ec_users').value;
			data.UsersPreview =  document.getElementById(prefix + '_ec_users_preview').innerHTML;
			
			return true;
		}
		
		function NewEmbedCode()
		{
			parent.CurrentWindow.Waiting("正在处理，请稍候...");
			DoCommand("Create", {});
		}
		
		function Preview(a, id)
		{
		
			function EncodeEmbedConfig(str)
			{
				var hex = "0123456789ABCDEF";
				var res = "";
				for (var i = 0; i < str.length; i++)
				{
					var code = str.charCodeAt(i);
					res += hex.charAt(code >> 12);
					res += hex.charAt((code >> 8) & 15);
					res += hex.charAt((code >> 4) & 15);
					res += hex.charAt(code & 15);
				}
				return res;
			}
			var data = {};
			if (check(id.toString(), data))
			{
				a.href = parent.String.format("../EmbedTest.aspx?ID=0&EmbedConfig={0}",EncodeEmbedConfig(Core.Utility.RenderJson(data)));
				return true;
			}
			return false;
		}
		
		function UpdateEmbedCode(id)
		{
			var data = {};
			if (check(id.toString(), data))
			{
				parent.CurrentWindow.Waiting("正在处理，请稍候...");
				DoCommand("Update", { ID: id, EmbecConfig: data });
			}
		}
		
		function DeleteEmbedCode(id, Name)
		{
			if(confirm(parent.String.format("您确定要删除嵌入代码\"{0}\"？",Name)))
			{
				parent.CurrentWindow.Waiting("正在处理，请稍候...");
				DoCommand("Delete", { ID: id});
			}
		}
		
		function SelectCSUsers(prefix)
		{
			var config = {
				Left: 0,
				Top: 0,
				Width: 350,
				Height: 400,
				MinWidth: 350,
				MinHeight: 400,
				Title: {
					InnerHTML: "选择客服人员..."
				},
				Resizable: false,
				HasMaxButton: false,
				HasMinButton: true,
				AnchorStyle: parent.Core.WindowAnchorStyle.Left | parent.Core.WindowAnchorStyle.Top
			}
			var tag = {
				OnSelected: function(ids, users, depts)
				{
					var tree = {};
					var id_arr = ids.split(",");
					for (var i = 0; i < id_arr.length; i += 2)
					{
						if (tree[parseInt(id_arr[i + 1])] == undefined) tree[parseInt(id_arr[i + 1])] = [];
						tree[parseInt(id_arr[i + 1])].push(parseInt(id_arr[i]));
					}
					var html = [];
					for (var k in tree)
					{
						html.push(parent.String.format("{0}", depts[k].Name));
						for (var i in tree[k])
						{
							html.push(parent.String.format("  {0}({1})", users[tree[k][i]].Nickname, users[tree[k][i]].Name));
						}
					}
					document.getElementById(prefix + '_ec_users_preview').innerHTML = "<pre>" + html.join("\r\n") + "\r\n</pre>";
					document.getElementById(prefix + '_ec_users').value = ids;
				}
			};
			var form = parent.Core.CreateWindow(config);
			form.SetTag(tag);
			form.MoveEx('CENTER', 0, 0, true);
			form.ShowDialog(parent.CurrentWindow, "center", 0, 0, true, null);
			var url = parent.Core.GetUrl("SelCSUsersForm.htm");
			form.Load(url, null);
		}
		
		function Edit(id, visible)
		{
			if(visible)
			{
				document.getElementById(id+"_info").style.display = "none";
				document.getElementById(id+"_editor").style.display = "block";
				window.location = "#ec_" + id;
			}
			else
			{
				document.getElementById(id+"_info").style.display = "block";
				document.getElementById(id+"_editor").style.display = "none";
			}
		}
		
		function Parse(text)
		{
			if(parent.ClientMode) window.external.Parse(text);
			else window.clipboardData.setData('text', text);
			alert("代码已复制到剪切板！");
		}
	</script>

</head>
<body>
	<form id="form1" runat="server">
	<div class="table_blue">
		<table cellpadding="0" cellspacing="0">
			<tr class="header">
				<td style="text-align: right; font-weight:normal;">
					<a href="javascript:NewEmbedCode();">创建嵌入代码</a>
				</td>
			</tr>
		</table>
	</div>
	<br />
	<asp:PlaceHolder ID="PlaceHolder1" runat="server"></asp:PlaceHolder>
	<uc1:CommandCtrl ID="CommandCtrl" runat="server" />
	</form>
</body>
</html>
