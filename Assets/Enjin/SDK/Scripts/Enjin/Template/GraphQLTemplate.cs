namespace EnjinSDK
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class GraphQLTemplate
    {
        public enum QueryType
        {
            UNKNOWN,
            LOGIN,
            GET_PLATFORM_INFO,
            UPDATE_BALANCES,
            CREATE_USER,
            UPDATE_USER,
            INVITE_USER,
            INVITE_USER_WITH_NAME,
            CREATE_IDENTITY_BY_USER_ID,
            CREATE_IDENTITY_BY_EMAIL,
            CREATE_ITEM,
            GET_ITEM_BY_ID,
            GET_ITEM_BALANCES_FOR_ADDRESS,
            GET_ITEM_BALANCES_FOR_IDENTITY_ID,
            MINT_ITEM,
            CREATE_ROLE,
            ADD_ROLE,
            DELETE_ROLE,
            UPDATE_ROLE,
            UPDATE_ROLE_NAME,
            CREATE_APP,
            UPDATE_APP,
            SEND_ITEMS,
            SET_MAX_ALLOWANCE,
            REFRESH_IDENTITY,
            CREATE_TRADE,
            COMPLETE_TRADE,
            MELT_FUNGIBLE_ITEM,
            MELT_NONFUNGIBLE_ITEM
        }

        public Dictionary<QueryType, string> Queries { get; private set; }

        public GraphQLTemplate(string templateFile)
        {
            Queries = new Dictionary<QueryType, string>();
            ReadTemplate(templateFile);
        }

        private void ReadTemplate(string file)
        {
            TextAsset templateData = Resources.Load<TextAsset>("Templates/" + file);

            if (templateData.text == string.Empty)
                return;

            string[] lines = templateData.text.Split('\n');

            foreach (string line in lines)
            {
                if (line.Contains("|"))
                {
                    string[] queryTypeFormat = line.Split('|');

                    if (queryTypeFormat.Length == 2)
                    {
                        bool validQueryType = Enum.TryParse(queryTypeFormat[0], out QueryType queryType);

                        if (validQueryType)
                            Queries.Add(queryType, queryTypeFormat[1]);
                        else
                            EnjinLogger.ConsoleReporter(LoggerType.WARNING, string.Format("Ignoring template {0} with no known query type.", queryTypeFormat[0]));
                    }
                    else
                        EnjinLogger.ConsoleReporter(LoggerType.WARNING, "Invalid template entry. Check template entry is formatted correctly.");
                }
                else
                    EnjinLogger.ConsoleReporter(LoggerType.WARNING, string.Format("Ignoring malformed template entry."));
            }
        }
    }
}