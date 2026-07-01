using System;
using System.Collections.Generic;

namespace VanguardThaiSim.Multiplayer
{
    [Serializable]
    public sealed class MultiplayerProtocolValidationResult
    {
        public bool accepted = true;
        public List<string> issues = new List<string>();

        public void Reject(string issue)
        {
            accepted = false;
            issues.Add(issue);
        }

        public string FirstIssue
        {
            get { return issues.Count == 0 ? null : issues[0]; }
        }
    }
}
