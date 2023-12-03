using App.Data;
using App.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace App.Services
{
    public class ConnectionManagerService
    {
        private readonly Dictionary<string, List<string>> connections;
        private readonly Dictionary<string, string> roleMapper;
        private readonly ILogger _logger;

        public ConnectionManagerService(ILogger<ConnectionManagerService> logger)
        {
            connections = new();
            roleMapper = new();
            _logger = logger;
        }

        public void AddConnection(string userId, string connectionId, bool isAdmin = false)
        {
            try
            {
                if (!connections.ContainsKey(userId))
                {
                    connections[userId] = new();
                }

                connections[userId].Add(connectionId);
                if (isAdmin)
                {
                    roleMapper[userId] = "admin";
                }
                else
                {
                    roleMapper[userId] = "client";
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public void RemoveConnection(string userId, string connectionId)
        {
            try
            {
                if (connections.ContainsKey(userId) && connections[userId].Contains(connectionId))
                {
                    connections[userId].Remove(connectionId);
                }

                if (connections[userId].Count == 0)
                {
                    connections.Remove(userId);
                    roleMapper.Remove(userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public List<string> GetConnectionId(string userId)
        {
            if (connections.ContainsKey(userId))
                return connections[userId];

            return new();
        }

        public List<string> GetConnectionIdAdmin()
        {
            var connectionsOfAdmin = connections.Where(p => roleMapper.ContainsKey(p.Key) && roleMapper[p.Key] == "admin").SelectMany(p => p.Value).ToList();

            return connectionsOfAdmin;
        }

        public int AdminLogged()
        {
            return roleMapper.Where(x => x.Value == "admin").Count();
        }
    }
}