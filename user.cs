
    #region Login
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Json, XmlSerializeString = false)]
    public void UserLogin(string UserName, string Password)
    {
        JavaScriptSerializer ser = new JavaScriptSerializer();
        try
        {
            Database db;
            DataSet dsList = null;
            JSSM.Entity.User ObjUser = new JSSM.Entity.User();
            db = JSSM.Entity.DBConnection.dbCon;
            DbCommand cmd = db.GetStoredProcCommand(constants.StoredProcedures.usp_Select_UserLogin);
            db.AddInParameter(cmd, "@UserName", DbType.String, UserName);
            db.AddInParameter(cmd, "@Password", DbType.String, CommonMethods.Security.Encrypt(Password, true));
            //db.AddInParameter(cmd, "@Password", DbType.String, Password);
            dsList = db.ExecuteDataSet(cmd);
            List<JSSM.Entity.User> iDetail = new List<JSSM.Entity.User>();
            if (dsList.Tables.Count > 0 && dsList.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow drrow in dsList.Tables[0].Rows)
                {
                    ObjUser = new JSSM.Entity.User();


                    ObjUser.UserID = Convert.ToInt32(drrow["PK_UserID"]);

                    ObjUser.UserName = Convert.ToString(drrow["UserName"]);

                    ObjUser.EmployeeName = Convert.ToInt32(drrow["FK_RoleID"]);

                    ObjUser.BranchName = Convert.ToString(drrow["BranchName"]);

                    iDetail.Add(ObjUser);
                }
            }
        }
        catch
        {
            throw;
        }
    }

    public class Employee
    {
        public List<JSSM.Entity.User> Details;
        public string status;
    }

    #endregion