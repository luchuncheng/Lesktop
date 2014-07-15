(function() {

function IsChatRoom(info)
{
	return info.Type == 1 && info.SubType == 3;
}

var UserInfoHtmlFmt = 
"<img class='userinfo_image' src='{0}'/>" +
"<br/>" +
"<table class='userinfo_table' cellspacing='0' cellpadding='0'>" +
"<tr><td class='userinfo_table_left'>姓名：</td><td class='userinfo_table_right'>{1}</td></tr>" +
"<tr><td class='userinfo_table_left'>邮箱：</td><td class='userinfo_table_right'><a href='mailto:{2}' title='{2}'>{2}</a></td></tr>" +
"<tr><td class='userinfo_table_left'>电话：</td><td class='userinfo_table_right'>{3}</td></tr>" +
"<tr><td class='userinfo_table_left'>手机：</td><td class='userinfo_table_right'>{4}</td></tr>" +
"</table>";

Core.UI.UserInfoPanel = function(parent, config)
{
	var this_ = this;
	var user_ = null;
	
	Core.UI.Control.call(this, parent, config);
	
	var container = this_.GetContainer();
	
	this_.Refresh = function(user)
	{
		user_ = user;
		container.innerHTML = String.format(
			UserInfoHtmlFmt,
			Core.CreateHeadImgUrl(user_.ID, 150, user_.State == "Offline", user_.HeadIMG),
			user_.Nickname, user_.EMail, user_.Tel, user_.Mobile
		);
	}
	
	this_.RefreshState = function(state)
	{
		user_.State = state;
		container.firstChild.src = Core.CreateHeadImgUrl(user_.ID, 150, user_.State == "Offline", user_.HeadIMG);
	}
	
	this_.Refresh(config.User);
}

Core.UI.UserImageCtrl = function(parent, config)
{
	var this_ = this;
	var user_ = config.User;
	
	Core.UI.Control.call(this, parent, config);
	
	var container = this_.GetContainer();
	container.innerHTML = String.format(
		"<img class='userinfo_image' src='{0}'/>",
		Core.CreateHeadImgUrl(user_.ID, 150, user_.State == "Offline", user_.HeadIMG)
	);
	
	this_.Refresh = function(user)
	{
		user_ = user;
		container.firstChild.src = Core.CreateHeadImgUrl(user_.ID, 150, user_.State == "Offline", user_.HeadIMG);
	}
}

Core.UI.UserChatPanel = function(config)
{    
	var this_ = this;
	var tabctrl_ = null;
	var userinfopanel_ = null;
	var userimagectrl_ = null;
	var chatpanel_ = null;
	
	function OnNotify(command, data)
	{
		if (command == "UserStateChanged")
		{
			if(data.User == config.Peer.ID)
			{
				userinfopanel_.RefreshState(data.State);
				this_.MarkLeaveStatus(data.State == "Leave");
			}
		}
		else if (command == "AccountInfoChanged")
		{
			if(data.Details.ID == config.Peer.ID)
			{
				userinfopanel_.Refresh(data.Details);
			}
			if(data.Details.ID == config.User.ID)
			{
				userimagectrl_.Refresh(data.Details);
			}
		}
	}
	
	CurrentWindow.OnNotify.Attach(OnNotify);
	
	CurrentWindow.OnClosed.Attach(
		function()
		{
			CurrentWindow.OnNotify.Detach(OnNotify);
		}
	);
	
	this_.MarkLeaveStatus = function(leave)
	{
		if (leave == undefined) leave = true;
		if (leave)
		{
			chatpanel_.ShowLeaveTip(true);
		}
		else
		{
			chatpanel_.ShowLeaveTip(false);
		}
	}
	
	this_.ShowChatTab = function()
	{
		tabctrl_.Select("TAB0");
	}
	
	this_.AddMessage = function(msg)
	{
		chatpanel_.AddMessage(msg);
	}
	
	this_.Focus = function()
	{
		if(chatpanel_ != null) chatpanel_.Focus();
	}
	
	var ctrl = new Core.UI.Control(
		Core.UI.PagePanel,
		{
			Controls: [
				{
					ID: "rightpanel",
					Css: "",
					Width: 182,
					Margin: 1,
					DockStyle: Core.UI.DockStyle.Right,
					Controls: [
						{
							ID: "userimagectrl",
							Css: "userimagectrl",
							BorderWidth: 1,
							Height: 180,
							Margin: [4, 0, 0, 0],
							Type: Core.UI.UserImageCtrl,
							DockStyle: Core.UI.DockStyle.Bottom,
							User: config.User
						},
						{
							ID: "userinfopanel",
							Css: "userinfopanel",
							BorderWidth: 1,
							Type: Core.UI.UserInfoPanel,
							DockStyle: Core.UI.DockStyle.Fill,
							User: config.Peer
						}
					]
				},
				{
					ID: "tabctrl",
					Css: "ct_tab",
					Margin: [1, 4, 1, 1],
					Type: Core.UI.TabControl,
					DockStyle: Core.UI.DockStyle.Fill,
					Tabs: [
						{ 
							ID: "TAB0", MinWidth: 100, Text: "当前会话", IsSelected: true,
							Controls: [
								{
									ID: "chatpanel",
									Css: "chatpanel",
									Margin: [4, 6, 6, 6],
									Type: Core.UI.ChatPanel,
									DockStyle: Core.UI.DockStyle.Fill,
									User: config.User,
									Peer: config.Peer,
									Owner: this_
								}
							]
						}
					]
				}
			],
			DockStyle: Core.UI.DockStyle.Fill
		}
	);
	
	tabctrl_ = ctrl.FindControl("tabctrl");
	chatpanel_ = ctrl.FindControlRecursive("chatpanel");
	userimagectrl_ = ctrl.FindControlRecursive("userimagectrl");
	userinfopanel_ = ctrl.FindControlRecursive("userinfopanel");
}

function Match(reg, str)
{
	reg.lastIndex = 0;
	var ft = reg.exec(str);
	return (ft != null && ft.length == 1 && ft[0] == str)
}

function CheckEMail(email)
{
	return Match(/[a-zA-Z0-9._\-]+@[a-zA-Z0-9._\-]+/ig, email);
}

function CheckTel(tel)
{
	return Match(/[0-9\-]{6,30}/ig, tel);
}

function CheckMobile(mobile)
{
	return Match(/[0-9]{11,11}/ig, mobile);
}

function VisitorCommandPanel(parent_, config_)
{
	var this_ = this;

	Core.UI.Control.call(this_, parent_, config_);

	var fillpanel_ = new Core.UI.Control(
		this_,
		{
			ID: "fillpanel",
			DockStyle: Core.UI.DockStyle.Fill,
			Controls: [
				{
					Margin: [0, 0, 10, 0],
					Height: 34,
					DockStyle: Core.UI.DockStyle.Top,
					Controls: [
						{
							Css: "item_name",
							Width: 40,
							Content: "姓名：",
							DockStyle: Core.UI.DockStyle.Left
						},
						{
							ID: "name",
							Css: "item_textbox",
							Content: "<div><input type='text' class='item_textbox'/></div>",
							DockStyle: Core.UI.DockStyle.Fill
						}
					]
				},
				{
					Margin: [0, 0, 10, 0],
					Height: 34,
					DockStyle: Core.UI.DockStyle.Top,
					Controls: [
						{
							Css: "item_name",
							Width: 40,
							Content: "邮箱：",
							DockStyle: Core.UI.DockStyle.Left
						},
						{
							ID: "email",
							Css: "item_textbox",
							Content: "<div><input type='text' class='item_textbox'/></div>",
							DockStyle: Core.UI.DockStyle.Fill
						}
					]
				},
				{
					Margin: [0, 0, 10, 0],
					Height: 34,
					DockStyle: Core.UI.DockStyle.Top,
					Controls: [
						{
							Css: "item_name",
							Width: 40,
							Content: "电话：",
							DockStyle: Core.UI.DockStyle.Left
						},
						{
							ID: "tel",
							Css: "item_textbox",
							Content: "<div><input type='text'/></div>",
							DockStyle: Core.UI.DockStyle.Fill
						}
					]
				},
				{
					Margin: [4, 0, 8, 0],
					Height: 24,
					Content: "您的留言（不超过500字）：",
					DockStyle: Core.UI.DockStyle.Top
				},
				{
					Margin: [10, 0, 0, 0],
					Height: 36,
					DockStyle: Core.UI.DockStyle.Bottom,
					Controls: [
						{ ID: "btnok", Css: "ct_custom_default_button", Width: 84, DockStyle: Core.UI.DockStyle.Right }
					]
				},
				{
					ID: "content",
					Css: "item_textarea",
					Content: "<div><textarea type='text'></textarea></div>",
					DockStyle: Core.UI.DockStyle.Fill
				}
			]
		}
	);
	var btnok_ = new Core.UI.Button(fillpanel_.FindContainerRecursive("btnok"), { Text: "发送留言" });
	var edit_name_ = fillpanel_.FindContainerRecursive("name").firstChild.firstChild;
	var edit_email = fillpanel_.FindContainerRecursive("email").firstChild.firstChild;
	var edit_tel_ = fillpanel_.FindContainerRecursive("tel").firstChild.firstChild;
	var edit_content_ = fillpanel_.FindContainerRecursive("content").firstChild.firstChild;

	btnok_.OnClick.Attach(
		function(btn)
		{
			if (edit_name_.value == "")
			{
				Core.Utility.ShowWarning("姓名不能为空！");
				edit_name_.focus();
				return;
			}
			
			if (edit_name_.value.length > 16 || edit_name_.value.length < 2)
			{
				Core.Utility.ShowWarning("姓名格式错误！请输入2-16个字符");
				edit_name_.focus();
				return;
			}
			
			if (edit_email.value == "")
			{
				Core.Utility.ShowWarning("邮箱不能为空！");
				edit_email.focus();
				return;
			}
			
			if (edit_email.value.length > 80)
			{
				Core.Utility.ShowWarning("邮箱不能超过80字符！");
				edit_email.focus();
				return;
			}
			
			if (!CheckEMail(edit_email.value))
			{
				Core.Utility.ShowWarning("邮箱格式错误！");
				edit_email.focus();
				return;
			}
			
			if (edit_tel_.value == "")
			{
				Core.Utility.ShowWarning("电话不能为空！");
				edit_tel_.focus();
				return;
			}
			
			if (edit_tel_.length > 30 || edit_tel_.length < 6)
			{
				Core.Utility.ShowWarning("电话格式错误！请输入6-30位数字");
				edit_tel_.focus();
				return;
			}
			
			if (!CheckTel(edit_tel_.value))
			{
				Core.Utility.ShowWarning("电话格式错误！请输入6-30位数字");
				edit_tel_.focus();
				return;
			}
			
			if (edit_content_.value.length < 3)
			{
				Core.Utility.ShowWarning("留言内容至少需3个字符！");
				edit_content_.focus();
				return;
			}
			
			if (edit_content_.value.length > 500)
			{
				Core.Utility.ShowWarning("留言内容不能超过500字符！");
				edit_content_.focus();
				return;
			}
			
			var data = {
				Action: "NewComment",
				SenderID: Core.Session.GetUserID(),
				ReceiverID: config_.Peer.ID,
				Mail: edit_email.value,
				Tel: edit_tel_.value,
				Content: edit_content_.value,
				Name: edit_name_.value
			
			}
			
			CurrentWindow.Waiting("正在提交留言，请稍候...");
			
			Core.SendCommand(
				function(ret)
				{
					edit_email.value = "";
					edit_tel_.value = "";
					edit_content_.value = "";
					edit_name_.value = "";
					CurrentWindow.Completed();
				},
				function(ex)
				{
					CurrentWindow.Completed();
					Core.Utility.ShowError(ex.toString());
				},
				Core.Utility.RenderJson(data), "Core.Web Common_CH", false
			);
		}
	);
}

Core.UI.VisitorChatPanel = function(config)
{    
	var this_ = this;
	var tabctrl_ = null;
	var userinfopanel_ = null;
	var userimagectrl_ = null;
	var chatpanel_ = null;
	var msgmanager_ = null;
	
	function OnNotify(command, data)
	{
		if (command == "UserStateChanged")
		{
			if(data.User == config.Peer.ID)
			{
				userinfopanel_.RefreshState(data.State);
				this_.MarkLeaveStatus(data.State == "Leave");
			}
		}
		else if (command == "AccountInfoChanged")
		{
			if(data.Details.ID == config.Peer.ID)
			{
				userinfopanel_.Refresh(data.Details);
			}
			if(data.Details.ID == config.User.ID)
			{
				userimagectrl_.Refresh(data.Details);
			}
		}
	}
	
	CurrentWindow.OnNotify.Attach(OnNotify);
	
	CurrentWindow.OnClosed.Attach(
		function()
		{
			CurrentWindow.OnNotify.Detach(OnNotify);
		}
	);
	
	this_.MarkLeaveStatus = function(leave)
	{
		if (leave == undefined) leave = true;
		if (leave)
		{
			chatpanel_.ShowLeaveTip(true);
		}
		else
		{
			chatpanel_.ShowLeaveTip(false);
		}
	}
	
	this_.ShowChatTab = function()
	{
		tabctrl_.Select("TAB0");
	}
	
	this_.AddMessage = function(msg)
	{
		chatpanel_.AddMessage(msg);
	}
	
	this_.Focus = function()
	{
		if(chatpanel_ != null) chatpanel_.Focus();
	}

	var tbcmd_handlers_ = {
		"MsgRecord": function()
		{
			tabctrl_.Select("TAB2");
			msgmanager_.Select(config.Peer.ID);
		}
	};
	
	var ctrl = new Core.UI.Control(
		Core.UI.PagePanel,
		{
			Controls: [
				{
					ID: "rightpanel",
					Css: "",
					Width: 182,
					Margin: 1,
					DockStyle: Core.UI.DockStyle.Right,
					Controls: [
						{
							ID: "userinfopanel",
							Css: "userinfopanel",
							BorderWidth: 1,
							Type: Core.UI.UserInfoPanel,
							DockStyle: Core.UI.DockStyle.Fill,
							User: config.Peer
						}
					]
				},
				{
					ID: "tabctrl",
					Css: "ct_tab",
					Margin: [1, 4, 1, 1],
					Type: Core.UI.TabControl,
					DockStyle: Core.UI.DockStyle.Fill,
					Tabs: [
						{ 
							ID: "TAB0", MinWidth: 100, Text: "当前会话", IsSelected: true,
							Controls: [
								{
									ID: "chatpanel",
									Css: "chatpanel",
									Margin: [4, 6, 6, 6],
									Type: Core.UI.ChatPanel,
									DockStyle: Core.UI.DockStyle.Fill,
									ToolbarCommandHandlers: tbcmd_handlers_,
									User: config.User,
									Peer: config.Peer,
									Owner: this_
								}
							]
						},
						{ 
							ID: "TAB1", MinWidth: 100, Text: "留言", 
							Controls: [	
								{
									ID: "visitor_comment",
									Css: "visitor_comment",
									Margin: 12,
									Type: VisitorCommandPanel,
									DockStyle: Core.UI.DockStyle.Fill,
									User: config.User,
									Peer: config.Peer
								}
							]
						},
						{ 
							ID: "TAB2", MinWidth: 100, Text: "聊天记录", 
							Controls: [	
								{
									ID: "msgmanager",
									Css: "msgmanager",
									Margin: [4, 6, 6, 6],
									Type: Core.UI.MsgManager,
									DockStyle: Core.UI.DockStyle.Fill,
									Peer: config.Peer
								}
							]
						}
					]
				}
			],
			DockStyle: Core.UI.DockStyle.Fill
		}
	);
	
	tabctrl_ = ctrl.FindControl("tabctrl");
	chatpanel_ = ctrl.FindControlRecursive("chatpanel");
	userimagectrl_ = ctrl.FindControlRecursive("userimagectrl");
	userinfopanel_ = ctrl.FindControlRecursive("userinfopanel");
	msgmanager_ = ctrl.FindControlRecursive("msgmanager");

	tabctrl_.OnSelectedTab.Attach(
		function(tab)
		{
			if(tab == "TAB2")
			{
				msgmanager_.Select(config.Peer.ID);
			}
		}
	);

	if(config.Peer.State == "Offline")
	{
		tabctrl_.Select("TAB1");
	}
}

function GroupMemberTreeDS(config)
{
	var this_ = this;
	var members_ = null;
	var group_ = null;
	var creator_ = null;
	var managers_ = null;

	function GetSubNodes(callback, item)
	{
		if (item == null)
		{
			var nodes = [];
			if (!config.Group.IsTemp && config.Group.SubType == 0)
			{
				nodes.push(
					{
						Name: "Managers",
						IsExpand: true,
						Text: "群管理员",
						Tag: null,
						ImageCss: "membertree_icon"
					}
				);
			}
			if (config.Group.SubType == 3 || config.Group.IsTemp == 1)
			{
				nodes.push(
					{
						Name: "Members",
						IsExpand: true,
						Text: "参与人员",
						Tag: null,
						ImageCss: "membertree_icon"
					}
				);
			}
			else
			{
				nodes.push(
					{
						Name: "Members",
						IsExpand: true,
						Text: "群组成员",
						Tag: null,
						ImageCss: "membertree_icon"
					}
				);
			}
			callback(nodes);
		}
		else
		{
			var nodes = [];
			if (item.GetFullPath() == "/Managers")
			{
				for (var i in members_)
				{
					var member = members_[i];
					if (member.ID == creator_.ID)
					{
						var node = {
							Name: member.ID.toString(),
							Text: member.Nickname,
							Tag: member,
							HasChildren: false,
							ImageSrc: Core.CreateHeadImgUrl(member.ID, 16, member.State == "Offline", member.HeadIMG)
						};
						nodes.push(node);
					}
				}
			}
			else if (item.GetFullPath() == "/Members")
			{
				for (var i in members_)
				{
					var member = members_[i];
					if (member.ID != creator_.ID && (!IsChatRoom(config.Group) || member.State == "Online"))
					{
						var node = {
							Name: member.ID.toString(),
							Text: member.Nickname,
							Tag: member,
							HasChildren: false,
							ImageSrc: Core.CreateHeadImgUrl(member.ID, 16, member.State == "Offline", member.HeadIMG)
						};
						nodes.push(node);
					}
				}
			}
			callback(nodes);
		}
	}

	this_.ClearCache = function()
	{
		managers_ = null;
		members_ = null;
		group_ = null;
		creator_ = null;
	}

	this_.GetGroupMembers = function(callback)
	{
		if (members_ != null)
		{
			callback(true, members_, group_, creator_, managers_);
		}
		else
		{
			var data = {
				Action: "GetGroupMembers",
				ID: config.Group.ID
			};
			Core.SendCommand(
			function(ret)
			{
				members_ = ret.Members;
				group_ = ret.GroupInfo;
				creator_ = ret.GroupCreator;
				managers_ = ret.Managers;
				callback(true, members_, group_, creator_, managers_);
			},
			function(ex)
			{
				callback(false, ex);
			},
			Core.Utility.RenderJson(data), "Core.Web Common_CH", false);
		}
	}

	this_.GetSubNodes = function(callback, item)
	{
		if (members_ == null)
		{
			this_.GetGroupMembers(function(result, members, group_info, creator, managers)
			{
				if (result)
				{
					members_ = members;
					group_ = group_info;
					creator_ = creator;
					managers_ = managers;
					GetSubNodes(callback, item);
				}
				else
				{
					callback(null, arguments[1]);
				}
			});
		}
		else
		{
			GetSubNodes(callback, item);
		}
	}
}

Core.UI.GroupMemberTree = function(parent, config)
{
	var this_ = this;
	var group_ = config.Group;
	var treeview_ = null;
	var datasource_ = null;
	
	Core.UI.Control.call(this, parent, config);

	datasource_ = new GroupMemberTreeDS({ Group: config.Group });
	treeview_ = new Core.UI.TreeView(this_.GetContainer(), { DataSource: datasource_ });
	treeview_.Load(
		function()
		{
			treeview_.Expand(Core.EmptyCallback, "/Members");
			treeview_.Expand(Core.EmptyCallback, "/Managers");
		}
	);
	
	this_.RefreshMemberInfo = function(memberId, info)
	{
		treeview_.GetAllNodes(
			function(node)
			{
				if (node.GetTag() != null && node.GetTag().ID == memberId)
				{
					node.SetImage(Core.CreateHeadImgUrl(info.ID, 16, info.State == "Offline", info.HeadIMG));
					node.SetText(info.Nickname);
					node.SetTag(info);
				}
			}
		);
	}
	
	this_.RefreshMemberState = function(memberId, state)
	{
		treeview_.GetAllNodes(
			function(node)
			{
				if (node.GetTag() != null && node.GetTag().ID == memberId)
				{
					var member = node.GetTag();
					node.SetImage(Core.CreateHeadImgUrl(member.ID, 16, state == "Offline", member.HeadIMG));
				}
			}
		);
	}
	
	this_.RefreshMembers = function()
	{
		datasource_.ClearCache();
		treeview_.Load(
			function()
			{
				treeview_.Expand(Core.EmptyCallback, "/Members");
				treeview_.Expand(Core.EmptyCallback, "/Managers");
			}
		);
	}
}

Core.UI.GroupChatPanel = function(config)
{
	var this_ = this;
	var loadsharepage_ = false;
	var remarkctrl_ = null;
	var tabctrl_ = null;
	var chatpanel_ = null;
	var membertree_ = null;
	var sharepageframe_ = null;
	var sharetoolbar_ = null;
	
	this_.OnDispose = new Core.Delegate();
	
	function OnNotify(command, data)
	{
		if (command == "UserStateChanged")
		{
			membertree_.RefreshMemberState(data.User, data.State);
		}
		else if (command == "RemoveFromGroup")
		{
			if (data.GroupID == config.Peer.ID)
			{
				membertree_.RefreshMembers();
			}
		}
		else if (command == "AddToGroup")
		{
			if (data.GroupID == config.Peer.ID)
			{
				membertree_.RefreshMembers();
			}
		}
		else if (command == "ExitTempGroup")
		{
			if (data.GroupID == config.Peer.ID)
			{
				var dom = chatpanel_.AppendElement("DIV");
				dom.className = "message";
				dom.innerHTML = String.format("<div class='error'>\"{0}\"已退出多人会话！</div>", data.User.Nickname);
				membertree_.RefreshMembers();
			}
		}
		else if (command == "AddTempGroup")
		{
			if (data.GroupID == config.Peer.ID)
			{
				var html = [];
				for (var i in data.Users)
				{
					html.push(String.format("\"{0}\"已加入多人会话！", data.Users[i].Nickname));
				}
				var dom = chatpanel_.AppendElement("DIV");
				dom.className = "message";
				dom.innerHTML = String.format("<div class='error'>{0}</div>", html.join("<br/>"));
				membertree_.RefreshMembers();
			}
		}
		else if (command == "AccountInfoChanged")
		{
			if (data.Details.ID == config.Peer.ID)
			{
				remarkctrl_.childNodes[1].innerHTML = Core.Utility.FilterHtml(config.Peer.Remark);
				CurrentWindow.SetTitle(String.format("{0}({1})", data.Details.Nickname, data.Details.Name));
				membertree_.RefreshMembers();
			}
			else
			{
				membertree_.RefreshMemberInfo(data.Details.ID, data.Details);
			}
		}
	}
	
	CurrentWindow.OnNotify.Attach(OnNotify);
	
	CurrentWindow.OnClosed.Attach(
		function()
		{
			CurrentWindow.OnNotify.Detach(OnNotify);
		}
	);
	
	this_.ShowChatTab = function()
	{
		tabctrl_.Select("TAB0");
	}
	
	this_.AddMessage = function(msg)
	{
		chatpanel_.AddMessage(msg);
	}
	
	this_.Focus = function()
	{
		if(chatpanel_ != null) chatpanel_.Focus();
	}
	
	var ctrl = new Core.UI.Control(
		Core.UI.PagePanel,
		{
			Controls: [
				{
					ID: "rightpanel",
					Css: "",
					Width: 182,
					Margin: 1,
					DockStyle: Core.UI.DockStyle.Right,
					Controls: [
						{
							ID: "remarkctrl",
							Css: "remarkctrl",
							BorderWidth: 1,
							Height: 145,
							Visible: !config.Peer.IsTemp,
							Resizable: true,
							SplitterSize: 3,
							Content: "<div class='header'>公告</div><div class='remark'></div>",
							DockStyle: Core.UI.DockStyle.Top
						},
						{
							ID: "membertreecontainer",
							Css: "membertreecontainer",
							BorderWidth: 1,
							DockStyle: Core.UI.DockStyle.Fill,
							Controls: [
								{
									ID: "membertreeheader",
									Css: "membertreeheader",
									Height: 20,
									Content: "群成员",
									DockStyle: Core.UI.DockStyle.Top
								},
								{
									ID: "membertree",
									Css: "membertree ct_treeview noborder",
									Type: Core.UI.GroupMemberTree,
									DockStyle: Core.UI.DockStyle.Fill,
									Group: config.Peer
								}
							]
						}
					]
				},
				{
					ID: "tabctrl",
					Css: "ct_tab",
					Margin: [1, 4, 1, 1],
					Type: Core.UI.TabControl,
					DockStyle: Core.UI.DockStyle.Fill,
					Tabs: [
						{ 
							ID: "TAB0", MinWidth: 100, Text: "当前会话", IsSelected: true,
							Controls: [
								{
									ID: "chatpanel",
									Css: "chatpanel",
									Margin: [4, 6, 6, 6],
									Type: Core.UI.ChatPanel,
									DockStyle: Core.UI.DockStyle.Fill,
									User: config.User,
									Peer: config.Peer,
									Owner: this_
								}
							]
						},
						{ 
							ID: "TAB1", MinWidth: 100, Text: "群共享", 
							Controls: [
								{ ID: "sharetoolbar", Css: "ct_toolbar", Margin: [4, 6, 2, 6], Height: 28, DockStyle: Core.UI.DockStyle.Top },
								{ ID: "sharepageframe", Css: "frame_container", Margin: [0, 6, 6, 6], BorderWidth: 1, DockStyle: Core.UI.DockStyle.Fill }
							]
						}
					]
				}
			],
			DockStyle: Core.UI.DockStyle.Fill
		}
	);
	
	tabctrl_ = ctrl.FindControl("tabctrl");
	chatpanel_ = ctrl.FindControlRecursive("chatpanel");
	membertree_ = ctrl.FindControlRecursive("membertree");
	remarkctrl_ = ctrl.FindContainerRecursive("remarkctrl");
	remarkctrl_.childNodes[1].innerHTML = Core.Utility.FilterHtml(config.Peer.Remark);
	
	sharepageframe_ = Core.UI.CreateFrame(ctrl.FindContainerRecursive("sharepageframe"));	
	var sharetoolbar_config = {
		Items: [
			{
				ID: "RefreshFileList",
				Type: "Button",
				Css: "image_text_button btnrefreshfilelist",
				Config: { 
					Text: "刷新文件列表"
				}
			},
			{
				ID: "Upload",
				Type: "Button",
				Css: "image_text_button btnupload",
				Config: { 
					Text: "上传文件"
				}
			}
		]
	};
	sharetoolbar_ = new Core.UI.Toolbar(ctrl.FindContainerRecursive("sharetoolbar"), sharetoolbar_config);
	sharetoolbar_.SetItemVisible("Upload", ClientMode);
	
	tabctrl_.OnSelectedTab.Attach(
		function(id)
		{
			CurrentWindow.CompleteAll();
			if (id == "TAB1")
			{
				if (ClientMode)
				{
					if (loadsharepage_)
					{
						sharepageframe_.contentWindow.LoadFileList();
					}
					else
					{
						sharepageframe_.src = String.format("Share.htm?Group={0}&random={1}", config.Peer.ID, (new Date()).getTime());
						loadsharepage_ = true;
					}
				}
				else
				{
					CurrentWindow.Waiting("");
					sharepageframe_.src = String.format("Share.aspx?Group={0}&random={1}", config.Peer.ID, (new Date()).getTime());
				}
			}
		}
	);
	
	sharetoolbar_.OnCommand.Attach(
		function(command)
		{
			if (command == "RefreshFileList")
			{
				if (ClientMode)
				{
					if(loadsharepage_)
					{
						sharepageframe_.contentWindow.LoadFileList();
					}
				}
				else
				{
					CurrentWindow.CompleteAll();
					CurrentWindow.Waiting("");
					sharepageframe_.src = String.format("Share.aspx?Group={0}&random={1}", config.Peer.ID, (new Date()).getTime());
				}
			}
			else if (command == "Upload")
			{
				var file = window.external.OpenFile("");
				if (file != "")
				{
					sharepageframe_.contentWindow.Upload(file);
				}
			}
		}
	);
	
	CurrentWindow.OnClosed.Attach(
		function()
		{
			if (ClientMode && loadsharepage_)
			{
				try
				{
					 sharepageframe_.contentWindow.CancelAll();
				}
				catch(ex)
				{
				}
			}
		}
	);
	
	tabctrl_.Select("TAB0");
}

})();