using System.Collections.Generic;
using System.Threading.Tasks;
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
    public class VesselGrain : Phenix.Actor.GrainBase, IVesselGrain
    {
        public VesselGrain(
            [PersistentState(nameof(Phenix.iPost.CSS.Plugin.Business.Norms.VesselStatus))]
            IPersistentState<VesselStatus> vesselStatus,
            [PersistentState(nameof(Phenix.iPost.CSS.Plugin.Business.VesselBerthing))]
            IPersistentState<VesselBerthing> vesselBerthing,
            [PersistentState(nameof(VesselImportBayPlan))]
            IPersistentState<VesselImportBayPlan> importBayPlan,
            [PersistentState(nameof(VesselPreBayPlan))]
            IPersistentState<VesselPreBayPlan> preBayPlan,
            [PersistentState(nameof(VesselExportBayPlan))]
            IPersistentState<VesselExportBayPlan> exportBayPlan)
        {
            _vesselStatus = vesselStatus;
            _vesselBerthing = vesselBerthing;
            _importBayPlan = importBayPlan;
            _preBayPlan = preBayPlan;
            _exportBayPlan = exportBayPlan;
        }

        #region 属性

        /// <summary>
        /// 船舶代码
        /// </summary>
        protected string VesselCode
        {
            get { return PrimaryKeyString; }
        }

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

        private readonly IPersistentState<VesselBerthing> _vesselBerthing;

        /// <summary>
        /// 船舶靠泊
        /// </summary>
        protected VesselBerthing VesselBerthing
        {
            get => _vesselBerthing.State;
            set => _vesselBerthing.State = value;
        }

        /// <summary>
        /// IStorage
        /// </summary>
        protected IStorage VesselBerthingStorage => _vesselBerthing;

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

        async Task IVesselGrain.OnBerthing(VesselBerthing vesselBerthing)
        {
            VesselStatus = VesselStatus.Berthing;
            await VesselStatusStorage.WriteStateAsync();

            VesselBerthing = vesselBerthing;
            await VesselBerthingStorage.WriteStateAsync();
        }

        async Task IVesselGrain.OnDeparted()
        {
            VesselStatus = VesselStatus.Departed;
            await VesselStatusStorage.WriteStateAsync();
        }

        async Task IVesselGrain.OnRefreshImportBayPlan(IDictionary<int, IDictionary<int, IList<Container>>> info)
        {
            ImportBayPlan.OnRefresh(info);
            await ImportBayPlanStorage.WriteStateAsync();
        }

        async Task IVesselGrain.OnRefreshPreBayPlan(IDictionary<int, IDictionary<int, IList<Container>>> info)
        {
            PreBayPlan.OnRefresh(info);
            await PreBayPlanStorage.WriteStateAsync();
        }

        async Task IVesselGrain.OnRefreshExportBayPlan(IDictionary<int, IDictionary<int, IList<Container>>> info)
        {
            ExportBayPlan.OnRefresh(info);
            await ExportBayPlanStorage.WriteStateAsync();
        }

        #endregion

        #endregion
    }
}