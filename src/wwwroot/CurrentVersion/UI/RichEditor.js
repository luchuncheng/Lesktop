(function(){

if(window.Core == undefined) window.Core = {};
if(window.Core.UI == undefined) window.Core.UI = {};

var FontSizeToPixel = [0, "xx-small", "x-small", "small", "medium", "large", "x-large", "xx-large"];
var PixelToFontSize = {
	"xx-small" : 1, 
	"x-smaller" : 2, 
	"small" : 3, 
	"medium" : 4, 
	"large" : 5, 
	"x-large" : 6, 
	"xx-large" : 7
};

function IsEmpty(str)
{
	for (var i = 0; i < str.length; )
	{
		var c = str.substr(i, 1);
		if (str.substr(i, 6).toLowerCase() == "&nbsp;") i += 6;
		else if (c == '\n' || c == '\r' || c == '\f' || c == '\t' || c == '\v' || c == ' ') i++;
		else return false;
	}
	return true;
}

var EmotionForm = (function()
{
	var this_ = {};

	var callback_ = null;
	var cur_unit_ = null;

	var dom = document.createElement("DIV");
	dom.className = "emotform";
	dom.id = Core.GenerateUniqueId();
	dom.style.display = "none";
	dom.style.width = "436px";
	dom.style.height = "175px";
	
	for (var y = 0; y < 6; y++)
	{
		for (var x = 0; x < 15; x++)
		{
			(function(x, y)
			{
				var emot_div = document.createElement("DIV");
				emot_div.className = "emotunit";
				Core.UI.AttachButtonEvent(emot_div);
				emot_div.onmousedown = function(evt)
				{
					cur_unit_ = emot_div;
					Core.Utility.CancelBubble(evt == undefined ? window.event : evt);
					this_.Close();
					if (callback_ != null) callback_(y * 15 + x + 100);
				}
				dom.appendChild(emot_div);
			})(x, y)
		}
	}

	this_.Popup = function(btn, callback)
	{
		var btn_coord = Core.Utility.GetClientCoord(btn);
		var y = btn_coord.Y + btn.offsetHeight;	
		if(y + this_.GetHeight() > Core.UI.PagePanel.GetHeight())
		{
			y = btn_coord.Y - this_.GetHeight();
		}
		var x = btn_coord.X;
		
		Core.UI.PagePanel.Popup(
			this_.GetDom(), x, y,
			function()
			{
				if (callback_ != null) callback_();
			}
		);
		
		callback_ = callback;
		
		if(cur_unit_ != null)
		{
			Core.Utility.ModifyCss(cur_unit_, "-ct_hover", "-ct_press", "ct_normal");
		}
	}

	this_.Close = function()
	{
		Core.UI.PagePanel.ClosePopupCtrl();
	}

	this_.GetDom = function()
	{
		return dom;
	}

	this_.GetWidth = function()
	{
		return 436;
	}

	this_.GetHeight = function()
	{
		return 175;
	}

	return this_;
})();

var ColorSelForm = (function()
{
	var this_ = {};

	var dom = document.createElement("DIV");
	dom.className = "color_select_from";
	dom.style.display = "none";
	dom.style.width = "144px";
	dom.style.height = "144px";

	var colors_ = [
		"#000000", "#400000", "#800000", "#804040", "#FF0000", "#FF8080",
		"#808000", "#804000", "#FF8000", "#FF8040", "#FFFF00", "#FFFF80",
		"#808080", "#004000", "#008040", "#008080", "#00FF40", "#00FF80",
		"#408080", "#000080", "#0000FF", "#004080", "#00FFFF", "#0080C0",
		"#C0C0C0", "#000040", "#0000A0", "#8080FF", "#008040", "#0080FF",
		"#FFFFFF", "#400040", "#800080", "#800040", "#8080C0", "#FF8040"
	];

	var callback_ = null;
	var cur_color_unit = null;

	for (var y = 0; y < 6; y++)
	{
		for (var x = 0; x < 6; x++)
		{
			(function(x, y)
			{
				var emot_div = document.createElement("DIV");
				emot_div.className = "color_unit";
				emot_div.innerHTML = "<div class='color_unit_pre'></div>";
				emot_div.firstChild.style.backgroundColor = colors_[y * 6 + x];
				emot_div.title = colors_[y * 6 + x];
				Core.UI.AttachButtonEvent(emot_div);
				emot_div.onmousedown = function(evt)
				{
					cur_color_unit = emot_div;
					Core.Utility.CancelBubble(evt == undefined ? window.event : evt);
					this_.Close();
					if (callback_ != null) callback_(colors_[y * 6 + x]);
				}
				dom.appendChild(emot_div);
			})(x, y)
		}
	}

	Core.Utility.DisableSelect(dom, true);

	this_.Popup = function(btn, callback)
	{
		var btn_coord = Core.Utility.GetClientCoord(btn);
		var y = btn_coord.Y + btn.offsetHeight;	
		if(y + this_.GetHeight() > Core.UI.PagePanel.GetHeight())
		{
			y = btn_coord.Y - this_.GetHeight();
		}
		var x = btn_coord.X;
		
		Core.UI.PagePanel.Popup(
			ColorSelForm.GetDom(), x, y,
			function()
			{
				if (callback_ != null) callback_();
			}
		);
		
		if(cur_color_unit != null)
		{
			Core.Utility.ModifyCss(cur_color_unit, "-ct_hover", "-ct_press", "ct_normal");
		}
		
		callback_ = callback;
	}

	this_.Close = function()
	{
		Core.UI.PagePanel.ClosePopupCtrl();
	}

	this_.GetDom = function()
	{
		return dom;
	}

	this_.GetWidth = function()
	{
		return 144 + 2;
	}

	this_.GetHeight = function()
	{
		return 144 + 2;
	}

	return this_;
})();

Core.UI.RichEditor = function(container, config_)
{
	var this_ = {};
	
	container.innerHTML =
	'<div class="richeditor">' +
		'<div class="ct_toolbar"></div>' +
		'<div class="richeditor_frame"><iframe class="" frameborder="0"></iframe></div>' +
	'</div>';

	var toolbar = new Core.UI.Toolbar(
		container.firstChild.childNodes[0],
		{
			Items: [
				{
					ID: "richeditor_btn_emot",
					Type: "Button",
					Css: "image_button btn_emot",
					Tooltip: "表情",
					Config: { Text: "", TabIndex: 1 }
				},
				{
					ID: "richeditor_btn_blod",
					Type: "Button",
					Css: "image_button btn_b",
					Tooltip: "粗体",
					Config: { Text: "", TabIndex: 2 }
				},
				{
					ID: "richeditor_btn_italic",
					Type: "Button",
					Css: "image_button btn_i",
					Tooltip: "斜体",
					Config: { Text: "", TabIndex: 3 }
				},
				{
					ID: "richeditor_btn_underline",
					Type: "Button",
					Css: "image_button btn_u",
					Tooltip: "下划线",
					Config: { Text: "", TabIndex: 4 }
				},
				{
					ID: "richeditor_btn_color",
					Type: "Button",
					Css: "image_button btn_color",
					Tooltip: "文字颜色",
					Config: { Text: "", TabIndex: 5 }
				},
				{
					ID: "richeditor_btn_backcolor",
					Type: "Button",
					Css: "image_button btn_backcolor",
					Tooltip: "文字背景色",
					Config: { Text: "", TabIndex: 6 }
				},
				{
					ID: "combox_fontsize",
					Type: "Combox",
					Config: {
						CustomCss: "combox_fontsize",
						TabIndex: 6,
						Items: [
							{ Text: "8", Value: 1 },
							{ Text: "10", Value: 2 },
							{ Text: "12", Value: 3 },
							{ Text: "14", Value: 4 },
							{ Text: "18", Value: 5 },
							{ Text: "24", Value: 6 },
							{ Text: "36", Value: 7 }
						]
					}
				},
				{
					ID: "combox_font",
					Type: "Combox",
					Config: { CustomCss: "combox_font",
						TabIndex: 7,
						Items: [
							{ Text: "宋体", Value: "simsun" },
							{ Text: "Times New Roman", Value: "times new roman" },
							{ Text: "Courier New", Value: "courier new" },
							{ Text: "Verdana", Value: "verdana" }
						]
					}
				}
			]
		}
	);
	
	var btn_bold = toolbar.GetItem("richeditor_btn_blod");
	var btn_italic = toolbar.GetItem("richeditor_btn_italic");
	var btn_underline = toolbar.GetItem("richeditor_btn_underline");

	var combox_font = toolbar.GetItem("combox_font");
	combox_font.SetValue("simsun");

	var combox_fontsize = toolbar.GetItem("combox_fontsize");
	combox_fontsize.SetValue(2);
	
	var btn_color = toolbar.GetItem("richeditor_btn_color");
	var btn_backcolor = toolbar.GetItem("richeditor_btn_backcolor");
	var btn_emot = toolbar.GetItem("richeditor_btn_emot");
			
	var editor = container.firstChild.childNodes[1].firstChild;
	editor.width = "100%";
	editor.height = "100%";
	editor.style.position = "absolute";
	editor.style.width = "100%";
	editor.style.height = "100%";

	var editor_document_ = editor.contentWindow.document;
	var editor_window_ = editor.contentWindow;
	var default_fmt_ = {
		Italic: false,
		Underline: false,
		FontSize: "small",
		FontName: "宋体",
		ForeColor: "#000000"
	};

	if (Core.GetBrowser() == "Firefox")
	{
		editor.onload = function()
		{
			editor_document_.designMode = "on";
		}
	}
	else
	{
		editor_document_.designMode = "on";
	}

	editor_document_.open();
	editor_document_.write("<html><head></head><body style='margin:0px; padding: 0px; font-family: SimSun; font-size: small; height:100%;'></body></html>");
	editor_document_.close();

	if (window.ClientMode != true)
	{
		Core.Utility.AttachEvent(
			editor_document_, "mousedown",
			function()
			{
				if(window.CurrentWindow != undefined) window.CurrentWindow.BringToTop();
			}
		);
	}

	function RefreshToolbarStatus(viafmt)
	{
		if(viafmt == true)
		{
			if(default_fmt_.FontSize != null) combox_fontsize.SetValue(PixelToFontSize[default_fmt_.FontSize], false);		
			if(default_fmt_.FontName != null) combox_font.SetValue(default_fmt_.FontName, false);
			if(default_fmt_.Bold!= null) btn_bold.SetStatus(default_fmt_.Bold ? "ct_checked" : "ct_normal");
			if(default_fmt_.Italic != null) btn_italic.SetStatus(default_fmt_.Italic ? "ct_checked" : "ct_normal");
			if(default_fmt_.Underline != null) btn_underline.SetStatus(default_fmt_.Underline ? "ct_checked" : "ct_normal");
		}
		else
		{
			var font_size = editor_document_.queryCommandValue("FontSize");
			if(font_size != null) combox_fontsize.SetValue(font_size, false);
		
			var font_name = editor_document_.queryCommandValue("FontName");
			if(font_name != null) combox_font.SetValue(font_name.toLowerCase(), false);
		
			var bold = editor_document_.queryCommandState("bold");
			if(bold != null) btn_bold.SetStatus(bold ? "ct_checked" : "ct_normal");
		
			var italic = editor_document_.queryCommandState("italic");
			if(italic != null) btn_italic.SetStatus(italic ? "ct_checked" : "ct_normal");
		
			var underline = editor_document_.queryCommandState("underline");
			if(underline != null) btn_underline.SetStatus(underline ? "ct_checked" : "ct_normal");
		}
	}	
	
	this_.OnKeyDown = new Core.Delegate();
	this_.OnKeyUp = new Core.Delegate();
	
	editor_document_.body.onmouseup = function(e)
	{
		RefreshToolbarStatus();
	}
	
	editor_document_.body.onkeydown = function(e)
	{
		if(e == undefined) e = editor_window_.event;
		this_.OnKeyDown.Call(e);
	}
	
	editor_document_.body.onkeyup = function(e)
	{
		if(e == undefined) e = editor_window_.event;
		this_.OnKeyUp.Call(e);
	}
	
	btn_bold.OnClick.Attach(
		function()
		{
			if(config_.BeforeSetFormat == undefined || !config_.BeforeSetFormat("Bold"))
			{
				editor_document_.execCommand("Bold", false, null);
				RefreshToolbarStatus();
			}
			this_.Focus();
		}
	);
	
	btn_italic.OnClick.Attach(
		function()
		{
			if(config_.BeforeSetFormat == undefined || !config_.BeforeSetFormat("Italic"))
			{
				editor_document_.execCommand("Italic", false, null);
				RefreshToolbarStatus();
			}
			this_.Focus();
		}
	);
	
	btn_underline.OnClick.Attach(
		function()
		{
			if(config_.BeforeSetFormat == undefined || !config_.BeforeSetFormat("Underline"))
			{
				editor_document_.execCommand("Underline", false, null);
				RefreshToolbarStatus();
			}
			this_.Focus();
		}
	);
	
	combox_fontsize.OnChanged.Attach(
		function()
		{
			if(config_.BeforeSetFormat == undefined || !config_.BeforeSetFormat("FontSize", FontSizeToPixel[combox_fontsize.GetValue()]))
			{
				editor_document_.execCommand("FontSize", false, combox_fontsize.GetValue());
				RefreshToolbarStatus();
			}
			this_.Focus();
		}
	);

	combox_font.OnChanged.Attach(
		function()
		{
			if(config_.BeforeSetFormat == undefined || !config_.BeforeSetFormat("FontName", combox_font.GetValue()))
			{
				editor_document_.execCommand("FontName", false, combox_font.GetValue());
				RefreshToolbarStatus();
			}
			this_.Focus();
		}
	);
	
	btn_color.OnClick.Attach(
		function()
		{
			ColorSelForm.Popup(
				btn_color.GetDom(), 
				function(color)
				{
					btn_color.SetStatus("ct_normal");
					if(color != undefined)
					{
						if(config_.BeforeSetFormat == undefined || !config_.BeforeSetFormat("ForeColor", color))
						{
							editor_document_.execCommand("ForeColor", false, color);
							RefreshToolbarStatus();
						}
						this_.Focus();
					}
				}
			);
			btn_color.SetStatus("ct_checked");
		}
	);
	
	btn_backcolor.OnClick.Attach(
		function()
		{
			ColorSelForm.Popup(
				btn_backcolor.GetDom(), 
				function(color)
				{
					btn_backcolor.SetStatus("ct_normal");
					if(color != undefined)
					{
						if(config_.BeforeSetFormat == undefined || !config_.BeforeSetFormat("BackColor", color))
						{
							editor_document_.execCommand("BackColor", false, color);
							RefreshToolbarStatus();
						}
						this_.Focus();
					}
				}
			);
			btn_backcolor.SetStatus("ct_checked");
		}
	);
	
	btn_emot.OnClick.Attach(
		function()
		{
			EmotionForm.Popup(
				btn_emot.GetDom(), 
				function(emot)
				{
					btn_emot.SetStatus("ct_normal");
					if(emot != undefined && config_.EmotUrlFmt != undefined)
					{
						this_.InsertImage(String.format(config_.EmotUrlFmt, emot));
					}
				}
			);
			btn_emot.SetStatus("ct_checked");
		}
	);
	
	this_.InsertImage = function(url)
	{
		this_.Focus();
		var emot_html = String.format("<img src='{0}' />", url);
		if (editor_document_.selection)
		{
			var range = editor_document_.selection.createRange();
			if(range.pasteHTML == undefined)
			{
				editor_document_.execCommand("InsertImage", false, url);
			}
			else
			{
				range.pasteHTML(emot_html);
			}
		}
		else
		{
			editor_document_.execCommand("InsertHtml", false, emot_html);
		}
	}
	
	this_.Focus = function()
	{
		try
		{
			if (Core.GetBrowser() == "IE")
			{
				editor_document_.body.focus();
			}
			else
			{
				editor.focus();
			}
		}
		catch(ex)
		{
		}
	}
	
	this_.GetDocument = function()
	{
		return editor_document_;
	}
	
	this_.AppendHtml = function(html)
	{
		editor_document_.body.innerHTML += html;
	}
	
	this_.LoadCss = function(url)
	{
		Core.Utility.LoadCss(url, editor_document_);
	}
	
	this_.SetText = function(text)
	{
		editor_document_.body.innerHTML = text;
	}
	
	this_.GetText = function(with_fmt)
	{
		if (with_fmt == false || IsEmpty(editor_document_.body.innerHTML)) return editor_document_.body.innerHTML;
		
		var html = editor_document_.body.innerHTML;
		if(html.length >= 6 && html.substr(0, 6) == "&nbsp;") html = html.substr(6, html.length - 6);

		var style = "";

		style += String.format("font-family: {0};", default_fmt_.FontName);
		style += String.format("font-size: {0};", default_fmt_.FontSize);
		style += String.format("color: {0};", default_fmt_.ForeColor);
		style += String.format("font-weight: {0};", default_fmt_.Bold ? "bold" : "normal");
		style += String.format("font-style: {0};", default_fmt_.Italic ? "italic" : "normal");
		style += String.format("text-decoration: {0};", default_fmt_.Underline ? "underline" : "none");

		return String.format("<div style='{0}; display:inline;'>{1}</div>", style, html);
	}

	this_.GetSelection = function()
	{
		if (Core.GetBrowser() == "IE")
		{
			var r = editor_document_.selection.createRange();
			if (r.htmlText != undefined) return r.htmlText; else return "";
		}
		else if (Core.GetBrowser() == "Firefox" || Core.GetBrowser() == "Chrome")
		{
			var sel = editor_window_.getSelection();
			if (sel.rangeCount > 0)
			{
				var r = null;
				r = sel.getRangeAt(0);
				return Core.Utility.GetInnerHTML(r.cloneContents().childNodes);
			}
			else
			{
				return "";
			}
		}
		else
		{
			return "";
		}
	}

	editor_document_.body.onblur = function()
	{
		RefreshToolbarStatus(true);
	}

	this_.SetDefaultFormat = function(fmt)
	{
		try
		{
			editor_document_.body.style.fontFamily = fmt.FontName;
			editor_document_.body.style.fontSize = fmt.FontSize;
			editor_document_.body.style.color = fmt.ForeColor;
			editor_document_.body.style.fontWeight = fmt.Bold ? "bold" : "normal";
			editor_document_.body.style.fontStyle = fmt.Italic ? "italic" : "normal";
			editor_document_.body.style.textDecoration = fmt.Underline ? "underline" : "none";

			RefreshToolbarStatus(true);

			default_fmt_ = fmt;
		}
		catch (ex)
		{
		}
	}
	
	return this_;
}

})();