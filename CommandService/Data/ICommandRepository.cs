using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandService.Models;

namespace CommandService.Data
{
    public interface ICommandRepository
    {
        bool SaveChanges();

        // Platform Related
        IEnumerable<Platform> getAllPlatforms();
        void CreatePlatform(Platform platform);
        bool PlatformExists(int platformId);
        bool ExternalPlatformExists(int externalPlatformId);

        // Commands related
        IEnumerable<Command> getCommandsForPlatform(int platformId);
        Command GetCommand(int platformId, int commandId);
        void CreateCommand(int platformId, Command command);

    }
}