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
            db = JSSM.Entity.DBConnection.dbCon;

            DbCommand cmd = db.GetStoredProcCommand(constants.StoredProcedures.usp_Select_UserLogin);
            db.AddInParameter(cmd, "@UserName", DbType.String, UserName);
            db.AddInParameter(cmd, "@Password", DbType.String, CommonMethods.Security.Encrypt(Password, true));

            dsList = db.ExecuteDataSet(cmd);

            List<object> userArray = new List<object>();

            if (dsList.Tables.Count > 0 && dsList.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in dsList.Tables[0].Rows)
                {
                    var userObj = new
                    {
                        UserName = Convert.ToString(dr["UserName"]),
                        Password = Password,  // ⚠️ Normally don’t return password (security issue)
                        EmployeeName = Convert.ToString(dr["FK_RoleID"]),
                        BranchName = Convert.ToString(dr["BranchName"])
                    };

                    userArray.Add(userObj);
                }
            }

            HttpContext.Current.Response.ContentType = "application/json";
            HttpContext.Current.Response.Write(ser.Serialize(userArray));
        }
        catch
        {
            throw;
        }
    }


    #endregion
