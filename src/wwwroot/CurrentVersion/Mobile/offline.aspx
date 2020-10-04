<%@ Page Language="C#" AutoEventWireup="true" Inherits="Core.Web.Mobile_Offline" %>

<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, height=device-height, user-scalable=no, initial-scale=1.0, minimum-scale=1.0, maximum-scale=1.0">
<meta name="format-detection" content="telephone=no">
<title>云骞移动版</title>
    <link rel="stylesheet" href="../layim/css/layui.css">
    <script type="text/javascript" src="../layim/layui.js"></script>
    <style type="text/css">
        .div_top_margin
        {
            width: 100%;
            height: 200px;
        }
		.offline_tip
		{
			width: 100%;
			height: 40px;
			font-size: 20px;
			color: red;
			text-align:center;
		}
        .div_login_btn
        {
            text-align: center;
            width: 100%;
        }
        .login_btn 
        {
            font-size: 20px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div class="div_top_margin">&nbsp;&nbsp;</div>
    <div>
		<label class="layui-form-label offline_tip"><%=Tip %></label>
    </div>
    <div class="div_login_btn">
        <button name="relogin" type="submit" class="layui-btn layui-btn-normal login_btn">重新登录</button>
    </div>
    </form>
</body>
</html>
