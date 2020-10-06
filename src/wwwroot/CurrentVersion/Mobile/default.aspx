<%@ Page Language="C#" AutoEventWireup="true" Inherits="Core.Web.Mobile_Default" %>

<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, height=device-height, user-scalable=no, initial-scale=1.0, minimum-scale=1.0, maximum-scale=1.0">
    <meta name="format-detection" content="telephone=no">
    <title>云骞移动版</title>
    <link rel="stylesheet" type="text/css" href="../layim/css/layui.mobile.css" />
    <script src="../layim/layui.js"></script>
	<script language="javascript" type="text/javascript">
	    window.ClientMode = false;
	    window.Device = 2; // 0 - web, 1 - client, 2 - mobile web
	</script>
	<script type="text/javascript" src="../../Embed.ashx?device=2&t=<%=DateTime.Now.Ticks.ToString() %>"></script>
	<script type="text/javascript" src="mobile.js"></script>
	<script language="javascript" type="text/javascript">
		window.MobileInitParams = <%=InitParams%>;
	</script>
	<script language="javascript" type="text/javascript">
		window.onload = function ()
		{
		    LayIM_Init();
		}
	</script>
</head>
<body>
</body>
</html>
