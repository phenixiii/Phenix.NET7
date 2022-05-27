using System.Threading.Tasks;
using Dapr;
using Microsoft.AspNetCore.Mvc;
using Phenix.Core.Event;
using Phenix.iPost.ROS.Plugin.Adapter.EventHandling;
using Phenix.iPost.ROS.Plugin.Adapter.Events.Sub;

namespace Phenix.iPost.ROS.Plugin.Adapter
{
    /// <summary>
    /// 订阅事件
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        [HttpPost]
        [Topic(IntegrationEvent.PubSubName, nameof(AckVehicleTaskEvent))]
        public Task AckVehicleTask(AckVehicleTaskEvent @event,
            [FromServices] AckVehicleTaskEventHandler handler) => handler.Handle(@event);

        [HttpPost]
        [Topic(IntegrationEvent.PubSubName, nameof(CraneActionEvent))]
        public Task CraneAction(CraneActionEvent @event,
            [FromServices] CraneActionEventHandler handler) => handler.Handle(@event);

        [HttpPost]
        [Topic(IntegrationEvent.PubSubName, nameof(CraneSpreaderActionEvent))]
        public Task CraneSpreaderAction(CraneSpreaderActionEvent @event,
            [FromServices] CraneSpreaderActionEventHandler handler) => handler.Handle(@event);

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
        [Topic(IntegrationEvent.PubSubName, nameof(VehicleBerthOperationEvent))]
        public Task VehicleBerthOperation(VehicleBerthOperationEvent @event,
            [FromServices] VehicleBerthOperationEventHandler handler) => handler.Handle(@event);

        [HttpPost]
        [Topic(IntegrationEvent.PubSubName, nameof(VehicleYardOperationEvent))]
        public Task VehicleYardOperation(VehicleYardOperationEvent @event,
            [FromServices] VehicleYardOperationEventHandler handler) => handler.Handle(@event);

        [HttpPost]
        [Topic(IntegrationEvent.PubSubName, nameof(VesselStatusEvent))]
        public Task VesselStatus(VesselStatusEvent @event,
            [FromServices] VesselStatusEventHandler handler) => handler.Handle(@event);
    }
}
