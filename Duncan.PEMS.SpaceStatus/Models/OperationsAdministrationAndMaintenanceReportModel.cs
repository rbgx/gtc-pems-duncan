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


using Duncan.PEMS.SpaceStatus.DataShapes;
using Duncan.PEMS.SpaceStatus.DataSuppliers;
using Duncan.PEMS.SpaceStatus.UtilityClasses;

namespace Duncan.PEMS.SpaceStatus.Models
{

    public class OpsAdminMaintExceptionsReportParameters
    {
        public bool ReportOnHistoricalData { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string ScopedAreaName { get; set; }
        public string ScopedMeter { get; set; }

        public bool IncludeDetails { get; set; }
        public bool IncludeAreaSummary { get; set; }
        public bool IncludeMeterSummary { get; set; }
        public bool IncludeSpaceSummary { get; set; }

        public OpsAdminMaintExceptionsReportParameters()
        {
            ReportOnHistoricalData = false; // False means current data, True means analyze historical records
            StartTime = DateTime.MinValue;
            EndTime = DateTime.MaxValue;
            ScopedAreaName = string.Empty;
            ScopedMeter = string.Empty;

            IncludeDetails = true;
            IncludeAreaSummary = true;
            IncludeMeterSummary = true;
            IncludeSpaceSummary = true;
        }
    }

    public class OpsAdminMaintExceptionsStats
    {
        public int NoRecentCommsCount { get; set; }
        public int NoRecentSensorEventsCount { get; set; }
        public int LowBatteryCount { get; set; }

        public OpsAdminMaintExceptionsStats()
        {
            InitValues();
        }

        protected void InitValues()
        {
            NoRecentCommsCount = 0;
            NoRecentSensorEventsCount = 0;
            LowBatteryCount = 0;
        }
    }

    public class OpsAdminMaintExceptionsStats_BayAndMeter : OpsAdminMaintExceptionsStats
    {
        public int MeterID { get; set; }
        public int BayID { get; set; }

        public OpsAdminMaintExceptionsStats_BayAndMeter()
            : base()
        {
        }
    }

    public class OpsAdminMaintExceptionsStats_Meter : OpsAdminMaintExceptionsStats
    {
        public int MeterID { get; set; }

        public OpsAdminMaintExceptionsStats_Meter()
            : base()
        {
        }
    }

    public class OpsAdminMaintExceptionsStats_Area : OpsAdminMaintExceptionsStats
    {
        public int AreaID { get; set; }
        public string AreaName { get; set; }

        public OpsAdminMaintExceptionsStats_Area()
            : base()
        {
        }

        private int GetAreaIDForMeterID(int meterID, CustomerConfig customerCfg)
        {
            int result = -1;
            foreach (MeterAsset asset in CustomerLogic.CustomerManager.GetMeterAssetsForCustomer(customerCfg))
            {
                if (asset.MeterID == meterID)
                {
                    result = asset.AreaID_PreferLibertyBeforeInternal;
                    break;
                }
            }
            return result;
        }
    }

    public class OperationsAdministrationAndMaintenanceReportModel
    {
        public List<SensorEventAndCommsRecord> SensorsWithoutRecentOccupancyEvents = new List<SensorEventAndCommsRecord>();
        public List<SensorHeartbeatRecord> SensorsWithoutRecentComms = new List<SensorHeartbeatRecord>();
        public List<SensorBatteryDiagnostics> SensorsWithLowBatteries = new List<SensorBatteryDiagnostics>();

        public OpsAdminMaintExceptionsStats OverallGroupStats = new OpsAdminMaintExceptionsStats();
        public List<OpsAdminMaintExceptionsStats_BayAndMeter> BayStats = new List<OpsAdminMaintExceptionsStats_BayAndMeter>();
        public List<OpsAdminMaintExceptionsStats_Meter> MeterStats = new List<OpsAdminMaintExceptionsStats_Meter>();
        public List<OpsAdminMaintExceptionsStats_Area> AreaStats = new List<OpsAdminMaintExceptionsStats_Area>();
    }


    #region Report statistic sorters
    public sealed class OpsAdminMaintExceptionsStats_AreaLogicalComparer : System.Collections.Generic.IComparer<OpsAdminMaintExceptionsStats_Area>
    {
        private static readonly System.Collections.Generic.IComparer<OpsAdminMaintExceptionsStats_Area> _default = new OpsAdminMaintExceptionsStats_AreaLogicalComparer(true);
        private bool _SortByName = false;

        public OpsAdminMaintExceptionsStats_AreaLogicalComparer(bool SortByName)
        {
            _SortByName = SortByName;
        }

        public static System.Collections.Generic.IComparer<OpsAdminMaintExceptionsStats_Area> Default
        {
            get { return _default; }
        }

        public int Compare(OpsAdminMaintExceptionsStats_Area s1, OpsAdminMaintExceptionsStats_Area s2)
        {
            // Are we sorting by Name or ID?
            if (this._SortByName == true)
                return string.CompareOrdinal(s1.AreaName, s2.AreaName);
            else
                return s1.AreaID.CompareTo(s2.AreaID);
        }
    }

    public sealed class OpsAdminMaintExceptionsStats_BayAndMeterLogicalComparer : System.Collections.Generic.IComparer<OpsAdminMaintExceptionsStats_BayAndMeter>
    {
        private static readonly System.Collections.Generic.IComparer<OpsAdminMaintExceptionsStats_BayAndMeter> _default = new OpsAdminMaintExceptionsStats_BayAndMeterLogicalComparer();

        public OpsAdminMaintExceptionsStats_BayAndMeterLogicalComparer()
        {
        }

        public static System.Collections.Generic.IComparer<OpsAdminMaintExceptionsStats_BayAndMeter> Default
        {
            get { return _default; }
        }

        public int Compare(OpsAdminMaintExceptionsStats_BayAndMeter s1, OpsAdminMaintExceptionsStats_BayAndMeter s2)
        {
            // We are sorting primarly by BayID, and then by MeterID
            int s1_ID = s1.BayID;
            int s2_ID = s2.BayID;

            // Compare the Bay IDs
            int result = s1_ID.CompareTo(s2_ID);

            // If Bay IDs are the same, then compare the MeterIDs
            if (result == 0)
            {
                s1_ID = s1.MeterID;
                s2_ID = s2.MeterID;
                result = s1_ID.CompareTo(s2_ID);
            }

            return result;
        }
    }

    public sealed class OpsAdminMaintExceptionsStats_MeterLogicalComparer : System.Collections.Generic.IComparer<OpsAdminMaintExceptionsStats_Meter>
    {
        private static readonly System.Collections.Generic.IComparer<OpsAdminMaintExceptionsStats_Meter> _default = new OpsAdminMaintExceptionsStats_MeterLogicalComparer();

        public OpsAdminMaintExceptionsStats_MeterLogicalComparer()
        {
        }

        public static System.Collections.Generic.IComparer<OpsAdminMaintExceptionsStats_Meter> Default
        {
            get { return _default; }
        }

        public int Compare(OpsAdminMaintExceptionsStats_Meter s1, OpsAdminMaintExceptionsStats_Meter s2)
        {
            int s1_ID = s1.MeterID;
            int s2_ID = s2.MeterID;
            int result = s1_ID.CompareTo(s2_ID);
            return result;
        }
    }
    #endregion


    public class OpsAdminMaintExceptionsReportEngine
    {
        #region Private/Protected Members
        protected CustomerConfig _CustomerConfig = null;

        protected OperationsAdministrationAndMaintenanceReportModel _ReportModel = new OperationsAdministrationAndMaintenanceReportModel();
        #endregion

        #region Public Methods
        public OpsAdminMaintExceptionsReportEngine(CustomerConfig customerCfg)
        {
            _CustomerConfig = customerCfg;
        }

        public void GetReportAsExcelSpreadsheet(List<int> listOfMeterIDs, MemoryStream ms, OpsAdminMaintExceptionsReportParameters reportParams)
        {
            // Start diagnostics timer
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            DateTime NowAtDestination = Convert.ToDateTime(this._CustomerConfig.DestinationTimeZoneDisplayName);

            this.GatherReportData(listOfMeterIDs, reportParams);

            OfficeOpenXml.ExcelWorksheet ws = null;
            int rowIdx = -1;

            using (OfficeOpenXml.ExcelPackage pck = new OfficeOpenXml.ExcelPackage())
            {
                // Let's create a report coversheet and overall summary page, with hyperlinks to the other worksheets
                // Create the worksheet
                ws = pck.Workbook.Worksheets.Add("Summary");

                // Render the header row
                rowIdx = 1; // Excel uses 1-based indexes
                ws.Cells[rowIdx, 1].Value = "Operations, Administration and Maintenance Exceptions Report";
                using (OfficeOpenXml.ExcelRange rng = ws.Cells[rowIdx, 1, rowIdx, 11])
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
                ws.Cells[rowIdx, 1, rowIdx, 11].Merge = true;

                // Header needs to reflect if the report was run for current status or historical records
                if (reportParams.ReportOnHistoricalData == true)
                {
                    rowIdx++;
                    rtfCollection = ws.Cells[rowIdx, 1].RichText;
                    AddRichTextNameAndValue(rtfCollection, "Report Type:  ", "Historical Data");
                    ws.Cells[rowIdx, 1, rowIdx, 11].Merge = true;

                    rowIdx++;
                    rtfCollection = ws.Cells[rowIdx, 1].RichText;
                    AddRichTextNameAndValue(rtfCollection, "Date Range:  ", reportParams.StartTime.ToString("yyyy-MM-dd") + "  " + reportParams.StartTime.ToShortTimeString());
                    AddRichTextNameAndValue(rtfCollection, " to ", reportParams.EndTime.ToString("yyyy-MM-dd") + "  " + reportParams.EndTime.ToShortTimeString());
                    ws.Cells[rowIdx, 1, rowIdx, 11].Merge = true;
                }
                else
                {
                    rowIdx++;
                    rtfCollection = ws.Cells[rowIdx, 1].RichText;
                    AddRichTextNameAndValue(rtfCollection, "Report Type:  ", "Current Status");
                    ws.Cells[rowIdx, 1, rowIdx, 11].Merge = true;
                }

                if (!string.IsNullOrEmpty(reportParams.ScopedAreaName))
                {
                    rowIdx++;
                    rtfCollection = ws.Cells[rowIdx, 1].RichText;
                    AddRichTextNameAndValue(rtfCollection, "Report limited to area: ", reportParams.ScopedAreaName);
                    ws.Cells[rowIdx, 1, rowIdx, 11].Merge = true;
                }

                if (!string.IsNullOrEmpty(reportParams.ScopedMeter))
                {
                    rowIdx++;
                    rtfCollection = ws.Cells[rowIdx, 1].RichText;
                    AddRichTextNameAndValue(rtfCollection, "Report limited to meter: ", reportParams.ScopedMeter);
                    ws.Cells[rowIdx, 1, rowIdx, 11].Merge = true;
                }


                using (OfficeOpenXml.ExcelRange rng = ws.Cells[2, 1, rowIdx, 11])
                {
                    rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                    rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(207, 221, 237));  //Set color to lighter blue FromArgb(184, 204, 228)
                }


                rowIdx++;
                int hyperlinkstartRowIdx = rowIdx;

                if (reportParams.IncludeMeterSummary == true)
                {
                    rowIdx++;
                    ws.Cells[rowIdx, 1].Formula = "HYPERLINK(\"#'Meter Summary'!A1\", \"Click here for Meter summary\")";
                    // Even though its a hyperlink, it won't look like one unless we style it
                    ws.Cells[rowIdx, 1].Style.Font.Color.SetColor(System.Drawing.Color.Blue);
                    ws.Cells[rowIdx, 1].Style.Font.UnderLine = true;
                    ws.Cells[rowIdx, 1, rowIdx, 11].Merge = true;
                }
                if (reportParams.IncludeSpaceSummary == true)
                {
                    rowIdx++;
                    ws.Cells[rowIdx, 1].Formula = "HYPERLINK(\"#'Space Summary'!A1\", \"Click here for Space summary\")";
                    // Even though its a hyperlink, it won't look like one unless we style it
                    ws.Cells[rowIdx, 1].Style.Font.Color.SetColor(System.Drawing.Color.Blue);
                    ws.Cells[rowIdx, 1].Style.Font.UnderLine = true;
                    ws.Cells[rowIdx, 1, rowIdx, 11].Merge = true;
                }
                if (reportParams.IncludeAreaSummary == true)
                {
                    rowIdx++;
                    ws.Cells[rowIdx, 1].Formula = "HYPERLINK(\"#'Area Summary'!A1\", \"Click here for Area summary\")";
                    //ws.Cells[rowIdx, 1].Hyperlink = new ExcelHyperLink("#'Area Overstay'!A1", "Click here to jump to Area Overstay summary"); 
                    // Even though its a hyperlink, it won't look like one unless we style it
                    ws.Cells[rowIdx, 1].Style.Font.Color.SetColor(System.Drawing.Color.Blue);
                    ws.Cells[rowIdx, 1].Style.Font.UnderLine = true;
                    ws.Cells[rowIdx, 1, rowIdx, 11].Merge = true;
                }
                if (reportParams.IncludeDetails == true)
                {
                    rowIdx++;
                    ws.Cells[rowIdx, 1].Formula = "HYPERLINK(\"#'Sensor Status Details'!A1\", \"Click here for Sensor status details\")";
                    // Even though its a hyperlink, it won't look like one unless we style it
                    ws.Cells[rowIdx, 1].Style.Font.Color.SetColor(System.Drawing.Color.Blue);
                    ws.Cells[rowIdx, 1].Style.Font.UnderLine = true;
                    ws.Cells[rowIdx, 1, rowIdx, 11].Merge = true;

                    rowIdx++;
                    ws.Cells[rowIdx, 1].Formula = "HYPERLINK(\"#'Sensor Communication Details'!A1\", \"Click here for Sensor communication details\")";
                    // Even though its a hyperlink, it won't look like one unless we style it
                    ws.Cells[rowIdx, 1].Style.Font.Color.SetColor(System.Drawing.Color.Blue);
                    ws.Cells[rowIdx, 1].Style.Font.UnderLine = true;
                    ws.Cells[rowIdx, 1, rowIdx, 11].Merge = true;

                    rowIdx++;
                    ws.Cells[rowIdx, 1].Formula = "HYPERLINK(\"#'Sensor Low Battery Details'!A1\", \"Click here for Sensor battery details\")";
                    // Even though its a hyperlink, it won't look like one unless we style it
                    ws.Cells[rowIdx, 1].Style.Font.Color.SetColor(System.Drawing.Color.Blue);
                    ws.Cells[rowIdx, 1].Style.Font.UnderLine = true;
                    ws.Cells[rowIdx, 1, rowIdx, 11].Merge = true;
                }

                rowIdx++;
                rowIdx++;

                using (OfficeOpenXml.ExcelRange rng = ws.Cells[hyperlinkstartRowIdx, 1, rowIdx, 11])
                {
                    rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                    rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                }


                ws.Cells[rowIdx, 1].Value = "Overall Summary";
                ws.Cells[rowIdx, 1, rowIdx, 2].Merge = true; //Merge columns start and end range
                ws.Cells[rowIdx, 1, rowIdx, 2].Style.Font.Bold = true; //Font should be bold
                ws.Cells[rowIdx, 1, rowIdx, 2].Style.Font.Size = 12;
                ws.Cells[rowIdx, 1, rowIdx, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Render the header row of overall summary
                rowIdx++;
                int overallSummaryHeaderRowIdx = rowIdx;
                ws.Cells[rowIdx, 1].Value = "Category";
                ws.Cells[rowIdx, 2].Value = "Count";

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
                // We only have a few rows of data for Overall summary, so output it now
                ws.Cells[rowIdx, 1].Value = "Sensor Occupancy Inactivity";
                ws.Cells[rowIdx, 2].Value = this._ReportModel.OverallGroupStats.NoRecentSensorEventsCount;

                rowIdx++;
                ws.Cells[rowIdx, 1].Value = "Sensor Heartbeat Inactivity";
                ws.Cells[rowIdx, 2].Value = this._ReportModel.OverallGroupStats.NoRecentCommsCount;

                rowIdx++;
                ws.Cells[rowIdx, 1].Value = "Low Battery Voltage Events";
                ws.Cells[rowIdx, 2].Value = this._ReportModel.OverallGroupStats.LowBatteryCount;

                // Column 1 should be aligned left
                ApplyNumberStyleToColumn(ws, 1, rowIdx, rowIdx, "@", ExcelHorizontalAlignment.Left);

                // Column 2 is numeric integer
                ApplyNumberStyleToColumn(ws, 2, rowIdx, rowIdx, "########0", ExcelHorizontalAlignment.Right);

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
                    ws = pck.Workbook.Worksheets.Add("Meter Summary");

                    // Render the header row
                    rowIdx = 1; // Excel uses 1-based indexes
                    ws.Cells[rowIdx, 1].Value = "Meter #";
                    ws.Cells[rowIdx, 2].Value = "Sensor Occupancy Inactivity";
                    ws.Cells[rowIdx, 3].Value = "Sensor Heartbeat Inactivity";
                    ws.Cells[rowIdx, 4].Value = "Low Battery Voltage Events";

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

                    foreach (OpsAdminMaintExceptionsStats_Meter meterStat in this._ReportModel.MeterStats)
                    {
                        // Output row values for data
                        ws.Cells[rowIdx, 1].Value = meterStat.MeterID;
                        ws.Cells[rowIdx, 2].Value = meterStat.NoRecentSensorEventsCount;
                        ws.Cells[rowIdx, 3].Value = meterStat.NoRecentCommsCount;
                        ws.Cells[rowIdx, 4].Value = meterStat.LowBatteryCount;

                        // Increment the row index, which will now be the next row of our data
                        rowIdx++;
                    }

                    // We will add autofilters to our headers so user can sort the columns easier
                    using (OfficeOpenXml.ExcelRange rng = ws.Cells[1, 1, rowIdx, 4])
                    {
                        rng.AutoFilter = true;
                    }

                    // Apply formatting to the columns as appropriate (Starting row is 2 (first row of data), and ending row is the current rowIdx value)

                    // Column 1 is numeric integer (Meter ID)
                    ApplyNumberStyleToColumn(ws, 1, 2, rowIdx, "########0", ExcelHorizontalAlignment.Left);

                    // Columns 2, 3, 4 are numeric integer
                    ApplyNumberStyleToColumn(ws, 2, 2, rowIdx, "########0", ExcelHorizontalAlignment.Right);
                    ApplyNumberStyleToColumn(ws, 3, 2, rowIdx, "########0", ExcelHorizontalAlignment.Right);
                    ApplyNumberStyleToColumn(ws, 4, 2, rowIdx, "########0", ExcelHorizontalAlignment.Right);

                    // And now lets size the columns
                    for (int autoSizeColIdx = 1; autoSizeColIdx <= 4; autoSizeColIdx++)
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
                    ws = pck.Workbook.Worksheets.Add("Space Summary");

                    // Render the header row
                    rowIdx = 1; // Excel uses 1-based indexes
                    ws.Cells[rowIdx, 1].Value = "Space #";
                    ws.Cells[rowIdx, 2].Value = "Meter #";
                    ws.Cells[rowIdx, 3].Value = "Sensor Occupancy Inactivity";
                    ws.Cells[rowIdx, 4].Value = "Sensor Heartbeat Inactivity";
                    ws.Cells[rowIdx, 5].Value = "Low Battery Voltage Events";

                    // Format the header row
                    using (OfficeOpenXml.ExcelRange rng = ws.Cells[1, 1, 1, 5])
                    {
                        rng.Style.Font.Bold = true;
                        rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                        rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));  //Set color to dark blue
                        rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    // Increment the row index, which will now be the 1st row of our data
                    rowIdx++;

                    foreach (OpsAdminMaintExceptionsStats_BayAndMeter bayStat in this._ReportModel.BayStats)
                    {
                        // Output row values for data
                        ws.Cells[rowIdx, 1].Value = bayStat.BayID;
                        ws.Cells[rowIdx, 2].Value = bayStat.MeterID;
                        ws.Cells[rowIdx, 3].Value = bayStat.NoRecentSensorEventsCount;
                        ws.Cells[rowIdx, 4].Value = bayStat.NoRecentCommsCount;
                        ws.Cells[rowIdx, 5].Value = bayStat.LowBatteryCount;

                        // Increment the row index, which will now be the next row of our data
                        rowIdx++;
                    }

                    // We will add autofilters to our headers so user can sort the columns easier
                    using (OfficeOpenXml.ExcelRange rng = ws.Cells[1, 1, rowIdx, 5])
                    {
                        rng.AutoFilter = true;
                    }

                    // Apply formatting to the columns as appropriate (Starting row is 2 (first row of data), and ending row is the current rowIdx value)

                    // Column 1 is numeric integer (Bay ID)
                    ApplyNumberStyleToColumn(ws, 1, 2, rowIdx, "########0", ExcelHorizontalAlignment.Left);

                    // Column 2 is numeric integer (Meter ID)
                    ApplyNumberStyleToColumn(ws, 2, 2, rowIdx, "########0", ExcelHorizontalAlignment.Left);

                    // Columns 3, 4, 5 are numeric integer
                    ApplyNumberStyleToColumn(ws, 3, 2, rowIdx, "########0", ExcelHorizontalAlignment.Right);
                    ApplyNumberStyleToColumn(ws, 4, 2, rowIdx, "########0", ExcelHorizontalAlignment.Right);
                    ApplyNumberStyleToColumn(ws, 5, 2, rowIdx, "########0", ExcelHorizontalAlignment.Right);

                    // And now lets size the columns
                    for (int autoSizeColIdx = 1; autoSizeColIdx <= 5; autoSizeColIdx++)
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
                    ws = pck.Workbook.Worksheets.Add("Area Summary");

                    // Render the header row
                    rowIdx = 1; // Excel uses 1-based indexes
                    ws.Cells[rowIdx, 1].Value = "Area";
                    ws.Cells[rowIdx, 2].Value = "Sensor Occupancy Inactivity";
                    ws.Cells[rowIdx, 3].Value = "Sensor Heartbeat Inactivity";
                    ws.Cells[rowIdx, 4].Value = "Low Battery Voltage Events";

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

                    foreach (OpsAdminMaintExceptionsStats_Area areaStat in this._ReportModel.AreaStats)
                    {
                        // Output row values for data
                        ws.Cells[rowIdx, 1].Value = areaStat.AreaName;
                        ws.Cells[rowIdx, 2].Value = areaStat.NoRecentSensorEventsCount;
                        ws.Cells[rowIdx, 3].Value = areaStat.NoRecentCommsCount;
                        ws.Cells[rowIdx, 4].Value = areaStat.LowBatteryCount;

                        // Increment the row index, which will now be the next row of our data
                        rowIdx++;
                    }

                    // We will add autofilters to our headers so user can sort the columns easier
                    using (OfficeOpenXml.ExcelRange rng = ws.Cells[1, 1, rowIdx, 4])
                    {
                        rng.AutoFilter = true;
                    }

                    // Apply formatting to the columns as appropriate (Starting row is 2 (first row of data), and ending row is the current rowIdx value)

                    // Column 1 should be aligned left
                    ApplyNumberStyleToColumn(ws, 1, 2, rowIdx, "@", ExcelHorizontalAlignment.Left);

                    // Columns 2, 3, 4 are numeric integer
                    ApplyNumberStyleToColumn(ws, 2, 2, rowIdx, "########0", ExcelHorizontalAlignment.Right);
                    ApplyNumberStyleToColumn(ws, 3, 2, rowIdx, "########0", ExcelHorizontalAlignment.Right);
                    ApplyNumberStyleToColumn(ws, 4, 2, rowIdx, "########0", ExcelHorizontalAlignment.Right);

                    // And now lets size the columns
                    for (int autoSizeColIdx = 1; autoSizeColIdx <= 4; autoSizeColIdx++)
                    {
                        using (OfficeOpenXml.ExcelRange col = ws.Cells[1, autoSizeColIdx, rowIdx, autoSizeColIdx])
                        {
                            col.AutoFitColumns();
                        }
                    }
                }

                // Should we include a Details worksheet?
                if (reportParams.IncludeDetails == true)
                {
                    #region Sensor Status Details (Current status or historical records)
                    // This will have different columns depending on if we are doing historical or current data reporting...
                    if ((reportParams.ReportOnHistoricalData == false) || (reportParams.ReportOnHistoricalData == true))
                    {
                        // Create the worksheet
                        ws = pck.Workbook.Worksheets.Add("Sensor Status Details");

                        // Render the header row
                        rowIdx = 1; // Excel uses 1-based indexes
                        ws.Cells[rowIdx, 1].Value = "Meter Name";
                        ws.Cells[rowIdx, 2].Value = "Meter #";
                        ws.Cells[rowIdx, 3].Value = "Space #";
                        ws.Cells[rowIdx, 4].Value = "Area";
                        if (reportParams.ReportOnHistoricalData == false)
                        {
                            ws.Cells[rowIdx, 5].Value = "Last Sensor Status";
                            ws.Cells[rowIdx, 6].Value = "Last Sensor Status Time";
                        }
                        else
                        {
                            ws.Cells[rowIdx, 5].Value = "Sensor Status";
                            ws.Cells[rowIdx, 6].Value = "Sensor Status Time";
                        }
                        ws.Cells[rowIdx, 7].Value = "Elapsed";

                        // Format the header row
                        using (OfficeOpenXml.ExcelRange rng = ws.Cells[1, 1, 1, 7])
                        {
                            rng.Style.Font.Bold = true;
                            rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                            rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));  //Set color to dark blue
                            rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
                        }

                        // Increment the row index, which will now be the 1st row of our data
                        rowIdx++;

                        foreach (SensorEventAndCommsRecord sensorEvent in _ReportModel.SensorsWithoutRecentOccupancyEvents)
                        {
                            // Output row values for data
                            ws.Cells[rowIdx, 1].Value = sensorEvent.MeterName;
                            ws.Cells[rowIdx, 2].Value = sensorEvent.MeterID;
                            ws.Cells[rowIdx, 3].Value = sensorEvent.BayID;
                            ws.Cells[rowIdx, 4].Value = ResolvedAreaNameForAreaID(sensorEvent.AreaID);

                            if (sensorEvent.LastSensorStatus == 1)
                                ws.Cells[rowIdx, 5].Value = "Occupied";
                            else
                                ws.Cells[rowIdx, 5].Value = "Vacant";

                            ws.Cells[rowIdx, 6].Value = sensorEvent.LastSensorStatusTime;
                            ws.Cells[rowIdx, 7].Value = FormatTimeSpanAsHoursMinutesAndSeconds(sensorEvent.CalculatedDuration);

                            // Increment the row index, which will now be the next row of our data
                            rowIdx++;
                        }

                        // We will add autofilters to our headers so user can sort the columns easier
                        using (OfficeOpenXml.ExcelRange rng = ws.Cells[1, 1, rowIdx, 7])
                        {
                            rng.AutoFilter = true;
                        }

                        // Apply formatting to the columns as appropriate (Starting row is 2 (first row of data), and ending row is the current rowIdx value)

                        ApplyNumberStyleToColumn(ws, 1, 2, rowIdx, "@", ExcelHorizontalAlignment.Left);
                        ApplyNumberStyleToColumn(ws, 2, 2, rowIdx, "@", ExcelHorizontalAlignment.Left);
                        ApplyNumberStyleToColumn(ws, 3, 2, rowIdx, "@", ExcelHorizontalAlignment.Left);
                        ApplyNumberStyleToColumn(ws, 4, 2, rowIdx, "@", ExcelHorizontalAlignment.Left);
                        ApplyNumberStyleToColumn(ws, 5, 2, rowIdx, "@", ExcelHorizontalAlignment.Left);
                        ApplyNumberStyleToColumn(ws, 6, 2, rowIdx, "yyyy-mm-dd hh:mm:ss tt", ExcelHorizontalAlignment.Right);
                        ApplyNumberStyleToColumn(ws, 7, 2, rowIdx, "@", ExcelHorizontalAlignment.Right);

                        // And now lets size the columns
                        for (int autoSizeColIdx = 1; autoSizeColIdx <= 7; autoSizeColIdx++)
                        {
                            using (OfficeOpenXml.ExcelRange col = ws.Cells[1, autoSizeColIdx, rowIdx, autoSizeColIdx])
                            {
                                col.AutoFitColumns();
                            }
                        }
                    }
                    #endregion

                    #region Sensor Heartbeat Details (Current status or historical records)
                    // This will have different columns depending on if we are doing historical or current data reporting...
                    if ((reportParams.ReportOnHistoricalData == false) || (reportParams.ReportOnHistoricalData == true))
                    {
                        // Create the worksheet
                        ws = pck.Workbook.Worksheets.Add("Sensor Communication Details");

                        // Render the header row
                        rowIdx = 1; // Excel uses 1-based indexes
                        ws.Cells[rowIdx, 1].Value = "Meter Name";
                        ws.Cells[rowIdx, 2].Value = "Meter #";
                        ws.Cells[rowIdx, 3].Value = "Space #";
                        ws.Cells[rowIdx, 4].Value = "Area";
                        if (reportParams.ReportOnHistoricalData == false)
                        {
                            ws.Cells[rowIdx, 5].Value = "Last Sensor Communication";
                            ws.Cells[rowIdx, 6].Value = "Last Sensor Communication Time";
                        }
                        else
                        {
                            ws.Cells[rowIdx, 5].Value = "Sensor Communication";
                            ws.Cells[rowIdx, 6].Value = "Sensor Communication Time";
                        }
                        ws.Cells[rowIdx, 7].Value = "Elapsed";

                        // Format the header row
                        using (OfficeOpenXml.ExcelRange rng = ws.Cells[1, 1, 1, 7])
                        {
                            rng.Style.Font.Bold = true;
                            rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                            rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));  //Set color to dark blue
                            rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
                        }

                        // Increment the row index, which will now be the 1st row of our data
                        rowIdx++;

                        foreach (SensorHeartbeatRecord sensorEvent in _ReportModel.SensorsWithoutRecentComms)
                        {
                            // Output row values for data
                            ws.Cells[rowIdx, 1].Value = sensorEvent.MeterName;
                            ws.Cells[rowIdx, 2].Value = sensorEvent.MeterID;
                            ws.Cells[rowIdx, 3].Value = sensorEvent.BayID;
                            ws.Cells[rowIdx, 4].Value = ResolvedAreaNameForAreaID(sensorEvent.AreaID);
                            ws.Cells[rowIdx, 5].Value = sensorEvent.LastCommunicationType;
                            ws.Cells[rowIdx, 6].Value = sensorEvent.LastCommunication;
                            ws.Cells[rowIdx, 7].Value = FormatTimeSpanAsHoursMinutesAndSeconds(sensorEvent.CalculatedDuration);

                            // Increment the row index, which will now be the next row of our data
                            rowIdx++;
                        }

                        // We will add autofilters to our headers so user can sort the columns easier
                        using (OfficeOpenXml.ExcelRange rng = ws.Cells[1, 1, rowIdx, 7])
                        {
                            rng.AutoFilter = true;
                        }

                        // Apply formatting to the columns as appropriate (Starting row is 2 (first row of data), and ending row is the current rowIdx value)

                        ApplyNumberStyleToColumn(ws, 1, 2, rowIdx, "@", ExcelHorizontalAlignment.Left);
                        ApplyNumberStyleToColumn(ws, 2, 2, rowIdx, "@", ExcelHorizontalAlignment.Left);
                        ApplyNumberStyleToColumn(ws, 3, 2, rowIdx, "@", ExcelHorizontalAlignment.Left);
                        ApplyNumberStyleToColumn(ws, 4, 2, rowIdx, "@", ExcelHorizontalAlignment.Left);
                        ApplyNumberStyleToColumn(ws, 5, 2, rowIdx, "@", ExcelHorizontalAlignment.Left);
                        ApplyNumberStyleToColumn(ws, 6, 2, rowIdx, "yyyy-mm-dd hh:mm:ss tt", ExcelHorizontalAlignment.Right);
                        ApplyNumberStyleToColumn(ws, 7, 2, rowIdx, "@", ExcelHorizontalAlignment.Right);

                        // And now lets size the columns
                        for (int autoSizeColIdx = 1; autoSizeColIdx <= 7; autoSizeColIdx++)
                        {
                            using (OfficeOpenXml.ExcelRange col = ws.Cells[1, autoSizeColIdx, rowIdx, autoSizeColIdx])
                            {
                                col.AutoFitColumns();
                            }
                        }
                    }
                    #endregion

                    #region Sensor Low Battery Details (Current status)
                    // This will have different columns depending on if we are doing historical or current data reporting...
                    if (reportParams.ReportOnHistoricalData == false)
                    {
                        // Create the worksheet
                        ws = pck.Workbook.Worksheets.Add("Sensor Low Battery Details");

                        // Render the header row
                        rowIdx = 1; // Excel uses 1-based indexes
                        ws.Cells[rowIdx, 1].Value = "Meter Name";
                        ws.Cells[rowIdx, 2].Value = "Meter #";
                        ws.Cells[rowIdx, 3].Value = "Space #";
                        ws.Cells[rowIdx, 4].Value = "Area";
                        ws.Cells[rowIdx, 5].Value = "Battery Event";
                        ws.Cells[rowIdx, 6].Value = "Dry Battery Volts - Currrent";
                        ws.Cells[rowIdx, 7].Value = "Dry Battery Volts - Minimum";
                        ws.Cells[rowIdx, 8].Value = "Recharge Battery Volts - Currrent";
                        ws.Cells[rowIdx, 9].Value = "Recharge Battery Volts - Minimum";

                        // Format the header row
                        using (OfficeOpenXml.ExcelRange rng = ws.Cells[1, 1, 1, 9])
                        {
                            rng.Style.Font.Bold = true;
                            rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                            rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));  //Set color to dark blue
                            rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
                        }

                        // Increment the row index, which will now be the 1st row of our data
                        rowIdx++;

                        foreach (SensorBatteryDiagnostics batteryEvent in _ReportModel.SensorsWithLowBatteries)
                        {
                            // Output row values for data
                            ws.Cells[rowIdx, 1].Value = batteryEvent.MeterName;
                            ws.Cells[rowIdx, 2].Value = batteryEvent.MeterID;
                            ws.Cells[rowIdx, 3].Value = batteryEvent.BayID;
                            ws.Cells[rowIdx, 4].Value = ResolvedAreaNameForAreaID(batteryEvent.AreaID);
                            ws.Cells[rowIdx, 5].Value = batteryEvent.TimestampOfLatestBatteryVoltageReport;

                            ws.Cells[rowIdx, 6].Value = batteryEvent.GetNumericValue_DryBattCurrV();
                            if (batteryEvent.GetNumericValue_DryBattCurrV() < 3.3f)
                            {
                                ws.Cells[rowIdx, 6].Style.Font.Bold = true;
                                ws.Cells[rowIdx, 6].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                                ws.Cells[rowIdx, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                                ws.Cells[rowIdx, 6].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                            }

                            ws.Cells[rowIdx, 7].Value = batteryEvent.GetNumericValue_DryBattMinV();
                            if (batteryEvent.GetNumericValue_DryBattMinV() < 3.3f)
                            {
                                ws.Cells[rowIdx, 7].Style.Font.Bold = true;
                                ws.Cells[rowIdx, 7].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                                ws.Cells[rowIdx, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                                ws.Cells[rowIdx, 7].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                            }

                            ws.Cells[rowIdx, 8].Value = batteryEvent.GetNumericValue_RechargeBattCurrV();
                            if (batteryEvent.GetNumericValue_RechargeBattCurrV() < 3.3f)
                            {
                                ws.Cells[rowIdx, 8].Style.Font.Bold = true;
                                ws.Cells[rowIdx, 8].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                                ws.Cells[rowIdx, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                                ws.Cells[rowIdx, 8].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                            }

                            ws.Cells[rowIdx, 9].Value = batteryEvent.GetNumericValue_RechargeBattMinV();
                            if (batteryEvent.GetNumericValue_RechargeBattMinV() < 3.3f)
                            {
                                ws.Cells[rowIdx, 9].Style.Font.Bold = true;
                                ws.Cells[rowIdx, 9].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                                ws.Cells[rowIdx, 9].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                                ws.Cells[rowIdx, 9].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                            }

                            // Increment the row index, which will now be the next row of our data
                            rowIdx++;
                        }

                        // We will add autofilters to our headers so user can sort the columns easier
                        using (OfficeOpenXml.ExcelRange rng = ws.Cells[1, 1, rowIdx, 9])
                        {
                            rng.AutoFilter = true;
                        }

                        // Apply formatting to the columns as appropriate (Starting row is 2 (first row of data), and ending row is the current rowIdx value)

                        ApplyNumberStyleToColumn(ws, 1, 2, rowIdx, "@", ExcelHorizontalAlignment.Left);
                        ApplyNumberStyleToColumn(ws, 2, 2, rowIdx, "@", ExcelHorizontalAlignment.Left);
                        ApplyNumberStyleToColumn(ws, 3, 2, rowIdx, "@", ExcelHorizontalAlignment.Left);
                        ApplyNumberStyleToColumn(ws, 4, 2, rowIdx, "@", ExcelHorizontalAlignment.Left);
                        ApplyNumberStyleToColumn(ws, 5, 2, rowIdx, "yyyy-mm-dd hh:mm:ss tt", ExcelHorizontalAlignment.Right);
                        ApplyNumberStyleToColumn(ws, 6, 2, rowIdx, "###0.0", ExcelHorizontalAlignment.Right);
                        ApplyNumberStyleToColumn(ws, 7, 2, rowIdx, "###0.0", ExcelHorizontalAlignment.Right);
                        ApplyNumberStyleToColumn(ws, 8, 2, rowIdx, "###0.0", ExcelHorizontalAlignment.Right);
                        ApplyNumberStyleToColumn(ws, 9, 2, rowIdx, "###0.0", ExcelHorizontalAlignment.Right);

                        // And now lets size the columns
                        for (int autoSizeColIdx = 1; autoSizeColIdx <= 9; autoSizeColIdx++)
                        {
                            using (OfficeOpenXml.ExcelRange col = ws.Cells[1, autoSizeColIdx, rowIdx, autoSizeColIdx])
                            {
                                col.AutoFitColumns();
                            }
                        }
                    }
                    #endregion

                    #region Sensor Low Battery Details (Historical records)
                    // This will have different columns depending on if we are doing historical or current data reporting...
                    if (reportParams.ReportOnHistoricalData == true)
                    {
                        // Create the worksheet
                        ws = pck.Workbook.Worksheets.Add("Sensor Low Battery Details");

                        // Render the header row
                        rowIdx = 1; // Excel uses 1-based indexes
                        ws.Cells[rowIdx, 1].Value = "Meter Name";
                        ws.Cells[rowIdx, 2].Value = "Meter #";
                        ws.Cells[rowIdx, 3].Value = "Space #";
                        ws.Cells[rowIdx, 4].Value = "Area";
                        ws.Cells[rowIdx, 5].Value = "Battery Event";
                        ws.Cells[rowIdx, 6].Value = "Battery Event Type";
                        ws.Cells[rowIdx, 7].Value = "Battery Voltage";
                        //ws.Cells[rowIdx, 6].Value = "Dry Battery Volts - Currrent";
                        //ws.Cells[rowIdx, 7].Value = "Dry Battery Volts - Minimum";
                        //ws.Cells[rowIdx, 8].Value = "Recharge Battery Volts - Currrent";
                        //ws.Cells[rowIdx, 9].Value = "Recharge Battery Volts - Minimum";

                        // Format the header row
                        using (OfficeOpenXml.ExcelRange rng = ws.Cells[1, 1, 1, 7])
                        {
                            rng.Style.Font.Bold = true;
                            rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                            rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));  //Set color to dark blue
                            rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
                        }

                        // Increment the row index, which will now be the 1st row of our data
                        rowIdx++;

                        foreach (SensorBatteryDiagnostics batteryEvent in _ReportModel.SensorsWithLowBatteries)
                        {
                            // Output row values for data
                            ws.Cells[rowIdx, 1].Value = batteryEvent.MeterName;
                            ws.Cells[rowIdx, 2].Value = batteryEvent.MeterID;
                            ws.Cells[rowIdx, 3].Value = batteryEvent.BayID;
                            ws.Cells[rowIdx, 4].Value = ResolvedAreaNameForAreaID(batteryEvent.AreaID);
                            ws.Cells[rowIdx, 5].Value = batteryEvent.TimestampOfLatestBatteryVoltageReport;

                            if (!string.IsNullOrEmpty(batteryEvent.DryBattCurrV))
                            {
                                if (batteryEvent.GetNumericValue_DryBattCurrV() < 3.3f)
                                {
                                    ws.Cells[rowIdx, 6].Value = "Dry Battery Volts - Currrent";
                                    ws.Cells[rowIdx, 7].Value = batteryEvent.GetNumericValue_DryBattCurrV();
                                    ws.Cells[rowIdx, 7].Style.Font.Bold = true;
                                    ws.Cells[rowIdx, 7].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                                    ws.Cells[rowIdx, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                                    ws.Cells[rowIdx, 7].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                                }
                            }

                            if (!string.IsNullOrEmpty(batteryEvent.DryBattMinV))
                            {
                                if (batteryEvent.GetNumericValue_DryBattMinV() < 3.3f)
                                {
                                    ws.Cells[rowIdx, 6].Value = "Dry Battery Volts - Minimum";
                                    ws.Cells[rowIdx, 7].Value = batteryEvent.GetNumericValue_DryBattMinV();
                                    ws.Cells[rowIdx, 7].Style.Font.Bold = true;
                                    ws.Cells[rowIdx, 7].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                                    ws.Cells[rowIdx, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                                    ws.Cells[rowIdx, 7].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                                }
                            }

                            if (!string.IsNullOrEmpty(batteryEvent.RechargeBattCurrV))
                            {
                                if (batteryEvent.GetNumericValue_RechargeBattCurrV() < 3.3f)
                                {
                                    ws.Cells[rowIdx, 6].Value = "Recharge Battery Volts - Currrent";
                                    ws.Cells[rowIdx, 7].Value = batteryEvent.GetNumericValue_RechargeBattCurrV();
                                    ws.Cells[rowIdx, 7].Style.Font.Bold = true;
                                    ws.Cells[rowIdx, 7].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                                    ws.Cells[rowIdx, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                                    ws.Cells[rowIdx, 7].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                                }
                            }

                            if (!string.IsNullOrEmpty(batteryEvent.RechargeBattMinV))
                            {
                                if (batteryEvent.GetNumericValue_RechargeBattMinV() < 3.3f)
                                {
                                    ws.Cells[rowIdx, 6].Value = "Recharge Battery Volts - Minimum";
                                    ws.Cells[rowIdx, 7].Value = batteryEvent.GetNumericValue_RechargeBattMinV();
                                    ws.Cells[rowIdx, 7].Style.Font.Bold = true;
                                    ws.Cells[rowIdx, 7].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                                    ws.Cells[rowIdx, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                                    ws.Cells[rowIdx, 7].Style.Font.Color.SetColor(System.Drawing.Color.Black);
                                }
                            }

                            // Increment the row index, which will now be the next row of our data
                            rowIdx++;
                        }

                        // We will add autofilters to our headers so user can sort the columns easier
                        using (OfficeOpenXml.ExcelRange rng = ws.Cells[1, 1, rowIdx, 7])
                        {
                            rng.AutoFilter = true;
                        }

                        // Apply formatting to the columns as appropriate (Starting row is 2 (first row of data), and ending row is the current rowIdx value)

                        ApplyNumberStyleToColumn(ws, 1, 2, rowIdx, "@", ExcelHorizontalAlignment.Left);
                        ApplyNumberStyleToColumn(ws, 2, 2, rowIdx, "@", ExcelHorizontalAlignment.Left);
                        ApplyNumberStyleToColumn(ws, 3, 2, rowIdx, "@", ExcelHorizontalAlignment.Left);
                        ApplyNumberStyleToColumn(ws, 4, 2, rowIdx, "@", ExcelHorizontalAlignment.Left);
                        ApplyNumberStyleToColumn(ws, 5, 2, rowIdx, "yyyy-mm-dd hh:mm:ss tt", ExcelHorizontalAlignment.Right);
                        ApplyNumberStyleToColumn(ws, 6, 2, rowIdx, "@", ExcelHorizontalAlignment.Left);
                        ApplyNumberStyleToColumn(ws, 7, 2, rowIdx, "###0.0", ExcelHorizontalAlignment.Right);

                        // And now lets size the columns
                        for (int autoSizeColIdx = 1; autoSizeColIdx <= 7; autoSizeColIdx++)
                        {
                            using (OfficeOpenXml.ExcelRange col = ws.Cells[1, autoSizeColIdx, rowIdx, autoSizeColIdx])
                            {
                                col.AutoFitColumns();
                            }
                        }
                    }
                    #endregion
                }


                // All cells in spreadsheet are populated now, so render (save the file) to a memory stream 
                byte[] bytes = pck.GetAsByteArray();
                ms.Write(bytes, 0, bytes.Length);
            }


            // Stop diagnostics timer
            sw.Stop();
            System.Diagnostics.Debug.WriteLine("OperationsAdministrationAndMaintenanceExceptions Report Generation took: " + sw.Elapsed.ToString());
        }
        #endregion


        #region Private/Protected Methods
        protected void GatherReportData(List<int> listOfMeterIDs, OpsAdminMaintExceptionsReportParameters reportParams)
        {
            this._ReportModel = new OperationsAdministrationAndMaintenanceReportModel();

            // Adjust the date ranges as needed for our SQL queries.
            // The end time needs to be inclusive of the entire minute (seconds and milliseconds are not in the resolution of the passed EndTime parameter)
            DateTime AdjustedStartTime;
            DateTime AdjustedEndTime;
            AdjustedStartTime = reportParams.StartTime;
            AdjustedEndTime = new DateTime(reportParams.EndTime.Year, reportParams.EndTime.Month, reportParams.EndTime.Day, reportParams.EndTime.Hour, reportParams.EndTime.Minute, 0).AddMinutes(1);
            reportParams.StartTime = AdjustedStartTime;
            reportParams.EndTime = AdjustedEndTime;

            SensingDatabaseSource sensorDataProvider = new SensingDatabaseSource(_CustomerConfig);

            List<SensorEventAndCommsRecord> allOccupancyStateRecs = sensorDataProvider.GetOccupancyDataForOpsAdminMaintExceptions(
                this._CustomerConfig.CustomerId, listOfMeterIDs, reportParams);

            List<SensorHeartbeatRecord> allSensorCommRecs = sensorDataProvider.GetCommunicationDataForOpsAdminMaintExceptions(
                    this._CustomerConfig.CustomerId, listOfMeterIDs, reportParams);

            List<SensorBatteryDiagnostics> allBatteryRecs = sensorDataProvider.GetBatteryVoltageDataForOpsAdminMaintExceptions(
                this._CustomerConfig.CustomerId, listOfMeterIDs, reportParams);

            // Analyze data for each meter
            foreach (int nextMeterID in listOfMeterIDs)
            {
                AnalyzeDataForMeter(allOccupancyStateRecs, allSensorCommRecs, allBatteryRecs, nextMeterID, reportParams);
            }

            // Gather a unique list of AreaIDs associated with all meters involved in this report
            List<int> listOfAreaIDs = new List<int>();
            foreach (int nextMeterID in listOfMeterIDs)
            {
                int resolvedAreaIDForMeterID = ResolveAreaIDForMeterID(nextMeterID);
                if (listOfAreaIDs.Contains(resolvedAreaIDForMeterID) == false)
                    listOfAreaIDs.Add(resolvedAreaIDForMeterID);
            }

            // Now for each unique area included in the report, we will gather aggregate data (based on meter aggregates)
            foreach (int nextAreaID in listOfAreaIDs)
            {
                // Find or create an area stats object for the current AreaID
                OpsAdminMaintExceptionsStats_Area AreaStatsObj = null;
                foreach (OpsAdminMaintExceptionsStats_Area existingArea in this._ReportModel.AreaStats)
                {
                    if (existingArea.AreaID == nextAreaID)
                    {
                        AreaStatsObj = existingArea;
                        break;
                    }
                }
                if (AreaStatsObj == null)
                {
                    AreaStatsObj = new OpsAdminMaintExceptionsStats_Area();
                    AreaStatsObj.AreaID = nextAreaID;
                    AreaStatsObj.AreaName = ResolvedAreaNameForAreaID(nextAreaID);
                    this._ReportModel.AreaStats.Add(AreaStatsObj);
                }

                // Now aggregate info about exceptional occupancy state changes for this area
                foreach (SensorEventAndCommsRecord nextSensorWithoutRecentOccupancyChange in _ReportModel.SensorsWithoutRecentOccupancyEvents)
                {
                    // Skip next overstay vio record if its not for the desired area
                    if (nextSensorWithoutRecentOccupancyChange.AreaID != nextAreaID)
                        continue;

                    AreaStatsObj.NoRecentSensorEventsCount++;
                }

                // Now aggregate info about exceptional communication/heartbeat events for this area
                foreach (SensorHeartbeatRecord nextSensorWithoutRecentHeartbeat in _ReportModel.SensorsWithoutRecentComms)
                {
                    // Skip next overstay vio record if its not for the desired area
                    if (nextSensorWithoutRecentHeartbeat.AreaID != nextAreaID)
                        continue;

                    AreaStatsObj.NoRecentCommsCount++;
                }

                // Now aggregate info about exceptional battery voltage events for this area
                foreach (SensorBatteryDiagnostics nextSensorWithLowBattery in _ReportModel.SensorsWithLowBatteries)
                {
                    // Skip next overstay vio record if its not for the desired area
                    if (nextSensorWithLowBattery.AreaID != nextAreaID)
                        continue;

                    AreaStatsObj.LowBatteryCount++;
                }
            }


            // We will also do an overall stats for exceptional occupancy state, heartbeat, and battery voltage events
            foreach (SensorEventAndCommsRecord nextSensorWithoutRecentOccupancyChange in _ReportModel.SensorsWithoutRecentOccupancyEvents)
            {
                _ReportModel.OverallGroupStats.NoRecentSensorEventsCount++;
            }
            foreach (SensorHeartbeatRecord nextSensorWithoutRecentHeartbeat in _ReportModel.SensorsWithoutRecentComms)
            {
                _ReportModel.OverallGroupStats.NoRecentCommsCount++;
            }
            foreach (SensorBatteryDiagnostics nextSensorWithLowBattery in _ReportModel.SensorsWithLowBatteries)
            {
                _ReportModel.OverallGroupStats.LowBatteryCount++;
            }

            // Sort this.ReportDataModel.AreaStats by AreaName so it renders in Excel in a nice sort order
            this._ReportModel.AreaStats.Sort(new OpsAdminMaintExceptionsStats_AreaLogicalComparer(true));

            // Sort this.ReportDataModel.MeterStats by MeterID so it renders in Excel in a nice sort order
            this._ReportModel.MeterStats.Sort(new OpsAdminMaintExceptionsStats_MeterLogicalComparer());

            // Sort this.ReportDataModel.BayStats by BayID so it renders in Excel in a nice sort order
            this._ReportModel.BayStats.Sort(new OpsAdminMaintExceptionsStats_BayAndMeterLogicalComparer());
        }

        protected void AnalyzeDataForMeter(List<SensorEventAndCommsRecord> allOccupancyStateRecs, List<SensorHeartbeatRecord> allSensorCommRecs,
            List<SensorBatteryDiagnostics> allBatteryRecs, int meterId, OpsAdminMaintExceptionsReportParameters reportParams)
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
                // Get filtered list of data that applies to the current meter and space
                List<SensorEventAndCommsRecord> VSDataForCurrentMeterAndBay = GetQuestionableOccupancyStateRecsForMeterIDAndBayID(allOccupancyStateRecs, meterId, nextBayID);
                List<SensorHeartbeatRecord> SensorCommDataForCurrentMeterAndBay = GetQuestionableSensorCommRecsForMeterIDAndBayID(allSensorCommRecs, meterId, nextBayID);
                List<SensorBatteryDiagnostics> SensorBatteryDataForCurrentMeterAndBay = GetQuestionableBatteryRecsForMeterIDAndBayID(allBatteryRecs, meterId, nextBayID);

                // Analyze the data for this space
                AnalyzeDataForSpace(VSDataForCurrentMeterAndBay, SensorCommDataForCurrentMeterAndBay, SensorBatteryDataForCurrentMeterAndBay,
                    meterId, nextBayID, reportParams);
            }

            // Create a meter statistics object for the current meter, and add to our report list
            OpsAdminMaintExceptionsStats_Meter MeterStatsObj = new OpsAdminMaintExceptionsStats_Meter();
            MeterStatsObj.MeterID = meterId;
            this._ReportModel.MeterStats.Add(MeterStatsObj);

            // Now aggregate info about exceptional occupancy state changes for this meter
            foreach (SensorEventAndCommsRecord nextSensorWithoutRecentOccupancyChange in _ReportModel.SensorsWithoutRecentOccupancyEvents)
            {
                // Skip next overstay vio record if its not for the desired meter
                if (nextSensorWithoutRecentOccupancyChange.MeterID != meterId)
                    continue;

                MeterStatsObj.NoRecentSensorEventsCount++;
            }

            // Now aggregate info about exceptional communication/heartbeat events for this meter
            foreach (SensorHeartbeatRecord nextSensorWithoutRecentHeartbeat in _ReportModel.SensorsWithoutRecentComms)
            {
                // Skip next overstay vio record if its not for the desired meter
                if (nextSensorWithoutRecentHeartbeat.MeterID != meterId)
                    continue;

                MeterStatsObj.NoRecentCommsCount++;
            }

            // Now aggregate info about exceptional battery voltage events for this meter
            foreach (SensorBatteryDiagnostics nextSensorWithLowBattery in _ReportModel.SensorsWithLowBatteries)
            {
                // Skip next overstay vio record if its not for the desired meter
                if (nextSensorWithLowBattery.MeterID != meterId)
                    continue;

                MeterStatsObj.LowBatteryCount++;
            }
        }

        protected void AnalyzeDataForSpace(List<SensorEventAndCommsRecord> questionableOccupancyStateRecs, List<SensorHeartbeatRecord> questionableSensorCommRecs,
            List<SensorBatteryDiagnostics> questionableBatteryRecs, int meterId, int bayNumber, OpsAdminMaintExceptionsReportParameters reportParams)
        {
            // Need to know current time in customer's timezone
            DateTime NowAtDestination = Convert.ToDateTime(this._CustomerConfig.DestinationTimeZoneDisplayName);


            // We have to do different processing logic depending on if we are looking at "current" status or "historical" records
            if (reportParams.ReportOnHistoricalData == false)
            {
                // Analyze occupancy data to find exceptions with age of occupancy events
                // (For reporting style of "current" data instead of "historical" data)
                foreach (SensorEventAndCommsRecord rawVSDataRec in questionableOccupancyStateRecs)
                {
                    // The data should be filtered to the correct meter and bay, but we might as well check it
                    if ((rawVSDataRec.MeterID != meterId) || (rawVSDataRec.BayID != bayNumber))
                        continue;

                    // Determine the duration since the last occupancy event.  If the duration is greater than our threshold for exceptions,
                    // then add this record to our list of records to include in report
                    TimeSpan duration = (NowAtDestination - rawVSDataRec.LastSensorStatusTime);
                    if (duration.TotalMinutes > (24 * 60)) // Duration exceeds 24 hours? 
                    {
                        rawVSDataRec.CalculatedDuration = duration;
                        _ReportModel.SensorsWithoutRecentOccupancyEvents.Add(rawVSDataRec);
                    }
                }

                // Analyze sensor comms/hearbeat data to find exceptions with age of communication events
                // (For reporting style of "current" data instead of "historical" data)
                foreach (SensorHeartbeatRecord rawSensorCommDataRec in questionableSensorCommRecs)
                {
                    // The data should be filtered to the correct meter and bay, but we might as well check it
                    if ((rawSensorCommDataRec.MeterID != meterId) || (rawSensorCommDataRec.BayID != bayNumber))
                        continue;

                    // Determine the duration since the last occupancy event.  If the duration is greater than our threshold for exceptions,
                    // then add this record to our list of records to include in report
                    TimeSpan duration = (NowAtDestination - rawSensorCommDataRec.LastCommunication);
                    if (duration.TotalMinutes > (24 * 60)) // Duration exceeds 24 hours? 
                    {
                        rawSensorCommDataRec.CalculatedDuration = duration;
                        _ReportModel.SensorsWithoutRecentComms.Add(rawSensorCommDataRec);
                    }
                }

                // Analyze sensor battery data to find exceptions with battery voltages
                // (For reporting style of "current" data instead of "historical" data)
                foreach (SensorBatteryDiagnostics rawSensorBatteryDataRec in questionableBatteryRecs)
                {
                    // The data should be filtered to the correct meter and bay, but we might as well check it
                    if ((rawSensorBatteryDataRec.MeterID != meterId) || (rawSensorBatteryDataRec.BayID != bayNumber))
                        continue;

                    // If a record has any battery voltage value less than 3.3 volts, we consider it an exception
                    bool hasBatteryException = false;

                    if ((!string.IsNullOrEmpty(rawSensorBatteryDataRec.DryBattCurrV)) && (rawSensorBatteryDataRec.GetNumericValue_DryBattCurrV() < 3.3f))
                        hasBatteryException = true;

                    if ((!string.IsNullOrEmpty(rawSensorBatteryDataRec.DryBattMinV)) && (rawSensorBatteryDataRec.GetNumericValue_DryBattMinV() < 3.3f))
                        hasBatteryException = true;

                    if ((!string.IsNullOrEmpty(rawSensorBatteryDataRec.RechargeBattCurrV)) && (rawSensorBatteryDataRec.GetNumericValue_RechargeBattCurrV() < 3.3f))
                        hasBatteryException = true;

                    if ((!string.IsNullOrEmpty(rawSensorBatteryDataRec.RechargeBattMinV)) && (rawSensorBatteryDataRec.GetNumericValue_RechargeBattMinV() < 3.3f))
                        hasBatteryException = true;

                    if (hasBatteryException)
                    {
                        _ReportModel.SensorsWithLowBatteries.Add(rawSensorBatteryDataRec);
                    }
                }
            }
            else if (reportParams.ReportOnHistoricalData == true)
            {
                // Analyze occupancy data to find exceptions with age of occupancy events
                // (For reporting style of "historical" data instead of "current" data, so we must evaluate durations between events!)
                SensorEventAndCommsRecord currentVSEventToProcess = null;
                foreach (SensorEventAndCommsRecord rawVSDataRec in questionableOccupancyStateRecs)
                {
                    // The data should be filtered to the correct meter and bay, but we might as well check it
                    if ((rawVSDataRec.MeterID != meterId) || (rawVSDataRec.BayID != bayNumber))
                        continue;

                    // Are we ready to begin a new object?
                    if (currentVSEventToProcess == null)
                    {
                        // Nothing more to do until we get to the next record (or end of records)
                        currentVSEventToProcess = rawVSDataRec;
                        continue;
                    }

                    // Get the duration between the 2 events
                    TimeSpan duration = (rawVSDataRec.LastSensorStatusTime - currentVSEventToProcess.LastSensorStatusTime);
                    if (duration.TotalMinutes > (24 * 60)) // Duration exceeds 24 hours? 
                    {
                        currentVSEventToProcess.CalculatedDuration = duration;
                        _ReportModel.SensorsWithoutRecentOccupancyEvents.Add(currentVSEventToProcess);
                        currentVSEventToProcess = rawVSDataRec;
                    }
                }
                if (currentVSEventToProcess != null)
                {
                    // Get the duration of this last event.  Limit its end to the earilier of current timestamp or end of reportable date range
                    DateTime LesserOfCurrentTimeOrReportEndTime = NowAtDestination;
                    if (NowAtDestination > reportParams.EndTime)
                        LesserOfCurrentTimeOrReportEndTime = reportParams.EndTime;

                    TimeSpan duration = (LesserOfCurrentTimeOrReportEndTime - currentVSEventToProcess.LastSensorStatusTime);
                    if (duration.TotalMinutes > (24 * 60)) // Duration exceeds 24 hours? 
                    {
                        currentVSEventToProcess.CalculatedDuration = duration;
                        _ReportModel.SensorsWithoutRecentOccupancyEvents.Add(currentVSEventToProcess);
                    }
                }

                // Analyze sensor comms/hearbeat data to find exceptions with age of communication events
                // (For reporting style of "historical" data instead of "current" data, so we must evaluate durations between events!)
                SensorHeartbeatRecord currentCommRecordToProcess = null;
                foreach (SensorHeartbeatRecord rawSensorCommDataRec in questionableSensorCommRecs)
                {
                    // The data should be filtered to the correct meter and bay, but we might as well check it
                    if ((rawSensorCommDataRec.MeterID != meterId) || (rawSensorCommDataRec.BayID != bayNumber))
                        continue;

                    // Are we ready to begin a new object?
                    if (currentCommRecordToProcess == null)
                    {
                        // Nothing more to do until we get to the next record (or end of records)
                        currentCommRecordToProcess = rawSensorCommDataRec;
                        continue;
                    }

                    // Get the duration between the 2 events
                    TimeSpan duration = (rawSensorCommDataRec.LastCommunication - currentCommRecordToProcess.LastCommunication);
                    if (duration.TotalMinutes > (24 * 60)) // Duration exceeds 24 hours? 
                    {
                        currentCommRecordToProcess.CalculatedDuration = duration;
                        _ReportModel.SensorsWithoutRecentComms.Add(currentCommRecordToProcess);
                        currentCommRecordToProcess = rawSensorCommDataRec;
                    }
                }
                if (currentCommRecordToProcess != null)
                {
                    // Get the duration of this last event.  Limit its end to the earilier of current timestamp or end of reportable date range
                    DateTime LesserOfCurrentTimeOrReportEndTime = NowAtDestination;
                    if (NowAtDestination > reportParams.EndTime)
                        LesserOfCurrentTimeOrReportEndTime = reportParams.EndTime;

                    TimeSpan duration = (LesserOfCurrentTimeOrReportEndTime - currentCommRecordToProcess.LastCommunication);
                    if (duration.TotalMinutes > (24 * 60)) // Duration exceeds 24 hours? 
                    {
                        currentCommRecordToProcess.CalculatedDuration = duration;
                        _ReportModel.SensorsWithoutRecentComms.Add(currentCommRecordToProcess);
                    }
                }

                // Analyze sensor battery data to find exceptions with battery voltages
                // (For reporting style of "historical" data instead of "current" data.  Historical records will only have one type of diagnostic value to looks at)
                foreach (SensorBatteryDiagnostics rawSensorBatteryDataRec in questionableBatteryRecs)
                {
                    // The data should be filtered to the correct meter and bay, but we might as well check it
                    if ((rawSensorBatteryDataRec.MeterID != meterId) || (rawSensorBatteryDataRec.BayID != bayNumber))
                        continue;

                    // If a record has any battery voltage value less than 3.3 volts, we consider it an exception
                    bool hasBatteryException = false;

                    if ((!string.IsNullOrEmpty(rawSensorBatteryDataRec.DryBattCurrV)) && (rawSensorBatteryDataRec.GetNumericValue_DryBattCurrV() < 3.3f))
                        hasBatteryException = true;

                    if ((!string.IsNullOrEmpty(rawSensorBatteryDataRec.DryBattMinV)) && (rawSensorBatteryDataRec.GetNumericValue_DryBattMinV() < 3.3f))
                        hasBatteryException = true;

                    if ((!string.IsNullOrEmpty(rawSensorBatteryDataRec.RechargeBattCurrV)) && (rawSensorBatteryDataRec.GetNumericValue_RechargeBattCurrV() < 3.3f))
                        hasBatteryException = true;

                    if ((!string.IsNullOrEmpty(rawSensorBatteryDataRec.RechargeBattMinV)) && (rawSensorBatteryDataRec.GetNumericValue_RechargeBattMinV() < 3.3f))
                        hasBatteryException = true;

                    if (hasBatteryException)
                    {
                        _ReportModel.SensorsWithLowBatteries.Add(rawSensorBatteryDataRec);
                    }
                }
            }


            // Create a bay statistics object for the current bay, and add to our report list
            OpsAdminMaintExceptionsStats_BayAndMeter BayStatsObj = new OpsAdminMaintExceptionsStats_BayAndMeter();
            BayStatsObj.BayID = bayNumber;
            BayStatsObj.MeterID = meterId;
            this._ReportModel.BayStats.Add(BayStatsObj);

            // Now aggregate info about exceptional occupancy state changes for this bay
            foreach (SensorEventAndCommsRecord nextSensorWithoutRecentOccupancyChange in _ReportModel.SensorsWithoutRecentOccupancyEvents)
            {
                // Skip next record if its not for the desired bay
                if ((nextSensorWithoutRecentOccupancyChange.BayID != bayNumber) || (nextSensorWithoutRecentOccupancyChange.MeterID != meterId))
                    continue;

                BayStatsObj.NoRecentSensorEventsCount++;
            }

            // Now aggregate info about exceptional communication/heartbeat events for this bay
            foreach (SensorHeartbeatRecord nextSensorWithoutRecentHeartbeat in _ReportModel.SensorsWithoutRecentComms)
            {
                // Skip next overstay vio record if its not for the desired bay
                if ((nextSensorWithoutRecentHeartbeat.BayID != bayNumber) || (nextSensorWithoutRecentHeartbeat.MeterID != meterId))
                    continue;

                BayStatsObj.NoRecentCommsCount++;
            }

            // Now aggregate info about exceptional battery voltage events for this bay
            foreach (SensorBatteryDiagnostics nextSensorWithLowBattery in _ReportModel.SensorsWithLowBatteries)
            {
                // Skip next overstay vio record if its not for the desired bay
                if ((nextSensorWithLowBattery.BayID != bayNumber) || (nextSensorWithLowBattery.MeterID != meterId))
                    continue;

                BayStatsObj.LowBatteryCount++;
            }
        }

        protected List<SensorEventAndCommsRecord> GetQuestionableOccupancyStateRecsForMeterIDAndBayID(List<SensorEventAndCommsRecord> allOccupancyStateRecs, int meterID, int bayID)
        {
            List<SensorEventAndCommsRecord> result = new List<SensorEventAndCommsRecord>();
            foreach (SensorEventAndCommsRecord nextRec in allOccupancyStateRecs)
            {
                if ((nextRec.MeterID == meterID) && (nextRec.BayID == bayID))
                    result.Add(nextRec);
            }
            return result;
        }

        protected List<SensorHeartbeatRecord> GetQuestionableSensorCommRecsForMeterIDAndBayID(List<SensorHeartbeatRecord> allSensorCommRecs, int meterID, int bayID)
        {
            List<SensorHeartbeatRecord> result = new List<SensorHeartbeatRecord>();
            foreach (SensorHeartbeatRecord nextRec in allSensorCommRecs)
            {
                if ((nextRec.MeterID == meterID) && (nextRec.BayID == bayID))
                    result.Add(nextRec);
            }
            return result;
        }

        protected List<SensorBatteryDiagnostics> GetQuestionableBatteryRecsForMeterIDAndBayID(List<SensorBatteryDiagnostics> allBatteryRecs, int meterID, int bayID)
        {
            List<SensorBatteryDiagnostics> result = new List<SensorBatteryDiagnostics>();
            foreach (SensorBatteryDiagnostics nextRec in allBatteryRecs)
            {
                if ((nextRec.MeterID == meterID) && (nextRec.BayID == bayID))
                    result.Add(nextRec);
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
        #endregion
    }

}