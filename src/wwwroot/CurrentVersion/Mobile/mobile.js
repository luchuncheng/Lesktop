
var layim = null;
var layim_config = null;
var layer = null;

var LayIMGroup_Other = "1";		// 其他联系人分组ID
var LayIMGroup_MyFriend = "2";	// 我的好友分组

var LayIM_Faces = ["微笑", "嘻嘻", "哈哈", "可爱", "可怜", "挖鼻", "吃惊", "害羞", "挤眼", "闭嘴", "鄙视", "爱你", "泪", "偷笑", "亲亲", "生病", "太开心", "白眼", "右哼哼", "左哼哼", "嘘", "衰", "委屈", "吐", "哈欠", "抱抱", "怒", "疑问", "馋嘴", "拜拜", "思考", "汗", "困", "睡", "钱", "失望", "酷", "色", "哼", "鼓掌", "晕", "悲伤", "抓狂", "黑线", "阴险", "怒骂", "互粉", "心", "伤心", "猪头", "熊猫", "兔子", "ok", "耶", "good", "NO", "赞", "来", "弱", "草泥马", "神马", "囧", "浮云", "给力", "围观", "威武", "奥特曼", "礼物", "钟", "话筒", "蜡烛", "蛋糕"];
var LayIM_FaceToFile = {};

function GetFriends()
{
	var friends = [];
	for (var i = 0; i < window.MobileInitParams.Categories.length; i++)
	{
		var category = window.MobileInitParams.Categories[i];
		if (category.Type == 1)
		{
			var groupid = category.ID + 10000;
			var group = {
				"groupname": category.Name,
				"id": groupid.toString(),
				"online": 0,
				"list": []
			};
			var user_count = 0;
			var online_count = 0;
			for (var j = 0; j < window.MobileInitParams.CategorieItems.length; j++)
			{
				var item = window.MobileInitParams.CategorieItems[j];
				if (item.CategoryID == category.ID)
				{
					var friend_info = window.MobileInitParams.CategorieUsers[item.ItemID.toString()];
					if(friend_info != undefined)
					{
						group.list.push({
							"username": friend_info.Nickname,
							"id": friend_info.ID.toString(),
							"avatar": Core.CreateHeadImgUrl(friend_info.ID, 150, false, friend_info.HeadIMG),
							"sign": ""
						});
						user_count++;
						if (friend_info.State == "Online")
						{
							online_count++;
						}
					}
				}
			}
			if (user_count > 0)
			{
				friends.push(group);
			}
		}
	}
	
	var grou_myfriend = {
		"groupname": "我的好友",
		"id": LayIMGroup_MyFriend,
		"online": 0,
		"list": []
	}
	
	var current_user = window.MobileInitParams.UserInfo;

	for (var i = 0; i < window.MobileInitParams.VisibleUsers.length; i++)
	{
		var user = window.MobileInitParams.VisibleUsers[i];
		if (user.Type == 0 && ((current_user.SubType == 1 && user.SubType == 0) || current_user.SubType == 0))
		{
			// 注册用户，并添加自己为好友的
			grou_myfriend.list.push({
				"username": user.Nickname,
				"id": user.ID.toString(),
				"avatar": Core.CreateHeadImgUrl(user.ID, 150, false, user.HeadIMG),
				"sign": ""
			});
		}
	}

	friends.push(grou_myfriend);

	friends.push({
		"groupname": "其他联系人",
		"id": LayIMGroup_Other,
		"online": 0,
		"list": []
	});

	return friends;
}

function GetGroups()
{
	var groups = [];
	for (var i = 0; i < window.MobileInitParams.VisibleUsers.length; i++)
	{
		var user = window.MobileInitParams.VisibleUsers[i];
		if(user.Type == 1)
		{
			groups.push({
				"groupname": user.Nickname,
				"id": user.ID.toString(),
				"avatar": Core.CreateGroupImgUrl(user.HeadIMG, user.IsTemp)
			});
		}
	}
	return groups;
}

function LayIM_GroupExists(id)
{
	for(var i = 0; i < layim_config.init.group.length; i++)
	{
		if (layim_config.init.group[i].id == id) return true;
	}
	return false;
}

function LayIM_UserExists(id)
{
	for (var i = 0; i < layim_config.init.friend.length; i++)
	{
		var group = layim_config.init.friend[i];
		for (var j = 0; j < group.list.length; j++)
		{
			var user = group.list[j];
			if(user.id == id)
			{
				return true;
			}
		}
	}
	return false;
}

function LayIM_Details(data)
{
	//console.log(data); //获取当前会话对象
	layim.panel({
		title: data.name + ' 聊天信息', //标题
		tpl: '<div style="padding: 10px;">自定义模版，<a href="http://www.layui.com/doc/modules/layim_mobile.html#ondetail" target="_blank">参考文档</a></div>', //模版
		data: { //数据
			test: '么么哒'
		}
	});
}

function LayIM_More(obj)
{
	switch (obj.alias)
	{
	case 'logout':
	    {
	        location = Core.GetUrl("Mobile/logout.aspx");
			break;
		}
	}
}

function LayIM_Back()
{
	//如果你只是弹出一个会话界面（不显示主面板），那么可通过监听返回，跳转到上一页面，如：history.back();
}

function LayIM_Tool(insert, send)
{
	insert('[pre class=layui-code]123[/pre]'); //将内容插入到编辑器
	send();
}

function LayIM_SendMsg_GetFileName(fileurl)
{
    var filename_regex = /FileName\=([^\s\x28\x29\x26]+)/ig;
    filename_regex.lastIndex = 0
    var ret = filename_regex.exec(fileurl);
    if (ret == null || ret.length <= 1)
    {
        return "";
    }
    return ret[1];
}

function LayIM_SendMsg(data)
{
    var msgdata = {
        Action: "NewMessage",
        Sender: parseInt(data.mine.id, 10),
        Receiver: parseInt(data.to.id, 10),
        DelTmpFile: 0,
        Content: ""
    };

    var content = data.mine.content;
    content = content.replace(
        /img\x5B([^\x5B\x5D]+)\x5D/ig,
        function(imgtext, src)
        {
        	var filename = LayIM_SendMsg_GetFileName(src);
        	return String.format('<img src="{0}">', Core.CreateDownloadUrl(filename));
        }
    );
    content = content.replace(
        /file\x28([^\x28\x29]+)\x29\x5B([^\x5B\x5D]+)\x5D/ig,
        function (filetext, fileurl, ope)
        {
            var path = unescape(LayIM_SendMsg_GetFileName(fileurl));
            return Core.CreateFileHtml([path]);
        }
    );
    content = Core.TranslateMessage(content, msgdata);

    content = content.replace(
        /face\[([^\s\[\]]+?)\]/g,
        function (face, face_type)
        {
            var face_file = LayIM_FaceToFile[face_type];
            if(face_file != undefined)
            {
                return String.format('<img src="{0}/{1}">', Core.GetUrl("layim/images/face"), face_file);
            }
        }
    );

    msgdata.Content = content;

	Core.SendCommand(
		function (ret)
		{
			var message = ret;
		},
		function (ex)
		{
			var errmsg = String.format("由于网络原因，消息\"{0}\"发送失败，错误信息:{1}", text, ex.Message);
		},
		Core.Utility.RenderJson(msgdata), "Core.Web WebIM", false
	);
}

function LayIM_ChatLog(data, ul)
{
}

function LayIM_ParseMsg(text)
{
	var newText = text;
	try
	{
		newText = text.toString().replace(
			/<([a-zA-Z0-9]+)([\s]+)[^<>]*>/ig,
			function (html, tag)
			{
				if (tag.toLowerCase() == "img")
				{
					var filename = Core.GetFileNameFromImgTag(html);
					if (filename != "")
					{
					    var url = Core.CreateDownloadUrl(filename);
					    return String.format("a({0})[img[{0}&MaxWidth=450&MaxHeight=800]]", url);
					}
					else
					{
					    var src = Core.GetSrcFromImgTag(html);
					    return String.format("img[{0}]", src);
					}
				}
				return "";
			}
		)
		.replace(
			/\x5BFILE:([^\x5B\x5D]+)\x5D/ig,
			function (filetag, filepath)
			{
				var path = unescape(filepath)
				var ext = Core.Path.GetFileExtension(path).toLowerCase();
				if (ext == ".mp4" || ext == ".mov")
				{
					return String.format("video[{0}]", Core.CreateDownloadUrl(path), Core.Path.GetFileName(path));
				}
				else if (ext == "mp3")
				{
					return String.format("audio[{0}]", Core.CreateDownloadUrl(path), Core.Path.GetFileName(path));
				}
				else
				{
					return String.format("file({0})[{1}]", Core.CreateDownloadUrl(path), Core.Path.GetFileName(path));
				}
			}
		)
		.replace(
			/<([a-zA-Z0-9]+)[\x2F]{0,1}>/ig,
			function (html, tag)
			{
				return "";
			}
		)
		.replace(
			/<\/([a-zA-Z0-9]+)>/ig,
			function (html, tag)
			{
				return "";
			}
		);
	}
	catch(ex)
	{
		newText += " ERROR:";
		newText += ex.message;
	}
	return newText;
}

function LayIM_OnNewMessage(msg)
{
	// msg.Sender, msg.Receiver只包括最基本的ID，Name，需重新获取详细信息
	var sender_info = Core.AccountData.GetAccountInfo(msg.Sender.ID);
	if (sender_info == null) sender_info = msg.Sender;
	var receiver_info = Core.AccountData.GetAccountInfo(msg.Receiver.ID);
	if (receiver_info == null) receiver_info = msg.Receiver;

	if (msg.Receiver.Type == 0)
	{
		if (!LayIM_UserExists(sender_info.ID.toString()))
		{
			layim.addList({
				type: 'friend',
				"username": sender_info.Nickname,
				"id": sender_info.ID.toString(),
				"groupid": LayIMGroup_Other,
				"avatar": Core.CreateHeadImgUrl(sender_info.ID, 150, false, sender_info.HeadIMG),
				"sign": ""
			});
		}

		layim.getMessage({
			username: sender_info.Nickname,
			avatar: Core.CreateHeadImgUrl(msg.Sender.ID, 150, false, sender_info.HeadIMG),
			id: msg.Sender.ID.toString(),
			type: "friend",
			cid: msg.ID.toString(),
			content: LayIM_ParseMsg(msg.Content)
		});
	}
	else if (msg.Receiver.Type == 1)
	{
		if (!LayIM_GroupExists(receiver_info.ID.toString()))
		{
			layim.addList({
				"type": "group",
				"groupname": receiver_info.Nickname,
				"id": receiver_info.ID.toString(),
				"avatar": Core.CreateGroupImgUrl(receiver_info.HeadIMG, receiver_info.IsTemp)
			});
		}
		layim.getMessage({
			username: sender_info.Nickname,
			avatar: Core.CreateHeadImgUrl(msg.Sender.ID, 150, false, sender_info.HeadIMG),
			id: msg.Receiver.ID.toString(),
			type: "group",
			cid: msg.ID.toString(),
			content: LayIM_ParseMsg(msg.Content)
		});
	}
}

Core.OnNewMessage.Attach(LayIM_OnNewMessage);

function StartServiceCallback()
{
}

function LayIM_Init()
{
	var userinfo = window.MobileInitParams.UserInfo;

    for (var i = 0; i < LayIM_Faces.length; i++)
    {
        LayIM_FaceToFile[LayIM_Faces[i]] = i.toString() + ".gif";
    }

	layui.config({ version: true }).use(
		'mobile',
		function ()
		{
			var mobile = layui.mobile;

			layim = mobile.layim;
			layer = mobile.layer;

			//提示层
			layer.msg = function (content)
			{
				return layer.open({
					content: content
				  , skin: 'msg'
				  , time: 2 //2秒后自动关闭
				});
			};

			layim_config = {
				//上传图片接口
				uploadImage: {
					url: Core.GetUrl("Mobile/uploadfile.ashx?type=image&maxwidth=450&maxheight=800"), //（返回的数据格式见下文）
					type: '' //默认post
				},

				//上传文件接口
				uploadFile: {
				    url: Core.GetUrl("Mobile/uploadfile.ashx?type=file"), //（返回的数据格式见下文）
					type: '' //默认post
				},

				//,brief: true

				init: {
					//我的信息
					mine: {
						"username": userinfo.Nickname, //我的昵称
						"id": userinfo.ID.toString(), //我的ID
						"avatar": Core.CreateHeadImgUrl(userinfo.ID, 150, false, userinfo.HeadIMG), //我的头像
						"sign": ""
					},
					//我的好友列表
					friend: GetFriends(),
					group: GetGroups()
				},

				//扩展聊天面板工具栏
				/*
				tool: [{
					alias: 'code',
					title: '代码',
					iconUnicode: '&#xe64e;'
				}],
				*/
				//扩展更多列表
				moreList: [{
				    alias: 'logout',
					title: '退出登录',
					iconUnicode: '&#xe628;', //图标字体的unicode，可不填
					iconClass: '' //图标字体的class类名
				}],

				//tabIndex: 1, //用户设定初始打开的Tab项下标
				isNewFriend: false, //是否开启“新的朋友”
				isgroup: true, //是否开启“群聊”
				//chatTitleColor: '#c00', //顶部Bar颜色
				title: userinfo.Nickname
			};
			layim.config(layim_config);

			//查看聊天信息
			layim.on('detail', LayIM_Details);
			//监听点击更多列表
			layim.on('moreList', LayIM_More);
			//监听返回
			layim.on('back', LayIM_Back);
			//监听自定义工具栏点击，以添加代码为例
			layim.on('tool(code)', LayIM_Tool);
			//监听发送消息
			layim.on('sendMessage', LayIM_SendMsg);
			//监听查看更多记录
			layim.on('chatlog', LayIM_ChatLog);


			StartService(StartServiceCallback);

			//模拟"更多"有新动态
			//layim.showNew('More', true);
			//layim.showNew('find', true);
		});
}