
var AccountTree = null;
var Plugins = null;
var PluginsCommandHandlers = {};

function init()
{
	Plugins = Core.PluginsUtil.GetPlugins("MainForm");
	Core.UI.PagePanel.Create('');
	var page_ctrl = new Core.UI.Control(
		Core.UI.PagePanel,
		{
			Padding: 1,
			Css: "panel",
			Controls: [
				{
					ID: "toolbar_ctrl",
					Css: "toolbar_ctrl",
					Height: 24,
					Resizable: false,
					BorderWidth: 0,
					Margin: [1, 0, 1, 0], 
					DockStyle: Core.UI.DockStyle.Top
				},
				{
					ID: "acctree_ctrl",
					Css: "acctree_ctrl ct_treeview",
					DockStyle: Core.UI.DockStyle.Fill
				}
			]
		}
	);
	
	var toolbar_config = {
		Items: [
			{
				ID: "MainMenu",
				Type: "Button",
				Css: "image_text_button mainmenu_btn",
				Config: { 
					Text: "主菜单", TabIndex: 1,
					Menu: Core.CreateMainMenu(CurrentWindow)
				}
			}
		]
	};
	
	if(!Core.Session.GetUserInfo().IsTemp && (Core.PluginsUtil.ExistPlugin("EIM") || Core.PluginsUtil.ExistPlugin("Comm") || Core.PluginsUtil.ExistPlugin("EmbedCS")))
	{
		var tempgroupmenu_config = {
			ID: "CreateTempGroup",
			Type: "Button",
			Css: "image_text_button tempgroup_btn",
			Config: { 
				Text: "多人会话"
			}
		};
		toolbar_config.Items.push(tempgroupmenu_config);
	}
	
	try
	{
		for (var i in Plugins)
		{
			var plugin = Plugins[i];
			var imc = plugin.InjectModules["MainForm"];
			if (imc.UserFormToolbarExtent != undefined)
			{
				for (var j in imc.UserFormToolbarExtent)
				{
					try
					{
						var eb = imc.UserFormToolbarExtent[j];
						if ((Core.Session.GetUserInfo().IsAdmin || !eb.AdminPermission) && 
							(!Core.Session.GetUserInfo().IsTemp || me.AllowTempUser != false))
						{
							var tb_config = {
								ID: plugin.ID + ":" + eb.Command,
								Type: "Button",
								Css: "image_text_button " + eb.Css,
								Config: { 
									Text: eb.Text
								}
							};
							toolbar_config.Items.push(tb_config);
							PluginsCommandHandlers[tb_config.ID] = eb.OnCommand;
						}
					}
					catch(ex)
					{
					}
				}
			}
		}
	}
	catch(ex)
	{		    
	}
	
	PluginsCommandHandlers["CreateTempGroup"] = function()
	{
		Core.Session.GetGlobal("SingletonForm").ShowCreateTempGroupForm();
	}
	
	var toolbar_ctrl = page_ctrl.FindControl("toolbar_ctrl");	
	var toolbar = new Core.UI.Toolbar(toolbar_ctrl.GetContainer(), toolbar_config);
	toolbar.OnCommand.Attach(
		function(cmd)
		{
			if(PluginsCommandHandlers[cmd] != undefined)
			{
				PluginsCommandHandlers[cmd]();
			}
		}
	);
	
	var acctree_ctrl = page_ctrl.FindControl("acctree_ctrl");
	var acctree_config = {
		PopupContextMenu: true
	};
	if(Core.Session.GetUserInfo().IsTemp)
	{
		acctree_config.DSConfig = {
			ShowUserState: true,
			ShowTempGroup: false,
			ShowCompany: false,
			ShowCommFriends: false,
			ShowCSUsers: true
		}
	}
	else
	{
		acctree_config.DSConfig = {
			ShowUserState: true,
			ShowTempGroup: true
		}
	}
	AccountTree = new Core.UI.AccountTree(acctree_ctrl.GetContainer(), acctree_config);    
	AccountTree.OnDblClick.Attach(
		function()
		{
			var node = AccountTree.GetSelectedNode();
			if(node != null)
			{
				var tag = node.GetTag();
				if (tag.Type == "User" && tag.Data.ID != Core.Session.GetUserInfo().ID)
				{
					var form = Core.Session.GetGlobal("ChatService").Open(tag.Data.ID, false);
				}
				else if (tag.Type == "Group")
				{
					var form = Core.Session.GetGlobal("ChatService").Open(tag.Data.ID, false);
				}
			}
		}
	);
	if (ClientMode)
	{
		Core.Utility.AttachEvent(
			AccountTree.GetDom(), "mousedown",
			function(evt)
			{
				if (evt == undefined) evt = window.event;
				var node = AccountTree.HitTest(evt.clientX, evt.clientY);
				if (Core.Utility.GetButton(evt) == "Left")
				{
					if (node != null && node.GetTag().Type == "User")
					{
						var data = Core.Utility.RenderJson(
							{
								ID: node.GetTag().Data.ID,
								Name: node.GetTag().Data.Name
							}
						);
						CurrentWindow.DoDragDrop("User", data);
					}
				}
			}
		);
	}

	function ExpandCSNode()
	{
		AccountTree.Expand(
			function(node)
			{
				if(node != null)
				{
					node.GetSubNodes(
						function(subnodes)
						{
							for(var i in subnodes)
							{
								subnodes[i].Expand(Core.EmptyCallback);
							}
						}
					);
				}
			},
			"/CUSTOMSERVICE"
		);
	}

	AccountTree.RefreshTree(
		function()
		{
			if(Core.Session.GetUserInfo().IsTemp) ExpandCSNode();
		}
	);
}