
var layim = null;
var layer = null;

//演示自动回复
var autoReplay = [
  '您好，我现在有事不在，一会再和您联系。',
  '你没发错吧？face[微笑] ',
  '洗澡中，请勿打扰，偷窥请购票，个体四十，团体八折，订票电话：一般人我不告诉他！face[哈哈] ',
  '你好，我是主人的美女秘书，有什么事就跟我说吧，等他回来我会转告他的。face[心] face[心] face[心] ',
  'face[威武] face[威武] face[威武] face[威武] ',
  '<（@￣︶￣@）>',
  '你要和我说话？你真的要和我说话？你确定自己想说吗？你一定非说不可吗？那你说吧，这是自动回复。',
  'face[黑线]  你慢慢说，别急……',
  '(*^__^*) face[嘻嘻] ，是贤心吗？'
];

function GetFriends()
{
	var friends = [];
	for (var i = 0; i < window.MobileInitParams.Categories.length; i++)
	{
		var category = window.MobileInitParams.Categories[i];
		if (category.Type == 1)
		{
			var group = {
				"groupname": category.Name,
				"id": category.ID.toString(),
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
			friends.push(group);
		}
	}
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
	case 'find':
		{
			layer.msg('自定义发现动作');

			//模拟标记“发现新动态”为已读
			layim.showNew('More', false);
			layim.showNew('find', false);
			break;
		}
	case 'share':
		{
			layim.panel({
				title: '邀请好友', //标题
				tpl: '<div style="padding: 10px;">自定义模版，{{d.data.test}}</div>', //模版
				data: { //数据
					test: '么么哒'
				}
			});
		}
		break;
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

function LayIM_SendMsg(data)
{
	var msgdata = {
		Action: "NewMessage",
		Sender: parseInt(data.mine.id, 10),
		Receiver: parseInt(data.to.id, 10)
	};

	msgdata.Content = Core.TranslateMessage(data.mine.content, msgdata);

	Core.SendCommand(
		function (ret)
		{
			var message = ret;
			msgpanel_.AddMessage(message, temp);
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
	console.log(data);
	layim.panel({
		title: '与 ' + data.name + ' 的聊天记录', //标题
		tpl: '<div style="padding: 10px;">这里是模版，{{d.data.test}}</div>', //模版
		data: { //数据
			test: 'Hello'
		}
	});
}

var Mobile_HtmlBeginTagRegex = /<([a-zA-Z0-9]+)(\s+)[^<>]+>/ig;
var Mobile_HtmlEndTagRegex = /<\/([a-zA-Z0-9]+)>/ig;
var Mobile_HtmlImgSrcRegex = /src\=\x22([^<>\x22]+)\x22/ig;

function GetImageSrc(img)
{
	Mobile_HtmlImgSrcRegex.lastIndex = 0
	var ret = Mobile_HtmlImgSrcRegex.exec(img);
	if (ret == null || ret.length <= 1)
	{
		return "";
	}
	return ret[1];
}

function ClearHTML(text)
{
	var newText = text;
	try
	{
		newText = text.toString().replace(
			Mobile_HtmlBeginTagRegex,
			function (html, tag)
			{
				if (tag.toLowerCase() == "img")
				{
					var url = GetImageSrc(html);
					return String.format("img[{0}]", url);
				}
				return "";
			}
		)
		.replace(
			Mobile_HtmlEndTagRegex,
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

function OnNewMessage(msg)
{
	if (msg.Sender.Type == 0)
	{
		layim.getMessage({
			username: msg.Sender.Nickname,
			avatar: Core.CreateHeadImgUrl(msg.Sender.ID, 150, false, msg.Sender.HeadIMG),
			id: msg.Sender.ID.toString(),
			type: "friend",
			cid: msg.ID.toString(),
			content: ClearHTML(msg.Content)
		});
	}
}

Core.OnNewMessage.Attach(OnNewMessage);

function StartServiceCallback()
{
	var userinfo = Core.Session.GetUserInfo();

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

			var layim_config = {
				//上传图片接口
				uploadImage: {
					url: '/upload/image', //（返回的数据格式见下文）
					type: '' //默认post
				},

				//上传文件接口
				uploadFile: {
					url: '/upload/file', //（返回的数据格式见下文）
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
				tool: [{
					alias: 'code',
					title: '代码',
					iconUnicode: '&#xe64e;'
				}],

				//扩展更多列表
				moreList: [{
					alias: 'find',
					title: '发现',
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

			//模拟"更多"有新动态
			//layim.showNew('More', true);
			//layim.showNew('find', true);
		});
}