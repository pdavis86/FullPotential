using FullPotential.Api.Gameplay.Events;

namespace FullPotential.Api.Gameplay.Player.Events
{
    public readonly struct ShowUiAlertEvent : IEvent
    {
        public string Template { get; }

        public string[] Arguments { get; }

        public ShowUiAlertEvent(string template, string[] arguments = null)
        {
            Template = template;
            Arguments = arguments;
        }
    }
}
