<%@ Page Language="C#" AutoEventWireup="true" Inherits="System.Web.UI.Page" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title></title>
</head>
<body>
	<form id="form1" runat="server">
	<div>
	
	</div>
	</form>

	<script language="javascript" type="text/javascript"> 
		var html = '<script src="../EmbedCS.ashx?ID=<%= Request.QueryString["ID"] %>&' + (new Date()).getTime() + '" type="text/javascript"></' + 'script>';
		document.writeln(html);
	</script>
</body>
</html>
