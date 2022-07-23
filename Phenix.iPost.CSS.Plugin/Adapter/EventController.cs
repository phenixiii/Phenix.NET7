using System.Threading.Tasks;
using Dapr;
using Microsoft.AspNetCore.Mvc;
using Phenix.Core.Event;
using Phenix.iPost.CSS.Plugin.Adapter.EventHandling;
using Phenix.iPost.CSS.Plugin.Adapter.Events.Sub;

namespace Phenix.iPost.CSS.Plugin.Adapter
{
    /// <summary>
    /// 订阅事件
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        [HttpPost]
        [Topic(IntegrationEvent.PubSubName, nameof(CraneActionEvent))]
        public Task CraneAction(CraneActionEvent @event,
            [FromServices] CraneActionEventHandler handler) => handler.Handle(@event);

        [HttpPost]
        [Topic(IntegrationEvent.PubSubName, nameof(CraneGrabActionEvent))]
        public Task CraneGrabAction(CraneGrabActionEvent @event,
            [FromServices] CraneGrabActionEventHandler handler) => handler.Handle(@event);

        [HttpPost]
        [Topic(IntegrationEvent.PubSubName, nameof(MachinePowerEvent))]
        public Task MachinePower(MachinePowerEvent @event,
            [FromServices] MachinePowerEventHandler handler) => handler.Handle(@event);

        [HttpPost]
        [Topic(IntegrationEvent.PubSubName, nameof(MachineSpaceTimeEvent))]
        public Task MachineSpaceTime(MachineSpaceTimeEvent @event,
            [FromServices] MachineSpaceTimeEventHandler handler) => handler.Handle(@event);

        [HttpPost]
        [Topic(IntegrationEvent.PubSubName, nameof(MachineStatusEvent))]
        public Task MachineStatus(MachineStatusEvent @event,
            [FromServices] MachineStatusEventHandler handler) => handler.Handle(@event);

        [HttpPost]
        [Topic(IntegrationEvent.PubSubName, nameof(MachineTaskAckEvent))]
        public Task MachineTaskAck(MachineTaskAckEvent @event,
            [FromServices] MachineTaskAckEventHandler handler) => handler.Handle(@event);

        [HttpPost]
        [Topic(IntegrationEvent.PubSubName, nameof(VehicleBerthActionEvent))]
        public Task VehicleBerthAction(VehicleBerthActionEvent @event,
            [FromServices] VehicleBerthActionEventHandler handler) => handler.Handle(@event);

        [HttpPost]
        [Topic(IntegrationEvent.PubSubName, nameof(VehicleYardActionEvent))]
        public Task VehicleYardAction(VehicleYardActionEvent @event,
            [FromServices] VehicleYardActionEventHandler handler) => handler.Handle(@event);

        [HttpPost]
        [Topic(IntegrationEvent.PubSubName, nameof(VesselBerthingEvent))]
        public Task VesselBerthing(VesselBerthingEvent @event,
            [FromServices] VesselBerthingEventHandler handler) => handler.Handle(@event);

        [HttpPost]
        [Topic(IntegrationEvent.PubSubName, nameof(VesselDepartedEvent))]
        public Task VesselDeparted(VesselDepartedEvent @event,
            [FromServices] VesselDepartedEventHandler handler) => handler.Handle(@event);
        
        [HttpPost]
        [Topic(IntegrationEvent.PubSubName, nameof(VesselImportBayPlanEvent))]
        public Task VesselImportBayPlan(VesselImportBayPlanEvent @event,
            [FromServices] VesselImportBayPlanEventHandler handler) => handler.Handle(@event);
        
        [HttpPost]
        [Topic(IntegrationEvent.PubSubName, nameof(VesselPreBayPlanEvent))]
        public Task VesselPreBayPlan(VesselPreBayPlanEvent @event,
            [FromServices] VesselPreBayPlanEventHandler handler) => handler.Handle(@event);
    }
}
