[WebMethod]
[ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Json, XmlSerializeString = false)]
public void dashboardDetails()
{
    JavaScriptSerializer ser = new JavaScriptSerializer();
    try
    {
        Database db = JSSM.Entity.DBConnection.dbCon;
        DataSet dsList = db.ExecuteDataSet(db.GetStoredProcCommand("usp_Select_DashboardJobCard"));

        List<JobSummary> recentJobs = new List<JobSummary>();
        int totalPending = 0, totalCompleted = 0;
        decimal avgTurnaround = 0;

        // Recent jobs (first 5 only)
        if (dsList.Tables.Count > 0 && dsList.Tables[0].Rows.Count > 0)
        {
            foreach (DataRow dr in dsList.Tables[0].Rows)
            {
                if (recentJobs.Count >= 5) break;

                recentJobs.Add(new JobSummary
                {
                    JobCardID = dr["JobCardID"].ToString(),
                    CustomerName = dr["CustomerName"].ToString(),
                    Compliants = dr["Compliants"].ToString(),
                    Status = dr["JobcardStatus"].ToString(),
                    CreatedDate = Convert.ToDateTime(dr["JobCard_Date"]).ToString("dd/MM/yyyy")
                });

                string status = dr["JobcardStatus"].ToString();
                if (status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                    totalPending++;
                else if (status.Equals("Job Completed", StringComparison.OrdinalIgnoreCase) || status.Equals("Delivered", StringComparison.OrdinalIgnoreCase))
                    totalCompleted++;
            }
        }

        if (dsList.Tables.Count > 1 && dsList.Tables[1].Rows.Count > 0)
        {
            DataRow drSummary = dsList.Tables[1].Rows[0];
            avgTurnaround = Convert.ToDecimal(drSummary["AverageTAT"]);
        }

        int totalJobs = totalPending + totalCompleted;

        var response = new
        {
            Details = new
            {
                DashboardCount = new
                {
                    totaljobs = totalJobs,
                    totalpending = totalPending,
                    totalcompletion = totalCompleted,
                    last10DaysCompletedPercent = Convert.ToDecimal(dsList.Tables[1].Rows[0]["Last10DaysCompletedPercent"]),
                    last10DaysPendingPercent = Convert.ToDecimal(dsList.Tables[1].Rows[0]["Last10DaysPendingPercent"]),
                    last30DaysCompletedPercent = Convert.ToDecimal(dsList.Tables[1].Rows[0]["Last30DaysCompletedPercent"]),
                    last30DaysPendingPercent = Convert.ToDecimal(dsList.Tables[1].Rows[0]["Last30DaysPendingPercent"])
                },
                RecentJobs = recentJobs,  // Only top 5 jobs
                PerformanceMetrics = new
                {
                    completionrate = totalJobs > 0 ? (totalCompleted * 100 / totalJobs) : 0,
                    average = avgTurnaround
                }
            },
            status = (recentJobs.Count > 0 ? 1 : 0)
        };

        Context.Response.ContentType = "application/json; charset=utf-8";
        Context.Response.Write(ser.Serialize(response));
    }
    catch
    {
        throw;
    }
}

// JobSummary class
public class JobSummary
{
    public string JobCardID { get; set; }
    public string CustomerName { get; set; }
    public string Compliants { get; set; }
    public string Status { get; set; }
    public string CreatedDate { get; set; }
}
