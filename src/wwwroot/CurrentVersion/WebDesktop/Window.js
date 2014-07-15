
var Window = null;

(function(){

var BorderCursorCss = [
	"nw-resize", "n-resize", "ne-resize",
	"w-resize", "default", "e-resize",
	"sw-resize", "s-resize", "se-resize"
];

function MoveCallback(action, data, e)
{
	if(action == "moving")
	{
		var diffX = e.GetEvent().clientX - data.PreCurLeft;
		var diffY = e.GetEvent().clientY - data.PreCurTop;
		if(data.Action == "move")
		{
			data.Window.Move(data.PreLeft + diffX, data.PreTop + diffY);
		}
		else 
		{
			var newLeft = data.PreLeft,newTop=data.PreTop,newWidth=data.PreWidth,newHeight=data.PreHeight;
			if(data.Action == 0)
			{
				newLeft = data.PreLeft + diffX;
				newTop = data.PreTop + diffY;
				newWidth = data.PreWidth - diffX;
				newHeight = data.PreHeight - diffY;
				
				if(newWidth < data.MinWidth) 
				{
					newLeft -= data.MinWidth - newWidth;
					newWidth = data.MinWidth;
				}
				
				if(newHeight < data.MinHeight) 
				{
					newTop -= data.MinHeight - newHeight;
					newHeight = data.MinHeight;
				}
			}
			else if(data.Action == 1)
			{
				newTop = data.PreTop + diffY;
				newHeight = data.PreHeight - diffY;
				
				if(newHeight < data.MinHeight) 
				{
					newTop -= data.MinHeight - newHeight;
					newHeight = data.MinHeight;
				}
			}
			else if(data.Action == 2)
			{
				newTop = data.PreTop + diffY;
				newWidth = data.PreWidth + diffX;
				newHeight = data.PreHeight - diffY;
				
				if(newWidth < data.MinWidth) 
				{
					newWidth = data.MinWidth;
				}
				
				if(newHeight < data.MinHeight) 
				{
					newTop -= data.MinHeight - newHeight;
					newHeight = data.MinHeight;
				}
			}
			else if(data.Action == 3)
			{
				newLeft = data.PreLeft + diffX;
				newWidth = data.PreWidth - diffX;
				
				if(newWidth < data.MinWidth) 
				{
					newLeft -= data.MinWidth - newWidth;
					newWidth = data.MinWidth;
				}
			}
			else if(data.Action == 5)
			{
				newWidth = data.PreWidth + diffX;
				
				if(newWidth < data.MinWidth) 
				{
					newWidth = data.MinWidth;
				}
			}
			else if(data.Action == 6)
			{
				newLeft = data.PreLeft + diffX;
				newWidth = data.PreWidth - diffX;
				newHeight = data.PreHeight + diffY;
				
				if(newWidth < data.MinWidth) 
				{
					newLeft -= data.MinWidth - newWidth;
					newWidth = data.MinWidth;
				}
				
				if(newHeight < data.MinHeight) 
				{
					newHeight = data.MinHeight;
				}
			}
			else if(data.Action == 7)
			{
				newHeight = data.PreHeight + diffY;
				
				if(newHeight < data.MinHeight) 
				{
					newHeight = data.MinHeight;
				}
			}
			else if(data.Action == 8)
			{
				newWidth = data.PreWidth + diffX;
				newHeight = data.PreHeight + diffY;
				
				if(newWidth < data.MinWidth) 
				{
					newWidth = data.MinWidth;
				}
				
				if(newHeight < data.MinHeight) 
				{
					newHeight = data.MinHeight;
				}
			}
			
			data.Window.Move(newLeft, newTop);
			data.Window.Resize(newWidth, newHeight);
		}
	}
	else if(action == "end")
	{
	}
}

function WindowBkImage(config)
{
	var m_Config=Core.Utility.Clone(config);
	
	var m_Dom=document.createElement("DIV");
	
	m_Dom.innerHTML=
	"<table cellspacing='0' cellpadding='0'>"+
		"<tbody>"+
			"<tr><td></td><td></td><td></td></tr>"+
			"<tr><td></td><td></td><td></td></tr>"+
			"<tr><td></td><td></td><td></td></tr>"+
		"</tbody>"+
	"</table>";
	
	m_Dom.className = m_Config.Css;
	
	m_Dom.style.left="0px";
	m_Dom.style.top="0px";
	
	function resize(width,height)
	{
		var xs = [m_Config.Horiz[0], width - (m_Config.Horiz[0] + m_Config.Horiz[2]), m_Config.Horiz[2]];
		var ys = [m_Config.Vertical[0], height - (m_Config.Vertical[0] + m_Config.Vertical[2]), m_Config.Vertical[2]];

		for (var x = 0; x < 3; x++)
		{
			for (var y = 0; y < 3; y++)
			{
				var cell = m_Dom.firstChild.rows[y].cells[x];
				cell.style.width = xs[x] + 'px';
				cell.style.height = ys[y] + 'px';
			}
		}
	}
	
	this.Resize=resize;

	this.GetDom = function() { return m_Dom; };
	
	for (var x = 0; x < 3; x++)
	{
		for (var y = 0; y < 3; y++)
		{
			var cell = m_Dom.firstChild.rows[y].cells[x];
			cell.className = m_Config.Css + "_block_" + (y * 3 + x);
			cell.style.padding = '0px';
			cell.style.margin = '0px';
		}
	}
	
	resize(m_Config.Horiz[0] + m_Config.Horiz[1] + m_Config.Horiz[2],m_Config.Vertical[0] + m_Config.Vertical[1] + m_Config.Vertical[2]);
}

/*
config = {
	Left:0
	Top:0
	Width:100
	Height:100
	Css:"window"
	HasMinButton:true
	HasMaxButton:true
	Resizable:true
	Title:{
		Height:18
		InnerHTML:""
	}
	BorderWidth:6,
	OnClose:null
}
*/

Window = function(config)
{
	var This = this;
	
	if(config == undefined) config = {};
	
	var _config = {};
	_config.Left = Core.Utility.IsNull(config.Left, 100);
	_config.Top = Core.Utility.IsNull(config.Top, 100);
	_config.Width = Core.Utility.IsNull(config.Width, 400);
	_config.Height = Core.Utility.IsNull(config.Height, 300);
	_config.MinWidth = Core.Utility.IsNull(config.MinWidth, Math.min(_config.Width, 400));
	_config.MinHeight = Core.Utility.IsNull(config.MinHeight, Math.min(_config.Height, 300));
	_config.HasMinButton = Core.Utility.IsNull(config.HasMinButton, true);
	_config.HasMaxButton = Core.Utility.IsNull(config.HasMaxButton, true);
	_config.Resizable = Core.Utility.IsNull(config.Resizable, true);
	_config.ShowInTaskbar = Core.Utility.IsNull(config.ShowInTaskbar, _config.HasMinButton);
	_config.Css = Core.Utility.IsNull(config.Css, "window");
	_config.BorderWidth = Core.Utility.IsNull(config.BorderWidth, 6);
	_config.Tag = Core.Utility.IsNull(config.Tag, null);
	_config.AnchorStyle = Core.Utility.IsNull(config.AnchorStyle, Core.WindowAnchorStyle.Left | Core.WindowAnchorStyle.Top);
	
	if(config.Title == undefined)
	{
		_config.Title = {
			Height:18,
			InnerHTML:""
		};
	}
	else
	{
		_config.Title = {};
		_config.Title.Height = Core.Utility.IsNull(config.Title.Height, 18);
		_config.Title.InnerHTML = Core.Utility.IsNull(config.Title.InnerHTML, "");
	}
	
	_config.OnClose = Core.Utility.IsNull(config.OnClose, null);
	
	Core.IWindow.apply(this);

	This.GetLeft = function() { return _config.Left; }
	This.GetTop = function() { return _config.Top; }
	This.GetWidth = function() { return _config.Width; }
	This.GetHeight = function() { return _config.Height; }
	This.GetClientWidth = function() { return _config.Width - _config.BorderWidth * 2; };
	This.GetClientHeight = function() { return _config.Height - _config.BorderWidth * 2 - _config.Title.Height; };
	This.GetMinClientWidth = function() { return _config.MinWidth - _config.BorderWidth * 2; };
	This.GetMinClientHeight = function() { return _config.MinHeight - _config.BorderWidth * 2 - _config.Title.Height; };
	
	This.GetDom = function()
	{
		return _dom;
	}
	
	This.Resize = function(width, height)
	{
		bk.Resize(width, height);
		
		_config.Width = width;
		_config.Height = height;
		
		var ws = [_config.BorderWidth, _config.Width - _config.BorderWidth * 2, _config.BorderWidth];
		var hs = [_config.BorderWidth, _config.Height - _config.BorderWidth * 2, _config.BorderWidth];
		
		if(_config.Title.Height>0)
		{
			m_TitleDiv.style.width = ws[1] + "px";
			m_TitleDiv.style.height = _config.Title.Height + "px";
		}
		
		m_WaitingDiv.style.width = ws[1] + "px";
		m_WaitingDiv.style.height = "24px";
		m_WaitingDiv.style.left = ws[0] + "px";
		m_WaitingDiv.style.top = (hs[0] + _config.Title.Height) + "px";
		
		m_Background.style.width = width + "px";
		m_Background.style.height = height + "px";
		
		m_Browser.width = ws[1];
		m_Browser.height = hs[1] - _config.Title.Height;

		m_Client.style.width = ws[1] + "px";
		m_Client.style.height = (hs[1] - _config.Title.Height) + "px";

		if (_config.BorderWidth > 0)
		{
			for (var x = 0; x < 3; x++)
			{
				for (var y = 0; y < 3; y++)
				{
					var i = y * 3 + x;
					m_BroderDiv.childNodes[i].style.width = ws[x] + "px";
					m_BroderDiv.childNodes[i].style.height = hs[y] + "px";
				}
			}
		}
		
		_dom.style.width = width + "px";
		_dom.style.height = height + "px";
		
		m_DisableDiv.style.width = width + "px";
		m_DisableDiv.style.height = height + "px";
		
		This.OnResize.Call(This);
	}
	
	This.GetAnchorStyle = function()
	{
		return _config.AnchorStyle;
	}
	
	This.Move = function(left, top)
	{
		_config.Left = left;
		_config.Top = top;
		_dom.style.left = left + "px";
		_dom.style.top = top + "px";
	}
	
	This.MoveEx = function(position, x, y, relativeParent)
	{
		var left,top;
		
		var width=This.GetWidth();
		var height=This.GetHeight();
		
		if(x==undefined || x==null) x=0;
		if(y==undefined || y==null) y=0;
		
		if(_parent==undefined || _parent==null) relativeParent=false;
		
		var rect=null;
		if(relativeParent && _parent!=null)
			rect={Width:_parent.GetWidth(),Height:_parent.GetHeight(),Top:_parent.GetTop(),Left:_parent.GetLeft()};
		else
			rect={Width:Desktop.GetWidth(),Height:Desktop.GetHeight(),Top:0,Left:0};
			
		position=position.toUpperCase();
		
		var align;
		var verticalAlign;
		
		if(position=='CENTER')
		{
			align="MIDDLE";
			verticalAlign="MIDDLE";
		}
		else
		{
			var ps=position.split("|");
			align=ps.length>0?ps[0]:"NULL";
			verticalAlign=ps.length>1?ps[1]:"NULL";
		}
		
		switch(align)
		{
		case "LEFT":
			left=0;
			break;
		case "RIGHT":
			left=rect.Width-width;
			break;
		case "MIDDLE":
			left=Math.round((rect.Width-width)/2);
			break;
		default:
			left=0;
		}
		left+=rect.Left+x;
		
		switch(verticalAlign)
		{
		case "TOP":
			top=0;
			break;
		case "BOTTOM":
			top=rect.Height-height;
			break;
		case "MIDDLE":
			top=Math.round((rect.Height-height)/2);
			break;
		default:
			top=0;
		}
		top+=rect.Top+y;
		
		if(left<0) left=0;
		if(top<0) top=0;
		This.Move(left,top);
	}
	
	This.Show = function(isTop)
	{
		if(_dom.style.display != "block")
		{
			if(_parent != null) _parent.Show();
			_dom.style.display = "block";
			if(_child != null) _child.Show();
		}

		if (_waitingCount <= 0)
		{
			_waitingCount = 0;
			m_WaitingDiv.style.display = "block";
			m_WaitingDiv.style.display = "none";
		}
		
		This.SetTop();
	}
	
	This.ShowWindow = function(cmd)
	{
		This.Show(false);
	}
	
	This.Hide = function()
	{
		if(_dom.style.display != "none")
		{
			if(_child != null) _child.Hide();
			_dom.style.display = "none";
			if(_parent != null) _parent.Hide();
			This.OnHidden.Call(This);
		}
	}
	
	This.Minimum = function()
	{
		This.Hide();
	}
	
	This.Close = function()
	{
		This.OnClosed.Call(This);
		Desktop.RemoveWindow(This);
	}
	
	var _parent = null;
	var _child = null;
	
	This.GetParent = function()
	{
		return _parent;
	}
	
	This.SetParent = function(p)
	{
		if(_parent != p)
		{
			var preParent = _parent;
			_parent = p;
			if(preParent != null) preParent.AddChild(null);
			if(_parent != null) _parent.AddChild(This);
		}
	}
	
	This.GetFirstParent = function()
	{
		return _parent == null ? This : _parent.GetFirstParent();
	}
	
	This.GetChild = function()
	{
		return _child;
	}
	
	This.AddChild = function(c)
	{
		if(_child != c)
		{
			var preChild = _child;
			_child = c;
			if(preChild != null) preChild.SetParent(null);
			if(_child != null) _child.SetParent(This);
		}
	}
	
	This.GetLastChild = function()
	{
		return _child == null ? This : _child.GetLastChild();
	}
	
	This.SetTop = function()
	{
		if(!This.IsTop())
		{
			var w = This.GetFirstParent();
			while(w != null) 
			{
				Desktop.SetTop(w);
				w = w.GetChild();
			}
		}
	}
	
	This.Disable = function()
	{
		m_DisableDiv.style.display = "block";
	}
	
	This.Enable = function()
	{
		m_DisableDiv.style.display = "none";
	}
	
	This.ShowDialog = function(parent, pos, left, top, relativeParent, callback)
	{
		var p = parent.GetLastChild();
		p.AddChild(This);
		This.MoveEx(pos, left, top, relativeParent);
		p.Disable();
		This.OnClosed.Attach(
			function()
			{
				p.Enable();
				This.SetParent(null);
				if(callback != null) callback(This);
			}
		);
		This.Show();
		parent.Show();
	}
	
	var _htmlWindow = null;
	
	This.GetHtmlWindow = function()
	{
		return _htmlWindow;
	}
	
	var isLoad = false;
	
	This.IsLoad = function()
	{
		return isLoad;
	}
	
	This.Load = function(src,onload_callback)
	{
		This.Waiting("\u6B63\u5728\u8F7D\u5165\u7A97\u53E3...");
		Core.Utility.AttachEvent(
			m_Browser,"load",
			function()
			{
				This.Completed();
				
				isLoad = true;
				try
				{
					_htmlWindow = m_Browser.contentWindow;
					Core.Utility.AttachEvent(
						_htmlWindow.document, "mousedown",
						function()
						{
							try
							{
								if(_htmlWindow.document.activeElement == null)
								{
									_htmlWindow.document.body.focus();
								}
								else
								{
									_htmlWindow.document.activeElement.focus();
								}
							}
							catch(ex)
							{
							}
						}
					);
					m_Browser.contentWindow.SetClientMode(false,This);
				}
				catch(ex)
				{
					return;
				}
				if(onload_callback!=undefined) onload_callback();
				This.OnLoad.Call(This);
			}
		);
		
		m_Browser.src = src;
	}
	
	This.Navigate = function(src)
	{
		This.Waiting("\u6B63\u5728\u8F7D\u5165\u7A97\u53E3...");
		Core.Utility.AttachEvent(
			m_Browser,"load",
			function()
			{
				This.Completed();
				isLoad = true;
			}
		);
		
		m_Browser.src = src;
	}
	
	var _waitingCount = 0;
	
	This.Waiting = function(text)
	{
		_waitingCount++;
		
		m_WaitingDiv.innerHTML = String.format("<div>{0}</div>", (text != "" ? text : "\u6B63\u5728\u52A0\u8F7D..."));
		
		if(_waitingCount > 0)
		{
			m_WaitingDiv.style.display="block";
		}
	}

	This.Completed = function()
	{
		_waitingCount--;

		if (_waitingCount <= 0)
		{
			_waitingCount = 0;
			m_WaitingDiv.style.display = "none";
		}
	}

	This.CompleteAll = function()
	{
		_waitingCount = 0;
		m_WaitingDiv.style.display = "none";
	}
	
	This.Reset = function()
	{
		This.CompleteAll();
	}
	
	This.Notify = function()
	{
		if(m_Task!=null)
		{
			m_Task.Shine(true);
		}
	}
	
	var _tag = null;
	
	This.GetTag = function()
	{
		return _tag;
	}
	
	This.SetTag = function(tag)
	{
		_tag = tag;
	}
	
	This.GetTitle = function()
	{
		return m_TitleText.innerHTML;
	}
	
	This.SetTitle = function(title)
	{
		m_TitleText.innerHTML = title;
		if(m_Task != null) m_Task.SetText(title);
	}
	
	This.IsTop = function()
	{
		return Desktop.IsTop(This.GetLastChild());
	}
	
	This.IsVisible = function()
	{
		return _dom.style.display != "none";
	}
	
	This.BringToTop = function()
	{
		Desktop.SetTop(This);
	}
	
	This.GetFrame = function()
	{
		return m_Browser;
	}
	
	This.GetClient = function()
	{
		return m_Client;
	}
	
	This.SetMode = function(mode)
	{
		if(mode == 0)
		{
			m_Client.style.display = "none";
			m_Browser.style.display = "block";
		}
		else
		{
			m_Client.style.display = "block";
			m_Browser.style.display = "none";
		}
	}
	
	This.GetClientCoord = function(x, y)
	{
		var coord = Core.Utility.GetClientCoord(m_Browser);
		coord.X += x;
		coord.Y += y;
		var desktopCoord = Core.Utility.GetClientCoord(Desktop.GetDom());
		coord.X -= desktopCoord.X;
		coord.Y -= desktopCoord.Y;
		return coord;
	}
	
	var _dom = document.createElement("DIV");
	
	_dom.tabIndex=-1;
	_dom.style.outline='none';
	_dom.style.padding = "0px";
	_dom.style.margin = "0px";
	_dom.style.borderWidth = "0px";
	_dom.style.position = "absolute";
	_dom.className = _config.Css;
	
	_dom.innerHTML=
	"<div>"+
	"</div>"+
	"<div class='border' style='position:absolute; z-Index:2;'>"+
		"<div style='float:left;font-size:1px;'></div>"+
		"<div style='float:left;font-size:1px;'></div>"+
		"<div style='float:left;font-size:1px;'></div>"+
		"<div style='float:left;font-size:1px;'></div>"+
		"<div style='float:left;'>"+
			"<div class='title'>"+
				"<div class='icon' style='float:left;'></div>"+
				"<div class='text' style='float:left;'></div>"+
				"<div class='closeButton' style='float:right;'></div>"+
				"<div class='maxButton' style='float:right;'></div>"+
				"<div class='restoreButton' style='float:right;'></div>"+
				"<div class='minButton' style='float:right;'></div>"+
			"</div>"+
			"<iframe frameBorder='0' allowTransparency='true' style='background-color:transparent;'></iframe>"+
			"<div class='client' style='diaplay:none;'></div>"+
		"</div>"+
		"<div style='float:left;font-size:1px;'></div>"+
		"<div style='float:left;font-size:1px;'></div>"+
		"<div style='float:left;font-size:1px;'></div>"+
		"<div style='float:left;font-size:1px;'></div>"+
	"</div>"+
	"<div style='display:none; z-Index:4;' class='disabled'></div>"+
	"<div style='display:none; z-Index:5;' class='waiting'></div>";

	var bk = new WindowBkImage(
		{
			Css: (_config.BorderWidth == 0 && _config.Title.Height == 0) ? "background_noborder" : "background",
			Horiz: [6, 100, 6],
			Vertical: [24, 100, 6]
		}
	);
	_dom.replaceChild(bk.GetDom(), _dom.firstChild);
	
	_dom.onselectstart=function(evt)
	{
		var e = new Core.Event(evt, window);
		
		if(e.GetTarget().tagName!=undefined && e.GetTarget().tagName.toUpperCase()!='DIV')
		{
			return true;
		}
		else
		{
			return false;
		}
	}
	
	var m_Background = _dom.childNodes[0];
	var m_BroderDiv = _dom.childNodes[1];

	for (var x = 0; x < 3; x++)
	{
		for (var y = 0; y < 3; y++)
		{
			var i = y * 3 + x;
			if (i != 4)
			{
				m_BroderDiv.childNodes[i].style.display = _config.BorderWidth > 0 ? "block" : "none";
			}
		}
	}
	
	var m_DisableDiv = _dom.childNodes[2];
	var m_WaitingDiv = _dom.childNodes[3];
	
	var m_TitleDiv = m_BroderDiv.childNodes[4].firstChild;
	m_TitleDiv.style.display = _config.Title.Height > 0 ? "block" : "none";
	
	var m_Browser = m_BroderDiv.childNodes[4].childNodes[1];
	
	var m_Client = m_BroderDiv.childNodes[4].childNodes[2];
	m_Client.style.display = "none"
	
	var m_TitleText = m_TitleDiv.childNodes[1];
	var m_CloseButton = m_TitleDiv.childNodes[2];
	var m_MaxButton = m_TitleDiv.childNodes[3];
	var m_RestoreButton = m_TitleDiv.childNodes[4];
	var m_MinButton = m_TitleDiv.childNodes[5];
			
	m_MaxButton.style.display = "none";
	m_RestoreButton.style.display = "none";
	
	m_TitleText.innerHTML = _config.Title.InnerHTML;
	
	m_MinButton.style.display = _config.HasMinButton?"block":"none";
	
	
	_dom.onmousedown = function()
	{
		This.SetTop();
	}
	
	m_TitleDiv.onmousedown = function()
	{
		var e = new Core.Event(arguments[0]);
						
		if(e.GetTarget() == m_TitleDiv || 
			e.GetTarget() == m_TitleDiv.childNodes[0] || 
			e.GetTarget() == m_TitleDiv.childNodes[1])
		{
			var data = {
				Window:This,
				PreLeft:_config.Left,
				PreTop:_config.Top,
				PreCurLeft:e.GetEvent().clientX,
				PreCurTop:e.GetEvent().clientY,
				Action:"move"
			};
			
			Desktop.EnterMove(MoveCallback, data, "move");
		}
	}

	if (_config.BorderWidth > 0)
	{
		for (var i = 0; i < 9; i++)
		{
			if (_config.Resizable)
			{
				m_BroderDiv.childNodes[i].style.cursor = BorderCursorCss[i];
				if (i != 4)
				{
					(function(block, index, cursor, win, c)
					{

						block.onmousedown = function()
						{
							var e = new Core.Event(arguments[0]);

							if (e.GetTarget() != block) return;

							var data = {
								Window: win,
								PreLeft: win.GetLeft(),
								PreTop: win.GetTop(),
								PreWidth: win.GetWidth(),
								PreHeight: win.GetHeight(),
								PreCurLeft: e.GetEvent().clientX,
								PreCurTop: e.GetEvent().clientY,
								MinWidth: c.MinWidth,
								MinHeight: c.MinHeight,
								Action: index
							};

							Desktop.EnterMove(MoveCallback, data, cursor);
						}
					})(m_BroderDiv.childNodes[i], i, BorderCursorCss[i], This, _config);
				}
			}
		}
	}
	
	m_CloseButton.onclick = function()
	{
		if(_config.OnClose == null) This.Close();
		else _config.OnClose(This);
	}
	
	m_MinButton.onclick = function()
	{
		This.Hide();
	}
	
	This.Hide();
	
	var m_Task = null;
	
	if(_config.ShowInTaskbar)
	{
		m_Task = Taskbar.AddTask(This);
		This.OnClosed.Attach(
			function()
			{
				Taskbar.RemoveTask(This);
			}
		);
	}
	
	Desktop.AddWindow(This);
	This.Resize(_config.Width, _config.Height);
	This.Move(_config.Left,_config.Top);
}

})();
