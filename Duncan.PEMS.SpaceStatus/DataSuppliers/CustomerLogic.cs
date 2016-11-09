using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Xml;

using Duncan.PEMS.SpaceStatus.DataShapes;
using Duncan.PEMS.SpaceStatus.DataSuppliers;
using Duncan.PEMS.SpaceStatus.Models;
using Duncan.PEMS.SpaceStatus.UtilityClasses;
using Duncan.PEMS.SpaceStatus.DataMappers;
//using Duncan.PEMS.Framework.Controller;

namespace Duncan.PEMS.SpaceStatus.DataSuppliers
{
    //-----------------------------------------------------------------------------------------------------------------//
    //            ================================
    //            | Important General Information |
    //            ================================
    //
    // CustomerConfig class is a DTO/Model class that basically only holds basic information about a customer that is safe
    // to share with other layers -- it doesn't expose things like DB connection info or inventory objects
    //
    // CustomerLogic class is the main container of everything related to a customer.  It houses members and child classes that
    // identifiy the customer, configuration options, DB connection info, and holds an inventory of assets related to the customer.
    // The CustomerLogic class may appear to be laid out in a strange manner -- there are multiple "partial class CustomerLogic" 
    // definitions in the same file.  Note however that this is done intentionally to aid in the readability and seperation of
    // CustomerLogic and its nested CustomerManager class.
    //
    // CustomerManager class is a Singleton class that manages access to customer information. It is defined as a nested class of
    // CustomerLogic so it can access private members of CustomerLogic.  Due to this design, it is only visible via CustomerLogic,
    // so it must be referred to as "CustomerLogic.CustomerManager"
    //-----------------------------------------------------------------------------------------------------------------//


    #region CustomerLogic and CustomerManager shell definition
    // Partial class definitions to show nested relationship between CustomerLogic and CustomerManager.
    // The implementation of each class will be inside seperate definitions of "partial class CustomerLogic" for easier readability.
    public partial class CustomerLogic
    {
        public sealed partial class CustomerManager
        {
        }
    }
    #endregion

    #region CustomerLogic implementation
    public partial class CustomerLogic
    {
        private object _dtoLocker = new object();

        public int CustomerId = -1;

        private CustomerConfig _CustomerDTO = new CustomerConfig();

        public CustomerConfig CustomerDTO
        {
            get
            {
                // In a thread-safe manner, build and return a copy of our internal object
                lock (_dtoLocker)
                {
                    CustomerConfig clonedDto = this._CustomerDTO.ShallowCopy();
                    return clonedDto;
                }
            }

            set
            {
                // In a thread-safe manner, build a copy of the passed object, then assign to our internal object
                lock (_dtoLocker)
                {
                    CustomerConfig clonedDto = value.ShallowCopy();
                    this._CustomerDTO = clonedDto;
                }
            }
        }

        // For thread-safety, use _InventoryLocker when accessing collections in Inventory. We are using ReaderWriterLockSlim since most accesses will be read-only and can be concurrent
        private CustomerAssets _Inventory = new CustomerAssets();
        private ReaderWriterLockSlim _InventoryLocker = new ReaderWriterLockSlim();

        private SqlServerDAO _sharedSqlDao = null;

        public SqlServerDAO SharedSqlDao
        {
            get { return _sharedSqlDao; }
            set { _sharedSqlDao = value; }
        }

        /*
        public string PaymentXMLSource;  // For obtaining info from PAM on HTTP
        public string PaymentXMLSourceExpiry;  // For obtaining info from PAM on HTTP
        public string PAMServerTCPAddress;  // For obtaining info from PAM via TCP/UDP
        public string PAMServerTCPPort;  // For obtaining info from PAM via TCP/UDP
        */

        private AreaAsset GetAreaAsset(int areaID)
        {
            try
            {
                // Use a read lock for this operation
                _InventoryLocker.EnterReadLock();

                AreaAsset result = null;
                foreach (AreaAsset asset in this._Inventory.Areas)
                {
                    if (asset.AreaID == areaID)
                    {
                        result = asset;
                        break;
                    }
                }
                return result;
            }
            finally
            {
                // Release the read lock           
                if (_InventoryLocker.IsReadLockHeld)
                    _InventoryLocker.ExitReadLock();
            }
        }

        private List<AreaAsset> GetAreaAssets()
        {
            try
            {
                // Use a read lock for this operation
                _InventoryLocker.EnterReadLock();

                List<AreaAsset> result = new List<AreaAsset>();
                result.AddRange(_Inventory.Areas);
                return result;
            }
            finally
            {
                // Release the read lock           
                if (_InventoryLocker.IsReadLockHeld)
                    _InventoryLocker.ExitReadLock();
            }
        }

        private PAMClusterAsset GetClusterAsset(int clusterID)
        {
            try
            {
                // Use a read lock for this operation
                _InventoryLocker.EnterReadLock();

                PAMClusterAsset result = null;
                foreach (PAMClusterAsset asset in this._Inventory.PAMClusters)
                {
                    if (asset.ClusterID == clusterID)
                    {
                        result = asset;
                        break;
                    }
                }
                return result;
            }
            finally
            {
                // Release the read lock           
                if (_InventoryLocker.IsReadLockHeld)
                    _InventoryLocker.ExitReadLock();
            }
        }

        private List<PAMClusterAsset> GetClusterAssets()
        {
            try
            {
                // Use a read lock for this operation
                _InventoryLocker.EnterReadLock();

                List<PAMClusterAsset> result = new List<PAMClusterAsset>();
                result.AddRange(_Inventory.PAMClusters);
                return result;
            }
            finally
            {
                // Release the read lock           
                if (_InventoryLocker.IsReadLockHeld)
                    _InventoryLocker.ExitReadLock();
            }
        }

        private MeterAsset GetMeterAsset(int meterID)
        {
            try
            {
                // Use a read lock for this operation
                _InventoryLocker.EnterReadLock();

                MeterAsset result = null;
                foreach (MeterAsset asset in this._Inventory.Meters)
                {
                    if (asset.MeterID == meterID)
                    {
                        result = asset;
                        break;
                    }
                }
                return result;
            }
            finally
            {
                // Release the read lock           
                if (_InventoryLocker.IsReadLockHeld)
                    _InventoryLocker.ExitReadLock();
            }
        }


        private String GetMeterAssetName(int meterID)
        {
            try
            {
                // Use a read lock for this operation
                _InventoryLocker.EnterReadLock();

                String result = "";
                foreach (MeterAsset asset in this._Inventory.Meters)
                {
                    if (asset.MeterID == meterID)
                    {
                        if (asset.MeterName != "" || asset.MeterName != null)
                            result = asset.MeterName;
                        else
                            result = meterID.ToString();
                        break;
                    }
                }
                return result;
            }
            finally
            {
                // Release the read lock           
                if (_InventoryLocker.IsReadLockHeld)
                    _InventoryLocker.ExitReadLock();
            }
        }

        private List<MeterAsset> GetMeterAssets()
        {
            try
            {
                // Use a read lock for this operation
                _InventoryLocker.EnterReadLock();

                List<MeterAsset> result = new List<MeterAsset>();
                result.AddRange(_Inventory.Meters);
                return result;
            }
            finally
            {
                // Release the read lock           
                if (_InventoryLocker.IsReadLockHeld)
                    _InventoryLocker.ExitReadLock();
            }
        }

        private SpaceAsset GetSpaceAsset(int meterID, int spaceID)
        {
            try
            {
                // Use a read lock for this operation
                _InventoryLocker.EnterReadLock();

                SpaceAsset result = null;
                foreach (SpaceAsset asset in this._Inventory.Spaces)
                {
                    if ((asset.MeterID == meterID) && (asset.SpaceID == spaceID))
                    {
                        result = asset;
                        break;
                    }
                }
                return result;
            }
            finally
            {
                // Release the read lock           
                if (_InventoryLocker.IsReadLockHeld)
                    _InventoryLocker.ExitReadLock();
            }
        }

        private List<SpaceAsset> GetSpaceAssetsForMeter(int meterID)
        {
            try
            {
                // Use a read lock for this operation
                _InventoryLocker.EnterReadLock();

                List<SpaceAsset> result = new List<SpaceAsset>();
                foreach (SpaceAsset asset in this._Inventory.Spaces)
                {
                    if (asset.MeterID == meterID)
                        result.Add(asset);
                }
                return result;
            }
            finally
            {
                // Release the read lock           
                if (_InventoryLocker.IsReadLockHeld)
                    _InventoryLocker.ExitReadLock();
            }
        }

        private List<SpaceAsset> GetSpaceAssets()
        {
            try
            {
                // Use a read lock for this operation
                _InventoryLocker.EnterReadLock();

                List<SpaceAsset> result = new List<SpaceAsset>();
                result.AddRange(_Inventory.Spaces);
                return result;
            }
            finally
            {
                // Release the read lock           
                if (_InventoryLocker.IsReadLockHeld)
                    _InventoryLocker.ExitReadLock();
            }
        }

        private CustomGroup1Asset GetCustomGroup1Asset(int CustomGroupId)
        {
            try
            {
                // Use a read lock for this operation
                _InventoryLocker.EnterReadLock();

                CustomGroup1Asset result = null;
                foreach (CustomGroup1Asset asset in this._Inventory.CustomGroup1s)
                {
                    if (asset.CustomGroupId == CustomGroupId)
                    {
                        result = asset;
                        break;
                    }
                }
                return result;
            }
            finally
            {
                // Release the read lock           
                if (_InventoryLocker.IsReadLockHeld)
                    _InventoryLocker.ExitReadLock();
            }
        }

        private List<CustomGroup1Asset> GetCustomGroup1Assets()
        {
            try
            {
                // Use a read lock for this operation
                _InventoryLocker.EnterReadLock();

                List<CustomGroup1Asset> result = new List<CustomGroup1Asset>();
                result.AddRange(_Inventory.CustomGroup1s);
                return result;
            }
            finally
            {
                // Release the read lock           
                if (_InventoryLocker.IsReadLockHeld)
                    _InventoryLocker.ExitReadLock();
            }
        }

        public void GatherAllInventory()
        {
            // Let's launch this work in a background thread
            Thread task = new Thread(GatherAllInventory_ThreadStart);
            task.Start();
            //GatherAllInventory_ThreadStart();
        }

        private void GatherAllInventory_ThreadStart()
        {
            // Use our specialized StopWatchProfiler class that makes it easier to
            // profile and log the time spend doing these activities
            using (StopWatchProfiler swProfile = new StopWatchProfiler(string.Empty/*"Queried DB for Space assets"*/, Logging.LogLevel.Debug,
                MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name, System.Threading.Thread.CurrentThread.ManagedThreadId))
            {
                GatherSpaceAssets();

                swProfile.Restart(string.Empty/*"Queried DB for Meter assets"*/);
                GatherMeterAssets();

                swProfile.Restart(string.Empty/*"Queried DB for PAM Cluster assets"*/);
                GatherClusterAssets();

                swProfile.Restart(string.Empty/*"Queried DB for Area assets"*/);
                GatherAreaAssets();

                swProfile.Restart(string.Empty/*"Queried DB for CustomGroup1 assets"*/);
                GatherCustomGroup1Assets();
            }
        }

        private void GatherSpaceAssets()
        {
            try
            {
                // Don't even attempt this if the customer id is invalid
                if (this.CustomerId == -1)
                    return;

                int loadedAssetCount = 0;
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                // Use a DTO mapper to deserialize from database to object list
                StringBuilder sql = new StringBuilder();

                sql.Append(
                    "select distinct ps.ParkingSpaceId, ps.BayNumber, ps.AreaID, mm.AreaId2 as LibertyArea, ps.MeterID," +
                    " cl.ClusterId, ps.HasSensor, psd.SpaceType, ps.Latitude, ps.Longitude, mm.CollRouteId," +
                    " mm.EnfRouteId, mm.MaintRouteId, mm.CustomGroup1, mm.CustomGroup2, mm.CustomGroup3, hm.IsActive, m.MeterName" +
                    " from ParkingSpaces as ps" +
                    " left join Meters as m on ps.meterid = m.meterid and ps.customerid = m.customerid and ps.areaid = m.areaid" +
                    " left join PAMClusters as cl on m.meterid = cl.meterid and m.customerid = cl.customerid" +
                    " left join MeterMap as mm on m.meterid = mm.meterid and m.customerid = mm.customerid and m.areaid = mm.areaid" +
                    " left join ParkingSpaceDetails psd on ps.ParkingSpaceID = psd.ParkingSpaceID" +
                    " left join HousingMaster hm on hm.customerid = m.customerid and hm.housingname = m.metername" +
                    " where m.customerid = @CustomerId" +
                   
                    " order by ps.baynumber");

                using (SqlCommand command = _sharedSqlDao.GetSqlCommand(sql.ToString()))
                {
                    command.Parameters.Add(_sharedSqlDao.CreateParameter("@CustomerId", this.CustomerId));

                    try
                    {
                        // Use an exclusive write lock for this operation
                        _InventoryLocker.EnterWriteLock();

                        this._Inventory.Spaces = _sharedSqlDao.GetList<SpaceAsset>(command);
                        loadedAssetCount = this._Inventory.Spaces.Count;

                        // After loading the assets from the database, we need to merge with info from our local list of "Inactive Spaces"
                        //List<InactiveSpace> allInactiveSpaces = InactiveSpacesRepository.Repository.GetAll();
                        //InactiveSpacePredicate inactivePredicate = null;
                        //foreach (SpaceAsset nextAsset in this._Inventory.Spaces)
                        //{
                        //    // Assume the space is active
                        //    nextAsset.IsActive = true;

                        //    // Search for a matching inactive space
                        //    if ((allInactiveSpaces != null) && (allInactiveSpaces.Count > 0))
                        //    {
                        //        inactivePredicate = new InactiveSpacePredicate(this.CustomerId, nextAsset.MeterID, nextAsset.SpaceID);
                        //        InactiveSpace inactiveItem = allInactiveSpaces.Find(inactivePredicate.Compare);
                        //        // If inactive space is found, then mark the space asset as inactive
                        //        if (inactiveItem != null)
                        //        {
                        //            nextAsset.IsActive = false;

                        //            // We might as well log the fact that we loaded a space that is marked as "inactive".  This might be useful info to have available when 
                        //            // researching an issue in the future...
                        //            Duncan.PEMS.SpaceStatus.UtilityClasses.Logging.AddTextToGenericLog(UtilityClasses.Logging.LogLevel.Debug,
                        //                string.Format("Space is flagged as inactive: ParkingSpaceID={0}, CID={1}, MID={2}, BAY={3} (Based on InactiveSpaces.xml)", 
                        //                nextAsset.ParkingSpaceId_Internal, this.CustomerId, nextAsset.MeterID, nextAsset.SpaceID),
                        //                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name,
                        //                System.Threading.Thread.CurrentThread.ManagedThreadId);
                        //        }
                        //    }
                        //}
                    }
                    finally
                    {
                        // Release the exclusive write lock           
                        if (_InventoryLocker.IsWriteLockHeld)
                            _InventoryLocker.ExitWriteLock();
                    }
                }

                sw.Stop();

                // Log this information
                Logging.AddTextToGenericLog(Logging.LogLevel.Info, "    Loaded " + loadedAssetCount.ToString() +
                    " space assets for customer: " + this.CustomerId.ToString() + " (" + this.CustomerDTO.CustomerName + ")" +
                    "; Elapsed: " + sw.Elapsed.ToString(),
                     MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name, System.Threading.Thread.CurrentThread.ManagedThreadId);
            }
            catch (Exception ex)
            {
                Logging.AddTextToGenericLog(Logging.LogLevel.Error, ex.ToString(),
                    MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name, System.Threading.Thread.CurrentThread.ManagedThreadId);
            }
        }

        private void GatherMeterAssets()
        {
            try
            {
                // Don't even attempt this if the customer id is invalid
                if (this.CustomerId == -1)
                    return;

                int loadedAssetCount = 0;
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                // Use a DTO mapper to deserialize from database to object list
                StringBuilder sql = new StringBuilder();

                // DEBUG: We should get the HousingName from HousingMaster, which is essentially the "Location ID" that customers know
                // DEBUG: A meter asset should have a "display name" that is either the HousingName, MeterName, or MeterID -- whichever is available?
                /*
select mm.meterid, mm.housingid, mm.customgroup1, hm.housingname from metermap mm
left join housingmaster hm on mm.housingid = hm.housingid
where mm.customerid = 4140 and mm.customgroup1 is not null;

                */

                sql.Append(
                    "select distinct m.MeterId, m.MeterName, m.Description, m.MeterType, " +
                    " m.AreaId, m.MeterGroup, mg.MeterGroupDesc," +
                    " cl.clusterid, mm.AreaId2 as LibertyArea, " +
                    " m.Latitude, m.Longitude," +
                    " mm.CollRouteId, mm.EnfRouteId, mm.MaintRouteId, " +
                    " mm.CustomGroup1, mm.CustomGroup2, mm.CustomGroup3" +
                    " from meters as m" +
                    " left join MeterGroup as mg on m.metergroup = mg.metergroupid" +
                    " left join PAMClusters as cl on m.meterid = cl.meterid and m.customerid = cl.customerid" +
                    " left join MeterMap as mm on m.meterid = mm.meterid and m.customerid = mm.customerid and m.areaid = mm.areaid" +
                    " where m.customerid = @CustomerId and m.GlobalMeterId > 1 and mg.MeterGroupId=0 " +
                    " order by m.MeterName");
                using (SqlCommand command = _sharedSqlDao.GetSqlCommand(sql.ToString()))
                {
                    command.Parameters.Add(_sharedSqlDao.CreateParameter("@CustomerId", this.CustomerId));

                    try
                    {
                        // Use an exclusive write lock for this operation
                        _InventoryLocker.EnterWriteLock();

                        this._Inventory.Meters = _sharedSqlDao.GetList<MeterAsset>(command);
                        loadedAssetCount = this._Inventory.Meters.Count;
                    }
                    finally
                    {
                        // Release the exclusive write lock           
                        if (_InventoryLocker.IsWriteLockHeld)
                            _InventoryLocker.ExitWriteLock();
                    }
                }

                sw.Stop();

                // Log this information
                Logging.AddTextToGenericLog(Logging.LogLevel.Info, "    Loaded " + loadedAssetCount.ToString() +
                    " meter assets for customer: " + this.CustomerId.ToString() + " (" + this.CustomerDTO.CustomerName + ")" +
                    "; Elapsed: " + sw.Elapsed.ToString(),
                     MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name, System.Threading.Thread.CurrentThread.ManagedThreadId);
            }
            catch (Exception ex)
            {
                Logging.AddTextToGenericLog(Logging.LogLevel.Error, ex.ToString(),
                    MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name, System.Threading.Thread.CurrentThread.ManagedThreadId);
            }

        }

        private void GatherClusterAssets()
        {
            try
            {
                // Don't even attempt this if the customer id is invalid
                if (this.CustomerId == -1)
                    return;

                int loadedAssetCount = 0;
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                // Use a DTO mapper to deserialize from database to object list
                StringBuilder sql = new StringBuilder();
                sql.Append(
                    "select distinct cl.ClusterID, cl.Description," +
                    " cl.HostedBayStart, cl.HostedBayEnd" +
                    " from PAMClusters cl" +
                    " where cl.customerid = @CustomerId" +
                    " order by cl.ClusterID");
                using (SqlCommand command = _sharedSqlDao.GetSqlCommand(sql.ToString()))
                {
                    command.Parameters.Add(_sharedSqlDao.CreateParameter("@CustomerId", this.CustomerId));

                    try
                    {
                        // Use an exclusive write lock for this operation
                        _InventoryLocker.EnterWriteLock();

                        this._Inventory.PAMClusters = _sharedSqlDao.GetList<PAMClusterAsset>(command);
                        loadedAssetCount = this._Inventory.PAMClusters.Count;
                    }
                    finally
                    {
                        // Release the exclusive write lock           
                        if (_InventoryLocker.IsWriteLockHeld)
                            _InventoryLocker.ExitWriteLock();
                    }
                }

                sw.Stop();

                // Log this information
                Logging.AddTextToGenericLog(Logging.LogLevel.Info, "    Loaded " + loadedAssetCount.ToString() +
                    " cluster assets for customer: " + this.CustomerId.ToString() + " (" + this.CustomerDTO.CustomerName + ")" +
                    "; Elapsed: " + sw.Elapsed.ToString(),
                     MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name, System.Threading.Thread.CurrentThread.ManagedThreadId);
            }
            catch (Exception ex)
            {
                Logging.AddTextToGenericLog(Logging.LogLevel.Error, ex.ToString(),
                    MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name, System.Threading.Thread.CurrentThread.ManagedThreadId);
            }
        }

        private void GatherAreaAssets()
        {
            try
            {
                // Don't even attempt this if the customer id is invalid
                if (this.CustomerId == -1)
                    return;

                int loadedAssetCount = 0;
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                // Use a DTO mapper to deserialize from database to object list
                StringBuilder sql = new StringBuilder();

                sql.Append(
                    "select distinct a.AreaID, a.AreaName, a.Description" +
                    " from Areas a" +
                    " where a.CustomerID = @CustomerId" +
                    " and a.AreaId not in (1,97,98,99)" +
                    " order by a.AreaID");
                using (SqlCommand command = _sharedSqlDao.GetSqlCommand(sql.ToString()))
                {
                    command.Parameters.Add(_sharedSqlDao.CreateParameter("@CustomerId", this.CustomerId));

                    try
                    {
                        // Use an exclusive write lock for this operation
                        _InventoryLocker.EnterWriteLock();

                        this._Inventory.Areas = _sharedSqlDao.GetList<AreaAsset>(command);
                        loadedAssetCount = this._Inventory.Areas.Count;
                    }
                    finally
                    {
                        // Release the exclusive write lock           
                        if (_InventoryLocker.IsWriteLockHeld)
                            _InventoryLocker.ExitWriteLock();
                    }
                }

                sw.Stop();

                // Log this information
                Logging.AddTextToGenericLog(Logging.LogLevel.Info, "    Loaded " + loadedAssetCount.ToString() +
                    " area assets for customer: " + this.CustomerId.ToString() + " (" + this.CustomerDTO.CustomerName + ")" +
                    "; Elapsed: " + sw.Elapsed.ToString(),
                     MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name, System.Threading.Thread.CurrentThread.ManagedThreadId);
            }
            catch (Exception ex)
            {
                Logging.AddTextToGenericLog(Logging.LogLevel.Error, ex.ToString(),
                    MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name, System.Threading.Thread.CurrentThread.ManagedThreadId);
            }
        }

        private void GatherCustomGroup1Assets()
        {
            try
            {
                // Don't even attempt this if the customer id is invalid
                if (this.CustomerId == -1)
                    return;

                int loadedAssetCount = 0;
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                // Use a DTO mapper to deserialize from database to object list
                StringBuilder sql = new StringBuilder();
                sql.Append(
                    "select distinct cg1.CustomGroupId, cg1.DisplayName, cg1.Comment" +
                    " from CustomGroup1 cg1" +
                    " where cg1.CustomerID = @CustomerId" +
                    " order by cg1.CustomGroupId");
                using (SqlCommand command = _sharedSqlDao.GetSqlCommand(sql.ToString()))
                {
                    command.Parameters.Add(_sharedSqlDao.CreateParameter("@CustomerId", this.CustomerId));

                    try
                    {
                        // Use an exclusive write lock for this operation
                        _InventoryLocker.EnterWriteLock();

                        this._Inventory.CustomGroup1s = _sharedSqlDao.GetList<CustomGroup1Asset>(command);
                        loadedAssetCount = this._Inventory.CustomGroup1s.Count;
                    }
                    finally
                    {
                        // Release the exclusive write lock           
                        if (_InventoryLocker.IsWriteLockHeld)
                            _InventoryLocker.ExitWriteLock();
                    }
                }

                sw.Stop();

                // Log this information
                Logging.AddTextToGenericLog(Logging.LogLevel.Info, "    Loaded " + loadedAssetCount.ToString() +
                    " CustomGroup1 assets for customer: " + this.CustomerId.ToString() + " (" + this.CustomerDTO.CustomerName + ")" +
                    "; Elapsed: " + sw.Elapsed.ToString(),
                     MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name, System.Threading.Thread.CurrentThread.ManagedThreadId);
            }
            catch (Exception ex)
            {
                Logging.AddTextToGenericLog(Logging.LogLevel.Error, ex.ToString(),
                    MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name, System.Threading.Thread.CurrentThread.ManagedThreadId);
            }
        }

        public void LogMessage(System.Diagnostics.TraceLevel logLevel, string textToLog, string classAndMethod, int threadID)
        {
            Logging.LogLevel convertedLogLevel = Logging.LogLevel.Debug;
            switch (logLevel)
            {
                case System.Diagnostics.TraceLevel.Error:
                    convertedLogLevel = Logging.LogLevel.Error;
                    break;
                case System.Diagnostics.TraceLevel.Info:
                    convertedLogLevel = Logging.LogLevel.Info;
                    break;
                case System.Diagnostics.TraceLevel.Verbose:
                    convertedLogLevel = Logging.LogLevel.Debug;
                    break;
                case System.Diagnostics.TraceLevel.Warning:
                    convertedLogLevel = Logging.LogLevel.Warning;
                    break;
                case System.Diagnostics.TraceLevel.Off:
                    convertedLogLevel = Logging.LogLevel.DebugTraceOutput;
                    break;
                default:
                    convertedLogLevel = Logging.LogLevel.Debug;
                    break;
            }

            Logging.AddTextToGenericLog(convertedLogLevel, textToLog, classAndMethod, threadID);
        }

         public void InsertOverstayVioAction(int CustomerID, int MeterID, int AreaID, int BayNumber, int RBACUserID,
             DateTime EventTimestamp, string ActionTaken)
        {
          

            // Set the SQL query parameters
            Dictionary<string, object> sqlParams = new Dictionary<string, object>();
            sqlParams.Add("@CustomerID", CustomerID);
            sqlParams.Add("@MeterID", MeterID);
            sqlParams.Add("@AreaID", AreaID);
            sqlParams.Add("@BayNumber", BayNumber);
            sqlParams.Add("@RBACUserID", RBACUserID);
            sqlParams.Add("@EventTimestamp", EventTimestamp);
            sqlParams.Add("@ActionTaken", ActionTaken);

            // Perform DB Insert
          //  DataSuppliers.SqLiteDAO sqLiteDataLayer = OverstayVioActionDBManager.Instance._DAO;
            Int64 autoIncValue = _sharedSqlDao.ExecuteInsertAndReturnAutoInc(
                "INSERT INTO [OverstayVioActions]" +
                " (CustomerID, MeterID, AreaID, BayNumber, RBACUserID, EventTimestamp, ActionTaken)" +
                " VALUES (@CustomerID, @MeterID, @AreaID, @BayNumber, @RBACUserID, @EventTimestamp, @ActionTaken" +
                " )", sqlParams, true);
        }


         public OverstayVioActionsDTO GetLatestVioActionForSpace(int customerID, int meterID, int areaID, int bayNumber)
         {

             Dictionary<string, object> sqlParams = new Dictionary<string, object>();
             sqlParams.Add("@CustomerID", customerID);
             sqlParams.Add("@MeterID", meterID);
             //sqlParams.Add("AreaID", areaID);
             sqlParams.Add("@BayNumber", bayNumber);

             string query =
                 "SELECT Top 1 UniqueKey, CustomerID, AreaID, MeterID, BayNumber, RBACUserID, EventTimestamp, ActionTaken" +
                 " FROM OverstayVioActions" +
                 " where CustomerID = @CustomerID" +
                 " and MeterID = @MeterID" +
                 " and BayNumber = @BayNumber" +
                 " order by EventTimestamp desc";
             return _sharedSqlDao.GetSingle<OverstayVioActionsDTO>(query, sqlParams, true);
         }


         public OverstayVioActionsDTO GetVioActionForSpaceDuringTimeRange(int customerID, int meterID, int areaID, int bayNumber, DateTime startTime, DateTime endTime)
         {

             Dictionary<string, object> sqlParams = new Dictionary<string, object>();
             sqlParams.Add("@CustomerID", customerID);
             sqlParams.Add("@MeterID", meterID);
             //sqlParams.Add("AreaID", areaID);
             sqlParams.Add("@BayNumber", bayNumber);
             sqlParams.Add("@StartTime", startTime);
             sqlParams.Add("@EndTime", endTime);

             string query =
                 "SELECT Top 1 UniqueKey, CustomerID, AreaID, MeterID, BayNumber, RBACUserID, EventTimestamp, ActionTaken" +
                 " FROM OverstayVioActions" +
                 " where CustomerID = @CustomerID" +
                 " and MeterID = @MeterID" +
                 " and BayNumber = @BayNumber" +
                 " and EventTimestamp >= @StartTime" +
                 " and EventTimestamp <= @EndTime" +
                 " order by EventTimestamp desc";

             return _sharedSqlDao.GetSingle<OverstayVioActionsDTO>(query, sqlParams, true);
         }



    }
    #endregion

    #region CustomerManager implementation (nested inside CustomerLogic)
    public partial class CustomerLogic 
    {
        public sealed partial class CustomerManager
        {
            #region Singleton Pattern members and contructor
            private static readonly CustomerManager _SingletonInstance = new CustomerManager();

            // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
            static CustomerManager() { }

            private CustomerManager()
            {
                // Get info about timezones, then keep a reference to the one that applies to this machine
                TimeZones = Duncan.PEMS.SpaceStatus.UtilityClasses.TimeZoneInfo.GetTimeZonesFromRegistry();

                // Look through each timezone and find ours
                // Note: We can't call CustomerConfigManager.FindTimeZoneByStandardName() yet because the singleton 
                // intance isn't assigned yet since we are still in the contructor
                string timeZoneName = System.TimeZone.CurrentTimeZone.StandardName;
                foreach (Duncan.PEMS.SpaceStatus.UtilityClasses.TimeZoneInfo zone in TimeZones)
                {
                    if (String.Equals(zone.StandardName, timeZoneName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        ServerTimeZone = zone;
                    }
                }
            }

            public static CustomerManager Instance
            {
                get { return _SingletonInstance; }
            }
            #endregion

            // Dictionary of Customers. Will be keyed by Customer ID (integer). 
            // For thread-safety, use _CustomersLocker when accessing _Customer. We are using ReaderWriterLockSlim since most accesses will be read-only and can be concurrent
            public Dictionary<int, CustomerLogic> _Customers = new Dictionary<int, CustomerLogic>();
            private ReaderWriterLockSlim _CustomersLocker = new ReaderWriterLockSlim();

            private string _RootPath = string.Empty;

            public Duncan.PEMS.SpaceStatus.UtilityClasses.TimeZoneInfo[] TimeZones = null;

            public Duncan.PEMS.SpaceStatus.UtilityClasses.TimeZoneInfo ServerTimeZone = null;

            //internal string _DBConnectStr_ReinoComm = string.Empty;
            //internal string PaymentXMLSource = string.Empty;  // For obtaining info from PAM on HTTP
            //internal string PAMServerTCPAddress = string.Empty;  // For obtaining info from PAM via TCP/UDP
            //internal string PAMServerTCPPort = string.Empty;  // For obtaining info from PAM via TCP/UDP

            public string _DBConnectStr_ReinoComm = string.Empty;
            public string PaymentXMLSource = string.Empty;  // For obtaining info from PAM on HTTP
            public string PAMServerTCPAddress = string.Empty;  // For obtaining info from PAM via TCP/UDP
            public string PAMServerTCPPort = string.Empty;  // For obtaining info from PAM via TCP/UDP

            //static public void LoadAllCustomers(int custmerID)
            //{
            //    try
            //    {
            //      //  AddNewCustomerToSite(9991);
            //    //    CustomerManager.Instance._RootPath = rootPath;

            //        Logging.AddTextToGenericLog(Logging.LogLevel.Info, "Loading Customers",
            //            MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name, System.Threading.Thread.CurrentThread.ManagedThreadId);

                    
            //        try
            //        {
            //            // Use an exclusive write lock since we are modifying our Customers collection
            //            CustomerManager.Instance._CustomersLocker.EnterWriteLock();

            //            CustomerManager.Instance._Customers = new Dictionary<int, CustomerLogic>();
            //            DataSet dsCustomerList = new DataSet();


            //            if (CustomerManager.Instance._Customers.ContainsKey(oneCustomer.CustomerId) == false)
            //                CustomerManager.Instance._Customers.Add(oneCustomer.CustomerId, oneCustomer);

            //        }
            //        finally
            //        {
            //            // Release the exclusive write lock           
            //            if (CustomerManager.Instance._CustomersLocker.IsWriteLockHeld)
            //                CustomerManager.Instance._CustomersLocker.ExitWriteLock();
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Logging.AddTextToGenericLog(Logging.LogLevel.Error, ex.ToString(),
            //            MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name, System.Threading.Thread.CurrentThread.ManagedThreadId);
            //    }
            //}



            static public CustomerLogic GetLogic(CustomerConfig customerCfg)
            {
                try
                {
                    // Use a read lock for this operation
                    CustomerManager.Instance._CustomersLocker.EnterReadLock();

                    CustomerLogic result = null;
                    CustomerManager.Instance._Customers.TryGetValue(customerCfg.CustomerId, out result);
                    return result;
                }
                finally
                {
                    // Release the read lock           
                    if (CustomerManager.Instance._CustomersLocker.IsReadLockHeld)
                        CustomerManager.Instance._CustomersLocker.ExitReadLock();
                }
            }

            static public CustomerLogic GetLogic(string customerName)
            {
                try
                {
                    // Use a read lock for this operation
                    CustomerManager.Instance._CustomersLocker.EnterReadLock();

                    foreach (var pair in CustomerManager.Instance._Customers)
                    {
                        if (string.Compare(pair.Value.CustomerDTO.CustomerName, customerName, true) == 0)
                            return pair.Value;
                    }
                    return null;
                }
                finally
                {
                    // Release the read lock           
                    if (CustomerManager.Instance._CustomersLocker.IsReadLockHeld)
                        CustomerManager.Instance._CustomersLocker.ExitReadLock();
                }
            }

            static public CustomerLogic GetLogicForCID(int customerId)
            {
                try
                {
                    // Use a read lock for this operation
                    CustomerManager.Instance._CustomersLocker.EnterReadLock();

                    CustomerLogic result = null;
                    CustomerManager.Instance._Customers.TryGetValue(customerId, out result);
                    return result;
                }
                finally
                {
                    // Release the read lock           
                    if (CustomerManager.Instance._CustomersLocker.IsReadLockHeld)
                        CustomerManager.Instance._CustomersLocker.ExitReadLock();
                }
            }

            static public CustomerLogic GetLogicForCID(string customerIdAsString)
            {
                try
                {
                    // Use a read lock for this operation
                    CustomerManager.Instance._CustomersLocker.EnterReadLock();

                    CustomerLogic result = null;
                    int cid = -1;
                    if (Int32.TryParse(customerIdAsString, out cid))
                    {
                        CustomerManager.Instance._Customers.TryGetValue(cid, out result);
                    }
                    return result;
                }
                finally
                {
                    // Release the read lock           
                    if (CustomerManager.Instance._CustomersLocker.IsReadLockHeld)
                        CustomerManager.Instance._CustomersLocker.ExitReadLock();
                }
            }

            static public string GetCustomerNameForCID(int customerId)
            {
                try
                {
                    // Use a read lock for this operation
                    CustomerManager.Instance._CustomersLocker.EnterReadLock();

                    CustomerLogic dto = null;
                    string result = string.Empty;
                    if (CustomerManager.Instance._Customers.TryGetValue(customerId, out dto))
                    {
                        if (dto != null)
                            result = dto.CustomerDTO.CustomerName;
                    }

                    return result;
                }
                finally
                {
                    // Release the read lock           
                    if (CustomerManager.Instance._CustomersLocker.IsReadLockHeld)
                        CustomerManager.Instance._CustomersLocker.ExitReadLock();
                }
            }


            static public CustomerConfig GetDTO(string customerName)
            {
                CustomerLogic customer = CustomerLogic.CustomerManager.GetLogic(customerName);
                if (customer == null)
                    return null;
                else
                    return customer.CustomerDTO;
            }

            static public CustomerConfig GetDTOForCID(int customerId)
            {
                CustomerLogic customer = CustomerLogic.CustomerManager.GetLogicForCID(customerId);
                if (customer == null)
                    return null;
                else
                    return customer.CustomerDTO;
            }

            static public CustomerConfig GetDTOForCID(string customerIdAsString)
            {
                CustomerLogic customer = CustomerLogic.CustomerManager.GetLogicForCID(customerIdAsString);
                if (customer == null)
                    return null;
                else
                    return customer.CustomerDTO;
            }

            static public List<CustomerConfig> GetAllCustomerDTOs()
            {
                try
                {
                    // Use a read lock for this operation
                    CustomerManager.Instance._CustomersLocker.EnterReadLock();

                    List<CustomerConfig> result = new List<CustomerConfig>();
                    foreach (var pair in CustomerManager.Instance._Customers)
                    {
                        result.Add(pair.Value.CustomerDTO);
                    }
                    return result;
                }
                finally
                {
                    // Release the read lock           
                    if (CustomerManager.Instance._CustomersLocker.IsReadLockHeld)
                        CustomerManager.Instance._CustomersLocker.ExitReadLock();
                }
            }



            static public AreaAsset GetAreaAsset(CustomerConfig customerDto, int areaID)
            {
                CustomerLogic customer = GetLogic(customerDto);
                return customer.GetAreaAsset(areaID);
            }

            static public List<AreaAsset> GetAreaAssetsForCustomer(CustomerConfig customerDto)
            {
                CustomerLogic customer = GetLogic(customerDto);
                return customer.GetAreaAssets();
            }

            static public PAMClusterAsset GetClusterAsset(CustomerConfig customerDto, int clusterID)
            {
                CustomerLogic customer = GetLogic(customerDto);
                return customer.GetClusterAsset(clusterID);
            }

            static public List<PAMClusterAsset> GetClusterAssetsForCustomer(CustomerConfig customerDto)
            {
                CustomerLogic customer = GetLogic(customerDto);
                return customer.GetClusterAssets();
            }

            static public MeterAsset GetMeterAsset(CustomerConfig customerDto, int meterID)
            {
                CustomerLogic customer = GetLogic(customerDto);
                return customer.GetMeterAsset(meterID);
            }

            static public string GetMeterAssetName(CustomerConfig customerDto, int meterID)
            {
                CustomerLogic customer = GetLogic(customerDto);
                return customer.GetMeterAssetName(meterID);
            }

            static public List<MeterAsset> GetMeterAssetsForCustomer(CustomerConfig customerDto)
            {
                CustomerLogic customer = GetLogic(customerDto);
                return customer.GetMeterAssets();
            }

            static public SpaceAsset GetSpaceAsset(CustomerConfig customerDto, int meterID, int spaceID)
            {
                CustomerLogic customer = GetLogic(customerDto);
                return customer.GetSpaceAsset(meterID, spaceID);
            }

            static public List<SpaceAsset> GetSpaceAssetsForCustomer(CustomerConfig customerDto)
            {
                CustomerLogic customer = GetLogic(customerDto);
                return customer.GetSpaceAssets();
            }

            static public List<SpaceAsset> GetSpaceAssetsForMeter(CustomerConfig customerDto, int meterID)
            {
                CustomerLogic customer = GetLogic(customerDto);
                return customer.GetSpaceAssetsForMeter(meterID);
            }

            static public CustomGroup1Asset GetCustomGroup1Asset(CustomerConfig customerDto, int customGroupId)
            {
                CustomerLogic customer = GetLogic(customerDto);
                return customer.GetCustomGroup1Asset(customGroupId);
            }

            static public List<CustomGroup1Asset> GetCustomGroup1AssetsForCustomer(CustomerConfig customerDto)
            {
                CustomerLogic customer = GetLogic(customerDto);
                return customer.GetCustomGroup1Assets();
            }

            static private CustomerLogic LoadCustomerConfig(int customerID, string customerName, string baseFolder)
            {
              
                // Create new customer object, and assign the passed customer name
                CustomerLogic result = new CustomerLogic();
                result.CustomerId = customerID;
                string dbConnectStr = CustomerLogic.CustomerManager.Instance._DBConnectStr_ReinoComm;
                result.SharedSqlDao = new SqlServerDAO(dbConnectStr, result.LogMessage);

                CustomerConfig dto = new CustomerConfig();
                dto.CustomerName = customerName;
                dto.CustomerId = customerID;

                // Convert the customer name to a DOS-friendly string by converting reserved characters to underscores
                dto.CustomerNameForFiles = Regex.Replace(customerName, @"[?:\/*""<>|]", "_");

                // Construct the physical fileaname we use for storing the relevant settings in a customer-specific "web.config" file
                string configFilename = Path.Combine(baseFolder, "Web.config." + dto.CustomerNameForFiles);

                // Populate the config from the persisted file (if there is one)
                PopulateCustomerDtoFromConfigFile(dto, configFilename);

                // If we have an invalid customer id after loading the config, then we won't allow this customer to finish loading into the site
                if (dto.CustomerId == -1)
                {
                    // Log info
                    if (File.Exists(configFilename))
                    {
                        Logging.AddTextToGenericLog(Logging.LogLevel.Error, "  Failed to load customer '" + dto.CustomerName + "' because " +
                          " no valid CustomerID was found in the config file: '" + configFilename + "'",
                            MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name, System.Threading.Thread.CurrentThread.ManagedThreadId);
                    }
                    else
                    {
                        Logging.AddTextToGenericLog(Logging.LogLevel.Error, "  Failed to load customer '" + dto.CustomerName + "' because " +
                          " no valid CustomerID was resolved",
                            MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name, System.Threading.Thread.CurrentThread.ManagedThreadId);
                    }

                    // Return null result because the objects we started to create are useless
                    result = null;
                    return result;
                }

                // Now set the logic's DTO to the one we created and populated
                result.CustomerDTO = dto;

                // Gather asset/inventory info for this customer from the database -- this will happen in a background thread
                result.GatherAllInventory();

                // Log info
                Logging.AddTextToGenericLog(Logging.LogLevel.Info, "  Loaded Customer: " +
                    dto.CustomerId.ToString() + " (" + dto.CustomerNameForFiles + ")",
                    MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name, System.Threading.Thread.CurrentThread.ManagedThreadId);

                // Return final result
                return result;
            }


            static private CustomerLogic LoadCustomerConfig(string customerName, string baseFolder)
            {
                // Create new customer object, and assign the passed customer name
                CustomerLogic result = new CustomerLogic();
                string dbConnectStr = CustomerLogic.CustomerManager.Instance._DBConnectStr_ReinoComm;
                result.SharedSqlDao = new SqlServerDAO(dbConnectStr, result.LogMessage);

                CustomerConfig dto = new CustomerConfig();
                dto.CustomerName = customerName;

                // Convert the customer name to a DOS-friendly string by converting reserved characters to underscores
                dto.CustomerNameForFiles = Regex.Replace(customerName, @"[?:\/*""<>|]", "_");

                // Construct the physical fileaname we use for storing the relevant settings in a customer-specific "web.config" file
                string configFilename = Path.Combine(baseFolder, "Web.config." + dto.CustomerNameForFiles);

                // If there is no config file, we are essentially stuck, because we don't know the customer id
                if (File.Exists(configFilename) == false)
                {
                    // Log info
                    Logging.AddTextToGenericLog(Logging.LogLevel.Error, "  Failed to load customer '" + dto.CustomerName + "' because " +
                      " required config file not found: '" + configFilename + "'",
                        MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name, System.Threading.Thread.CurrentThread.ManagedThreadId);

                    // Return null result because the objects we started to create are useless
                    result = null;
                    return result;
                }

                // Populate the config from the persisted file (if there is one)
                PopulateCustomerDtoFromConfigFile(dto, configFilename);

                // If we have an invalid customer id after loading the config, then we won't allow this customer to finish loading into the site
                if (dto.CustomerId == -1)
                {
                    // Log info
                    Logging.AddTextToGenericLog(Logging.LogLevel.Error, "  Failed to load customer '" + dto.CustomerName + "' because " +
                      " no valid CustomerID was found in the config file: '" + configFilename + "'",
                        MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name, System.Threading.Thread.CurrentThread.ManagedThreadId);

                    // Return null result because the objects we started to create are useless
                    result = null;
                    return result;
                }

                // Now set the logic's DTO to the one we created and populated
                result.CustomerDTO = dto;

                // Copy the CustomerID from child DTO object to main object
                result.CustomerId = dto.CustomerId;

                // Gather asset/inventory info for this customer from the database -- this will happen in a background thread
                result.GatherAllInventory();

                // Log info
                Logging.AddTextToGenericLog(Logging.LogLevel.Info, "  Loaded Customer: " +
                    dto.CustomerId.ToString() + " (" + dto.CustomerNameForFiles + ")",
                    MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name, System.Threading.Thread.CurrentThread.ManagedThreadId);

                // Return final result
                return result;

                #region Experimental stuff
                // DEBUG: Work-in-progress for Schema v1 (lousy schema) of the Parking Regulations.  We are using our own XML-based regulations instead -- which are "normalized" and a better schema...
                // DEBUG: RegulatedHours_ExperimentalDBSchemaV1Repository was partially implemented for RegulatedHours storage in ReinoComm DB,
                // but was never completed because the DB schema wasn't what we wanted.  For interim, we are using an XML file for repository
                // instead of the database. Therefore, RegulatedHours_ExperimentalDBSchemaV1Repository code is still a useful starting point
                // incase we end up moving to DB instead of XML, but it will need to be finalized and work with the current schema...
                /*
                if (RegulatedHours_ExperimentalDBSchemaV1Repository.Repository.SharedSqlDao == null)
                    RegulatedHours_ExperimentalDBSchemaV1Repository.Repository.SharedSqlDao = result._sharedSqlDao;

                RegulatedHours_ExperimentalDBSchemaV1Repository.Repository.UpdateMemAndXmlCacheForCustomerFromDB(result.CustomerId, false);

                // DEBUG: just for testing the cache expiration logic, we are calling it again for a test..  Need to remove these redundant test lines!
                RegulatedHours_ExperimentalDBSchemaV1Repository.Repository.UpdateMemAndXmlCacheForCustomerFromDB(result.CustomerId, false);
                RegulatedHours_ExperimentalDBSchemaV1Repository.Repository.UpdateMemAndXmlCacheForCustomerFromDB(result.CustomerId, false);
                RegulatedHours_ExperimentalDBSchemaV1Repository.Repository.UpdateMemAndXmlCacheForCustomerFromDB(result.CustomerId, false);
                RegulatedHours_ExperimentalDBSchemaV1Repository.Repository.UpdateMemAndXmlCacheForCustomerFromDB(result.CustomerId, false);
                RegulatedHours_ExperimentalDBSchemaV1Repository.Repository.UpdateMemAndXmlCacheForCustomerFromDB(result.CustomerId, false);
                RegulatedHours_ExperimentalDBSchemaV1Repository.Repository.UpdateMemAndXmlCacheForCustomerFromDB(result.CustomerId, false);
                RegulatedHours_ExperimentalDBSchemaV1Repository.Repository.UpdateMemAndXmlCacheForCustomerFromDB(result.CustomerId, false);
                RegulatedHours_ExperimentalDBSchemaV1Repository.Repository.UpdateMemAndXmlCacheForCustomerFromDB(result.CustomerId, false);
                RegulatedHours_ExperimentalDBSchemaV1Repository.Repository.UpdateMemAndXmlCacheForCustomerFromDB(result.CustomerId, false);
                RegulatedHours_ExperimentalDBSchemaV1Repository.Repository.UpdateMemAndXmlCacheForCustomerFromDB(result.CustomerId, false);
                RegulatedHours_ExperimentalDBSchemaV1Repository.Repository.UpdateMemAndXmlCacheForCustomerFromDB(result.CustomerId, false);
                */
                #endregion
            }

            //static private CustomerLogic LoadCustomerConfig(string customerName, string baseFolder)
            //{
            //    // Create new customer object, and assign the passed customer name
            //    CustomerLogic result = new CustomerLogic();
            //    string dbConnectStr = CustomerLogic.CustomerManager.Instance._DBConnectStr_ReinoComm;
            //    result.SharedSqlDao = new SqlServerDAO(dbConnectStr, result.LogMessage);

            //    CustomerConfig dto = new CustomerConfig();
            //    dto.CustomerName = customerName;

            //    // Convert the customer name to a DOS-friendly string by converting reserved characters to underscores
            //    dto.CustomerNameForFiles = Regex.Replace(customerName, @"[?:\/*""<>|]", "_");

            //    // Construct the physical fileaname we use for storing the relevant settings in a customer-specific "web.config" file
            //    string configFilename = Path.Combine(baseFolder, "Web.config." + dto.CustomerNameForFiles);

            //    // If there is no config file, we are essentially stuck, because we don't know the customer id
            //    if (File.Exists(configFilename) == false)
            //    {
            //        // Log info
            //        Logging.AddTextToGenericLog(Logging.LogLevel.Error, "  Failed to load customer '" + dto.CustomerName + "' because " +
            //          " required config file not found: '" + configFilename + "'",
            //            MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name, System.Threading.Thread.CurrentThread.ManagedThreadId);

            //        // Return null result because the objects we started to create are useless
            //        result = null;
            //        return result;
            //    }

            //    // Populate the config from the persisted file (if there is one)
            //    PopulateCustomerDtoFromConfigFile(dto, configFilename);

            //    // If we have an invalid customer id after loading the config, then we won't allow this customer to finish loading into the site
            //    if (dto.CustomerId == -1)
            //    {
            //        // Log info
            //        Logging.AddTextToGenericLog(Logging.LogLevel.Error, "  Failed to load customer '" + dto.CustomerName + "' because " +
            //          " no valid CustomerID was found in the config file: '" + configFilename + "'",
            //            MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name, System.Threading.Thread.CurrentThread.ManagedThreadId);

            //        // Return null result because the objects we started to create are useless
            //        result = null;
            //        return result;
            //    }

            //    // Now set the logic's DTO to the one we created and populated
            //    result.CustomerDTO = dto;

            //    // Copy the CustomerID from child DTO object to main object
            //    result.CustomerId = dto.CustomerId;

            //    // Gather asset/inventory info for this customer from the database -- this will happen in a background thread
            //    result.GatherAllInventory();

            //    // Log info
            //    Logging.AddTextToGenericLog(Logging.LogLevel.Info, "  Loaded Customer: " +
            //        dto.CustomerId.ToString() + " (" + dto.CustomerNameForFiles + ")",
            //        MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name, System.Threading.Thread.CurrentThread.ManagedThreadId);

            //    // Return final result
            //    return result;

            //    #region Experimental stuff
            //    // DEBUG: Work-in-progress for Schema v1 (lousy schema) of the Parking Regulations.  We are using our own XML-based regulations instead -- which are "normalized" and a better schema...
            //    // DEBUG: RegulatedHours_ExperimentalDBSchemaV1Repository was partially implemented for RegulatedHours storage in ReinoComm DB,
            //    // but was never completed because the DB schema wasn't what we wanted.  For interim, we are using an XML file for repository
            //    // instead of the database. Therefore, RegulatedHours_ExperimentalDBSchemaV1Repository code is still a useful starting point
            //    // incase we end up moving to DB instead of XML, but it will need to be finalized and work with the current schema...
            //    /*
            //    if (RegulatedHours_ExperimentalDBSchemaV1Repository.Repository.SharedSqlDao == null)
            //        RegulatedHours_ExperimentalDBSchemaV1Repository.Repository.SharedSqlDao = result._sharedSqlDao;

            //    RegulatedHours_ExperimentalDBSchemaV1Repository.Repository.UpdateMemAndXmlCacheForCustomerFromDB(result.CustomerId, false);

            //    // DEBUG: just for testing the cache expiration logic, we are calling it again for a test..  Need to remove these redundant test lines!
            //    RegulatedHours_ExperimentalDBSchemaV1Repository.Repository.UpdateMemAndXmlCacheForCustomerFromDB(result.CustomerId, false);
            //    RegulatedHours_ExperimentalDBSchemaV1Repository.Repository.UpdateMemAndXmlCacheForCustomerFromDB(result.CustomerId, false);
            //    RegulatedHours_ExperimentalDBSchemaV1Repository.Repository.UpdateMemAndXmlCacheForCustomerFromDB(result.CustomerId, false);
            //    RegulatedHours_ExperimentalDBSchemaV1Repository.Repository.UpdateMemAndXmlCacheForCustomerFromDB(result.CustomerId, false);
            //    RegulatedHours_ExperimentalDBSchemaV1Repository.Repository.UpdateMemAndXmlCacheForCustomerFromDB(result.CustomerId, false);
            //    RegulatedHours_ExperimentalDBSchemaV1Repository.Repository.UpdateMemAndXmlCacheForCustomerFromDB(result.CustomerId, false);
            //    RegulatedHours_ExperimentalDBSchemaV1Repository.Repository.UpdateMemAndXmlCacheForCustomerFromDB(result.CustomerId, false);
            //    RegulatedHours_ExperimentalDBSchemaV1Repository.Repository.UpdateMemAndXmlCacheForCustomerFromDB(result.CustomerId, false);
            //    RegulatedHours_ExperimentalDBSchemaV1Repository.Repository.UpdateMemAndXmlCacheForCustomerFromDB(result.CustomerId, false);
            //    RegulatedHours_ExperimentalDBSchemaV1Repository.Repository.UpdateMemAndXmlCacheForCustomerFromDB(result.CustomerId, false);
            //    RegulatedHours_ExperimentalDBSchemaV1Repository.Repository.UpdateMemAndXmlCacheForCustomerFromDB(result.CustomerId, false);
            //    */
            //    #endregion
            //}

            static private string SafeGetAppSetting(Configuration appConfig, string key)
            {
                string result = string.Empty;
                try
                {
                    if (appConfig.AppSettings.Settings[key] != null)
                        result = appConfig.AppSettings.Settings[key].Value;
                }
                catch { }
                return result;
            }

            static private void PopulateCustomerDtoFromConfigFile(CustomerConfig dto, string configFilename)
            {
                // Although we are using a "Web.config" file instead of "application.exe.config", we can't access it
                // via the WebConfigurationManager because it doesn't give enough flexibility to specifiy the actual 
                // filename we want to use. Fortunately we can use an ExeConfigurationManager and ExeConfigurationFileMap
                // to specify the exact filename we want to use, and the sections we need to use seem to be compatible.
                ExeConfigurationFileMap FileMap = new ExeConfigurationFileMap();
                FileMap.ExeConfigFilename = configFilename;
                Configuration appConfig = ConfigurationManager.OpenMappedExeConfiguration(FileMap, ConfigurationUserLevel.None);
                dto.CustomerTimeZone = null;  // Clear value here, then we will try to populate from file. At the end, if still fails, will set to server timezone
                if (appConfig.HasFile)
                {
                    // Retreive the application settings from the configuration manager
                    int tempIntValue = -1;
                    if (Int32.TryParse(SafeGetAppSetting(appConfig, "CustomerId"), out tempIntValue))
                        dto.CustomerId = tempIntValue;

                    // See if vehicle sensing is enabled in the configuration
                    bool tempBoolValue = false;
                    if (Boolean.TryParse(SafeGetAppSetting(appConfig, "VehSensingEnabled"), out tempBoolValue))
                        dto.VehSensingEnabled = tempBoolValue;

                    // Get the grace period boolean value
                    tempBoolValue = false;
                    if (Boolean.TryParse(SafeGetAppSetting(appConfig, "GracePeriodEnabled"), out tempBoolValue))
                        dto.GracePeriodEnabled = tempBoolValue;

                    // Get the integer value for refresh interval
                    tempIntValue = -1;
                    if (Int32.TryParse(SafeGetAppSetting(appConfig, "DesktopRefreshTimeInSeconds"), out tempIntValue))
                        dto.DesktopRefreshTimeInSeconds = tempIntValue;

                    // Get the integer value for refresh interval
                    tempIntValue = -1;
                    if (Int32.TryParse(SafeGetAppSetting(appConfig, "MobileRefreshTimeInSeconds"), out tempIntValue))
                        dto.MobileRefreshTimeInSeconds = tempIntValue;

                    // Get the integer value for payment expiration times to be considered critical (expires soon)
                    tempIntValue = -1;
                    if (Int32.TryParse(SafeGetAppSetting(appConfig, "ExpiryTimeToBeCriticalInSeconds"), out tempIntValue))
                        dto.ExpiryTimeToBeCriticalInSeconds = tempIntValue;

                    // Get the integer value for bay occupancy tolerance minutes
                    tempIntValue = -1;
                    if (Int32.TryParse(SafeGetAppSetting(appConfig, "BayOccupancyToleranceInMinutes"), out tempIntValue))
                        dto.BayOccupancyToleranceInMinutes = tempIntValue;

                    // Simple string values
                    dto.InvalidBayString = SafeGetAppSetting(appConfig, "InvalidBayString");
                    dto.DestinationTimeZoneDisplayName = SafeGetAppSetting(appConfig, "DestinationTimeZoneDisplayName");
                    dto.ShortSecondsSpanFormat = SafeGetAppSetting(appConfig, "ShortSecondsSpanFormat");
                    dto.ShortHoursMinutesSpanFormat = SafeGetAppSetting(appConfig, "ShortHoursMinutesSpanFormat");
                    dto.ShortDaysSpanFormat = SafeGetAppSetting(appConfig, "ShortDaysSpanFormat");
                    dto.ShortMonthsSpanFormat = SafeGetAppSetting(appConfig, "ShortMonthsSpanFormat");
                    dto.ShortYearsSpanFormat = SafeGetAppSetting(appConfig, "ShortYearsSpanFormat");

                    // Vehicle Sensing Provider
                    string tempStringValue = SafeGetAppSetting(appConfig, "VehSensingProvider");
                    if (string.IsNullOrEmpty(tempStringValue))
                    {
                        if (dto.VehSensingEnabled == true)
                            dto.VehSensingProvider = CustomerConfig.VehSensingProviders.GTC;
                        else
                            dto.VehSensingProvider = CustomerConfig.VehSensingProviders.None;
                    }
                    else
                    {
                        try
                        {
                            dto.VehSensingProvider = (CustomerConfig.VehSensingProviders)Enum.Parse(typeof(CustomerConfig.VehSensingProviders), tempStringValue, true);
                        }
                        catch
                        {
                            dto.VehSensingProvider = CustomerConfig.VehSensingProviders.None;
                        }
                    }
                }

                // After config is loaded, setup some timezone information that will be used throughout the application
                dto.ServerTimeZone = CustomerManager.Instance.ServerTimeZone;
                dto.CustomerTimeZone = CustomerManager.FindTimeZoneByDisplayName(dto.DestinationTimeZoneDisplayName);

                // Set a default fallback for the timezone incase we couldn't resolve it
                if (dto.CustomerTimeZone == null)
                {
                    // Log the fact that we couldn't resolve the desired timezone
                    Logging.AddTextToGenericLog(Logging.LogLevel.Warning, "Couldn't resolve timezone for '" + dto.DestinationTimeZoneDisplayName + "', so defaulting to '" +
                        dto.ServerTimeZone.DisplayName + "' for Customer: " + dto.CustomerId.ToString() + " (" + dto.CustomerNameForFiles + ")",
                        MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name, System.Threading.Thread.CurrentThread.ManagedThreadId);

                    // Assign the timezone and display name of the server as our fallback value
                    dto.CustomerTimeZone = dto.ServerTimeZone;
                    dto.DestinationTimeZoneDisplayName = dto.ServerTimeZone.DisplayName;
                }
            }



            static private void RenameCustomer(CustomerConfig currentDto, CustomerConfig editedDto)
            {
                try
                {
                    try
                    {
                        // Use an exclusive write lock since we are modifying our Customers collection
                        CustomerManager.Instance._CustomersLocker.EnterWriteLock();

                        // Load the existing customer list
                        DataSet dsCustomerList = new DataSet();
                        dsCustomerList.ReadXml(Path.Combine(CustomerManager.Instance._RootPath, "Customers.xml"));
                        // Create a Customer table if the dataset doesn't already have it
                        DataTable dtCustomers = null;
                        dtCustomers = dsCustomerList.Tables["Customer"];
                        if (dtCustomers == null)
                        {
                            dtCustomers = new DataTable("Customer");
                            dsCustomerList.Tables.Add(dtCustomers);
                            DataColumn col = new DataColumn("CustomerName", typeof(String));
                            dtCustomers.Columns.Add(col);
                        }

                        // Look through the customer table to find the current customer name (the one we will be renaming)
                        for (int i = 0; i < dtCustomers.Rows.Count; i++)
                        {
                            DataRow nextRow = dtCustomers.Rows[i];
                            if (string.Compare(nextRow["CustomerName"].ToString(), currentDto.CustomerName, true) == 0)
                            {
                                // Update the row with the new customer name
                                nextRow["CustomerName"] = editedDto.CustomerName;
                            }
                        }
                        // Accept changes, persist the file, then cleanup resources
                        dsCustomerList.AcceptChanges();
                        dsCustomerList.WriteXml(Path.Combine(CustomerManager.Instance._RootPath, "Customers.xml"));
                        dsCustomerList.Dispose();

                        // Now we need to see if there is a customer config file that needs to be renamed also
                        // Convert the customer name to a DOS-friendly string by converting reserved characters to underscores
                        currentDto.CustomerNameForFiles = Regex.Replace(currentDto.CustomerName, @"[?:\/*""<>|]", "_");
                        editedDto.CustomerNameForFiles = Regex.Replace(editedDto.CustomerName, @"[?:\/*""<>|]", "_");

                        // Construct the physical fileaname we use for storing the relevant settings in a customer-specific "web.config" file
                        string oldFilename = Path.Combine(CustomerManager.Instance._RootPath, "Web.config." + currentDto.CustomerNameForFiles);
                        string newFilename = Path.Combine(CustomerManager.Instance._RootPath, "Web.config." + editedDto.CustomerNameForFiles);

                        // Rename the file if necessary
                        if (File.Exists(newFilename))
                        {
                            File.Delete(newFilename);
                        }
                        if (File.Exists(oldFilename))
                        {
                            File.Move(oldFilename, newFilename);
                        }

                        // Now update the internal object inside the customer logic
                        CustomerLogic logic = null;
                        CustomerManager.Instance._Customers.TryGetValue(currentDto.CustomerId, out logic);
                        if (logic != null)
                        {
                            currentDto.CustomerName = editedDto.CustomerName;
                            currentDto.CustomerNameForFiles = editedDto.CustomerNameForFiles;
                            logic.CustomerDTO = currentDto;
                        }
                    }
                    finally
                    {
                        // Release the exclusive write lock           
                        if (CustomerManager.Instance._CustomersLocker.IsWriteLockHeld)
                            CustomerManager.Instance._CustomersLocker.ExitWriteLock();
                    }
                }
                catch (Exception ex)
                {
                    Logging.AddTextToGenericLog(Logging.LogLevel.Error, ex.ToString(),
                        MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name, System.Threading.Thread.CurrentThread.ManagedThreadId);
                }
            }
            
            static public void EditCustomer(int customerId, CustomerConfig editedDto)
            {
                // Get a copy of the existing dto object
                CustomerConfig currentDto = GetDTOForCID(customerId);

                // Does this edit involve renaming the customer? (If so, there are special considerations because Customers.xml needs to be modified,
                // and the existing customer configuration file needs to be renamed)
                if (string.Compare(currentDto.CustomerName, editedDto.CustomerName) != 0)
                {
                    RenameCustomer(currentDto, editedDto);

                    // The rename process altered our current object, so we need to get a new instance of it
                    currentDto = GetDTOForCID(customerId);
                }

                try
                {
                    // Update the current DTO object with selected values from the edited DTO object
                    currentDto.BayOccupancyToleranceInMinutes = editedDto.BayOccupancyToleranceInMinutes;
                    currentDto.CustomerName = editedDto.CustomerName;
                    currentDto.CustomerNameForFiles = Regex.Replace(currentDto.CustomerName, @"[?:\/*""<>|]", "_");
                    currentDto.CustomerTimeZone = editedDto.CustomerTimeZone;
                    currentDto.DesktopRefreshTimeInSeconds = editedDto.DesktopRefreshTimeInSeconds;
                    currentDto.DestinationTimeZoneDisplayName = editedDto.DestinationTimeZoneDisplayName;
                    currentDto.ExpiryTimeToBeCriticalInSeconds = editedDto.ExpiryTimeToBeCriticalInSeconds;
                    currentDto.GracePeriodEnabled = editedDto.GracePeriodEnabled;
                    currentDto.MobileRefreshTimeInSeconds = editedDto.MobileRefreshTimeInSeconds;
                    currentDto.VehSensingEnabled = editedDto.VehSensingEnabled;
 
                    // These properties are not editable in the UI (yet), so the editedDto won't have correct values to copy from 
                    /*
                    //currentDto.InvalidBayString = editedDto.InvalidBayString;
                    //currentDto.ShortDaysSpanFormat = editedDto.ShortDaysSpanFormat;
                    //currentDto.ShortHoursMinutesSpanFormat = editedDto.ShortHoursMinutesSpanFormat;
                    //currentDto.ShortMonthsSpanFormat = editedDto.ShortMonthsSpanFormat;
                    //currentDto.ShortSecondsSpanFormat = editedDto.ShortSecondsSpanFormat;
                    //currentDto.ShortYearsSpanFormat = editedDto.ShortYearsSpanFormat;
                    //currentDto.VehSensingProvider = editedDto.VehSensingProvider;
                    */

                    try
                    {
                        // Use an exclusive write lock since we are modifying our Customers collection
                        CustomerManager.Instance._CustomersLocker.EnterWriteLock();

                        // Now update the internal object inside the customer logic
                        CustomerLogic logic = null;
                        CustomerManager.Instance._Customers.TryGetValue(currentDto.CustomerId, out logic);
                        if (logic != null)
                        {
                            logic.CustomerDTO = currentDto;
                        }

                        // And now we need to persist the changes to file
                        PersistCustomerDtoToConfigFile(currentDto);
                    }
                    finally
                    {
                        // Release the exclusive write lock           
                        if (CustomerManager.Instance._CustomersLocker.IsWriteLockHeld)
                            CustomerManager.Instance._CustomersLocker.ExitWriteLock();
                    }
                }
                catch (Exception ex)
                {
                    Logging.AddTextToGenericLog(Logging.LogLevel.Error, ex.ToString(),
                        MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name, System.Threading.Thread.CurrentThread.ManagedThreadId);
                }
            }

            static public void DeleteCustomer(CustomerConfig dto)
            {
                try
                {
                    try
                    {
                        // Use an exclusive write lock since we are modifying our Customers collection
                        CustomerManager.Instance._CustomersLocker.EnterWriteLock();

                        // Remove the customer from our in-memory list
                        if (CustomerManager.Instance._Customers.ContainsKey(dto.CustomerId))
                        {
                            // Get the existing customer logic object
                            CustomerLogic logic = CustomerManager.Instance._Customers[dto.CustomerId];

                            // Clear out the inventory for this customer before we remove the customer from the site
                            try
                            {
                                // Use an exclusive write lock for this operation
                                logic._InventoryLocker.EnterWriteLock();

                                // Clear the inventory categories
                                logic._Inventory.Areas.Clear();
                                logic._Inventory.Meters.Clear();
                                logic._Inventory.PAMClusters.Clear();
                                logic._Inventory.Spaces.Clear();

                            }
                            finally
                            {
                                // Release the exclusive write lock           
                                if (logic._InventoryLocker.IsWriteLockHeld)
                                    logic._InventoryLocker.ExitWriteLock();
                            }

                            // Finally we will remove the customer logic from our internal list
                            CustomerManager.Instance._Customers.Remove(dto.CustomerId);
                        }


                        // Load the persisted customer list, which we will be removing the customer from
                        DataSet dsCustomerList = new DataSet();
                        dsCustomerList.ReadXml(Path.Combine(CustomerManager.Instance._RootPath, "Customers.xml"));
                        // Create a Customer table if the dataset doesn't already have it
                        DataTable dtCustomers = null;
                        dtCustomers = dsCustomerList.Tables["Customer"];
                        if (dtCustomers == null)
                        {
                            dtCustomers = new DataTable("Customer");
                            dsCustomerList.Tables.Add(dtCustomers);
                            DataColumn col = new DataColumn("CustomerName", typeof(String));
                            dtCustomers.Columns.Add(col);
                        }
                        // Look through the customer table to find the row to remove
                        foreach (DataRow nextRow in dtCustomers.Rows)
                        {
                            if (string.Compare(nextRow["CustomerName"].ToString(), dto.CustomerName, true) == 0)
                            {
                                nextRow.Delete();
                                break;
                            }
                        }
                        // Accept changes, persist the file, then cleanup resources
                        dsCustomerList.AcceptChanges();
                        dsCustomerList.WriteXml(Path.Combine(CustomerManager.Instance._RootPath, "Customers.xml"));
                        dsCustomerList.Dispose();
                    }
                    finally
                    {
                        // Release the exclusive write lock           
                        if (CustomerManager.Instance._CustomersLocker.IsWriteLockHeld)
                            CustomerManager.Instance._CustomersLocker.ExitWriteLock();
                    }
                }
                catch (Exception ex)
                {
                    Logging.AddTextToGenericLog(Logging.LogLevel.Error, ex.ToString(),
                        MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name, System.Threading.Thread.CurrentThread.ManagedThreadId);
                }
            }

            static public void AddNewCustomerToSite(int customerID)
            {              
                string dbConnectStr = CustomerLogic.CustomerManager.Instance._DBConnectStr_ReinoComm;
                SqlServerDAO sqlDao = new SqlServerDAO(dbConnectStr, LogMessage);

                // Build SQL statement to get/verify the customer name and id from the database
                StringBuilder sql = new StringBuilder();
                sql.Append("select CustomerId, Name from Customer where CustomerId = @CustomerId");
                using (SqlCommand command = sqlDao.GetSqlCommand(sql.ToString()))
                {
                    // Set SQL query parameters
                    command.Parameters.Add(sqlDao.CreateParameter("@CustomerId", customerID));

                    try
                    {
                        // Query the DB and deserialize the result into a CustomerNameAndId object
                        CustomerNameAndId CidAndNameFromDB = sqlDao.GetSingle<CustomerNameAndId>(command);
                        if (CidAndNameFromDB != null)
                        {
                            // Call our overloaded version of LoadCustomerConfig where we are supplying the CustomerID 
                            // (It's quite likely the config file that holds the CustomerID doesn't exist yet!)
                            CustomerLogic oneCustomer = CustomerManager.LoadCustomerConfig(CidAndNameFromDB.CustomerId, CidAndNameFromDB.Name, CustomerManager.Instance._RootPath);
                            CustomerConfig currentDto = null;
                            if (oneCustomer != null)
                            {
                                // Get a local copy of the current DTO object for this customer
                                currentDto = oneCustomer.CustomerDTO;

                                try
                                {
                                    // Use an exclusive write lock since we are modifying our Customers collection
                                    CustomerManager.Instance._CustomersLocker.EnterWriteLock();

                                    // Add the customer to the customer manager
                                    if (CustomerManager.Instance._Customers.ContainsKey(oneCustomer.CustomerId) == false)
                                        CustomerManager.Instance._Customers.Add(oneCustomer.CustomerId, oneCustomer);

                                    // Load the existing customer list, which we will need to add our customer to
                                    DataSet dsCustomerList = new DataSet();
                                    dsCustomerList.ReadXml(Path.Combine(CustomerManager.Instance._RootPath, "Customers.xml"));
                                    // Create a Customer table if the dataset doesn't already have it
                                    DataTable dtCustomers = null;
                                    dtCustomers = dsCustomerList.Tables["Customer"];
                                    if (dtCustomers == null)
                                    {
                                        dtCustomers = new DataTable("Customer");
                                        dsCustomerList.Tables.Add(dtCustomers);
                                        DataColumn col = new DataColumn("CustomerName", typeof(String));
                                        dtCustomers.Columns.Add(col);
                                    }
                                    // Just to be extra careful, we will Look through the customer table and make sure we aren't trying to add a second instance of the customer
                                    bool customerIsInTable = false;
                                    foreach (DataRow nextRow in dtCustomers.Rows)
                                    {
                                        if (string.Compare(nextRow["CustomerName"].ToString(), CidAndNameFromDB.Name, true) == 0)
                                        {
                                            customerIsInTable = true;
                                            break;
                                        }
                                    }
                                    // If the customer wasn't already in the table, we will add it and then persist to file
                                    if (customerIsInTable == false)
                                    {
                                        DataRow drCustomer = dtCustomers.NewRow();
                                        dtCustomers.Rows.Add(drCustomer);
                                        drCustomer["CustomerName"] = CidAndNameFromDB.Name;

                                        // Accept changes, persist the file, then cleanup resources
                                        dsCustomerList.AcceptChanges();
                                        dsCustomerList.WriteXml(Path.Combine(CustomerManager.Instance._RootPath, "Customers.xml"));
                                    }
                                    dsCustomerList.Dispose();

                                    // Now we need to create a configuration file for this customer
                                    PersistCustomerDtoToConfigFile(currentDto);
                                }
                                finally
                                {
                                    // Release the exclusive write lock           
                                    if (CustomerManager.Instance._CustomersLocker.IsWriteLockHeld)
                                        CustomerManager.Instance._CustomersLocker.ExitWriteLock();
                                }
                            }
                        }
                    }
                    finally
                    {
                    }
                }
            }

            static public List<CustomerNameAndId> GetCustomersThatCanBeAddedToSite()
            {
                List<CustomerNameAndId> result = null;
                List<CustomerConfig> currentDTOs = GetAllCustomerDTOs();

                string dbConnectStr = CustomerLogic.CustomerManager.Instance._DBConnectStr_ReinoComm;
                SqlServerDAO sqlDao = new SqlServerDAO(dbConnectStr, LogMessage);

                StringBuilder sql = new StringBuilder();
                sql.Append(
                    "select CustomerId, Name from Customers order by CustomerId");
                using (SqlCommand command = sqlDao.GetSqlCommand(sql.ToString()))
                {
                    try
                    {
                        // Get list of customers from the database
                        result = sqlDao.GetList<CustomerNameAndId>(command);

                        if ((result != null) && (currentDTOs != null))
                        {
                            // Working backward through the list, we want to remove any that match Customer IDs we already have loaded
                            for (int loIdx = result.Count - 1; loIdx >= 0; loIdx--)
                            {
                                CustomerNameAndId pendingItem = result[loIdx];
                                foreach (CustomerConfig nextDto in currentDTOs)
                                {
                                    // If there is a match on customer id, then remove the item from the result
                                    if (pendingItem.CustomerId == nextDto.CustomerId)
                                    {
                                        result.RemoveAt(loIdx);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                    }
                }

                return result;
            }

            static public DateTime GetDBServerTime()
            {
                DateTime result = DateTime.MinValue;

                string dbConnectStr = CustomerLogic.CustomerManager.Instance._DBConnectStr_ReinoComm;
                SqlServerDAO sqlDao = new SqlServerDAO(dbConnectStr, LogMessage);

                StringBuilder sql = new StringBuilder();
                sql.Append("select GetDate()");
                using (SqlCommand command = sqlDao.GetSqlCommand(sql.ToString()))
                {
                    try
                    {
                        result = Convert.ToDateTime(sqlDao.ExecuteScalar(command));
                    }
                    finally
                    {
                    }
                }

                return result;
            }
            
            static public void RefreshCustomerInventory(CustomerConfig dto)
            {
                try
                {
                    CustomerLogic logic = GetLogicForCID(dto.CustomerId);
                    logic.GatherAllInventory();
                }
                catch (Exception ex)
                {
                    Logging.AddTextToGenericLog(Logging.LogLevel.Error, ex.ToString(),
                        MethodBase.GetCurrentMethod().DeclaringType.Name + "." + MethodBase.GetCurrentMethod().Name, System.Threading.Thread.CurrentThread.ManagedThreadId);
                }
            }


            static private void PersistCustomerDtoToConfigFile(CustomerConfig dto)
            {
                // Convert the customer name to a DOS-friendly string by converting reserved characters to underscores
                dto.CustomerNameForFiles = Regex.Replace(dto.CustomerName, @"[?:\/*""<>|]", "_");

                // Create or update the configuration file for this customer
                string configFilename = Path.Combine(CustomerManager.Instance._RootPath, "Web.config." + dto.CustomerNameForFiles);
                ExeConfigurationFileMap FileMap = new ExeConfigurationFileMap();
                FileMap.ExeConfigFilename = configFilename;
                Configuration appConfig = null;

                appConfig = ConfigurationManager.OpenMappedExeConfiguration(FileMap, ConfigurationUserLevel.None);
                if (appConfig.HasFile == false)
                {
                    appConfig.Save();
                }

                // Update all applicable configuration elements in the app settings section
                UpdateOrAddWAppSetting(appConfig, "CustomerId", dto.CustomerId.ToString());
                UpdateOrAddWAppSetting(appConfig, "VehSensingEnabled", dto.VehSensingEnabled.ToString());
                UpdateOrAddWAppSetting(appConfig, "DesktopRefreshTimeInSeconds", dto.DesktopRefreshTimeInSeconds.ToString());
                UpdateOrAddWAppSetting(appConfig, "MobileRefreshTimeInSeconds", dto.MobileRefreshTimeInSeconds.ToString());
                UpdateOrAddWAppSetting(appConfig, "InvalidBayString", dto.InvalidBayString);
                UpdateOrAddWAppSetting(appConfig, "ExpiryTimeToBeCriticalInSeconds", dto.ExpiryTimeToBeCriticalInSeconds.ToString());
                UpdateOrAddWAppSetting(appConfig, "DestinationTimeZoneDisplayName", dto.DestinationTimeZoneDisplayName);
                UpdateOrAddWAppSetting(appConfig, "ShortSecondsSpanFormat", dto.ShortSecondsSpanFormat);
                UpdateOrAddWAppSetting(appConfig, "ShortHoursMinutesSpanFormat", dto.ShortHoursMinutesSpanFormat);
                UpdateOrAddWAppSetting(appConfig, "ShortDaysSpanFormat", dto.ShortDaysSpanFormat);
                UpdateOrAddWAppSetting(appConfig, "ShortMonthsSpanFormat", dto.ShortMonthsSpanFormat);
                UpdateOrAddWAppSetting(appConfig, "ShortYearsSpanFormat", dto.ShortYearsSpanFormat);
                UpdateOrAddWAppSetting(appConfig, "BayOccupancyToleranceInMinutes", dto.BayOccupancyToleranceInMinutes.ToString());
                UpdateOrAddWAppSetting(appConfig, "GracePeriodEnabled", dto.GracePeriodEnabled.ToString());
                UpdateOrAddWAppSetting(appConfig, "VehSensingProvider", dto.VehSensingProvider.ToString());

                // Do final save
                appConfig.Save();
            }

            static private void UpdateOrAddWAppSetting(Configuration appConfig, string key, string value)
            {
                if (appConfig.AppSettings.Settings[key] == null)
                    appConfig.AppSettings.Settings.Add(key, value);
                else
                    appConfig.AppSettings.Settings[key].Value = value;
            }



            static public Duncan.PEMS.SpaceStatus.UtilityClasses.TimeZoneInfo FindTimeZoneByDisplayName(String timeZoneName)
            {
                foreach (Duncan.PEMS.SpaceStatus.UtilityClasses.TimeZoneInfo zone in CustomerManager.Instance.TimeZones)
                {
                    if (String.Equals(zone.DisplayName, timeZoneName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return zone;
                    }
                }

                // Try searching again with a common GMT and UTC substitution (Seems to have changed in Windows 7)
                timeZoneName = timeZoneName.Replace("(GMT", "(UTC");
                foreach (Duncan.PEMS.SpaceStatus.UtilityClasses.TimeZoneInfo zone in CustomerManager.Instance.TimeZones)
                {
                    if (String.Equals(zone.DisplayName, timeZoneName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return zone;
                    }
                }

                return null;
            }

            static private Duncan.PEMS.SpaceStatus.UtilityClasses.TimeZoneInfo FindTimeZoneByStandardName(String timeZoneName)
            {
                foreach (Duncan.PEMS.SpaceStatus.UtilityClasses.TimeZoneInfo zone in CustomerManager.Instance.TimeZones)
                {
                    if (String.Equals(zone.StandardName, timeZoneName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return zone;
                    }
                }
                return null;
            }



            static public void LogMessage(System.Diagnostics.TraceLevel logLevel, string textToLog, string classAndMethod, int threadID)
            {
                Logging.LogLevel convertedLogLevel = Logging.LogLevel.Debug;
                switch (logLevel)
                {
                    case System.Diagnostics.TraceLevel.Error:
                        convertedLogLevel = Logging.LogLevel.Error;
                        break;
                    case System.Diagnostics.TraceLevel.Info:
                        convertedLogLevel = Logging.LogLevel.Info;
                        break;
                    case System.Diagnostics.TraceLevel.Verbose:
                        convertedLogLevel = Logging.LogLevel.Debug;
                        break;
                    case System.Diagnostics.TraceLevel.Warning:
                        convertedLogLevel = Logging.LogLevel.Warning;
                        break;
                    case System.Diagnostics.TraceLevel.Off:
                        convertedLogLevel = Logging.LogLevel.DebugTraceOutput;
                        break;
                    default:
                        convertedLogLevel = Logging.LogLevel.Debug;
                        break;
                }

                Logging.AddTextToGenericLog(convertedLogLevel, textToLog, classAndMethod, threadID);
            }

            #region Deprecated, but useful code snippets
            /*
            static private string GetDBConnectStringFromXml(string connectionName, XmlDocument doc)
            {
                return doc.SelectSingleNode("/configuration/connectionStrings/add[@name='" + connectionName + "']").Attributes["connectionString"].Value;
            }

            static private string GetAppSettingFromXml(string keyName, XmlDocument doc)
            {
                return doc.SelectSingleNode("/configuration/appSettings/add[@key='" + keyName + "']").Attributes["value"].Value;
            }
            */
            #endregion
        }

    }
    #endregion





    #region CustomerAssets implementation
    public partial class CustomerLogic
    {
        public class CustomerAssets
        {
            public List<SpaceAsset> Spaces = new List<SpaceAsset>();
            public List<MeterAsset> Meters = new List<MeterAsset>();
            public List<PAMClusterAsset> PAMClusters = new List<PAMClusterAsset>();
            public List<AreaAsset> Areas = new List<AreaAsset>();
            public List<CustomGroup1Asset> CustomGroup1s = new List<CustomGroup1Asset>();

            public CustomerAssets()
            {
            }
        }
    }
    #endregion

}
