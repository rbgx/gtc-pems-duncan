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
    
    public class SensorAndPaymentReportEngine
    {

        public class CommonSensorAndPaymentEvent
        {
            public SpaceAsset BayInfo { get; set; }

            public DateTime SensorEvent_Start { get; set; }
            public DateTime SensorEvent_End { get; set; }
            public TimeSpan SensorEvent_Duration { get; set; }
            public Boolean SensorEvent_IsOccupied { get; set; }
            public Boolean IsDummySensorEvent { get; set; } // Dummy sensor event is used as a placeholder for payments -- doesn't actually count as a vehicle occupancy event
            public DateTime SensorEvent_RecCreationDateTime { get; set; }
            public TimeSpan SensorEvent_Latency { get; set; }

            public List<OverstayVioEvent> Overstays { get; set; }
            public List<PaymentEvent> PaymentEvents { get; set; }
            public List<PaymentVioEvent> PaymentVios { get; set; }

            public List<CommonSensorAndPaymentEvent> RepeatSensorEvents { get; set; }

            public CommonSensorAndPaymentEvent()
            {
                InitValues();
            }

            protected void InitValues()
            {
                BayInfo = new SpaceAsset();
                SensorEvent_Start = DateTime.MinValue;
                SensorEvent_End = DateTime.MinValue;
                SensorEvent_Duration = new TimeSpan();
                SensorEvent_IsOccupied = false;
                IsDummySensorEvent = false;

                this.SensorEvent_RecCreationDateTime = DateTime.MinValue;
                this.SensorEvent_Latency = new TimeSpan();

                Overstays = new List<OverstayVioEvent>();
                PaymentEvents = new List<PaymentEvent>();
                PaymentVios = new List<PaymentVioEvent>();
                RepeatSensorEvents = new List<CommonSensorAndPaymentEvent>();
            }
        }

        public class OverstayVioEvent
        {
            public DateTime StartOfOverstayViolation { get; set; }
            public TimeSpan DurationOfTimeBeyondStayLimits { get; set; }

            public RegulatedHoursDetail OverstayBasedOnRuleDetail { get; set; }
            public RegulatedHoursGroup OverstayBasedOnRuleGroup { get; set; }

            public string EnforcementActionTaken { get; set; }
            public string EnforcementActionTakenByUser { get; set; }
            public DateTime EnforcementActionTakenTimeStamp { get; set; }

            public OverstayVioEvent()
            {
                InitValues();
            }

            protected void InitValues()
            {
                StartOfOverstayViolation = DateTime.MinValue;
                DurationOfTimeBeyondStayLimits = new TimeSpan();
                OverstayBasedOnRuleDetail = null;
                OverstayBasedOnRuleGroup = null;
                EnforcementActionTaken = string.Empty;
                EnforcementActionTakenByUser = string.Empty;
                EnforcementActionTakenTimeStamp = DateTime.MinValue;
            }
        }

        public class PaymentEvent
        {
            public DateTime PaymentEvent_Start { get; set; }
            public DateTime PaymentEvent_End { get; set; }
            public TimeSpan PaymentEvent_Duration { get; set; }
            public Boolean PaymentEvent_IsPaid { get; set; }

            public Boolean WasStoppedShortViaZeroOutTrans { get; set; }
            public DateTime OriginalPaymentEvent_End { get; set; }

            public PaymentEvent()
            {
                InitValues();
            }

            protected void InitValues()
            {
                PaymentEvent_Start = DateTime.MinValue;
                PaymentEvent_End = DateTime.MinValue;
                OriginalPaymentEvent_End = DateTime.MinValue;
                PaymentEvent_Duration = new TimeSpan();
                PaymentEvent_IsPaid = false;
                WasStoppedShortViaZeroOutTrans = false;
            }
        }

        public class PaymentVioEvent
        {
            public DateTime StartOfPayViolation { get; set; }
            public TimeSpan DurationOfTimeInViolation { get; set; }

            public string EnforcementActionTaken { get; set; }
            public string EnforcementActionTakenByUser { get; set; }
            public DateTime EnforcementActionTakenTimeStamp { get; set; }

            public PaymentVioEvent()
            {
                InitValues();
            }

            protected void InitValues()
            {
                StartOfPayViolation = DateTime.MinValue;
                DurationOfTimeInViolation = new TimeSpan();
                EnforcementActionTaken = string.Empty;
                EnforcementActionTakenByUser = string.Empty;
                EnforcementActionTakenTimeStamp = DateTime.MinValue;
            }
        }

        public class CommonReportDataModel
        {
            public List<CommonSensorAndPaymentEvent> ReportableEvents = new List<CommonSensorAndPaymentEvent>();

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

            public List<CommonSensorAndPaymentEvent> FindRecsForArea(int areaID)
            {
                IEnumerable<CommonSensorAndPaymentEvent> result =
                    from t1 in this.ReportableEvents
                    where t1.BayInfo.AreaID_PreferLibertyBeforeInternal == areaID
                    /*orderby t1.AreaID ascending*/
                    select t1;
                return result.ToList();
            }

            public List<CommonSensorAndPaymentEvent> FindRecsForMeter(int meterID)
            {
                IEnumerable<CommonSensorAndPaymentEvent> result =
                    from t1 in this.ReportableEvents
                    where t1.BayInfo.MeterID == meterID
                    /*orderby t1.MeterID ascending*/
                    select t1;
                return result.ToList();
            }

            public List<CommonSensorAndPaymentEvent> FindRecsForBayAndMeter(int bayNumber, int meterID)
            {
                IEnumerable<CommonSensorAndPaymentEvent> result =
                    from t1 in this.ReportableEvents
                    where t1.BayInfo.MeterID == meterID && t1.BayInfo.SpaceID == bayNumber
                    /*orderby t1.MeterID ascending*/
                    select t1;
                return result.ToList();
            }
        }

        public enum ReportableEnforcementActivity
        {
            AllActivity, Actioned, NotActioned, Enforced, NotEnforced, Cautioned, Fault
        }

        public enum RegulatedHoursRestrictions
        {
            AllActivity, OnlyDuringRegulatedHours, OnlyDuringUnregulatedHours
        }

        public enum TimeIsolations
        {
            None, Hour, Date, Month, DateAndHour, MonthAndHour
        }

        public class TimeIsolationOptions
        {
            public TimeIsolations IsolationType = TimeIsolations.None;
            public int Hour = 0;
            public DateTime Date = DateTime.MinValue;
            public int Month = 0;
        }

        public class CommonReportParameters
        {
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }

            public bool IncludeHourlyStatistics { get; set; }
            
            public bool IncludeAreaSummary { get; set; }
            public bool IncludeMeterSummary { get; set; }
            public bool IncludeSpaceSummary { get; set; }
            public bool IncludeDailySummary { get; set; }
            public bool IncludeMonthlySummary { get; set; }
            public bool IncludeDetailRecords { get; set; }
            
            public string ScopedAreaName { get; set; }
            public string ScopedMeter { get; set; }
            
            public ReportableEnforcementActivity ActionTakenRestrictionFilter { get; set; }
            public RegulatedHoursRestrictions RegulatedHoursRestrictionFilter { get; set; }

            public CommonReportParameters()
            {
                StartTime = DateTime.Today;
                EndTime = DateTime.Today.AddDays(1).AddSeconds(-1);
                IncludeHourlyStatistics = false;
                IncludeAreaSummary = false;
                IncludeMeterSummary = false;
                IncludeSpaceSummary = false;
                IncludeDailySummary = false;
                IncludeMonthlySummary = false;
                IncludeDetailRecords = false;
                ScopedAreaName = string.Empty;
                ScopedMeter = string.Empty;
                ActionTakenRestrictionFilter = ReportableEnforcementActivity.AllActivity;
                RegulatedHoursRestrictionFilter = RegulatedHoursRestrictions.AllActivity;
            }
        }

        public class RequiredDataElements
        {
            public bool NeedsSensorData { get; set; }
            public bool NeedsPaymentData { get; set; }
            public bool NeedsOverstayData { get; set; }
            public bool NeedsEnforcementActionData { get; set; }

            public RequiredDataElements()
            {
                this.NeedsSensorData = true;
                this.NeedsPaymentData = true;
                this.NeedsOverstayData = true;
                this.NeedsEnforcementActionData = true;
            }
        }


        #region Private/Protected Members
        protected CustomerConfig _CustomerConfig = null;

        protected List<SpaceAsset> _cachedSpaceAssets = null;
        protected List<MeterAsset> _cachedMeterAssets = null;
        protected List<AreaAsset> _cachedAreaAssets = null;
        protected List<RegulatedHoursGroup> _CachedRegulations = null;

        private List<RBACProvider.RBACUserInfo> _AllUsers = null;

        private CommonReportParameters _ReportParams = null;
        private RequiredDataElements _RequiredDataElements = null;
        #endregion

        public CommonReportDataModel ReportDataModel = new CommonReportDataModel();
        
        #region Public Methods
        public SensorAndPaymentReportEngine(CustomerConfig customerCfg, CommonReportParameters repParams)
        {
            _CustomerConfig = customerCfg;
            _ReportParams = repParams;

            // Adjust the date ranges as needed for our SQL queries.
            // The end time needs to be inclusive of the entire minute (seconds and milliseconds are not in the resolution of the passed EndTime parameter)
            DateTime AdjustedEndTime = new DateTime(_ReportParams.EndTime.Year, _ReportParams.EndTime.Month, _ReportParams.EndTime.Day, _ReportParams.EndTime.Hour, _ReportParams.EndTime.Minute, 0).AddMinutes(1);
            _ReportParams.EndTime = AdjustedEndTime;




            // Gather a local cache of assets related to the customer
            _cachedSpaceAssets = CustomerLogic.CustomerManager.GetSpaceAssetsForCustomer(_CustomerConfig);
            _cachedMeterAssets = CustomerLogic.CustomerManager.GetMeterAssetsForCustomer(_CustomerConfig);
            _cachedAreaAssets = CustomerLogic.CustomerManager.GetAreaAssetsForCustomer(_CustomerConfig);

            // Gather a local cache of regulations for the customer
         ///   _CachedRegulations = RegulatedHoursGroupRepository.Repository.GetGroups(_CustomerConfig.CustomerId).ToList();

            // Gather a list of all users, because we will need to cross-reference UserIDs to UserNames
         ///   this._AllUsers = MvcApplication.RBAC.GetAllUserNamesAndIDs();
        }

        public void GatherReportData(List<int> listOfMeterIDs, RequiredDataElements requiredDataElements, CustomerLogic result)
        {
            this._RequiredDataElements = requiredDataElements;

            // Gather all applicable vehicle sensing data (minimizes how many individual SQL queries must be executed)
            List<HistoricalSensingRecord> RawSensingDataForAllMeters = new List<HistoricalSensingRecord>();
            if (_RequiredDataElements.NeedsSensorData)
            {
                RawSensingDataForAllMeters = new SensingDatabaseSource(_CustomerConfig).GetHistoricalVehicleSensingDataForMeters_StronglyTyped(
                    this._CustomerConfig.CustomerId, listOfMeterIDs, this._ReportParams.StartTime, this._ReportParams.EndTime, true);
            }

            // Payment data is not applicable to this report, so we will just use an empty data set instead of getting from database
            List<PaymentRecord> RawPaymentDataForAllMeters = new List<PaymentRecord>();
            if (_RequiredDataElements.NeedsPaymentData)
            {
                RawPaymentDataForAllMeters = new PaymentDatabaseSource(_CustomerConfig).GetHistoricalPaymentDataForMeters_StronglyTyped(
                    this._CustomerConfig.CustomerId, listOfMeterIDs, this._ReportParams.StartTime, this._ReportParams.EndTime, true);
            }

            // Analyze data for each meter
            foreach (int nextMeterID in listOfMeterIDs)
            {
                AnalyzeDataForMeter(RawSensingDataForAllMeters, RawPaymentDataForAllMeters, nextMeterID, this._ReportParams.StartTime, this._ReportParams.EndTime,  result);
            }

            // Gather a unique list of Area and Meter assets involved in this report
            foreach (int nextMeterID in listOfMeterIDs)
            {
                MeterAsset meterAsset = null;
                if (ReportDataModel.MeterIsInListOfMetersIncludedInReport(nextMeterID) == false)
                {
                    meterAsset = GetMeterAsset(nextMeterID);
                    if (meterAsset != null)
                    {
                        ReportDataModel.MetersIncludedInReport.Add(meterAsset);

                        if (ReportDataModel.AreaIsInListOfAreasIncludedInReport(meterAsset.AreaID_PreferLibertyBeforeInternal) == false)
                        {
                            AreaAsset areaAsset = GetAreaAsset(meterAsset.AreaID_PreferLibertyBeforeInternal);
                            if (areaAsset != null)
                                ReportDataModel.AreasIncludedInReport.Add(areaAsset);
                        }
                    }
                }
            }

            // Now sort the area, meter, and space asset lists
            ReportDataModel.AreasIncludedInReport = ReportDataModel.AreasIncludedInReport.OrderBy(e => e.AreaName).ToList();
            ReportDataModel.MetersIncludedInReport = ReportDataModel.MetersIncludedInReport.OrderBy(e => e.MeterID).ToList();
            ReportDataModel.SpacesIncludedInReport = ReportDataModel.SpacesIncludedInReport.OrderBy(e => e.SpaceID).ThenBy(e => e.MeterID).ToList();
        }

        public SpaceAsset GetSpaceAsset(int meterID, int spaceID)
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

        public List<SpaceAsset> GetSpaceAssetsForMeter(int meterID)
        {
            List<SpaceAsset> result = new List<SpaceAsset>();

            // Look through our cached assets if possible, otherwise go ask the customer manager for it
            if (_cachedSpaceAssets != null)
            {
                foreach (SpaceAsset asset in _cachedSpaceAssets)
                {
                    if (asset.MeterID == meterID)
                    {
                        result.Add(asset);
                    }
                }
            }
            else
            {
                result = CustomerLogic.CustomerManager.GetSpaceAssetsForMeter(_CustomerConfig, meterID);
            }

            return result;
        }

        public MeterAsset GetMeterAsset(int meterID)
        {
            MeterAsset result = null;

            if (_cachedMeterAssets != null)
            {
                foreach (MeterAsset asset in _cachedMeterAssets)
                {
                    if (asset.MeterID == meterID)
                    {
                        result = asset;
                        break;
                    }
                }
            }
            else
            {
                result = CustomerLogic.CustomerManager.GetMeterAsset(_CustomerConfig, meterID);
            }

            return result;
        }

        public AreaAsset GetAreaAsset(int areaID)
        {
            AreaAsset result = null;

            if (_cachedAreaAssets != null)
            {
                foreach (AreaAsset asset in _cachedAreaAssets)
                {
                    if (asset.AreaID == areaID)
                    {
                        result = asset;
                        break;
                    }
                }
            }
            else
            {
                result = CustomerLogic.CustomerManager.GetAreaAsset(_CustomerConfig, areaID);
            }

            return result;
        }


        public GroupStats GetOverallStats(TimeIsolationOptions timeIsolation)
        {
            GroupStats result = new GroupStats();
            CalculateCommonStats(result, this.ReportDataModel.ReportableEvents, timeIsolation);

            // Calculate the maxium potential occupancy time based on number of assets, date range, activity restrictions, etc
            CalculateMaxPotentialOccupancyTime(result, ReportDataModel.SpacesIncludedInReport, _ReportParams.StartTime, _ReportParams.EndTime, timeIsolation);

            // And now do the final aggegations
            result.AggregateSelf();

            return result;
        }

        public GroupStats GetMeterStats(int meterID, TimeIsolationOptions timeIsolation)
        {
            GroupStats result = new GroupStats();
            List<SensorAndPaymentReportEngine.CommonSensorAndPaymentEvent> meterRecs = this.ReportDataModel.FindRecsForMeter(meterID);
            CalculateCommonStats(result, meterRecs, timeIsolation);

            // Calculate the maxium potential occupancy time based on number of spaces for this meter, date range, activity restrictions, etc
            List<SpaceAsset> spacesForMeter = GetSpaceAssetsForMeter(meterID);
            CalculateMaxPotentialOccupancyTime(result, spacesForMeter, _ReportParams.StartTime, _ReportParams.EndTime, timeIsolation);

            // And now do the final aggegations
            result.AggregateSelf();

            return result;
        }

        public GroupStats GetAreaStats(int areaID, TimeIsolationOptions timeIsolation)
        {
            GroupStats result = new GroupStats();
            List<SensorAndPaymentReportEngine.CommonSensorAndPaymentEvent> areaRecs = this.ReportDataModel.FindRecsForArea(areaID);
            CalculateCommonStats(result, areaRecs, timeIsolation);

            // Calculate the maxium potential occupancy time based on number of spaces for this area, date range, activity restrictions, etc
            List<SpaceAsset> spacesForArea = new List<SpaceAsset>();
            foreach (SpaceAsset nextBay in ReportDataModel.SpacesIncludedInReport)
            {
                if (nextBay.AreaID_PreferLibertyBeforeInternal == areaID)
                    spacesForArea.Add(nextBay);
            }
            CalculateMaxPotentialOccupancyTime(result, spacesForArea, _ReportParams.StartTime, _ReportParams.EndTime, timeIsolation);

            // And now do the final aggegations
            result.AggregateSelf();

            return result;
        }

        public GroupStats GetSpaceStats(int meterID, int bayNumber, TimeIsolationOptions timeIsolation)
        {
            GroupStats result = new GroupStats();
            List<SensorAndPaymentReportEngine.CommonSensorAndPaymentEvent> spaceRecs = this.ReportDataModel.FindRecsForBayAndMeter(bayNumber, meterID);
            CalculateCommonStats(result, spaceRecs, timeIsolation);

            // Calculate the maxium potential occupancy time based date range, activity restrictions, etc
            List<SpaceAsset> spaces = new List<SpaceAsset>();
            SpaceAsset bayInfo = GetSpaceAsset(meterID, bayNumber);
            spaces.Add(bayInfo);
            CalculateMaxPotentialOccupancyTime(result, spaces, _ReportParams.StartTime, _ReportParams.EndTime, timeIsolation);

            // And now do the final aggegations
            result.AggregateSelf();

            return result;
        }



        public bool TimestampMatchesIsolationOption(DateTime targetTimestamp, TimeIsolationOptions timeIsolation)
        {
            if ((timeIsolation == null) || (timeIsolation.IsolationType == TimeIsolations.None))
                return true;

            bool result = false;
            
            if (timeIsolation.IsolationType == TimeIsolations.Hour)
            {
                if (targetTimestamp.Hour == timeIsolation.Hour)
                    result = true;
            }
            else if (timeIsolation.IsolationType == TimeIsolations.Date)
            {
                if (targetTimestamp.Date.Ticks == timeIsolation.Date.Date.Ticks)
                    result = true;
            }
            else if (timeIsolation.IsolationType == TimeIsolations.Month)
            {
                if (targetTimestamp.Month == timeIsolation.Month)
                    result = true;
            }
            else if (timeIsolation.IsolationType == TimeIsolations.DateAndHour)
            {
                if ((targetTimestamp.Date.Ticks == timeIsolation.Date.Date.Ticks) && (targetTimestamp.Hour == timeIsolation.Hour))
                    result = true;
            }
            else if (timeIsolation.IsolationType == TimeIsolations.MonthAndHour)
            {
                if ((targetTimestamp.Month == timeIsolation.Month) && (targetTimestamp.Hour == timeIsolation.Hour))
                    result = true;
            }

            return result;
        }

        public bool IsTimestampAllowedForActivityRestrictionInEffect(DateTime timestamp, int meterID, int bayID)
        {
            // Simply return true if we are not supposed to restrict reported content to enforcement/regulated hours
            if (this._ReportParams.RegulatedHoursRestrictionFilter == RegulatedHoursRestrictions.AllActivity)
                return true;

            // Resolve the associated area for the meter
            SpaceAsset bayInfo = GetSpaceAsset(meterID, bayID);

            // Try to obtain the regulated hours applicable to this meter
            RegulatedHoursGroup regulatedHours = null;
            if (bayInfo != null)
                this.GetBestGroupForMeter(this._CustomerConfig.CustomerId, bayInfo.AreaID_PreferLibertyBeforeInternal, meterID);

            // If no regulated hour defintions came back, then we will default to assumption that the timestamp is allowed
            if ((regulatedHours == null) || (regulatedHours.Details == null) || (regulatedHours.Details.Count == 0))
                return true;

            // Determine the day of week that is involved
            int dayOfWeek = (int)timestamp.DayOfWeek;

            bool result = false;
            TimeSpan earliestAllowed;
            TimeSpan latestAllowed;
            TimeSpan justTimeFromTimestamp = new TimeSpan(timestamp.Hour, timestamp.Minute, timestamp.Second);

            // Loop through the daily rules and see if the timestamp falls within a Regulated or No Parking timeslot for the appropriate day
            foreach (RegulatedHoursDetail detail in regulatedHours.Details)
            {
                // Skip this one if its for a different day of the week
                if (detail.DayOfWeek != dayOfWeek)
                    continue;

                // We are interested in timeslots that are Regulated (or No Parking), so skip ones that are for "Unregulated"
                if (string.Compare(detail.Type, "Unregulated", true) == 0)
                    continue;

                // Determine if the time of day is within this timeslot
                earliestAllowed = new TimeSpan(detail.StartTime.Hour, detail.StartTime.Minute, 0);
                latestAllowed = new TimeSpan(detail.EndTime.Hour, detail.EndTime.Minute, 59);
                if ((justTimeFromTimestamp >= earliestAllowed) && (justTimeFromTimestamp < latestAllowed))
                {
                    // Timestamp falls within this category, so no need to look further
                    result = true;
                    break;
                }
            }

            // If we are wanting to restrict to unregulated hours, simply swap the boolean result we determined above
            if (this._ReportParams.RegulatedHoursRestrictionFilter == RegulatedHoursRestrictions.OnlyDuringUnregulatedHours)
                result = !result;

            return result;
        }

        public List<TimeSlot> AllTimeslotsInRangeAndAllowedForActivityRestrictionInEffect(TimeSlot dateRange, int meterID, int bayID)
        {
            List<TimeSlot> result = new List<TimeSlot>();

            // Simply return full range if we are not supposed to restrict reported content to enforcement/regulated hours
            if (this._ReportParams.RegulatedHoursRestrictionFilter == RegulatedHoursRestrictions.AllActivity)
            {
                result.Add(new TimeSlot(dateRange));
                return result;
            }

            // Resolve the associated area for the meter
            SpaceAsset bayInfo = GetSpaceAsset(meterID, bayID);

            // Try to obtain the regulated hours applicable to this meter
            RegulatedHoursGroup regulatedHours = null;
            if (bayInfo != null)
                this.GetBestGroupForMeter(this._CustomerConfig.CustomerId, bayInfo.AreaID_PreferLibertyBeforeInternal, meterID);

            // If no regulated hour definitions came back, then we will default to assumption that the full date range is allowed
            if ((regulatedHours == null) || (regulatedHours.Details == null) || (regulatedHours.Details.Count == 0))
            {
                result.Add(new TimeSlot(dateRange));
                return result;
            }

            // Get start and end times, but adjusted to start and end of day
            DateTime tempStartTime = new DateTime(dateRange.Start.Year, dateRange.Start.Month, dateRange.Start.Day, 0, 0, 0, 0);
            DateTime tempEndTime = new DateTime(dateRange.End.Year, dateRange.End.Month, dateRange.End.Day, 23, 59, 59, 999);

            // Loop until entire timeslot is processed 
            while (tempStartTime < tempEndTime)
            {

                // Determine the day of week that is involved
                int dayOfWeek = (int)tempStartTime.DayOfWeek;

                TimeSlot allowedSlot = null;
                TimeSpan justTimeFromTimestamp = new TimeSpan(tempStartTime.Hour, tempStartTime.Minute, tempStartTime.Second);

                // Loop through the daily rules and see if the timestamp falls within a Regulated or No Parking timeslot for the appropriate day
                foreach (RegulatedHoursDetail detail in regulatedHours.Details)
                {
                    // Skip this one if its for a different day of the week
                    if (detail.DayOfWeek != dayOfWeek)
                        continue;

                    if (this._ReportParams.RegulatedHoursRestrictionFilter == RegulatedHoursRestrictions.OnlyDuringUnregulatedHours)
                    {
                        // We are only interested if the type is Unregulated
                        if (string.Compare(detail.Type, "Unregulated", true) == 0)
                        {
                            // Determine if the time of day is within this timeslot
                            allowedSlot = new TimeSlot(
                                new DateTime(tempStartTime.Year, tempStartTime.Month, tempStartTime.Day, detail.StartTime.Hour, detail.StartTime.Minute, 0),
                                new DateTime(tempStartTime.Year, tempStartTime.Month, tempStartTime.Day, detail.EndTime.Hour, detail.EndTime.Minute, 59, 999)
                                );

                            // Does the allowed slot intersect with the date range?
                            if (allowedSlot.IntersectsWith(dateRange))
                                result.Add(allowedSlot.GetIntersection(dateRange));
                        }
                    }
                    else
                    {
                        // We are only interested if the type is NOT Unregulated
                        if (string.Compare(detail.Type, "Unregulated", true) != 0)
                        {
                            // Determine if the time of day is within this timeslot
                            allowedSlot = new TimeSlot(
                                new DateTime(tempStartTime.Year, tempStartTime.Month, tempStartTime.Day, detail.StartTime.Hour, detail.StartTime.Minute, 0),
                                new DateTime(tempStartTime.Year, tempStartTime.Month, tempStartTime.Day, detail.EndTime.Hour, detail.EndTime.Minute, 59, 999)
                                );

                            // Does the allowed slot intersect with the date range?
                            if (allowedSlot.IntersectsWith(dateRange))
                                result.Add(allowedSlot.GetIntersection(dateRange));
                        }
                    }
                }

                // Add a day to our start time and then continue to next loop iteration to evaluate the next day in our range
                tempStartTime = tempStartTime.AddDays(1);
            }

            return result;
        }

        public TimeSpan GetElapsedPortionMatchingTimeIsolation(TimeSlot source, TimeIsolationOptions timeIsolation)
        {
            if ((timeIsolation == null) || (timeIsolation.IsolationType == TimeIsolations.None))
                return source.Duration;

            TimeSpan result = new TimeSpan();

            if (timeIsolation.IsolationType == TimeIsolations.Hour)
            {
                // Get start and end times, but adjusted to nearest beginning and ending hours
                DateTime tempStartTime = new DateTime(source.Start.Year, source.Start.Month, source.Start.Day, source.Start.Hour, 0, 0, 0);
                DateTime tempEndTime = new DateTime(source.End.Year, source.End.Month, source.End.Day, 23, 59, 59, 999);
                TimeSlot hourlySlot = null;

                // Loop until entire timeslot is processed 
                while (tempStartTime < tempEndTime)
                {
                    // Ignore if current section isn't part of the desired hour
                    if (tempStartTime.Hour == timeIsolation.Hour)
                    {
                        hourlySlot = new TimeSlot(tempStartTime, new DateTime(tempStartTime.Year, tempStartTime.Month, tempStartTime.Day, tempStartTime.Hour, 59, 59, 999));
                        if (source.IntersectsWith(hourlySlot))
                        {
                            result += source.GetIntersection(hourlySlot).Duration;
                        }
                    }

                    tempStartTime = tempStartTime.AddHours(1);
                }
            }
            else if (timeIsolation.IsolationType == TimeIsolations.Date)
            {
                // Get start and end times, but adjusted to start and end of day
                DateTime tempStartTime = new DateTime(source.Start.Year, source.Start.Month, source.Start.Day, 0, 0, 0, 0);
                DateTime tempEndTime = new DateTime(source.End.Year, source.End.Month, source.End.Day, 23, 59, 59, 999);
                TimeSlot dailySlot = null;

                // Loop until entire timeslot is processed 
                while (tempStartTime < tempEndTime)
                {
                    // Ignore if current section isn't part of the desired date
                    if (tempStartTime.Date.Ticks == timeIsolation.Date.Ticks)
                    {
                        dailySlot = new TimeSlot(tempStartTime, new DateTime(tempStartTime.Year, tempStartTime.Month, tempStartTime.Day, 23, 59, 59, 999));
                        if (source.IntersectsWith(dailySlot))
                        {
                            result += source.GetIntersection(dailySlot).Duration;
                        }
                    }

                    tempStartTime = tempStartTime.AddDays(1);
                }
            }
            else if (timeIsolation.IsolationType == TimeIsolations.Month)
            {
                // Get start and end times, but adjusted to start and end of day
                DateTime tempStartTime = new DateTime(source.Start.Year, source.Start.Month, source.Start.Day, 0, 0, 0, 0);
                DateTime tempEndTime = new DateTime(source.End.Year, source.End.Month, source.End.Day, 23, 59, 59, 999);
                TimeSlot dailySlot = null;

                // Loop until entire timeslot is processed 
                while (tempStartTime < tempEndTime)
                {
                    // Ignore if current section isn't part of the desired month
                    if (tempStartTime.Month == timeIsolation.Month)
                    {
                        dailySlot = new TimeSlot(tempStartTime, new DateTime(tempStartTime.Year, tempStartTime.Month, tempStartTime.Day, 23, 59, 59, 999));
                        if (source.IntersectsWith(dailySlot))
                        {
                            result += source.GetIntersection(dailySlot).Duration;
                        }
                    }

                    tempStartTime = tempStartTime.AddDays(1);
                }
            }
            else if (timeIsolation.IsolationType == TimeIsolations.DateAndHour)
            {
                // Get start and end times, but adjusted to start and end of day
                DateTime tempStartTime = new DateTime(source.Start.Year, source.Start.Month, source.Start.Day, 0, 0, 0, 0);
                DateTime tempEndTime = new DateTime(source.End.Year, source.End.Month, source.End.Day, 23, 59, 59, 999);
                TimeSlot hourlySlot = null;

                // Loop until entire timeslot is processed 
                while (tempStartTime < tempEndTime)
                {
                    // Ignore if current section isn't part of the desired date and hour
                    if (tempStartTime.Date.Ticks == timeIsolation.Date.Ticks)
                    {
                        if (tempStartTime.Hour == timeIsolation.Hour)
                        {
                            hourlySlot = new TimeSlot(tempStartTime, new DateTime(tempStartTime.Year, tempStartTime.Month, tempStartTime.Day, tempStartTime.Hour, 59, 59, 999));
                            if (source.IntersectsWith(hourlySlot))
                            {
                                result += source.GetIntersection(hourlySlot).Duration;
                            }
                        }
                    }

                    tempStartTime = tempStartTime.AddHours(1);
                }
            }
            else if (timeIsolation.IsolationType == TimeIsolations.MonthAndHour)
            {
                // Get start and end times, but adjusted to start and end of day
                DateTime tempStartTime = new DateTime(source.Start.Year, source.Start.Month, source.Start.Day, 0, 0, 0, 0);
                DateTime tempEndTime = new DateTime(source.End.Year, source.End.Month, source.End.Day, 23, 59, 59, 999);
                TimeSlot hourlySlot = null;

                // Loop until entire timeslot is processed 
                while (tempStartTime < tempEndTime)
                {
                    // Ignore if current section isn't part of the desired month and hour
                    if (tempStartTime.Month == timeIsolation.Month)
                    {
                        if (tempStartTime.Hour == timeIsolation.Hour)
                        {
                            hourlySlot = new TimeSlot(tempStartTime, new DateTime(tempStartTime.Year, tempStartTime.Month, tempStartTime.Day, tempStartTime.Hour, 59, 59, 999));
                            if (source.IntersectsWith(hourlySlot))
                            {
                                result += source.GetIntersection(hourlySlot).Duration;
                            }
                        }
                    }

                    tempStartTime = tempStartTime.AddHours(1);
                }
            }

            // Lets round up or down to nearest whole second
            if (result.Milliseconds >= 500)
                result += new TimeSpan(0, 0, 0, 0, 1000 - result.Milliseconds);
            else
                result -= new TimeSpan(0, 0, 0, 0, result.Milliseconds);

            return result;
        }

        public bool TimestampIsInsideTimeSlots(DateTime targetTime, List<TimeSlot> timeSlots)
        {
            bool result = false;
            foreach (TimeSlot nextSlot in timeSlots)
            {
                if (nextSlot.HasInside(targetTime))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        public void CalculateCommonStats(GroupStats stats, List<SensorAndPaymentReportEngine.CommonSensorAndPaymentEvent> eventsToCalculate, TimeIsolationOptions timeIsolation)
        {
            foreach (SensorAndPaymentReportEngine.CommonSensorAndPaymentEvent occupancyEvent in eventsToCalculate)
            {
                // Get a list of "allowed" time slots that correspond to the reports date/time range, and also with consideration for chosen activity restrictions
                // (i.e. Do we need to exclude time segments that fall into "Unregulated" hours?)
                List<TimeSlot> allowedSlots = AllTimeslotsInRangeAndAllowedForActivityRestrictionInEffect(
                    new TimeSlot(_ReportParams.StartTime, _ReportParams.EndTime), occupancyEvent.BayInfo.MeterID, occupancyEvent.BayInfo.SpaceID);
                
                TimeSlot OccupiedSegment = new TimeSlot(occupancyEvent.SensorEvent_Start, occupancyEvent.SensorEvent_End);
                TimeSlot OverstaySegment = null;
                TimeSlot PaymentSegment = null;
                TimeSlot OccupiedAndPaidIntersection = null;

                // Only count the vehicle In/Out if it started during desired timeframe
                // (But ignore if its a "dummy" sensor event -- which is a placeholder for payments not associated with space occupancy info)
                if (occupancyEvent.IsDummySensorEvent == false)
                {
                    if (TimestampMatchesIsolationOption(occupancyEvent.SensorEvent_Start, timeIsolation) == true)
                    {
                        // Besides being okay for the time isolation parameters, it also needs to be okay for chosen regulation activity restrictions
                        if (TimestampIsInsideTimeSlots(occupancyEvent.SensorEvent_Start, allowedSlots))
                        {
                            if (occupancyEvent.SensorEvent_IsOccupied)
                                stats.ingress++;
                            else
                                stats.egress++;
                        }

                        stats.TotalSensorEvent_Latency += occupancyEvent.SensorEvent_Latency;
                    }

                    // See if there are any child "repeat" sensor events that need to be counted for this time period
                    // (Note: Repeat events means sequential sensor events were for the same occupancy status.  These will be counted, 
                    // but don't alter the overall occupied/vacant duration, nor will we collect seperate stats or overstay info for them,
                    // since they are just considered a segment of the overall stay)
                    if (occupancyEvent.RepeatSensorEvents != null)
                    {
                        foreach (CommonSensorAndPaymentEvent repeatEvent in occupancyEvent.RepeatSensorEvents)
                        {
                            // Only count the vehicle In/Out if it started during desired timeframe
                            if (TimestampMatchesIsolationOption(repeatEvent.SensorEvent_Start, timeIsolation) == true)
                            {
                                // Besides being okay for the time isolation parameters, it also needs to be okay for chosen regulation activity restrictions
                                if (TimestampIsInsideTimeSlots(repeatEvent.SensorEvent_Start, allowedSlots))
                                {
                                    if (repeatEvent.SensorEvent_IsOccupied)
                                        stats.ingress++;
                                    else
                                        stats.egress++;
                                }

                                stats.TotalSensorEvent_Latency += occupancyEvent.SensorEvent_Latency;
                            }
                        }
                    }
                }

                // Occupancy duration must be limited to within desired timeframe
                // (But ignore if its a "dummy" sensor event -- which is a placeholder for payments not associated with space occupancy info)
                if (occupancyEvent.IsDummySensorEvent == false)
                {
                    if (occupancyEvent.SensorEvent_IsOccupied)
                    {
                        // Calculated duration needs to account for time isolation and regulation activity restrictions
                        foreach (TimeSlot nextAllowed in allowedSlots)
                        {
                            if (nextAllowed.IntersectsWith(OccupiedSegment))
                                stats.TotalOccupancyTime += GetElapsedPortionMatchingTimeIsolation(nextAllowed.GetIntersection(OccupiedSegment), timeIsolation);
                        }
                    }

                    foreach (SensorAndPaymentReportEngine.OverstayVioEvent overstay in occupancyEvent.Overstays)
                    {
                        if (TimestampMatchesIsolationOption(overstay.StartOfOverstayViolation, timeIsolation) == true)
                        {
                            // Besides being okay for the time isolation parameters, it also needs to be okay for chosen regulation activity restrictions
                            if (TimestampIsInsideTimeSlots(overstay.StartOfOverstayViolation, allowedSlots))
                            {
                                stats.OverstayCount++;

                                if (string.Compare(overstay.EnforcementActionTaken, "Cautioned", true) == 0)
                                {
                                    stats.TotalOverstaysCautioned++;
                                    stats.TotalOverstaysActioned++;
                                }
                                else if (string.Compare(overstay.EnforcementActionTaken, "Enforced", true) == 0)
                                {
                                    stats.TotalOverstaysEnforced++;
                                    stats.TotalOverstaysActioned++;
                                }
                                else if (string.Compare(overstay.EnforcementActionTaken, "NotEnforced", true) == 0)
                                {
                                    stats.TotalOverstaysNotEnforced++;
                                    stats.TotalOverstaysActioned++;
                                }
                                else if (string.Compare(overstay.EnforcementActionTaken, "Fault", true) == 0)
                                {
                                    stats.TotalOverstaysFaulty++;
                                    stats.TotalOverstaysActioned++;
                                }

                                // Now see which overall actioned "Capture Rate" category it should be counted in
                                if ((!string.IsNullOrEmpty(overstay.EnforcementActionTaken)) && (overstay.EnforcementActionTakenTimeStamp != DateTime.MinValue))
                                {
                                    TimeSpan elapsedUntilActioned = (overstay.EnforcementActionTakenTimeStamp - overstay.StartOfOverstayViolation);
                                    if (elapsedUntilActioned.TotalMinutes < 15)
                                        stats.TotalViosActioned0To15Mins++;
                                    else if (elapsedUntilActioned.TotalMinutes < 40)
                                        stats.TotalViosActioned15To40Mins++;
                                    else 
                                        stats.TotalViosActionedOver40Mins++;
                                }
                            }
                        }

                        OverstaySegment = new TimeSlot(overstay.StartOfOverstayViolation, overstay.DurationOfTimeBeyondStayLimits);
                        // Calculated duration needs to account for time isolation and regulation activity restrictions
                        foreach (TimeSlot nextAllowed in allowedSlots)
                        {
                            if (nextAllowed.IntersectsWith(OverstaySegment))
                                stats.TotalOverstayDuration += GetElapsedPortionMatchingTimeIsolation(nextAllowed.GetIntersection(OverstaySegment), timeIsolation);
                        }
                    }
                }

                // We need to get the duration of occupied and paid, but watch out for overlapping payments which should be overall elapsed time, NOT total of each elapsed time!
                if ((occupancyEvent.IsDummySensorEvent == false) && (occupancyEvent.SensorEvent_IsOccupied))
                {
                    List<TimeSlot> paidSegments = new List<TimeSlot>();
                    foreach (SensorAndPaymentReportEngine.PaymentEvent payEvent in occupancyEvent.PaymentEvents)
                    {
                        if (payEvent.PaymentEvent_IsPaid)
                        {
                            paidSegments.Add(new TimeSlot(payEvent.PaymentEvent_Start, payEvent.PaymentEvent_End));
                        }
                    }
                    List<TimeSlot> OccupiedAndUnpaidGaps = TimeSlotGapCalculator.GetGaps(OccupiedSegment, paidSegments);
                    List<TimeSlot> OccupiedAndPaidSegmentsWithoutOverlaps = TimeSlotGapCalculator.GetGaps(OccupiedSegment, OccupiedAndUnpaidGaps);
                    foreach (TimeSlot nextOccupiedAndPaid in OccupiedAndPaidSegmentsWithoutOverlaps)
                    {
                        // Calculated duration needs to account for time isolation and regulation activity restrictions
                        foreach (TimeSlot nextAllowed in allowedSlots)
                        {
                            if (nextAllowed.IntersectsWith(nextOccupiedAndPaid))
                            {
                                stats.TotalOccupancyPaidTime += GetElapsedPortionMatchingTimeIsolation(nextAllowed.GetIntersection(nextOccupiedAndPaid), timeIsolation);
                            }
                        }
                    }
                }


                foreach (SensorAndPaymentReportEngine.PaymentEvent payEvent in occupancyEvent.PaymentEvents)
                {
                    if (payEvent.PaymentEvent_IsPaid)
                    {
                        // The same payment may be included in more than one "reportable event", so only count it if it occurred during this event
                        if (OccupiedSegment.HasInside(payEvent.PaymentEvent_Start))
                        {
                            if (TimestampMatchesIsolationOption(payEvent.PaymentEvent_Start, timeIsolation) == true)
                            {
                                // Besides being okay for the time isolation parameters, it also needs to be okay for chosen regulation activity restrictions
                                if (TimestampIsInsideTimeSlots(payEvent.PaymentEvent_Start, allowedSlots))
                                {
                                    stats.PaymentCount++;

                                    // See if there is info about zero-out event that should be reported
                                    if (payEvent.WasStoppedShortViaZeroOutTrans == true)
                                        stats.TotalZeroedOutEvents++;
                                }
                            }
                        }

                        // The real payment segment might extend past our "reportable event" which is based on a stay. Therefore, we need to truncate
                        // this payment segment to fit inside the occupancy event.  Any remainder of the payment should be accounted for in the next
                        // reportable event which also has this overlapping payment associated with it
                        PaymentSegment = new TimeSlot(payEvent.PaymentEvent_Start, payEvent.PaymentEvent_End).GetIntersection(OccupiedSegment);

                        // Calculated duration needs to account for time isolation and regulation activity restrictions
                        foreach (TimeSlot nextAllowed in allowedSlots)
                        {
                            if (nextAllowed.IntersectsWith(PaymentSegment))
                            {
                                stats.TotalPaidTime += GetElapsedPortionMatchingTimeIsolation(nextAllowed.GetIntersection(PaymentSegment), timeIsolation);

                                // See if there is info about zero-out event that should be reported
                                if (payEvent.WasStoppedShortViaZeroOutTrans == true)
                                {
                                    stats.TotalZeroedOutDuration = (payEvent.OriginalPaymentEvent_End - payEvent.PaymentEvent_End);
                                }
                            }
                        }

                    }
                }

                foreach (PaymentVioEvent payVio in occupancyEvent.PaymentVios)
                {
                    if (TimestampMatchesIsolationOption(payVio.StartOfPayViolation, timeIsolation) == true)
                    {
                        // Besides being okay for the time isolation parameters, it also needs to be okay for chosen regulation activity restrictions
                        if (TimestampIsInsideTimeSlots(payVio.StartOfPayViolation, allowedSlots))
                        {
                            stats.PayVioCount++;

                            if (string.Compare(payVio.EnforcementActionTaken, "Cautioned", true) == 0)
                            {
                                stats.TotalPayViosCautioned++;
                                stats.TotalPayViosActioned++;
                            }
                            else if (string.Compare(payVio.EnforcementActionTaken, "Enforced", true) == 0)
                            {
                                stats.TotalPayViosEnforced++;
                                stats.TotalPayViosActioned++;
                            }
                            else if (string.Compare(payVio.EnforcementActionTaken, "NotEnforced", true) == 0)
                            {
                                stats.TotalPayViosNotEnforced++;
                                stats.TotalPayViosActioned++;
                            }
                            else if (string.Compare(payVio.EnforcementActionTaken, "Fault", true) == 0)
                            {
                                stats.TotalPayViosFaulty++;
                                stats.TotalPayViosActioned++;
                            }

                            // Now see which overall actioned "Capture Rate" category it should be counted in
                            if ((!string.IsNullOrEmpty(payVio.EnforcementActionTaken)) && (payVio.EnforcementActionTakenTimeStamp != DateTime.MinValue))
                            {
                                TimeSpan elapsedUntilActioned = (payVio.EnforcementActionTakenTimeStamp - payVio.StartOfPayViolation);
                                if (elapsedUntilActioned.TotalMinutes < 15)
                                    stats.TotalViosActioned0To15Mins++;
                                else if (elapsedUntilActioned.TotalMinutes < 40)
                                    stats.TotalViosActioned15To40Mins++;
                                else
                                    stats.TotalViosActionedOver40Mins++;
                            }
                        }
                    }

                    TimeSlot PayVioSegment = new TimeSlot(payVio.StartOfPayViolation, payVio.DurationOfTimeInViolation);
                    // Calculated duration needs to account for time isolation and regulation activity restrictions
                    foreach (TimeSlot nextAllowed in allowedSlots)
                    {
                        if (nextAllowed.IntersectsWith(PayVioSegment))
                            stats.TotalPayVioDuration += GetElapsedPortionMatchingTimeIsolation(nextAllowed.GetIntersection(PayVioSegment), timeIsolation);
                    }
                }

            }
        }

        public void CalculateMaxPotentialOccupancyTime(GroupStats stats, List<SpaceAsset> bays, DateTime startTime, DateTime endTime, TimeIsolationOptions timeIsolation)
        {
            // Calculate the maximum potential occupancy time based on number of spaces, date range, activity restrictions, time isolation options, etc
            foreach (SpaceAsset nextBay in bays)
            {
                List<TimeSlot> allowedSlots = AllTimeslotsInRangeAndAllowedForActivityRestrictionInEffect(new TimeSlot(startTime, endTime), nextBay.MeterID, nextBay.SpaceID);
                foreach (TimeSlot nextAllowed in allowedSlots)
                {
                    stats.MaximumPotentialOccupancyTime += GetElapsedPortionMatchingTimeIsolation(nextAllowed, timeIsolation);
                }
            }
        }

        #endregion


        #region Private/Protected Methods

        public RegulatedHoursGroup GetBestGroupForMeter(int? cid, int? aid, int? mid)
        {
            try
            {
                // Start by trying to find for all 3 scope elements together
                RegulatedHoursGroupPredicate predicate = new RegulatedHoursGroupPredicate(cid, aid, mid);
                List<RegulatedHoursGroup> groupsWithSameScope = _CachedRegulations.FindAll(predicate.CompareScope);

                // If no group found at meter-level, then we will try at the area-level
                if ((groupsWithSameScope == null) || (groupsWithSameScope.Count == 0))
                {
                    predicate = new RegulatedHoursGroupPredicate(cid, aid, null);
                    groupsWithSameScope = _CachedRegulations.FindAll(predicate.CompareScope);
                }

                // If no group found at area-level, then we will try at the customer-level
                if ((groupsWithSameScope == null) || (groupsWithSameScope.Count == 0))
                {
                    predicate = new RegulatedHoursGroupPredicate(cid, null, null);
                    groupsWithSameScope = _CachedRegulations.FindAll(predicate.CompareScope);
                }

                if ((groupsWithSameScope == null) || (groupsWithSameScope.Count == 0))
                    return null;
                else
                    return groupsWithSameScope[0];
            }
            finally
            {
            }
        }

        protected void AnalyzeDataForMeter(List<HistoricalSensingRecord> allSensingData, List<PaymentRecord> allPaymentData, int meterId, DateTime startTime, DateTime endTime_NotInclusive, CustomerLogic result)
        {
            // Get list of all Spaces associated with the MeterID
            List<int> SortedBayIds = new List<int>();
            List<SpaceAsset> spaceAssets = GetSpaceAssetsForMeter(meterId);
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
                    nextBayID, meterId, nextBayID, startTime, endTime_NotInclusive,  result);
            }
        }

        protected void AnalyzeDataForSpace(List<HistoricalSensingRecord> filteredSensingData, List<PaymentRecord> filteredPaymentData, int legacyBitNumber,
            int meterId, int bayNumber, DateTime startTime, DateTime endTime_NotInclusive, CustomerLogic result)
        {
            CommonSensorAndPaymentEvent currentReportableEvent = null;
            SpaceAsset spaceAsset = GetSpaceAsset(meterId, bayNumber);

            // Need to know current time in customer's timezone
            DateTime NowAtDestination = Convert.ToDateTime(this._CustomerConfig.DestinationTimeZoneDisplayName);

            if (this.ReportDataModel.SpaceIsInListOfSpacesIncludedInReport(bayNumber, meterId) == false)
            {
                if (spaceAsset != null)
                    this.ReportDataModel.SpacesIncludedInReport.Add(spaceAsset);
            }

            // Start analyzing vehicle sensing data here 
            foreach (HistoricalSensingRecord rawVSDataRec in filteredSensingData)
            {
                // The data should be filtered to the correct meter and bay, but we might as well check it
                if ((rawVSDataRec.MeterMappingId != meterId) || (rawVSDataRec.BayId != bayNumber))
                    continue;

                // Are we ready to begin a new object?
                if (currentReportableEvent == null)
                {
                    currentReportableEvent = new CommonSensorAndPaymentEvent();
                    currentReportableEvent.BayInfo = spaceAsset;
                    currentReportableEvent.SensorEvent_Start = rawVSDataRec.DateTime;
                    currentReportableEvent.SensorEvent_IsOccupied = rawVSDataRec.IsOccupied;
                    currentReportableEvent.SensorEvent_RecCreationDateTime = rawVSDataRec.RecCreationDateTime;
                    currentReportableEvent.SensorEvent_Latency = (currentReportableEvent.SensorEvent_RecCreationDateTime - currentReportableEvent.SensorEvent_Start);

                    // Nothing more to do until we get to the next record (or end of records)
                    continue;
                }

                // Need to see if there is a change in occupancy status. If so, we need to finalize our previous object and then start a new one
                if (currentReportableEvent.SensorEvent_IsOccupied != rawVSDataRec.IsOccupied)
                {
                    currentReportableEvent.SensorEvent_End = rawVSDataRec.DateTime;
                    currentReportableEvent.SensorEvent_Duration = (currentReportableEvent.SensorEvent_End - currentReportableEvent.SensorEvent_Start);

                    // If there are any child "repeat" events, then set their end time and durations also
                    if (currentReportableEvent.RepeatSensorEvents != null)
                    {
                        foreach (CommonSensorAndPaymentEvent repeatEvent in currentReportableEvent.RepeatSensorEvents)
                        {
                            repeatEvent.SensorEvent_End = rawVSDataRec.DateTime;
                            repeatEvent.SensorEvent_Duration = (repeatEvent.SensorEvent_End - repeatEvent.SensorEvent_Start);
                        }
                    }

                    // Determine duration, and see if it qualifies as an overstay violation
                    AnalyzeOccupancyEventWithOverstayViolationForOverstayViolation(currentReportableEvent,  result);
                    this.ReportDataModel.ReportableEvents.Add(currentReportableEvent);

                    // Finished getting data for previous event, so clear current object reference so a new one will be started
                    currentReportableEvent = null;

                    // We must be careful not to skip events! This means the next event object needs to be started based on the current record, not the next!
                    currentReportableEvent = new CommonSensorAndPaymentEvent();
                    currentReportableEvent.BayInfo = spaceAsset;
                    currentReportableEvent.SensorEvent_Start = rawVSDataRec.DateTime;
                    currentReportableEvent.SensorEvent_IsOccupied = rawVSDataRec.IsOccupied;
                    currentReportableEvent.SensorEvent_RecCreationDateTime = rawVSDataRec.RecCreationDateTime;
                    currentReportableEvent.SensorEvent_Latency = (currentReportableEvent.SensorEvent_RecCreationDateTime - currentReportableEvent.SensorEvent_Start);
                }
                else
                {
                    // An occupancy event is detected, but not a status change.  We will consider this as a "Repeat" event, which is a "child" of current one
                    CommonSensorAndPaymentEvent repeatEvent = new CommonSensorAndPaymentEvent();
                    repeatEvent.BayInfo = spaceAsset;
                    repeatEvent.SensorEvent_Start = rawVSDataRec.DateTime;
                    repeatEvent.SensorEvent_IsOccupied = rawVSDataRec.IsOccupied;
                    currentReportableEvent.SensorEvent_RecCreationDateTime = rawVSDataRec.RecCreationDateTime;
                    currentReportableEvent.SensorEvent_Latency = (currentReportableEvent.SensorEvent_RecCreationDateTime - currentReportableEvent.SensorEvent_Start);
                    // Add the event to the repeat list
                    currentReportableEvent.RepeatSensorEvents.Add(repeatEvent);
                }
            }

            // No more records, so finalize our current object (if there is one)
            // We will need to assume an end time, but let's make sure it does't exceed current time (in customer timezone), or the report end time
            if (currentReportableEvent != null)
            {
                DateTime LesserOfCurrentTimeOrReportEndTime = NowAtDestination;
                if (NowAtDestination > endTime_NotInclusive)
                    LesserOfCurrentTimeOrReportEndTime = endTime_NotInclusive;

                currentReportableEvent.SensorEvent_End = LesserOfCurrentTimeOrReportEndTime;
                currentReportableEvent.SensorEvent_Duration = (currentReportableEvent.SensorEvent_End - currentReportableEvent.SensorEvent_Start);

                // If there are any child "repeat" events, then set their end time and durations also
                if (currentReportableEvent.RepeatSensorEvents != null)
                {
                    foreach (CommonSensorAndPaymentEvent repeatEvent in currentReportableEvent.RepeatSensorEvents)
                    {
                        repeatEvent.SensorEvent_End = LesserOfCurrentTimeOrReportEndTime;
                        repeatEvent.SensorEvent_Duration = (repeatEvent.SensorEvent_End - repeatEvent.SensorEvent_Start);
                    }
                }

                // Need to determine duration, then see if we qualify as an overstay, etc...
                AnalyzeOccupancyEventWithOverstayViolationForOverstayViolation(currentReportableEvent,  result);
                this.ReportDataModel.ReportableEvents.Add(currentReportableEvent);
            }



            //////   Payments  //////

            // Start analyzing payment data here 
            foreach (PaymentRecord rawPayDataRec in filteredPaymentData)
            {
                // The data should be filtered to the correct meter and bay, but we might as well check it
                if ((rawPayDataRec.MeterId != meterId) || (rawPayDataRec.BayNumber != bayNumber))
                    continue;

                // First, we will find all reportable events that have sensor events intersecting with our payment event
                TimeSlot paymentSlot = new TimeSlot(rawPayDataRec.TransactionDateTime, rawPayDataRec.TimePaid);
                List<CommonSensorAndPaymentEvent> applicableReportableEvents = GetOrCreateAllReportableEventsCoveringPaymentEvent(meterId, bayNumber, paymentSlot);

                // If the payment record is the special "ZeroOut" type (vehicle arrival zeros out remaining time), we need to do special processing
                // to see if any previous payment events need to be "stopped short"
                if (rawPayDataRec.Method == PaymentMethod.ZeroOut)
                {
                    List<CommonSensorAndPaymentEvent> recordsForThisBay = this.ReportDataModel.FindRecsForBayAndMeter(bayNumber, meterId);
                    foreach (CommonSensorAndPaymentEvent nextReportableEvent in recordsForThisBay)
                    {
                        foreach (PaymentEvent nextPayEvent in nextReportableEvent.PaymentEvents)
                        {
                            TimeSlot PaidSegment = new TimeSlot(nextPayEvent.PaymentEvent_Start, nextPayEvent.PaymentEvent_End);
                            if (PaidSegment.HasInside(rawPayDataRec.TransactionDateTime))
                            {
                                nextPayEvent.WasStoppedShortViaZeroOutTrans = true;
                                nextPayEvent.OriginalPaymentEvent_End = new DateTime(nextPayEvent.PaymentEvent_End.Ticks);
                                nextPayEvent.PaymentEvent_End = rawPayDataRec.TransactionDateTime;
                                nextPayEvent.PaymentEvent_Duration = (nextPayEvent.PaymentEvent_End - nextPayEvent.PaymentEvent_Start);
                            }
                        }
                    }
                }
                else
                {
                    // Create payment event from info in current payment record
                    PaymentEvent payEvent = new PaymentEvent();
                    payEvent.PaymentEvent_Start = paymentSlot.Start;
                    payEvent.PaymentEvent_End = paymentSlot.End;
                    payEvent.PaymentEvent_Duration = paymentSlot.Duration;
                    payEvent.PaymentEvent_IsPaid = true;

                    // Add payment event to each applicable reportable event
                    foreach (CommonSensorAndPaymentEvent nextReportableEvent in applicableReportableEvents)
                    {
                        nextReportableEvent.PaymentEvents.Add(payEvent);
                    }
                }
            }

            // Analyze all reportable events for payment violations, based on being occupied and expired
            // (But we can skip this logic if the report doesn't even care about payment data)
            if (this._RequiredDataElements.NeedsPaymentData == true)
            {
                List<CommonSensorAndPaymentEvent> recordsToAnalyze = this.ReportDataModel.FindRecsForBayAndMeter(bayNumber, meterId);
                foreach (CommonSensorAndPaymentEvent nextReportableEvent in recordsToAnalyze)
                {
                    AnalyzeForPaymentViolation(nextReportableEvent,  result);
                }
            }
        }

        protected List<CommonSensorAndPaymentEvent> GetOrCreateAllReportableEventsCoveringPaymentEvent(int meterID, int bayID, TimeSlot paymentSlot)
        {
            List<CommonSensorAndPaymentEvent> result = new List<CommonSensorAndPaymentEvent>();
            TimeSlot reportableSlot = null;

            // Find all reportable events that have sensor events intersecting/overlapping with the payment event (there may be more than 1!)
            foreach (CommonSensorAndPaymentEvent nextReportableEvent in this.ReportDataModel.ReportableEvents)
            {
                if ((nextReportableEvent.BayInfo.MeterID == meterID) && (nextReportableEvent.BayInfo.SpaceID == bayID))
                {
                    reportableSlot = new TimeSlot(nextReportableEvent.SensorEvent_Start, nextReportableEvent.SensorEvent_End);
                    if (reportableSlot.IntersectsWith(paymentSlot))
                    {
                        result.Add(nextReportableEvent);
                    }
                }
            }

            // If no existing reportable events cover this payment, we will need to create a "dummy" one
            if (result.Count == 0)
            {
                CommonSensorAndPaymentEvent dummyEvent = new CommonSensorAndPaymentEvent();
                dummyEvent.BayInfo = GetSpaceAsset(meterID, bayID);
                dummyEvent.SensorEvent_Start = paymentSlot.Start;
                dummyEvent.SensorEvent_RecCreationDateTime = paymentSlot.Start;
                dummyEvent.SensorEvent_Latency = (paymentSlot.Start - paymentSlot.Start);
                dummyEvent.SensorEvent_End = paymentSlot.End;
                dummyEvent.SensorEvent_Duration = (dummyEvent.SensorEvent_End - dummyEvent.SensorEvent_Start);
                dummyEvent.SensorEvent_IsOccupied = false; // assume unoccupied, since we don't know the real occupancy state
                dummyEvent.IsDummySensorEvent = true; // Mark it as a "dummy" sensor event

                this.ReportDataModel.ReportableEvents.Add(dummyEvent);
                result.Add(dummyEvent);
            }

            return result;
        }

        protected List<HistoricalSensingRecord> GetSensingDataSubsetForMeterIDAndBayID(List<HistoricalSensingRecord> allSensingData, int meterID, int bayID)
        {
            List<HistoricalSensingRecord> result = new List<HistoricalSensingRecord>();
            foreach (HistoricalSensingRecord nextRec in allSensingData)
            {
                if ((nextRec.MeterMappingId == meterID) && (nextRec.BayId == bayID))
                    result.Add(nextRec);
            }

            // Make sure this set of data is sorted by date, in ascending order!
            result.Sort(new Duncan.PEMS.SpaceStatus.DataSuppliers.HistoricalSensingRecordsSorter(true));

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

            // Make sure this set of data is sorted by date, in ascending order!
            result.Sort(new Duncan.PEMS.SpaceStatus.DataSuppliers.HistoricalPaymentRecordsSorter(true));
            
            return result;
        }

        protected void AnalyzeOccupancyEventWithOverstayViolationForOverstayViolation(CommonSensorAndPaymentEvent currentReportableEvent, CustomerLogic result)
        {
            // There is nothing to do if this event is not for occupied status
            if (currentReportableEvent.SensorEvent_IsOccupied == false)
                return;

            // Don't do anything if the report doesn't need overstay information
            if (this._RequiredDataElements.NeedsOverstayData == false)
                return;

            // Find the space asset associated with this data model.  If the space is "inactive" (based on the "IsActive" column of "HousingMaster" table in database),
            // then we will not consider the space to be in a violating state, because the sensor is effectively marked as bad/untrusted

            // Nothing more to do if the space isn't active
            if (currentReportableEvent.BayInfo.IsActive == false)
                return;
            
            // Try to obtain the regulated hours applicable to this meter
            RegulatedHoursGroup regulatedHours = this.GetBestGroupForMeter(this._CustomerConfig.CustomerId, currentReportableEvent.BayInfo.AreaID_PreferLibertyBeforeInternal, currentReportableEvent.BayInfo.MeterID);

            // If no regulated hour defintions came back, then we are unable to calculate any overstay violation, so just exit
            if ((regulatedHours == null) || (regulatedHours.Details == null) || (regulatedHours.Details.Count == 0))
                return;

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
                                OverstayVioEvent overstayEvent = new OverstayVioEvent();
                                overstayEvent.OverstayBasedOnRuleGroup = regulatedHours;
                                overstayEvent.OverstayBasedOnRuleDetail = detail;
                                overstayEvent.StartOfOverstayViolation = new DateTime(OccupiedIntersection.Start.Ticks).AddMinutes(timeLimitMinutes);
                                overstayEvent.DurationOfTimeBeyondStayLimits = new TimeSpan(OccupiedIntersection.Duration.Ticks).Add(new TimeSpan(0, (-1) * timeLimitMinutes, 0));

                                // Now let's see if there was an action taken during this violation period
                                DateTime minDateCutoff = overstayEvent.StartOfOverstayViolation;

                                // Get the most recent action taken for this space asset, but only if the report needs this type of data
                                if (this._RequiredDataElements.NeedsEnforcementActionData == true)
                                {
                                    OverstayVioActionsDTO ActionTakenDTO = result.GetVioActionForSpaceDuringTimeRange(this._CustomerConfig.CustomerId,
                                        currentReportableEvent.BayInfo.MeterID, currentReportableEvent.BayInfo.AreaID_PreferLibertyBeforeInternal,
                                        currentReportableEvent.BayInfo.SpaceID, overstayEvent.StartOfOverstayViolation, currentReportableEvent.SensorEvent_End);

                                    if ((ActionTakenDTO != null) && (minDateCutoff > DateTime.MinValue))
                                    {
                                        // If the action taken was recorded after the start of this violation, then it qualifies as being the action taken for this
                                        // violation condition. If so, retain the "Action Taken" reason
                                        if (ActionTakenDTO.EventTimestamp >= minDateCutoff)
                                        {
                                            overstayEvent.EnforcementActionTaken = ActionTakenDTO.ActionTaken;
                                            overstayEvent.EnforcementActionTakenTimeStamp = ActionTakenDTO.EventTimestamp;

                                            try
                                            {
                                                // Try to resolve the user associated with this action taken
                                                RBACProvider.RBACUserInfo actionTakenByUser = this._AllUsers.Find(item => item.DBUserCustomSID_AsInt == ActionTakenDTO.RBACUserID);
                                                if (actionTakenByUser != null)
                                                {
                                                    // Get the username, then chop off the domain info
                                                    string username = actionTakenByUser.UserName;
                                                    username = Duncan.PEMS.SpaceStatus.Models.LogOnModel.RemoveDomainFromUsername(username);
                                                    overstayEvent.EnforcementActionTakenByUser = username;
                                                }
                                            }
                                            catch { }
                                        }
                                    }
                                }

                                // Since this has been evaluated as an overstay violation, we will add it to the list that the report will use,
                                // but only if it also qualifies for whatever "Action Taken" filtering is in effect
                                bool activityTypeMatchesFiltering = false;
                                if (
                                    (this._ReportParams.ActionTakenRestrictionFilter == ReportableEnforcementActivity.AllActivity) ||
                                    ((this._ReportParams.ActionTakenRestrictionFilter == ReportableEnforcementActivity.Actioned) && (!string.IsNullOrEmpty(overstayEvent.EnforcementActionTaken)))
                                   )
                                {
                                    activityTypeMatchesFiltering = true;
                                }
                                else if (
                                    ((this._ReportParams.ActionTakenRestrictionFilter == ReportableEnforcementActivity.Cautioned) && (!string.IsNullOrEmpty(overstayEvent.EnforcementActionTaken)) &&
                                    (string.Compare(overstayEvent.EnforcementActionTaken, "Cautioned", true) == 0))
                                   )
                                {
                                    activityTypeMatchesFiltering = true;
                                }
                                else if (
                                    ((this._ReportParams.ActionTakenRestrictionFilter == ReportableEnforcementActivity.Enforced) && (!string.IsNullOrEmpty(overstayEvent.EnforcementActionTaken)) &&
                                    (string.Compare(overstayEvent.EnforcementActionTaken, "Enforced", true) == 0))
                                   )
                                {
                                    activityTypeMatchesFiltering = true;
                                }
                                else if (
                                    ((this._ReportParams.ActionTakenRestrictionFilter == ReportableEnforcementActivity.Fault) && (!string.IsNullOrEmpty(overstayEvent.EnforcementActionTaken)) &&
                                    (string.Compare(overstayEvent.EnforcementActionTaken, "Fault", true) == 0))
                                   )
                                {
                                    activityTypeMatchesFiltering = true;
                                }
                                else if (
                                    ((this._ReportParams.ActionTakenRestrictionFilter == ReportableEnforcementActivity.NotEnforced) && (!string.IsNullOrEmpty(overstayEvent.EnforcementActionTaken)) &&
                                    (string.Compare(overstayEvent.EnforcementActionTaken, "NotEnforced", true) == 0))
                                   )
                                {
                                    activityTypeMatchesFiltering = true;
                                }
                                else if (
                                    ((this._ReportParams.ActionTakenRestrictionFilter == ReportableEnforcementActivity.NotActioned) && (string.IsNullOrEmpty(overstayEvent.EnforcementActionTaken) == true))
                                   )
                                {
                                    activityTypeMatchesFiltering = true;
                                }

                                // Ok, if the activity filtering matches, then this really is a reportable overstay violation, 
                                // so we need to add it to the violation list of the main event
                                if (activityTypeMatchesFiltering == true)
                                    currentReportableEvent.Overstays.Add(overstayEvent);
                            }
                        }
                    }
                }

                // Rules for current day of week have been processed.  So now we will advance to beginning of next day and see if there are more violations that we will use
                // to add accumulated time in violation state...
                OccupiedSegment = new TimeSlot(new DateTime(OccupiedSegment.Start.Year, OccupiedSegment.Start.Month, OccupiedSegment.Start.Day).AddDays(1),
                    currentReportableEvent.SensorEvent_End);
            }
        }

        protected void AnalyzeForPaymentViolation(CommonSensorAndPaymentEvent currentReportableEvent,CustomerLogic result)
        {
            // There is nothing to do if this event is not for occupied status
            if (currentReportableEvent.SensorEvent_IsOccupied == false)
                return;

            // Don't do anything if the report doesn't need payment information
            if (this._RequiredDataElements.NeedsPaymentData == false)
                return;

            // Find the space asset associated with this data model.  If the space is "inactive" (based on the "IsActive" column of "HousingMaster" table in database),
            // then we will not consider the space to be in a violating state, because the sensor is effectively marked as bad/untrusted

            // Nothing more to do if the space isn't active
            if (currentReportableEvent.BayInfo.IsActive == false)
                return;

            TimeSlot OccupiedSegment = new TimeSlot(currentReportableEvent.SensorEvent_Start, currentReportableEvent.SensorEvent_End);
            List<TimeSlot> paidSegments = new List<TimeSlot>();
            foreach (PaymentEvent nextPayEvent in currentReportableEvent.PaymentEvents)
            {
                paidSegments.Add(new TimeSlot(nextPayEvent.PaymentEvent_Start, nextPayEvent.PaymentEvent_End));
            }
            List<TimeSlot> OccupiedAndUnpaidGaps = TimeSlotGapCalculator.GetGaps(OccupiedSegment, paidSegments);

            foreach (TimeSlot nextVioSlot in OccupiedAndUnpaidGaps)
            {
                PaymentVioEvent payVio = new PaymentVioEvent();
                payVio.StartOfPayViolation = nextVioSlot.Start;
                payVio.DurationOfTimeInViolation = nextVioSlot.Duration;

                // Get the most recent action taken for this space asset, but only if the report needs this type of data
                if (this._RequiredDataElements.NeedsEnforcementActionData == true)
                {
                    OverstayVioActionsDTO ActionTakenDTO = result.GetVioActionForSpaceDuringTimeRange(this._CustomerConfig.CustomerId,
                        currentReportableEvent.BayInfo.MeterID, currentReportableEvent.BayInfo.AreaID_PreferLibertyBeforeInternal,
                        currentReportableEvent.BayInfo.SpaceID, payVio.StartOfPayViolation, payVio.StartOfPayViolation.Add(payVio.DurationOfTimeInViolation));

                    DateTime minDateCutoff = payVio.StartOfPayViolation;

                    if ((ActionTakenDTO != null) && (minDateCutoff > DateTime.MinValue))
                    {
                        // If the action taken was recorded after the start of this violation, then it qualifies as being the action taken for this
                        // violation condition. If so, retain the "Action Taken" reason
                        if (ActionTakenDTO.EventTimestamp >= minDateCutoff)
                        {
                            payVio.EnforcementActionTaken = ActionTakenDTO.ActionTaken;
                            payVio.EnforcementActionTakenTimeStamp = ActionTakenDTO.EventTimestamp;

                            try
                            {
                                // Try to resolve the user associated with this action taken
                                RBACProvider.RBACUserInfo actionTakenByUser = this._AllUsers.Find(item => item.DBUserCustomSID_AsInt == ActionTakenDTO.RBACUserID);
                                if (actionTakenByUser != null)
                                {
                                    // Get the username, then chop off the domain info
                                    string username = actionTakenByUser.UserName;
                                    username = Duncan.PEMS.SpaceStatus.Models.LogOnModel.RemoveDomainFromUsername(username);
                                    payVio.EnforcementActionTakenByUser = username;
                                }
                            }
                            catch { }
                        }
                    }
                }

                // Now add to the event's payment violations
                currentReportableEvent.PaymentVios.Add(payVio);
                /*
                if (currentReportableEvent.PaymentVios.Count > 10)
                    System.Diagnostics.Debug.WriteLine("currentReportableEvent.PaymentVios.Count > 10");
                */
            }
        }

        #endregion
    }


    public class ComplianceOverstayReport : BaseSensorReport
    {
        public ComplianceOverstayReport(CustomerConfig customerCfg, SensorAndPaymentReportEngine.CommonReportParameters reportParams)
            : base(customerCfg, reportParams)
        {
            _ReportName = "Compliance - Overstay Report";
            numberOfHourlyCategories = 13;

            if (this._ReportParams.IncludeHourlyStatistics == false)
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
            this._ReportEngine.GatherReportData(listOfMeterIDs, requiredDataElements,  result);

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
                    RenderWorksheetHyperlink(ws, "Meter Overstay", "Meter Overstay summary");
                if (_ReportParams.IncludeSpaceSummary == true)
                    RenderWorksheetHyperlink(ws, "Space Overstay", "Space Overstay summary");
                if (_ReportParams.IncludeAreaSummary == true)
                    RenderWorksheetHyperlink(ws, "Area Overstay", "Area Overstay summary");
                if (_ReportParams.IncludeDailySummary == true)
                    RenderWorksheetHyperlink(ws, "Daily Overstay", "Daily Overstay summary");
                if (_ReportParams.IncludeMonthlySummary == true)
                    RenderWorksheetHyperlink(ws, "Monthly Overstay", "Monthly Overstay summary");
                if (_ReportParams.IncludeDetailRecords == true)
                    RenderWorksheetHyperlink(ws, "Details", "Overstay details");

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
                    RenderMeterSummaryWorksheet(pck, "Meter Overstay");
                }

                // Should we include a worksheet with Space aggregates?
                if (_ReportParams.IncludeSpaceSummary == true)
                {
                    RenderSpaceSummaryWorksheet(pck, "Space Overstay");
                }

                // Should we include a worksheet with Area aggregates?
                if (_ReportParams.IncludeAreaSummary == true)
                {
                    RenderAreaSummaryWorksheet(pck, "Area Overstay");
                }

                // Should we include a worksheet with Daily aggregates?
                if (_ReportParams.IncludeDailySummary == true)
                {
                    RenderDailySummaryWorksheet(pck, "Daily Overstay");
                }

                // Should we include a worksheet with Monthly aggregates?
                if (_ReportParams.IncludeDailySummary == true)
                {
                    RenderMonthlySummaryWorksheet(pck, "Monthly Overstay");
                }




                // Should we include a Details worksheet?
                if (_ReportParams.IncludeDetailRecords == true)
                {
                    // Create the worksheet
                    ws = pck.Workbook.Worksheets.Add("Details");
                    int detailColumnCount = 12;

                    // Render the header row
                    rowIdx = 1; // Excel uses 1-based indexes
                    ws.SetValue(rowIdx, 1, "Space #");
                    ws.SetValue(rowIdx, 2, "Meter #");
                    ws.SetValue(rowIdx, 3, "Area #");
                    ws.SetValue(rowIdx, 4, "Area");
                    ws.SetValue(rowIdx, 5, "Arrival");
                    ws.SetValue(rowIdx, 6, "Departure");
                    ws.SetValue(rowIdx, 7, "Duration of Overstay");
                    ws.SetValue(rowIdx, 8, "Violation Timestamp");
                    ws.SetValue(rowIdx, 9, "Action Taken");
                    ws.SetValue(rowIdx, 10, "Action Taken Timestamp");
                    ws.SetValue(rowIdx, 11, "Action Taken by User");
                    ws.SetValue(rowIdx, 12, "Overstay Rule");

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

                    foreach (SensorAndPaymentReportEngine.CommonSensorAndPaymentEvent repEvents in this._ReportEngine.ReportDataModel.ReportableEvents)
                    {
                        AreaAsset areaAsset = _ReportEngine.GetAreaAsset(repEvents.BayInfo.AreaID_PreferLibertyBeforeInternal);

                        foreach (SensorAndPaymentReportEngine.OverstayVioEvent overstay in repEvents.Overstays)
                        {
                            // Output row values for data
                            ws.SetValue(rowIdx, 1, repEvents.BayInfo.SpaceID);
                            ws.SetValue(rowIdx, 2, repEvents.BayInfo.MeterID);

                            if (areaAsset != null)
                            {
                                ws.SetValue(rowIdx, 3, areaAsset.AreaID);
                                ws.SetValue(rowIdx, 4, areaAsset.AreaName);
                            }

                            ws.SetValue(rowIdx, 5, repEvents.SensorEvent_Start);
                            ws.SetValue(rowIdx, 6, repEvents.SensorEvent_End);
                            ws.SetValue(rowIdx, 7, FormatTimeSpanAsHoursMinutesAndSeconds(overstay.DurationOfTimeBeyondStayLimits));

                            ws.SetValue(rowIdx, 8, overstay.StartOfOverstayViolation);
                            if (!string.IsNullOrEmpty(overstay.EnforcementActionTaken))
                                ws.SetValue(rowIdx, 9, overstay.EnforcementActionTaken);
                            if (overstay.EnforcementActionTakenTimeStamp > DateTime.MinValue)
                                ws.SetValue(rowIdx, 10, overstay.EnforcementActionTakenTimeStamp);
                            if (!string.IsNullOrEmpty(overstay.EnforcementActionTakenByUser))
                                ws.SetValue(rowIdx, 11, overstay.EnforcementActionTakenByUser);

                            StringBuilder sb = new StringBuilder();
                            sb.Append(Enum.ToObject(typeof(DayOfWeek), overstay.OverstayBasedOnRuleDetail.DayOfWeek).ToString() + " ");
                            sb.Append(overstay.OverstayBasedOnRuleDetail.StartTime.ToString("hh:mm:ss tt") + " - " +
                                    overstay.OverstayBasedOnRuleDetail.EndTime.ToString("hh:mm:ss tt") + ", ");
                            sb.Append(overstay.OverstayBasedOnRuleDetail.Type + ", Max Stay: " + overstay.OverstayBasedOnRuleDetail.MaxStayMinutes.ToString());

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

                            ws.SetValue(rowIdx, 12, sb.ToString());

                            // Increment the row index, which will now be the next row of our data
                            rowIdx++;
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
                    ApplyNumberStyleToColumn(ws, 6, 2, rowIdx, "yyyy-mm-dd hh:mm:ss tt", ExcelHorizontalAlignment.Right);
                    ApplyNumberStyleToColumn(ws, 8, 2, rowIdx, "yyyy-mm-dd hh:mm:ss tt", ExcelHorizontalAlignment.Right);
                    ApplyNumberStyleToColumn(ws, 10, 2, rowIdx, "yyyy-mm-dd hh:mm:ss tt", ExcelHorizontalAlignment.Right);

                    ApplyNumberStyleToColumn(ws, 7, 2, rowIdx, "@", ExcelHorizontalAlignment.Right); // String value, right-aligned

                    ApplyNumberStyleToColumn(ws, 9, 2, rowIdx, "@", ExcelHorizontalAlignment.Left);
                    ApplyNumberStyleToColumn(ws, 11, 2, rowIdx, "@", ExcelHorizontalAlignment.Left);
                    ApplyNumberStyleToColumn(ws, 12, 2, rowIdx, "@", ExcelHorizontalAlignment.Left);

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
            ws.SetValue(renderRowIdx, renderColIdx, "Occupied Duration"); // Column 1

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, "Max Possible" + Environment.NewLine + "Occupied Duration"); // Column 2
            ApplyWrapTextStyleToCell(ws, renderRowIdx, renderColIdx);
            ws.Column(renderColIdx).Width = 20;

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, "Overstay Duration"); // Column 3

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, "Overstay %"); // Column 4

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, "Average Overstay"); // Column 5

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, "Vehicle" + Environment.NewLine + "Arrivals"); // Column 6
            ApplyWrapTextStyleToCell(ws, renderRowIdx, renderColIdx);
            ws.Column(renderColIdx).Width = 12;

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, "Vehicle" + Environment.NewLine + "Departures"); // Column 7
            ApplyWrapTextStyleToCell(ws, renderRowIdx, renderColIdx);
            ws.Column(renderColIdx).Width = 12;

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, "Overstays" + Environment.NewLine + "Actioned"); // Column 8
            ApplyWrapTextStyleToCell(ws, renderRowIdx, renderColIdx);
            ws.Column(renderColIdx).Width = 12;

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, "Total" + Environment.NewLine + "Overstays"); // Column 9
            ApplyWrapTextStyleToCell(ws, renderRowIdx, renderColIdx);
            ws.Column(renderColIdx).Width = 12;

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, "Total" + Environment.NewLine + "Enforced"); // Column 10
            ApplyWrapTextStyleToCell(ws, renderRowIdx, renderColIdx);
            ws.Column(renderColIdx).Width = 12;

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, "Total" + Environment.NewLine + "Cautioned"); // Column 11
            ApplyWrapTextStyleToCell(ws, renderRowIdx, renderColIdx);
            ws.Column(renderColIdx).Width = 12;

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, "Total" + Environment.NewLine + "Not Enforced"); // Column 12
            ApplyWrapTextStyleToCell(ws, renderRowIdx, renderColIdx);
            ws.Column(renderColIdx).Width = 12;

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, "Total" + Environment.NewLine + "Faulty"); // Column 13
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
                ws.SetValue(renderRowIdx, renderColIdx, "Hourly Category");  // Column 14
                ws.Column(renderColIdx).Width = 24;

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
            ws.SetValue(renderRowIdx, renderColIdx, FormatTimeSpanAsHoursMinutesAndSeconds(statsObj.MaximumPotentialOccupancyTime));

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, FormatTimeSpanAsHoursMinutesAndSeconds(statsObj.TotalOverstayDuration));

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, statsObj.PercentageOverstayedDuration);

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, FormatTimeSpanAsHoursMinutesAndSeconds(statsObj.AverageLengthOfOverstay));

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, statsObj.ingress);

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, statsObj.egress);

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, statsObj.TotalOverstaysActioned);

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, statsObj.OverstayCount);

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, statsObj.TotalOverstaysEnforced);

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, statsObj.TotalOverstaysCautioned);

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, statsObj.TotalOverstaysNotEnforced);

            renderColIdx++;
            ws.SetValue(renderRowIdx, renderColIdx, statsObj.TotalOverstaysFaulty);

            ApplyNumberStyleToColumn(ws, colIdx_1stCommonColumn + 3, renderRowIdx, renderRowIdx, "###0.00", ExcelHorizontalAlignment.Right);
            ApplyNumberStyleToColumn(ws, colIdx_1stCommonColumn + 5, renderRowIdx, renderRowIdx, "########0", ExcelHorizontalAlignment.Right);
            ApplyNumberStyleToColumn(ws, colIdx_1stCommonColumn + 6, renderRowIdx, renderRowIdx, "########0", ExcelHorizontalAlignment.Right);
            ApplyNumberStyleToColumn(ws, colIdx_1stCommonColumn + 7, renderRowIdx, renderRowIdx, "########0", ExcelHorizontalAlignment.Right);
            ApplyNumberStyleToColumn(ws, colIdx_1stCommonColumn + 8, renderRowIdx, renderRowIdx, "########0", ExcelHorizontalAlignment.Right);
            ApplyNumberStyleToColumn(ws, colIdx_1stCommonColumn + 9, renderRowIdx, renderRowIdx, "########0", ExcelHorizontalAlignment.Right);
            ApplyNumberStyleToColumn(ws, colIdx_1stCommonColumn + 10, renderRowIdx, renderRowIdx, "########0", ExcelHorizontalAlignment.Right);
            ApplyNumberStyleToColumn(ws, colIdx_1stCommonColumn + 11, renderRowIdx, renderRowIdx, "########0", ExcelHorizontalAlignment.Right);
            ApplyNumberStyleToColumn(ws, colIdx_1stCommonColumn + 12, renderRowIdx, renderRowIdx, "########0", ExcelHorizontalAlignment.Right);

            // And now lets autosize the columns
            for (int autoSizeColIdx = colIdx_1stCommonColumn; autoSizeColIdx <= renderColIdx; autoSizeColIdx++)
            {
                using (OfficeOpenXml.ExcelRange col = ws.Cells[/*startRowIdx*/1, autoSizeColIdx, renderRowIdx, autoSizeColIdx])
                {
                    col.AutoFitColumns();
                    col.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                }
            }

            // And now finally we must manually size the columns that have wrap text (autofit doesn't work nicely when we have wrap text)
            ws.Column(colIdx_1stCommonColumn + 1).Width = 20;
            ws.Column(colIdx_1stCommonColumn + 5).Width = 12;
            ws.Column(colIdx_1stCommonColumn + 6).Width = 12;
            ws.Column(colIdx_1stCommonColumn + 7).Width = 12;
            ws.Column(colIdx_1stCommonColumn + 8).Width = 12;
            ws.Column(colIdx_1stCommonColumn + 9).Width = 12;
            ws.Column(colIdx_1stCommonColumn + 10).Width = 12;
            ws.Column(colIdx_1stCommonColumn + 11).Width = 12;
            ws.Column(colIdx_1stCommonColumn + 12).Width = 12;


            if (this._ReportParams.IncludeHourlyStatistics == true)
            {
                // Now we will construct the hourly category column, followed by hour detail columns

                ws.SetValue(renderRowIdx + 0, colIdx_HourlyCategory, "Occupied Duration");
                ws.SetValue(renderRowIdx + 1, colIdx_HourlyCategory, "Max Possible Duration");
                ws.SetValue(renderRowIdx + 2, colIdx_HourlyCategory, "Overstayed Duration");
                ws.SetValue(renderRowIdx + 3, colIdx_HourlyCategory, "% Overstayed");
                ws.SetValue(renderRowIdx + 4, colIdx_HourlyCategory, "Average Overstay");
                ws.SetValue(renderRowIdx + 5, colIdx_HourlyCategory, "Arrivals");
                ws.SetValue(renderRowIdx + 6, colIdx_HourlyCategory, "Departures");
                ws.SetValue(renderRowIdx + 7, colIdx_HourlyCategory, "Overstays Actioned");
                ws.SetValue(renderRowIdx + 8, colIdx_HourlyCategory, "Total Overstays");
                ws.SetValue(renderRowIdx + 9, colIdx_HourlyCategory, "Total Enforced");
                ws.SetValue(renderRowIdx + 10, colIdx_HourlyCategory, "Total Cautioned");
                ws.SetValue(renderRowIdx + 11, colIdx_HourlyCategory, "Total Not Enforced");
                ws.SetValue(renderRowIdx + 12, colIdx_HourlyCategory, "Total Faulty");

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
                ws.SetValue(renderRowIdx + 1, renderColIdx, FormatTimeSpanAsHoursMinutesAndSeconds(hourlyStats.MaximumPotentialOccupancyTime));
                ws.SetValue(renderRowIdx + 2, renderColIdx, FormatTimeSpanAsHoursMinutesAndSeconds(hourlyStats.TotalOverstayDuration));

                ws.SetValue(renderRowIdx + 3, renderColIdx, hourlyStats.PercentageOverstayedDuration);
                ApplyNumberStyleToCell(ws, renderRowIdx + 3, renderColIdx, "###0.00", ExcelHorizontalAlignment.Left);

                ws.SetValue(renderRowIdx + 4, renderColIdx, FormatTimeSpanAsHoursMinutesAndSeconds(hourlyStats.AverageLengthOfOverstay));

                ws.SetValue(renderRowIdx + 5, renderColIdx, hourlyStats.ingress);
                ApplyNumberStyleToCell(ws, renderRowIdx + 5, renderColIdx, "########0", ExcelHorizontalAlignment.Left);

                ws.SetValue(renderRowIdx + 6, renderColIdx, hourlyStats.egress);
                ApplyNumberStyleToCell(ws, renderRowIdx + 6, renderColIdx, "########0", ExcelHorizontalAlignment.Left);

                ws.SetValue(renderRowIdx + 7, renderColIdx, hourlyStats.TotalOverstaysActioned);
                ApplyNumberStyleToCell(ws, renderRowIdx + 7, renderColIdx, "########0", ExcelHorizontalAlignment.Left);

                ws.SetValue(renderRowIdx + 8, renderColIdx, hourlyStats.OverstayCount);
                ApplyNumberStyleToCell(ws, renderRowIdx + 8, renderColIdx, "########0", ExcelHorizontalAlignment.Left);

                ws.SetValue(renderRowIdx + 9, renderColIdx, hourlyStats.TotalOverstaysEnforced);
                ApplyNumberStyleToCell(ws, renderRowIdx + 9, renderColIdx, "########0", ExcelHorizontalAlignment.Left);

                ws.SetValue(renderRowIdx + 10, renderColIdx, hourlyStats.TotalOverstaysCautioned);
                ApplyNumberStyleToCell(ws, renderRowIdx + 10, renderColIdx, "########0", ExcelHorizontalAlignment.Left);

                ws.SetValue(renderRowIdx + 11, renderColIdx, hourlyStats.TotalOverstaysNotEnforced);
                ApplyNumberStyleToCell(ws, renderRowIdx + 11, renderColIdx, "########0", ExcelHorizontalAlignment.Left);

                ws.SetValue(renderRowIdx + 12, renderColIdx, hourlyStats.TotalOverstaysFaulty);
                ApplyNumberStyleToCell(ws, renderRowIdx + 12, renderColIdx, "########0", ExcelHorizontalAlignment.Left);
            }
        }
    }


    public class GroupStats
    {
        public TimeSpan TotalOverstayDuration { get; set; }
        public int OverstayCount { get; set; }

        public int TotalOverstaysEnforced { get; set; }
        public int TotalOverstaysCautioned { get; set; }
        public int TotalOverstaysNotEnforced { get; set; }
        public int TotalOverstaysFaulty { get; set; }
        public int TotalOverstaysActioned { get; set; }

        public int TotalPayViosEnforced { get; set; }
        public int TotalPayViosCautioned { get; set; }
        public int TotalPayViosNotEnforced { get; set; }
        public int TotalPayViosFaulty { get; set; }
        public int TotalPayViosActioned { get; set; }

        public int TotalViosActioned0To15Mins { get; set; }
        public int TotalViosActioned15To40Mins { get; set; }
        public int TotalViosActionedOver40Mins { get; set; }

        public TimeSpan TotalPayVioDuration { get; set; }

        public int TotalZeroedOutEvents { get; set; }
        public TimeSpan TotalZeroedOutDuration { get; set; }

        public TimeSpan TotalSensorEvent_Latency { get; set; }

        public int ingress { get; set; }       // Count of vehicle arrivals
        public int egress { get; set; }        // Count of vehicle departures
        public int PaymentCount { get; set; }
        public int PayVioCount { get; set; }

        public TimeSpan TotalPaidTime { get; set; }
        public TimeSpan TotalOccupancyTime { get; set; }
        public TimeSpan TotalOccupancyPaidTime { get; set; }
        public TimeSpan TotalOccupancyNotPaidTime { get; set; }

        public TimeSpan MaximumPotentialOccupancyTime { get; set; }

        public TimeSpan AverageOccupancyTime { get; set; }
        public TimeSpan AveragePaidTime { get; set; }

        public TimeSpan AverageLengthOfOverstay { get; set; }

        public float PercentageOccupancy { get; set; }
        public float PercentageOccupiedPaid { get; set; }
        public float PercentageOccupiedNotPaid { get; set; }

        public float PercentageOverstayedDuration { get; set; }


        public GroupStats()
        {
            InitValues();
        }

        protected virtual void InitValues()
        {
            this.TotalOverstayDuration = new TimeSpan();
            this.OverstayCount = 0;

            this.TotalOverstaysEnforced = 0;
            this.TotalOverstaysCautioned = 0;
            this.TotalOverstaysNotEnforced = 0;
            this.TotalOverstaysFaulty = 0;
            this.TotalOverstaysActioned = 0;

            this.TotalPayViosEnforced = 0;
            this.TotalPayViosCautioned = 0;
            this.TotalPayViosNotEnforced = 0;
            this.TotalPayViosFaulty = 0;
            this.TotalPayViosActioned = 0;

            this.TotalViosActioned0To15Mins = 0;
            this.TotalViosActioned15To40Mins = 0;
            this.TotalViosActionedOver40Mins = 0;

            this.TotalPayVioDuration = new TimeSpan();

            this.TotalSensorEvent_Latency = new TimeSpan();

            this.TotalZeroedOutEvents = 0;
            this.TotalZeroedOutDuration = new TimeSpan();

            
            this.ingress = 0;
            this.egress = 0;
            this.PaymentCount = 0;
            this.PayVioCount = 0;

            this.TotalPaidTime = new TimeSpan();
            this.TotalOccupancyTime = new TimeSpan();
            this.TotalOccupancyPaidTime = new TimeSpan();
            this.TotalOccupancyNotPaidTime = new TimeSpan();

            this.MaximumPotentialOccupancyTime = new TimeSpan();

            this.AverageOccupancyTime = new TimeSpan();
            this.AveragePaidTime = new TimeSpan();
            this.AverageLengthOfOverstay = new TimeSpan();

            this.PercentageOccupancy = 0.00f;
            this.PercentageOccupiedPaid = 0.00f;
            this.PercentageOccupiedNotPaid = 0.00f;

            this.PercentageOverstayedDuration = 0.00f;
        }

        public virtual void AggregateSelf()
        {
            this.TotalOccupancyNotPaidTime = this.TotalOccupancyTime - this.TotalOccupancyPaidTime;

            if (this.TotalOccupancyTime.TotalMilliseconds == 0)
            {
                this.PercentageOccupiedPaid = (float)0.00f;
                this.PercentageOccupiedNotPaid = (float)0.00f;
            }
            else
            {
                //this.PercentageOccupiedPaid = (float)Math.Round(((this.TotalOccupancyPaidTime.TotalMilliseconds / this.TotalOccupancyTime.TotalMilliseconds) * 100), 2);
                //this.PercentageOccupiedNotPaid = (float)Math.Round((((this.TotalOccupancyTime.TotalMilliseconds - this.TotalOccupancyPaidTime.TotalMilliseconds) / this.TotalOccupancyTime.TotalMilliseconds) * 100), 2);

                // Hans -- modified to remove rounding of decimal formore precise reporting
                this.PercentageOccupiedPaid = (float)((this.TotalOccupancyPaidTime.TotalMilliseconds / this.TotalOccupancyTime.TotalMilliseconds) * 100.0f);
                this.PercentageOccupiedNotPaid = (float)(((this.TotalOccupancyTime.TotalMilliseconds - this.TotalOccupancyPaidTime.TotalMilliseconds) / this.TotalOccupancyTime.TotalMilliseconds) * 100.0f);
            }

            if (this.MaximumPotentialOccupancyTime.TotalMilliseconds == 0)
            {
                this.PercentageOccupancy = (float)0.00f;
            }
            else
            {
                //this.PercentageOccupancy = (float)Math.Round(((this.TotalOccupancyTime.TotalMilliseconds / this.MaximumPotentialOccupancyTime.TotalMilliseconds) * 100), 2);
                this.PercentageOccupancy = (float)((this.TotalOccupancyTime.TotalMilliseconds / this.MaximumPotentialOccupancyTime.TotalMilliseconds) * 100.0f);
            }


            if (Math.Max(this.ingress, this.egress) > 0)
            {
                this.AverageOccupancyTime = new TimeSpan(this.TotalOccupancyTime.Ticks / Math.Max(this.ingress, this.egress));
            }

            if (this.PaymentCount != 0)
            {
                this.AveragePaidTime = new TimeSpan(this.TotalPaidTime.Ticks / this.PaymentCount);
            }


            if (this.OverstayCount != 0)
            {
                this.AverageLengthOfOverstay = new TimeSpan(this.TotalOverstayDuration.Ticks / this.OverstayCount);
            }

            // DEBUG: Should this percentage be based on occupancy time or vehicle arrivals?
            if ((this.TotalOverstayDuration.TotalMilliseconds == 0) || (this.MaximumPotentialOccupancyTime.TotalMilliseconds == 0))
            {
                this.PercentageOverstayedDuration = (float)0.00f;
            }
            else
            {
                this.PercentageOverstayedDuration = (float)((this.TotalOverstayDuration.TotalMilliseconds / this.MaximumPotentialOccupancyTime.TotalMilliseconds) * 100.0f);
            }
        }
    }
}