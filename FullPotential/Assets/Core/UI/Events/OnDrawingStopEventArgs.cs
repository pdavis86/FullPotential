using System;
using FullPotential.Api.Obsolete;

namespace FullPotential.Core.UI.Events
{
    public class OnDrawingStopEventArgs : EventArgs
    {
        public string EventSource { get; }
        public string DrawnShape { get; }
        public string ItemId { get; }
        public SlotGameObjectName? SlotGameObjectName { get; }

        public OnDrawingStopEventArgs(
            string eventSource,
            string drawnShape,
            string itemId,
            SlotGameObjectName? slotGameObjectName)
        {
            EventSource = eventSource;
            DrawnShape = drawnShape;
            ItemId = itemId;
            SlotGameObjectName = slotGameObjectName;
        }
    }
}
