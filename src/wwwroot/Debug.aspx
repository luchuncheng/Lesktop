<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Debug.aspx.cs" Inherits="_Debug" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
	<title></title>
	<script language="javascript" type="text/javascript">
		document.writeln('<script type="text/javascript" src="Embed.ashx?' + (new Date()).getTime() + '"><' + '/script>');
		window.onload = function ()
		{
			StartService();
		}
	</script>
</head>
<body>
	<form id="form1" runat="server">
	<div style="height: 1000px;">
	</div>
	</form>
</body>
</html>
