using Orleans;

namespace Platformex.Domain
{
    public class EventSubscriberAttribute : ImplicitStreamSubscriptionAttribute
    {
        public EventSubscriberAttribute() : base("InitializeSubscriptions")
        {

        }
    }
}
