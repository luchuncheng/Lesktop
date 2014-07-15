if(window.Core == undefined) window.Core = {};
if(window.Core.UI == undefined) window.Core.UI = {};

Core.UI.TreeView = function(container, config)
{
	var this_ = this;

	this_.Private = {};

	var rootNodes = null;
	var nodeIndex = null;
	var selectedNode = null;

	var dataSource = config.DataSource;
	var nodeHeight = Core.Utility.IsNull(config.NodeHeight, 32);
	var hasImage = Core.Utility.IsNull(config.HasImage, false);

	var dom = document.createElement("DIV");
	dom.style.overflow = "auto";
	dom.className = "ct treeview_body";
	dom.innerHTML = "<div style='overflow:visible;'></div>";
	container.appendChild(dom);

	
	this_.GetConfig = function()
	{
		return config;
	}

	this_.GetDom = function()
	{
		return dom;
	}

	this_.OnDblClick = new Core.Delegate();
	this_.OnClick = new Core.Delegate();
	this_.OnExpand = new Core.Delegate();
	this_.OnCollapse = new Core.Delegate();
	this_.OnBeginRequest = new Core.Delegate();
	this_.OnEndRequest = new Core.Delegate();
	
	this_.GetNode = function(callback, name)
	{
		if (rootNodes == null)
		{
			this_.Refresh(
				function(nodes, error)
				{
					if (nodes != null) callback(nodeIndex[name]);
					else callback(null, error);
				}
			);
		}
		else
		{
			callback(nodeIndex[name]);
		}
	}

	this_.GetExistingNode = function(path)
	{
		var pathNodes = path.split('/');
		if (pathNodes.length > 1 && pathNodes[0] == "")
		{
			if (nodeIndex == null) return null;
			if (nodeIndex[pathNodes[1]] == undefined) return null;
			return nodeIndex[pathNodes[1]].Private.GetExistingNode(pathNodes, 2);
		}
		else
		{
			return null;
		}
	}

	this_.Clear = function()
	{
		dom.firstChild.innerHTML = "";
	}
	
	this_.Refresh = function(callback)
	{
		try
		{
			dataSource.GetSubNodes(
				function(subNodeData, error)
				{
					if (subNodeData != null)
					{
						dom.firstChild.innerHTML = "";
						rootNodes = [];
						nodeIndex = {};
						for (var i in subNodeData)
						{
							var nd = subNodeData[i];
							var node = new Core.UI.TreeNode(this_, null, nd.Name, nd.Text, nd.ImageCss, nd.Tag, dom.firstChild, dataSource, nodeHeight, i == subNodeData.length - 1, nd.HasChildren, nd.ImageSrc, nd.HasCheckBox);
							rootNodes.push(node);
							nodeIndex[nd.Name] = node;
						}
						if(callback != undefined) callback(rootNodes);
					}
					else
					{
						if(callback != undefined) callback(null, error);
					}
				},
				null
			);
		}
		catch (ex)
		{
			if(callback != undefined) callback(null, new Core.Exception(ex.name, ex.message));
		}
	}

	this_.Load = this_.Refresh;
	
	this_.Find = function(callback, path)
	{
		var pathNodes = path.split('/');
		if (pathNodes.length > 1 && pathNodes[0] == "")
		{
			this_.GetNode(
				function(node, error)
				{
					if (node != null) node.Private.Find(callback, pathNodes, 2);
					else callback(null, error);
				},
				pathNodes[1]
			);
		}
		else
		{
			callback(null, new Core.Exception("Error", "invalid path!"));
		}
	}

	this_.GetSelectedNode = function()
	{
		return selectedNode;
	}

	this_.Private.Select = function(callback, node)
	{
		if (selectedNode != null) selectedNode.Private.Deselect();
		node.Private.Select(
			function(result, error)
			{
				if (result)
				{
					selectedNode = node;
					callback(node);
				}
				else
					callback(null, error);
			}
		);
	}
	
	this_.Select = function(callback, path)
	{
		this_.Find(
			function(node, error)
			{
				if (node != null) this_.Private.Select(callback, node);
				else callback(node, error);
			},
			path
		);
	}
	
	this_.Expand = function(callback, path)
	{
		this_.Find(
			function(node)
			{
				if (node == null)
				{
					callback(null);
					return;
				}
				node.Expand(
					function(result, error)
					{
						if (result) callback(node);
						else callback(null, error);
					}
				);
			},
			path
		);
	}

	this_.GetCheckedNodes = function()
	{
		var out = [];
		for (var i in rootNodes) rootNodes[i].GetCheckedNodes(out);
		return out;
	}

	this_.HitTest = function(x, y)
	{
		for (var i in rootNodes)
		{
			var node = rootNodes[i].HitTest(x, y);
			if (node != null) return node;
		}
		return null;
	}

	this_.GetAllNodes = function(out)
	{
		for (var i in rootNodes) rootNodes[i].GetAllNodes(out);
	}
	
	this_.GetRootNodes = function()
	{
		return rootNodes;
	}

	this_.GetDataSource = function()
	{
		return dataSource;
	}
}

Core.UI.TreeNode = function(treeView, parentNode, name, text, imgSrc, tag, container, dataSource, height, isLast, hasChildren, imagePath, hasCheckBox)
{
	if (hasChildren == undefined || hasChildren == null) hasChildren = true;

	if (hasCheckBox == undefined) hasCheckBox = false;

	var this_ = this;
	var subNodes = null;
	var nodeIndex = null;

	this_.Private = {};

	this_.GetFullPath = function()
	{
		return (parentNode == null ? "/" + name : parentNode.GetFullPath() + "/" + name);
	}
	
	this_.GetParent = function()
	{
		return parentNode;
	}

	this_.IsLast = function()
	{
		return isLast;
	}

	this_.IsExpand = function()
	{
		return isExpand;
	}

	this_.GetName = function()
	{
		return name;
	}

	this_.GetText = function()
	{
		return text;
	}

	this_.SetText = function(val)
	{
		text = val;
		textDiv.innerHTML = Core.Utility.FilterHtml(val);
	}

	this_.GetTag = function()
	{
		return tag;
	}

	//获取附加数据
	this_.SetTag = function(newtag)
	{
		tag = newtag;
	}
	
	this_.HasRefresh = function()
	{
		return subNodes != null;
	}
	this_.GetNode = function(callback, name)
	{
		if (subNodes == null)
		{
			this_.Refresh(
				function(nodes, error)
				{
					if (nodeIndex != null) callback(nodeIndex[name]);
					else callback(null, error);
				}
			);
		}
		else
		{
			callback(nodeIndex[name]);
		}
	}

	this_.Private.GetExistingNode = function(pathNodes, s)
	{
		if (s >= pathNodes.length)
		{
			return this_;
		}
		else
		{
			if (nodeIndex == null) return null;
			if (nodeIndex[pathNodes[s]] == undefined) return null;
			return nodeIndex[pathNodes[s]].Private.GetExistingNode(pathNodes, s + 1);
		}
	}

	this_.GetSubNodes = function(callback)
	{
		if (subNodes == null)
		{
			this_.Refresh(callback);
		}
		else
		{
			callback(subNodes);
		}
	}

	this_.Refresh = function(callback)
	{
		try
		{
			treeView.OnBeginRequest.Call(this_);
			dataSource.GetSubNodes(
				function(subNodeData, error)
				{
					if (subNodeData != null)
					{
						subNodes = [];
						nodeIndex = {};
						subNodesContainer.innerHTML = "";
						for (var i in subNodeData)
						{
							var nd = subNodeData[i];
							var node = new Core.UI.TreeNode(treeView, this_, nd.Name, nd.Text, nd.ImageCss, nd.Tag, subNodesContainer, dataSource, height, i == subNodeData.length - 1, nd.HasChildren, nd.ImageSrc, nd.HasCheckBox);
							subNodes.push(node);
							nodeIndex[nd.Name] = node;
						}
						if (this_.IsExpand())
						{
							subNodesContainer.style.display = (subNodes.length > 0 ? 'block' : 'none');
						}
						buttonDiv.className = GenerateBtnCss();
						callback(subNodes);
					}
					else
					{
						callback(null, error);
					}
					treeView.OnEndRequest.Call(this_);
				},
				this_
			);
		}
		catch (ex)
		{
			callback(null, new Core.Exception(ex.name, ex.message));
		}
	}
	this_.ResetBtnCss = function(last)
	{
		isLast = last;
		buttonDiv.className = GenerateBtnCss();
	}
	
	this_.AddSubNode = function(nd, index)
	{
		if(hasChildren == false) return null;
		if(subNodes == null) subNodes = [];
		
		var node = new Core.UI.TreeNode(treeView, this_, nd.Name, nd.Text, nd.ImageCss, nd.Tag, subNodesContainer, dataSource, 
			height, index < 0 || index >= subNodes.length, nd.HasChildren, nd.ImageSrc, nd.HasCheckBox, 
			(index >= 0 && index < subNodes.length) ? index : -1);
		if(index < 0 || index >= subNodes.length)
		{
			if(subNodes.length > 0)
			{
				subNodes[subNodes.length - 1].ResetBtnCss(false);
				subNodes[subNodes.length - 1].ResetLineCss();
			}
			subNodes.push(node);
		}
		else
		{
			var i = 0, nodes = [];
			for(;i < index; i++) nodes.push(subNodes[i]);
			nodes.push(node);
			for(;i < subNodes.length; i++) nodes.push(subNodes[i]);
			subNodes = nodes;
		}
		if (this_.IsExpand())
		{
			subNodesContainer.style.display = (subNodes.length > 0 ? 'block' : 'none');
		}
		return node;
	}
	
	this_.RemoveSubNode = function(node)
	{
		if(hasChildren == false || subNodes == null) return;
		var index = 0;
		for(;index < subNodes.length; index++)
		{
			if(subNodes[index] == node) break;
		}
		if(index > subNodes.length) return;
		if(index == subNodes.length - 1 && subNodes.length > 1)
		{
			subNodes[subNodes.length - 2].ResetBtnCss(true);
			subNodes[subNodes.length - 2].ResetLineCss();
		}
		subNodes.splice(index, 1);
		subNodesContainer.removeChild(node.GetDom());
		if (this_.IsExpand())
		{
			subNodesContainer.style.display = (subNodes.length > 0 ? 'block' : 'none');
		}
	}

	this_.Expand = function(callback)
	{
		function expand()
		{
			if (hasChildren && !isExpand)
			{
				this_.GetSubNodes(
					function(nodes, error)
					{
						if (nodes != null)
						{
							if (nodes.length > 0) subNodesContainer.style.display = 'block';
							isExpand = true;
							buttonDiv.className = GenerateBtnCss();
							treeView.OnExpand.Call(this_);
							callback(true);
						}
						else
						{
							callback(false, error);
						}
					}
				);
			}
			else
			{
				callback(true);
			}
		}
		if (parentNode != null)
		{
			parentNode.Expand(
				function(result, error)
				{
					if (result) expand();
					else callback(false, error);
				}
			);
		}
		else
		{
			expand();
		}

	}

	this_.Collapse = function()
	{
		if (hasChildren)
		{
			subNodesContainer.style.display = 'none';
			isExpand = false;
			buttonDiv.className = GenerateBtnCss();
			treeView.OnCollapse.Call(this_);
		}
		return true;
	}

	this_.GetCheckedNodes = function(out)
	{
		if (hasCheckBox && cbCell.firstChild.checked) out.push(this);

		for (var i in subNodes)
		{
			subNodes[i].GetCheckedNodes(out);
		}
	}

	this_.GetAllNodes = function(out)
	{
		if (out.push != undefined) out.push(this);
		else if (out.call != undefined) out(this);

		for (var i in subNodes)
		{
			subNodes[i].GetAllNodes(out);
		}
	}

	this_.Private.Find = function(callback, nodes, s)
	{
		if (s >= nodes.length)
		{
			callback(this_);
		}
		else
		{
			this_.GetNode(
				function(node, error)
				{
					if (node == null) callback(null, error);
					else node.Private.Find(callback, nodes, s + 1);
				},
				nodes[s]
			);
		}
	}

	this_.Private.Select = function(callback)
	{
		textDiv.className = 'nodeText_Selected'
		if (parentNode != null)
		{
			parentNode.Expand(
				function(result, error)
				{
					callback(result, error);
				}
			);
		}
		else
		{
			callback(true);
		}
	}


	this_.Private.Deselect = function()
	{
		textDiv.className = 'nodeText';
		return true;
	}

	this_.HitTest = function(x, y)
	{
		if (dom.firstChild.getBoundingClientRect != undefined)
		{
			var rect = dom.firstChild.getBoundingClientRect();
			if (x >= rect.left && x <= rect.right && y >= rect.top && y <= rect.bottom)
			{
				return this_;
			}
			else
			{
				for (var i in subNodes)
				{
					var node = subNodes[i].HitTest(x, y);
					if (node != null) return node;
				}
				return null;
			}
		}
	}
	
	this_.SetChecked = function(checked)
	{
		cbCell.firstChild.checked = checked;
	}
	
	this_.IsChecked = function(checked)
	{
		return cbCell.firstChild.checked;
	}
	
	var editing_ = false;
	var after_edit_callback_ = null;
	var text_before_edit_ = "";
	
	function stop_editing(cancel)
	{
		if(editing_)
		{
			var cell = nodeRow.cells[nodeRow.cells.length - 1].firstChild;
			var text = cell.firstChild.value;
			cell.firstChild.onblur = function() {};
			if(cancel != true)
			{
				try
				{
					if(after_edit_callback_ == undefined || after_edit_callback_(cell.firstChild.value, text_before_edit_))
					{
						cell.innerHTML = text;
					}
					else
					{
						cell.innerHTML = text_before_edit_;
					}
				}
				catch(ex)
				{
				}
			}
			else
			{
				cell.innerHTML = text_before_edit_;
				if(after_edit_callback_ != undefined && after_edit_callback_ != null) after_edit_callback_(null);
			}
			Core.Utility.ModifyCss(cell, "-editing");
			editing_ = false;
		}
	}
	
	function editor_oninput()
	{
	}
	
	function editor_onblur()
	{
		stop_editing();
	}
	
	function editor_onmousedown(e)
	{
		if(e == undefined) e = window.event;
		Core.Utility.CancelBubble(e);
	}
	
	function editor_onkeydown(e)
	{
		if(e == undefined) e = window.event;
		if(e.keyCode == 13)
		{
			stop_editing();
		}
		else if(e.keyCode == 27)
		{
			stop_editing(true);
		}
		Core.Utility.CancelBubble(e);
	}
	
	function editor_onclick(e)
	{
		if(e == undefined) e = window.event;
		Core.Utility.CancelBubble(e);
	}
	
	this_.Edit = function(after_edit)
	{
		if(!editing_)
		{
			var cell = nodeRow.cells[nodeRow.cells.length - 1].firstChild;
			var input_elem = document.createElement("INPUT");
			input_elem.type = "text";
			input_elem.className = "ct_treeview_nodetext_editor";
			input_elem.value = cell.innerHTML;
			Core.Utility.ModifyCss(cell, "editing");
			text_before_edit_ = cell.innerHTML;
			cell.innerHTML = "";
			cell.appendChild(input_elem);
			input_elem.onpropertychange = editor_oninput;
			input_elem.oninput = editor_oninput;
			input_elem.onblur = editor_onblur;
			input_elem.onmousedown = editor_onmousedown;
			input_elem.onclick = editor_onclick;
			input_elem.onkeydown = editor_onkeydown;
			try
			{
				input_elem.focus();
			}
			catch(ex)
			{
			}
			input_elem.select();
			editing_ = true;
			after_edit_callback_ = after_edit;
		}
	}
	
	this_.ResetLineCss = function()
	{
		var i = 0;
		for(;i < nodeRow.cells.length; i++)
		{
			var cell = nodeRow.cells[i];
			if(cell == buttonCell) break;			
		}
		var p = parentNode;
		while (p != null && (--i) >= 0)
		{
			var cell = nodeRow.cells[i];
			cell.innerHTML = String.format("<div class='{0}'></div>", p.IsLast() ? "line_none" : "line_ns");
			p = p.GetParent();
		}
		for(var node_i in subNodes)
		{
			subNodes[node_i].ResetLineCss();
		}
	}

	function GenerateBtnCss()
	{
		var css = "";
		if (!hasChildren) css += "line";
		else if (subNodes == null) css += "collapse";
		else if (subNodes.length == 0) css += "line";
		else if (isExpand) css += "expand";
		else css += "collapse";

		if (isLast) css += "_ne"
		else css += '_nes';

		return css;
	}

	var isExpand = false;

	var dom = document.createElement("DIV");
	dom.innerHTML = "<div><table cellpadding='0' cellspacing='0'><tr></tr></table></div><div></div>";
	dom.tabIndex = 1;
	dom.hideFocus = true;
	dom.style.outline = "none";
	
	dom.firstChild.className = "ct_treenode";
	
	var nodeContainer = dom.childNodes[0];
	nodeContainer.style.padding = '0px';
	nodeContainer.style.margin = '0px';
	var nodeRow = nodeContainer.firstChild.rows[0];
	var subNodesContainer = dom.childNodes[1];
	subNodesContainer.style.display = 'none';

	var p = parentNode;

	while (p != null)
	{
		var cell = nodeRow.insertCell(0);
		cell.innerHTML = String.format("<div class='{0}'></div>", p.IsLast() ? "line_none" : "line_ns");
		p = p.GetParent();
	}

	var buttonCell = nodeRow.insertCell(-1);
	buttonCell.innerHTML = String.format("<div class='{0}'></div>", GenerateBtnCss());
	var buttonDiv = buttonCell.firstChild;
	buttonDiv.onmousedown = function()
	{
		dom.focus();
		if (this_.IsExpand()) this_.Collapse(); else this_.Expand(function(result, error) { if (!result) Core.Utility.ShowError(error) });
	}
	buttonDiv.onclick = function()
	{
	}

	var cbCell = null;

	if (hasCheckBox)
	{
		cbCell = nodeRow.insertCell(-1);
		cbCell.innerHTML = "<input type='checkbox' />";
	}

	var imgCell = nodeRow.insertCell(-1);
	if (imagePath == undefined) imgCell.innerHTML = String.format("<div class='nodeImage {0}'></div>", imgSrc);
	else imgCell.innerHTML = String.format("<img class='nodeImage' src='{0}'/>", imagePath);

	this_.SetImage = function(src, css)
	{
		if (src == null) imgCell.innerHTML = String.format("<div class='nodeImage {0}'></div>", css);
		else imgCell.innerHTML = String.format("<img class='nodeImage' src='{0}'/>", src);
	}

	var textCell = nodeRow.insertCell(-1);
	textCell.innerHTML = String.format("<div class='nodeText'>{0}</div>", Core.Utility.FilterHtml(text));
	var textDiv = textCell.firstChild;

	imgCell.firstChild.onclick = function()
	{
		treeView.OnClick.Call(this_);
	}

	textDiv.onclick = function()
	{
		treeView.OnClick.Call(this_);
	}

	imgCell.firstChild.onmousedown = function()
	{
		dom.focus();
		treeView.Private.Select(function() { }, this_);
	}
	
	if(treeView.GetConfig().MouseUpSelectMode == true)
	{
		dom.firstChild.onmouseup = function(evt)
		{
			if(evt == undefined) evt = window.event;
			if(Core.Utility.GetButton(evt) == "Right")
			{
				dom.focus();
				treeView.Private.Select(function() { }, this_);
			}
		}
	}

	dom.firstChild.onmousedown = function(evt)
	{
		dom.focus();
		treeView.Private.Select(function() { }, this_);
	}

	dom.firstChild.ondblclick = function()
	{
		treeView.OnDblClick.Call(this_);
	}

	imgCell.firstChild.ondblclick = function()
	{
		treeView.OnDblClick.Call(this_);
	}

	container.appendChild(dom);
}