    #region Login
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Json, XmlSerializeString = false)]
    public void UserLogin(string UserName, string Password)
    {
        JavaScriptSerializer ser = new JavaScriptSerializer();
        try
        {
            string userNameStr = Convert.ToString(UserName);
            string passwordStr = Convert.ToString(Password);

            Database db = JSSM.Entity.DBConnection.dbCon;
            DataSet dsList = null;

            // Call stored procedure
            DbCommand cmd = db.GetStoredProcCommand(constants.StoredProcedures.usp_Select_User);
            db.AddInParameter(cmd, "@UserName", DbType.String, userNameStr);
            db.AddInParameter(cmd, "@Password", DbType.String, CommonMethods.Security.Encrypt(passwordStr, true));

            dsList = db.ExecuteDataSet(cmd);

            object responseObj;

            if (dsList.Tables.Count > 0 && dsList.Tables[0].Rows.Count > 0)
            {
                var userDetailsList = new List<object>();

                foreach (DataRow dr in dsList.Tables[0].Rows)
                {
                    string employeeID = dr["FK_EmployeeID"].ToString();

                    // Default values
                    string empName = "", empEmail = "", empMobile = "";

                    if (Convert.ToInt32(employeeID) > 0)
                    {
                        // Fetch employee details
                        DbCommand empCmd = db.GetStoredProcCommand(constants.StoredProcedures.USP_SELECT_EMPLOYEE);
                        db.AddInParameter(empCmd, "@PK_EmployeeID", DbType.Int32, Convert.ToInt32(employeeID));

                        DataSet dsEmp = db.ExecuteDataSet(empCmd);

                        if (dsEmp.Tables.Count > 0 && dsEmp.Tables[0].Rows.Count > 0)
                        {
                            DataRow drEmp = dsEmp.Tables[0].Rows[0];
                            empName = Convert.ToString(drEmp["EmployeeName"]);
                            empEmail = Convert.ToString(drEmp["Email"]);
                            empMobile = Convert.ToString(drEmp["MobileNo"]);
                        }
                    }

                    userDetailsList.Add(new
                    {
                        UserID = Convert.ToString(dr["PK_UserID"]),
                        UserName = Convert.ToString(dr["UserName"]),
                        EmployeeID = employeeID,
                        EmployeeName = empName,
                        Email = empEmail,
                        MobileNo = empMobile,
                        BranchName = Convert.ToString(dr["BranchName"])
                    });
                }

                responseObj = new
                {
                    Status = true,
                    Message = "Login successful",
                    Details = userDetailsList
                };
            }
            else
            {
                responseObj = new
                {
                    Status = false,
                    Message = "Invalid username or password",
                    Details = new List<object>() // empty
                };
            }

            HttpContext.Current.Response.ContentType = "application/json";
            HttpContext.Current.Response.Write(ser.Serialize(responseObj));
        }
        catch (Exception ex)
        {
            HttpContext.Current.Response.ContentType = "application/json";
            HttpContext.Current.Response.Write(ser.Serialize(new
            {
                Status = false,
                Message = ex.Message,
                Details = new List<object>()
            }));
        }
    }
    #endregion
