var depttree_ = null;

function EmptyCallback()
{
}

function init()
{
	Core.UI.PagePanel.Create('');
	var page_ctrl = new Core.UI.Control(
		Core.UI.PagePanel,
		{
			Padding: 1,
			Css: "panel",
			Controls: [
				{
					ID: "dept_tree",
					Css: "dept_tree ct_treeview",
					Width: 220,
					MinWidth: 220,
					MaxWidth: 400,
					Resizable: true,
					SplitterSize: 5,
					BorderWidth: 0,
					DockStyle: Core.UI.DockStyle.Left
				},
				{
					ID: "tabctrl",
					Css: "tabctrl ct_tab",
					Type: Core.UI.TabControl,
					Tabs: [
						{ 
							ID: "TAB0", MinWidth: 100, Text: "部门资料", IsSelected: true,
							Controls: [
								{ ID: "frame_container", Css: "frame_container", Margin: 6, BorderWidth: 1, DockStyle: Core.UI.DockStyle.Fill }
							]
						},
						{ 
							ID: "TAB1", MinWidth: 100, Text: "子部门管理",
							Controls: [
								{ ID: "frame_container", Css: "frame_container", Margin: 6, BorderWidth: 1, DockStyle: Core.UI.DockStyle.Fill }
							]
						},
						{ 
							ID: "TAB2", MinWidth: 100, Text: "部门员工管理",
							Controls: [
								{ ID: "frame_container", Css: "frame_container", Margin: 6, BorderWidth: 1, DockStyle: Core.UI.DockStyle.Fill }
							]
						},
						{ 
							ID: "TAB3", MinWidth: 100, Text: "部门群组管理",
							Controls: [
								{ ID: "frame_container", Css: "frame_container", Margin: 6, BorderWidth: 1, DockStyle: Core.UI.DockStyle.Fill }
							]
						},
						{ 
							ID: "TAB4", MinWidth: 100, Text: "群组资料",
							Controls: [
								{ ID: "frame_container", Css: "frame_container", Margin: 6, BorderWidth: 1, DockStyle: Core.UI.DockStyle.Fill }
							]
						},
						{ 
							ID: "TAB5", MinWidth: 100, Text: "群组成员", 
							Controls: [
								{ ID: "frame_container", Css: "frame_container", Margin: 6, BorderWidth: 1, DockStyle: Core.UI.DockStyle.Fill }
							]
						},
						{ 
							ID: "TAB6", MinWidth: 100, Text: "员工资料",
							Controls: [
								{ ID: "frame_container", Css: "frame_container", Margin: 6, BorderWidth: 1, DockStyle: Core.UI.DockStyle.Fill }
							]
						}
					],
					DockStyle: Core.UI.DockStyle.Fill
				}
			]
		}
	);
	
	var dept_tree_ctrl = page_ctrl.FindControl("dept_tree");
	var tabctrl = page_ctrl.FindControl("tabctrl");
	var itemid_ = 1;
	var itemtype_ = "Dept";
	var frames_ = {};
	
	function Refresh()
	{
		CurrentWindow.CompleteAll();
	
		var current_tab = tabctrl.GetCurrentTab();
		
		if (itemtype_ == "Dept")
		{
			tabctrl.SetTabVisible("TAB0", true);
			tabctrl.SetTabVisible("TAB1", true);
			tabctrl.SetTabVisible("TAB2", true);
			tabctrl.SetTabVisible("TAB3", true);
			tabctrl.SetTabVisible("TAB4", false);
			tabctrl.SetTabVisible("TAB5", false);
			tabctrl.SetTabVisible("TAB6", false);
	
			if (itemid_ == 1)
			{
				tabctrl.SetTabTitle("TAB0", "公司名称");
				tabctrl.SetTabTitle("TAB1", "部门管理");
				tabctrl.SetTabTitle("TAB2", "员工管理");
				tabctrl.SetTabTitle("TAB3", "群组管理");
			}
			else
			{
				tabctrl.SetTabTitle("TAB0", "部门资料");
				tabctrl.SetTabTitle("TAB1", "子部门管理");
				tabctrl.SetTabTitle("TAB2", "员工管理");
				tabctrl.SetTabTitle("TAB3", "群组管理");
			}
	
			if (current_tab == "TAB0")
			{
				CurrentWindow.Waiting("正在载入部门列表...");
				frames_[current_tab].src = String.format("Management/UpdateDeptInfo.aspx?DeptID={1}&random={0}", (new Date()).getTime(), itemid_);

			}
			else if (current_tab == "TAB1")
			{
				CurrentWindow.Waiting("正在载入部门列表...");
				frames_[current_tab].src = String.format("Management/AllDepts.aspx?DeptID={1}&random={0}", (new Date()).getTime(), itemid_);

			}
			else if (current_tab == "TAB2")
			{
				CurrentWindow.Waiting("正在载入用户列表...");
			   frames_[current_tab].src = String.format("Management/AllUSers.aspx?DeptID={1}&random={0}", (new Date()).getTime(), itemid_);

			}
			else if (current_tab == "TAB3")
			{
				CurrentWindow.Waiting("正在载入群列表...");
				frames_[current_tab].src = String.format("Management/AllGroups.aspx?DeptID={1}&random={0}", (new Date()).getTime(), itemid_);

			}
			else
			{
				tabctrl.Select("TAB0");
			}
		}
		else if (itemtype_ == "Group")
		{
			tabctrl.SetTabVisible("TAB0", false);
			tabctrl.SetTabVisible("TAB1", false);
			tabctrl.SetTabVisible("TAB2", false);
			tabctrl.SetTabVisible("TAB3", false);
			tabctrl.SetTabVisible("TAB4", true);
			tabctrl.SetTabVisible("TAB5", true);
			tabctrl.SetTabVisible("TAB6", false);
			if (current_tab == "TAB4")
			{
				CurrentWindow.Waiting("正在载入群组资料...");
				frames_[current_tab].src = String.format("Management/UpdateGroupInfo.aspx?ID={1}&random={0}", (new Date()).getTime(), itemid_);
			}
			else if (current_tab == "TAB5")
			{
				CurrentWindow.Waiting("正在载入群组成员列表...");
				frames_[current_tab].src = String.format("Management/GroupMembers.aspx?GroupID={1}&random={0}", (new Date()).getTime(), itemid_);
			}
			else
			{
				tabctrl.Select("TAB4");
			}
		}
		else if (itemtype_ == "User")
		{
			tabctrl.SetTabVisible("TAB0", false);
			tabctrl.SetTabVisible("TAB1", false);
			tabctrl.SetTabVisible("TAB2", false);
			tabctrl.SetTabVisible("TAB3", false);
			tabctrl.SetTabVisible("TAB4", false);
			tabctrl.SetTabVisible("TAB5", false);
			tabctrl.SetTabVisible("TAB6", true);
			if (current_tab == "TAB6")
			{
				CurrentWindow.Waiting("正在载入员工资料...");
				frames_[current_tab].src = String.format("Management/UpdateUserInfo.aspx?ID={1}&random={0}", (new Date()).getTime(), itemid_);
			}
			else
			{
				tabctrl.Select("TAB6");
			}
		}
	}
	
	
	var depttree_config = {
		ExpandDeptRoot: true,
		DSConfig: {
			ShowUser: true,
			ShowFrequentContactAndDept: false,
			ShowCommFriends: false
		}
	};
	depttree_ = new Core.UI.AccountTree(dept_tree_ctrl.GetContainer(), depttree_config);   
	depttree_.RefreshTree(
		function()
		{
			depttree_.Select(EmptyCallback, "/COMPANY");
		},
		true
	);
	
	depttree_.OnClick.Attach(
		function()
		{
			itemtype_ = depttree_.GetSelectedNode().GetTag().Type;
			itemid_ = depttree_.GetSelectedNode().GetTag().Data.ID;
			Refresh();
		}
	);
	
	tabctrl.OnSelectedTab.Attach(
		function(cur, pre)
		{
			Refresh();
		}
	);
	
	for(var i = 0; i <= 6; i++)
	{
		var tid = "TAB" + i.toString();
		var tab_panel = tabctrl.GetPanel(tid);
		var frame_container = tab_panel.FindControl("frame_container");
		var frame = Core.UI.CreateFrame(frame_container.GetContainer());
		frames_[tid] = frame;
	}
	
	Refresh();
}