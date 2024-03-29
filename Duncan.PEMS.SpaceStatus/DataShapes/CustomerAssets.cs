﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using System.Web.Mvc;
using System.Web.Routing;

namespace Duncan.PEMS.SpaceStatus.DataShapes
{
    #region CustomerConfig implementation

        

    /// <summary>
    /// DTO class to hold general customer information and configuration options -- doesn't contain business logic or secrets like DB connection strings, etc.
    /// </summary>
    public class CustomerConfig
    {
        public enum VehSensingProviders { None, Legacy, Banner, StreetLine, GTC };

        // DEBUG: This was added for IPI demo reasons because the quality of populated asset info in database isn't good enough to know
        // if a space is a sensor-only, or if there is a meter and PAM associated with it, etc...
        [RenderMode(RenderMode.None)]
        public bool ForcePamIsApplicable
        {
            get
            {
                bool result = false;
                if ((this.CustomerId == 9994) && (this.CustomerName == "Sensor Testing"))
                    result = true;
                return result;
            }
        }

        [DisplayName("Customer Name")]
        public string CustomerName { get; set; }

        [RenderMode(RenderMode.None)]
        public string CustomerNameForFiles { get; set; }

        [DisplayName("Customer ID")]
        //[RenderMode(RenderMode.DisplayModeOnly)]
        [HiddenInput(DisplayValue = true)] // false
        public int CustomerId { get; set; }

        //[RenderMode(RenderMode.None)]
        //public string StreetLineCustomerName { get; set; }

        //[RenderMode(RenderMode.None)]
        //public string StreetLineCustomerID { get; set; }

        //[RenderMode(RenderMode.None)]
        //public string ParkingSpotStatusURI { get; set; }

        //[RenderMode(RenderMode.None)]
        //public string PollParkingSpotStatusURI { get; set; }

        //[RenderMode(RenderMode.None)]
        //public string GetParkingSpotStatusURI { get; set; }

        [DisplayName("Vehicle Sensing Enabled?")]
        public bool VehSensingEnabled { get; set; }
        //public bool VehSensingIsRipNet192 { get; set; }

        private VehSensingProviders _VehSensingProvider = VehSensingProviders.GTC;

        [RenderMode(RenderMode.None)]
        [DisplayName("Vehicle Sensor Provider")]
        public VehSensingProviders VehSensingProvider
        {
            get { return _VehSensingProvider; }
            set { _VehSensingProvider = value; }
        }

        //[RenderMode(RenderMode.None)]
        //public bool GISViewEnabled { get; set; }

        [DisplayName("Grace Period Enabled?")]
        public bool GracePeriodEnabled { get; set; }

        public int DesktopRefreshTimeInSeconds { get; set; }
        public int MobileRefreshTimeInSeconds { get; set; }

        [RenderMode(RenderMode.None)]
        public string InvalidBayString { get; set; }

        public int ExpiryTimeToBeCriticalInSeconds { get; set; }

        [DisplayName("Time Zone")]
        [DropDownList("TimeZoneDisplayNames", "DataValue", "DataText" /*, "[Select TimeZone]"*/)]
        public string DestinationTimeZoneDisplayName { get; set; }


        [RenderMode(RenderMode.None)]
        public string ShortSecondsSpanFormat { get; set; }

        [RenderMode(RenderMode.None)]
        public string ShortHoursMinutesSpanFormat { get; set; }

        [RenderMode(RenderMode.None)]
        public string ShortDaysSpanFormat { get; set; }

        [RenderMode(RenderMode.None)]
        public string ShortMonthsSpanFormat { get; set; }

        [RenderMode(RenderMode.None)]
        public string ShortYearsSpanFormat { get; set; }
        
        public int BayOccupancyToleranceInMinutes { get; set; }

        //[RenderMode(RenderMode.None)]
        //public string MobileBayOccupancyOccupiedString { get; set; }
        //[RenderMode(RenderMode.None)]
        //public string MobileBayOccupancyEmptyString { get; set; }
        //[RenderMode(RenderMode.None)]
        //public string MobileBayOccupancyNotAvailableString { get; set; }
        //[RenderMode(RenderMode.None)]
        //public string MobileBayOccupancyOutOfDateString { get; set; }
        //[RenderMode(RenderMode.None)]
        //public string MobileBayViolation { get; set; }
        //[RenderMode(RenderMode.None)]
        //public string MobileBayMeterFeeding { get; set; }
        //[RenderMode(RenderMode.None)]
        //public string MobileBayUnknown { get; set; }
        
        //public int TimeDurationForPayment { get; set; }
        //public int AllowedBayOccupancyTime { get; set; }


        public Duncan.PEMS.SpaceStatus.UtilityClasses.TimeZoneInfo ServerTimeZone = null;
        public Duncan.PEMS.SpaceStatus.UtilityClasses.TimeZoneInfo CustomerTimeZone = null;

        public int GetJavaScriptTicksForMobileRefreshInterval()
        {
            //int result = 30000; // Default to 30,000 = 30 seconds
            int result = MobileRefreshTimeInSeconds * 1000;
            return result;
        }

        public int GetJavaScriptTicksForDesktopRefreshInterval()
        {
            int result = DesktopRefreshTimeInSeconds * 1000;
            return result;
        }

        public int GetJavaScriptTicksForDemoRefreshInterval()
        {
            int result = 2000;
            return result;
        }

        public CustomerConfig()
        {
            // Init default values
            this.CustomerId = -1;
            this.CustomerName = string.Empty;
            this.CustomerNameForFiles = string.Empty;
            this.VehSensingEnabled = true;
            this.VehSensingProvider = VehSensingProviders.GTC;
            this.DesktopRefreshTimeInSeconds = 30;
            this.MobileRefreshTimeInSeconds = 45;
            this.InvalidBayString = "--";
            this.ExpiryTimeToBeCriticalInSeconds = 300; // Default is 300 seconds (5 Minutes)
            this.DestinationTimeZoneDisplayName = "(GMT-08:00) Pacific Time (US & Canada)";
            this.ShortSecondsSpanFormat = "{0}S";
            this.ShortHoursMinutesSpanFormat = "{0:00}:{1:00}";
            this.ShortDaysSpanFormat = "{0}D";
            this.ShortMonthsSpanFormat = "{0}M";
            this.ShortYearsSpanFormat = "{0}Y";
            this.BayOccupancyToleranceInMinutes = 129600;
        }

        public CustomerConfig ShallowCopy()
        {
            return (CustomerConfig)this.MemberwiseClone();
        }

    }
    #endregion

    public sealed class CustomerConfigComparer : System.Collections.Generic.IComparer<CustomerConfig>
    {
        private static readonly System.Collections.Generic.IComparer<CustomerConfig> _default = new CustomerConfigComparer(true);
        private bool _SortByName = false;

        public CustomerConfigComparer(bool SortByName)
        {
            _SortByName = SortByName;
        }

        public static System.Collections.Generic.IComparer<CustomerConfig> Default
        {
            get { return _default; }
        }

        public int Compare(CustomerConfig dto1, CustomerConfig dto2)
        {
            // Are we sorting by Name or ID?
            if (this._SortByName == true)
                return string.CompareOrdinal(dto1.CustomerName, dto2.CustomerName);
            else
                return dto1.CustomerId.CompareTo(dto2.CustomerId);
        }
    }

    public class SpaceAsset
    {
        // We will use [XmlElement] attributes to trigger our DataMapper source code generator
        // to know when the DB column name differs from the property name.
        
        // Also note that we can use the [XmlIgnore] attribute if we don't want to attempt
        // populating a property from a database field


        [XmlElement("ParkingSpaceID")]
        public Int64 ParkingSpaceId_Internal { get; set; }

        [XmlElement("BayNumber")]
        public int SpaceID { get; set; }

        [XmlElement("AreaID")]
        public int AreaID_Internal { get; set; }

        [XmlElement("LibertyArea")]
        public int AreaID_Liberty { get; set; }

        public int AreaID_PreferLibertyBeforeInternal
        {
            get
            {
                if (this.AreaID_Liberty != -1)
                    return this.AreaID_Liberty;
                else
                    return this.AreaID_Internal;
            }
        }

        public int MeterID { get; set; }

        public string MeterName { get; set; }

        [XmlElement("ClusterID")]
        public int PAMClusterID { get; set; }

         [XmlElement("HasSensor")]
        public bool HasSensor { get; set; }
        public int SpaceType { get; set; } // 0 = SENSOR-ONLY, 1 = METERED-SPACE (-1 will be considered as a metered space)

        public float Latitude { get; set; }
        public float Longitude { get; set; }

        [XmlElement("CollRouteID")]
        public int CollectionRouteID { get; set; }

        [XmlElement("EnfRouteID")]
        public int EnforcementRouteID { get; set; }

        [XmlElement("MaintRouteID")]
        public int MaintRouteID { get; set; }

        [XmlElement("CustomGroup1")]
        public int CustomGroup1ID { get; set; }

        [XmlElement("CustomGroup2")]
        public int CustomGroup2ID { get; set; }

        [XmlElement("CustomGroup3")]
        public int CustomGroup3ID { get; set; }

        public bool IsActive { get; set; }

        public bool IsSensorOnly
        {
            get { return this.SpaceType == 0; }
        }

        public SpaceAsset()
        {
            // Init key fields to -1
            ParkingSpaceId_Internal = -1;
            SpaceID = -1;
            AreaID_Internal = -1;
            AreaID_Liberty = -1;
            MeterID = -1;
            PAMClusterID = -1;
            SpaceType = -1;
            CollectionRouteID = -1;
            EnforcementRouteID = -1;
            MaintRouteID = -1;
            CustomGroup1ID = -1;
            CustomGroup2ID = -1;
            CustomGroup3ID = -1;
            IsActive = true; // Assume the space is active
            HasSensor = false;
        }
    }

    public class MeterAsset
    {
        public int MeterID { get; set; }        

        [XmlElement("AreaID")]
        public int AreaID_Internal { get; set; }

        [XmlElement("LibertyArea")]
        public int AreaID_Liberty { get; set; }

        public int AreaID_PreferLibertyBeforeInternal
        {
            get
            {
                if (this.AreaID_Liberty != -1)
                    return this.AreaID_Liberty;
                else
                    return this.AreaID_Internal;
            }
        }

        [XmlElement("ClusterID")]
        public int PAMClusterID { get; set; }

        public string MeterName { get; set; }

        [XmlElement("Description")]
        public string MeterDescription { get; set; }

        [XmlElement("MeterGroup")]
        public int MeterGroupID { get; set; }

        public string MeterGroupDesc { get; set; }

        [XmlElement("MeterType")]
        public int MeterTypeID { get; set; }

        public float Latitude { get; set; }
        public float Longitude { get; set; }


        public MeterAsset()
        {
            // Init key fields to -1
            MeterID = -1;            
            AreaID_Internal = -1;
            AreaID_Liberty = -1;
            PAMClusterID = -1;
            MeterGroupID = -1;
            MeterTypeID = -1;
        }
    }

    // MeterWithArea class is used in the service methods to supply outside applications with limited info
    public class MeterWithArea
    {
        [XmlAttribute("MeterID")]
        public int MeterID { get; set; }

        [XmlAttribute("AreaID")]
        public int AreaID { get; set; }

        public MeterWithArea()
        {
        }
    }

    public class PAMClusterAsset
    {
        public int ClusterID { get; set; }

        [XmlElement("Description")]
        public string ClusterDescription { get; set; }
        
        public int HostedBayStart { get; set; }
        public int HostedBayEnd { get; set; }

        public PAMClusterAsset()
        {
            // Init key fields to -1
            ClusterID = -1;
        }
    }

    public class AreaAsset
    {
        public int AreaID { get; set; }
        public string AreaName { get; set; }

        [XmlElement("Description")]
        public string AreaDescription { get; set; }

        public AreaAsset()
        {
            // Init key fields to -1
            AreaID = -1;
        }
    }

    public class CustomGroup1Asset
    {
        public int CustomGroupId { get; set; }
        public string DisplayName { get; set; }

        [XmlElement("Comment")]
        public string Comment { get; set; }

        public CustomGroup1Asset()
        {
            // Init key fields to -1
            CustomGroupId = -1;
        }
    }

    public class CustomerNameAndId
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }

        public CustomerNameAndId()
        {
        }

    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class RenderModeAttribute : Attribute
    {
        public RenderMode RenderMode { get; set; }

        public RenderModeAttribute(RenderMode renderMode)
        {
            RenderMode = renderMode;
        }
    }

    public enum RenderMode
    {
        Any,
        EditModeOnly,
        DisplayModeOnly,
        None
    }

    // Instaid of Global.asax

    public interface ITemplateField
    {
        string TemplateName
        {
            get;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class DropDownListAttribute : Attribute, ITemplateField
    {
        private static string defaultTemplateName;

        public DropDownListAttribute(string viewDataKey, string dataValueField)
            : this(viewDataKey, dataValueField, null)
        {
        }

        public DropDownListAttribute(string viewDataKey, string dataValueField, string dataTextField)
            : this(viewDataKey, dataValueField, dataTextField, null)
        {
        }

        public DropDownListAttribute(string viewDataKey, string dataValueField, string dataTextField, string optionLabel)
            : this(DefaultTemplateName, viewDataKey, dataValueField, dataTextField, optionLabel, null)
        {
        }

        public DropDownListAttribute(string viewDataKey, string dataValueField, string dataTextField, string optionLabel, object htmlAttributes)
            : this(DefaultTemplateName, viewDataKey, dataValueField, dataTextField, optionLabel, htmlAttributes)
        {
        }

        public DropDownListAttribute(string templateName, string viewDataKey, string dataValueField, string dataTextField, string optionLabel, object htmlAttributes)
        {
            if (string.IsNullOrEmpty(templateName))
            {
                throw new ArgumentException("Template name cannot be empty.");
            }

            if (string.IsNullOrEmpty(viewDataKey))
            {
                throw new ArgumentException("View data key cannot be empty.");
            }

            if (string.IsNullOrEmpty(dataValueField))
            {
                throw new ArgumentException("Data value field cannot be empty.");
            }

            TemplateName = templateName;
            ViewDataKey = viewDataKey;
            DataValueField = dataValueField;
            DataTextField = dataTextField;
            OptionLabel = optionLabel;
            HtmlAttributes = new RouteValueDictionary(htmlAttributes);

            HtmlAttributes.Add("class", "chzn-select");
            //HtmlAttributes.Add("width", "100%");
        }

        public static string DefaultTemplateName
        {
            get
            {
                if (string.IsNullOrEmpty(defaultTemplateName))
                {
                    defaultTemplateName = "DropDownList";
                }

                return defaultTemplateName;
            }
            set
            {
                defaultTemplateName = value;
            }
        }

        public string TemplateName { get; private set; }

        public string ViewDataKey { get; private set; }

        public string DataValueField { get; private set; }

        public string DataTextField { get; private set; }

        public string OptionLabel { get; private set; }

        public IDictionary<string, object> HtmlAttributes { get; private set; }

        public object GetSelectedValue(object model)
        {
            return GetPropertyValue(model, DataValueField);
        }

        public object GetSelectedText(object model)
        {
            return GetPropertyValue(model, !string.IsNullOrEmpty(DataTextField) ? DataTextField : DataValueField);
        }

        private static object GetPropertyValue(object model, string propertyName)
        {
            if (model != null)
            {
                PropertyDescriptor property = GetTypeDescriptor(model.GetType()).GetProperties()
                                                                                .Cast<PropertyDescriptor>()
                                                                                .SingleOrDefault(p => string.Compare(p.Name, propertyName, StringComparison.OrdinalIgnoreCase) == 0);

                if (property != null)
                {
                    return property.GetValue(model);
                }
            }

            return null;
        }

        private static ICustomTypeDescriptor GetTypeDescriptor(Type type)
        {
            return new AssociatedMetadataTypeTypeDescriptionProvider(type).GetTypeDescriptor(type);
        }
    }


}