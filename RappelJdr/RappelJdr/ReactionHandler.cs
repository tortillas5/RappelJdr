namespace RappelJdr
{
    using System;
    using System.Collections.Generic;
    using RappelJdr.Database;
    using RappelJdr.Entities;

    public static class ReactionHandler
    {
        static ReactionHandler()
        {
            FollowedMessageService = new FollowedMessageService();
            RoleService = new RoleService();
        }

        public static FollowedMessageService FollowedMessageService { get; set; }
        public static RoleService RoleService { get; set; }

        public static List<Role> GetRoles()
        {
            return RoleService.GetEntities();
        }

        public static bool IsFollowedMessage(ulong idMessage)
        {
            return FollowedMessageService.GetEntities().Exists(e => e.MessageId == idMessage);
        }

        public static void MessageToFollow(ulong idMessage)
        {
            try
            {
                FollowedMessage followedMessage = new FollowedMessage()
                {
                    MessageId = idMessage
                };

                FollowedMessageService.Add(followedMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}