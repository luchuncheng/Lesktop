
var MenuManager = new (function()
{
	var currentMenu = null;
	var close_callback = function()
	{
		currentMenu.OnCommand.Call("");
	}

	this.Popup = function(menu, clientX, clientY)
	{
		Desktop.Popup(menu.GetDom(), clientX, clientY, close_callback);
		menu.Show(clientX, clientY);
		currentMenu = menu;
	}

	this.Hide = function(menu)
	{
		if (menu != undefined)
		{
			menu.Hide();
			Desktop.Unpopup(menu.GetDom());
			if (currentMenu == menu) currentMenu = null;
		}
		else
		{
			if (currentMenu != null) currentMenu.Hide();
			currentMenu = null;
		}
	}

})();

/*
menuConfig={
	Css:"",
	Items=[
		{Text:文本,ID:Menu Id,Css:样式}
		{
			Text:文本,
			ID:Menu Id,
			SubMenu:menuConfig
		}
	]
}
*/
function Menu(menuConfig)
{
	var This = this;

	//框架
	var _dom = document.createElement("DIV");
	_dom.style.overflow = "hidden";
	_dom.style.position = "absolute";
	_dom.style.margin = '0px';
	_dom.style.padding = '0px';
	_dom.style.borderWidth = '1px';
	This.GetDom = function() { return _dom; }

	This.Popup = function(clientX, clientY)
	{
		MenuManager.Popup(This, clientX, clientY);
	}

	This.Show = function(clientX, clientY)
	{
		This.GetDom().style.display = 'block';
		mainMenu.Show(clientX, clientY);
	}

	This.Hide = function()
	{
		This.GetDom().style.display = 'none';
		mainMenu.Hide();
	}

	This.DoCommand = function(menuId, data)
	{
		MenuManager.Hide(This);
		if (menuId != null && menuId != "") This.OnCommand.Call(menuId, data);
	}

	This.AppendChild = function(child)
	{
		This.GetDom().appendChild(child);
	}

	This.OnCommand = new Core.Delegate();

	var dom = This.GetDom();
	dom.style.position = "absolute";
	dom.style.left = "0px";
	dom.style.top = "0px";
	dom.style.width = "10px";
	dom.style.height = "10px";
	dom.style.zIndex = "10000000";
	dom.style.overflow = "visible";

	var mainMenu = new SingleMenu(menuConfig, null, This);
}

function SingleMenu(menuConfig, parentItem, mainMenu)
{

	var This = this;

	var css = (menuConfig.Css == undefined ? "menu" : menuConfig.Css);

	var obj = document.createElement("DIV");
	obj.className = css;
	obj.style.position = "absolute";
	obj.style.display = 'none';

	mainMenu.AppendChild(obj);

	var items = []
	for (var i in menuConfig.Items)
	{
		var menuItem = new MenuItem(menuConfig.Items[i], This, mainMenu)
		items.push(menuItem);
		obj.appendChild(menuItem.GetDom());
	}

	This.GetDom = function() { return obj; }

	This.Show = function(clientX, clientY)
	{
		obj.style.display = "block";
		var x = clientX, y = clientY;
		if (x + obj.offsetWidth > Desktop.GetWidth())
		{
			x -= obj.offsetWidth;
			if (parentItem != null) x -= parentItem.GetDom().offsetWidth;
		}
		if (y + obj.offsetHeight > Desktop.GetHeight())
		{
			y -= obj.offsetHeight;
			if (parentItem != null) y += parentItem.GetDom().offsetHeight;
		}
		obj.style.left = x + "px";
		obj.style.top = y + "px";
	}

	This.Hide = function()
	{
		for (var i in items) items[i].HideSubMenu();
		obj.style.display = "none";
	}

	This.HideSubMenus = function()
	{
		for (var i in items) items[i].HideSubMenu();
	}
}

function MenuItem(itemConfig, parent, mainMenu)
{
	var This = this;

	var css, text, id;
	var subMenu = null;

	id = (itemConfig.ID == undefined ? "" : itemConfig.ID);
	if (id == null || id == "")
	{
		text = "";
		css = (itemConfig.Css == undefined ? "menuSplit" : itemConfig.Css);
	}
	else
	{
		text = (itemConfig.Text == undefined ? "Menu" : itemConfig.Text);
		css = (itemConfig.Css == undefined ? "menuItem" : itemConfig.Css);
	}
	subMenu = (itemConfig.SubMenu == undefined ? null : new SingleMenu(itemConfig.SubMenu, This, mainMenu))

	var obj = document.createElement("DIV");
	obj.innerHTML = Core.Utility.ReplaceHtml(text);
	obj.className = css;
	Core.Utility.AttachButtonEvent(obj, css, css+"_hover", css+"_hover");

	This.GetDom = function() { return obj; }

	This.HideSubMenu = function()
	{
		if (subMenu != null) subMenu.Hide();
	}

	This.ShowSubMenu = function()
	{
		var pobj = parent.GetDom();
		if (subMenu != null) subMenu.Show(pobj.offsetLeft + obj.offsetLeft + obj.offsetWidth, pobj.offsetTop + obj.offsetTop);
	}
	if (id != null && id != "")
	{
		obj.onmousemove = function()
		{
			parent.HideSubMenus();
			This.ShowSubMenu();
		}

		obj.onmousedown = function(evt)
		{
			if (evt == undefined) evt = event;
			if (evt.cancelBubble != undefined) evt.cancelBubble = true;
			if (evt.stopPropagation != undefined) evt.stopPropagation();
			return true;
		}

		obj.onclick = function()
		{
			mainMenu.DoCommand(id, itemConfig.Data);
			return true;
		}
	}
}