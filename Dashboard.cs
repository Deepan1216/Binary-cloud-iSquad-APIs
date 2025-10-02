    #region Dashboard
    [WebMethod]
    [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Json, XmlSerializeString = false)]
    public void dashboardDetails()
    {
        JavaScriptSerializer ser = new JavaScriptSerializer();
        try
        {
            Database db = JSSM.Entity.DBConnection.dbCon;
            DataSet dsList = db.ExecuteDataSet(db.GetStoredProcCommand(constants.StoredProcedures.USP_SELECT_JOBCARD));

            List<JobSummary> recentJobs = new List<JobSummary>();
            int totalPending = 0, totalCompleted = 0;
            decimal avgTurnaround = 0;

            // Date ranges
            DateTime today = DateTime.Today;
            DateTime last10Days = today.AddDays(-10);
            DateTime last30Days = today.AddDays(-30);

            int last10DaysCompleted = 0, last10DaysPending = 0;
            int last30DaysCompleted = 0, last30DaysPending = 0;

            if (dsList.Tables.Count > 0 && dsList.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in dsList.Tables[0].Rows)
                {
                    DateTime jobDate = Convert.ToDateTime(dr["JobCard_Date"]);
                    string status = Convert.ToString(dr["JobcardStatus"]);

                    // Recent jobs (top 5)
                    if (recentJobs.Count < 5)
                    {
                        recentJobs.Add(new JobSummary
                        {
                            JobCardID = Convert.ToString(dr["JobCardID"]),
                            CustomerName = Convert.ToString(dr["CustomerName"]),
                            Status = status,
                            CreatedDate = jobDate.ToString("dd/MM/yyyy")
                        });
                    }

                    // Total counts
                    if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                        totalCompleted++;
                    else if (status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                        totalPending++;

                    // Last 10 days counts
                    if (jobDate >= last10Days)
                    {
                        if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                            last10DaysCompleted++;
                        else if (status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                            last10DaysPending++;
                    }

                    // Last 30 days counts
                    if (jobDate >= last30Days)
                    {
                        if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                            last30DaysCompleted++;
                        else if (status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                            last30DaysPending++;
                    }
                }
            }

            // Summary counts from Table[1]
            if (dsList.Tables.Count > 1 && dsList.Tables[1].Rows.Count > 0)
            {
                DataRow drSummary = dsList.Tables[1].Rows[0];
                avgTurnaround = Convert.ToDecimal(drSummary["Average_TAT"]);
            }

            // Total jobs
            int totalJobs = totalCompleted + totalPending;

            // Percentages
            decimal last10Total = last10DaysCompleted + last10DaysPending;
            decimal last30Total = last30DaysCompleted + last30DaysPending;

            decimal last10CompletedPercent = last10Total > 0 ? (last10DaysCompleted * 100 / last10Total) : 0;
            decimal last10PendingPercent = last10Total > 0 ? (last10DaysPending * 100 / last10Total) : 0;

            decimal last30CompletedPercent = last30Total > 0 ? (last30DaysCompleted * 100 / last30Total) : 0;
            decimal last30PendingPercent = last30Total > 0 ? (last30DaysPending * 100 / last30Total) : 0;

            // Build JSON response
            var response = new
            {
                Details = new
                {
                    DashboardCount = new
                    {
                        totaljobs = totalJobs,
                        totalpending = totalPending,
                        totalcompletion = totalCompleted,
                        last10DaysCompletedPercent = last10CompletedPercent,
                        last10DaysPendingPercent = last10PendingPercent,
                        last30DaysCompletedPercent = last30CompletedPercent,
                        last30DaysPendingPercent = last30PendingPercent
                    },
                    RecentJobs = recentJobs,
                    PerformanceMetrics = new
                    {
                        completionrate = totalJobs > 0 ? ((totalCompleted * 100) / totalJobs) : 0,
                        average = avgTurnaround
                    }
                },
                status = (recentJobs.Count > 0 ? 1 : 0)
            };

            this.Context.Response.ContentType = "application/json; charset=utf-8";
            this.Context.Response.Write(ser.Serialize(response));
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
        public string Status { get; set; }
        public string CreatedDate { get; set; }
    }
}
#endregion