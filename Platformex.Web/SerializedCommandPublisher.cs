using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Platformex.Domain.Definitions;

namespace Platformex.Web
{
    public class SerializedCommandPublisher : ISerializedCommandPublisher
    {
        private readonly IPlatform _platform;

        public SerializedCommandPublisher(IPlatform platform)
        {
            _platform = platform;
        }

        public async Task<CommandResult> PublishSerilizedCommandAsync(
            string name,
            int version,
            string json,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (version <= 0)
                throw new ArgumentOutOfRangeException(nameof(version));
            if (string.IsNullOrEmpty(json))
                throw new ArgumentNullException(nameof(json));

            if (!_platform.Definitions.Commands.TryGetDefinition(name, version, out CommandDefinition commandDefinition))
                throw new ArgumentException($"No command definition found for command '{name}' v{version}");

            ICommand command;
            try
            {
                command = (ICommand)JsonConvert.DeserializeObject(json, commandDefinition.Type);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to deserialize command '{name}' v{version}: {ex.Message}", ex);
            }
            var executionResult = await _platform.Publish(command);
            return executionResult;
        }
    }
}