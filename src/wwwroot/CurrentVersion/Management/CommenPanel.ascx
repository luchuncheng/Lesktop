<%@ Control Language="C#" AutoEventWireup="true" Inherits="Comment_CommenPanel" %>
<div class="comment">
	<div class='messageTitle'>
		<div class='sender'>
			<%= Data["Name"].ToString().Replace("<", "&lt;")%></div>
		<div class='time'>
			<%= String.Format("{0:yyyy-MM-dd HH:mm:ss}", Data["CreatedTime"]).Replace("<", "&lt;")%></div>
	</div>
	<div class='messageContent'>
		<%= Data["Content"].ToString().Replace("<","&lt;") %></div>
	<div class='comment_custom'>
		<div class='comment_ope'>
			<div style="float:left;">手机：<%= Data["Tel"].ToString().Replace("<", "&lt;")%>&nbsp;&nbsp;</div>
			<div style="float:left;">电子邮件：<%= Data["Mail"].ToString().Replace("<", "&lt;")%></div><a href='mailto:<%= Data["Mail"].ToString() %>'>回复</a><a href="javascript:DoCommand('Delete','<%= Convert.ToInt32(Data["ID"]) %>')">删除</a><a href="javascript:DoCommand('MarkAsRead','<%= Convert.ToInt32(Data["ID"]) %>')" style="display:<%= Convert.ToInt32(Data["IsRead"]) == 1?"none":"inline"%>"><%= "设为已读" %></a><a href="javascript:DoCommand('MarkAsUnread','<%= Convert.ToInt32(Data["ID"]) %>')" style="display:<%= Convert.ToInt32(Data["IsRead"]) == 0?"none":"inline"%>"><%= "设为未读" %></a><a href="javascript:DoCommand('MarkAsImportant','<%= Convert.ToInt32(Data["ID"]) %>')" style="display:<%= Convert.ToInt32(Data["IsImportant"]) == 1?"none":"inline"%>"><%= "设为重要留言" %></a><a href="javascript:DoCommand('MarkAsUnimportant','<%= Convert.ToInt32(Data["ID"]) %>')" style="display:<%= Convert.ToInt32(Data["IsImportant"]) == 0?"none":"inline"%>"><%= "设为普通留言" %></a>
		</div>
	</div>
	<br />
</div>
