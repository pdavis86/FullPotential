namespace FullPotential.Api.Gameplay.Events
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
