using System;

using FullPotential.Api.CoreTypeIds;

namespace FullPotential.Core.UI.Events
{
    public class OnDrawingStopEventArgs : EventArgs
    {
        public string EventSource { get; }
        public string DrawnShape { get; }
        public string ItemId { get; }
        public SlotId SlotId { get; }

        public OnDrawingStopEventArgs(
            string eventSource,
            string drawnShape,
            string itemId,
            SlotId slotId)
        {
            EventSource = eventSource;
            DrawnShape = drawnShape;
            ItemId = itemId;
            SlotId = slotId;
        }
    }
}
