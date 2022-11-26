﻿namespace RappelJdr
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using RappelJdr.Database;
    using RappelJdr.Entities;

    /// <summary>
    /// Class handling the reactions of the users.
    /// </summary>
    public static class ReactionHandler
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        static ReactionHandler()
        {
            FollowedMessageService = new FollowedMessageService();
            RoleService = new RoleService();
        }

        /// <summary>
        /// Get or set the service of the followed messages.
        /// </summary>
        public static FollowedMessageService FollowedMessageService { get; set; }

        /// <summary>
        /// Get or set the service of the roles.
        /// </summary>
        public static RoleService RoleService { get; set; }

        /// <summary>
        /// Return the existing roles.
        /// </summary>
        /// <returns>List of roles.</returns>
        public static List<Role> GetRoles(ulong serverId)
        {
            return RoleService.GetEntities().Where(e => e.ServerId == serverId).ToList();
        }

        /// <summary>
        /// Return a boolean indicating if a message is followed or not.
        /// </summary>
        /// <param name="idMessage">Id of a message.</param>
        /// <returns>Value indicating if a message is followed or not.</returns>
        public static bool IsFollowedMessage(ulong idMessage, ulong serverId)
        {
            return FollowedMessageService.GetEntities().Exists(e => e.ServerId == serverId && e.MessageId == idMessage);
        }

        /// <summary>
        /// Add a message to follow to the database.
        /// </summary>
        /// <param name="idMessage">Id of a message.</param>
        public static void MessageToFollow(ulong idMessage, ulong serverId)
        {
            try
            {
                FollowedMessage followedMessage = new FollowedMessage()
                {
                    MessageId = idMessage,
                    ServerId = serverId
                };

                FollowedMessageService.GetEntities()
                    .Where(e => e.ServerId == serverId).ToList()
                    .ForEach(e => FollowedMessageService.Remove(e));

                FollowedMessageService.Add(followedMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}