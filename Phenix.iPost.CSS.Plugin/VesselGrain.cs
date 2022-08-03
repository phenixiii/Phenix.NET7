using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Orleans.Core;
using Orleans.Runtime;
using Phenix.iPost.CSS.Plugin.Business;
using Phenix.iPost.CSS.Plugin.Business.Norms;

namespace Phenix.iPost.CSS.Plugin
{
    /// <summary>
    /// 船舶Grain
    /// key: VesselCode
    /// </summary>
    public class VesselGrain : Phenix.Actor.GrainBase, IVesselGrain, IRemindable
    {
        public VesselGrain(
            [PersistentState(nameof(VesselStatus))] IPersistentState<VesselStatus> vesselStatus,
            [PersistentState(nameof(BerthingInfo))] IPersistentState<VesselBerthingInfo> berthingInfo,
            [PersistentState(nameof(ImportBayPlan))] IPersistentState<VesselImportBayPlan> importBayPlan,
            [PersistentState(nameof(PreBayPlan))] IPersistentState<VesselPreBayPlan> preBayPlan,
            [PersistentState(nameof(ExportBayPlan))] IPersistentState<VesselExportBayPlan> exportBayPlan)
        {
            _vesselStatus = vesselStatus;
            _berthingInfo = berthingInfo;
            _importBayPlan = importBayPlan;
            _preBayPlan = preBayPlan;
            _exportBayPlan = exportBayPlan;
        }

        #region 属性

        /// <summary>
        /// 船舶代码
        /// </summary>
        protected string VesselCode => PrimaryKeyString;

        #region Kernel

        private readonly IPersistentState<VesselStatus> _vesselStatus;

        /// <summary>
        /// 船舶状态
        /// </summary>
        protected VesselStatus VesselStatus
        {
            get => _vesselStatus.State;
            set => _vesselStatus.State = value;
        }

        /// <summary>
        /// IStorage
        /// </summary>
        protected IStorage VesselStatusStorage => _vesselStatus;

        private readonly IPersistentState<VesselBerthingInfo> _berthingInfo;

        /// <summary>
        /// 船舶靠泊
        /// </summary>
        protected VesselBerthingInfo BerthingInfo
        {
            get => _berthingInfo.State;
            set => _berthingInfo.State = value;
        }

        /// <summary>
        /// IStorage
        /// </summary>
        protected IStorage BerthingInfoStorage => _berthingInfo;

        private readonly IPersistentState<VesselImportBayPlan> _importBayPlan;

        /// <summary>
        /// 进口船图
        /// </summary>
        protected VesselImportBayPlan ImportBayPlan
        {
            get { return _importBayPlan.State ??= new VesselImportBayPlan(); }
        }

        /// <summary>
        /// IStorage
        /// </summary>
        protected IStorage ImportBayPlanStorage => _importBayPlan;

        private readonly IPersistentState<VesselPreBayPlan> _preBayPlan;

        /// <summary>
        /// 预配船图
        /// </summary>
        protected VesselPreBayPlan PreBayPlan
        {
            get { return _preBayPlan.State ??= new VesselPreBayPlan(); }
        }

        /// <summary>
        /// IStorage
        /// </summary>
        protected IStorage PreBayPlanStorage => _preBayPlan;

        private readonly IPersistentState<VesselExportBayPlan> _exportBayPlan;

        /// <summary>
        /// 出口船图
        /// </summary>
        protected VesselExportBayPlan ExportBayPlan
        {
            get { return _exportBayPlan.State ??= new VesselExportBayPlan(); }
        }

        /// <summary>
        /// IStorage
        /// </summary>
        protected IStorage ExportBayPlanStorage => _exportBayPlan;

        #endregion

        #endregion

        #region 方法

        #region Event

        async Task IVesselGrain.OnBerthing(VesselBerthingInfo berthingInfo)
        {
            if (VesselStatus != VesselStatus.Berthing)
            {
                VesselStatus = VesselStatus.Berthing;
                await VesselStatusStorage.WriteStateAsync();
            }

            if (BerthingInfo != berthingInfo)
            {
                BerthingInfo = berthingInfo;
                await BerthingInfoStorage.WriteStateAsync();
            }

            // 启动作业
            IGrainReminder reminder = await GetReminder(VesselCode);
            if (reminder == null)
                await RegisterOrUpdateReminder(VesselCode, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }

        async Task IVesselGrain.OnDeparted()
        {
            if (VesselStatus != VesselStatus.Departed)
            {
                VesselStatus = VesselStatus.Departed;
                await VesselStatusStorage.WriteStateAsync();
            }

            // 关闭作业
            IGrainReminder reminder = await GetReminder(VesselCode);
            if (reminder != null)
                await UnregisterReminder(reminder);
        }

        async Task IVesselGrain.OnRefreshImportBayPlan(IDictionary<int, IDictionary<int, IList<ContainerInfo>>> info)
        {
            ImportBayPlan.OnRefresh(info);
            await ImportBayPlanStorage.WriteStateAsync();
        }

        async Task IVesselGrain.OnRefreshPreBayPlan(IDictionary<int, IDictionary<int, IList<ContainerInfo>>> info)
        {
            PreBayPlan.OnRefresh(info);
            await PreBayPlanStorage.WriteStateAsync();
        }

        async Task IVesselGrain.OnRefreshExportBayPlan(IDictionary<int, IDictionary<int, IList<ContainerInfo>>> info)
        {
            ExportBayPlan.OnRefresh(info);
            await ExportBayPlanStorage.WriteStateAsync();
        }

        #endregion

        async Task IRemindable.ReceiveReminder(string reminderName, TickStatus status)
        {
            if (VesselStatus == VesselStatus.Berthing && ImportBayPlan.Info.Count > 0)
            {

            }
        }

        #endregion
    }
}