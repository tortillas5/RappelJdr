namespace RappelJdr
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using RappelJdr.Database;
    using RappelJdr.Entities;

    /// <summary>
    /// Class handling the messages of the users.
    /// </summary>
    public static class MessageHandler
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        static MessageHandler()
        {
            SessionService = new SessionService();
            RoleService = new RoleService();
            AdminService = new AdminService();
        }

        /// <summary>
        /// Get or set the service of the admins.
        /// </summary>
        public static AdminService AdminService { get; set; }

        /// <summary>
        /// Get or set the service of the roles.
        /// </summary>
        public static RoleService RoleService { get; set; }

        /// <summary>
        /// Get or set the service of the sessions.
        /// </summary>
        public static SessionService SessionService { get; set; }

        /// <summary>
        /// Retourne la liste des commandes utilisables par le bot.
        /// </summary>
        /// <returns>Message contenant les commandes utilisables par le bot.</returns>
        public static string Help()
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Help.txt");
            string text = File.ReadAllText(path);

            return text;
        }

        #region Sessions

        /// <summary>
        /// Delete a session.
        /// </summary>
        /// <param name="id">Id of a session.</param>
        /// <param name="serverId">Id of the server sending the request.</param>
        /// <param name="channelId">Id of the channel sending the request.</param>
        /// <returns>Message saying if the deletion succeded or not.</returns>
        public static string DeleteSession(int id, ulong serverId, ulong channelId)
        {
            try
            {
                var session = SessionService.GetEntities().FirstOrDefault(e => e.ServerId == serverId && e.ChannelId == channelId && e.Id == id);

                if (session != null)
                {
                    SessionService.RemoveById(id);

                    return "La session a bien été supprimée.";
                }
                else
                {
                    return "Aucune session avec cet identifiant à supprimer.";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Return a message with the list of the next rp sessions.
        /// Return a message saying no next session if there is none.
        /// </summary>
        /// <param name="serverId">Id of the server sending the request.</param>
        /// <param name="channelId">Id of the channel sending the request.</param>
        /// <returns>A message.</returns>
        public static string ListSession(ulong serverId, ulong channelId)
        {
            try
            {
                var sessions = SessionService.GetEntities().Where(e => e.ServerId == serverId && e.ChannelId == channelId);

                if (sessions.Count() > 0)
                {
                    return "Prochaines sessions :\n" + String.Join("\n", sessions.Select(e => "Id : " + e.Id.ToString() + " Date : " + e.Date.ToString("dd/MM/yyyy HH:mm")));
                }
                else
                {
                    return "Pas de prochaine session prévue.";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Add a session.
        /// </summary>
        /// <param name="sessionDate">Date of the session to add.</param>
        /// <param name="serverId">Id of the server sending the request.</param>
        /// <param name="channelId">Id of the channel sending the request.</param>
        /// <returns>Message saying if the adding was successful or not.</returns>
        public static string SetSession(string sessionDate, ulong serverId, ulong channelId)
        {
            try
            {
                DateTime dateSession = DateTime.ParseExact(sessionDate, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

                if (dateSession < DateTime.Now)
                {
                    return "Impossible d'ajouter une session dans le passé.";
                }

                var sessions = SessionService.GetEntities();

                if (sessions.Exists(m => m.Date == dateSession && m.ServerId == serverId && m.ChannelId == channelId))
                {
                    return "La session existe déjà.";
                }

                Session session = new Session()
                {
                    Date = dateSession,
                    ServerId = serverId,
                    ChannelId = channelId
                };

                SessionService.Add(session);

                return "La session a bien été ajoutée.";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #endregion Sessions

        #region Roles

        /// <summary>
        /// Add a rôle to the list of possible rôles.
        /// </summary>
        /// <param name="emoji">Emoji to be used as a reaction.</param>
        /// <param name="roleName">Name of the rôle to add.</param>
        /// <param name="userName">Name of the user adding the rôle (must be an admin).</param>
        /// <returns>Message saying if the rôle was successfully added or not.</returns>
        public static string AddRole(string emoji, string roleName, string userName, List<string> serverRoles)
        {
            try
            {
                if (!IsAdmin(userName))
                {
                    return "Vous n'avez pas le droit.";
                }

                if (!serverRoles.Contains(roleName))
                {
                    return "Ce rôle n'existe pas sur le serveur.";
                }

                var roles = RoleService.GetEntities();

                if (roles.Exists(e => e.Emoji == emoji))
                {
                    return "Emoji déjà utilisé.";
                }

                if (roles.Exists(e => e.Name == roleName))
                {
                    return "Rôle déjà attribué à un emoji.";
                }

                Role role = new Role()
                {
                    Emoji = emoji,
                    Name = roleName
                };

                RoleService.Add(role);

                return "Le rôle a bien été ajouté.";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Return the list of possible roles and their names if there is any.
        /// Return a message saying there is no role if there is none.
        /// </summary>
        /// <returns>A message.</returns>
        public static string ListRole()
        {
            try
            {
                var roles = RoleService.GetEntities();

                if (roles.Count() > 0)
                {
                    return "Liste des rôles :\n" + String.Join("\n", roles.Select(e => e.Emoji + " : " + e.Name));
                }
                else
                {
                    return "Aucun rôle défini.";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Return a message to react to with the list of existing roles.
        /// </summary>
        /// <returns>A message.</returns>
        public static string ReactTo()
        {
            try
            {
                var roles = RoleService.GetEntities();

                if (roles.Count() > 0)
                {
                    return "Réagissez à ce message pour vous ajouter / retirer un rôle.\n" +
                        "Liste des rôles :\n" + String.Join("\n", roles.Select(e => e.Emoji + " : " + e.Name));
                }
                else
                {
                    return "Aucun rôle défini.";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Remove a role with it's linked emoji.
        /// </summary>
        /// <param name="emoji">Emoji of the role to remove.</param>
        /// <param name="userName">Name of the user removing the role (must be an admin).</param>
        /// <returns></returns>
        public static string RemoveRole(string emoji, string userName)
        {
            try
            {
                if (!IsAdmin(userName))
                {
                    return "Vous n'avez pas le droit.";
                }

                var role = RoleService.GetEntities().FirstOrDefault(e => e.Emoji == emoji);

                if (role != null)
                {
                    RoleService.Remove(role);

                    return "Le rôle a bien été retiré de la liste des réactions.";
                }
                else
                {
                    return "Aucun rôle n'est lié à cet émoji.";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Check wether a user is admin or not.
        /// </summary>
        /// <param name="userName">Name of an user.</param>
        /// <returns>Value indicating if the user is admin or not.</returns>
        private static bool IsAdmin(string userName)
        {
            return AdminService.GetEntities().Exists(e => e.Name == userName);
        }

        #endregion Roles
    }
}