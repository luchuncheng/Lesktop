var ClientMode = false;

__embed_config.NewWindow = false;

__embed_config.Config = {
	Version: "{VERSION}",
	ServiceUrl: "{SERVICEURL}"
};

__embed_config.Depts = {};
__embed_config.Users = {};

for (var i in __embed_config.Details)
{
	var r = __embed_config.Details[i];
	if (__embed_config.Depts[r.DeptID] == undefined) __embed_config.Depts[r.DeptID] = { Name: "", Users: [] };
	var d = __embed_config.Depts[r.DeptID];
	var u = { ID: r.ID, Name: r.Name, Nickname: r.Nickname, IsOnline: r.IsOnline };
	d.Name = r.DeptName;
	d.ID = r.DeptID;
	d.Users.push(u);
	__embed_config.Users[u.Name.toUpperCase()] = u;
}

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

function StartChat(peer)
{
	var url = __embed_config.Format(
		__embed_config.Config.ServiceUrl + "/CustomService.aspx?Peer={0}&Source={1}&random={2}",
		peer,
		__embed_config.EncodeReferrer(document.referrer == "" ? document.location.toString() : document.referrer),
		(new Date()).getTime()
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