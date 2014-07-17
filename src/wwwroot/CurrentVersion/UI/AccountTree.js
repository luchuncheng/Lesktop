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
	if (config.ShowFrequentContactAndDept == undefined) config.ShowFrequentContactAndDept = true;
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
		
		if (config.ShowFrequentContactAndDept && Core.Session.GetUserInfo().SubType == 1)
		{
			for(var i in category_data.Categories)
			{
				var c = category_data.Categories[i];
				
				if(c.ParentID == 0 && (config.CategoryType == undefined || config.CategoryType == c.Type))
				{
					nodes.push(
						{
							Name: String.format("CATEGORY[{0}]", c.ID),
							Text: c.Name,
							Tag: {
								Type: "Category",
								Data: c,
								IsFCNode: true
							},
							HasChildren: true,
							ImageCss: "Image16_Folder",
							HasCheckBox: config.ShowCategoryCheckBox
						}
					);
				}
			}
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
		
		if(cu.SubType == 1 && !cu.IsTemp && config.ShowCompany)
		{
			nodes.push(
				{
					Name: "COMPANY",
					Text: category_data.CompanyInfo.Name,
					Tag: {
						Type: "Dept",
						Data: category_data.CompanyInfo,
						IsFCNode: false
					},
					HasChildren: true,
					ImageCss: "Image16_Folder",
					HasCheckBox: config.ShowRootDeptCheckBox
				}
			);
		}

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

	function CategoryNodeHandler(result, category_data, callback, node)
	{
		CurrentWindow.Completed();
		if(!result)
		{
			Core.Utility.ShowError(dept_data.toString());
			callback([]);
			return;
		}
		
		var category = node.GetTag().Data;
		var nodes = [];
		if (config.ShowUser)
		{
			for (var i in category_data.CategoryItems)
			{
				var ci = category_data.CategoryItems[i];
				if(ci.CategoryType == 1)
				{
					var user = category_data.Users[ci.ItemID];
					if(user != undefined && ci.CategoryID == category.ID && user.Type == 0)
					{
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
				}
			}
		}
		if (config.ShowGroup)
		{
			for (var i in category_data.CategoryItems)
			{
				var ci = category_data.CategoryItems[i];
				if(ci.CategoryType == 2)
				{
					var group = category_data.Users[ci.ItemID];
					if(group != undefined && ci.CategoryID == category.ID && group.Type == 1)
					{
						nodes.push(
							{
								Name: group.ID.toString(),
								Text: group.Nickname,
								Tag: {
									Type: "Group",
									Data: group,
									IsFCNode: true
								},
								HasChildren: false,
								ImageSrc: Core.CreateGroupImgUrl(group.ID, group.IsTemp),
								HasCheckBox: config.ShowGroupCheckBox
							}
						);
					}
				}
			}
		}
		
		if(config.ShowDept)
		{
			for (var i in category_data.CategoryItems)
			{
				var ci = category_data.CategoryItems[i];
				if(ci.CategoryType == 3)
				{
					var dept = category_data.Depts[ci.ItemID];
					if(dept != undefined && ci.CategoryID == category.ID)
					{
						nodes.push(
							{
								Name: dept.ID.toString(),
								Text: dept.Name,
								Tag: {
									Type: "Dept",
									Data: dept,
									IsFCNode: true
								},
								HasChildren: true,
								ImageCss: "Image16_Folder",
								HasCheckBox: config.ShowDeptCheckBox
							}
						);
					}
				}
			}
		}
		
		for(var i in category_data.Categories)
		{
			var c = category_data.Categories[i];
			if(c.ParentID == category.ID)
			{
				nodes.push(
					{
						Name: String.format("CATEGORY[{0}]", c.ID),
						Text: c.Name,
						Tag: {
							Type: "Category",
							Data: c,
							IsFCNode: true
						},
						HasChildren: true,
						ImageCss: "Image16_Folder",
						HasCheckBox: config.ShowCategoryCheckBox
					}
				);
			}
		}
		callback(nodes);
	}
	
	function AddDeptSubNodes(result, dept_data, callback, node)
	{
		CurrentWindow.Completed();
		if(!result)
		{
			Core.Utility.ShowError(dept_data.toString());
			callback([]);
			return;
		}
		
		var nodes = [];
		var dept_id = node.GetTag().Data.ID;
		for (var k in dept_data.SubDepts)
		{
			var dept = dept_data.SubDepts[k];
			nodes.push(
				{
					Name: dept.ID.toString(),
					Text: dept.Name,
					Tag: {
						Type: "Dept",
						Data: dept,
						IsFCNode: false
					},
					HasChildren: true,
					ImageCss: "Image16_Folder",
					HasCheckBox: config.ShowDeptCheckBox
				}
			);
		}
		
		if (config.ShowUser)
		{
			for (var k in dept_data.Items)
			{
				var user = dept_data.Items[k];
				if(user.Type == 0)
				{
					var gred = user.State == undefined || user.State == "Offline";
					nodes.push(
						{
							Name: user.ID.toString(),
							Text: user.Nickname,
							Tag: {
								Type: "User",
								Data: user,
								IsFCNode: false
							},
							HasChildren: false,
							ImageSrc:  Core.CreateHeadImgUrl(user.ID, 16, config.ShowUserState ? gred: false, user.HeadIMG),
							HasCheckBox: config.ShowUserCheckBox
						}
					);
				}
			}
		}
		
		if(config.ShowGroup)
		{
			for (var k in dept_data.Items)
			{
				var group = dept_data.Items[k];
				if(group.Type == 1)
				{
					nodes.push(
						{
							Name: group.ID.toString(),
							Text: group.Nickname,
							Tag: {
								Type: "Group",
								Data: group,
								IsFCNode: false
							},
							HasChildren: false,
							ImageSrc: Core.CreateGroupImgUrl(group.ID, group.IsTemp),
							HasCheckBox: config.ShowGroupCheckBox
						}
					);
				}
			}
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
		else if(node.GetTag().Type == "Category")
		{
			CurrentWindow.Waiting("正在获取联系人列表...");
			Core.CategoryData.Fetch(CategoryNodeHandler, callback, node);
		}
		else if(node.GetTag().Type == "Dept")
		{
			CurrentWindow.Waiting("正在获取联系人列表...");
			Core.AccountData.GetDeptData(AddDeptSubNodes, node.GetTag().Data.ID, callback, node);
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
			if (tag.Type == "Category")
			{
				node.SetTag(node_config.Tag);
				node.SetText(node_config.Tag.Data.Name);
				return true;
			}
			else if (tag.Type == "Dept")
			{
				node.SetTag(node_config.Tag);
				node.SetText(node_config.Tag.Data.Name);
				return true;
			}
		}
		return false;
	}
	
	this_.RefreshDeptNode = function(dept_id, dept_info)
	{
		var all_nodes = [];
		this_.GetAllNodes(all_nodes);
		for(var i in all_nodes)
		{
			var node = all_nodes[i];
			if (node.GetTag().Type == "Dept" && (dept_id == 0 || node.GetTag().Data.ID == dept_id))
			{
				if(node.GetTag().Data.ID == dept_id && dept_info != undefined)
				{
					node.SetText(dept_info.Name);
					node.GetTag().Data = dept_info;
				}
				if(node.HasRefresh())
				{
					node.Refresh(EmptyCallback, dept_id);
				}
			}
		}
	}
	
	this_.GetAllCategories = function()
	{
		return Core.CategoryData.GetCategories();
	}
	
	this_.FindCategoryNode = function(category_id)
	{
		var root_nodes = this_.GetRootNodes();
		var category_node = null;
		if (root_nodes != null)
		{
			var get_all_nodes_cb = function(node)
			{
				if (node.GetTag().Type == "Category" && node.GetTag().Data.ID == category_id)
				{
					category_node = node;
				}
			}
			for (var i in root_nodes)
			{
				var rn = root_nodes[i];
				if (rn.GetTag().Type == "Category")
				{
					rn.GetAllNodes(get_all_nodes_cb);
				}
			}
		}
		return category_node;
	}
	
	function ResetCategoryData(category_items, new_user_info, new_dept_info, categories)
	{
		if (category_items != undefined && category_items != null) Core.CategoryData.ResetCategoryItems(category_items);
		if (new_user_info != undefined && new_user_info != null) Core.CategoryData.AddUserInfo(new_user_info);
		if (new_dept_info != undefined && new_dept_info != null) Core.CategoryData.AddDeptInfo(new_dept_info);
		if (categories != undefined && categories != null) Core.CategoryData.ResetCategories(categories);
	}
	
	this_.RefreshCategoryNode = function(category_id, category_items, new_user_info, new_dept_info, categories)
	{
		ResetCategoryData(category_items, new_user_info, new_dept_info, categories);
	
		var node = this_.FindCategoryNode(category_id);
		if (node != null)
		{
			node.Refresh(function() { node.Expand(EmptyCallback) });
		}
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
	
	function OnCategoryDataChanged(reason, data)
	{
		if(reason == "ResetCategories")
		{
			for (var i in data.NewCategorys) this_.RefreshCategoryNode(data.NewCategorys[i]);
			for (var i in data.PreCategories) this_.RefreshCategoryNode(data.PreCategories[i].ID);
		}
		else if(reason == "AddItemsToCategory")
		{
			this_.RefreshCategoryNode(data.Category.ID);
		}
		else if(reason == "CategoryInfoChanged" || reason == "CreateCategory" || reason == "DeleteCategory")
		{
			this_.RefreshCategoryNode(data.Category.ParentID);
		}
		else if(reason == "RemoveCategoryItem")
		{
			this_.RefreshCategoryNode(data.Category.ID);
		}
	}
	Core.CategoryData.OnDataChanged.Attach(OnCategoryDataChanged);
	
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
		else if(reason == "DeptDataChanged")
		{
			this_.RefreshDeptNode(data.DeptID, data.DeptInfo);
		}
	}
	Core.AccountData.OnDataChanged.Attach(OnAccountDataChanged);
	
	CurrentWindow.OnClosed.Attach(
		function()
		{
			Core.CategoryData.OnDataChanged.Detach(OnCategoryDataChanged);
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
	
	var m_NodeTypeToDesc = {
		"1": "联系人",
		"2": "群组",
		"3": "部门"
	};
	
	var CreateCategoryMenu = function(node)
	{
		var tag = node.GetTag();
		var menu_config = null;
		if(tag.Data.ParentID != 0)
		{
			menu_config = {
				Items: [
					{
						Text: String.format("添加{0}", m_NodeTypeToDesc[tag.Data.Type.toString()]),
						ID: "AddItemsToCategory" + tag.Data.Type.toString()
					},
					{
						Text: "重命名分组",
						ID: "RenameCategory"
					},
					{
						Text: "新建分组",
						ID: "CreateCategory"
					},
					{
						Text: "删除分组",
						ID: "DeleteCategory"
					}
				],
				OwnerForm: CurrentWindow
			};
		}
		else
		{
			menu_config = {
				Items: [
					{
						Text: String.format("添加{0}", m_NodeTypeToDesc[tag.Data.Type.toString()]),
						ID: "AddItemsToCategory" + tag.Data.Type.toString()
					},
					{
						Text: "新建分组",
						ID: "CreateCategory"
					}
				],
				OwnerForm: CurrentWindow
			};
		}
		var menu = Core.CreateMenu(menu_config);
		if(menu != null) menu.OnCommand.Attach(Menu_Command);
		return menu;
	}
	
	var m_RootCategoryMenus = {};
	var m_CategoryMenus = {};

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

	//常用联系人右键菜单
	var m_DeleteFrequentContactMenuConfig = {
		Items: [
			{
				Text: "删除常用联系人",
				ID: "RemoveFromCategory"
			}
		],
		OwnerForm: CurrentWindow
	};
	var m_DeleteFrequentContactMenu = Core.CreateMenu(m_DeleteFrequentContactMenuConfig);
	if (m_DeleteFrequentContactMenu != null) m_DeleteFrequentContactMenu.OnCommand.Attach(Menu_Command);

	//常用部门右键菜单
	var m_DeleteFrequentDeptMenuConfig = {
		Items: [
			{
				Text: "删除常用部门",
				ID: "RemoveFromCategory"
			}
		],
		OwnerForm: CurrentWindow
	};	
	var m_DeleteFrequentDeptMenu = Core.CreateMenu(m_DeleteFrequentDeptMenuConfig);
	if (m_DeleteFrequentDeptMenu != null) m_DeleteFrequentDeptMenu.OnCommand.Attach(Menu_Command);

	//组织架构右键菜单
	var m_RefreshRootDeptMenuConfig = {
		Items: [
			{
				Text: "刷新组织架构",
				ID: "RefreshRootDept"
			}
		],
		OwnerForm: CurrentWindow
	};	
	var m_RefreshRootDeptMenu = Core.CreateMenu(m_RefreshRootDeptMenuConfig);
	if (m_RefreshRootDeptMenu != null) m_RefreshRootDeptMenu.OnCommand.Attach(Menu_Command);

	function CreateAddToCategoryMenu(type)
	{
		var text = "";
		if (type == 1) text = "添加到常用联系人";
		else if (type == 2) text = "添加到常用群组";
		else if (type == 3) text = "添加到常用部门";

		var fc_menu_config = {
			Items: [{
				ID: "ResetCategories",
				Text: text,
				Data: {
					Type: type
				}
			}],
			OwnerForm: CurrentWindow
		};
		var menu = Core.CreateMenu(fc_menu_config);
		menu.OnCommand.Attach(Menu_Command);
		return menu;
	}
	
	var m_AddToCategoryMenus = [
		null, 
		CreateAddToCategoryMenu(1),
		CreateAddToCategoryMenu(2),
		CreateAddToCategoryMenu(3)
	];
	
	function OnContextMenu(evt)
	{
		if (!config.PopupContextMenu) return;
		
		if (evt == undefined) evt = window.event;
		if (Core.Utility.GetButton(evt) == "Right")
		{
			var node = this_.HitTest(evt.clientX, evt.clientY);
			if(node == null) return;

			var current_user = Core.Session.GetUserInfo();
			
			var menu = null;
			if (node.GetTag().Type == "Category" && node.GetTag().Data.ParentID == 0)
			{
				var menu_id = node.GetTag().Data.Type.toString();
				if(m_RootCategoryMenus[menu_id] == undefined) m_RootCategoryMenus[menu_id] = CreateCategoryMenu(node);
				menu = m_RootCategoryMenus[menu_id];
			}
			else if (node.GetTag().Type == "Category")
			{
				var menu_id = node.GetTag().Data.Type.toString();
				if(m_CategoryMenus[menu_id] == undefined) m_CategoryMenus[menu_id] = CreateCategoryMenu(node);
				menu = m_CategoryMenus[menu_id];
			}
			else if (node.GetTag().Type == "Group" && node.GetTag().Data.IsTemp)
			{
				menu = m_TempGroupMenu;
			}
			else if (current_user.SubType == 1 && node.GetTag().Type == "User" && node.GetTag().IsFCNode == false)
			{
				if (m_AddToCategoryMenus[1] == null) m_AddToCategoryMenus[1] = CreateAddToCategoryMenu(1);
				menu = m_AddToCategoryMenus[1];
			}
			else if (current_user.SubType == 1 && node.GetTag().Type == "Group" && node.GetTag().IsFCNode == false)
			{
				if (m_AddToCategoryMenus[2] == null) m_AddToCategoryMenus[2] = CreateAddToCategoryMenu(2);
				menu = m_AddToCategoryMenus[2];
			}
			else if (current_user.SubType == 1 && node.GetTag().Type == "Dept" && node.GetTag().IsFCNode == false && node.GetTag().Data.ID != 1)
			{
				if (m_AddToCategoryMenus[3] == null) m_AddToCategoryMenus[3] = CreateAddToCategoryMenu(3);
				menu = m_AddToCategoryMenus[3];
			}
			else if (node.GetTag().Type == "User" && node.GetTag().IsFCNode == true)
			{
				menu = m_DeleteFrequentContactMenu;
			}
			else if (node.GetTag().Type == "Group" && node.GetTag().IsFCNode == true)
			{
				menu = m_DeleteFrequentContactMenu;
			}
			else if (node.GetTag().Type == "Dept" && node.GetTag().IsFCNode == true && node.GetTag().Data.ID != 1)
			{
				menu = m_DeleteFrequentDeptMenu;
			}
			else if (node.GetTag().Type == "Dept" && node.GetTag().Data.ID == 1)
			{
				menu = m_RefreshRootDeptMenu;
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
	
	MenuCommandHandler["RenameCategory"] = function()
	{
		var context_menu_node = m_ContextMenuNode;
		context_menu_node.Edit(
			function(newtext, oldtext)
			{
				if (newtext != null && newtext != "")
				{
					CurrentWindow.Waiting("正在重命名分组...");
					var data = {
						Action: "RenameCategory",
						Name: newtext,
						ID: context_menu_node.GetTag().Data.ID
					};
					Core.SendCommand(
						function(ret)
						{
							CurrentWindow.Completed();
							ResetCategoryData(null, null, null, ret.Categories);
							Core.CategoryData.FireDataChangedEvent("CategoryInfoChanged", { Category: context_menu_node.GetTag().Data });
						},
						function(ex)
						{
							CurrentWindow.Completed();
							Core.Utility.ShowError(ex);
							context_menu_node.SetText(oldtext);
						},
						Core.Utility.RenderJson(data), "Core.Web Category_CH", false
					);
					return true;
				}
				else
				{
					return false;
				}
			}
		);
	}
	
	MenuCommandHandler["CreateCategory"] = function()
	{
		var context_menu_node = m_ContextMenuNode;
		var rcns = this_.GetRootCategoryNodes();
		var parent_node = context_menu_node;
		if (parent_node == undefined) return;

		var nodeconfig = {
			Name: "TEMPNODE",
			Text: "新分组",
			ImageCss: "Image16_Folder",
			HasChildren: true,
			Tag: {
				Type: "NULL"
			}
		};
		parent_node.Expand(
			function()
			{
				var node = parent_node.AddSubNode(nodeconfig, -1);
				if (node != null)
				{
					node.Edit(
						function(newtext)
						{
							if (newtext != null && newtext != "")
							{
								CurrentWindow.Waiting("正在创建分组...");
								var data = {
									Action: "CreateCategory",
									Name: newtext,
									ParentID: parent_node.GetTag().Data.ID
								};
								Core.SendCommand(
									function(ret)
									{
										CurrentWindow.Completed();
										ResetCategoryData(null, null, null, ret.Categories);
										Core.CategoryData.FireDataChangedEvent("CreateCategory", { Category: ret.Category });
										//m_AddToCategoryMenus[ret.Category.Type] = null;
									},
									function(ex)
									{
										CurrentWindow.Completed();
										parent_node.RemoveSubNode(node);
										Core.Utility.ShowError(ex);
									},
									Core.Utility.RenderJson(data), "Core.Web Category_CH", false
								);
								return true;
							}
							else
							{
								parent_node.RemoveSubNode(node);
							}
						}
					);
				}
			}
		);
	}
	
	MenuCommandHandler["DeleteCategory"] = function()
	{
		var context_menu_node = m_ContextMenuNode;
		if (!confirm(String.format("您确定要删除分组\"{0}\"?", context_menu_node.GetTag().Data.Name))) return;

		CurrentWindow.Waiting("正在删除分组...");
		var type = context_menu_node.GetTag().Data.Type;
		var parent_id = context_menu_node.GetTag().Data.ParentID;
		var data = {
			Action: "DeleteCategory",
			ID: context_menu_node.GetTag().Data.ID
		};
		Core.SendCommand(
			function(ret)
			{
				CurrentWindow.Completed();
				ResetCategoryData(null, null, null, ret.Categories);
				Core.CategoryData.FireDataChangedEvent("DeleteCategory", { Category: context_menu_node.GetTag().Data });
			},
			function(ex)
			{
				CurrentWindow.Completed();
				Core.Utility.ShowError(ex);
			},
			Core.Utility.RenderJson(data), "Core.Web Category_CH", false
		);
	}
	
	MenuCommandHandler["AddToCategory"] = function()
	{
		CurrentWindow.Waiting("正在处理...");
		var tag = context_menu_node.GetTag();

		var data = {
			Action: "AddToCategory",
			Type: m_NodeType[tag.Type],
			ItemID: (tag.Type == "Dept" ? tag.Data.ID: tag.Data.ID),
			CategoryID: cmddata.ID
		};
		Core.SendCommand(
			function(ret)
			{
				this_.RefreshCategoryNode(cmddata.ID, ret.CategoryItems, ret.User, ret.Dept);
				CurrentWindow.Completed();
			},
			function(ex)
			{
				CurrentWindow.Completed();
				Core.Utility.ShowError(ex);
			},
			Core.Utility.RenderJson(data), "Core.Web Category_CH", false
		);
	}    
	
	MenuCommandHandler["RemoveFromCategory"] = function()
	{
		var context_menu_node = m_ContextMenuNode;
		CurrentWindow.Waiting("正在处理...");
		var tag = context_menu_node.GetTag();
		var category_id = context_menu_node.GetParent().GetTag().Data.ID;

		var data = {
			Action: "RemoveFromCategory",
			Type: m_NodeType[tag.Type],
			ItemID: (tag.Type == "Dept" ? tag.Data.ID: tag.Data.ID),
			CategoryID: category_id
		};
		Core.SendCommand(
			function(ret)
			{
				ResetCategoryData(ret.CategoryItems);
				Core.CategoryData.FireDataChangedEvent("RemoveCategoryItem", {Category: context_menu_node.GetParent().GetTag().Data});
				CurrentWindow.Completed();
			},
			function(ex)
			{
				CurrentWindow.Completed();
				Core.Utility.ShowError(ex);
			},
			Core.Utility.RenderJson(data), "Core.Web Category_CH", false
		);
	}
	
	MenuCommandHandler["ResetCategories"] = function(cmddata)
	{
		var context_menu_node = m_ContextMenuNode;
		var node_tag = context_menu_node.GetTag();
		var config = {
			Left: 0,
			Top: 0,
			Width: Math.min(270, CurrentWindow.GetWidth() - 30),
			Height: Math.min(300, CurrentWindow.GetHeight() - 30),
			MinWidth: Math.min(270, CurrentWindow.GetWidth() - 30),
			MinHeight: Math.min(300, CurrentWindow.GetHeight() - 30),
			Title: {
				InnerHTML: String.format("请选择\"{0}\"所属分组...", node_tag.Data.Nickname)
			},
			Resizable: false,
			HasMaxButton: false,
			HasMinButton: false,
			AnchorStyle: Core.WindowAnchorStyle.Left | Core.WindowAnchorStyle.Top
		};

		var form = Core.CreateWindow(config);
		var tag = {
			Type: cmddata.Type,
			ItemID: (cmddata.Type == 3 ? node_tag.Data.ID: node_tag.Data.ID)
		};
		form.SetTag(tag);
		form.MoveEx('CENTER', 0, 0, true);
		form.ShowDialog(
			CurrentWindow, 'CENTER', 0, 0, true,
			function()
			{
				if (form.GetTag().IDS != undefined)
				{
					CurrentWindow.Waiting("正在处理...");
					var data = {
						Action: "ResetCategories",
						Type: m_NodeType[node_tag.Type],
						ItemID: (node_tag.Type == "Dept" ? node_tag.Data.ID: node_tag.Data.ID),
						Categorys: form.GetTag().IDS.join(',')
					};
					Core.SendCommand(
						function(ret)
						{
							ResetCategoryData(ret.CategoryItems, ret.User, ret.Dept);
							Core.CategoryData.FireDataChangedEvent("ResetCategories", {PreCategories: ret.PreCategories, NewCategorys: form.GetTag().IDS});
							CurrentWindow.Completed();
						},
						function(ex)
						{
							CurrentWindow.Completed();
							Core.Utility.ShowError(ex);
						},
						Core.Utility.RenderJson(data), "Core.Web Category_CH", false
					);
				}
			}
		);
		var url = Core.GetUrl("SelectCategoriesForm.htm");
		form.Load(url, null);
	}

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
	
	MenuCommandHandler["RefreshRootDept"] = function(cmddata)
	{
		var context_menu_node = m_ContextMenuNode;
		Core.AccountData.ClearDeptData(0);
		Core.AccountData.FireDataChangedEvent("DeptDataChanged", { DeptID: 0 });
	}
	
	MenuCommandHandler["AddItemsToCategory1"] = function()
	{
		var context_menu_node = m_ContextMenuNode;
		var config = {
			Left: 0,
			Top: 0,
			Width: 290,
			Height: 350,
			MinWidth: 290,
			MinHeight: 350,
			Title: {
				InnerHTML: String.format(
					"请选择要添加到分组\"{0}\"的{1}...", 
					context_menu_node.GetTag().Data.Name, 
					context_menu_node.GetTag().Data.Type == 1 ? "人员" : "群组"
				)
			},
			Resizable: false,
			HasMaxButton: false,
			HasMinButton: false,
			AnchorStyle: Core.WindowAnchorStyle.Left | Core.WindowAnchorStyle.Top
		};

		var form = Core.CreateWindow(config);
		var form_tag = {
			IDS: "",
			DSConfig: {
				ShowUser: context_menu_node.GetTag().Data.Type == 1,
				ShowGroup: context_menu_node.GetTag().Data.Type == 2,
				ShowUserCheckBox: true,
				ShowGroupCheckBox: true,
				ShowUserState: true,
				ShowFrequentContactAndDept: false,
				ShowCommFriends: false
			}
		};
		form.SetTag(form_tag);
		form.MoveEx('CENTER', 0, 0, true);
		form.ShowDialog(
			CurrentWindow, 'CENTER', 0, 0, true,
			function()
			{
				if (form.GetTag().IDS != "")
				{
					var data = {
						Action: "AddItemsToCategory",
						Items: form.GetTag().IDS,
						CategoryID: context_menu_node.GetTag().Data.ID
					};
					CurrentWindow.Waiting("正在添加...");
					Core.SendCommand(
					function(ret)
					{
						CurrentWindow.Completed();
						Core.CategoryData.ResetCategoryItems(ret.CategoryItems);
						for(var i in form.GetTag().Users)
						{
							Core.CategoryData.AddUserInfo(form.GetTag().Users[i]);
						}
						Core.CategoryData.FireDataChangedEvent("AddItemsToCategory", {Category: context_menu_node.GetTag().Data});
					},
					function(ex)
					{
						CurrentWindow.Completed();
						Core.Utility.ShowError(ex);
					},
					Core.Utility.RenderJson(data), "Core.Web Category_CH", false);
				}
			}
		);
		var url = Core.GetUrl("SelectUsersForm.htm");
		form.Load(url, null);
	}
	
	MenuCommandHandler["AddItemsToCategory2"] = MenuCommandHandler["AddItemsToCategory1"];
	
	MenuCommandHandler["AddItemsToCategory3"] = function()
	{
		var context_menu_node = m_ContextMenuNode;
		var config = {
			Left: 0, Top: 0,
			Width: 290,
			Height: 350,
			MinWidth: 290,
			MinHeight: 350,
			Title: {
				InnerHTML: String.format("请选择要添加到分组\"{0}\"的部门...", context_menu_node.GetTag().Data.Name)
			},
			Resizable: false,
			HasMaxButton: false,
			HasMinButton: false,
			AnchorStyle: Core.WindowAnchorStyle.Left | Core.WindowAnchorStyle.Top
		}

		var form = Core.CreateWindow(config);
		var form_tag = {
			IDS: "",
			DSConfig: {
				ShowGroup: false,
				ShowUser: false,
				ShowFrequentContactAndDept: false,
				ShowCommFriends: false,
				ShowDeptCheckBox: true,
				ShowRootDeptCheckBox: false
			}
		};
		form.SetTag(form_tag);
		form.MoveEx('CENTER', 0, 0, true);
		form.ShowDialog(
			CurrentWindow, 'CENTER', 0, 0, true, 
			function()
			{
				if (form_tag.IDS != "")
				{
					var data = {
						Action: "AddItemsToCategory",
						Items: form_tag.IDS,
						CategoryID: context_menu_node.GetTag().Data.ID
					};
					CurrentWindow.Waiting("正在添加...");
					Core.SendCommand(
					function(ret)
					{
						CurrentWindow.Completed();
						Core.CategoryData.ResetCategoryItems(ret.CategoryItems);
						for(var i in form.GetTag().Depts)
						{
							Core.CategoryData.AddDeptInfo(form.GetTag().Depts[i]);
						}
						Core.CategoryData.FireDataChangedEvent("AddItemsToCategory", {Category: context_menu_node.GetTag().Data});
					},
					function(ex)
					{
						CurrentWindow.Completed();
						Core.Utility.ShowError(ex);
					},
					Core.Utility.RenderJson(data), "Core.Web Category_CH", false);
				}
			}
		);
		var url = Core.GetUrl("SelectDeptForm.htm");
		form.Load(url, null);
	}
}

})();