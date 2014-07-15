(function() {

if(window.Core == undefined) window.Core = {};
if(window.Core.UI == undefined) window.Core.UI = {};
	
Core.UI.LoginPanel = function (container_, login_callback_, register_callback_)
{	
	var this_ = this;

	var id_ = 'login_panel_' + Core.GenerateUniqueId();
	
	var html = 
	"<div class='ct login_panel'>" +
		"<div class='logo'></div>" +
		"<div class='login'>" +
			"<div class='link_container'>" +
				"<a class='ct_link' type='button'>下载客户端</a>" +
				"<a class='ct_link' type='button'>注册新用户</a>" +
			"</div>" +
			"<div class='client_opt'>" +
				"<label for='cb_autologin'>自动登录</label><input id='cb_autologin' type='checkbox' />" +
				"<label for='cb_autostart'>系统启动时自动启动</label><input id='cb_autostart' type='checkbox' />" +
			"</div>" +
			"<input class='ct_button viewreg_button' type='button' value='注&nbsp;册'/>" +
			"<input class='ct_button login_button' type='button' value='登&nbsp录'/>" +
		"</div>" +
		"<div class='register'>" +
			"<div class='link_container'>" +
				"<a class='ct_link' type='button'>返回登录页面</a>" +
			"</div>" +
			"<input class='ct_button register_button' type='button' value='注&nbsp;册'/>" +
		"</div>" +
	"</div>"
	
	container_.innerHTML = html;

	if(window.ClientMode == true)
	{
		Core.Utility.ModifyCss(container_.firstChild, "clientmode");
	}
	
	var login_page_ = container_.firstChild.childNodes[1];
	var login_link_container_ = login_page_.firstChild;
	var viewreg_button_ = login_page_.childNodes[2];
	var login_button_ = login_page_.childNodes[3];	
	var register_link_ = login_link_container_.childNodes[1];	
	var download_link_ = login_link_container_.childNodes[0];	
	
	var client_opt_ = login_page_.childNodes[1];
	var autostart_ = client_opt_.childNodes[3];
	var autologin_ = client_opt_.childNodes[1];

	var login_inputunits = [
		new Core.UI.InputUnit(
			"用户名：", "text", "login_name", 32, "", "请输入用户名！", 
			[/[\x5Fa-zA-Z0-9]{4,32}/ig, "无效的用户名"]
		),
		new Core.UI.InputUnit(
			"密&nbsp;&nbsp;码：", "password", "login_password", 16, "", null
		)
	];
	for(var i in login_inputunits)
	{
		login_page_.insertBefore(login_inputunits[i].GetDom(), login_link_container_);
	}
	
	var register_page_ = container_.firstChild.childNodes[2];	
	var register_link_container_ = register_page_.firstChild;
	var register_button_ = register_page_.childNodes[1];	
	var back_link_ = register_link_container_.childNodes[0];
	var register_inputunits = [
		new Core.UI.InputUnit(
			"用户名：", "text", "register_name", 32, "请输入4到32位字符或数字", "请输入用户名！",
			[/[a-zA-Z0-9]{4,32}/ig, "无效的用户名"]
		),
		new Core.UI.InputUnit(
			"昵&nbsp;&nbsp;称：", "text", "register_nickname", 32, "请输入1到32位字符或数字", "请输入昵称！"
		),
		new Core.UI.InputUnit(
			"登录密码：", "password", "register_password", 16, "请输入6到16位字符", "请输入密码！",
			[/\S{6,16}/g, "无效的密码！"]
		),
		new Core.UI.InputUnit(
			"密码确认：", "password", "register_password_confirm", 16, "请输入6到16位字符", "请再次输入密码！",
			[function (value) { return register_inputunits[2].GetValue() == value }, "两次输入的密码不一致！"]
		)
	];

	for(var i in register_inputunits)
	{
		register_page_.insertBefore(register_inputunits[i].GetDom(), register_link_container_);
	}

	this_.GetValues = function()
	{
		var values = {
			Name: login_inputunits[0].GetValue(), 
			Password: login_inputunits[1].GetValue(),
			RegName: register_inputunits[0].GetValue(), 
			RegNickname: register_inputunits[1].GetValue(), 
			RegPassword: register_inputunits[2].GetValue()
		};
		return values;
	}

	this_.SetProps = function(values)
	{
		if(values.Name != undefined) login_inputunits[0].SetValue(values.Name);
		if(values.Password != undefined) login_inputunits[1].SetValue(values.Password);
		if(values.RegName != undefined) register_inputunits[0].SetValue(values.RegName);
		if(values.RegNickname != undefined) register_inputunits[1].SetValue(values.RegNickname);
		if(values.RegPassword != undefined) register_inputunits[2].SetValue(values.RegPassword);
		if(values.AutoLogin != undefined) autologin_.checked = values.AutoLogin;
		if(values.AutoStart != undefined) autostart_.checked = values.AutoStart;
		if(values.DownloadLink != undefined)
		{
			if(values.DownloadLink == "none")
			{
				download_link_.style.display = "none";
			}
			else
			{
				download_link_.style.display = "";
				download_link_.href = values.DownloadLink;
			}
		}
		
		if(values.ShowRegLink != undefined)
		{
			register_link_.style.display = (values.ShowRegLink == false ? "none" : "");
		}

		if(values.EnableInputName != undefined)
		{
			login_inputunits[0].SetReadOnly(values.EnableInputName == false);
			if(values.EnableInputName == false)
			{
				login_link_container_.style.display = "none";
				client_opt_.style.display = "none";
				viewreg_button_.style.display = "none";
			}
		}
	}

	this_.DoLogin = function()
	{
		login_button_.click();
	}

	this_.IsAutoLogin = function()
	{
		return autologin_.checked;
	}

	this_.IsAutoStart = function()
	{
		return autostart_.checked;
	}
	
	Core.Utility.AttachEvent(
		document, "keydown",
		function(evt)
		{
			if(evt == undefined) evt = window.event;
			if(evt.keyCode == 13)
			{	
				if(login_page_.style.display != "none")
				{
					login_button_.click();
				}
				else
				{
					register_button_.click();
				}
			}
		}
	);
	
	viewreg_button_.onclick = function()
	{
		login_page_.style.display = "none";
		register_page_.style.display = "block";
	}

	register_link_.onclick = function()
	{
		login_page_.style.display = "none";
		register_page_.style.display = "block";
	}
	
	back_link_.onclick = function()
	{
		login_page_.style.display = "block";
		register_page_.style.display = "none";
	}
	
	register_button_.onclick = function()
	{
		if(Core.UI.InputUnit.MultiCheck(register_inputunits) && register_callback_)
		{
			var values = {
				RegName: register_inputunits[0].GetValue(), 
				RegNickname: register_inputunits[1].GetValue(), 
				RegPassword: register_inputunits[2].GetValue()
			};
			register_callback_(values);
		}
	}
	
	login_button_.onclick = function()
	{
		if(Core.UI.InputUnit.MultiCheck(login_inputunits) && login_callback_ != undefined)
		{
			var values = {
				Name: login_inputunits[0].GetValue(), 
				Password: login_inputunits[1].GetValue()
			};
			login_callback_(values);
		}
	}
}

})();