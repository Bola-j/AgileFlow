using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgileFlow.Domain.Entities;

namespace AgileFlow.Domain.Entities
{
    public class BoardColumn : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public int BoardId { get; private set; }
        public Board Board { get; private set; } = null!;

        private BoardColumn() { }

        public BoardColumn(string name, int boardId)
        {
            Name = name;
            BoardId = boardId;
        }

        public void UpdateName(string name)
        {
            Name = name;
            Update();
        }

        public ICollection<ProjectTask> Tasks { get; private set; } = new List<ProjectTask>();
    }
}
