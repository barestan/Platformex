namespace Platformex.Infrastructure
{
    public class Platform : IPlatform
    {
        public Platform WithContext<T>() where  T : Context, new()
        {
            foreach (var def in new T().AggregateDefinitions)
            {
                Definitions.Register(def); 
            }

            return this;
        }

        public Definitions Definitions { get; } = new Definitions();
    }
}