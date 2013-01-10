using System;
using ManyConsole;
using TeamCityBuildChanges.ExternalApi.Jira;

namespace TeamCityBuildChanges.Commands
{
    public class EncodeCommand : ConsoleCommand
    {
        private string _username;
        private string _password;

        public EncodeCommand()
        {
            IsCommand("encode", "Encodes username and password to a token for use in Jira authentication.");
            HasRequiredOption("u|username=", "Username to use for encoding.", s => _username = s);
            HasRequiredOption("p|password=", "Password to use for encoding.", s => _password = s);

        }
        public override int Run(string[] remainingArguments)
        {
            Console.WriteLine("Encoded string for use as username/password:  {0}", JiraApi.GetEncodedCredentials(_username, _password));
            return 0;
        }
    }
}
