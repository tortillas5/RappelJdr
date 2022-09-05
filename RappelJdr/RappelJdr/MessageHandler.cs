namespace RappelJdr
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using TortillasDatabase;
    using TortillasEntities;

    /// <summary>
    /// Class handling the requests of the users.
    /// </summary>
    public static class MessageHandler
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        static MessageHandler()
        {
            SessionService = new SessionService();
        }

        /// <summary>
        /// Get or set the service of the sessions.
        /// </summary>
        public static SessionService SessionService { get; set; }

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
        /// Retourne la liste des commandes utilisables par le bot.
        /// </summary>
        /// <returns>Message contenant les commandes utilisables par le bot.</returns>
        public static string Help()
        {
            return "Liste des commandes :\n" +
                "-help Liste des commandes.\n" +
                "-set dd/MM/yyyy HH:mm Défini une session à rappeler.\n" +
                "-list Liste des sessions mises en rappel.\n" +
                "-delete [Number] Supprime une session avec son numéro.\n";
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

                var sessions =  SessionService.GetEntities();

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
    }
}