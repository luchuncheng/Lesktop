<%@ Control Language="C#" AutoEventWireup="true" Inherits="Core.Web.Message_MessageCtrl" %>
<table id="msg_<%= MsgID %>" cellpadding="0" cellspacing="0" class="messageCtrl">
	<tr class='tr_info'>
		<td class="checkbox"><input type="checkbox" name="<%= MsgID %>" /></td>
		<td class="nickname"><%= SenderNickname %></td>
		<td class="nickname"><%= ReceiverNickname%></td>
		<td class="summary" onclick="return SwitchMessageVisible('msg_<%= MsgID %>','<%= Core.Utility.TransferCharJavascript(Content)%>','<%= Core.Utility.TransferCharJavascript(ReceiverName)%>')"><span><%= Summary%></span></td>
		<td class="createdTime"><%= CreatedTime %></td>
	</tr>
	<tr style="display:none;">
		<td class="checkbox"></td>
		<td colspan="4"><div class="messageContentDiv"></div></td>
	</tr>
</table>