(function() {

if(window.Core == undefined) window.Core = {};
if(window.Core.UI == undefined) window.Core.UI = {};

function EmptyCallback()
{
}

function CatetoryTreeDataSource(config)
{
	var obj = this;
	
	if (config == undefined) config = {};
	if (config.ShowUser == undefined) config.ShowUser = true;
	if (config.ShowGroup == undefined) config.ShowGroup = true;
	if (config.ShowDept == undefined) config.ShowDept = true;
	if (config.ShowAllUsersNode == undefined) config.ShowAllUsersNode = false;
	if (config.ShowUserCheckBox == undefined) config.ShowUserCheckBox = false;
	if (config.ShowGroupCheckBox == undefined) config.ShowGroupCheckBox = false;
	if (config.ShowDeptCheckBox == undefined) config.ShowDeptCheckBox = false;
	if (config.ShowRootDeptCheckBox == undefined) config.ShowRootDeptCheckBox = config.ShowDeptCheckBox;
	if (config.ShowUserState == undefined) config.ShowUserState = false;
	if (config.ShowTempGroup == undefined) config.ShowTempGroup = false;
	if (config.ShowCompany == undefined)  config.ShowCompany = true;
	if (config.ShowCommFriends == undefined) config.ShowCommFriends = true;
	if (config.ShowCategoryCheckBox == undefined) config.ShowCategoryCheckBox = false;
	if (config.ShowCSUsers == undefined) config.ShowCSUsers = false;
	
	var has_expand_company_node_ = false;
	
	obj.HasExpandCompanyNode = function()
	{
		return has_expand_company_node_;
	}

	function RootNodeHandler(result, category_data, callback, node)
	{
		CurrentWindow.Completed();
		if(!result)
		{
			Core.Utility.ShowError(dept_data.toString());
			callback([]);
			return;
		}
		
		var nodes = [];
		
		if (config.ShowAllUsersNode)
		{
			nodes.push(
				{
					Name: "ROOT_ALL",
					Text: "所有人员",
					Tag: {
						Type: "All"
					},
					HasChildren: false,
					ImageCss: "Image16_Group"
				}
			);
		}
		if(Core.PluginsUtil.ExistPlugin("Comm") &&  config.ShowCommFriends)
		{
			nodes.push(
				{
					Name: "COMMFRIENDS",
					Text: "我的好友",
					Tag: {
						Type: "CommFriends",
						IsFCNode: false
					},
					HasChildren: true,
					ImageCss: "Image16_Folder",
					HasCheckBox: false
				}
			);
			nodes.push(
				{
					Name: "COMMGROUPS",
					Text: "我的群组",
					Tag: {
						Type: "CommGroups",
						IsFCNode: false
					},
					HasChildren: true,
					ImageCss: "Image16_Folder",
					HasCheckBox: false
				}
			);
		}

		if (config.ShowTempGroup)
		{
			nodes.push(
				{
					Name: "TEMPGROUP",
					Text: "多人会话",
					Tag: {
						Type: "TempGroups"
					},
					HasChildren: true,
					ImageCss: "Image16_Folder"
				}
			);
		}
		
		var cu = Core.Session.GetUserInfo();

		if(config.ShowCSUsers && Core.Session.GetGlobal("CSConfig") != null)
		{
			var csconfig =  Core.Session.GetGlobal("CSConfig");
			nodes.push(
				{
					Name: "CUSTOMSERVICE",
					Text: csconfig.CompanyInfo.Name,
					Tag: {
						Type: "CustomService",
						IsFCNode: false
					},
					HasChildren: true,
					ImageCss: "Image16_Folder",
					HasCheckBox: false
				}
			);
		}
		callback(nodes);
	}
		
	function AddCommFriendNodes(result, data, callback, node)
	{
		CurrentWindow.Completed();
		if(!result)
		{
			Core.Utility.ShowError(dept_data.toString());
			callback([]);
			return;
		}
		
		var cu = Core.Session.GetUserInfo();
		
		var comm_friends = data.All();
		var nodes = [];	
		if(config.ShowUser && node.GetTag().Type == "CommFriends")
		{
			for (var i in comm_friends)
			{
				var friend = comm_friends[i];
				if(friend.Type == 0 && (cu.SubType == 0 || (cu.SubType == 1 && friend.SubType == 0)))
				{
					var gred = (friend.State == undefined || friend.State == "Offline");
					nodes.push(
						{
							Name: friend.ID.toString(),
							Text: friend.Nickname,
							Tag: {
								Type: "User",
								Data: friend,
								IsFCNode: false
							},
							HasChildren: false,
							ImageSrc: Core.CreateHeadImgUrl(friend.ID, 16, config.ShowUserState ? gred: false, friend.HeadIMG),
							HasCheckBox: config.ShowUserCheckBox
						}
					);
				}
			}
		}
		if(config.ShowGroup && node.GetTag().Type == "CommGroups")
		{
			for (var i in comm_friends)
			{
				var friend = comm_friends[i];
				if(friend.Type == 1 && (cu.SubType == 0 || (cu.SubType == 1 && friend.SubType == 0)))
				{
					nodes.push(
						{
							Name: friend.ID.toString(),
							Text: friend.Nickname,
							Tag: {
								Type: "Group",
								Data: friend,
								IsFCNode: false
							},
							HasChildren: false,
							ImageSrc: Core.CreateGroupImgUrl(friend.ID, friend.IsTemp),
							HasCheckBox: config.ShowGroupCheckBox
						}
					);
				}
			}
		}
		callback(nodes);
	}
	
	function AddTempGroupNodes(result, temp_groups, callback, node)
	{
		CurrentWindow.Completed();
		if(!result)
		{
			Core.Utility.ShowError(arguments[1].toString());
			callback([]);
			return;
		}
		var nodes = [];	
		if(config.ShowGroup)
		{
			for (var k in temp_groups)
			{
				var group = temp_groups[k];
				if (group.Type == 1 && group.IsTemp)
				{
					nodes.push(
						{
							Name: group.ID.toString(),
							Text: group.Nickname,
							Tag: {
								Type: "Group",
								Data: group
							},
							HasChildren: false,
							ImageSrc: Core.CreateGroupImgUrl(group.ID, group.IsTemp)
						}
					);
				}
			}
		}
		callback(nodes);
	}

	function AddCSDepts(callback, node)
	{
		var csconfig = Core.Session.GetGlobal("CSConfig");
		if(csconfig != null)
		{
			var nodes = [];		
			for(var csdeptid in csconfig.Depts)
			{
				var csdept = csconfig.Depts[csdeptid];
				nodes.push(
					{
						Name: csdeptid.toString(),
						Text: csdept.Name,
						Tag: {
							Type: "CSDept",
							Data: csdept,
							IsFCNode: false
						},
						HasChildren: true,
						ImageCss: "Image16_Folder",
						HasCheckBox: false
					}
				);
			}
			callback(nodes);
		}
	}

	function AddCSUsers(callback, node)
	{
		var csdept = node.GetTag().Data;
		var csconfig = Core.Session.GetGlobal("CSConfig");
		if(csdept != null && csconfig != null)
		{
			var nodes = [];
			for(var uid in csdept.Users)
			{
				var user = csdept.Users[uid];
				var gred = user.State == undefined || user.State == "Offline";
				nodes.push(
					{
						Name: user.ID.toString(),
						Text: user.Nickname,
						Tag: {
							Type: "User",
							Data: user,
							IsFCNode: true
						},
						HasChildren: false,
						ImageSrc: Core.CreateHeadImgUrl(user.ID, 16, config.ShowUserState ? gred: false, user.HeadIMG),
						HasCheckBox: config.ShowUserCheckBox
					}
				);
			}
			callback(nodes);
		}
	}
	
	obj.GetSubNodes = function(callback, node)
	{
		if(config.CustomGetSubNodes != undefined)
		{
			var pre_callback = callback;
			callback = function(nodes)
			{
				config.CustomGetSubNodes(pre_callback, node, nodes);
			}
		}
		if(node == null)
		{
			CurrentWindow.Waiting("正在获取联系人列表...");
			Core.CategoryData.Fetch(RootNodeHandler, callback, node);
		}
		else if(node.GetTag().Type == "CommFriends" || node.GetTag().Type == "CommGroups")
		{
			CurrentWindow.Waiting("正在获取联系人列表...");
			Core.AccountData.GetCommFriends(AddCommFriendNodes, callback, node);
		}
		else if(node.GetTag().Type == "TempGroups")
		{
			CurrentWindow.Waiting("");
			Core.AccountData.GetTempGroups(AddTempGroupNodes, callback, node);
		}
		else if(node.GetTag().Type == "CustomService")
		{
			AddCSDepts(callback, node);
		}
		else if(node.GetTag().Type == "CSDept")
		{
			AddCSUsers(callback, node);
		}
		else
		{
			callback([]);
		}
	}
}


Core.UI.AccountTree = function(container, config)
{
	var this_ = this;
	var OwnerForm = this;
	
	if (config.ExpandDeptRoot == undefined) config.ExpandDeptRoot = false;
	if (config.PopupContextMenu == undefined) config.PopupContextMenu = false;
	if (config.PopupContextMenu) config.MouseUpSelectMode = true;
	
	config.DataSource = new CatetoryTreeDataSource(config.DSConfig);

	Core.UI.TreeView.call(this_, container, config);

	this_.BeforeCreateNode = function(node, node_config, parent_node, bcn_data)
	{
		var tag = node.GetTag();
		if(tag != null)
		{
			if (tag.Type == "Dept")
			{
				node.SetTag(node_config.Tag);
				node.SetText(node_config.Tag.Data.Name);
				return true;
			}
		}
		return false;
	}
	
	this_.RefreshTempGroups = function()
	{
		var root_nodes = this_.GetRootNodes();
		if (root_nodes != null)
		{
			for (var i in root_nodes)
			{
				var rn = root_nodes[i];
				if (rn.GetTag().Type == "TempGroups")
				{
					rn.Refresh(EmptyCallback);
				}
			}
		}
	}
	
	this_.RefreshCommNodes = function()
	{
		var root_nodes = this_.GetRootNodes();
		if (root_nodes != null)
		{
			for (var i in root_nodes)
			{
				var rn = root_nodes[i];
				if (rn.GetTag().Type == "CommFriends" || rn.GetTag().Type == "CommGroups")
				{
					rn.Refresh(EmptyCallback);
				}
			}
		}
	}
	
	this_.GetRootCategoryNodes = function()
	{
		var nodes = {};
		var root_nodes = this_.GetRootNodes();
		for (var i in root_nodes)
		{
			var rn = root_nodes[i];
			if (rn.GetTag().Type == "Category")
			{
				nodes[rn.GetTag().Data.ID] = rn;
			}
		}
		return nodes;
	}
	
	this_.RefreshTree = function(callback, expand_company)
	{
		this_.Refresh(
			function()
			{
				try
				{
					var root_nodes = this_.GetRootNodes();
					for (var i in root_nodes)
					{
						var rn = root_nodes[i];
						if(expand_company && rn.GetTag().Type == "Dept" && rn.GetTag().Data.ID == 1
							|| rn.GetTag().Type == "Category"|| rn.GetTag().Type == "CommFriends"|| rn.GetTag().Type == "CommGroups")
						{
							rn.Expand(EmptyCallback);
						}
					}
				}
				finally
				{
					//
				}
				if (callback != undefined) callback();
			}
		);
	}
	
	function OnAccountDataChanged(reason, data)
	{
		if(reason == "AddTempGroup" || reason == "RemoveTempGroup" || reason == "UpdateTempGroup")
		{
			this_.RefreshTempGroups();
		}
		else if(reason == "AddCommFriend" || reason == "RemoveCommFriend")
		{
			this_.RefreshCommNodes();
		}
	}
	Core.AccountData.OnDataChanged.Attach(OnAccountDataChanged);
	
	CurrentWindow.OnClosed.Attach(
		function()
		{
			Core.AccountData.OnDataChanged.Detach(OnAccountDataChanged);
		}
	);
	
	CurrentWindow.OnNotify.Attach(
		function(command, data)
		{
			if (command == "AccountInfoChanged")
			{
				this_.GetAllNodes(
					function(node)
					{
						if (node.GetTag().Type == "User" && node.GetTag().Data.ID == data.Details.ID)
						{
							node.GetTag().Data = data.Details;
							node.SetText(data.Details.Nickname);
						}
					}
				);
			}
		}
	);
	
	if (config.DSConfig.ShowUserState)
	{
		CurrentWindow.OnNotify.Attach(
		function(command, data)
		{
			if (command == "UserStateChanged")
			{
				this_.GetAllNodes(
					function(node)
					{
						if (node.GetTag().Type == "User" && node.GetTag().Data.ID == data.User)
						{
							var user_data = node.GetTag().Data;						
							node.SetImage(Core.CreateHeadImgUrl(user_data.ID, 16, data.State == "Offline", user_data.HeadIMG));
						}
					}
				);
			}
		});
	};
	
	var m_ContextMenuNode = null;
	
	var MenuCommandHandler = {
	};
	
	function Menu_Command(command, data)
	{
		var handler = MenuCommandHandler[command];
		if(handler != undefined) handler(data);
	}

	//多人回话右键菜单
	var m_TempGroupMenuConfig = {
		Items: [
			{
				Text: "退出多人会话",
				ID: "ExitTempGroup"
			}
		],
		OwnerForm: CurrentWindow
	};
	var m_TempGroupMenu = Core.CreateMenu(m_TempGroupMenuConfig);
	if (m_TempGroupMenu != null) m_TempGroupMenu.OnCommand.Attach(Menu_Command);
	
	function OnContextMenu(evt)
	{
		if (!config.PopupContextMenu) return;
		
		if (evt == undefined) evt = window.event;
		if (Core.Utility.GetButton(evt) == "Right")
		{
			var node = this_.HitTest(evt.clientX, evt.clientY);
			if(node == null) return;
			
			var menu = null;
			if (node.GetTag().Type == "Group" && node.GetTag().Data.IsTemp)
			{
				menu = m_TempGroupMenu;
			}
			
			if(menu != null)
			{
				var clientCoord = CurrentWindow.GetClientCoord(evt.clientX, evt.clientY);
				menu.Popup(clientCoord.X, clientCoord.Y);
				m_ContextMenuNode = node;
				Core.Utility.PreventDefault(evt);
				Core.Utility.CancelBubble(evt);
			}
		}
	}
	Core.Utility.AttachEvent(this_.GetDom(), "mouseup", OnContextMenu);
	
	var m_NodeType = {
		"User": 1,
		"Group": 2,
		"Dept": 3
	};

	MenuCommandHandler["ExitTempGroup"] = function(cmddata)
	{
		var context_menu_node = m_ContextMenuNode;
		
		if (!confirm("您确定要退出多人会话？")) return;

		CurrentWindow.Waiting("正在退出多人会话...");
		var data = {
			Action: "ExitTempGroup",
			GroupID: context_menu_node.GetTag().Data.ID
		};
		Core.SendCommand(
			function(ret)
			{
				CurrentWindow.Completed();
			},
			function(ex)
			{
				CurrentWindow.Completed();
				Core.Utility.ShowError(ex);
			},
			Core.Utility.RenderJson(data), "Core.Web Common_CH", false
		);
	}
}

})();