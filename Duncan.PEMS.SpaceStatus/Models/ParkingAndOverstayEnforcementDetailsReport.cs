﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web;

using OfficeOpenXml; // Namespace inside the open source EPPlus.dll from http://epplus.codeplex.com/
using OfficeOpenXml.Style;

using Duncan.PEMS.SpaceStatus.DataShapes;
using Duncan.PEMS.SpaceStatus.DataMappers;
using Duncan.PEMS.SpaceStatus.DataSuppliers;
using Duncan.PEMS.SpaceStatus.UtilityClasses;

namespace Duncan.PEMS.SpaceStatus.Models
{
    
    public class ParkingAndOverstayEnforcementDetailsReport : BaseSensorReport
    {

        #region ParkingAndOverstayGroupStats
        // Derived class that adds more statistics that are specialized for the ParkingAndOverstayEnforcementDetailsReport,
        // and aren't common statistics that will be needed by multiple reports
        public class ParkingAndOverstayGroupStats : GroupStats
        {
            public int TotalOverstaysDuration0To15Mins { get; set; }
            public int TotalOverstaysDuration15To30Mins { get; set; }
            public int TotalOverstaysDuration30To60Mins { get; set; }
            public int TotalOverstaysDurationOver60Mins { get; set; }

            public int TotalOverstaysActioned0To15Mins { get; set; }
            public int TotalOverstaysActioned15To30Mins { get; set; }
            public int TotalOverstaysActioned30To60Mins { get; set; }
            public int TotalOverstaysActionedOver60Mins { get; set; }

            public TimeSpan AverageLatency { get; set; }
            public float PercentVacantDuration { get; set; }
            public float PercentOverstayedCount { get; set; }

            public float PercentOverstayedDuration_0To15Min { get; set; }
            public float PercentOverstayedDuration_15To30Min { get; set; }
            public float PercentOverstayedDuration_30To60Min { get; set; }
            public float PercentOverstayedDuration_Over60Min { get; set; }

            public float PercentOverstaysActioned { get; set; }
            public float PercentOverstaysIssued { get; set; }
            public float PercentOverstaysCautioned { get; set; }
            public float PercentOverstaysNotIssued { get; set; }
            public float PercentOverstaysFault { get; set; }
            public float PercentOverstaysMissed { get; set; }

            public float PercentOverstayedActioned_0To15Min { get; set; }
            public float PercentOverstayedActioned_15To30Min { get; set; }
            public float PercentOverstayedActioned_30To60Min { get; set; }
            public float PercentOverstayedActioned_Over60Min { get; set; }

            public ParkingAndOverstayGroupStats()
                : base()
            {
            }

            public ParkingAndOverstayGroupStats(GroupStats copyFromBase)
                : base()
            {
                // Copy member and property values from the base object
                Type t = copyFromBase.GetType();
                foreach (FieldInfo fieldInf in t.GetFields())
                {
                    fieldInf.SetValue(this, fieldInf.GetValue(copyFromBase));
                }
                foreach (PropertyInfo propInf in t.GetProperties())
                {
                    if (propInf.CanRead && propInf.CanWrite)
                        propInf.SetValue(this, propInf.GetValue(copyFromBase, null), null);
                }
            }

            protected override void InitValues()
            {
                // Do inherited stuff first
                base.InitValues();

                AverageLatency = new TimeSpan();
                PercentVacantDuration = 0.00f;
                PercentOverstayedCount = 0.00f;

                PercentOverstayedDuration_0To15Min = 0.00f;
                PercentOverstayedDuration_15To30Min = 0.00f;
                PercentOverstayedDuration_30To60Min = 0.00f;
                PercentOverstayedDuration_Over60Min = 0.00f;

                PercentOverstaysActioned = 0.00f;
                PercentOverstaysIssued = 0.00f;
                PercentOverstaysCautioned = 0.00f;
                PercentOverstaysNotIssued = 0.00f;
                PercentOverstaysFault = 0.00f;
                PercentOverstaysMissed = 0.00f;

                TotalOverstaysDuration0To15Mins = 0;
                TotalOverstaysDuration15To30Mins = 0;
                TotalOverstaysDuration30To60Mins = 0;
                TotalOverstaysDurationOver60Mins = 0;

                TotalOverstaysActioned0To15Mins = 0;
                TotalOverstaysActioned15To30Mins = 0;
                TotalOverstaysActioned30To60Mins = 0;
                TotalOverstaysActionedOver60Mins = 0;
            }

            public override void AggregateSelf()
            {
                // Do inherited stuff first
                base.AggregateSelf();

                // And now do calcs specific to this child class
                if ((this.ingress + this.egress) > 0)
                    AverageLatency = new TimeSpan(this.TotalSensorEvent_Latency.Ticks / (this.ingress + this.egress));

                if (this.MaximumPotentialOccupancyTime.TotalMilliseconds == 0)
                    PercentVacantDuration = (float)0.00f;
                else
                    PercentVacantDuration = (float)(((this.MaximumPotentialOccupancyTime - this.TotalOccupancyTime).TotalMilliseconds / this.MaximumPotentialOccupancyTime.TotalMilliseconds) * 100.0f);

                if (this.ingress == 0)
                    PercentOverstayedCount = (float)0.00f;
                else
                    PercentOverstayedCount = (float)(((float)this.OverstayCount / (float)this.ingress) * 100.0f);

                if (this.OverstayCount > 0)
                {
                    PercentOverstayedDuration_0To15Min = (float)(((float)this.TotalOverstaysDuration0To15Mins / (float)this.OverstayCount) * 100.0f);
                    PercentOverstayedDuration_15To30Min = (float)(((float)this.TotalOverstaysDuration15To30Mins / (float)this.OverstayCount) * 100.0f);
                    PercentOverstayedDuration_30To60Min = (float)(((float)this.TotalOverstaysDuration30To60Mins / (float)this.OverstayCount) * 100.0f);
                    PercentOverstayedDuration_Over60Min = (float)(((float)this.TotalOverstaysDurationOver60Mins / (float)this.OverstayCount) * 100.0f);

                    PercentOverstaysActioned = (float)(((float)this.TotalOverstaysActioned / (float)this.OverstayCount) * 100.0f);
                    PercentOverstaysIssued = (float)(((float)this.TotalOverstaysEnforced / (float)this.OverstayCount) * 100.0f);
                    PercentOverstaysCautioned = (float)(((float)this.TotalOverstaysCautioned / (float)this.OverstayCount) * 100.0f);
                    PercentOverstaysNotIssued = (float)(((float)this.TotalOverstaysNotEnforced / (float)this.OverstayCount) * 100.0f);
                    PercentOverstaysFault = (float)(((float)this.TotalOverstaysFaulty / (float)this.OverstayCount) * 100.0f);

                    PercentOverstaysMissed = (float)((((float)this.OverstayCount - (float)this.TotalOverstaysActioned) / (float)this.OverstayCount) * 100.0f);

                    PercentOverstayedActioned_0To15Min = (float)(((float)this.TotalOverstaysActioned0To15Mins / (float)this.OverstayCount) * 100.0f);
                    PercentOverstayedActioned_15To30Min = (float)(((float)this.TotalOverstaysActioned15To30Mins / (float)this.OverstayCount) * 100.0f);
                    PercentOverstayedActioned_30To60Min = (float)(((float)this.TotalOverstaysActioned30To60Mins / (float)this.OverstayCount) * 100.0f);
                    PercentOverstayedActioned_Over60Min = (float)(((float)this.TotalOverstaysActionedOver60Mins / (float)this.OverstayCount) * 100.0f);
                }

            }
        }

        #endregion

        public static new bool SupportsHourlyStatistics = false;
        public static new bool SupportsAreaSummary = false;
        public static new bool SupportsMeterSummary = false;
        public static new bool SupportsSpaceSummary = false;
        public static new bool SupportsDailySummary = false;
        public static new bool SupportsMonthlySummary = false;
        public static new bool SupportsDetailRecords = true;

        public ParkingAndOverstayEnforcementDetailsReport(CustomerConfig customerCfg, SensorAndPaymentReportEngine.CommonReportParameters reportParams)
            : base(customerCfg, reportParams)
        {
            _ReportName = "Parking and Overstay Enforcement Details Report";
            numberOfHourlyCategories = 0;
        }

        public void GetReportAsExcelSpreadsheet(List<int> listOfMeterIDs, MemoryStream ms, CustomerLogic result)
        {
            timeIsolation.IsolationType = SensorAndPaymentReportEngine.TimeIsolations.None;

            // Start diagnostics timer
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            DateTime NowAtDestination = Convert.ToDateTime(this._CustomerConfig.DestinationTimeZoneDisplayName);

            // Now gather and analyze data for the report
            SensorAndPaymentReportEngine.RequiredDataElements requiredDataElements = new SensorAndPaymentReportEngine.RequiredDataElements();
            requiredDataElements.NeedsSensorData = true;
            requiredDataElements.NeedsPaymentData = false;
            requiredDataElements.NeedsOverstayData = true;
            requiredDataElements.NeedsEnforcementActionData = true;

            this._ReportEngine = new SensorAndPaymentReportEngine(this._CustomerConfig, this._ReportParams);
            this._ReportEngine.GatherReportData(listOfMeterIDs, requiredDataElements, result);

            OfficeOpenXml.ExcelWorksheet ws = null;

            using (OfficeOpenXml.ExcelPackage pck = new OfficeOpenXml.ExcelPackage())
            {
                // Let's create a report coversheet and overall summary page, with hyperlinks to the other worksheets
                ws = pck.Workbook.Worksheets.Add("Details");

                // Render the standard report title lines
                rowIdx = 1; // Excel uses 1-based indexes
                colIdx = 1;
                RenderCommonReportTitle(ws, this._ReportName);

                // Render common report header for enforcement activity restriction filter, but only if its not for all activity
                if (this._ReportParams.ActionTakenRestrictionFilter != SensorAndPaymentReportEngine.ReportableEnforcementActivity.AllActivity)
                {
                    rowIdx++;
                    colIdx = 1;
                    RenderCommonReportFilterHeader_ActionTakenRestrictions(ws);
                }

                // Render common report header for regulated hour restriction filter
                rowIdx++;
                colIdx = 1;
                RenderCommonReportFilterHeader_RegulatedHourRestrictions(ws);

                using (OfficeOpenXml.ExcelRange rng = ws.Cells[2, 1, rowIdx, numColumnsMergedForHeader])
                {
                    rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                    rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(207, 221, 237));  //Set color to lighter blue FromArgb(184, 204, 228)
                }

                rowIdx++;
                rowIdx++;
                colIdx = 1;

                int detailsStartRow = rowIdx;
                int detailColumnCount = 29;

                // Render the header row
                ws.SetValue(rowIdx, 1, "Space #");
                ws.SetValue(rowIdx, 2, "Meter #");
                ws.SetValue(rowIdx, 3, "Area #");
                ws.SetValue(rowIdx, 4, "Area");
                ws.SetValue(rowIdx, 5, "Event Timestamp");
                ws.SetValue(rowIdx, 6, "Record Timestamp");
                ws.SetValue(rowIdx, 7, "Latency");
                ws.SetValue(rowIdx, 8, "Occupied");
                ws.SetValue(rowIdx, 9, "Vacant Duration");
                ws.SetValue(rowIdx, 10, "Time Arrived");
                ws.SetValue(rowIdx, 11, "Time Departed");
                ws.SetValue(rowIdx, 12, "Parked Duration");
                ws.SetValue(rowIdx, 13, "Max Stay Regulation");
                ws.SetValue(rowIdx, 14, "Overstay Violation");
                ws.SetValue(rowIdx, 15, "Overstay Duration");
                ws.SetValue(rowIdx, 16, "Overstay (0-15min)");
                ws.SetValue(rowIdx, 17, "Overstay (15-30min)");
                ws.SetValue(rowIdx, 18, "Overstay (30-60min)");
                ws.SetValue(rowIdx, 19, "Overstay (>60min)");
                ws.SetValue(rowIdx, 20, "Violation Actioned");
                ws.SetValue(rowIdx, 21, "Violation Issued");
                ws.SetValue(rowIdx, 22, "Violation Warning");
                ws.SetValue(rowIdx, 23, "Violation Not Issued");
                ws.SetValue(rowIdx, 24, "Violation Fault");
                ws.SetValue(rowIdx, 25, "Violation Missed");
                ws.SetValue(rowIdx, 26, "Capture Rate (0-15min)");
                ws.SetValue(rowIdx, 27, "Capture Rate (15-30min)");
                ws.SetValue(rowIdx, 28, "Capture Rate (30-60min)");
                ws.SetValue(rowIdx, 29, "Capture Rate (>60min)");

                // Format the header row
                using (OfficeOpenXml.ExcelRange rng = ws.Cells[detailsStartRow, 1, detailsStartRow, 4])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                    rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));  //Set color to dark blue
                    rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
                }
                using (OfficeOpenXml.ExcelRange rng = ws.Cells[detailsStartRow, 5, detailsStartRow, 8])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(36, 64, 98));
                    rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
                }
                using (OfficeOpenXml.ExcelRange rng = ws.Cells[detailsStartRow, 9, detailsStartRow, 13])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 176, 80));
                    rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
                }
                using (OfficeOpenXml.ExcelRange rng = ws.Cells[detailsStartRow, 14, detailsStartRow, 29])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(150, 54, 52));
                    rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
                }

                // Increment the row index, which will now be the 1st row of our data
                rowIdx++;
                detailsStartRow = rowIdx;

                timeIsolation.IsolationType = SensorAndPaymentReportEngine.TimeIsolations.None;
                GroupStats baseTotalStats = this._ReportEngine.GetOverallStats(timeIsolation);
                ParkingAndOverstayGroupStats totalStats = new ParkingAndOverstayGroupStats(baseTotalStats);

                #region Populate data for each record
                foreach (AreaAsset areaAsset in this._ReportEngine.ReportDataModel.AreasIncludedInReport)
                {
                    GroupStats baseAreaStats = this._ReportEngine.GetAreaStats(areaAsset.AreaID, timeIsolation);
                    ParkingAndOverstayGroupStats areaStats = new ParkingAndOverstayGroupStats(baseAreaStats);

                    foreach (SpaceAsset spaceAsset in this._ReportEngine.ReportDataModel.SpacesIncludedInReport)
                    {
                        // Skip record if its not applicable to the current area we are processing
                        if (spaceAsset.AreaID_PreferLibertyBeforeInternal != areaAsset.AreaID)
                            continue;

                        List<SensorAndPaymentReportEngine.CommonSensorAndPaymentEvent> spaceRecs = this._ReportEngine.ReportDataModel.FindRecsForBayAndMeter(spaceAsset.SpaceID, spaceAsset.MeterID);

                        TimeSpan previousVacantDuration = new TimeSpan(0);
                        SensorAndPaymentReportEngine.CommonSensorAndPaymentEvent previousOccupiedEvent = null;
                        foreach (SensorAndPaymentReportEngine.CommonSensorAndPaymentEvent repEvents in spaceRecs)
                        {
                            ws.SetValue(rowIdx, 1, repEvents.BayInfo.SpaceID);
                            ws.SetValue(rowIdx, 2, repEvents.BayInfo.MeterID);
                            if (areaAsset != null)
                            {
                                ws.SetValue(rowIdx, 3, areaAsset.AreaID);
                                ws.SetValue(rowIdx, 4, areaAsset.AreaName);
                            }

                            ws.SetValue(rowIdx, 5, repEvents.SensorEvent_Start);
                            ws.SetValue(rowIdx, 6, repEvents.SensorEvent_RecCreationDateTime);
                            ws.SetValue(rowIdx, 7, FormatTimeSpanAsHoursMinutesAndSeconds(repEvents.SensorEvent_Latency));
                            if (repEvents.SensorEvent_IsOccupied == true)
                            {
                                previousOccupiedEvent = repEvents;

                                ws.SetValue(rowIdx, 8, 1); // 1 for numeric version of "True"

                                if (previousVacantDuration.Ticks > 0)
                                    ws.SetValue(rowIdx, 9, FormatTimeSpanAsHoursMinutesAndSeconds(previousVacantDuration));

                                ws.SetValue(rowIdx, 10, repEvents.SensorEvent_Start);
                            }
                            else
                            {
                                ws.SetValue(rowIdx, 8, 0); // 1 for numeric version of "False"
                                ws.SetValue(rowIdx, 11, repEvents.SensorEvent_Start);

                                if (previousOccupiedEvent != null)
                                {
                                    bool firstOverstay = true;

                                    if (previousOccupiedEvent.Overstays.Count == 0)
                                    {
                                        ws.SetValue(rowIdx, 11, previousOccupiedEvent.SensorEvent_Start);
                                        ws.SetValue(rowIdx, 12, FormatTimeSpanAsHoursMinutesAndSeconds(previousOccupiedEvent.SensorEvent_Duration));

                                        RegulatedHoursDetail ruleForEvent = GetRegulationRuleAtEventTime(repEvents);
                                        if (ruleForEvent != null)
                                        {
                                            StringBuilder sb = new StringBuilder();
                                            sb.Append(ruleForEvent.MaxStayMinutes.ToString());
                                            ws.SetValue(rowIdx, 13, sb.ToString());
                                        }

                                        ws.SetValue(rowIdx, 14, "N");
                                    }

                                    foreach (SensorAndPaymentReportEngine.OverstayVioEvent overstay in previousOccupiedEvent.Overstays)
                                    {
                                        if (firstOverstay == false)
                                        {
                                            // Need to start new row and repeat the header info!
                                            rowIdx++;

                                            ws.SetValue(rowIdx, 1, previousOccupiedEvent.BayInfo.SpaceID);
                                            ws.SetValue(rowIdx, 2, previousOccupiedEvent.BayInfo.MeterID);
                                            if (areaAsset != null)
                                            {
                                                ws.SetValue(rowIdx, 3, areaAsset.AreaID);
                                                ws.SetValue(rowIdx, 4, areaAsset.AreaName);
                                            }

                                            ws.SetValue(rowIdx, 5, previousOccupiedEvent.SensorEvent_Start);
                                            ws.SetValue(rowIdx, 6, previousOccupiedEvent.SensorEvent_RecCreationDateTime);
                                            ws.SetValue(rowIdx, 7, FormatTimeSpanAsHoursMinutesAndSeconds(previousOccupiedEvent.SensorEvent_Latency));
                                            ws.SetValue(rowIdx, 8, 0); // 1 for numeric version of "False"
                                            ws.SetValue(rowIdx, 11, previousOccupiedEvent.SensorEvent_Start);
                                        }

                                        ws.SetValue(rowIdx, 12, FormatTimeSpanAsHoursMinutesAndSeconds(previousOccupiedEvent.SensorEvent_Duration));
                                        if (overstay.OverstayBasedOnRuleDetail != null)
                                        {
                                            StringBuilder sb = new StringBuilder();
                                            sb.Append(overstay.OverstayBasedOnRuleDetail.MaxStayMinutes.ToString());
                                            ws.SetValue(rowIdx, 13, sb.ToString());
                                        }

                                        ws.SetValue(rowIdx, 14, "Y");
                                        ws.SetValue(rowIdx, 15, FormatTimeSpanAsHoursMinutesAndSeconds(overstay.DurationOfTimeBeyondStayLimits));

                                        if (overstay.DurationOfTimeBeyondStayLimits.TotalMinutes < 15)
                                        {
                                            ws.SetValue(rowIdx, 16, "Y");
                                            areaStats.TotalOverstaysDuration0To15Mins++;
                                            totalStats.TotalOverstaysDuration0To15Mins++;
                                        }
                                        else if (overstay.DurationOfTimeBeyondStayLimits.TotalMinutes < 30)
                                        {
                                            ws.SetValue(rowIdx, 17, "Y");
                                            areaStats.TotalOverstaysDuration15To30Mins++;
                                            totalStats.TotalOverstaysDuration15To30Mins++;
                                        }
                                        else if (overstay.DurationOfTimeBeyondStayLimits.TotalMinutes < 60)
                                        {
                                            ws.SetValue(rowIdx, 18, "Y");
                                            areaStats.TotalOverstaysDuration30To60Mins++;
                                            totalStats.TotalOverstaysDuration30To60Mins++;
                                        }
                                        else
                                        {
                                            ws.SetValue(rowIdx, 19, "Y");
                                            areaStats.TotalOverstaysDurationOver60Mins++;
                                            totalStats.TotalOverstaysDurationOver60Mins++;
                                        }


                                        if (!string.IsNullOrEmpty(overstay.EnforcementActionTaken))
                                            ws.SetValue(rowIdx, 20, "Y");
                                        else
                                            ws.SetValue(rowIdx, 20, "N");


                                        if (string.Compare(overstay.EnforcementActionTaken, "Enforced", true) == 0)
                                            ws.SetValue(rowIdx, 21, "Y");
                                        else
                                            ws.SetValue(rowIdx, 21, "N");


                                        if (string.Compare(overstay.EnforcementActionTaken, "Cautioned", true) == 0)
                                            ws.SetValue(rowIdx, 22, "Y");
                                        else
                                            ws.SetValue(rowIdx, 22, "N");


                                        if (string.Compare(overstay.EnforcementActionTaken, "NotEnforced", true) == 0)
                                            ws.SetValue(rowIdx, 23, "Y");
                                        else
                                            ws.SetValue(rowIdx, 23, "N");


                                        if (string.Compare(overstay.EnforcementActionTaken, "Fault", true) == 0)
                                            ws.SetValue(rowIdx, 24, "Y");
                                        else
                                            ws.SetValue(rowIdx, 24, "N");


                                        if (string.IsNullOrEmpty(overstay.EnforcementActionTaken))
                                        {
                                            ws.SetValue(rowIdx, 25, "Y");

                                            ws.SetValue(rowIdx, 26, "N");
                                            ws.SetValue(rowIdx, 27, "N");
                                            ws.SetValue(rowIdx, 28, "N");
                                            ws.SetValue(rowIdx, 29, "N");
                                        }
                                        else
                                        {
                                            ws.SetValue(rowIdx, 25, "N");

                                            ws.SetValue(rowIdx, 26, "N");
                                            ws.SetValue(rowIdx, 27, "N");
                                            ws.SetValue(rowIdx, 28, "N");
                                            ws.SetValue(rowIdx, 29, "N");

                                            TimeSpan captureRate = (overstay.EnforcementActionTakenTimeStamp - overstay.StartOfOverstayViolation);
                                            if (captureRate.TotalMinutes < 15)
                                            {
                                                ws.SetValue(rowIdx, 26, "Y");
                                                areaStats.TotalOverstaysActioned0To15Mins++;
                                                totalStats.TotalOverstaysActioned0To15Mins++;
                                            }
                                            else if (captureRate.TotalMinutes < 30)
                                            {
                                                ws.SetValue(rowIdx, 27, "Y");
                                                areaStats.TotalOverstaysActioned15To30Mins++;
                                                totalStats.TotalOverstaysActioned15To30Mins++;
                                            }
                                            else if (captureRate.TotalMinutes < 60)
                                            {
                                                ws.SetValue(rowIdx, 28, "Y");
                                                areaStats.TotalOverstaysActioned30To60Mins++;
                                                totalStats.TotalOverstaysActioned30To60Mins++;
                                            }
                                            else
                                            {
                                                ws.SetValue(rowIdx, 29, "Y");
                                                areaStats.TotalOverstaysActionedOver60Mins++;
                                                totalStats.TotalOverstaysActionedOver60Mins++;
                                            }
                                        }

                                        // Set flag so we know we're no longer dealing with the first overstay of this occupied event
                                        firstOverstay = false;
                                    }
                                }
                            }

                            if (repEvents.SensorEvent_IsOccupied == false)
                            {
                                previousVacantDuration = new TimeSpan(repEvents.SensorEvent_Duration.Ticks);
                            }

                            // Increment the row index, which will now be the next row of our data
                            rowIdx++;
                        }
                    }

                    // Finish the area aggregations
                    areaStats.AggregateSelf();

                    colIdx = 1;
                    ws.SetValue(rowIdx, colIdx, "SUBTOTAL AREA");
                    MergeCellRange(ws, rowIdx, colIdx, rowIdx, 4);
                    using (OfficeOpenXml.ExcelRange rng = ws.Cells[rowIdx, colIdx, rowIdx, 29])
                    {
                        rng.Style.Font.Bold = true;
                    }

                    ws.SetValue(rowIdx, 7, FormatTimeSpanAsHoursMinutesAndSeconds(areaStats.AverageLatency));
                    ws.SetValue(rowIdx, 8, areaStats.ingress);
                    ws.SetValue(rowIdx, 9, areaStats.PercentVacantDuration.ToString("##0.00") + "%");
                    ws.SetValue(rowIdx, 12, areaStats.PercentageOccupancy.ToString("##0.00") + "%");
                    ws.SetValue(rowIdx, 14, areaStats.PercentOverstayedCount.ToString("##0.00") + "%");
                    ws.SetValue(rowIdx, 15, areaStats.PercentageOverstayedDuration.ToString("##0.00") + "%");
                    ws.SetValue(rowIdx, 16, areaStats.PercentOverstayedDuration_0To15Min.ToString("##0.00") + "%");
                    ws.SetValue(rowIdx, 17, areaStats.PercentOverstayedDuration_15To30Min.ToString("##0.00") + "%");
                    ws.SetValue(rowIdx, 18, areaStats.PercentOverstayedDuration_30To60Min.ToString("##0.00") + "%");
                    ws.SetValue(rowIdx, 19, areaStats.PercentOverstayedDuration_Over60Min.ToString("##0.00") + "%");
                    ws.SetValue(rowIdx, 20, areaStats.PercentOverstaysActioned.ToString("##0.00") + "%");
                    ws.SetValue(rowIdx, 21, areaStats.PercentOverstaysIssued.ToString("##0.00") + "%");
                    ws.SetValue(rowIdx, 22, areaStats.PercentOverstaysCautioned.ToString("##0.00") + "%");
                    ws.SetValue(rowIdx, 23, areaStats.PercentOverstaysNotIssued.ToString("##0.00") + "%");
                    ws.SetValue(rowIdx, 24, areaStats.PercentOverstaysFault.ToString("##0.00") + "%");
                    ws.SetValue(rowIdx, 25, areaStats.PercentOverstaysMissed.ToString("##0.00") + "%");
                    ws.SetValue(rowIdx, 26, areaStats.PercentOverstayedDuration_0To15Min.ToString("##0.00") + "%");
                    ws.SetValue(rowIdx, 27, areaStats.PercentOverstayedDuration_15To30Min.ToString("##0.00") + "%");
                    ws.SetValue(rowIdx, 28, areaStats.PercentOverstayedDuration_30To60Min.ToString("##0.00") + "%");
                    ws.SetValue(rowIdx, 29, areaStats.PercentOverstayedDuration_Over60Min.ToString("##0.00") + "%");

                    rowIdx++;
                    rowIdx++;
                }

                // Finish the total aggregations
                totalStats.AggregateSelf();

                colIdx = 1;
                ws.SetValue(rowIdx, colIdx, "TOTAL");
                MergeCellRange(ws, rowIdx, colIdx, rowIdx, 4);
                using (OfficeOpenXml.ExcelRange rng = ws.Cells[rowIdx, colIdx, rowIdx, 29])
                {
                    rng.Style.Font.Bold = true;
                }

                ws.SetValue(rowIdx, 7, FormatTimeSpanAsHoursMinutesAndSeconds(totalStats.AverageLatency));
                ws.SetValue(rowIdx, 8, totalStats.ingress);
                ws.SetValue(rowIdx, 9, totalStats.PercentVacantDuration.ToString("##0.00") + "%");
                ws.SetValue(rowIdx, 12, totalStats.PercentageOccupancy.ToString("##0.00") + "%");
                ws.SetValue(rowIdx, 14, totalStats.PercentOverstayedCount.ToString("##0.00") + "%");
                ws.SetValue(rowIdx, 15, totalStats.PercentageOverstayedDuration.ToString("##0.00") + "%");
                ws.SetValue(rowIdx, 16, totalStats.PercentOverstayedDuration_0To15Min.ToString("##0.00") + "%");
                ws.SetValue(rowIdx, 17, totalStats.PercentOverstayedDuration_15To30Min.ToString("##0.00") + "%");
                ws.SetValue(rowIdx, 18, totalStats.PercentOverstayedDuration_30To60Min.ToString("##0.00") + "%");
                ws.SetValue(rowIdx, 19, totalStats.PercentOverstayedDuration_Over60Min.ToString("##0.00") + "%");
                ws.SetValue(rowIdx, 20, totalStats.PercentOverstaysActioned.ToString("##0.00") + "%");
                ws.SetValue(rowIdx, 21, totalStats.PercentOverstaysIssued.ToString("##0.00") + "%");
                ws.SetValue(rowIdx, 22, totalStats.PercentOverstaysCautioned.ToString("##0.00") + "%");
                ws.SetValue(rowIdx, 23, totalStats.PercentOverstaysNotIssued.ToString("##0.00") + "%");
                ws.SetValue(rowIdx, 24, totalStats.PercentOverstaysFault.ToString("##0.00") + "%");
                ws.SetValue(rowIdx, 25, totalStats.PercentOverstaysMissed.ToString("##0.00") + "%");
                ws.SetValue(rowIdx, 26, totalStats.PercentOverstayedDuration_0To15Min.ToString("##0.00") + "%");
                ws.SetValue(rowIdx, 27, totalStats.PercentOverstayedDuration_15To30Min.ToString("##0.00") + "%");
                ws.SetValue(rowIdx, 28, totalStats.PercentOverstayedDuration_30To60Min.ToString("##0.00") + "%");
                ws.SetValue(rowIdx, 29, totalStats.PercentOverstayedDuration_Over60Min.ToString("##0.00") + "%");

                rowIdx++;
                rowIdx++;

                #endregion

                // AutoFilters aren't suitable for this report
                /*
                // We will add autofilters to our headers so user can sort the columns easier
                using (OfficeOpenXml.ExcelRange rng = ws.Cells[detailsStartRow - 1, 1, rowIdx, detailColumnCount])
                {
                    rng.AutoFilter = true;
                }
                */

                ApplyNumberStyleToColumn(ws, 1, detailsStartRow, rowIdx, "########0", ExcelHorizontalAlignment.Left);
                ApplyNumberStyleToColumn(ws, 2, detailsStartRow, rowIdx, "########0", ExcelHorizontalAlignment.Left);
                ApplyNumberStyleToColumn(ws, 3, detailsStartRow, rowIdx, "########0", ExcelHorizontalAlignment.Left);
                ApplyNumberStyleToColumn(ws, 4, detailsStartRow, rowIdx, "@", ExcelHorizontalAlignment.Left);
                ApplyNumberStyleToColumn(ws, 5, detailsStartRow, rowIdx, "yyyy-mm-dd hh:mm:ss tt", ExcelHorizontalAlignment.Right);
                ApplyNumberStyleToColumn(ws, 6, detailsStartRow, rowIdx, "yyyy-mm-dd hh:mm:ss tt", ExcelHorizontalAlignment.Right);
                ApplyNumberStyleToColumn(ws, 7, detailsStartRow, rowIdx, "@", ExcelHorizontalAlignment.Right);
                ApplyNumberStyleToColumn(ws, 8, detailsStartRow, rowIdx, "########0", ExcelHorizontalAlignment.Right);
                ApplyNumberStyleToColumn(ws, 9, detailsStartRow, rowIdx, "@", ExcelHorizontalAlignment.Right);
                ApplyNumberStyleToColumn(ws, 10, detailsStartRow, rowIdx, "yyyy-mm-dd hh:mm:ss tt", ExcelHorizontalAlignment.Right);
                ApplyNumberStyleToColumn(ws, 11, detailsStartRow, rowIdx, "yyyy-mm-dd hh:mm:ss tt", ExcelHorizontalAlignment.Right);
                ApplyNumberStyleToColumn(ws, 12, detailsStartRow, rowIdx, "@", ExcelHorizontalAlignment.Right);
                ApplyNumberStyleToColumn(ws, 13, detailsStartRow, rowIdx, "@", ExcelHorizontalAlignment.Right);
                ApplyNumberStyleToColumn(ws, 14, detailsStartRow, rowIdx, "@", ExcelHorizontalAlignment.Right);
                ApplyNumberStyleToColumn(ws, 15, detailsStartRow, rowIdx, "@", ExcelHorizontalAlignment.Right);
                ApplyNumberStyleToColumn(ws, 16, detailsStartRow, rowIdx, "@", ExcelHorizontalAlignment.Right);
                ApplyNumberStyleToColumn(ws, 17, detailsStartRow, rowIdx, "@", ExcelHorizontalAlignment.Right);
                ApplyNumberStyleToColumn(ws, 18, detailsStartRow, rowIdx, "@", ExcelHorizontalAlignment.Right);
                ApplyNumberStyleToColumn(ws, 19, detailsStartRow, rowIdx, "@", ExcelHorizontalAlignment.Right);
                ApplyNumberStyleToColumn(ws, 20, detailsStartRow, rowIdx, "@", ExcelHorizontalAlignment.Right);
                ApplyNumberStyleToColumn(ws, 21, detailsStartRow, rowIdx, "@", ExcelHorizontalAlignment.Right);
                ApplyNumberStyleToColumn(ws, 22, detailsStartRow, rowIdx, "@", ExcelHorizontalAlignment.Right);
                ApplyNumberStyleToColumn(ws, 23, detailsStartRow, rowIdx, "@", ExcelHorizontalAlignment.Right);
                ApplyNumberStyleToColumn(ws, 24, detailsStartRow, rowIdx, "@", ExcelHorizontalAlignment.Right);
                ApplyNumberStyleToColumn(ws, 25, detailsStartRow, rowIdx, "@", ExcelHorizontalAlignment.Right);
                ApplyNumberStyleToColumn(ws, 26, detailsStartRow, rowIdx, "@", ExcelHorizontalAlignment.Right);
                ApplyNumberStyleToColumn(ws, 27, detailsStartRow, rowIdx, "@", ExcelHorizontalAlignment.Right);
                ApplyNumberStyleToColumn(ws, 28, detailsStartRow, rowIdx, "@", ExcelHorizontalAlignment.Right);
                ApplyNumberStyleToColumn(ws, 29, detailsStartRow, rowIdx, "@", ExcelHorizontalAlignment.Right);

                // And now lets size the columns
                for (int autoSizeColIdx = 1; autoSizeColIdx <= detailColumnCount; autoSizeColIdx++)
                {
                    using (OfficeOpenXml.ExcelRange col = ws.Cells[detailsStartRow - 1, autoSizeColIdx, rowIdx, autoSizeColIdx])
                    {
                        col.AutoFitColumns();
                    }
                }

                // And finally we will freeze the header rows for nicer scrolling
                ws.View.FreezePanes(7, 1);

                // All cells in spreadsheet are populated now, so render (save the file) to a memory stream 
                byte[] bytes = pck.GetAsByteArray();
                ms.Write(bytes, 0, bytes.Length);
            }

            // Stop diagnostics timer
            sw.Stop();
            System.Diagnostics.Debug.WriteLine(this._ReportName + " generation took: " + sw.Elapsed.ToString());
        }

        private RegulatedHoursDetail GetRegulationRuleAtEventTime(SensorAndPaymentReportEngine.CommonSensorAndPaymentEvent currentReportableEvent)
        {
            RegulatedHoursDetail result = null;

            // Try to obtain the regulated hours applicable to this meter
            RegulatedHoursGroup regulatedHours = this._ReportEngine.GetBestGroupForMeter(this._CustomerConfig.CustomerId, currentReportableEvent.BayInfo.AreaID_PreferLibertyBeforeInternal, currentReportableEvent.BayInfo.MeterID);

            // If no regulated hour defintions came back, then we are unable to calculate any overstay violation, so just exit
            if ((regulatedHours == null) || (regulatedHours.Details == null) || (regulatedHours.Details.Count == 0))
                return result;

            DateTime ruleStart = DateTime.MinValue;
            DateTime ruleEnd = DateTime.MinValue;

            TimeSlot OccupiedSegment = new TimeSlot(currentReportableEvent.SensorEvent_Start, currentReportableEvent.SensorEvent_End);

            // We need to check if this single occupancy event is an overstay violation for multiple rules, or even for more than one day, etc.
            while (OccupiedSegment.Start < currentReportableEvent.SensorEvent_End)
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
                        result = detail;
                        return result;
                    }
                }

                // Nothing more to do, so break out of loop!
                break; 
            }

            return result;
        }
    
    }

}