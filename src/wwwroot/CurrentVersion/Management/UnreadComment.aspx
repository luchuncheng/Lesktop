<%@ Page Language="C#" AutoEventWireup="true" Inherits="Comment_UnreadCommand" %>

<%@ Register src="CommenPanel.ascx" tagname="CommenPanel" tagprefix="uc1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>无标题页</title>
	
	<link href='../Themes/Default/Common.css' rel='stylesheet' type='text/css' />	
	<link href='../Themes/Default/Comment.css' rel='stylesheet' type='text/css' />	
	
	<script language="javascript" type="text/javascript">
	
	window.onload = function()
	{
		parent.CurrentWindow.Completed();
		document.onkeydown = function(evt)
		{
			if(evt==undefined) evt = event;
			if(evt.keyCode==116 || (evt.ctrlKey && evt.keyCode==82))
			{
				evt.keyCode=0; 
				evt.returnValue=false;
				return false;
			}
			if(evt.keyCode == 70 && evt.ctrlKey && !evt.altKey && !evt.shiftKey)
			{
				evt.keyCode=0; 
				evt.returnValue=false;
				return false;
			}
		}
	}
	function DoCommand(cmd,data)
	{
		if(cmd == "Delete")
		{
			if(!confirm("你确定要删除指定的留言？")) return;
		}
		parent.CurrentWindow.Waiting(cmd == "Delete" ? "正在删除留言..." : "正在处理...");
		document.getElementById("hidCommnad").value=cmd;
		document.getElementById("hidData").value=data;
		document.getElementById("form1").submit();
	}
	</script>
</head>
<body>
	<div class="nodata" style="display:<%= Count>0?"none":"block" %>">
	没有未读留言
	</div>
	<form id="form1" runat="server">
	<input runat="server" id="hidCommnad" name="hidCommnad" type="hidden" />
	<input runat="server" id="hidData" name="hidCommnad" type="hidden" />
	</form>
</body>
</html>
