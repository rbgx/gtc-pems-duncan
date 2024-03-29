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
using Duncan.PEMS.SpaceStatus.DataMappers;
using Duncan.PEMS.SpaceStatus.DataSuppliers;
using Duncan.PEMS.SpaceStatus.UtilityClasses;


namespace Duncan.PEMS.SpaceStatus.Models
{

    public class CompliancePaymentReport : BaseSensorReport
    {
        public CompliancePaymentReport(CustomerConfig customerCfg, SensorAndPaymentReportEngine.CommonReportParameters reportParams)
            : base(customerCfg, reportParams)
        {
            _ReportName = "Compliance - Payment Report";
            numberOfHourlyCategories = 16;

            if (this._ReportParams.IncludeHourlyStatistics == false)
                numberOfHourlyCategories = 0;
        }

        public void GetReportAsExcelSpreadsheet(List<int> listOfMeterIDs, MemoryStream ms,CustomerLogic result)
        {
            timeIsolation.IsolationType = SensorAndPaymentReportEngine.TimeIsolations.None;

            // Start diagnostics timer
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            DateTime NowAtDestination = Convert.ToDateTime(this._CustomerConfig.DestinationTimeZoneDisplayName);

            // Now gather and analyze data for the report
            SensorAndPaymentReportEngine.RequiredDataElements requiredDataElements = new SensorAndPaymentReportEngine.RequiredDataElements();
            requiredDataElements.NeedsSensorData = true;
            requiredDataElements.NeedsPaymentData = true;
            requiredDataElements.NeedsOverstayData = false;
            requiredDataElements.NeedsEnforcementActionData = true;

            this._ReportEngine = new SensorAndPaymentReportEngine(this._CustomerConfig, this._ReportParams);
            this._ReportEngine.GatherReportData(listOfMeterIDs, requiredDataElements, result);

            OfficeOpenXml.ExcelWorksheet ws = null;

            using (OfficeOpenXml.ExcelPackage pck = new OfficeOpenXml.ExcelPackage())
            {
                // Let's create a report coversheet and overall summary page, with hyperlinks to the other worksheets
                ws = pck.Workbook.Worksheets.Add("Summary");

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
                colIdx = 1;
                int hyperlinkstartRowIdx = rowIdx;

                if (_ReportParams.IncludeMeterSummary == true)
                    RenderWorksheetHyperlink(ws, "Meter Payment", "Meter Payment summary");
                if (_ReportParams.IncludeSpaceSummary == true)
                    RenderWorksheetHyperlink(ws, "Space Payment", "Space Payment summary");
                if (_ReportParams.IncludeAreaSummary == true)
                    RenderWorksheetHyperlink(ws, "Area Payment", "Area Payment summary");
                if (_ReportParams.IncludeDailySummary == true)
                    RenderWorksheetHyperlink(ws, "Daily Payment", "Daily Payment summary");
                if (_ReportParams.IncludeMonthlySummary == true)
                    RenderWorksheetHyperlink(ws, "Monthly Payment", "Monthly Payment summary");
                if (_ReportParams.IncludeDetailRecords == true)
                    RenderWorksheetHyperlink(ws, "Details", "Payment details");

                rowIdx++;
                rowIdx++;
                colIdx = 1;

                using (OfficeOpenXml.ExcelRange rng = ws.Cells[hyperlinkstartRowIdx, 1, rowIdx, numColumnsMergedForHeader])
                {
                    rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                    rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                }

                // Now start the report data summary header
                RenderOverallReportSummary(ws);

                //  --- END OF OVERALL SUMMARY WORKSHEET ---

                // Should we include a worksheet with Meter aggregates?
                if (_ReportParams.IncludeMeterSummary == true)
                {
                    RenderMeterSummaryWorksheet(pck, "Meter Payment");
                }

                // Should we include a worksheet with Space aggregates?
                if (_ReportParams.IncludeSpaceSummary == true)
                {
                    RenderSpaceSummaryWorksheet(pck, "Space Payment");
                }

                // Should we include a worksheet with Area aggregates?
                if (_ReportParams.IncludeAreaSummary == true)
                {
                    RenderAreaSummaryWorksheet(pck, "Area Payment");
                }

                // Should we include a worksheet with Daily aggregates?
                if (_ReportParams.IncludeDailySummary == true)
                {
                    RenderDailySummaryWorksheet(pck, "Daily Payment");
                }

                // Should we include a worksheet with Monthly aggregates?
                if (_ReportParams.IncludeDailySummary == true)
                {
                    RenderMonthlySummaryWorksheet(pck, "Monthly Payment");
                }



                // Should we include a Details worksheet?
                if (_ReportParams.IncludeDetailRecords == true)
                {
                    // Create the worksheet
                    ws = pck.Workbook.Worksheets.Add("Details");
                    int detailColumnCount = 7;

                    // Render the header row
                    rowIdx = 1; // Excel uses 1-based indexes
                    ws.SetValue(rowIdx, 1, "Space #");
                    ws.SetValue(rowIdx, 2, "Meter #");
                    ws.SetValue(rowIdx, 3, "Area #");
                    ws.SetValue(rowIdx, 4, "Area");
                    ws.SetValue(rowIdx, 5, "Event Time");
                    ws.SetValue(rowIdx, 6, "Event Type");
                    ws.SetValue(rowIdx, 7, "Event Info");

                    // Format the header row
                    using (OfficeOpenXml.ExcelRange rng = ws.Cells[1, 1, 1, detailColumnCount])
                    {
                        rng.Style.Font.Bold = true;
                        rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;                 //Set Pattern for the background to Solid
                        rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));  //Set color to dark blue
                        rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    }

                    // Increment the row index, which will now be the 1st row of our data
                    rowIdx++;

                    #region Populate data for each record

                    foreach (SpaceAsset spaceAsset in this._ReportEngine.ReportDataModel.SpacesIncludedInReport)
                    {
                        List<SensorAndPaymentReportEngine.CommonSensorAndPaymentEvent> spaceRecs = this._ReportEngine.ReportDataModel.FindRecsForBayAndMeter(spaceAsset.SpaceID, spaceAsset.MeterID);
                        foreach (SensorAndPaymentReportEngine.CommonSensorAndPaymentEvent nextEvent in spaceRecs)
                        {
                            AreaAsset areaAsset = _ReportEngine.GetAreaAsset(spaceAsset.AreaID_PreferLibertyBeforeInternal);

                            if (nextEvent.IsDummySensorEvent == false)
                            {
                                // Output row values for data
                                ws.SetValue(rowIdx, 1, spaceAsset.SpaceID);
                                ws.SetValue(rowIdx, 2, spaceAsset.MeterID);

                                if (areaAsset != null)
                                {
                                    ws.SetValue(rowIdx, 3, areaAsset.AreaID);
                                    ws.SetValue(rowIdx, 4, areaAsset.AreaName);
                                }

                                ws.SetValue(rowIdx, 5, nextEvent.SensorEvent_Start);
                                if (nextEvent.SensorEvent_IsOccupied)
                                {
                                    ws.SetValue(rowIdx, 6, "Arrival");
                                    ws.SetValue(rowIdx, 7, "Stayed Duration: " + FormatTimeSpanAsHoursMinutesAndSeconds(nextEvent.SensorEvent_Duration));
                                }
                                else
                                {
                                    ws.SetValue(rowIdx, 6, "Departure");
                                    ws.SetValue(rowIdx, 7, "Vacant Duration: " + FormatTimeSpanAsHoursMinutesAndSeconds(nextEvent.SensorEvent_Duration));
                                }
                                rowIdx++;

                                if ((nextEvent.RepeatSensorEvents != null) && (nextEvent.RepeatSensorEvents.Count > 0))
                                {
                                    foreach (SensorAndPaymentReportEngine.CommonSensorAndPaymentEvent repeatEvent in nextEvent.RepeatSensorEvents)
                                    {
                                        ws.SetValue(rowIdx, 1, spaceAsset.SpaceID);
                                        ws.SetValue(rowIdx, 2, spaceAsset.MeterID);
                                        if (areaAsset != null)
                                        {
                                            ws.SetValue(rowIdx, 3, areaAsset.AreaID);
                                            ws.SetValue(rowIdx, 4, areaAsset.AreaName);
                                        }
                                        ws.SetValue(rowIdx, 5, repeatEvent.SensorEvent_Start);
                                        if (nextEvent.SensorEvent_IsOccupied)
                                        {
                                            ws.SetValue(rowIdx, 6, "Arrival");
                                            ws.SetValue(rowIdx, 7, "Repeat sensor state");
                                        }
                                        else
                                        {
                                            ws.SetValue(rowIdx, 6, "Departure");
                                            ws.SetValue(rowIdx, 7, "Repeat sensor state");
                                        }
                                        rowIdx++;
                                    }
                                }
                            }

                            foreach (SensorAndPaymentReportEngine.PaymentEvent payEvent in nextEvent.PaymentEvents)
                            {
                                if (payEvent.PaymentEvent_IsPaid)
                                {
                                    ws.SetValue(rowIdx, 1, spaceAsset.SpaceID);
                                    ws.SetValue(rowIdx, 2, spaceAsset.MeterID);
                                    if (areaAsset != null)
                                    {
                                        ws.SetValue(rowIdx, 3, areaAsset.AreaID);
                                        ws.SetValue(rowIdx, 4, areaAsset.AreaName);
                                    }
                                    ws.SetValue(rowIdx, 5, payEvent.PaymentEvent_Start);
                                    if (payEvent.WasStoppedShortViaZeroOutTrans)
                                    {
                                        ws.SetValue(rowIdx, 6, "Payment");
                                        ws.SetValue(rowIdx, 7, "Remaining time zeroed-out at " + payEvent.PaymentEvent_End.ToString("yyyy-MM-dd hh:mm:ss tt"));
                                    }
                                    else
                                    {
                                        ws.SetValue(rowIdx, 6, "Payment");
                                        ws.SetValue(rowIdx, 7, "Paid until " + payEvent.PaymentEvent_End.ToString("yyyy-MM-dd hh:mm:ss tt"));
                                    }
                                    rowIdx++;
                                }
                            }

                            foreach (SensorAndPaymentReportEngine.PaymentVioEvent payVio in nextEvent.PaymentVios)
                            {
                                ws.SetValue(rowIdx, 1, spaceAsset.SpaceID);
                                ws.SetValue(rowIdx, 2, spaceAsset.MeterID);
                                if (areaAsset != null)
                                {
                                    ws.SetValue(rowIdx, 3, areaAsset.AreaID);
                                    ws.SetValue(rowIdx, 4, areaAsset.AreaName);
                                }

                                ws.SetValue(rowIdx, 5, payVio.StartOfPayViolation);
                                    ws.SetValue(rowIdx, 6, "Violation Detected");
                                    ws.SetValue(rowIdx, 7, "Violated Duration: " + FormatTimeSpanAsHoursMinutesAndSeconds(payVio.DurationOfTimeInViolation));
                                rowIdx++;

                                if (!string.IsNullOrEmpty(payVio.EnforcementActionTaken))
                                {
                                    ws.SetValue(rowIdx, 1, spaceAsset.SpaceID);
                                    ws.SetValue(rowIdx, 2, spaceAsset.MeterID);
                                    if (areaAsset != null)
                                    {
                                        ws.SetValue(rowIdx, 3, areaAsset.AreaID);
                                        ws.SetValue(rowIdx, 4, areaAsset.AreaName);
                                    }

                                    ws.SetValue(rowIdx, 5, payVio.EnforcementActionTakenTimeStamp);
                                    ws.SetValue(rowIdx, 6, "Enforcement Action");
                                    ws.SetValue(rowIdx, 7, payVio.EnforcementActionTaken);

                                    rowIdx++;
                                }
                            }

                        }
                    }

                    #endregion

                    // We will add autofilters to our headers so user can sort the columns easier
                    using (OfficeOpenXml.ExcelRange rng = ws.Cells[1, 1, rowIdx, detailColumnCount])
                    {
                        rng.AutoFilter = true;
                    }

                    // Apply formatting to the columns as appropriate (Starting row is 2 (first row of data), and ending row is the current rowIdx value)

                    // Column 1 & 2 are numeric integer
                    ApplyNumberStyleToColumn(ws, 1, 2, rowIdx, "########0", ExcelHorizontalAlignment.Left);
                    ApplyNumberStyleToColumn(ws, 2, 2, rowIdx, "########0", ExcelHorizontalAlignment.Left);
                    ApplyNumberStyleToColumn(ws, 3, 2, rowIdx, "########0", ExcelHorizontalAlignment.Left);
                    ApplyNumberStyleToColumn(ws, 4, 2, rowIdx, "@", ExcelHorizontalAlignment.Left);

                    ApplyNumberStyleToColumn(ws, 5, 2, rowIdx, "yyyy-mm-dd hh:mm:ss tt", ExcelHorizontalAlignment.Right);
                    ApplyNumberStyleToColumn(ws, 6, 2, rowIdx, "@", ExcelHorizontalAlignment.Left);
                    ApplyNumberStyleToColumn(ws, 7, 2, rowIdx, "@", ExcelHorizontalAlignment.Left);

                    // And now lets size the columns
                    for (int autoSizeColIdx = 1; autoSizeColIdx <= detailColumnCount; autoSizeColIdx++)
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
            System.Diagnostics.Debug.WriteLine(this._ReportName + " generation took: " + sw.Elapsed.ToString());
        }

        protected override void RenderCommonHeader(ExcelWorksheet ws, int startRowIdx, int startColIdx, ref int colIdx_HourlyCategory)
        {
            int overallSummaryHeaderRowIdx = startRowIdx;
            int renderRowIdx = startRowIdx;
            int renderColIdx = startColIdx;
            int colIdx_1stCommonColumn = startColIdx;
            //int colIdx_HourlyCategory = -1;

            renderColIdx = colIdx_1stCommonColumn;
            ws.SetValue(renderRowIdx, renderColIdx, "Occupied" + Environment.NewLine + "Duration"); // Column 1
            ApplyWrapTextStyleToCell(ws, renderRowIdx, renderColIdx);
            ws.Column(renderColIdx).Width = 15;

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, "Occupied & Paid" + Environment.NewLine + "Duration"); // Column 2
            ApplyWrapTextStyleToCell(ws, renderRowIdx, renderColIdx);
            ws.Column(renderColIdx).Width = 20;

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, "Max Possible" + Environment.NewLine + "Occupied Duration"); // Column 3
            ApplyWrapTextStyleToCell(ws, renderRowIdx, renderColIdx);
            ws.Column(renderColIdx).Width = 20;

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, "Occupied & Paid %" + Environment.NewLine + "(Compliance)"); // Column 4
            ApplyWrapTextStyleToCell(ws, renderRowIdx, renderColIdx);
            ws.Column(renderColIdx).Width = 20;

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, "Occupied & Not Paid %" + Environment.NewLine + "(Non-Compliance)"); // Column 5
            ApplyWrapTextStyleToCell(ws, renderRowIdx, renderColIdx);
            ws.Column(renderColIdx).Width = 24;

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, "Vehicle" + Environment.NewLine + "Arrivals"); // Column 6
            ApplyWrapTextStyleToCell(ws, renderRowIdx, renderColIdx);
            ws.Column(renderColIdx).Width = 12;

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, "Vehicle" + Environment.NewLine + "Departures"); // Column 7
            ApplyWrapTextStyleToCell(ws, renderRowIdx, renderColIdx);
            ws.Column(renderColIdx).Width = 12;

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, "Payment Count"); // Column 8
            ApplyVertAlignToCell(ws, renderRowIdx, renderColIdx, ExcelVerticalAlignment.Top);

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, "Zeroed Out" + Environment.NewLine + "Events"); // Column 9
            ApplyWrapTextStyleToCell(ws, renderRowIdx, renderColIdx);
            ws.Column(renderColIdx).Width = 15;

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, "Total Zeroed" + Environment.NewLine + "Out Time"); // Column 10
            ApplyWrapTextStyleToCell(ws, renderRowIdx, renderColIdx);
            ws.Column(renderColIdx).Width = 15;

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, "Violations" + Environment.NewLine + "Actioned"); // Column 11
            ApplyWrapTextStyleToCell(ws, renderRowIdx, renderColIdx);
            ws.Column(renderColIdx).Width = 12;

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, "Total" + Environment.NewLine + "Violations"); // Column 12
            ApplyWrapTextStyleToCell(ws, renderRowIdx, renderColIdx);
            ws.Column(renderColIdx).Width = 12;

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, "Total" + Environment.NewLine + "Enforced"); // Column 13
            ApplyWrapTextStyleToCell(ws, renderRowIdx, renderColIdx);
            ws.Column(renderColIdx).Width = 12;

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, "Total" + Environment.NewLine + "Cautioned"); // Column 14
            ApplyWrapTextStyleToCell(ws, renderRowIdx, renderColIdx);
            ws.Column(renderColIdx).Width = 12;

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, "Total" + Environment.NewLine + "Not Enforced"); // Column 15
            ApplyWrapTextStyleToCell(ws, renderRowIdx, renderColIdx);
            ws.Column(renderColIdx).Width = 12;

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, "Total" + Environment.NewLine + "Faulty"); // Column 16
            ApplyWrapTextStyleToCell(ws, renderRowIdx, renderColIdx);
            ws.Column(renderColIdx).Width = 12;

            // Format current portion of the header row
            using (OfficeOpenXml.ExcelRange rng = ws.Cells[renderRowIdx, colIdx_1stCommonColumn, renderRowIdx, renderColIdx])
            {
                rng.Style.Font.Bold = true;
                rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));
                rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
            }

            if (this._ReportParams.IncludeHourlyStatistics == true)
            {
                renderColIdx++;
                colIdx_HourlyCategory = renderColIdx; // Retain this column index as the known column index for hourly category
                ws.SetValue(renderRowIdx, renderColIdx, "Hourly Category");  // Column 17
                ws.Column(renderColIdx).Width = 40;

                // Format hourly category portion of the header row
                using (OfficeOpenXml.ExcelRange rng = ws.Cells[renderRowIdx, renderColIdx, renderRowIdx, renderColIdx])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.OliveDrab);
                    rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                }

                renderColIdx++; // Column 15 is start of hourly items (Midnight hour)
                DateTime tempHourlyTime = DateTime.Today;
                DateTime tempHourlyTime2 = DateTime.Today.AddHours(1);
                for (int hourlyIdx = 0; hourlyIdx < 24; hourlyIdx++)
                {
                    ws.SetValue(renderRowIdx, renderColIdx + hourlyIdx, tempHourlyTime.ToString("h ") + "-" + tempHourlyTime2.ToString(" h tt").ToLower());
                    tempHourlyTime = tempHourlyTime.AddHours(1);
                    tempHourlyTime2 = tempHourlyTime2.AddHours(1);
                    ws.Column(renderColIdx + hourlyIdx).Width = 14;
                }

                // Format hourly portion of the header row
                using (OfficeOpenXml.ExcelRange rng = ws.Cells[renderRowIdx, renderColIdx, renderRowIdx, renderColIdx + 23])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkSlateBlue);
                    rng.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                }
            }
        }

        protected override void RenderCommonData(ExcelWorksheet ws, int startRowIdx, int startColIdx, ref int colIdx_HourlyCategory, GroupStats statsObj)
        {
            int colIdx_1stCommonColumn = startColIdx;
            int renderColIdx = colIdx_1stCommonColumn;
            int renderRowIdx = startRowIdx;

            ws.SetValue(renderRowIdx, renderColIdx, FormatTimeSpanAsHoursMinutesAndSeconds(statsObj.TotalOccupancyTime));

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, FormatTimeSpanAsHoursMinutesAndSeconds(statsObj.TotalOccupancyPaidTime));

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, FormatTimeSpanAsHoursMinutesAndSeconds(statsObj.MaximumPotentialOccupancyTime));

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, statsObj.PercentageOccupiedPaid);

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, statsObj.PercentageOccupiedNotPaid);

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, statsObj.ingress);

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, statsObj.egress);

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, statsObj.PaymentCount);

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, statsObj.TotalZeroedOutEvents);

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, FormatTimeSpanAsHoursMinutesAndSeconds(statsObj.TotalZeroedOutDuration));
            
            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, statsObj.TotalPayViosActioned);

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, statsObj.PayVioCount);

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, statsObj.TotalPayViosEnforced);

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, statsObj.TotalPayViosCautioned);

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, statsObj.TotalPayViosNotEnforced);

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, statsObj.TotalPayViosFaulty);

            ApplyNumberStyleToColumn(ws, colIdx_1stCommonColumn + 3, renderRowIdx, renderRowIdx, "###0.00", ExcelHorizontalAlignment.Right);
            ApplyNumberStyleToColumn(ws, colIdx_1stCommonColumn + 4, renderRowIdx, renderRowIdx, "###0.00", ExcelHorizontalAlignment.Right);

            ApplyNumberStyleToColumn(ws, colIdx_1stCommonColumn + 5, renderRowIdx, renderRowIdx, "########0", ExcelHorizontalAlignment.Right);
            ApplyNumberStyleToColumn(ws, colIdx_1stCommonColumn + 6, renderRowIdx, renderRowIdx, "########0", ExcelHorizontalAlignment.Right);
            ApplyNumberStyleToColumn(ws, colIdx_1stCommonColumn + 7, renderRowIdx, renderRowIdx, "########0", ExcelHorizontalAlignment.Right);
            ApplyNumberStyleToColumn(ws, colIdx_1stCommonColumn + 8, renderRowIdx, renderRowIdx, "########0", ExcelHorizontalAlignment.Right);

            ApplyNumberStyleToColumn(ws, colIdx_1stCommonColumn + 10, renderRowIdx, renderRowIdx, "########0", ExcelHorizontalAlignment.Right);
            ApplyNumberStyleToColumn(ws, colIdx_1stCommonColumn + 11, renderRowIdx, renderRowIdx, "########0", ExcelHorizontalAlignment.Right);
            ApplyNumberStyleToColumn(ws, colIdx_1stCommonColumn + 12, renderRowIdx, renderRowIdx, "########0", ExcelHorizontalAlignment.Right);
            ApplyNumberStyleToColumn(ws, colIdx_1stCommonColumn + 13, renderRowIdx, renderRowIdx, "########0", ExcelHorizontalAlignment.Right);
            ApplyNumberStyleToColumn(ws, colIdx_1stCommonColumn + 14, renderRowIdx, renderRowIdx, "########0", ExcelHorizontalAlignment.Right);
            ApplyNumberStyleToColumn(ws, colIdx_1stCommonColumn + 15, renderRowIdx, renderRowIdx, "########0", ExcelHorizontalAlignment.Right);

            // And now lets autosize the columns
            for (int autoSizeColIdx = colIdx_1stCommonColumn; autoSizeColIdx <= renderColIdx; autoSizeColIdx++)
            {
                using (OfficeOpenXml.ExcelRange col = ws.Cells[1, autoSizeColIdx, renderRowIdx, autoSizeColIdx])
                {
                    col.AutoFitColumns();
                    col.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                }
            }

            // And now finally we must manually size the columns that have wrap text (autofit doesn't work nicely when we have wrap text)
            ws.Column(colIdx_1stCommonColumn + 0).Width = 15;
            ws.Column(colIdx_1stCommonColumn + 1).Width = 20;
            ws.Column(colIdx_1stCommonColumn + 2).Width = 20;
            ws.Column(colIdx_1stCommonColumn + 3).Width = 20;
            ws.Column(colIdx_1stCommonColumn + 4).Width = 24;

            ws.Column(colIdx_1stCommonColumn + 5).Width = 12;
            ws.Column(colIdx_1stCommonColumn + 6).Width = 12;

            ws.Column(colIdx_1stCommonColumn + 8).Width = 15;
            ws.Column(colIdx_1stCommonColumn + 9).Width = 15;

            ws.Column(colIdx_1stCommonColumn + 10).Width = 12;
            ws.Column(colIdx_1stCommonColumn + 11).Width = 12;
            ws.Column(colIdx_1stCommonColumn + 12).Width = 12;
            ws.Column(colIdx_1stCommonColumn + 13).Width = 12;
            ws.Column(colIdx_1stCommonColumn + 14).Width = 12;
            ws.Column(colIdx_1stCommonColumn + 15).Width = 12;

            ws.Column(colIdx_1stCommonColumn + 16).Width = 40;

            if (this._ReportParams.IncludeHourlyStatistics == true)
            {
                // Now we will construct the hourly category column, followed by hour detail columns

                ws.SetValue(renderRowIdx + 0, colIdx_HourlyCategory, "Occupied Duration");
                ws.SetValue(renderRowIdx + 1, colIdx_HourlyCategory, "Occupied & Paid Duration");
                ws.SetValue(renderRowIdx + 2, colIdx_HourlyCategory, "Max Possible Duration");
                ws.SetValue(renderRowIdx + 3, colIdx_HourlyCategory, "Occupied & Paid % (Compliance)");
                ws.SetValue(renderRowIdx + 4, colIdx_HourlyCategory, "Occupied & Not Paid % (Non-Compliance");
                ws.SetValue(renderRowIdx + 5, colIdx_HourlyCategory, "Arrivals");
                ws.SetValue(renderRowIdx + 6, colIdx_HourlyCategory, "Departures");
                ws.SetValue(renderRowIdx + 7, colIdx_HourlyCategory, "Payment Count");
                ws.SetValue(renderRowIdx + 8, colIdx_HourlyCategory, "Zeroed Out Events");
                ws.SetValue(renderRowIdx + 9, colIdx_HourlyCategory, "Total Zeroed Out Time");
                ws.SetValue(renderRowIdx + 10, colIdx_HourlyCategory, "Violations Actioned");
                ws.SetValue(renderRowIdx + 11, colIdx_HourlyCategory, "Total Violations");
                ws.SetValue(renderRowIdx + 12, colIdx_HourlyCategory, "Total Enforced");
                ws.SetValue(renderRowIdx + 13, colIdx_HourlyCategory, "Total Cautioned");
                ws.SetValue(renderRowIdx + 14, colIdx_HourlyCategory, "Total Not Enforced");
                ws.SetValue(renderRowIdx + 15, colIdx_HourlyCategory, "Total Faulty");

                using (OfficeOpenXml.ExcelRange col = ws.Cells[renderRowIdx, colIdx_HourlyCategory, renderRowIdx + (numberOfHourlyCategories - 1), colIdx_HourlyCategory])
                {
                    col.Style.Font.Bold = true;
                }

                MergeCellRange(ws, renderRowIdx + 1, 1, renderRowIdx + (numberOfHourlyCategories - 1), colIdx_HourlyCategory - 1);
            }
        }

        protected override void RenderCommonHourlyData(ExcelWorksheet ws, int startRowIdx, ref int colIdx_HourlyCategory, int hourIdx, GroupStats hourlyStats)
        {
            if (this._ReportParams.IncludeHourlyStatistics == true)
            {
                int renderRowIdx = startRowIdx;
                int renderColIdx = colIdx_HourlyCategory + 1 + hourIdx;

                // Output and format hourly data cells
                ws.SetValue(renderRowIdx + 0, renderColIdx, FormatTimeSpanAsHoursMinutesAndSeconds(hourlyStats.TotalOccupancyTime));
                ws.SetValue(renderRowIdx + 1, renderColIdx, FormatTimeSpanAsHoursMinutesAndSeconds(hourlyStats.TotalOccupancyPaidTime));
                ws.SetValue(renderRowIdx + 2, renderColIdx, FormatTimeSpanAsHoursMinutesAndSeconds(hourlyStats.MaximumPotentialOccupancyTime));

                ws.SetValue(renderRowIdx + 3, renderColIdx, hourlyStats.PercentageOccupiedPaid);
                ApplyNumberStyleToCell(ws, renderRowIdx + 3, renderColIdx, "###0.00", ExcelHorizontalAlignment.Left);

                ws.SetValue(renderRowIdx + 4, renderColIdx, hourlyStats.PercentageOccupiedNotPaid);
                ApplyNumberStyleToCell(ws, renderRowIdx + 4, renderColIdx, "###0.00", ExcelHorizontalAlignment.Left);

                ws.SetValue(renderRowIdx + 5, renderColIdx, hourlyStats.ingress);
                ApplyNumberStyleToCell(ws, renderRowIdx + 5, renderColIdx, "########0", ExcelHorizontalAlignment.Left);

                ws.SetValue(renderRowIdx + 6, renderColIdx, hourlyStats.egress);
                ApplyNumberStyleToCell(ws, renderRowIdx + 6, renderColIdx, "########0", ExcelHorizontalAlignment.Left);


                ws.SetValue(renderRowIdx + 7, renderColIdx, hourlyStats.PaymentCount);
                ApplyNumberStyleToCell(ws, renderRowIdx + 7, renderColIdx, "########0", ExcelHorizontalAlignment.Left);

                ws.SetValue(renderRowIdx + 8, renderColIdx, hourlyStats.TotalZeroedOutEvents);
                ApplyNumberStyleToCell(ws, renderRowIdx + 8, renderColIdx, "########0", ExcelHorizontalAlignment.Left);

                ws.SetValue(renderRowIdx + 9, renderColIdx, FormatTimeSpanAsHoursMinutesAndSeconds(hourlyStats.TotalZeroedOutDuration));

                ws.SetValue(renderRowIdx + 10, renderColIdx, hourlyStats.TotalPayViosActioned);
                ApplyNumberStyleToCell(ws, renderRowIdx + 10, renderColIdx, "########0", ExcelHorizontalAlignment.Left);

                ws.SetValue(renderRowIdx + 11, renderColIdx, hourlyStats.PayVioCount);
                ApplyNumberStyleToCell(ws, renderRowIdx + 11, renderColIdx, "########0", ExcelHorizontalAlignment.Left);

                ws.SetValue(renderRowIdx + 12, renderColIdx, hourlyStats.TotalPayViosEnforced);
                ApplyNumberStyleToCell(ws, renderRowIdx + 12, renderColIdx, "########0", ExcelHorizontalAlignment.Left);

                ws.SetValue(renderRowIdx + 13, renderColIdx, hourlyStats.TotalPayViosCautioned);
                ApplyNumberStyleToCell(ws, renderRowIdx + 13, renderColIdx, "########0", ExcelHorizontalAlignment.Left);

                ws.SetValue(renderRowIdx + 14, renderColIdx, hourlyStats.TotalPayViosNotEnforced);
                ApplyNumberStyleToCell(ws, renderRowIdx + 14, renderColIdx, "########0", ExcelHorizontalAlignment.Left);

                ws.SetValue(renderRowIdx + 15, renderColIdx, hourlyStats.TotalPayViosFaulty);
                ApplyNumberStyleToCell(ws, renderRowIdx + 15, renderColIdx, "########0", ExcelHorizontalAlignment.Left);
            }
        }
    }

}