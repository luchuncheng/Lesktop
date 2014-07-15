var ClientMode = false;

if (__embed_config.NewWindow == undefined) __embed_config.NewWindow = true;

if(window.Core == undefined) window.Core = {};

Core.Config = {
	Version: "{VERSION}",
	ServiceUrl: "{SERVICEURL}"
};

document.writeln('<link href="{URL}/Themes/Default/WebDesktop/Desktop.css" rel="stylesheet" type="text/css" />'.replace(/\{URL\}/ig, Core.Config.ServiceUrl));
document.writeln('<link href="{URL}/Themes/Default/Service.css" rel="stylesheet" type="text/css" />'.replace(/\{URL\}/ig, Core.Config.ServiceUrl));
document.writeln('<script src="{URL}/Core/Common.js" type="text/javascript"></script>'.replace(/\{URL\}/ig, Core.Config.ServiceUrl));
document.writeln('<script src="{URL}/Core/Extent.js" type="text/javascript"></script>'.replace(/\{URL\}/ig, Core.Config.ServiceUrl));
document.writeln('<script src="{URL}/WebDesktop/Desktop.js" type="text/javascript"></script>'.replace(/\{URL\}/ig, Core.Config.ServiceUrl));
document.writeln('<script src="{URL}/WebDesktop/Window.js" type="text/javascript"></script>'.replace(/\{URL\}/ig, Core.Config.ServiceUrl));

__embed_config.Depts = {};
__embed_config.Users = {};

for(var i in __embed_config.Details)
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

__embed_config.UserPanel = (function()
{
	var obj = {};

	var up_dom = document.createElement("DIV");
	up_dom.className = "user-panel";

	function Category(info)
	{
		var This = this;

		var c_dom = document.createElement("DIV");
		c_dom.className = "category";
		c_dom.innerHTML = "<div class='category_header'><div class='button_expand'></div><div class='title'></div></div><div class='items'></div>";

		c_dom.childNodes[0].childNodes[1].innerHTML = info.Name;

		up_dom.appendChild(c_dom);

		var _expand = true;

		this.Expand = function()
		{
			if (!_expand)
			{
				c_dom.childNodes[0].childNodes[0].className = 'button_expand';
				c_dom.childNodes[1].style.display = "block";
				_expand = true;
			}
		}

		this.Collapse = function()
		{
			if (_expand)
			{
				c_dom.childNodes[0].childNodes[0].className = 'button_collapse';
				c_dom.childNodes[1].style.display = "none";
				_expand = false;
			}
		}

		c_dom.childNodes[0].onclick = function()
		{
			if (_expand) This.Collapse(); else This.Expand();
		}

		this.AddItem = function(item)
		{
			c_dom.childNodes[1].appendChild(item.GetDom());
		}
	}

	function Item(info)
	{
		var item_dom = document.createElement("DIV");
		item_dom.className = 'user-item';
		item_dom.innerHTML = "<img class='header-image' width=40 height=40 /><div class='user-info'><div class='nickname'></div></div>";

		Core.Utility.AttachButtonEvent(item_dom, "user-item", "user-item_hover", "user-item_hover");

		item_dom.childNodes[0].src = String.format(Core.Config.ServiceUrl + "/HeadImg.ashx?user={0}&size={1}&gred={2}", info.ID, 40, !info.IsOnline);
		item_dom.childNodes[1].childNodes[0].innerHTML = info.Nickname;

		item_dom.ondblclick = function()
		{
			StartChat(info.Name);
		}

		this.GetDom = function()
		{
			return item_dom;
		}
	}

	var _catetories = [];
	var _catetories_index = {};

	obj.Create = function(parent)
	{
		parent.appendChild(up_dom);

		var data = __embed_config.Data;

		for (var i in __embed_config.Depts)
		{
			var dept = __embed_config.Depts[i];
			var c = new Category(dept);
			_catetories.push(c);
			_catetories_index[dept.ID] = c;
			
			for(var j in dept.Users)
			{
				c.AddItem(new Item(dept.Users[j]));
			}
		}
	}

	return obj;
})();

__embed_config.ChatForm = null;

function StartChat(peer)
{
	if(__embed_config.ChatForm == null)
	{
		Desktop.Create();

		__embed_config.ChatForm = new Window(
			{
				Left: 0, Top: 0,
				Width: 700, Height: 500,
				MinWidth: 700, MinHeight: 500,
				Title: {
					InnerHTML: ""
				},
				HasMaxButton: false, HasMinButton: false,
				Resizable: false,
				AnchorStyle: Core.WindowAnchorStyle.Left | Core.WindowAnchorStyle.Bottom,
				OnClose: function(f)
				{
					f.Hide();
					__embed_config.ChatForm.Navigate("about:blank");
				}
			}
		);
		__embed_config.ChatForm.MoveEx("MIDDLE|BOTTOM", 20, -20, false);
	}
	
	var url = String.format(
		Core.Config.ServiceUrl + "/CustomService.aspx?Peer={0}&Source={1}&random={2}",
		peer,
		__embed_config.EncodeReferrer(document.referrer == "" ? document.location.toString() : document.referrer),
		(new Date()).getTime()
	);
	
	if(__embed_config.NewWindow)
	{
		var __lesktop_desktop = open(
			url, "__lesktop_desktop",
			String.format("left={0}, top={1}, height=500, width=700, toolbar=no, menubar=no, scrollbars=no, resizable=yes,location=no, status=no", (window.screen.availWidth - 10 - 700) / 2, (window.screen.availHeight - 30 - 500) / 2)
		);
		__lesktop_desktop.blur();
		__lesktop_desktop.focus();
		window.blur();
	}
	else
	{
		__embed_config.ChatForm.SetTitle(String.format("\u548C\u0022{0}\u0022\u4EA4\u8C08\u4E2D", __embed_config.Users[peer.toUpperCase()].Nickname));
		__embed_config.ChatForm.Navigate(url, null);
		__embed_config.ChatForm.Show();
	}
}

__embed_config.window_onload = function()
{
	if(__embed_config.ShowWindow)
	{
		Desktop.Create();
		var form = new Window(
			{
				Left: 0, Top: 0,
				Width: Math.max(__embed_config.Width != undefined ? __embed_config.Width : 120, 120),
				Height: Math.max(__embed_config.Height != undefined ? __embed_config.Height : 250, 250),
				Title: {
					InnerHTML: __embed_config.Title == undefined ? "\u7F51\u4E0A\u5BA2\u670D\u4E2D\u5FC3" : __embed_config.Title
				},
				MinWidth: 220,
				HasMaxButton: false, HasMinButton: false,
				Resizable: false,
				AnchorStyle: Core.WindowAnchorStyle.Right | Core.WindowAnchorStyle.Bottom
			}
		);

		form.SetMode(1);
		var client = form.GetClient();

		__embed_config.UserPanel.Create(client);

		form.MoveEx("RIGHT|BOTTOM", -10, -20, false);
		form.Show();
	}
	
	if(window.__embed_cs_init != undefined) window.__embed_cs_init();
}

if (window.attachEvent)
{
	window.attachEvent("onload", __embed_config.window_onload);
}
else if (window.addEventListener)
{
	window.addEventListener("load", __embed_config.window_onload, false);
}