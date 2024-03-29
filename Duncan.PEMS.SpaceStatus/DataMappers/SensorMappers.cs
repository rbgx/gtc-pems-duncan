﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

using Duncan.PEMS.SpaceStatus.DataShapes;


namespace Duncan.PEMS.SpaceStatus.DataMappers
{
    #region HistoricalSensingRecordDataMapper
    public class HistoricalSensingRecordDataMapper : IDataMapper
    {
        private bool _isInitialized = false;
        private int _ordinal_LastUpdatedTS;
        private int _ordinal_RecCreationDate;
        private int _ordinal_LastStatus;
        private int _ordinal_BayNumber;
        private int _ordinal_MeterId;

        private void InitializeMapper(IDataReader reader)
        {
            PopulateOrdinals(reader);
            _isInitialized = true;
        }

        public void PopulateOrdinals(IDataReader reader)
        {
            _ordinal_LastUpdatedTS = reader.GetOrdinal("LastUpdatedTS");
            _ordinal_RecCreationDate = reader.GetOrdinal("RecCreationDate");
            _ordinal_LastStatus = reader.GetOrdinal("LastStatus");
            _ordinal_BayNumber = reader.GetOrdinal("BayNumber");
            _ordinal_MeterId = reader.GetOrdinal("MeterId");
        }

        public Object GetData(IDataReader reader)
        {
            // This is where we define the mapping between the object properties and the 
            // data columns. The convention that should be used is that the object property 
            // names are exactly the same as the column names. However if there is some 
            // compelling reason for the names to be different, the mapping can be defined here.

            // We assume the reader has data and is already on the row that contains the data 
            // we need. We don't need to call read. As a general rule, assume that every field must 
            // be null  checked. If a field is null then the nullvalue for that field has already 
            // been set by the DTO constructor, we don't have to change it.

            if (!_isInitialized) { InitializeMapper(reader); }

            // Now we can load the data into the DTO object from the DB reader
            HistoricalSensingRecord dto = new HistoricalSensingRecord();

            if (!reader.IsDBNull(_ordinal_LastUpdatedTS)) { dto.DateTime = Convert.ToDateTime(reader[_ordinal_LastUpdatedTS]); }
            if (!reader.IsDBNull(_ordinal_RecCreationDate)) { dto.RecCreationDateTime = Convert.ToDateTime(reader[_ordinal_RecCreationDate]); }
            if (!reader.IsDBNull(_ordinal_BayNumber)) { dto.BayId = Convert.ToInt32(reader[_ordinal_BayNumber]); }
            if (!reader.IsDBNull(_ordinal_MeterId)) { dto.MeterMappingId = Convert.ToInt32(reader[_ordinal_MeterId]); }

            // IsOccupied property requires some extra customization beyond the auto-generated sourcecode for this class
            // because LastStatus may contain other integer values, and we are only interested in explicitly matching on "1"
            /*if (!reader.IsDBNull(_ordinal_LastStatus)) { dto.IsOccupied = Convert.ToBoolean(reader[_ordinal_LastStatus]); }*/
            if (!reader.IsDBNull(_ordinal_LastStatus)) { dto.IsOccupied = (Convert.ToInt32(reader[_ordinal_LastStatus]) == 1); }
            
            return dto;
        }
    }
    #endregion

    #region CurrentSpaceOccupancyInformationDataMapper
    public class CurrentSpaceOccupancyInformationDataMapper : IDataMapper
    {
        private bool _isInitialized = false;
        private int _ordinal_LastUpdatedTS;
        private int _ordinal_LastStatus;
        private int _ordinal_CustomerId;
        private int _ordinal_MeterId;
        private int _ordinal_BayNumber;
        private int _ordinal_HasSensor;

        private void InitializeMapper(IDataReader reader)
        {
            PopulateOrdinals(reader);
            _isInitialized = true;
        }

        public void PopulateOrdinals(IDataReader reader)
        {
            _ordinal_LastUpdatedTS = reader.GetOrdinal("LastUpdatedTS");
            _ordinal_LastStatus = reader.GetOrdinal("LastStatus");
            _ordinal_CustomerId = reader.GetOrdinal("CustomerId");
            _ordinal_MeterId = reader.GetOrdinal("MeterId");
            _ordinal_BayNumber = reader.GetOrdinal("BayNumber");
             _ordinal_HasSensor = reader.GetOrdinal("HasSensor");
        }

        public Object GetData(IDataReader reader)
        {
            // This is where we define the mapping between the object properties and the 
            // data columns. The convention that should be used is that the object property 
            // names are exactly the same as the column names. However if there is some 
            // compelling reason for the names to be different, the mapping can be defined here.

            // We assume the reader has data and is already on the row that contains the data 
            // we need. We don't need to call read. As a general rule, assume that every field must 
            // be null  checked. If a field is null then the nullvalue for that field has already 
            // been set by the DTO constructor, we don't have to change it.

            if (!_isInitialized) { InitializeMapper(reader); }

            // Now we can load the data into the DTO object from the DB reader
            CurrentSpaceOccupancyInformation dto = new CurrentSpaceOccupancyInformation();

            if (!reader.IsDBNull(_ordinal_LastUpdatedTS)) { dto.LastInOut = Convert.ToDateTime(reader[_ordinal_LastUpdatedTS]); }
            if (!reader.IsDBNull(_ordinal_CustomerId)) { dto.CustomerID = Convert.ToInt32(reader[_ordinal_CustomerId]); }
            if (!reader.IsDBNull(_ordinal_MeterId)) { dto.MeterID = Convert.ToInt32(reader[_ordinal_MeterId]); }
            if (!reader.IsDBNull(_ordinal_BayNumber)) { dto.BayID = Convert.ToInt32(reader[_ordinal_BayNumber]); }
            if (!reader.IsDBNull(_ordinal_HasSensor)) { dto.HasSensor = Convert.ToBoolean(reader[_ordinal_HasSensor]); }
            // IsOccupied property requires some extra customization beyond the auto-generated sourcecode for this class
            // because LastStatus may contain other integer values, and we are only interested in explicitly matching on "1"
            /*if (!reader.IsDBNull(_ordinal_LastStatus)) { dto.IsOccupied = Convert.ToBoolean(reader[_ordinal_LastStatus]); }*/
            if (!reader.IsDBNull(_ordinal_LastStatus)) { dto.IsOccupied = (Convert.ToInt32(reader[_ordinal_LastStatus]) == 1); }

            return dto;
        }
    }
    #endregion

    #region SensorEventAndCommsRecordDataMapper
    public class SensorEventAndCommsRecordDataMapper : IDataMapper
    {
        private bool _isInitialized = false;
        private int _ordinal_MeterId;
        private int _ordinal_BayNumber;
        private int _ordinal_AreaId;
        private int _ordinal_MeterName;
        private int _ordinal_HasSensor;
        private int _ordinal_MaxBaysEnabled;
        private int _ordinal_MeterGroup;
        private int _ordinal_MeterType;
        private int _ordinal_LastSensorStatusTime;
        private int _ordinal_LastSensorStatus;
        private int _ordinal_LastCommunication = -1; // Special case because this is an "optional" field
        private int _ordinal_LastCommunicationMessage = -1; // Special case because this is an "optional" field

        private void InitializeMapper(IDataReader reader)
        {
            PopulateOrdinals(reader);
            _isInitialized = true;
        }

        private bool ColumnExists(IDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (string.Compare(reader.GetName(i), columnName, true) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        public void PopulateOrdinals(IDataReader reader)
        {
            _ordinal_MeterId = reader.GetOrdinal("MeterId");
            _ordinal_BayNumber = reader.GetOrdinal("BayNumber");
            _ordinal_AreaId = reader.GetOrdinal("AreaId");
            _ordinal_MeterName = reader.GetOrdinal("MeterName");
            _ordinal_HasSensor = reader.GetOrdinal("HasSensor");
            _ordinal_MaxBaysEnabled = reader.GetOrdinal("MaxBaysEnabled");
            _ordinal_MeterGroup = reader.GetOrdinal("MeterGroup");
            _ordinal_MeterType = reader.GetOrdinal("MeterType");
            _ordinal_LastSensorStatusTime = reader.GetOrdinal("LastSensorStatusTime");
            _ordinal_LastSensorStatus = reader.GetOrdinal("LastSensorStatus");

            // This is customized beyond the auto-generated sourcecode because these 
            // fields will be optional, depending on the actual SQL query that was executed
            if (ColumnExists(reader, "LastCommunication"))
                _ordinal_LastCommunication = reader.GetOrdinal("LastCommunication");
            if (ColumnExists(reader, "LastCommunicationMessage"))
                _ordinal_LastCommunicationMessage = reader.GetOrdinal("LastCommunicationMessage");
        }

        public Object GetData(IDataReader reader)
        {
            // This is where we define the mapping between the object properties and the 
            // data columns. The convention that should be used is that the object property 
            // names are exactly the same as the column names. However if there is some 
            // compelling reason for the names to be different, the mapping can be defined here.

            // We assume the reader has data and is already on the row that contains the data 
            // we need. We don't need to call read. As a general rule, assume that every field must 
            // be null  checked. If a field is null then the nullvalue for that field has already 
            // been set by the DTO constructor, we don't have to change it.

            if (!_isInitialized) { InitializeMapper(reader); }

            // Now we can load the data into the DTO object from the DB reader
            SensorEventAndCommsRecord dto = new SensorEventAndCommsRecord();

            if (!reader.IsDBNull(_ordinal_MeterId)) { dto.MeterID = Convert.ToInt32(reader[_ordinal_MeterId]); }
            if (!reader.IsDBNull(_ordinal_BayNumber)) { dto.BayID = Convert.ToInt32(reader[_ordinal_BayNumber]); }
            if (!reader.IsDBNull(_ordinal_AreaId)) { dto.AreaID = Convert.ToInt32(reader[_ordinal_AreaId]); }
            if (!reader.IsDBNull(_ordinal_MeterName)) { dto.MeterName = Convert.ToString(reader[_ordinal_MeterName]); }
            if (!reader.IsDBNull(_ordinal_HasSensor)) { dto.HasSensor = Convert.ToBoolean(reader[_ordinal_HasSensor]); }
            if (!reader.IsDBNull(_ordinal_MaxBaysEnabled)) { dto.MaxBaysEnabled = Convert.ToInt32(reader[_ordinal_MaxBaysEnabled]); }
            if (!reader.IsDBNull(_ordinal_MeterGroup)) { dto.MeterGroup = Convert.ToInt32(reader[_ordinal_MeterGroup]); }
            if (!reader.IsDBNull(_ordinal_MeterType)) { dto.MeterType = Convert.ToInt32(reader[_ordinal_MeterType]); }
            if (!reader.IsDBNull(_ordinal_LastSensorStatusTime)) { dto.LastSensorStatusTime = Convert.ToDateTime(reader[_ordinal_LastSensorStatusTime]); }
            if (!reader.IsDBNull(_ordinal_LastSensorStatus)) { dto.LastSensorStatus = Convert.ToInt32(reader[_ordinal_LastSensorStatus]); }

            // Customized beyond the auto-generated source code
            // LastCommunication and LastCommunicationMessage columns should only be present when doing a "current" status query.
            // They will not be present in the "historical" records...
            if (_ordinal_LastCommunication != -1)
            {
                if (!reader.IsDBNull(_ordinal_LastCommunication)) { dto.LastCommunication = Convert.ToDateTime(reader[_ordinal_LastCommunication]); }
            }

            if (_ordinal_LastCommunicationMessage != -1)
            {
                if (!reader.IsDBNull(_ordinal_LastCommunicationMessage)) { dto.LastCommunicationType = Convert.ToString(reader[_ordinal_LastCommunicationMessage]); }
            }

            return dto;
        }
    }
    #endregion

    #region SensorHeartbeatRecordDataMapper
    public class SensorHeartbeatRecordDataMapper : IDataMapper
    {
        private bool _isInitialized = false;
        private int _ordinal_MeterId;
        private int _ordinal_BayNumber;
        private int _ordinal_AreaId;
        private int _ordinal_MeterName;
        private int _ordinal_HasSensor;
        private int _ordinal_MaxBaysEnabled;
        private int _ordinal_MeterGroup;
        private int _ordinal_MeterType;
        private int _ordinal_LastCommunication;
        private int _ordinal_LastCommunicationMessage;

        private void InitializeMapper(IDataReader reader)
        {
            PopulateOrdinals(reader);
            _isInitialized = true;
        }

        public void PopulateOrdinals(IDataReader reader)
        {
            _ordinal_MeterId = reader.GetOrdinal("MeterId");
            _ordinal_BayNumber = reader.GetOrdinal("BayNumber");
            _ordinal_AreaId = reader.GetOrdinal("AreaId");
            _ordinal_MeterName = reader.GetOrdinal("MeterName");
            _ordinal_HasSensor = reader.GetOrdinal("HasSensor");
            _ordinal_MaxBaysEnabled = reader.GetOrdinal("MaxBaysEnabled");
            _ordinal_MeterGroup = reader.GetOrdinal("MeterGroup");
            _ordinal_MeterType = reader.GetOrdinal("MeterType");
            _ordinal_LastCommunication = reader.GetOrdinal("LastCommunication");
            _ordinal_LastCommunicationMessage = reader.GetOrdinal("LastCommunicationMessage");
        }

        public Object GetData(IDataReader reader)
        {
            // This is where we define the mapping between the object properties and the 
            // data columns. The convention that should be used is that the object property 
            // names are exactly the same as the column names. However if there is some 
            // compelling reason for the names to be different, the mapping can be defined here.

            // We assume the reader has data and is already on the row that contains the data 
            // we need. We don't need to call read. As a general rule, assume that every field must 
            // be null  checked. If a field is null then the nullvalue for that field has already 
            // been set by the DTO constructor, we don't have to change it.

            if (!_isInitialized) { InitializeMapper(reader); }

            // Now we can load the data into the DTO object from the DB reader
            SensorHeartbeatRecord dto = new SensorHeartbeatRecord();

            if (!reader.IsDBNull(_ordinal_MeterId)) { dto.MeterID = Convert.ToInt32(reader[_ordinal_MeterId]); }
            if (!reader.IsDBNull(_ordinal_BayNumber)) { dto.BayID = Convert.ToInt32(reader[_ordinal_BayNumber]); }
            if (!reader.IsDBNull(_ordinal_AreaId)) { dto.AreaID = Convert.ToInt32(reader[_ordinal_AreaId]); }
            if (!reader.IsDBNull(_ordinal_MeterName)) { dto.MeterName = Convert.ToString(reader[_ordinal_MeterName]); }
            if (!reader.IsDBNull(_ordinal_HasSensor)) { dto.HasSensor = Convert.ToBoolean(reader[_ordinal_HasSensor]); }
            if (!reader.IsDBNull(_ordinal_MaxBaysEnabled)) { dto.MaxBaysEnabled = Convert.ToInt32(reader[_ordinal_MaxBaysEnabled]); }
            if (!reader.IsDBNull(_ordinal_MeterGroup)) { dto.MeterGroup = Convert.ToInt32(reader[_ordinal_MeterGroup]); }
            if (!reader.IsDBNull(_ordinal_MeterType)) { dto.MeterType = Convert.ToInt32(reader[_ordinal_MeterType]); }
            if (!reader.IsDBNull(_ordinal_LastCommunication)) { dto.LastCommunication = Convert.ToDateTime(reader[_ordinal_LastCommunication]); }
            if (!reader.IsDBNull(_ordinal_LastCommunicationMessage)) { dto.LastCommunicationType = Convert.ToString(reader[_ordinal_LastCommunicationMessage]); }
            return dto;
        }
    }
    #endregion

    #region SensorBatteryDiagnosticsDataMapper
    public class SensorBatteryDiagnosticsDataMapper : IDataMapper
    {
        private bool _isInitialized = false;
        private int _ordinal_MeterId;
        private int _ordinal_BayNumber;
        private int _ordinal_AreaId;
        private int _ordinal_MeterName;
        private int _ordinal_HasSensor;
        private int _ordinal_MaxBaysEnabled;
        private int _ordinal_MeterGroup;
        private int _ordinal_MeterType;
        private int _ordinal_DryBattCurrV;
        private int _ordinal_RechargeBattCurrV;
        private int _ordinal_DryBattMinV;
        private int _ordinal_RechargeBattMinV;
        private int _ordinal_TimestampOfLatestBatteryVoltageReport;

        private int _ordinal_DiagnosticType = -1; // Special case because this is an "optional" field
        private int _ordinal_DiagnosticValue = -1; // Special case because this is an "optional" field

        private void InitializeMapper(IDataReader reader)
        {
            PopulateOrdinals(reader);
            _isInitialized = true;
        }

        private bool ColumnExists(IDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (string.Compare(reader.GetName(i), columnName, true) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        public void PopulateOrdinals(IDataReader reader)
        {
            _ordinal_MeterId = reader.GetOrdinal("MeterId");
            _ordinal_BayNumber = reader.GetOrdinal("BayNumber");
            _ordinal_AreaId = reader.GetOrdinal("AreaId");
            _ordinal_MeterName = reader.GetOrdinal("MeterName");
            _ordinal_HasSensor = reader.GetOrdinal("HasSensor");
            _ordinal_MaxBaysEnabled = reader.GetOrdinal("MaxBaysEnabled");
            _ordinal_MeterGroup = reader.GetOrdinal("MeterGroup");
            _ordinal_MeterType = reader.GetOrdinal("MeterType");
            _ordinal_DryBattCurrV = reader.GetOrdinal("DryBattCurrV");
            _ordinal_RechargeBattCurrV = reader.GetOrdinal("RechargeBattCurrV");
            _ordinal_DryBattMinV = reader.GetOrdinal("DryBattMinV");
            _ordinal_RechargeBattMinV = reader.GetOrdinal("RechargeBattMinV");
            _ordinal_TimestampOfLatestBatteryVoltageReport = reader.GetOrdinal("TimestampOfLatestBatteryVoltageReport");

            // This is customized beyond the auto-generated sourcecode because these 
            // fields will be optional, depending on the actual SQL query that was executed
            if (ColumnExists(reader, "LastCommunication"))
                _ordinal_DiagnosticType = reader.GetOrdinal("DiagnosticType");
            if (ColumnExists(reader, "LastCommunicationMessage"))
                _ordinal_DiagnosticValue = reader.GetOrdinal("DiagnosticValue");
        }

        public Object GetData(IDataReader reader)
        {
            // This is where we define the mapping between the object properties and the 
            // data columns. The convention that should be used is that the object property 
            // names are exactly the same as the column names. However if there is some 
            // compelling reason for the names to be different, the mapping can be defined here.

            // We assume the reader has data and is already on the row that contains the data 
            // we need. We don't need to call read. As a general rule, assume that every field must 
            // be null  checked. If a field is null then the nullvalue for that field has already 
            // been set by the DTO constructor, we don't have to change it.

            if (!_isInitialized) { InitializeMapper(reader); }

            // Now we can load the data into the DTO object from the DB reader
            SensorBatteryDiagnostics dto = new SensorBatteryDiagnostics();

            if (!reader.IsDBNull(_ordinal_MeterId)) { dto.MeterID = Convert.ToInt32(reader[_ordinal_MeterId]); }
            if (!reader.IsDBNull(_ordinal_BayNumber)) { dto.BayID = Convert.ToInt32(reader[_ordinal_BayNumber]); }
            if (!reader.IsDBNull(_ordinal_AreaId)) { dto.AreaID = Convert.ToInt32(reader[_ordinal_AreaId]); }
            if (!reader.IsDBNull(_ordinal_MeterName)) { dto.MeterName = Convert.ToString(reader[_ordinal_MeterName]); }
            if (!reader.IsDBNull(_ordinal_HasSensor)) { dto.HasSensor = Convert.ToBoolean(reader[_ordinal_HasSensor]); }
            if (!reader.IsDBNull(_ordinal_MaxBaysEnabled)) { dto.MaxBaysEnabled = Convert.ToInt32(reader[_ordinal_MaxBaysEnabled]); }
            if (!reader.IsDBNull(_ordinal_MeterGroup)) { dto.MeterGroup = Convert.ToInt32(reader[_ordinal_MeterGroup]); }
            if (!reader.IsDBNull(_ordinal_MeterType)) { dto.MeterType = Convert.ToInt32(reader[_ordinal_MeterType]); }
            if (!reader.IsDBNull(_ordinal_TimestampOfLatestBatteryVoltageReport)) { dto.TimestampOfLatestBatteryVoltageReport = Convert.ToDateTime(reader[_ordinal_TimestampOfLatestBatteryVoltageReport]); }

            // Customized beyond the auto-generated sourcecode because the battery values will depend 
            // on the presence of optional columns, based on parameters used to build a dynamic SQL query
            if ((_ordinal_DiagnosticType == -1) && (_ordinal_DiagnosticValue == -1))
            {
                if (!reader.IsDBNull(_ordinal_DryBattCurrV)) { dto.DryBattCurrV = Convert.ToString(reader[_ordinal_DryBattCurrV]); }
                if (!reader.IsDBNull(_ordinal_RechargeBattCurrV)) { dto.RechargeBattCurrV = Convert.ToString(reader[_ordinal_RechargeBattCurrV]); }
                if (!reader.IsDBNull(_ordinal_DryBattMinV)) { dto.DryBattMinV = Convert.ToString(reader[_ordinal_DryBattMinV]); }
                if (!reader.IsDBNull(_ordinal_RechargeBattMinV)) { dto.RechargeBattMinV = Convert.ToString(reader[_ordinal_RechargeBattMinV]); }
            }
            else
            {
                int diagnosticType = -1;
                if (!reader.IsDBNull(_ordinal_DiagnosticType)) { diagnosticType = Convert.ToInt32(reader[_ordinal_DiagnosticType]); }

                if (!reader.IsDBNull(_ordinal_DiagnosticValue))
                {
                    // Diagnostic types:
                    //  9 = Dry battery minimum voltage
                    // 10 = Dry battery current voltage
                    // 11 = Rechargable battery minimum voltage
                    // 12 = Rechargable batter current voltage
                    if (diagnosticType == 9)
                        dto.DryBattMinV = Convert.ToString(reader[_ordinal_DiagnosticValue]);
                    else if (diagnosticType == 10)
                        dto.DryBattCurrV = Convert.ToString(reader[_ordinal_DiagnosticValue]);
                    else if (diagnosticType == 11)
                        dto.RechargeBattMinV = Convert.ToString(reader[_ordinal_DiagnosticValue]);
                    else if (diagnosticType == 12)
                        dto.RechargeBattCurrV = Convert.ToString(reader[_ordinal_DiagnosticValue]);
                }
            }

            return dto;
        }
    }
    #endregion

}