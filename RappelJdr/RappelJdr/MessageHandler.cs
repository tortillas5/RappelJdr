namespace RappelJdr
{
    using System;
    using System.Globalization;
    using System.Linq;
    using TortillasDatabase;
    using TortillasEntities;

    /// <summary>
    /// Class handling the requests of the users.
    /// </summary>
    public static class MessageHandler
    {
        static MessageHandler()
        {
            SessionService = new SessionService();
        }

        public static SessionService SessionService { get; set; }

        public static string DeleteSession(int id)
        {
            try
            {
                SessionService.RemoveById(id);

                return "La session a bien été supprimée.";
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

        public static string ListSession()
        {
            try
            {
                var sessions = SessionService.GetEntities();

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

        public static string SetSession(string sessionDate, ulong serverId, ulong channelId)
        {
            try
            {
                DateTime dateSession = DateTime.ParseExact(sessionDate, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

                if (dateSession < DateTime.Now)
                {
                    return "Impossible d'ajouter une session dans le passé.";
                }

                Session session = new Session()
                {
                    Date = dateSession,
                    ServerId = serverId,
                    ChannelId = channelId
                };

                SessionService.Add(session);

                return "La session a bien été ajoutée";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}