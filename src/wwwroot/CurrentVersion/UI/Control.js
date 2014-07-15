if (window.Core == undefined) window.Core = {};
if (window.Core.UI == undefined) window.Core.UI = {};

(function(){

Core.UI.DockStyle = {
	None: 0,
	Left: 1,
	Top: 2,
	Right: 3,
	Bottom: 4,
	Fill: 5
};

var control_id_ = 1000000;

function GenerateControlId()
{
	control_id_++;
	return String.format("CONTROL_{0}", control_id_);
}

var resize_data_ = {
	SplitterPlaceHolder: document.createElement("DIV"),
	Control: null,
	Splitter: null,
	PlaceHolderPreLeft: null,
	PlaceHolderPreTop: null,
	PreviousClientX: null,
	PreviousClientY: null,
	PreviousBounds: null
};

resize_data_.SplitterPlaceHolder.className = "ct_splitter_placeholder";

Core.UI.Control = function(parent, config)
{
	var this_ = this;
	
	var dock_style_ = (config.DockStyle == undefined || typeof(config.DockStyle) != "number" || config.DockStyle < 0 || config.DockStyle > 5) ? Core.UI.DockStyle.None : config.DockStyle;
	
	var margin_ = null;
	
	if(typeof(config.Margin) == "number")
	{
		margin_ = {Left: config.Margin, Top: config.Margin, Right: config.Margin, Bottom: config.Margin};
	}
	else if(typeof(config.Margin) == "object" && config.Margin.push != undefined)
	{
		if(config.Margin.length == 1)  margin_ = {Top: config.Margin[0], Right: config.Margin[0], Bottom: config.Margin[0], Left: config.Margin[0]};
		else if(config.Margin.length == 2)  margin_ = {Top: config.Margin[0], Right: config.Margin[1], Bottom: config.Margin[0], Left: config.Margin[1]};
		else if(config.Margin.length == 4)  margin_ = {Top: config.Margin[0], Right: config.Margin[1], Bottom: config.Margin[2], Left: config.Margin[3]};
	}
	if(margin_ == null) margin_ = {Left: 0, Top: 0, Right: 0, Bottom: 0};
	
	var padding_ = null;
	
	if(typeof(config.Padding) == "number")
	{
		padding_ = {Left: config.Padding, Top: config.Padding, Right: config.Padding, Bottom: config.Padding};
	}
	else if(typeof(config.Padding) == "object" && config.Padding.push != undefined)
	{
		if(config.Padding.length == 1)  padding_ = {Top: config.Padding[0], Right: config.Padding[0], Bottom: config.Padding[0], Left: config.Padding[0]};
		else if(config.Padding.length == 2)  padding_ = {Top: config.Padding[0], Right: config.Padding[1], Bottom: config.Padding[0], Left: config.Padding[1]};
		else if(config.Padding.length == 4)  padding_ = {Top: config.Padding[0], Right: config.Padding[1], Bottom: config.Padding[2], Left: config.Padding[3]};
	}
	if(padding_ == null) padding_ = {Left: 0, Top: 0, Right: 0, Bottom: 0};
	
	var IsNull = Core.Utility.IsNull;

	var id_ = IsNull(config.ID, GenerateControlId());
	id_ = id_.replace(/\x2F/g, "");
	var border_width_ = IsNull(config.BorderWidth, 0);
	var min_size_ = { Width: IsNull(config.MinWidth, 0), Height: IsNull(config.MinHeight, 0) };
	var max_size_ = { Width: IsNull(config.MaxWidth, 10000), Height: IsNull(config.MaxHeight, 10000) };
	var resizable_ = IsNull(config.Resizable, false);
	var splitter_size_ = { Left: 0, Right: 0, Top: 0, Bottom: 0, Value: 0 };
	var splitter_ = null;
	if(config.SplitterSize == undefined && resizable_)
	{
		config.SplitterSize = 5;
	}
	if(config.SplitterSize != undefined && typeof(config.SplitterSize) == "number")
	{
		splitter_size_.Value = config.SplitterSize;
		switch(dock_style_)
		{
		case Core.UI.DockStyle.Left:
			{
				splitter_size_.Right = config.SplitterSize;
				splitter_ = document.createElement("DIV");
				splitter_.className = IsNull(config.SplitterCss, "ct_hsplitter");
				if(resizable_) splitter_.className += " ct_hspliter_resizable";
				break;
			}
		case Core.UI.DockStyle.Right:
			{
				splitter_size_.Left = config.SplitterSize;
				splitter_ = document.createElement("DIV");
				splitter_.className = IsNull(config.SplitterCss, "ct_hsplitter");
				if(resizable_) splitter_.className += " ct_hspliter_resizable";
				break;
			}
		case Core.UI.DockStyle.Top:
			{
				splitter_size_.Bottom = config.SplitterSize;
				splitter_ = document.createElement("DIV");
				splitter_.className = IsNull(config.SplitterCss, "ct_vsplitter");
				if(resizable_) splitter_.className += " ct_vspliter_resizable";
				break;
			}
		case Core.UI.DockStyle.Bottom:
			{
				splitter_size_.Top = config.SplitterSize;
				splitter_ = document.createElement("DIV");
				splitter_.className = IsNull(config.SplitterCss, "ct_vsplitter");
				if(resizable_) splitter_.className += " ct_vspliter_resizable";
				break;
			}
		}
	}
	
	if(splitter_ != null)
	{
		splitter_.style.padding = "0px";
		splitter_.style.margin = "0px";
		splitter_.style.position = "absolute";
	}
	
	var bounds_ = {
		Left: IsNull(config.Left, 0),
		Top: IsNull(config.Top, 0),
		Width: IsNull(config.Width, 0),
		Height: IsNull(config.Height, 0)
	};
	
	if(parent == Core.UI.PagePanel)
	{
		bounds_.Width = Core.UI.PagePanel.GetWidth();
		bounds_.Height = Core.UI.PagePanel.GetHeight();
	}
	
	var div_ = document.createElement("DIV");
	div_.style.padding = "0px";
	div_.style.margin = "0px";
	div_.style.borderWidth = border_width_ + "px";
	div_.style.position = "absolute";
	div_.style.display = "block";
	div_.style.paddingLeft = padding_.Left + "px";
	div_.style.paddingTop = padding_.Top + "px";
	div_.style.paddingRight = padding_.Right + "px";
	div_.style.paddingBottom = padding_.Bottom + "px";
	div_.className = "ct_control " + IsNull(config.Css, "");
	div_.style.display = (IsNull(config.Visible, true) ? "block" : "none");
	div_.innerHTML = IsNull(config.Content, "");
	
	var sub_controls_ = [];
	
	var ResetSplitter = function()
	{
		if(splitter_ != null)
		{			
			switch(dock_style_)
			{
			case Core.UI.DockStyle.Left:
				{
					splitter_.style.left = (bounds_.Left + bounds_.Width - margin_.Right - splitter_size_.Value) + "px";
					splitter_.style.top = (bounds_.Top + margin_.Top) + "px";
					splitter_.style.width = splitter_size_.Value + "px";
					splitter_.style.height = Math.max(0, bounds_.Height - margin_.Top - margin_.Bottom) + "px";
					break;
				}
			case Core.UI.DockStyle.Right:
				{
					splitter_.style.left = (bounds_.Left + margin_.Left) + "px";
					splitter_.style.top = (bounds_.Top + margin_.Top) + "px";
					splitter_.style.width = splitter_size_.Value + "px";
					splitter_.style.height = Math.max(0, bounds_.Height - margin_.Top - margin_.Bottom) + "px";
					break;
				}
			case Core.UI.DockStyle.Top:
				{
					splitter_.style.left = (bounds_.Left + margin_.Left) + "px";
					splitter_.style.top = (bounds_.Top + bounds_.Height - margin_.Bottom - splitter_size_.Value) + "px";
					splitter_.style.width = Math.max(0, bounds_.Width - margin_.Left - margin_.Right) + "px";
					splitter_.style.height = splitter_size_.Value + "px";
					break;
				}
			case Core.UI.DockStyle.Bottom:
				{
					splitter_.style.left = (bounds_.Left + margin_.Left) + "px";
					splitter_.style.top = (bounds_.Top + margin_.Top) + "px";
					splitter_.style.width = Math.max(0, bounds_.Width - margin_.Left - margin_.Right) + "px";
					splitter_.style.height = splitter_size_.Value + "px";
					break;
				}
			}
		}
	}
	
	var CalcNewSize = function(x, y)
	{
		var width = resize_data_.PreviousBounds.Width, height = resize_data_.PreviousBounds.Height;
		switch(resize_data_.Control.GetDockStyle())
		{
		case Core.UI.DockStyle.Left:
			{
				width += (x - resize_data_.PreviousClientX);
				break;
			}
		case Core.UI.DockStyle.Right:
			{
				width -= (x - resize_data_.PreviousClientX);
				break;
			}
		case Core.UI.DockStyle.Top:
			{
				height += (y - resize_data_.PreviousClientY);
				break;
			}
		case Core.UI.DockStyle.Bottom:
			{
				height -= (y - resize_data_.PreviousClientY);
				break;
			}
		}
		return { Width: width, Height: height }
	}
	
	if(resizable_)
	{
		var start_resizing_ = function(evt)
		{
			if(evt == undefined) evt = window.event;
			if(Core.Utility.GetButton(evt) != "Left") return;
			var coord = Core.Utility.GetClientCoord(splitter_);
			resize_data_.ClientX = evt.clientX;
			resize_data_.ClientY = evt.clientY;
			resize_data_.SplitterPlaceHolder.style.width = splitter_.style.width;
			resize_data_.SplitterPlaceHolder.style.height = splitter_.style.height;
			resize_data_.SplitterPlaceHolder.style.cursor = Core.Utility.GetCurrentStyleValue(splitter_, "cursor");
			Core.UI.PagePanel.SetCursor(Core.Utility.GetCurrentStyleValue(splitter_, "cursor"));
			Core.UI.PagePanel.Popup(resize_data_.SplitterPlaceHolder, coord.X, coord.Y);
			Core.UI.PagePanel.CaptureMouseEvent(	
				function(move_evt)
				{
					if(move_evt == undefined) move_evt = window.event;
					if(Core.Utility.GetButton(move_evt) != "Left")
					{
						if(resize_data_.Control != null)
						{
							Core.UI.PagePanel.ClosePopupCtrl();
							Core.UI.PagePanel.SetCursor("default");
						}
						return;
					}
					resize_data_.ClientX = move_evt.clientX;
					resize_data_.ClientY = move_evt.clientY;
					var s = CalcNewSize(resize_data_.ClientX, resize_data_.ClientY);
					var min_size = resize_data_.Control.Private.GetMinSize();
					var max_size = resize_data_.Control.Private.GetMaxSize();
					switch(resize_data_.Control.GetDockStyle())
					{
					case Core.UI.DockStyle.Left:
						{
							if(s.Width < min_size.Width) s.Width = min_size.Width;
							if(s.Width > max_size.Width) s.Width = max_size.Width;
							resize_data_.ClientX = resize_data_.PreviousClientX + (s.Width - resize_data_.PreviousBounds.Width);
							break;
						}
					case Core.UI.DockStyle.Right:
						{
							if(s.Width < min_size.Width) s.Width = min_size.Width;
							if(s.Width > max_size.Width) s.Width = max_size.Width;
							resize_data_.ClientX = resize_data_.PreviousClientX - (s.Width - resize_data_.PreviousBounds.Width);
							break;
						}
					case Core.UI.DockStyle.Top:
						{
							if(s.Height < min_size.Height) s.Height = min_size.Height;
							if(s.Height > max_size.Height) s.Height = max_size.Height;
							resize_data_.ClientY = resize_data_.PreviousClientY + (s.Height - resize_data_.PreviousBounds.Height);
							break;
						}
					case Core.UI.DockStyle.Bottom:
						{
							if(s.Height < min_size.Height) s.Height = min_size.Height;
							if(s.Height > max_size.Height) s.Height = max_size.Height;
							resize_data_.ClientY = resize_data_.PreviousClientY - (s.Height - resize_data_.PreviousBounds.Height);
							break;
						}
					}					
					var dock_style = resize_data_.Control.GetDockStyle();
					if(dock_style == Core.UI.DockStyle.Left || dock_style == Core.UI.DockStyle.Right)
					{
						resize_data_.SplitterPlaceHolder.style.left = resize_data_.PlaceHolderPreLeft + (resize_data_.ClientX - resize_data_.PreviousClientX) + "px";
					}
					if(dock_style == Core.UI.DockStyle.Top || dock_style == Core.UI.DockStyle.Bottom)
					{
						resize_data_.SplitterPlaceHolder.style.top = resize_data_.PlaceHolderPreTop + (resize_data_.ClientY - resize_data_.PreviousClientY) + "px";
					}
					
				},
				function(up_evt)
				{
					if(up_evt == undefined) up_evt = window.event;
					Core.UI.PagePanel.ClosePopupCtrl();
					Core.UI.PagePanel.SetCursor("default");
					var s = CalcNewSize(resize_data_.ClientX, resize_data_.ClientY);
					var min_size = resize_data_.Control.Private.GetMinSize();
					var max_size = resize_data_.Control.Private.GetMaxSize();
					switch(resize_data_.Control.GetDockStyle())
					{
					case Core.UI.DockStyle.Left:
					case Core.UI.DockStyle.Right:
						{
							if(s.Width < min_size.Width) s.Width = min_size.Width;
							if(s.Width > max_size.Width) s.Width = max_size.Width;		
							break;
						}
					case Core.UI.DockStyle.Top:
					case Core.UI.DockStyle.Bottom:
						{
							if(s.Height < min_size.Height) s.Height = min_size.Height;
							if(s.Height > max_size.Height) s.Height = max_size.Height;
							break;
						}
					}
					resize_data_.Control.Resize(s.Width, s.Height);
					resize_data_.Control = null;
				}
			);
			resize_data_.Splitter = splitter_;
			resize_data_.PlaceHolderPreLeft = Core.Utility.ParseInt(resize_data_.SplitterPlaceHolder.style.left);
			resize_data_.PlaceHolderPreTop = Core.Utility.ParseInt(resize_data_.SplitterPlaceHolder.style.top);
			resize_data_.PreviousClientX = evt.clientX;
			resize_data_.PreviousClientY = evt.clientY;
			resize_data_.PreviousBounds = this_.GetBounds();
			resize_data_.Control = this_;
			Core.Utility.CancelBubble(evt);
		}
		
		splitter_.onmousedown = start_resizing_;
	}
	
	this_.OnResize = new Core.Delegate();
	
	this_.Private = {};
	
	this_.Private.GetMinSize = function()
	{
		return min_size_;
	}
	
	this_.Private.GetMaxSize = function()
	{
		return max_size_;
	}
	
	this_.Private.OnAttach = function(p, container)
	{
		container.appendChild(div_);
		if(splitter_) container.appendChild(splitter_);
	}
	
	this_.Private.OnDetach = function(p, container)
	{
		container.removeChild(div_);
		if(splitter_) container.removeChild(splitter_);
	}
	
	this_.Private.SetBounds = function(bounds, focus)
	{
		if(bounds != undefined)
		{
			if(focus == undefined) focus = false;
			
			bounds_.Left = bounds.Left;
			bounds_.Top = bounds.Top;
			
			if(focus || dock_style_ != Core.UI.DockStyle.Left && dock_style_ != Core.UI.DockStyle.Right)
			{
				bounds_.Width = bounds.Width;
			}
			if(focus || dock_style_ != Core.UI.DockStyle.Top && dock_style_ != Core.UI.DockStyle.Bottom)
			{
				bounds_.Height = bounds.Height;
			}
		}

		var left = bounds_.Left + margin_.Left + splitter_size_.Left;
		var top = bounds_.Top + margin_.Top + splitter_size_.Top;
		var width = this_.GetClientWidth();
		var height = this_.GetClientHeight();

		div_.style.left = (left + 1) + "px";
		div_.style.top =  (top + 1) + "px";
		div_.style.width = (width + 1) + "px";
		div_.style.height = (height + 1) + "px";

		div_.style.left = left + "px";
		div_.style.top =  top + "px";
		div_.style.width = width + "px";
		div_.style.height = height + "px";

		ResetSplitter();
	}
	
	this_.Private.FindControl = function(path_nodes, start_index)
	{
		var ctrl = null;
		for(var i in sub_controls_)
		{
			if(sub_controls_[i].GetControlId() == path_nodes[start_index]) 
			{
				ctrl = sub_controls_[i];
			}
		}
		if(ctrl == null) return null;
		if(start_index == path_nodes.length - 1) return ctrl;
		return ctrl.Private.FindControl(path_nodes, start_index + 1);
	}
	
	this_.GetControlId = function()
	{
		return id_;
	}
	
	this_.FindControl = function(path)
	{
		var path_nodes = path.split('/');
		return this_.Private.FindControl(path_nodes, 0);
	}
	
	this_.FindControlRecursive = function(id)
	{
		for(var i in sub_controls_)
		{
			var c = sub_controls_[i];
			if(c.GetControlId() == id) 
			{
				return c;
			}
		}
		for(var i in sub_controls_)
		{
			var c = sub_controls_[i];
			var ctrl = c.FindControlRecursive(id);
			if(ctrl != null) return ctrl;
		}
		return null;
	}
	
	this_.FindContainerRecursive = function(name)
	{
		var ctrl = this_.FindControlRecursive(name);
		return ctrl == null ? null : ctrl.GetContainer();
	}

	this_.AppendChildNode = function(elem)
	{
		div_.appendChild(elem);
	}
	
	this_.GetClientWidth = function()
	{
		return Math.max(0, bounds_.Width - border_width_ * 2 - margin_.Left - margin_.Right - splitter_size_.Left - splitter_size_.Right - padding_.Left - padding_.Right);
	}
	
	this_.GetClientHeight = function()
	{
		return Math.max(0, bounds_.Height - border_width_ * 2 - margin_.Top - margin_.Bottom - splitter_size_.Top - splitter_size_.Bottom - padding_.Top - padding_.Bottom);
	}
	
	this_.GetBounds = function()
	{
		return {
			Left: bounds_.Left,
			Top: bounds_.Top,
			Width:  bounds_.Width,
			Height:  bounds_.Height
		};
	}
	
	this_.GetClientBounds = function()
	{
		return {
			Left: padding_.Left, Top: padding_.Top,
			Width: this_.GetClientWidth(),
			Height: this_.GetClientHeight()
		};
	}
	
	this_.Resize = function(width, height)
	{
		if(dock_style_ != Core.UI.DockStyle.Left && dock_style_ != Core.UI.DockStyle.Right)
		{
			bounds_.Height = height;
		}
		if(dock_style_ != Core.UI.DockStyle.Top && dock_style_ != Core.UI.DockStyle.Bottom)
		{
			bounds_.Width = width;
		}
		div_.style.width = this_.GetClientWidth() + "px";
		div_.style.height = this_.GetClientHeight() + "px";
		this_.OnResize.Call();
	}
	
	this_.GetDockStyle = function()
	{
		return dock_style_;
	}
	
	this_.GetMargin = function()
	{
		return margin_;
	}
	
	this_.SetVisible = function(visible, fire_event)
	{
		if(fire_event == undefined) fire_event = true;
		var pre_visible = this_.IsVisible();
		div_.style.display = (visible ? "block" : "none");
		if(splitter_ != null)
		{
			splitter_.style.display = (visible ? "block" : "none");
		}
		if(fire_event == true && pre_visible != visible && parent != null)
		{
			parent.RepositionSubControls();
		}
	}
	
	this_.IsVisible = function()
	{
		return div_.style.display == "block";
	}
	
	this_.AddControl = function(ctrl, k, repos)
	{
		if(repos == undefined) repos = true;

		var index = null;
		if(typeof(k) == "string")
		{
			for(var i in sub_controls_)
			{
				if(sub_controls_[i].ID == k) index = i;
			}
		}
		else if(typeof(k) == "number")
		{
			index = k;
		}
		else if(typeof(k) == "object")
		{
			for(var i in sub_controls_)
			{
				if(sub_controls_[i] == k) index = i;
			}
		}
		
		if(index != null && index >= 0 && index < sub_controls_.length)
		{
			var new_ctrls = [];
			var i = 0;
			for(i = 0; i < index && i < sub_controls_.length; i++)
			{
				new_ctrls.push(sub_controls_[i]);
			}
			new_ctrls.push(ctrl);
			for(;i < sub_controls_.length; i++)
			{
				new_ctrls.push(sub_controls_[i]);
			}
			sub_controls_ = new_ctrls;
		}
		else
		{
			sub_controls_.push(ctrl);
		}
		ctrl.Private.OnAttach(this_, div_);
		if(repos) this_.RepositionSubControls();
	}
	
	this_.RemoveControl = function(k)
	{
		var index = null;
		if(typeof(k) == "string")
		{
			for(var i in sub_controls_)
			{
				if(sub_controls_[i].ID == k) index = i;
			}
		}
		else if(typeof(k) == "number")
		{
			index = k;
		}
		else if(typeof(k) == "object")
		{
			for(var i in sub_controls_)
			{
				if(sub_controls_[i] == k) index = i;
			}
		}
		if(index != null && index >= 0 && index < sub_controls_.length)
		{
			sub_controls_.splice(index, 1);
			ctrl.Private.OnDetach(this_, div_);
			this_.RepositionSubControls();
		}
	}
	
	this_.GetContainer = function(path)
	{
		if(path != undefined && path != null && path != "")
		{
			var ctrl = this_.FindControl(path);
			return (ctrl == null ? null : ctrl.GetContainer());		
		}
		else
		{
			return div_;
		}
	}
	
	this_.RepositionSubControls = function()
	{
		var bounds = this_.GetClientBounds();
		for(var i in sub_controls_)
		{
			var ctrl = sub_controls_[i];
			if(!ctrl.IsVisible())
			{
				continue;
			}
			if(bounds.Width <= 0 || bounds.Height <= 0) 
			{
				ctrl.Private.SetBounds({Left: 0, Top: 0, Width: 0, Height: 0});
				continue;
			}
			var ctrl_bounds = ctrl.GetBounds();
			var ctrl_dock_style = ctrl.GetDockStyle();
			var ctrl_new_bounds = null;
			switch (ctrl_dock_style)
			{
			case Core.UI.DockStyle.Left:
				{
					ctrl_new_bounds = {
						Left: bounds.Left,
						Top: bounds.Top,
						Width: ctrl_bounds.Width,
						Height: bounds.Height
					};
					bounds.Left += ctrl_bounds.Width;
					bounds.Width -= ctrl_bounds.Width;
					break;
				}
			case Core.UI.DockStyle.Right:
				{
					ctrl_new_bounds = {
						Left: bounds.Left + bounds.Width - ctrl_bounds.Width,
						Top: bounds.Top,
						Width: ctrl_bounds.Width,
						Height: bounds.Height
					};
					bounds.Width -= ctrl_bounds.Width;
					break;
				}
			case Core.UI.DockStyle.Top:
				{
					ctrl_new_bounds = {
						Left: bounds.Left,
						Top: bounds.Top,
						Width: bounds.Width,
						Height: ctrl_bounds.Height
					};
					bounds.Top += ctrl_bounds.Height;
					bounds.Height -= ctrl_bounds.Height;
					break;
				}
			case Core.UI.DockStyle.Bottom:
				{
					ctrl_new_bounds = {
						Left: bounds.Left,
						Top: bounds.Top + bounds.Height - ctrl_bounds.Height,
						Width: bounds.Width,
						Height: ctrl_bounds.Height
					};
					bounds.Height -= ctrl_bounds.Height;
					break;
				}
			case Core.UI.DockStyle.Fill:
				{
					ctrl_new_bounds = {
						Left: bounds.Left,
						Top: bounds.Top,
						Width: bounds.Width,
						Height: bounds.Height
					};
					bounds = {Left: 0, Top: 0, Width: 0, Height: 0};
					break;
				}
			}
			if(ctrl_new_bounds != null)
			{
				if(ctrl_new_bounds.Width <= 0) ctrl_new_bounds.Width = 0;
				if(ctrl_new_bounds.Height <= 0) ctrl_new_bounds.Height = 0;
				ctrl.Private.SetBounds(ctrl_new_bounds);
				ctrl.RepositionSubControls();
			}
		}
	}
	
	this_.OnResize.Attach(
		function()
		{
			if(parent != null && parent != Core.UI.PagePanel) parent.RepositionSubControls();
			else this_.RepositionSubControls();
		}
	);

	if(parent != null)
	{
		if(parent != Core.UI.PagePanel)
		{
			parent.AddControl(this_, null, false);
		}
		else
		{
			parent.AppendChildNode(div_);
		}
	}

	if(config.Controls != undefined)
	{
		for(var i in config.Controls)
		{
			var ctrl_config = config.Controls[i];
			var ctrl = new (ctrl_config.Type || Core.UI.Control)(this_, ctrl_config);
		}
	}
	
	if(parent == Core.UI.PagePanel)
	{
		Core.UI.PagePanel.OnResize.Attach(
			function() 
			{ 
				this_.Resize(Core.UI.PagePanel.GetWidth(), Core.UI.PagePanel.GetHeight());
			}
		);
	}
	
	this_.Private.SetBounds();	
	if(parent == Core.UI.PagePanel)
	{
		this_.RepositionSubControls();
	}
}

Core.UI.TabControlButton = function(tab_config, header_dom)
{
	var this_ = this;

	this_.Config = tab_config;

	var dom_ = document.createElement("DIV");
	dom_.className = "ct tab_button_normal";
	dom_.innerHTML =
	'<div class="btn_left">' +
	'</div>' +
	'<div class="btn_center">' +
	'</div>' +
	'<div class="btn_right">' +
	'</div>';

	dom_.childNodes[1].innerHTML = String.format("<span>{0}</span>", Core.Utility.FilterHtml(tab_config.Text));

	header_dom.appendChild(dom_);

	if (tab_config.MinWidth != undefined)
	{
		if (tab_config.MinWidth > dom_.offsetWidth)
		{
			dom_.childNodes[1].style.width = (tab_config.MinWidth - dom_.childNodes[0].offsetWidth - dom_.childNodes[2].offsetWidth) + "px";
		}
	}

	this_.Select = function()
	{
		Core.Utility.ModifyCss(dom_, "-tab_button_normal", "tab_button_selected");
	}

	this_.Deselect = function()
	{
		Core.Utility.ModifyCss(dom_, "-tab_button_selected", "tab_button_normal");
	}
	
	this_.SetVisible = function(visible)
	{
		dom_.style.display = (visible ? "block" : "none");
	}
	
	this_.SetText = function(text)
	{
		this_.Config.Text = text;
		dom_.childNodes[1].innerHTML = String.format("<span>{0}</span>", Core.Utility.FilterHtml(this_.Config.Text));
	}

	this_.OnClick = new Core.Delegate();

	dom_.onclick = function()
	{
		this_.OnClick.Call(this_);
	}
}

Core.UI.TabControl = function(parent, config)
{
	var this_ = this;

	Core.UI.Control.call(this_, parent, config);
	
	var container = this_.GetContainer();
	
	var div_ = document.createElement("DIV");
	div_.className = "tab_body";
	div_.innerHTML = "<div class='tab_header'></div>";
	container.appendChild(div_);
	
	var header_dom_ = div_.firstChild;
	
	var tabs_ = [];
	var tabs_table_ = {};
	var current_selected_tab_ = null;
	
	this_.OnSelectedTab = new Core.Delegate();

	var SelectTab = function(t)
	{
		if(current_selected_tab_ != t)
		{
			var pretab = "";
			if (current_selected_tab_ != null)
			{
				current_selected_tab_.Deselect();
				current_selected_tab_.Panel.SetVisible(false);
				pretab = current_selected_tab_.Config.ID;
			}
			current_selected_tab_ = t;
			current_selected_tab_.Select();
			current_selected_tab_.Panel.SetVisible(true);
			this_.RepositionSubControls();
			this_.OnSelectedTab.Call(current_selected_tab_.Config.ID, pretab);
		}
	}

	var header_current_style = Core.Utility.GetCurrentStyle(header_dom_);
	
	for (var i in config.Tabs)
	{
		var tab = new Core.UI.TabControlButton(config.Tabs[i], header_dom_);
		tab.Config.DockStyle = Core.UI.DockStyle.Fill;
		tab.Config.Margin = [header_current_style == null ? 2 : Core.Utility.ParseInt(header_current_style.height) + 2, 2, 2, 2];
		tab.Panel = new Core.UI.Control(this_, tab.Config);
		tab.Panel.SetVisible(false, false);
		tab.OnClick.Attach(
			function(t)
			{
				SelectTab(t);
			}
		);
		if (tab.Config.IsSelected == true) SelectTab(tab);

		tabs_.push(tab);
		tabs_table_[tab.Config.ID] = tab;
		tab = null;
	}

	this_.GetPanel = function(id)
	{
		return tabs_table_[id] == undefined ? null : tabs_table_[id].Panel;
	}
	
	this_.GetTabPanelContainer = function(id, path)
	{
		var panel = this_.GetPanel(id);
		return panel == null ? null : panel.GetContainer(path);
	}
	
	this_.SetTabVisible = function(id, visible)
	{
		if(tabs_table_[id] != undefined)
		{
			var tab = tabs_table_[id];
			tab.SetVisible(visible);
		}
	}
	
	this_.SetTabTitle = function(id, title)
	{
		if(tabs_table_[id] != undefined)
		{
			var tab = tabs_table_[id];
			tab.SetText(title);
		}
	}
	
	this_.GetCurrentTab = function()
	{
		return current_selected_tab_ == null ? "" : current_selected_tab_.Config.ID;
	}
	
	this_.Select = function(id)
	{
		if(tabs_table_[id] != undefined)
		{
			var tab = tabs_table_[id];
			SelectTab(tab);
		}
	}
	
	this_.RepositionSubControls();
}

})();