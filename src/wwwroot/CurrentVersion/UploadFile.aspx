<%@ Page Language="C#" AutoEventWireup="true" Inherits="Lesktop_CurrentVersion_UploadFile" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title></title>
	<script src="Core/Common.js" type="text/javascript"></script>
	<style type="text/css">
		html
		{
			overflow: hidden;
			border: 0px;
		}
		
		body
		{
			background-color: Transparent;
			margin: 0px;
			padding: 0px;
			overflow: hidden;
			font-family: 宋体;
			font-size: 12px;
			border: 0px;
		}
		
		*
		{
			cursor: default;
		}
		
		#file
		{
			border:solid 1px #D0D0D0;
			font-size: 50px;
			filter: alpha(opacity=0);
			-moz-opacity: 0;
			opacity: 0;
		}
	</style>
	<script type="text/javascript" language="javascript">
		var fileid_ = "";

		function file_onchange()
		{
			if(window.UploadHandler == undefined) return;
			var name = document.getElementById("file").value;
			if(!window.UploadHandler.BeforeUpload(fileid_, name)) return;
			document.getElementById("form1").submit();
		}

		function Load()
		{
			fileid_ = document.getElementById("fileid").value;

			var json = document.getElementById("json").value;
			if(json != "")
			{
				var data = Core.Utility.ParseJson(json);
				if(window.UploadHandler != undefined)
				{
					if(data.Result)
					{
						window.UploadHandler.OnCompleted(fileid_, data.Path);
					}
					else
					{
						window.UploadHandler.OnError(fileid_, data.Exception);
					}
				}
			}
		}
	</script>
</head>
<body>
	<form id="form1" runat="server">
		<asp:FileUpload ID="file" runat="server" size="0" style="width: 100px; height: 100px;" onchange="return file_onchange()" />
		<input id="json" type="hidden" runat="server" />
		<input id="fileid" type="hidden" runat="server" />
	</form>
</body>
</html>
