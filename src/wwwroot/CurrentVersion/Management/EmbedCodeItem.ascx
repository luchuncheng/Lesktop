<%@ Control Language="C#" AutoEventWireup="true" Inherits="Lesktop_EmbedCode_EmbedCodeItem" %>
<a name="ec_<%= EmbedCodeID %>"></a>
<div id="<%= EmbedCodeID %>_info" class="table_blue">
	<table cellpadding="0" cellspacing="0">
		<tr class="header">
			<td class='td_left'>
				嵌入代码名称&nbsp;：
			</td>
			<td class='td_right' style="width:200px; color:#AA0000; font-weight:normal;">
				<%= EmbedConfig.Name %>&nbsp;
			</td>
			<td>&nbsp;</td>
		</tr>
	</table>
	<table cellpadding="0" cellspacing="0">
		<tr>
			<td class="embed_cb" style="border-bottom-width: 0px;">
			   <input type="checkbox" disabled="true" <%= EmbedConfig.ShowWindow ? "CHECKED" : "" %> />
			</td>
			<td class="td_left" style="border-bottom-width: 0px;">
				显示客服人员窗口
			</td>
			<td class="td_right" style="border-bottom-width: 0px;">
				&nbsp;
			</td>
			<td rowspan="5" style="border-left-width: 1px; border-right-width: 1px; width: 16px;
				text-align: center;">
				客<br />
				服<br />
				人<br />
				员
			</td>
			<td rowspan="4" style="background-color: #FFFFFF; border-bottom-width: 0px; padding: 0px;">
				<div style="height: 150px; overflow-y: auto; padding: 8px; vertical-align:top; color:Black;"><pre><%= UsersPreview%></pre></div>
			   </td>
		</tr>
		<tr>
			<td class="embed_cb" style="border-bottom-width: 0px;">
				&nbsp;
			</td>
			<td class="td_left" style="border-bottom-width: 0px;">
				浮动窗口宽度：
			</td>
			<td class="td_right" style="border-bottom-width: 0px;">
				<%= EmbedConfig.Width.ToString() %>
			</td>
		</tr>
		<tr>
			<td class="embed_cb" style="border-bottom-width: 0px;">
				&nbsp;
			</td>
			<td class="td_left" style="border-bottom-width: 0px;">
				浮动窗口高度：
			</td>
			<td class="td_right" style="border-bottom-width: 0px;">
				<%= EmbedConfig.Height.ToString() %>
			</td>
		</tr>
		<tr>
			<td>
				&nbsp;
			</td>
			<td>
				&nbsp;
			</td>
			<td>
				&nbsp;
			</td>
		</tr>
		<tr>
			<td class="embed_cb">
				<input type="checkbox" disabled="true"  <%= EmbedConfig.NewWindow ? "CHECKED" : "" %>/>
			</td>
			<td colspan="2">
					在新窗口中打开对话窗口
			</td>
			<td>
				&nbsp;
			</td>
		</tr>
	</table>
	<table cellpadding="0" cellspacing="0">
		<tr>
			<td>
			<div><%= EmbecCode.Replace("<","&lt;") %></div>
			<br /><br />
			<span style="color:red;">请将以上代码粘贴到需要嵌入在线客服系统的网页中，修改嵌入代码相关参数后，不需要重新在网页中粘贴嵌入代码。</span>
			</td>
		</tr>
		<tr>
			<td style="text-align: right">
			   <a href='javascript:Parse(<%= Core.Utility.RenderJson(EmbecCode) %>)'>复制嵌入代码</a> <a target="_blank" href="../EmbedTest.aspx?ID=<%= EmbedCodeID%>">预览</a> <a href='javascript:DeleteEmbedCode(<%= EmbedCodeID %>, <%= Core.Utility.RenderJson(EmbedConfig.Name) %>)'>删除</a> <a href="javascript:Edit(<%= EmbedCodeID %>, true);">修改嵌入代码</a>
			</td>
		</tr>
	</table>
</div>
<div id="<%= EmbedCodeID %>_editor" class="table_red" style="display:none">
	<table cellpadding="0" cellspacing="0">
		<tr class="header">
			<td class='td_left'>
				嵌入代码名称&nbsp;：
			</td>
			<td class='td_right' style="width:200px;">
				<input id="<%= EmbedCodeID %>_ec_name" type="text" value="<%= EmbedConfig.Name %>" />
			</td>
			<td>&nbsp;</td>
		</tr>
	</table>
	<table cellpadding="0" cellspacing="0">
		<tr>
			<td class="embed_cb" style="border-bottom-width: 0px;">
				<input id="<%= EmbedCodeID %>_ec_showwin" type="checkbox" <%= EmbedConfig.ShowWindow ? "CHECKED" : "" %> />
			</td>
			<td class="td_left" style="border-bottom-width: 0px;">
				显示客服人员窗口
			</td>
			<td class="td_right" style="border-bottom-width: 0px;">
				&nbsp;
			</td>
			<td rowspan="5" style="border-left-width: 1px; border-right-width: 1px; width: 16px;
				text-align: center;">
				客<br />
				服<br />
				人<br />
				员
			</td>
			<td rowspan="4" style="background-color: #FFFFFF; border-bottom-width: 0px; padding: 0px;">
				<div  id="<%= EmbedCodeID %>_ec_users_preview" style="height: 150px; overflow-y: auto; padding: 8px; vertical-align:top; color:Black;"><pre><%= UsersPreview %></pre></div>
				<input type="hidden"  id="<%= EmbedCodeID %>_ec_users" value="<%= EmbedConfig.Users %>" />
			</td>
		</tr>
		<tr>
			<td class="embed_cb" style="border-bottom-width: 0px;">
				&nbsp;
			</td>
			<td class="td_left" style="border-bottom-width: 0px;">
				浮动窗口宽度：
			</td>
			<td class="td_right" style="border-bottom-width: 0px;">
				<input id="<%= EmbedCodeID %>_ec_winwidth" type="text" value="<%= EmbedConfig.Width.ToString() %>" />
			</td>
		</tr>
		<tr>
			<td class="embed_cb" style="border-bottom-width: 0px;">
				&nbsp;
			</td>
			<td class="td_left" style="border-bottom-width: 0px;">
				浮动窗口高度：
			</td>
			<td class="td_right" style="border-bottom-width: 0px;">
				<input id="<%= EmbedCodeID %>_ec_winheight" type="text" value="<%= EmbedConfig.Height.ToString() %>" />
			</td>
		</tr>
		<tr>
			<td>
				&nbsp;
			</td>
			<td>
				&nbsp;
			</td>
			<td>
				&nbsp;
			</td>
		</tr>
		<tr>
			<td class="embed_cb">
				<input id="<%= EmbedCodeID %>_ec_newwin" type="checkbox"  <%= EmbedConfig.NewWindow ? "CHECKED" : "" %>/>
			</td>
			<td colspan="2">
					在新窗口中打开对话窗口
			</td>
			<td>
				<a href="javascript:SelectCSUsers('<%= EmbedCodeID %>')">选择客服人员</a>
			</td>
		</tr>
		<%--<tr>
			<td class="embed_cb" style="text-align: right">
				&nbsp;
			</td>
			<td>
				&nbsp;
			</td>
			<td class="td_right">
				&nbsp;
			</td>
			<td>
				&nbsp;
			</td>
			<td style="text-align: right">
			   <a href="#">复制嵌入代码</a> <a href="#">预览</a> <a href="javascript:DeleteEmbedCode(<%= EmbedCodeID %>)">删除</a> <a href="javascript:UpdateEmbedCode(<%= EmbedCodeID %>);">更新嵌入代码</a>
			</td>
		</tr>--%>
	</table>
	<table cellpadding="0" cellspacing="0">
		<tr>
			<td style="text-align: right">
			   <a target="_blank" onclick="return Preview(this, <%= EmbedCodeID %>)">预览</a> <a href='javascript:DeleteEmbedCode(<%= EmbedCodeID %>, <%= Core.Utility.RenderJson(EmbedConfig.Name) %>)'>删除</a> <a href="javascript:UpdateEmbedCode(<%= EmbedCodeID %>);">保存嵌入代码</a><a href="javascript:Edit(<%= EmbedCodeID %>, false);">取消</a>
			</td>
		</tr>
	</table>
</div>
<br />