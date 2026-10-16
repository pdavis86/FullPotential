namespace FullPotential.Api.EventManagement
{
    public readonly struct HandlerResult
    {
        public NextAction NextAction { get; }

        public object UpdatedEventArgs { get; }

        public HandlerResult(
            NextAction nextAction = NextAction.Continue,
            object updatedEventArgs = null)
        {
            NextAction = nextAction;
            UpdatedEventArgs = updatedEventArgs;
        }
    }
}
