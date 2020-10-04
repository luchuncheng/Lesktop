using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Core;

class Category_CH : Core.CommandHandler
{
	public Category_CH(HttpContext context, String sessionId, String id, String data)
		: base(context, sessionId, id, data)
	{

	}

	public override string Process()
	{
		Hashtable ps = Core.Utility.ParseJson(Data as string) as Hashtable;
		string action = ps["Action"] as string;
		AccountInfo cu = ServerImpl.Instance.GetCurrentUser(Context);
		if (action == "GetCategoryData")
		{
			DataRowCollection items = ServerImpl.Instance.CommonStorageImpl.GetCategoryItems(cu.ID);
			Hashtable users = GetAccountInfos(items);

			DataRowCollection dept_rows = ServerImpl.Instance.CommonStorageImpl.GetCategoryDepts(cu.ID);
			Hashtable depts = GetDeptInfos(dept_rows);

			return Utility.RenderHashJson(
				"Categories", ServerImpl.Instance.CommonStorageImpl.GetCategories(cu.ID),
				"CategoryItems", items,
				"Users", users,
				"Depts", depts,
				"CompanyInfo", ServerImpl.Instance.CommonStorageImpl.GetCompanyInfo()
			);
		}
		else if (action == "CreateCategory")
		{
			DataRow category = ServerImpl.Instance.CommonStorageImpl.CreateCategory(cu.ID, ps["Name"].ToString(), Convert.ToInt32(ps["ParentID"]));
			return Utility.RenderHashJson(
				"Categories", ServerImpl.Instance.CommonStorageImpl.GetCategories(cu.ID),
				"Category", category
			);
		}
		else if (action == "DeleteCategory")
		{
			ServerImpl.Instance.CommonStorageImpl.DeleteCategory(Convert.ToInt32(ps["ID"]));
			return Utility.RenderHashJson(
				"Categories", ServerImpl.Instance.CommonStorageImpl.GetCategories(cu.ID),
				"Result", true
			);
		}
		else if (action == "RenameCategory")
		{
			DataRow category = ServerImpl.Instance.CommonStorageImpl.RenameCategory(Convert.ToInt32(ps["ID"]), ps["Name"].ToString());
			return Utility.RenderHashJson(
				"Categories", ServerImpl.Instance.CommonStorageImpl.GetCategories(cu.ID),
				"Category", category
			);
		}
		else if (action == "AddToCategory")
		{
			ServerImpl.Instance.CommonStorageImpl.AddToCategory(cu.ID, Convert.ToInt32(ps["CategoryID"]), Convert.ToInt32(ps["ItemID"]));
			if (Convert.ToInt32(ps["Type"]) == 3)
			{
				return Utility.RenderHashJson(
					"Dept", AccountImpl.Instance.GetDeptInfo(Convert.ToInt32(ps["ItemID"])),
					"CategoryItems", ServerImpl.Instance.CommonStorageImpl.GetCategoryItems(cu.ID)
				);
			}
			else
			{
				return Utility.RenderHashJson(
					"User", AccountImpl.Instance.GetUserInfo(Convert.ToInt32(ps["ItemID"])).DetailsJson,
					"CategoryItems", ServerImpl.Instance.CommonStorageImpl.GetCategoryItems(cu.ID)
				);
			}
		}
		else if (action == "AddItemsToCategory")
		{
			ServerImpl.Instance.CommonStorageImpl.AddItemsToCategory(cu.ID, Convert.ToInt32(ps["CategoryID"]), Utility.ParseIntArray(Convert.ToString(ps["Items"])));
			return Utility.RenderHashJson(
				"CategoryItems", ServerImpl.Instance.CommonStorageImpl.GetCategoryItems(cu.ID)
			);
		}
		else if (action == "RemoveFromCategory")
		{
			ServerImpl.Instance.CommonStorageImpl.RemoveFromCategory(cu.ID, Convert.ToInt32(ps["CategoryID"]), Convert.ToInt32(ps["ItemID"]));
			return Utility.RenderHashJson(
				"CategoryItems", ServerImpl.Instance.CommonStorageImpl.GetCategoryItems(cu.ID)
			);
		}
		else if (action == "ResetCategories")
		{
			DataRowCollection pre_categories = ServerImpl.Instance.CommonStorageImpl.ResetCategories(
				cu.ID, Utility.ParseIntArray(Convert.ToString(ps["Categorys"])),
				Convert.ToInt32(ps["ItemID"]), Convert.ToInt32(ps["Type"])
			); 
			if (Convert.ToInt32(ps["Type"]) == 3)
			{
				return Utility.RenderHashJson(
					"Result", true,
					"Dept", AccountImpl.Instance.GetDeptInfo(Convert.ToInt32(ps["ItemID"])),
					"CategoryItems", ServerImpl.Instance.CommonStorageImpl.GetCategoryItems(cu.ID),
					"PreCategories", pre_categories
				 );
			}
			else
			{
				return Utility.RenderHashJson(
					"Result", true,
					"User", AccountImpl.Instance.GetUserInfo(Convert.ToInt32(ps["ItemID"])).DetailsJson,
					"CategoryItems", ServerImpl.Instance.CommonStorageImpl.GetCategoryItems(cu.ID),
					"PreCategories", pre_categories
				 );
			}
		}
		throw new NotImplementedException(String.Format("Command \"{0}\" isn't implemented", action));
	}

	private static Hashtable GetDeptInfos(DataRowCollection dept_rows)
	{
		Hashtable depts = new Hashtable();
		foreach (DataRow r in dept_rows)
		{
			if (!depts.ContainsKey(Convert.ToInt32(r["ID"])))
			{
				depts[Convert.ToInt32(r["ID"])] = r;
			}
		}
		return depts;
	}

	public static Hashtable GetAccountInfos(DataRowCollection items)
	{
		Hashtable users = new Hashtable();
		foreach (DataRow r in items)
		{
			if (Convert.ToInt32(r["CategoryType"]) == 1 || Convert.ToInt32(r["CategoryType"]) == 2)
			{
				AccountInfo ai = AccountImpl.Instance.GetUserInfo(Convert.ToInt32(r["ItemID"]));
				if (ai != null && !users.ContainsKey(ai.ID))
				{
					users.Add(ai.ID, ai.DetailsJson);
				}
			}
		}
		return users;
	}

	public override void Process(object data)
	{
		throw new NotImplementedException();
	}
}