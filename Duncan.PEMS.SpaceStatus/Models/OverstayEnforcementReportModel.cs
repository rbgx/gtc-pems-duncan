﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

using OfficeOpenXml; // Namespace inside the open source EPPlus.dll from http://epplus.codeplex.com/
using OfficeOpenXml.Style;


using Duncan.PEMS.SpaceStatus.Models;
using Duncan.PEMS.SpaceStatus.DataMappers;
using Duncan.PEMS.SpaceStatus.DataShapes;
using Duncan.PEMS.SpaceStatus.DataSuppliers;
using Duncan.PEMS.SpaceStatus.UtilityClasses;
using RBACProvider;

namespace Duncan.PEMS.SpaceStatus.Models
{

    public class OccupancyEventWithOverstayViolation
    {
        public int MeterID { get; set; }
        public int BayID { get; set; }
        public int AreaID { get; set; }
        public DateTime DateTime_Start { get; set; }
        public DateTime DateTime_End { get; set; }
        public TimeSpan Duration { get; set; }
        public bool IsOccupied;
        public OverstayViolationInfo OverstayViolation { get; set; }
        public string EnforcementActionTaken { get; set; }
        public string EnforcementActionTakenByUser { get; set; }
        public DateTime EnforcementActionTakenTimeStamp { get; set; }


        public OccupancyEventWithOverstayViolation()
        {
            this.MeterID = -1;
            this.BayID = -1;
            this.AreaID = -1;
            this.DateTime_Start = DateTime.MinValue;
            this.DateTime_End = DateTime.MinValue;
            this.Duration = new TimeSpan();
            this.IsOccupied = false;
            this.OverstayViolation = null;
            this.EnforcementActionTaken = null;
            this.EnforcementActionTakenByUser = null;
            this.EnforcementActionTakenTimeStamp = DateTime.MinValue;
        }
    }

    public class OverstayEnforcementReportModel
    {
        public List<OccupancyEventWithOverstayViolation> OccupanciesWithOverstay = new List<OccupancyEventWithOverstayViolation>();

        public List<AreaAsset> AreasIncludedInReport = new List<AreaAsset>();
        public List<MeterAsset> MetersIncludedInReport = new List<MeterAsset>();
        public List<SpaceAsset> SpacesIncludedInReport = new List<SpaceAsset>();

        public bool AreaIsInListOfAreasIncludedInReport(int areaID)
        {
            return this.AreasIncludedInReport.Any(item => item.AreaID == areaID);
        }

        public bool MeterIsInListOfMetersIncludedInReport(int meterID)
        {
            return this.MetersIncludedInReport.Any(item => item.MeterID == meterID);
        }

        public bool SpaceIsInListOfSpacesIncludedInReport(int bayNumber, int meterID)
        {
            return this.SpacesIncludedInReport.Any(item => (item.MeterID == meterID && item.SpaceID == bayNumber));
        }

        public List<OccupancyEventWithOverstayViolation> FindRecsForArea(int areaID)
        {
            IEnumerable<OccupancyEventWithOverstayViolation> result = 
                from t1 in this.OccupanciesWithOverstay
                where t1.AreaID == areaID
                /*orderby t1.AreaID ascending*/
                select t1;
            return result.ToList();
        }

        public List<OccupancyEventWithOverstayViolation> FindRecsForMeter(int meterID)
        {
            IEnumerable<OccupancyEventWithOverstayViolation> result =
                from t1 in this.OccupanciesWithOverstay
                where t1.MeterID == meterID
                /*orderby t1.MeterID ascending*/
                select t1;
            return result.ToList();
        }

        public List<OccupancyEventWithOverstayViolation> FindRecsForBayAndMeter(int bayNumber, int meterID)
        {
            IEnumerable<OccupancyEventWithOverstayViolation> result =
                from t1 in this.OccupanciesWithOverstay
                where t1.MeterID == meterID && t1.BayID == bayNumber 
                /*orderby t1.MeterID ascending*/
                select t1;
            return result.ToList();
        }

        public OverstayGroupStats GetOverallStats()
        {
            OverstayGroupStats result = new OverstayGroupStats();
            foreach (OccupancyEventWithOverstayViolation overstayVio in this.OccupanciesWithOverstay)
            {
                result.OverstayCount++;
                result.TotalOverstayDuration = result.TotalOverstayDuration.Add(overstayVio.OverstayViolation.DurationOfTimeBeyondStayLimits);
            }
            return result;
        }

        public OverstayGroupStats GetMeterStats(int meterID)
        {
            OverstayGroupStats result = new OverstayGroupStats();
            List<OccupancyEventWithOverstayViolation> meterRecs = this.FindRecsForMeter(meterID);
            foreach (OccupancyEventWithOverstayViolation overstayVio in meterRecs)
            {
                result.OverstayCount++;
                result.TotalOverstayDuration = result.TotalOverstayDuration.Add(overstayVio.OverstayViolation.DurationOfTimeBeyondStayLimits);
            }
            return result;
        }

        public OverstayGroupStats GetAreaStats(int areaID)
        {
            OverstayGroupStats result = new OverstayGroupStats();
            List<OccupancyEventWithOverstayViolation> areaRecs = this.FindRecsForArea(areaID);
            foreach (OccupancyEventWithOverstayViolation overstayVio in areaRecs)
            {
                result.OverstayCount++;
                result.TotalOverstayDuration = result.TotalOverstayDuration.Add(overstayVio.OverstayViolation.DurationOfTimeBeyondStayLimits);
            }
            return result;
        }

        public OverstayGroupStats GetSpaceStats(int meterID, int bayNumber)
        {
            OverstayGroupStats result = new OverstayGroupStats();
            List<OccupancyEventWithOverstayViolation> spaceRecs = this.FindRecsForBayAndMeter(bayNumber, meterID);
            foreach (OccupancyEventWithOverstayViolation overstayVio in spaceRecs)
            {
                result.OverstayCount++;
                result.TotalOverstayDuration = result.TotalOverstayDuration.Add(overstayVio.OverstayViolation.DurationOfTimeBeyondStayLimits);
            }
            return result;
        }
    }


    public class OverstayEnforcementReportEngine
    {
        public enum EnforcementActivity
        {
            AllActivity, Actioned, NotActioned, Enforced, NotEnforced, Cautioned, Fault
        }

        public class OverstayEnforcementReportParameters
        {
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public bool IncludeAreaSummary { get; set; }
            public bool IncludeMeterSummary { get; set; }
            public bool IncludeSpaceSummary { get; set; }
            public bool IncludeHourlySummary { get; set; }
            public bool IncludeDailySummary { get; set; }
            public bool IncludeMonthlySummary { get; set; }
            public string ScopedAreaName { get; set; }
            public string ScopedMeter { get; set; }
            public EnforcementActivity ActivityType { get; set; }
        }

        #region Private/Protected Members
        protected CustomerConfig _CustomerConfig = null;
        protected List<SpaceAsset> _cachedSpaceAssets = null;

        protected OverstayEnforcementReportModel _OverstayReportModel = new OverstayEnforcementReportModel();
        private List<RBACProvider.RBACUserInfo> _AllUsers = null;
        private OverstayEnforcementReportParameters _ReportParams = null;
        #endregion

        #region Public Methods
        public OverstayEnforcementReportEngine(CustomerConfig customerCfg)
        {
            _CustomerConfig = customerCfg;
            _cachedSpaceAssets = CustomerLogic.CustomerManager.GetSpaceAssetsForCustomer(_CustomerConfig);
        }

        public void GetReportAsExcelSpreadsheet(List<int> listOfMeterIDs, MemoryStream ms, OverstayEnforcementReportParameters reportParams)
        {
            bool includeDetails = true;
            this._ReportParams = reportParams;

            // Start diagnostics timer
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            DateTime NowAtDestination = Convert.ToDateTime(this._CustomerConfig.DestinationTimeZoneDisplayName);

            // Gather a list of all users, because we will need to cross-reference UserIDs to UserNames
            this._AllUsers = MvcApplication1.RBAC.GetAllUserNamesAndIDs();
            CustomerLogic result = new CustomerLogic();
            // Now gather and analyze data for the report
            this.GatherReportData(listOfMeterIDs, reportParams.StartTime, reportParams.EndTime,  result);

            OfficeOpenXml.ExcelWorksheet ws = null;
            int rowIdx = -1;

            using (OfficeOpenXml.ExcelPackage pck = new OfficeOpenXml.ExcelPackage())
            {
                // Let's create a report coversheet and overall summary page, with hyperlinks to the other worksheets
                // Create the worksheet
                ws = pck.Workbook.Worksheets.Add("Summary");

                int numColumnsMergedForHeader = 10;

                // Render the header row
                rowIdx = 1; // Excel uses 1-based indexes
                ws.Cells[rowIdx, 1].Value = "Overstay Violation Enforcement Statistics Report";
                using (OfficeOpenXml.ExcelRange rng = ws.Cells[rowIdx, 1, rowIdx, numColumnsMergedForHeader])
                {
                    rng.Merge = true; //Merge columns start and end range
                    rng.Style.Font.Bold = true;
                    rng.Style.Font.Italic = true;
                    rng.Style.Font.Size = 22;
                    rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(23, 55, 93));
                    rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
                }

                rowIdx++;
                ws.Cells[rowIdx, 1].IsRichText = true;
                OfficeOpenXml.Style.ExcelRichTextCollection rtfCollection = ws.Cells[rowIdx, 1].RichText;
                AddRichTextNameAndValue(rtfCollection, "Client: ", this._CustomerConfig.CustomerName);
                AddRichTextNameAndValue(rtfCollection, ",  Generated: ", NowAtDestination.ToString("yyyy-MM-dd") + "  " + NowAtDestination.ToShortTimeString());
                ws.Cells[rowIdx, 1, rowIdx, numColumnsMergedForHeader].Merge = true;

                rowIdx++;
                rtfCollection = ws.Cells[rowIdx, 1].RichText;
                AddRichTextNameAndValue(rtfCollection, "Date Range:  ", reportParams.StartTime.ToString("yyyy-MM-dd") + "  " + reportParams.StartTime.ToShortTimeString());
                AddRichTextNameAndValue(rtfCollection, " to ", reportParams.EndTime.ToString("yyyy-MM-dd") + "  " + reportParams.EndTime.ToShortTimeString());
                ws.Cells[rowIdx, 1, rowIdx, numColumnsMergedForHeader].Merge = true;

                if (!string.IsNullOrEmpty(reportParams.ScopedAreaName))
                {
                    rowIdx++;
                    rtfCollection = ws.Cells[rowIdx, 1].RichText;
                    AddRichTextNameAndValue(rtfCollection, "Report limited to area: ", reportParams.ScopedAreaName);
                    ws.Cells[rowIdx, 1, rowIdx, numColumnsMergedForHeader].Merge = true;
                }

                if (!string.IsNullOrEmpty(reportParams.ScopedMeter))
                {
                    rowIdx++;
                    rtfCollection = ws.Cells[rowIdx, 1].RichText;
                    AddRichTextNameAndValue(rtfCollection, "Report limited to meter: ", reportParams.ScopedMeter);
                    ws.Cells[rowIdx, 1, rowIdx, numColumnsMergedForHeader].Merge = true;
                }


                rowIdx++;
                rtfCollection = ws.Cells[rowIdx, 1].RichText;
                if (reportParams.ActivityType == EnforcementActivity.AllActivity)
                {
                    AddRichTextNameAndValue(rtfCollection, "Included Activity: ", "All");
                    ws.Cells[rowIdx, 1, rowIdx, 10].Merge = true;
                }
                else if (reportParams.ActivityType == EnforcementActivity.Actioned)
                {
                    AddRichTextNameAndValue(rtfCollection, "Included Activity: ", "Actioned violations");
                    ws.Cells[rowIdx, 1, rowIdx, 10].Merge = true;
                }
                else if (reportParams.ActivityType == EnforcementActivity.NotActioned)
                {
                    AddRichTextNameAndValue(rtfCollection, "Included Activity: ", "Non-actioned violations");
                    ws.Cells[rowIdx, 1, rowIdx, 10].Merge = true;
                }
                else if (reportParams.ActivityType == EnforcementActivity.Enforced)
                {
                    AddRichTextNameAndValue(rtfCollection, "Included Activity: ", "Enforced violations");
                    ws.Cells[rowIdx, 1, rowIdx, 10].Merge = true;
                }
                else if (reportParams.ActivityType == EnforcementActivity.NotEnforced)
                {
                    AddRichTextNameAndValue(rtfCollection, "Included Activity: ", "Non-enforced violations");
                    ws.Cells[rowIdx, 1, rowIdx, 10].Merge = true;
                }
                else if (reportParams.ActivityType == EnforcementActivity.Cautioned)
                {
                    AddRichTextNameAndValue(rtfCollection, "Included Activity: ", "Cautions");
                    ws.Cells[rowIdx, 1, rowIdx, 10].Merge = true;
                }
                else if (reportParams.ActivityType == EnforcementActivity.Fault)
                {
                    AddRichTextNameAndValue(rtfCollection, "Included Activity: ", "Faults");
                    ws.Cells[rowIdx, 1, rowIdx, 10].Merge = true;
                }

                using (OfficeOpenXml.ExcelRange rng = ws.Cells[2, 1, rowIdx, numColumnsMergedForHeader])
                {
                    rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                    rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(207, 221, 237));  //Set color to lighter blue FromArgb(184, 204, 228)
                }


                rowIdx++;
                int hyperlinkstartRowIdx = rowIdx;

                if (reportParams.IncludeMeterSummary == true)
                {
                    rowIdx++;
                    ws.Cells[rowIdx, 1].Formula = "HYPERLINK(\"#'Meter Overstay'!A1\", \"Click here for Meter Overstay summary\")";
                    // Even though its a hyperlink, it won't look like one unless we style it
                    ws.Cells[rowIdx, 1].Style.Font.Color.SetColor(System.Drawing.Color.Blue);
                    ws.Cells[rowIdx, 1].Style.Font.UnderLine = true;
                    ws.Cells[rowIdx, 1, rowIdx, numColumnsMergedForHeader].Merge = true;
                }
                if (reportParams.IncludeSpaceSummary == true)
                {
                    rowIdx++;
                    ws.Cells[rowIdx, 1].Formula = "HYPERLINK(\"#'Space Overstay'!A1\", \"Click here for Space Overstay summary\")";
                    // Even though its a hyperlink, it won't look like one unless we style it
                    ws.Cells[rowIdx, 1].Style.Font.Color.SetColor(System.Drawing.Color.Blue);
                    ws.Cells[rowIdx, 1].Style.Font.UnderLine = true;
                    ws.Cells[rowIdx, 1, rowIdx, numColumnsMergedForHeader].Merge = true;
                }
                if (reportParams.IncludeAreaSummary == true)
                {
                    rowIdx++;
                    ws.Cells[rowIdx, 1].Formula = "HYPERLINK(\"#'Area Overstay'!A1\", \"Click here for Area Overstay summary\")";
                    //ws.Cells[rowIdx, 1].Hyperlink = new ExcelHyperLink("#'Area Overstay'!A1", "Click here to jump to Area Overstay summary"); 
                    // Even though its a hyperlink, it won't look like one unless we style it
                    ws.Cells[rowIdx, 1].Style.Font.Color.SetColor(System.Drawing.Color.Blue);
                    ws.Cells[rowIdx, 1].Style.Font.UnderLine = true;
                    ws.Cells[rowIdx, 1, rowIdx, numColumnsMergedForHeader].Merge = true;
                }
                if (includeDetails == true)
                {
                    rowIdx++;
                    ws.Cells[rowIdx, 1].Formula = "HYPERLINK(\"#'Details'!A1\", \"Click here for Overstay details\")";
                    // Even though its a hyperlink, it won't look like one unless we style it
                    ws.Cells[rowIdx, 1].Style.Font.Color.SetColor(System.Drawing.Color.Blue);
                    ws.Cells[rowIdx, 1].Style.Font.UnderLine = true;
                    ws.Cells[rowIdx, 1, rowIdx, numColumnsMergedForHeader].Merge = true;
                }

                rowIdx++;
                rowIdx++;

                using (OfficeOpenXml.ExcelRange rng = ws.Cells[hyperlinkstartRowIdx, 1, rowIdx, numColumnsMergedForHeader])
                {
                    rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                    rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                }


                ws.Cells[rowIdx, 1].Value = "Overall Summary";
                ws.Cells[rowIdx, 1, rowIdx, 5].Merge = true; //Merge columns start and end range
                ws.Cells[rowIdx, 1, rowIdx, 5].Style.Font.Bold = true; //Font should be bold
                ws.Cells[rowIdx, 1, rowIdx, 5].Style.Font.Size = 12;
                ws.Cells[rowIdx, 1, rowIdx, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Render the header row of overall summary
                rowIdx++;
                int overallSummaryHeaderRowIdx = rowIdx;
                ws.Cells[rowIdx, 1].Value = "Overstay Violation Count";
                ws.Cells[rowIdx, 2].Value = "Overstay Duration";

                // Format the header row
                using (OfficeOpenXml.ExcelRange rng = ws.Cells[rowIdx, 1, rowIdx, 2])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                    rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));  //Set color to dark blue
                    rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
                }

                // Increment the row index, which will now be the 1st row of our data
                rowIdx++;
                // We only have one row of data for Overall summary, so output it now
                OverstayGroupStats overallStats = this._OverstayReportModel.GetOverallStats();
                ws.Cells[rowIdx, 1].Value = overallStats.OverstayCount;
                ws.Cells[rowIdx, 2].Value = FormatTimeSpanAsHoursMinutesAndSeconds(overallStats.TotalOverstayDuration);

                // Column 1 is numeric integer
                ApplyNumberStyleToColumn(ws, 1, rowIdx, rowIdx, "########0", ExcelHorizontalAlignment.Right);

                // Column 2 should be aligned right
                ApplyNumberStyleToColumn(ws, 2, rowIdx, rowIdx, "@", ExcelHorizontalAlignment.Right);

                // And now lets size the columns
                for (int autoSizeColIdx = 1; autoSizeColIdx <= 2; autoSizeColIdx++)
                {
                    using (OfficeOpenXml.ExcelRange col = ws.Cells[overallSummaryHeaderRowIdx, autoSizeColIdx, rowIdx, autoSizeColIdx])
                    {
                        col.AutoFitColumns();
                    }
                }


                //  --- END OF OVERALL SUMMARY WORKSHEET ---

                // Should we include a worksheet with Meter aggregates?
                if (reportParams.IncludeMeterSummary == true)
                {
                    // Create the worksheet
                    ws = pck.Workbook.Worksheets.Add("Meter Overstay");

                    // Render the header row
                    rowIdx = 1; // Excel uses 1-based indexes
                    ws.Cells[rowIdx, 1].Value = "Meter #";
                    ws.Cells[rowIdx, 2].Value = "Overstay Violation Count";
                    ws.Cells[rowIdx, 3].Value = "Overstay Duration";

                    // Format the header row
                    using (OfficeOpenXml.ExcelRange rng = ws.Cells[1, 1, 1, 3])
                    {
                        rng.Style.Font.Bold = true;
                        rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                        rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));  //Set color to dark blue
                        rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    // Increment the row index, which will now be the 1st row of our data
                    rowIdx++;

                    // Loop for each meter
                    foreach (MeterAsset meterAsset in this._OverstayReportModel.MetersIncludedInReport)
                    {
                        // Collect the stats applicable to this meter
                        OverstayGroupStats meterStats = this._OverstayReportModel.GetMeterStats(meterAsset.MeterID);

                        // Output row values for data
                        ws.Cells[rowIdx, 1].Value = meterAsset.MeterID;
                        ws.Cells[rowIdx, 2].Value = meterStats.OverstayCount;
                        ws.Cells[rowIdx, 3].Value = FormatTimeSpanAsHoursMinutesAndSeconds(meterStats.TotalOverstayDuration);

                        // Increment the row index, which will now be the next row of our data
                        rowIdx++;
                    }

                    // We will add autofilters to our headers so user can sort the columns easier
                    using (OfficeOpenXml.ExcelRange rng = ws.Cells[1, 1, rowIdx, 3])
                    {
                        rng.AutoFilter = true;
                    }

                    // Apply formatting to the columns as appropriate (Starting row is 2 (first row of data), and ending row is the current rowIdx value)

                    // Column 1 is numeric integer (Meter ID)
                    ApplyNumberStyleToColumn(ws, 1, 2, rowIdx, "########0", ExcelHorizontalAlignment.Left);

                    // Column 2 is numeric integer
                    ApplyNumberStyleToColumn(ws, 2, 2, rowIdx, "########0", ExcelHorizontalAlignment.Right);

                    // Column 3 is text, but right-justified
                    ApplyNumberStyleToColumn(ws, 3, 2, rowIdx, "@", ExcelHorizontalAlignment.Right);

                    // And now lets size the columns
                    for (int autoSizeColIdx = 1; autoSizeColIdx <= 3; autoSizeColIdx++)
                    {
                        using (OfficeOpenXml.ExcelRange col = ws.Cells[1, autoSizeColIdx, rowIdx, autoSizeColIdx])
                        {
                            col.AutoFitColumns();
                        }
                    }
                }



                // Should we include a worksheet with Space aggregates?
                if (reportParams.IncludeSpaceSummary == true)
                {
                    // Create the worksheet
                    ws = pck.Workbook.Worksheets.Add("Space Overstay");

                    // Render the header row
                    rowIdx = 1; // Excel uses 1-based indexes
                    ws.Cells[rowIdx, 1].Value = "Space #";
                    ws.Cells[rowIdx, 2].Value = "Meter #";
                    ws.Cells[rowIdx, 3].Value = "Overstay Violation Count";
                    ws.Cells[rowIdx, 4].Value = "Overstay Duration";

                    // Format the header row
                    using (OfficeOpenXml.ExcelRange rng = ws.Cells[1, 1, 1, 4])
                    {
                        rng.Style.Font.Bold = true;
                        rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                        rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));  //Set color to dark blue
                        rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    // Increment the row index, which will now be the 1st row of our data
                    rowIdx++;

                    // Loop for each space
                    foreach (SpaceAsset spaceAsset in this._OverstayReportModel.SpacesIncludedInReport)
                    {
                        // Collect the stats applicable to this meter
                        OverstayGroupStats spaceStats = this._OverstayReportModel.GetSpaceStats(spaceAsset.MeterID, spaceAsset.SpaceID);

                        // Output row values for data
                        ws.Cells[rowIdx, 1].Value = spaceAsset.SpaceID;
                        ws.Cells[rowIdx, 2].Value = spaceAsset.MeterID;
                        ws.Cells[rowIdx, 3].Value = spaceStats.OverstayCount;
                        ws.Cells[rowIdx, 4].Value = FormatTimeSpanAsHoursMinutesAndSeconds(spaceStats.TotalOverstayDuration);

                        // Increment the row index, which will now be the next row of our data
                        rowIdx++;
                    }

                    // We will add autofilters to our headers so user can sort the columns easier
                    using (OfficeOpenXml.ExcelRange rng = ws.Cells[1, 1, rowIdx, 4])
                    {
                        rng.AutoFilter = true;
                    }

                    // Apply formatting to the columns as appropriate (Starting row is 2 (first row of data), and ending row is the current rowIdx value)

                    // Column 1 is numeric integer (Bay ID)
                    ApplyNumberStyleToColumn(ws, 1, 2, rowIdx, "########0", ExcelHorizontalAlignment.Left);

                    // Column 2 is numeric integer (Meter ID)
                    ApplyNumberStyleToColumn(ws, 2, 2, rowIdx, "########0", ExcelHorizontalAlignment.Left);

                    // Column 3 is numeric integer
                    ApplyNumberStyleToColumn(ws, 3, 2, rowIdx, "########0", ExcelHorizontalAlignment.Right);

                    // Column 4 is text, but right-justified
                    ApplyNumberStyleToColumn(ws, 4, 2, rowIdx, "@", ExcelHorizontalAlignment.Right);

                    // And now lets size the columns
                    for (int autoSizeColIdx = 1; autoSizeColIdx <= 4; autoSizeColIdx++)
                    {
                        using (OfficeOpenXml.ExcelRange col = ws.Cells[1, autoSizeColIdx, rowIdx, autoSizeColIdx])
                        {
                            col.AutoFitColumns();
                        }
                    }
                }


                // Should we include a worksheet with Area aggregates?
                if (reportParams.IncludeAreaSummary == true)
                {
                    // Create the worksheet
                    ws = pck.Workbook.Worksheets.Add("Area Overstay");

                    // Render the header row
                    rowIdx = 1; // Excel uses 1-based indexes
                    ws.Cells[rowIdx, 1].Value = "Area";
                    ws.Cells[rowIdx, 2].Value = "Overstay Violation Count";
                    ws.Cells[rowIdx, 3].Value = "Overstay Duration";

                    // Format the header row
                    using (OfficeOpenXml.ExcelRange rng = ws.Cells[1, 1, 1, 3])
                    {
                        rng.Style.Font.Bold = true;
                        rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                        rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));  //Set color to dark blue
                        rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    // Increment the row index, which will now be the 1st row of our data
                    rowIdx++;

                    // Loop for each space
                    foreach (AreaAsset areaAsset in this._OverstayReportModel.AreasIncludedInReport)
                    {
                        // Collect the stats applicable to this meter
                        OverstayGroupStats areaStats = this._OverstayReportModel.GetAreaStats(areaAsset.AreaID);

                        // Output row values for data
                        ws.Cells[rowIdx, 1].Value = areaAsset.AreaName;
                        ws.Cells[rowIdx, 2].Value = areaStats.OverstayCount;
                        ws.Cells[rowIdx, 3].Value = FormatTimeSpanAsHoursMinutesAndSeconds(areaStats.TotalOverstayDuration);

                        // Increment the row index, which will now be the next row of our data
                        rowIdx++;
                    }

                    // We will add autofilters to our headers so user can sort the columns easier
                    using (OfficeOpenXml.ExcelRange rng = ws.Cells[1, 1, rowIdx, 3])
                    {
                        rng.AutoFilter = true;
                    }

                    // Apply formatting to the columns as appropriate (Starting row is 2 (first row of data), and ending row is the current rowIdx value)

                    // Column 2 is numeric integer
                    ApplyNumberStyleToColumn(ws, 2, 2, rowIdx, "########0", ExcelHorizontalAlignment.Right);

                    // Column 3 is text, but right-justified
                    ApplyNumberStyleToColumn(ws, 3, 2, rowIdx, "@", ExcelHorizontalAlignment.Right);

                    // And now lets size the columns
                    for (int autoSizeColIdx = 1; autoSizeColIdx <= 3; autoSizeColIdx++)
                    {
                        using (OfficeOpenXml.ExcelRange col = ws.Cells[1, autoSizeColIdx, rowIdx, autoSizeColIdx])
                        {
                            col.AutoFitColumns();
                        }
                    }
                }


                // Should we include a worksheet with Hourly aggregates?
                if (reportParams.IncludeHourlySummary == true)
                {
                    // Create the worksheet
                    ws = pck.Workbook.Worksheets.Add("Hourly Overstay");

                    // Render the header row
                    rowIdx = 1; // Excel uses 1-based indexes
                    ws.Cells[rowIdx, 1].Value = "Hour";
                    ws.Cells[rowIdx, 2].Value = "Overstay Violation Count";
                    ws.Cells[rowIdx, 3].Value = "Overstay Duration";

                    // Format the header row
                    using (OfficeOpenXml.ExcelRange rng = ws.Cells[1, 1, 1, 3])
                    {
                        rng.Style.Font.Bold = true;
                        rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                        rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));  //Set color to dark blue
                        rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    // Increment the row index, which will now be the 1st row of our data
                    rowIdx++;

                    // Loop for each hour of a day
                    for (int hourIdx = 0; hourIdx <= 23; hourIdx++)
                    {
                        // Get aggregates for this hour
                        OverstayGroupStats aggregates = new OverstayGroupStats();
                        foreach (OccupancyEventWithOverstayViolation overstayVio in this._OverstayReportModel.OccupanciesWithOverstay)
                        {
                            if (overstayVio.OverstayViolation.DateTime_StartOfOverstayViolation.Hour == hourIdx)
                            {
                                aggregates.OverstayCount++;
                                aggregates.TotalOverstayDuration = aggregates.TotalOverstayDuration.Add(overstayVio.OverstayViolation.DurationOfTimeBeyondStayLimits);
                            }
                        }

                        
                            // Output row values for data
                            ws.Cells[rowIdx, 1].Value = hourIdx.ToString().PadLeft(2, '0') + ":00";
                            ws.Cells[rowIdx, 2].Value = aggregates.OverstayCount;
                            ws.Cells[rowIdx, 3].Value = FormatTimeSpanAsHoursMinutesAndSeconds(aggregates.TotalOverstayDuration);

                            // Increment the row index, which will now be the next row of our data
                            rowIdx++;
                    }

                    // We will add autofilters to our headers so user can sort the columns easier
                    using (OfficeOpenXml.ExcelRange rng = ws.Cells[1, 1, rowIdx, 3])
                    {
                        rng.AutoFilter = true;
                    }

                    // Apply formatting to the columns as appropriate (Starting row is 2 (first row of data), and ending row is the current rowIdx value)

                    // Column 1 is text
                    ApplyNumberStyleToColumn(ws, 1, 2, rowIdx, "@", ExcelHorizontalAlignment.Left);

                    // Column 2 is numeric integer
                    ApplyNumberStyleToColumn(ws, 2, 2, rowIdx, "########0", ExcelHorizontalAlignment.Right);

                    // Column 3 is text, but right-justified
                    ApplyNumberStyleToColumn(ws, 3, 2, rowIdx, "@", ExcelHorizontalAlignment.Right);

                    // And now lets size the columns
                    for (int autoSizeColIdx = 1; autoSizeColIdx <= 3; autoSizeColIdx++)
                    {
                        using (OfficeOpenXml.ExcelRange col = ws.Cells[1, autoSizeColIdx, rowIdx, autoSizeColIdx])
                        {
                            col.AutoFitColumns();
                        }
                    }
                }


                // Should we include a worksheet with Daily aggregates?
                if (reportParams.IncludeDailySummary == true)
                {
                    // Create the worksheet
                    ws = pck.Workbook.Worksheets.Add("Daily Overstay");

                    // Render the header row
                    rowIdx = 1; // Excel uses 1-based indexes
                    ws.Cells[rowIdx, 1].Value = "Date";
                    ws.Cells[rowIdx, 2].Value = "Overstay Violation Count";
                    ws.Cells[rowIdx, 3].Value = "Overstay Duration";

                    // Format the header row
                    using (OfficeOpenXml.ExcelRange rng = ws.Cells[1, 1, 1, 3])
                    {
                        rng.Style.Font.Bold = true;
                        rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                        rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));  //Set color to dark blue
                        rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    // Increment the row index, which will now be the 1st row of our data
                    rowIdx++;

                    // Loop for each date in the reportable range
                    DateTime targetDate = reportParams.StartTime.Date;
                    while (targetDate <= reportParams.EndTime.Date)
                    {
                        // Get aggregates for this date
                        OverstayGroupStats aggregates = new OverstayGroupStats();
                        foreach (OccupancyEventWithOverstayViolation overstayVio in this._OverstayReportModel.OccupanciesWithOverstay)
                        {
                            if (overstayVio.OverstayViolation.DateTime_StartOfOverstayViolation.Date.Ticks == targetDate.Ticks)
                            {
                                aggregates.OverstayCount++;
                                aggregates.TotalOverstayDuration = aggregates.TotalOverstayDuration.Add(overstayVio.OverstayViolation.DurationOfTimeBeyondStayLimits);
                            }
                        }

                        // Output row values for data
                        ws.Cells[rowIdx, 1].Value = targetDate.ToString("yyyy-MM-dd");
                        ws.Cells[rowIdx, 2].Value = aggregates.OverstayCount;
                        ws.Cells[rowIdx, 3].Value = FormatTimeSpanAsHoursMinutesAndSeconds(aggregates.TotalOverstayDuration);

                        // Increment the row index, which will now be the next row of our data
                        rowIdx++;

                        // Increment the target date
                        targetDate = targetDate.AddDays(1);
                    }

                    // We will add autofilters to our headers so user can sort the columns easier
                    using (OfficeOpenXml.ExcelRange rng = ws.Cells[1, 1, rowIdx, 3])
                    {
                        rng.AutoFilter = true;
                    }

                    // Apply formatting to the columns as appropriate (Starting row is 2 (first row of data), and ending row is the current rowIdx value)

                    // Column 1 is date (without time)
                    ApplyNumberStyleToColumn(ws, 1, 2, rowIdx, "yyyy-mm-dd", ExcelHorizontalAlignment.Right);

                    // Column 2 is numeric integer
                    ApplyNumberStyleToColumn(ws, 2, 2, rowIdx, "########0", ExcelHorizontalAlignment.Right);

                    // Column 3 is text, but right-justified
                    ApplyNumberStyleToColumn(ws, 3, 2, rowIdx, "@", ExcelHorizontalAlignment.Right);

                    // And now lets size the columns
                    for (int autoSizeColIdx = 1; autoSizeColIdx <= 3; autoSizeColIdx++)
                    {
                        using (OfficeOpenXml.ExcelRange col = ws.Cells[1, autoSizeColIdx, rowIdx, autoSizeColIdx])
                        {
                            col.AutoFitColumns();
                        }
                    }
                }

                // Should we include a worksheet with Monthly aggregates?
                if (reportParams.IncludeDailySummary == true)
                {
                    // Create the worksheet
                    ws = pck.Workbook.Worksheets.Add("Monthly Overstay");

                    // Render the header row
                    rowIdx = 1; // Excel uses 1-based indexes
                    ws.Cells[rowIdx, 1].Value = "Month";
                    ws.Cells[rowIdx, 2].Value = "Overstay Violation Count";
                    ws.Cells[rowIdx, 3].Value = "Overstay Duration";

                    // Format the header row
                    using (OfficeOpenXml.ExcelRange rng = ws.Cells[1, 1, 1, 3])
                    {
                        rng.Style.Font.Bold = true;
                        rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                        rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));  //Set color to dark blue
                        rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    // Increment the row index, which will now be the 1st row of our data
                    rowIdx++;

                    // Loop for each date in the reportable range
                    DateTime targetDate = reportParams.StartTime.Date;
                    while (targetDate <= reportParams.EndTime.Date)
                    {
                        // Get aggregates for this month
                        OverstayGroupStats aggregates = new OverstayGroupStats();
                        foreach (OccupancyEventWithOverstayViolation overstayVio in this._OverstayReportModel.OccupanciesWithOverstay)
                        {
                            if (overstayVio.OverstayViolation.DateTime_StartOfOverstayViolation.Date.Month == targetDate.Month)
                            {
                                aggregates.OverstayCount++;
                                aggregates.TotalOverstayDuration = aggregates.TotalOverstayDuration.Add(overstayVio.OverstayViolation.DurationOfTimeBeyondStayLimits);
                            }
                        }

                        // Output row values for data
                        ws.Cells[rowIdx, 1].Value = targetDate.ToString("MMM yyyy");
                        ws.Cells[rowIdx, 2].Value = aggregates.OverstayCount;
                        ws.Cells[rowIdx, 3].Value = FormatTimeSpanAsHoursMinutesAndSeconds(aggregates.TotalOverstayDuration);

                        // Increment the row index, which will now be the next row of our data
                        rowIdx++;

                        // Increment the target date by a month
                        targetDate = targetDate.AddMonths(1);
                    }

                    // We will add autofilters to our headers so user can sort the columns easier
                    using (OfficeOpenXml.ExcelRange rng = ws.Cells[1, 1, rowIdx, 3])
                    {
                        rng.AutoFilter = true;
                    }

                    // Apply formatting to the columns as appropriate (Starting row is 2 (first row of data), and ending row is the current rowIdx value)

                    // Column 1 is text
                    ApplyNumberStyleToColumn(ws, 1, 2, rowIdx, "@", ExcelHorizontalAlignment.Left);

                    // Column 2 is numeric integer
                    ApplyNumberStyleToColumn(ws, 2, 2, rowIdx, "########0", ExcelHorizontalAlignment.Right);

                    // Column 3 is text, but right-justified
                    ApplyNumberStyleToColumn(ws, 3, 2, rowIdx, "@", ExcelHorizontalAlignment.Right);

                    // And now lets size the columns
                    for (int autoSizeColIdx = 1; autoSizeColIdx <= 3; autoSizeColIdx++)
                    {
                        using (OfficeOpenXml.ExcelRange col = ws.Cells[1, autoSizeColIdx, rowIdx, autoSizeColIdx])
                        {
                            col.AutoFitColumns();
                        }
                    }
                }



                // Should we include a Details worksheet?
                if (includeDetails == true)
                {
                    // Create the worksheet
                    ws = pck.Workbook.Worksheets.Add("Details");

                    // Render the header row
                    rowIdx = 1; // Excel uses 1-based indexes
                    ws.Cells[rowIdx, 1].Value = "Space #";
                    ws.Cells[rowIdx, 2].Value = "Meter #";
                    ws.Cells[rowIdx, 3].Value = "Area";
                    ws.Cells[rowIdx, 4].Value = "Arrival";
                    ws.Cells[rowIdx, 5].Value = "Departure";
                    ws.Cells[rowIdx, 6].Value = "Duration of Overstay";
                    ws.Cells[rowIdx, 7].Value = "Violation Timestamp";
                    ws.Cells[rowIdx, 8].Value = "Action Taken";
                    ws.Cells[rowIdx, 9].Value = "Action Taken Timestamp";
                    ws.Cells[rowIdx, 10].Value = "Action Taken by User";
                    ws.Cells[rowIdx, 11].Value = "Overstay Rule";

                    // Format the header row
                    using (OfficeOpenXml.ExcelRange rng = ws.Cells[1, 1, 1, 11])
                    {
                        rng.Style.Font.Bold = true;
                        rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                        rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));  //Set color to dark blue
                        rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    // Increment the row index, which will now be the 1st row of our data
                    rowIdx++;

                    #region Populate data for each record
                    foreach (OccupancyEventWithOverstayViolation overstay in this._OverstayReportModel.OccupanciesWithOverstay)
                    {
                        // Output row values for data
                        ws.Cells[rowIdx, 1].Value = overstay.BayID;
                        ws.Cells[rowIdx, 2].Value = overstay.MeterID;
                        ws.Cells[rowIdx, 3].Value = ResolvedAreaNameForAreaID(overstay.AreaID);
                        ws.Cells[rowIdx, 4].Value = overstay.DateTime_Start.ToString("yyyy-MM-dd") + "  " + overstay.DateTime_Start.ToShortTimeString();
                        ws.Cells[rowIdx, 5].Value = overstay.DateTime_End.ToString("yyyy-MM-dd") + "  " + overstay.DateTime_End.ToShortTimeString();
                        ws.Cells[rowIdx, 6].Value = FormatTimeSpanAsHoursMinutesAndSeconds(overstay.OverstayViolation.DurationOfTimeBeyondStayLimits);

                        ws.Cells[rowIdx, 7].Value = overstay.OverstayViolation.DateTime_StartOfOverstayViolation.ToString("yyyy-MM-dd") + "  " + overstay.OverstayViolation.DateTime_StartOfOverstayViolation.ToShortTimeString();
                        if (!string.IsNullOrEmpty(overstay.EnforcementActionTaken))
                            ws.Cells[rowIdx, 8].Value = overstay.EnforcementActionTaken;
                        if (overstay.EnforcementActionTakenTimeStamp > DateTime.MinValue)
                            ws.Cells[rowIdx, 9].Value = overstay.EnforcementActionTakenTimeStamp.ToString("yyyy-MM-dd") + "  " + overstay.EnforcementActionTakenTimeStamp.ToShortTimeString();
                        if (!string.IsNullOrEmpty(overstay.EnforcementActionTakenByUser))
                            ws.Cells[rowIdx, 10].Value = overstay.EnforcementActionTakenByUser;
                        
                        StringBuilder sb = new StringBuilder();
                        sb.Append(Enum.ToObject(typeof(DayOfWeek), overstay.OverstayViolation.Regulation_DayOfWeek).ToString() + " ");
                        sb.Append(overstay.OverstayViolation.Regulation_StartTime.ToString("hh:mm:ss tt") + " - " +
                                overstay.OverstayViolation.Regulation_EndTime.ToString("hh:mm:ss tt") + ", ");
                        sb.Append(overstay.OverstayViolation.Regulation_Type + ", Max Stay: " + overstay.OverstayViolation.Regulation_MaxStayMinutes.ToString());
                        /*
                            if (overstay.OverstayViolation.OverstayBasedOnRuleGroup != null)
                            {
                                if ((overstay.OverstayBasedOnRuleGroup.MID.HasValue) && (overstay.OverstayBasedOnRuleGroup.MID.Value > -1))
                                    sb.Append(" (Meter-level regulations)");
                                else if ((overstay.OverstayBasedOnRuleGroup.AID.HasValue) && (overstay.OverstayBasedOnRuleGroup.AID.Value > -1))
                                    sb.Append(" (Area-level regulations)");
                                else if ((overstay.OverstayBasedOnRuleGroup.CID.HasValue) && (overstay.OverstayBasedOnRuleGroup.CID.Value > -1))
                                    sb.Append(" (Site-wide regulations)");
                            }
                        */
                        ws.Cells[rowIdx, 11].Value = sb.ToString();

                        // Increment the row index, which will now be the next row of our data
                        rowIdx++;
                    }
                    #endregion

                    // We will add autofilters to our headers so user can sort the columns easier
                    using (OfficeOpenXml.ExcelRange rng = ws.Cells[1, 1, rowIdx, 11])
                    {
                        rng.AutoFilter = true;
                    }

                    // Apply formatting to the columns as appropriate (Starting row is 2 (first row of data), and ending row is the current rowIdx value)

                    // Column 1 & 2 are numeric integer
                    ApplyNumberStyleToColumn(ws, 1, 2, rowIdx, "########0", ExcelHorizontalAlignment.Left);
                    ApplyNumberStyleToColumn(ws, 2, 2, rowIdx, "########0", ExcelHorizontalAlignment.Left);

                    ApplyNumberStyleToColumn(ws, 4, 2, rowIdx, "yyyy-mm-dd hh:mm:ss tt", ExcelHorizontalAlignment.Right);
                    ApplyNumberStyleToColumn(ws, 5, 2, rowIdx, "yyyy-mm-dd hh:mm:ss tt", ExcelHorizontalAlignment.Right);
                    ApplyNumberStyleToColumn(ws, 7, 2, rowIdx, "yyyy-mm-dd hh:mm:ss tt", ExcelHorizontalAlignment.Right);
                    ApplyNumberStyleToColumn(ws, 9, 2, rowIdx, "yyyy-mm-dd hh:mm:ss tt", ExcelHorizontalAlignment.Right);

                    ApplyNumberStyleToColumn(ws, 6, 2, rowIdx, "@", ExcelHorizontalAlignment.Right);

                    // And now lets size the columns
                    for (int autoSizeColIdx = 1; autoSizeColIdx <= 11; autoSizeColIdx++)
                    {
                        using (OfficeOpenXml.ExcelRange col = ws.Cells[1, autoSizeColIdx, rowIdx, autoSizeColIdx])
                        {
                            col.AutoFitColumns();
                        }
                    }
                }

                // All cells in spreadsheet are populated now, so render (save the file) to a memory stream 
                byte[] bytes = pck.GetAsByteArray();
                ms.Write(bytes, 0, bytes.Length);
            }


            // Stop diagnostics timer
            sw.Stop();
            System.Diagnostics.Debug.WriteLine("Overstay Report Generation took: " + sw.Elapsed.ToString());
        }
        #endregion

        #region Private/Protected Methods
        protected void GatherReportData(List<int> listOfMeterIDs, DateTime StartTime, DateTime EndTime, CustomerLogic result)
        {
            // Adjust the date ranges as needed for our SQL queries.
            // The end time needs to be inclusive of the entire minute (seconds and milliseconds are not in the resolution of the passed EndTime parameter)
            DateTime AdjustedStartTime;
            DateTime AdjustedEndTime;
            AdjustedStartTime = StartTime;
            AdjustedEndTime = new DateTime(EndTime.Year, EndTime.Month, EndTime.Day, EndTime.Hour, EndTime.Minute, 0).AddMinutes(1);

            // Gather all applicable vehicle sensing data (minimizes how many individual SQL queries must be executed)
            List<HistoricalSensingRecord> RawSensingDataForAllMeters = new SensingDatabaseSource(_CustomerConfig).GetHistoricalVehicleSensingDataForMeters_StronglyTyped(
                this._CustomerConfig.CustomerId, listOfMeterIDs, AdjustedStartTime, AdjustedEndTime, true);

            // Payment data is no applicable to this report, so we will just use an empty data set instead of getting from database
            List<PaymentRecord> RawPaymentDataForAllMeters = new List<PaymentRecord>();

            // Analyze data for each meter
            foreach (int nextMeterID in listOfMeterIDs)
            {
                AnalyzeDataForMeter(RawSensingDataForAllMeters, RawPaymentDataForAllMeters, nextMeterID, AdjustedStartTime, AdjustedEndTime,  result);
            }

            // Gather a unique list of Area and Meter assets involved in this report
            foreach (int nextMeterID in listOfMeterIDs)
            {
                if (this._OverstayReportModel.MeterIsInListOfMetersIncludedInReport(nextMeterID) == false)
                {
                    MeterAsset meterAsset = CustomerLogic.CustomerManager.GetMeterAsset(_CustomerConfig, nextMeterID);
                    if (meterAsset != null)
                        this._OverstayReportModel.MetersIncludedInReport.Add(meterAsset);
                }


                int resolvedAreaIDForMeterID = ResolveAreaIDForMeterID(nextMeterID);
                if (this._OverstayReportModel.AreaIsInListOfAreasIncludedInReport(resolvedAreaIDForMeterID) == false)
                {
                    AreaAsset areaAsset = this.GetAreaAssetByID(resolvedAreaIDForMeterID);
                    if (areaAsset != null)
                        this._OverstayReportModel.AreasIncludedInReport.Add(areaAsset);
                }
            }

            // Now sort the area, meter, and space asset lists
            this._OverstayReportModel.AreasIncludedInReport = this._OverstayReportModel.AreasIncludedInReport.OrderBy(e => e.AreaName).ToList();
            this._OverstayReportModel.MetersIncludedInReport = this._OverstayReportModel.MetersIncludedInReport.OrderBy(e => e.MeterID).ToList();
            this._OverstayReportModel.SpacesIncludedInReport = this._OverstayReportModel.SpacesIncludedInReport.OrderBy(e => e.SpaceID).ThenBy(e => e.MeterID).ToList();
        }

        protected void AnalyzeDataForMeter(List<HistoricalSensingRecord> allSensingData, List<PaymentRecord> allPaymentData, int meterId, DateTime startTime, DateTime endTime_NotInclusive, CustomerLogic result)
        {
            // Get list of all Spaces associated with the MeterID
            List<int> SortedBayIds = new List<int>();
            List<SpaceAsset> spaceAssets = CustomerLogic.CustomerManager.GetSpaceAssetsForMeter(_CustomerConfig, meterId);
            foreach (SpaceAsset asset in spaceAssets)
            {
                SortedBayIds.Add(asset.SpaceID);
            }
            SortedBayIds.Sort();

            // Process data for each space associated with the MeterID
            foreach (int nextBayID in SortedBayIds)
            {
                // Get filtered list of vehicle sensing and payment data that applies to the current meter and space
                List<HistoricalSensingRecord> VSDataForCurrentMeterAndBay = GetSensingDataSubsetForMeterIDAndBayID(allSensingData, meterId, nextBayID);
                List<PaymentRecord> PaymentDataForCurrentMeterAndBay = GetPaymentDataSubsetForMeterIDAndBayID(allPaymentData, meterId, nextBayID);

                // Analyze the data for this space
                AnalyzeDataForSpace(VSDataForCurrentMeterAndBay, PaymentDataForCurrentMeterAndBay,
                    nextBayID, meterId, nextBayID, startTime, endTime_NotInclusive, result);
            }
        }

        protected void AnalyzeDataForSpace(List<HistoricalSensingRecord> filteredSensingData, List<PaymentRecord> Payment, int legacyBitNumber,
            int meterId, int bayNumber, DateTime startTime, DateTime endTime_NotInclusive, CustomerLogic result)
        {
            OccupancyEventWithOverstayViolation currentVSEventToProcess = null;

            List<OccupancyEventWithOverstayViolation> spaceEventsAndDurations = new List<OccupancyEventWithOverstayViolation>();

            // Need to know current time in customer's timezone
            DateTime NowAtDestination = Convert.ToDateTime(this._CustomerConfig.DestinationTimeZoneDisplayName);

            if (this._OverstayReportModel.SpaceIsInListOfSpacesIncludedInReport(bayNumber, meterId) == false)
            {
                SpaceAsset spaceAsset = CustomerLogic.CustomerManager.GetSpaceAsset(_CustomerConfig, meterId, bayNumber);
                if (spaceAsset != null)
                    this._OverstayReportModel.SpacesIncludedInReport.Add(spaceAsset);
            }

            // Start Analyzing data here 
            foreach (HistoricalSensingRecord rawVSDataRec in filteredSensingData)
            {
                // The data should be filtered to the correct meter and bay, but we might as well check it
                if ((rawVSDataRec.MeterMappingId != meterId) || (rawVSDataRec.BayId != bayNumber))
                    continue;

                // Are we ready to begin a new object?
                if (currentVSEventToProcess == null)
                {
                    currentVSEventToProcess = new OccupancyEventWithOverstayViolation();
                    currentVSEventToProcess.MeterID = meterId;
                    currentVSEventToProcess.BayID = bayNumber;
                    currentVSEventToProcess.DateTime_Start = rawVSDataRec.DateTime;
                    currentVSEventToProcess.IsOccupied = rawVSDataRec.IsOccupied;

                    // Add newly created object to our list
                    spaceEventsAndDurations.Add(currentVSEventToProcess);

                    // Nothing more to do until we get to the next record (or end of records)
                    continue;
                }

                // Need to see if there is a change in occupancy status. If so, we need to finalize our previous object
                // and then start a new one
                if (currentVSEventToProcess.IsOccupied != rawVSDataRec.IsOccupied)
                {
                    currentVSEventToProcess.DateTime_End = rawVSDataRec.DateTime;
                    currentVSEventToProcess.Duration = (currentVSEventToProcess.DateTime_End - currentVSEventToProcess.DateTime_Start);

                    // Determine duration, and see if it qualifies as an overstay violation
                    AnalyzeOccupancyEventWithOverstayViolationForOverstayViolation(currentVSEventToProcess,  result);

                    // Finished getting data for previous event, so clear current object reference so a new one will be started
                    currentVSEventToProcess = null;

                    // JLA 2013-08-01: We must be careful not to skip events! This means the next event object needs to be started based on the current record, not the next!
                    currentVSEventToProcess = new OccupancyEventWithOverstayViolation();
                    currentVSEventToProcess.MeterID = meterId;
                    currentVSEventToProcess.BayID = bayNumber;
                    currentVSEventToProcess.DateTime_Start = rawVSDataRec.DateTime;
                    currentVSEventToProcess.IsOccupied = rawVSDataRec.IsOccupied;
                    // Add newly created object to our list
                    spaceEventsAndDurations.Add(currentVSEventToProcess);
                }
            }

            // No more records, so finalize our current object (if there is one)
            // We will need to assume an end time, but let's make sure it does't exceed current time (in customer timezone), or the report end time
            if (currentVSEventToProcess != null)
            {
                DateTime LesserOfCurrentTimeOrReportEndTime = NowAtDestination;
                if (NowAtDestination > endTime_NotInclusive)
                    LesserOfCurrentTimeOrReportEndTime = endTime_NotInclusive;

                currentVSEventToProcess.DateTime_End = LesserOfCurrentTimeOrReportEndTime;
                currentVSEventToProcess.Duration = (currentVSEventToProcess.DateTime_End - currentVSEventToProcess.DateTime_Start);

                // Need to determine duration, then see if we qualify as an overstay, etc...
                AnalyzeOccupancyEventWithOverstayViolationForOverstayViolation(currentVSEventToProcess,  result);
            }
        }

        protected List<HistoricalSensingRecord> GetSensingDataSubsetForMeterIDAndBayID(List<HistoricalSensingRecord> allSensingData, int meterID, int bayID)
        {
            List<HistoricalSensingRecord> result = new List<HistoricalSensingRecord>();
            foreach (HistoricalSensingRecord nextRec in allSensingData)
            {
                if ((nextRec.MeterMappingId == meterID) && (nextRec.BayId == bayID))
                    result.Add(nextRec);
            }
            return result;
        }

        protected List<PaymentRecord> GetPaymentDataSubsetForMeterIDAndBayID(List<PaymentRecord> allPaymentData, int meterID, int bayID)
        {
            List<PaymentRecord> result = new List<PaymentRecord>();
            foreach (PaymentRecord nextRec in allPaymentData)
            {
                if ((nextRec.MeterId == meterID) && (nextRec.BayNumber == bayID))
                    result.Add(nextRec);
            }
            return result;
        }

        protected AreaAsset GetAreaAssetByID(int areaID)
        {
            AreaAsset result = null;
            foreach(AreaAsset asset in CustomerLogic.CustomerManager.GetAreaAssetsForCustomer(_CustomerConfig))
            {
                if (asset.AreaID == areaID)
                {
                    result = asset;
                    break;
                }
            }
            return result;
        }

        protected int ResolveAreaIDForMeterID(int meterID)
        {
            int result = -1;
            foreach (MeterAsset asset in CustomerLogic.CustomerManager.GetMeterAssetsForCustomer(_CustomerConfig))
            {
                if (asset.MeterID == meterID)
                {
                    result = asset.AreaID_PreferLibertyBeforeInternal;
                    break;
                }
            }
            return result;
        }

        protected string ResolvedAreaNameForAreaID(int areaID)
        {
            string result = string.Empty;
            foreach (AreaAsset asset in CustomerLogic.CustomerManager.GetAreaAssetsForCustomer(_CustomerConfig))
            {
                if (asset.AreaID == areaID)
                {
                    result = asset.AreaName;
                    break;
                }
            }

            // If we don't have any name, we will just have to use the Area ID as the name instead
            if (string.IsNullOrEmpty(result))
                result = areaID.ToString();

            return result;
        }

        protected string FormatTimeSpan(TimeSpan T)
        {
            string SpanFormat = "H:mm:ss";

            int NoDays = Math.Abs(T.Days);
            int Hours = Math.Abs(T.Hours);
            int Minutes = Math.Abs(T.Minutes);
            int Seconds = Math.Abs(T.Seconds);

            DateTime TAsDateTime = new DateTime().AddDays(NoDays).AddHours(Hours).AddMinutes(Minutes).AddSeconds(Seconds);

            // if more than a whole day, include those in the display
            if (T.Days > 0)
            {
                return T.Days.ToString() + "d " + TAsDateTime.ToString(SpanFormat);
            }
            else
            {
                return TAsDateTime.ToString(SpanFormat);
            }
        }

        protected string FormatTimeSpanAsHoursMinutesAndSeconds(TimeSpan T)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            int NoDays = Math.Abs(T.Days);
            int Hours = Math.Abs(T.Hours);
            int Minutes = Math.Abs(T.Minutes);
            int Seconds = Math.Abs(T.Seconds);

            // Days doesn't make sense, so we want to convert days to accumulate in the hour instead
            if (NoDays > 0)
                Hours += (NoDays * 24);

            sb.Append(string.Format("{0:D2}:{1:D2}:{2:D2}", Hours, Minutes, Seconds));
            return sb.ToString();
        }

        protected void ApplyNumberStyleToColumn(ExcelWorksheet ws, int colIdx, int rowStartIdx, int rowEndIdx, string numberFormat, ExcelHorizontalAlignment horzAlign)
        {
            using (OfficeOpenXml.ExcelRange col = ws.Cells[rowStartIdx, colIdx, rowEndIdx, colIdx])
            {
                col.Style.Numberformat.Format = numberFormat;
                col.Style.HorizontalAlignment = horzAlign;
            }
        }

        protected void AddRichTextNameAndValue(OfficeOpenXml.Style.ExcelRichTextCollection rtfCollection, string name, string value)
        {
            OfficeOpenXml.Style.ExcelRichText ert = rtfCollection.Add(name);
            ert.Bold = false;
            ert.Color = System.Drawing.Color.Black;

            ert = rtfCollection.Add(value);
            ert.Bold = true;
            ert.Color = System.Drawing.Color.Blue;
        }

        protected void AnalyzeOccupancyEventWithOverstayViolationForOverstayViolation(OccupancyEventWithOverstayViolation currentVSEventToProcess, CustomerLogic result)
        {
            // There is nothing to do if this event is not for occupied status
            if (currentVSEventToProcess.IsOccupied == false)
                return;

            // Find the space asset associated with this data model.  If the space is "inactive" (based on the "IsActive" column of "HousingMaster" table in database),
            // then we will not consider the space to be in a violating state, because the sensor is effectively marked as bad/untrusted
            SpaceAsset spcAsset = GetSpaceAsset(currentVSEventToProcess.MeterID, currentVSEventToProcess.BayID);
            if (spcAsset != null)
            {
                // Nothing more to do if the space isn't active
                if (spcAsset.IsActive == false)
                    return;
            }

            // Resolve the associated area for the meter
            int areaID = ResolveAreaIDForMeterID(currentVSEventToProcess.MeterID);

            // Try to obtain the regulated hours applicable to this meter
            RegulatedHoursGroup regulatedHours = RegulatedHoursGroupRepository.Repository.GetBestGroupForMeter(this._CustomerConfig.CustomerId, areaID, currentVSEventToProcess.MeterID);

            // If no regulated hour defintions came back, then we are unable to calculate any overstay violation, so just exit
            if ((regulatedHours == null) || (regulatedHours.Details == null) || (regulatedHours.Details.Count == 0))
                return;

            DateTime ruleStart = DateTime.MinValue;
            DateTime ruleEnd = DateTime.MinValue;

            TimeSlot OccupiedSegment = new TimeSlot(currentVSEventToProcess.DateTime_Start, currentVSEventToProcess.DateTime_End);

            // We need to check if this single occupancy event is an overstay violation for multiple rules, or even for more than one day, etc.
            while (OccupiedSegment.Start < currentVSEventToProcess.DateTime_End)
            {
                // Determine the day of week that is involved
                int dayOfWeek = (int)OccupiedSegment.Start.DayOfWeek;

                // Loop through the daily rules and see which ones overlap with our occupied period
                foreach (RegulatedHoursDetail detail in regulatedHours.Details)
                {
                    // Skip this one if its for a different day of the week
                    if (detail.DayOfWeek != dayOfWeek)
                        continue;

                    // Determine if the occupied timeslot overlaps with the rule's timeslot
                    ruleStart = new DateTime(OccupiedSegment.Start.Year, OccupiedSegment.Start.Month, OccupiedSegment.Start.Day, detail.StartTime.Hour, detail.StartTime.Minute, 0);
                    ruleEnd = new DateTime(OccupiedSegment.Start.Year, OccupiedSegment.Start.Month, OccupiedSegment.Start.Day, detail.EndTime.Hour, detail.EndTime.Minute, 59);
                    TimeSlot RuleSegment = new TimeSlot(ruleStart, ruleEnd);

                    // We only care about this overlapping rule if the MaxStayMinutes is greater than zero (zero or less means there is no MaxStay that is enforced),
                    // or if it's explicitly set as a "No Parking" regulation
                    if ((RuleSegment.OverlapsWith(OccupiedSegment) == true) &&
                         ((detail.MaxStayMinutes > 0) || (string.Compare(detail.Type, "No Parking", true) == 0))
                       )
                    {
                        // Normally we will use the verbatim value of the max stay minutes, but if its a "No Parking", we will always take that to mean 0 minutes is the actual max
                        int timeLimitMinutes = detail.MaxStayMinutes;
                        if (string.Compare(detail.Type, "No Parking", true) == 0)
                            timeLimitMinutes = 0;

                        // Get the intersection of the overlaps so we know how long the vehicle has been occupied during this rule
                        TimeSlot OccupiedIntersection = RuleSegment.GetIntersection(OccupiedSegment);

                        // Determine if the vehicle has been occupied during this rule segment in excess of the MaxStayMinutes
                        if (OccupiedIntersection != null)
                        {
                            if (OccupiedIntersection.Duration.TotalMinutes >= timeLimitMinutes)
                            {
                                // Since this event might qualify for multiple overstay violations, we must clone the object instead of manipulating the same one
                                OccupancyEventWithOverstayViolation reportableEvent = new OccupancyEventWithOverstayViolation();
                                reportableEvent.AreaID = areaID;
                                reportableEvent.MeterID = currentVSEventToProcess.MeterID;
                                reportableEvent.BayID = currentVSEventToProcess.BayID;
                                reportableEvent.DateTime_Start = currentVSEventToProcess.DateTime_Start;
                                reportableEvent.DateTime_End = currentVSEventToProcess.DateTime_End;
                                reportableEvent.Duration = currentVSEventToProcess.Duration;
                                reportableEvent.IsOccupied = currentVSEventToProcess.IsOccupied;

                                OverstayViolationInfo overstay = new OverstayViolationInfo();
                                reportableEvent.OverstayViolation = overstay;
                                overstay.Regulation_Date = new DateTime(OccupiedSegment.Start.Year, OccupiedSegment.Start.Month, OccupiedSegment.Start.Day);
                                overstay.Regulation_DayOfWeek = detail.DayOfWeek;
                                overstay.Regulation_EndTime = detail.EndTime;
                                overstay.Regulation_MaxStayMinutes = detail.MaxStayMinutes; // Instead of using our calculated time limit, we will record the configured max stay minutes here, because it will be displayed
                                overstay.Regulation_StartTime = detail.StartTime;
                                overstay.Regulation_Type = detail.Type;
                                overstay.DateTime_StartOfOverstayViolation = new DateTime(OccupiedIntersection.Start.Ticks).AddMinutes(timeLimitMinutes);
                                overstay.DurationOfTimeBeyondStayLimits = new TimeSpan(OccupiedIntersection.Duration.Ticks).Add(new TimeSpan(0, (-1) * timeLimitMinutes, 0));

                                // Now let's see if there was an action taken during this violation period
                                DateTime minDateCutoff = overstay.DateTime_StartOfOverstayViolation;

                                // Get the most recent action taken for this space asset
                                OverstayVioActionsDTO ActionTakenDTO = result.GetVioActionForSpaceDuringTimeRange(this._CustomerConfig.CustomerId, reportableEvent.MeterID, reportableEvent.AreaID,
                                    reportableEvent.BayID, overstay.DateTime_StartOfOverstayViolation, reportableEvent.DateTime_End);
                                if ((ActionTakenDTO != null) && (minDateCutoff > DateTime.MinValue))
                                {
                                    // If the action taken was recorded after the start of this violation, then it qualifies as being the action taken for this
                                    // violation condition. If so, retain the "Action Taken" reason
                                    if (ActionTakenDTO.EventTimestamp >= minDateCutoff)
                                    {
                                        reportableEvent.EnforcementActionTaken = ActionTakenDTO.ActionTaken;
                                        reportableEvent.EnforcementActionTakenTimeStamp = ActionTakenDTO.EventTimestamp;

                                        try
                                        {
                                            // Try to resolve the user associated with this action taken
                                            RBACProvider.RBACUserInfo actionTakenByUser = this._AllUsers.Find(item => item.DBUserCustomSID_AsInt == ActionTakenDTO.RBACUserID);
                                            if (actionTakenByUser != null)
                                            {
                                                // Get the username, then chop off the domain info
                                                string username = actionTakenByUser.UserName;
                                                username = Duncan.PEMS.SpaceStatus.Models.LogOnModel.RemoveDomainFromUsername(username);
                                                reportableEvent.EnforcementActionTakenByUser = username;
                                            }
                                        }
                                        catch { }
                                    }
                                }

                                // Since this has been evaluated as an overstay violation, we will add it to the list that the report will use,
                                // but only if it also qualifies for whatever "Action Taken" filtering is in effect
                                bool activityTypeMatchesFiltering = false;
                                if ( 
                                    (this._ReportParams.ActivityType == EnforcementActivity.AllActivity) ||
                                    ((this._ReportParams.ActivityType == EnforcementActivity.Actioned) && (!string.IsNullOrEmpty(reportableEvent.EnforcementActionTaken)))
                                   )
                                {
                                    activityTypeMatchesFiltering = true;
                                }
                                else if ( 
                                    ((this._ReportParams.ActivityType == EnforcementActivity.Cautioned) && (!string.IsNullOrEmpty(reportableEvent.EnforcementActionTaken)) &&
                                    (string.Compare(reportableEvent.EnforcementActionTaken, "Cautioned", true) == 0))
                                   )
                                {
                                    activityTypeMatchesFiltering = true;
                                }
                                else if ( 
                                    ((this._ReportParams.ActivityType == EnforcementActivity.Enforced) && (!string.IsNullOrEmpty(reportableEvent.EnforcementActionTaken)) &&
                                    (string.Compare(reportableEvent.EnforcementActionTaken, "Enforced", true) == 0))
                                   )
                                {
                                    activityTypeMatchesFiltering = true;
                                }
                                else if ( 
                                    ((this._ReportParams.ActivityType == EnforcementActivity.Fault) && (!string.IsNullOrEmpty(reportableEvent.EnforcementActionTaken)) &&
                                    (string.Compare(reportableEvent.EnforcementActionTaken, "Fault", true) == 0))
                                   )
                                {
                                    activityTypeMatchesFiltering = true;
                                }
                                else if ( 
                                    ((this._ReportParams.ActivityType == EnforcementActivity.NotEnforced) && (!string.IsNullOrEmpty(reportableEvent.EnforcementActionTaken)) &&
                                    (string.Compare(reportableEvent.EnforcementActionTaken, "NotEnforced", true) == 0))
                                   )
                                {
                                    activityTypeMatchesFiltering = true;
                                }
                                else if ( 
                                    ((this._ReportParams.ActivityType == EnforcementActivity.NotActioned) && (string.IsNullOrEmpty(reportableEvent.EnforcementActionTaken) == true))
                                   )
                                {
                                    activityTypeMatchesFiltering = true;
                                }


                                if (activityTypeMatchesFiltering == true)
                                    _OverstayReportModel.OccupanciesWithOverstay.Add(reportableEvent);
                            }
                        }
                    }
                }

                // Rules for current day of week have been processed.  So now we will advance to beginning of next day and see if there are more violations that we will use
                // to add accumulated time in violation state...
                OccupiedSegment = new TimeSlot(new DateTime(OccupiedSegment.Start.Year, OccupiedSegment.Start.Month, OccupiedSegment.Start.Day).AddDays(1),
                    currentVSEventToProcess.DateTime_End);
            }
        }

        protected SpaceAsset GetSpaceAsset(int meterID, int spaceID)
        {
            SpaceAsset result = null;

            // If we have a list of cached assets, look through it, which should be faster than constantly asking the customer manager for it
            // since we would have a local copy and not need to worry about concurrency issues
            if (_cachedSpaceAssets != null)
            {
                foreach (SpaceAsset asset in _cachedSpaceAssets)
                {
                    if ((asset.MeterID == meterID) && (asset.SpaceID == spaceID))
                    {
                        result = asset;
                        break;
                    }
                }
            }
            else
            {
                result = CustomerLogic.CustomerManager.GetSpaceAsset(_CustomerConfig, meterID, spaceID);
            }

            return result;
        }

        #endregion
    }

}
