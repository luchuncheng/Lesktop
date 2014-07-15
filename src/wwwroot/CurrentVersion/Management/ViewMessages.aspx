<%@ Page Language="C#" AutoEventWireup="true" Inherits="Core.Web.Message_ViewMessages" %>

<%@ Register Src="MessageCtrl.ascx" TagName="MessageCtrl" TagPrefix="uc1" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>无标题页</title>
	<link href='../Themes/Default/Common.css' rel='stylesheet' type='text/css' />	
	<link href='../Themes/Default/ViewMessages.css' rel='stylesheet' type='text/css' /> 
	<link href='../Themes/Default/EditorCss.css' rel='stylesheet' type='text/css' />

	<script language="javascript" type="text/javascript">

		var Core = parent.Core;
		var ClientMode = parent.ClientMode;

		var FileHtmlFormat = "";

		if (!ClientMode)
		{
			FileHtmlFormat = "<div class='div_filename'>文件名:{0}</div>" +
			"<div class='operationContainer'>" +
			"<div class='link_download'><a target='_blank' href='" + Core.GetUrl("Download.ashx") + "?FileName={1}'>下载</a></div>" +
			"</div>";
		}
		else
		{
			FileHtmlFormat = "<div class='div_filename'>文件名:{0}</div>" +
			"<div class='operationContainer'>" +
			"<div class='link_saveas'><span onclick='javascript:OpenFile(unescape(\"{1}\"),this.parentNode.parentNode.parentNode.childNodes[2])'>打开</span></div>" +
			"<div class='link_saveas'><span onclick='javascript:SaveFile(unescape(\"{1}\"),this.parentNode.parentNode.parentNode.childNodes[2])'>另存为</span></div>" +
			"</div>" +
			"<div class='download_tip' style='display:none;'></div>";
		}

		String.format = function(fmt)
		{
			var params = arguments;
			var pattern = /{{|{[1-9][0-9]*}|\x7B0\x7D/g;
			return fmt.replace(
			pattern,
			function(p)
			{
				if (p == "{{") return "{";
				return params[parseInt(p.substr(1, p.length - 2), 10) + 1]
			}
		);
		}

		function DownloadHandler(file, domContainer)
		{
			var html = String.format(
				"<div class='dl_image_dl'></div><div class='dl_text'><span class='span_normal'>正在下载 \"{0}\"...</span><span class='processing'></span><a>取消</a></div>",
				file
			);
			var dom = document.createElement("DIV");
			dom.className = 'message_file_dl';
			dom.innerHTML = html;

			var info_dom = dom.childNodes[1].childNodes[0];
			var pdom = dom.childNodes[1].childNodes[1];
			var a_dom = dom.childNodes[1].childNodes[2];
			var img_dom = dom.childNodes[0];

			var _isCanceled = false;
			a_dom.onclick = function()
			{
				_isCanceled = true;
			}

			var _recv = 0, _total = 0;

			this.BeforeDownload = function()
			{
				domContainer.style.display = "block";
				domContainer.appendChild(dom);
			}

			this.Processing = function(length, size, speed)
			{
				_recv = length;
				_total = size;
				var temp;
				if (size > 1024 * 1024 * 1024) temp = Math.round(size / (1024 * 1024 * 1024) * 100) / 100 + "GB";
				else if (size > 1024 * 1024) temp = Math.round(size / (1024 * 1024) * 100) / 100 + "MB";
				else if (size > 1024) temp = Math.round(size / 1024 * 100) / 100 + "KB";
				else temp = size + "B";
				pdom.innerHTML = String.format("下载速度:{2}KB/s 已完成:{0}%, 共 {1}", (Math.round(length / size * 1000) / 10), temp, Math.round(speed / 10) / 100);
				return _isCanceled ? 0 : 1;
			}

			this.AfterDownload = function()
			{
				info_dom.innerHTML = String.format(_recv == _total ? "\"{0}\" 下载完毕" : "\"{0}\" 下载已取消！", file);
				info_dom.className = (_recv == _total ? "span_normal" : "span_red");
				pdom.innerHTML = "";
				a_dom.style.display = 'none';
				img_dom.className = (_recv == _total ? "dl_image_file" : "dl_image_cancel");
			}

			this.HandleError = function(msg)
			{
				info_dom.className = "span_red";
				info_dom.innerHTML = String.format("下载 \"{0}\" 时发生错误: {1}", file, msg);
				pdom.innerHTML = "";
				a_dom.style.display = 'none';
				img_dom.className = "dl_image_error";
			}
		}

		function SaveFile(filePath, domContainer)
		{
			if (ClientMode)
			{
				parent.window.external.Save(
				document.cookie,
				parent.Core.GetUrl("Download.ashx?FileName=" + filePath),
				Core.Path.GetFileName(filePath),
				new DownloadHandler(Core.Path.GetFileName(filePath), domContainer)
			);
			}
		}

		function OpenFile(filePath, domContainer)
		{
			if (ClientMode)
			{
				parent.window.external.Open(
				document.cookie,
				parent.Core.GetUrl("Download.ashx?FileName=" + filePath),
				Core.Path.GetFileName(filePath),
				new DownloadHandler(Core.Path.GetFileName(filePath), domContainer)
			);
			}
		}

		function OpenLink(href)
		{
			return confirm(String.format("您确定要打开\"{0}\"?", href));
		}

		function CreateMsgHtml(content, receiver)
		{
			content = content.replace(/<img [^\t\n\r\f\v<>]+>/ig,
				function(img)
				{
					return img.replace(
						/src[^<>]*=[^<>]*(\x22|\x27)([^<>]+\/|)Download.ashx\x3FFileName=([^\t\n\r\f\v\x22]+)(\x22|\x27)/ig,
						function(text, v1, v2, file)
						{
							var url = Core.CreateDownloadUrl(file.substr(0, 1) == "/" ? file : "/" + receiver + "/" + file);
							return String.format("src=\"{0}\"", url);
						}
					);
				}
			);
		
			content = content.replace(
				/\x5BFILE\x3A([^\t\n\r\f\v\x5B\x5D]+)\x5D/ig,
				function(val, file)
				{
					return String.format(
						FileHtmlFormat,
						Core.Path.GetFileName(unescape(file)),
						file.substr(0, 1) == "/" ? file : "/" + receiver + "/" + file
					);
				}
			);

			content = content.replace(
				/\x5BA\x3A([^\t\n\r\f\v\x5B\x5D]+)\x5D/ig,
				function(val, file)
				{
					return String.format("<a target='_blank' href='{0}' onclick='return window.OpenLink(this.href);'>", unescape(file));
				}
			);

			content = content.replace(
				/\x5B\x2FA\x5D/ig,
				function(val)
				{
					return "</a>";
				}
			);

			return content;
		}

		var Params = (function()
		{
			var pairs = window.location.search.substr(1, window.location.search.length - 1).split("&");
			var params = {};
			for (var i in pairs)
			{
				var vs = pairs[i].split("=");
				params[vs[0]] = vs[1];
			}
			return params;
		})();

		function Refresh()
		{
			Params["random"] = (new Date()).getTime();

			var qs = "";
			for (var key in Params)
			{
				if (qs != "") qs += "&";
				qs += (key + "=" + Params[key]);
			}

			parent.CurrentWindow.Waiting("正在载入消息记录...");
			window.location = "ViewMessages.aspx?" + qs;
		}

		function GetPageCount()
		{
			return parseInt(document.getElementById("hiddenPageCount").value);
		}

		function GetCurrentPage()
		{
			if (Params["CurrentPage"] != undefined)
			{
				var cp = parseInt(Params["CurrentPage"]);
				return cp == -1 ? GetPageCount() : cp;
			}
			else if (document.getElementById("hiddenCurrentPage").value != "")
			{
				var cp = parseInt(document.getElementById("hiddenCurrentPage").value);
				return cp == -1 ? GetPageCount() : cp;
			}
			else
			{
				return GetPageCount();
			}
		}

		function SwitchMessageVisible(tab_id, content, receiver)
		{
			var tab = document.getElementById(tab_id);
			if (tab.rows[0].cells[3].firstChild.style.display == 'none')
			{
				tab.rows[0].cells[3].firstChild.style.display = "inline";
				tab.rows[1].style.display = "none";
			}
			else
			{
				tab.rows[0].cells[3].firstChild.style.display = "none";
				tab.rows[1].style.display = "";
				if (tab.rows[1].cells[1].firstChild.innerHTML == "")
				{
					tab.rows[1].cells[1].firstChild.innerHTML = CreateMsgHtml(content, receiver);
				}
			}
		}

		function First()
		{
			Params["CurrentPage"] = 1;
			Refresh();
		}

		function Last()
		{
			Params["CurrentPage"] = -1;
			Refresh();
		}

		function Prior()
		{
			var cp = GetCurrentPage();
			Params["CurrentPage"] = (cp == 1 ? 1 : cp - 1);
			Refresh();
		}

		function Next()
		{
			var cp = GetCurrentPage();
			Params["CurrentPage"] = (cp == GetPageCount() ? GetPageCount() : cp + 1);
			Refresh();
		}

		function page_onkeydown(evt)
		{
			if (evt == undefined) evt = window.event;
			if (evt.keyCode == 13)
			{
				var tp = parseInt(document.getElementById("page").value);

				if (tp > GetPageCount()) tp = GetPageCount();
				if (tp < 0) tp = 0;

				if (tp == GetCurrentPage()) return;

				Params["CurrentPage"] = tp;
				Refresh();
				if (evt && evt.stopPropagation) evt.stopPropagation();
				else evt.cancelBubble = true;

				return false;
			}
		}

		window.onload = function()
		{
			parent.CurrentWindow.Completed();
		}

		window.onunload = function()
		{
		}

		function SelectAll()
		{
			var cbAll = document.getElementById("cbAll");
			var div_msg = document.getElementById("MsgDiv");
			if (cbAll != null && div_msg != null)
			{
				var ids = "";
				for (var i = 0; i < div_msg.childNodes.length; i++)
				{
					var cn = div_msg.childNodes[i];
					if (cn.tagName != undefined && cn.tagName.toUpperCase() == "TABLE")
					{
						cn.rows[0].cells[0].firstChild.checked = cbAll.checked;
					}
				}
			}
		}

		function Delete()
		{
			var div_msg = document.getElementById("MsgDiv");
			if (div_msg != null)
			{
				var ids = "";
				for (var i = 0; i < div_msg.childNodes.length; i++)
				{
					var cn = div_msg.childNodes[i];
					if (cn.tagName != undefined && cn.tagName.toUpperCase() == "TABLE")
					{
						if (cn.rows[0].cells[0].firstChild.checked)
						{
							if (ids != "") ids += ",";
							ids += cn.rows[0].cells[0].firstChild.name;
						}
					}
				}
				if (ids != null)
				{
					if (confirm("你确定要删除选中的消息"))
					{
						parent.CurrentWindow.Waiting("正在删除...");
						document.getElementById("command").value = "Delete";
						document.getElementById("data").value = ids;
						document.getElementById("form1").submit();
					}
				}
				else
				{
					alert("请选择要删除的消息");
				}
			}
		}
		
		function ViewList(url)
		{
			window.location = url;
		}
	</script>

</head>
<body>
	<form id="form1" runat="server">
	<div>
		<div class="operation">
			<div style="float: left; height: 14px;">
				<a class="link" href='javascript:ViewList(<%= Core.Utility.RenderJson(ReturnUrl)%>)'>返回消息列表</a> <a class="link" href="javascript:Delete()">
					删除所选</a>
			</div>
			<div style="float: right; height: 12px;">
				<a class="<%= CurrentPage == 1 ? "link_invalid" : "link"%>" href="<%= CurrentPage == 1 ? "javascript:void(0);" : "javascript:First();"%>">
					第一页</a> <a class="<%= CurrentPage == 1 ? "link_invalid" : "link"%>" href="<%= CurrentPage == 1 ? "javascript:void(0);" : "javascript:Prior();"%>">
						上一页</a> 第<input type="text" id="page" value="<%= CurrentPage %>" onkeydown="return page_onkeydown();" />页/共<%= PageCount %>页
				<a class="<%= CurrentPage == PageCount ? "link_invalid" : "link"%>" href="<%= CurrentPage == PageCount ? "javascript:void(0);" : "javascript:Next();"%>">
					下一页</a> <a class="<%= CurrentPage == PageCount ? "link_invalid" : "link"%>" href="<%= CurrentPage == PageCount ? "javascript:void(0);" : "javascript:Last();"%>">
						最后一页</a>
			</div>
		</div>
		<br />
		<table id="header" cellpadding="0" cellspacing="0" class="messageCtrl" style="border-top: solid 1px #C4D6EC;
			font-weight: bold; background-color: #F3F7FA;">
			<tr>
				<td class="checkbox">
					<input type="checkbox" id="cbAll" onclick="SelectAll();" />
				</td>
				<td class="nickname" style="color: #074977;">
					发送人
				</td>
				<td class="nickname" style="color: #074977;">
					接收人
				</td>
				<td class="summary" style="color: #074977;">
					内容
				</td>
				<td class="createdTime" style="color: #074977;">
					发送时间
				</td>
			</tr>
		</table>
		<div id="MsgDiv">
			<asp:PlaceHolder ID="MsgContainer" runat="server"></asp:PlaceHolder>
		</div>
	</div>
	<input id="hiddenPageCount" type="hidden" value="<%= PageCount %>" />
	<input id="hiddenCurrentPage" type="hidden" value="<%= CurrentPage %>" />
	<input runat="server" id="data" type="hidden" value="" />
	<input runat="server" id="command" type="hidden" value="" />
	</form>
</body>
</html>
