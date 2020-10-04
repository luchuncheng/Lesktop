using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Core;

namespace Core.Web
{
	public class Mobile_Default : System.Web.UI.Page
	{
		string init_params_ = "{}";

		protected void Page_Load(object sender, EventArgs e)
		{
			AccountInfo current_user = ServerImpl.Instance.GetCurrentUser(Context);
			if(current_user != null)
			{
				String sessionId = Guid.NewGuid().ToString().ToUpper();
				ServerImpl.Instance.Login(sessionId, Context, current_user.ID, false, null, true, 2);

				DataRowCollection categories = ServerImpl.Instance.CommonStorageImpl.GetCategories(current_user.ID);

				DataRowCollection items = ServerImpl.Instance.CommonStorageImpl.GetCategoryItems(current_user.ID);
				Hashtable users = Category_CH.GetAccountInfos(items);

				AccountInfo[] visible_users = AccountImpl.Instance.GetVisibleUsersDetails(current_user.Name);

				init_params_ = Utility.RenderHashJson(
					"Result", true,
					"IsLogin", true,
					"UserInfo", current_user.DetailsJson,
					"SessionID", sessionId,
					"CompanyInfo", ServerImpl.Instance.CommonStorageImpl.GetCompanyInfo(),
					"Categories", categories,
					"CategorieItems", items,
					"CategorieUsers", users,
					"VisibleUsers", visible_users
				);
			}
			else
			{
				Response.Redirect("login.aspx");
			}
		}

		public string InitParams
		{
			get { return init_params_; }
		}
	}
}