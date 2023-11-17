using System;

namespace FullPotential.Core.UI.Events
{
    public class OnDrawingStopEventArgs : EventArgs
    {
        public string EventSource { get; }
        public string DrawnShape { get; }
        public string ItemId { get; }
        public string SlotId { get; }

        public OnDrawingStopEventArgs(
            string eventSource,
            string drawnShape,
            string itemId,
            string slotId)
        {
            EventSource = eventSource;
            DrawnShape = drawnShape;
            ItemId = itemId;
            SlotId = slotId;
        }
    }
}
