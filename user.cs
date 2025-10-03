  
 	#ISQUAD API 

#region Login
  [WebMethod]
  [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Json, XmlSerializeString = false)]
  public void UserLogin(string UserName, string Password)
  {
      JavaScriptSerializer ser = new JavaScriptSerializer();
      try
      {
          // Ensure input parameters are strings
          string userNameStr = Convert.ToString(UserName);
          string passwordStr = Convert.ToString(Password);

          Database db = JSSM.Entity.DBConnection.dbCon;
          DataSet dsList = null;

          // Prepare stored procedure command
          DbCommand cmd = db.GetStoredProcCommand(constants.StoredProcedures.usp_Select_User);
          db.AddInParameter(cmd, "@UserName", DbType.String, userNameStr);
          db.AddInParameter(cmd, "@Password", DbType.String, CommonMethods.Security.Encrypt(passwordStr, true));

          dsList = db.ExecuteDataSet(cmd);

          object responseObj;

          if (dsList.Tables.Count > 0 && dsList.Tables[0].Rows.Count > 0)
          {
              // Login successful, create details array
              var userDetailsList = new List<object>();
              foreach (DataRow dr in dsList.Tables[0].Rows)
              {
                  userDetailsList.Add(new
                  {
                      UserID = Convert.ToString(dr["PK_UserID"]),
                      UserName = Convert.ToString(dr["UserName"]),
                      //Email = Convert.ToString(dr["Email"]),
                      EmployeeID = Convert.ToString(dr["FK_RoleID"]),
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
              // Login failed
              responseObj = new
              {
                  Status = false,
                  Message = "Invalid username or password",
                  Details = new List<object>() // empty
              };
          }

          // Return JSON
          HttpContext.Current.Response.ContentType = "application/json";
          HttpContext.Current.Response.Write(ser.Serialize(responseObj));
      }
      catch (Exception ex)
      {
          // Error handling
          HttpContext.Current.Response.ContentType = "application/json";
          HttpContext.Current.Response.Write(ser.Serialize(new
          {
              Status = false,
              Message = ex.Message,
              Details = new List<object>() // empty
          }));
      }
  }
  #endregion
