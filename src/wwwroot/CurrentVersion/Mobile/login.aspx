<%@ Page Language="C#" AutoEventWireup="true" Inherits="Core.Web.Mobile_Login" %>

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
        .div_login_user,
        .div_login_pwb,
        .div_login_btn
        {
            text-align: center;
            padding: 10px;
            width: 100%;
        }
        .login_user,
        .login_pwb,
        .login_btn 
        {
            width: 70%;
            font-size: 20px;
        }
        
        .login_user,
        .login_pwb
        {
            display:inline;
        }
    </style>
</head>
<body>
<script>
    layui.config({
        version: true
    }).use('mobile', function () {
        var mobile = layui.mobile;
    });
</script>
    <form id="form1" runat="server">
    <div class="div_top_margin">&nbsp;&nbsp;</div>
    <div class="div_login_user">
        <input type="text" name="user" autocomplete="off" class="layui-input login_user"/>
    </div>
    <div class="div_login_pwb">
        <input type="password" name="pwd" autocomplete="off" class="layui-input login_pwb" />
    </div>
    <div class="div_login_btn">
        <button type="submit" class="layui-btn layui-btn-normal login_btn" name="login">登&nbsp;&nbsp;录</button>
    </div>
    <div class="div_login_btn">
        <button type="submit" class="layui-btn layui-btn-normal login_btn" name="visitor">访&nbsp;&nbsp;客</button>
    </div>
    </form>
</body>
</html>
