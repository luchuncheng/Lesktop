var ClientMode = false;

if (__embed_config.Init == undefined)
{
	__embed_config.Config = {
		Version: "{VERSION}",
		CSUrl: "{CSURL}",
	};

	__embed_config.EncodeReferrer = function(str)
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

	__embed_config.Format = function(fmt)
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

	window.StartChat = function(peer, tab)
	{
		if (peer == undefined || peer == null) peer = __embed_config.User.Name;
		if (tab == undefined || tab == null) tab = 0;

		var url = __embed_config.Format(
			__embed_config.Config.CSUrl + "?Peer={0}&random={1}",
			peer, (new Date()).getTime()
		);

		var __lesktop_desktop = open(
			url, "__lesktop_desktop",
			__embed_config.Format("left={0}, top={1}, height=500, width=700, toolbar=no, menubar=no, scrollbars=no, resizable=yes,location=no, status=no", (window.screen.availWidth - 10 - 700) / 2, (window.screen.availHeight - 30 - 500) / 2)
		);
		__lesktop_desktop.blur();
		__lesktop_desktop.focus();
		window.blur();
	}

	__embed_config.window_onload = function()
	{
		if (window.__embed_cs_init != undefined) window.__embed_cs_init();
	}

	if (window.attachEvent)
	{
		window.attachEvent("onload", __embed_config.window_onload);
	}
	else if (window.addEventListener)
	{
		window.addEventListener("load", __embed_config.window_onload, false);
	}

	__embed_config.Init = true;
}

(function(){
	var user = __embed_config.User;
	var html = __embed_config.Format(
		"<a href='javascript:StartChat(\"{0}\")'>{1}({2})</a>",
		user.Name, user.Nickname, user.State == "Online" ? "在线" : "离线"
	);
	document.write(html);
})();