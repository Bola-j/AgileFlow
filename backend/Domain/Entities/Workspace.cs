    using AgileFlow.Domain.Entities;
using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Workspace : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; }

        public Workspace() { }
        public Workspace(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public void UpdateName(string name)
        {
            Name = name;
            Update();
        }

        public void UpdateDescription(string description)
        {
            Description = description;
            Update();
        }
        public ICollection<UserWorkspace> UserWorkspaces { get; private set; } = new List<UserWorkspace>();
        public ICollection<Project> Projects { get; private set; } = new List<Project>();
    }
}
